CREATE EVENT monthly_statistics
ON SCHEDULE EVERY 1 MONTH
STARTS TIMESTAMP(DATE_FORMAT(NOW() + INTERVAL 1 MONTH, '%Y-%m-01 02:00:00'))
ON COMPLETION PRESERVE
ENABLE
DO BEGIN
    CREATE TEMPORARY TABLE temp_statistics AS
    SELECT
        doc_types.name as name,
        YEAR(docs.create_date) as year,
        MONTH(docs.create_date) as month,
        COUNT(images.id) as col,
        SUM(images.size / 1048576) as sum,
        AVG(images.size / 1048576) as avg
    FROM
        images
    LEFT JOIN docs ON images.doc_id = docs.id
    LEFT JOIN doc_types ON doc_types.id = docs.type_id
    WHERE
        docs.create_date < DATE_FORMAT(NOW(), '%Y-%m-01')
    GROUP BY
        doc_types.name,
        YEAR(docs.create_date),
        MONTH(docs.create_date);
    
    TRUNCATE TABLE statistics;
    INSERT INTO statistics (name, year, month, col, sum, avg) SELECT * FROM temp_statistics;
    DROP TEMPORARY TABLE IF EXISTS temp_statistics;
    
END
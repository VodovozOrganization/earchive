using Gtk;
using MySql.Data.MySqlClient;
using QSProjectsLib;
using System;

namespace earchive
{
    public partial class Statistics : Gtk.Dialog
    {
        public Statistics()
        {
            this.Build();

            stattree = new TreeStore(typeof(string), typeof(string), typeof(string), typeof(string));
            treeview1.AppendColumn("Тип/дата", new Gtk.CellRendererText(), "text", 0);
            treeview1.AppendColumn("Кол-во", new Gtk.CellRendererText(), "text", 1);
            treeview1.AppendColumn("Общий объем", new Gtk.CellRendererText(), "text", 2);
            treeview1.AppendColumn("Средний объем", new Gtk.CellRendererText(), "text", 3);

            treeview1.Model = stattree;
            treeview1.ShowAll();
            drawtable();
        }

        TreeStore stattree;

        private void drawtable()
        {
            string sql = "SELECT COUNT(*) as cnt FROM docs";
            QSMain.CheckConnectionAlive();
            MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
            MySqlDataReader rdr = cmd.ExecuteReader();
            rdr.Read();
            labelCountDoc.Text = String.Format("{0} шт.", rdr.GetInt32("cnt"));
            rdr.Close();

            sql = "SELECT * FROM (SELECT doc_types.name as name, YEAR(docs.create_date) as year, " +
                                            "MONTH(docs.create_date) as month, count(images.id/1048576) as col, " +
                                            "SUM(images.size/1048576) as sum, AVG(images.size/1048576) as avg FROM images " +
                "LEFT JOIN docs ON images.doc_id = docs.id " +
                "LEFT JOIN doc_types ON doc_types.id = docs.type_id " +
                "GROUP BY doc_types.name , year , month WITH ROLLUP) as tt " +
                "ORDER BY name, year, month;";
            cmd = new MySqlCommand(sql, QSMain.connectionDB);
            rdr = cmd.ExecuteReader();
            rdr.Read();
            labelCountImag.Text = String.Format("{0} шт.", rdr.GetInt64("col"));
            labelCommonImag.Text = String.Format("{0:N0} Мб", rdr.GetDouble("sum"));
            labelAvgImag.Text = String.Format("{0:N} Мб", rdr.GetDouble("avg"));


            TreeIter MainIter = TreeIter.Zero, NowIter = TreeIter.Zero;
            while (rdr.Read())
            {
                string col = String.Format("{0} шт.", rdr.GetInt32("col"));
                string sum = String.Format("{0:N0} Мб", rdr.GetDouble("sum"));
                string avg = String.Format("{0:N} Мб", rdr.GetDouble("avg"));
                if (rdr["month"] == DBNull.Value)
                {
                    if (rdr["year"] == DBNull.Value)
                    {
                        string name = rdr.GetString("name");
                        MainIter = stattree.AppendValues(name, col, sum, avg);
                    }
                    else
                    {
                        string year = rdr.GetInt32("year").ToString();
                        NowIter = stattree.AppendValues(MainIter, year, col, sum, avg);
                    }
                }
                else
                {
                    string month = String.Format("{0:MMMM}", new DateTime(1, rdr.GetInt32("month"), 1));
                    stattree.AppendValues(NowIter, month, col, sum, avg);
                }
            }
            rdr.Close();
        }
    }
}
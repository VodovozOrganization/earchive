using ClosedXML.Excel;
using Gtk;
using QS.Project.Services.FileDialog;
using System;
using System.IO;
using System.Text;

namespace earchive.Reports
{
	internal class UpdReport
	{
		private const string _xlsxFileFilter = "XLSX File (*.xlsx)";

		private readonly IFileDialogService _fileDialogService;
		private string _title;

		public UpdReport(IFileDialogService fileDialogService)
		{
			_fileDialogService = fileDialogService ?? throw new ArgumentNullException(nameof(fileDialogService));
		}

		public bool Export(ListStore listStore, DateTime? startDate, DateTime? endDate, out string message)
		{
			message = string.Empty;
			_title = GenerateTitle(startDate, endDate);

			using (var wb = new XLWorkbook())
			{
				var sheetName = _title;
				var ws = wb.Worksheets.Add(sheetName);

				Generate(listStore, ws);

				if(TryGetSavePath(out string path))
				{
					try
					{
						wb.SaveAs(path);
					}
					catch(IOException ex)
					{
						message = "Не удалось сохранить файл по указанному пути. Возможно, файл уже открыт. Пожалуйста, закройте файл и попробуйте снова.";
						return false;
					}
				}
			}

			return true;
		}

		private string GenerateTitle(DateTime? startDate, DateTime? endDate)
		{
			var sb = new StringBuilder();
			sb.Append("УПД");

			if(!startDate.HasValue && !endDate.HasValue)
			{
				return sb.ToString();
			}

			if(startDate.HasValue && !endDate.HasValue)
			{
				sb.Append($" c {startDate.Value:dd.MM.yyyy}");
			}
			else if(!startDate.HasValue && endDate.HasValue)
			{
				sb.Append($" по {endDate.Value:dd.MM.yyyy}");
			}
			else
			{
				sb.Append($" {startDate.Value:dd.MM.yyyy} - {endDate.Value:dd.MM.yyyy}");
			}

			return sb.ToString();
		}

		private void Generate(ListStore listStore, IXLWorksheet ws)
		{
			var colName = new string[]
			{
				"№",
				"Номер заказа",
				"Дата УПД",
				"Номер УПД",
				"Документ создан"
			};

			var row = 1;
			var columnsCount = colName.Length;

			var titleColumsRange = ws.Range(row, 1, row, columnsCount);
			titleColumsRange.Value = "Реестр электронного архива";
			titleColumsRange.Merge();
			titleColumsRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
			titleColumsRange.Style.Font.Bold = true;

			row += 2;

			for (int i = 0; i < colName.Length; i++)
			{
				ws.Cell(row, i + 1).Value = colName[i];
			}

			var index = 0;

			foreach (object[] items in listStore)
			{
				row++;
				ws.Cell(row, 1).Value = ++index;
				
				var i = 0;

				foreach(var item in items)
				{
					if(i == 0)
					{
						i++;
						continue;
					}

					if(i == 3)
					{
						ws.Cell(row, i + 2).Value = item;
					}
					else if(i == 4)
					{
						ws.Cell(row, i).Value = item;
					}
					else
					{
						ws.Cell(row, i + 1).Value = item;
					}

					i++;
				}
			}

			ws.Columns().AdjustToContents();
			ws.Columns().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
		}

		private bool TryGetSavePath(out string path)
		{
			var extension = ".xlsx";
			var dialogSettings = new DialogSettings
			{
				Title = "Сохранить",
				FileName = _title
			};

			dialogSettings.FileFilters.Add(new DialogFileFilter(_xlsxFileFilter, $"*{extension}"));
			var result = _fileDialogService.RunSaveFileDialog(dialogSettings);
			path = result.Path;

			return result.Successful;
		}
	}
}

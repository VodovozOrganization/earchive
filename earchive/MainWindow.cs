using earchive.UpdGrpc;
using EarchiveApi;
using Gtk;
using MySql.Data.MySqlClient;
using NLog;
using QS.Dialog.GtkUI;
using QS.Project.Versioning;
using QS.Project.Versioning.Product;
using QS.Project.ViewModels;
using QS.Project.Views;
using QS.Utilities;
using QSProjectsLib;
using QSWidgetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace earchive
{
	public partial class MainWindow : Window
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		IApplicationInfo applicationInfo = new ApplicationVersionInfo();
		ListStore DocsListStore;
		DocumentInformation CurDocType;
		int UsedExtraFields;
		InputDocs InputDocsWin;

		private int? _selectiedDocumentTypeId;
		private CounterpartyInfo _selectedCounterparty;
		private DeliveryPointInfo _selectedDeliveryPoint;
		private EarchiveUpdServiceClient _earchiveUpdServiceClient;

		public MainWindow() : base(WindowType.Toplevel)
		{
			Build();

			QSMain.StatusBarLabel = labelStatus;
			this.Title = $"{applicationInfo.ProductTitle} v{applicationInfo.Version} от {applicationInfo.BuildDate:dd.MM.yyyy HH:mm}";
			QSMain.MakeNewStatusTargetForNlog();

			Reference.RunReferenceItemDlg += OnRunReferenceItemDialog;
			QSMain.ReferenceUpdated += OnReferenceUpdate;

			UsersAction.Sensitive = QSMain.User.Admin;
			labelUser.LabelProp = QSMain.User.Name;
			ActionDocTypes.Sensitive = QSMain.User.Permissions["edit_db"];
			buttonInput.Sensitive = QSMain.User.Permissions["can_edit"];

			// Создаем главное окно
			ComboWorks.ComboFillReference(comboDocType, "doc_types", ComboWorks.ListMode.OnlyItems);
			selectperiodDocs.ActiveRadio = SelectPeriod.Period.Week;

			SetUpdControls();
		}

		private void SetUpdControls()
		{
			//Настройка контролов поиска кодов УПД
			_earchiveUpdServiceClient = new EarchiveUpdServiceClient();

			yentryClient.Completion = new EntryCompletion();
			yentryClient.Completion.Model = new ListStore(typeof(CounterpartyInfo));
			yentryClient.Completion.MinimumKeyLength = 2;
			yentryClient.Completion.TextColumn = 0;
			yentryClient.Completion.Complete();
			var cell = new CellRendererText();
			yentryClient.Completion.PackStart(cell, true);
			yentryClient.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);

			yentryClient.Completion.MatchSelected += OnCompletionMatchSelected;
			yentryClient.Completion.MatchFunc = CompletionMatchFunc;
			yentryClient.Changed += OnYentryClientChanged;

			comboboxAddress.SetRenderTextFunc<DeliveryPointInfo>(d => d.Address);
			comboboxAddress.ItemSelected += OnComboboxAddressItemSelected;
		}

		#region Свойства
		public int? SelectedDocumentTypeId
		{
			get => _selectiedDocumentTypeId;
			set 
			{ 
				if(_selectiedDocumentTypeId != value)
				{
					_selectiedDocumentTypeId = value;

					if (_selectiedDocumentTypeId.HasValue)
					{
						SetUpdSearchControlsSettings();
					}

					SelectedCounterparty = null;
					SelectedDeliveryPoint = null;
				}
			}
		}

		public CounterpartyInfo SelectedCounterparty
		{
			get => _selectedCounterparty;
			set
			{
				if(_selectedCounterparty != value)
				{
					_selectedCounterparty = value;

					if (_selectedCounterparty != null)
					{
						entryDocNumber.Text = string.Empty;
						GetAllCounterpartyAdresses();
					}
					else
					{
						ClearComboboxAddresses();
					}
				}
			}
		}

		public DeliveryPointInfo SelectedDeliveryPoint
		{
			get => _selectedDeliveryPoint;
			set 
			{
				if(_selectedDeliveryPoint != value)
				{
					_selectedDeliveryPoint = value;

					if (_selectedDeliveryPoint != null)
					{
						GetUpdDocs();
					}
				}
			}
		}
		#endregion

		protected void OnDeleteEvent(object sender, DeleteEventArgs a)
		{
			Application.Quit();
			a.RetVal = true;
		}

		protected void OnReferenceUpdate(object sender, QSMain.ReferenceUpdatedEventArgs e)
		{
			switch (e.ReferenceTable) {
				case "doc_types":
					ComboWorks.ComboFillReference(comboDocType, "doc_types", ComboWorks.ListMode.OnlyItems);
					break;
			}
		}

		protected void OnUsersActionActivated(object sender, EventArgs e)
		{
			Users WinUser = new Users();
			WinUser.Show();
			WinUser.Run();
			WinUser.Destroy();
		}

		protected void OnDialogAuthenticationActionActivated(object sender, EventArgs e)
		{
			QSMain.User.ChangeUserPassword(this);
		}

		void PrepareDocsTable()
		{
			UsedExtraFields = 0;
			if (CurDocType.FieldsList != null)
				foreach (DocFieldInfo item in CurDocType.FieldsList)
					if (item.Display || item.Search)
						UsedExtraFields++;

			Type[] Types = new Type[4 + UsedExtraFields];
			Types[0] = typeof(int); //0 - id
			Types[1] = typeof(string); //1 - number
			Types[2] = typeof(string);//2 - doc date
			Types[3] = typeof(string);//3 - created date;

			int i = 4;

			if (CurDocType.FieldsList != null)
				foreach (DocFieldInfo item in CurDocType.FieldsList) {
					if (!item.Display && !item.Search)
						continue;
					switch (item.Type) {
						case "varchar":
							Types[i] = typeof(string);
							item.ListStoreColumn = i;
							break;
					}
					i++;
				}

			DocsListStore = new ListStore(Types);

			foreach (TreeViewColumn col in treeviewDocs.Columns) {
				treeviewDocs.RemoveColumn(col);
			}

			treeviewDocs.AppendColumn("Номер", new CellRendererText(), "text", 1);
			treeviewDocs.AppendColumn("Дата", new CellRendererText(), "text", 2);
			if (CurDocType.FieldsList != null) {
				foreach (DocFieldInfo item in CurDocType.FieldsList) {
					if (!item.Display)
						continue;
					switch (item.Type) {
						case "varchar":
							treeviewDocs.AppendColumn(item.Name, new CellRendererText(), "text", item.ListStoreColumn);
							break;
					}
				}
			}
			treeviewDocs.AppendColumn("Создан", new CellRendererText(), "text", 3);

			treeviewDocs.Model = DocsListStore;
			treeviewDocs.ShowAll();
			buttonRefresh.Sensitive = true;
		}

		protected void OnAction2Activated(object sender, EventArgs e)
		{
			Reference winref = new Reference();
			winref.SetMode(false, false, true, true, true);
			winref.FillList("doc_types", "Тип документа", "Типы документов");
			winref.Show();
			winref.Run();
			winref.Destroy();
		}

		protected void OnRunReferenceItemDialog(object sender, Reference.RunReferenceItemDlgEventArgs e)
		{
			ResponseType Result;
			switch (e.TableName) {
				case "doc_types":
					DocsType DocTypeEdit = new DocsType();
					if (e.NewItem)
						DocTypeEdit.NewDocsType = true;
					else
						DocTypeEdit.Fill(e.ItemId);
					DocTypeEdit.Show();
					Result = (ResponseType)DocTypeEdit.Run();
					DocTypeEdit.Destroy();
					break;
				default:
					Result = ResponseType.None;
					break;
			}
			e.Result = Result;
		}

		protected void OnComboDocTypeChanged(object sender, EventArgs e)
		{
			CurDocType = null;
			if (comboDocType.GetActiveIter(out TreeIter iter)) 
			{
				int CurrentTypeId = (int)comboDocType.Model.GetValue(iter, 1);
				CurDocType = new DocumentInformation(CurrentTypeId);
				SelectedDocumentTypeId = CurrentTypeId;
				PrepareDocsTable();
				UpdateDocs();
			}
		}

		#region Поиск УПД
		private void SetUpdSearchControlsSettings()
		{
			yentryClient.Text = string.Empty;

			var selectedUpdDocumentType = SelectedDocumentTypeId == 5;

			if (selectedUpdDocumentType)
			{
				yentryClient.Visible = true;
				yentryClient.Sensitive = true;
				labelClient.Visible = true;

				comboboxAddress.Visible = true;
				comboboxAddress.Sensitive = true;
				labelAddress.Visible = true;

				return;
			}

			yentryClient.Visible = false;
			yentryClient.Sensitive = false;
			labelClient.Visible = false;

			comboboxAddress.Visible = false;
			comboboxAddress.Sensitive = false;
			labelAddress.Visible = false;
		}

		private void CounterpartyEntryFillAutocomplete()
		{
			var completionListStore = new ListStore(typeof(CounterpartyInfo));

			var items = new List<CounterpartyInfo>();

			if (_earchiveUpdServiceClient.IsConnectionActive)
			{
				items = _earchiveUpdServiceClient.GetCounterparties(yentryClient.Text.ToLower());
			}
			else
			{
				ShowGrpcServiceUnavailableMessage();
			}

			foreach (var item in items)
			{
				if (item.Id < 1 || String.IsNullOrWhiteSpace(item.Name))
				{
					continue;
				}
				completionListStore.AppendValues(item);
			}

			yentryClient.Completion.Model = completionListStore;

			if (yentryClient.HasFocus)
			{
				yentryClient.Completion.Complete();
			}
		}

		private void OnCellLayoutDataFunc(CellLayout cell_layout, CellRenderer cell, TreeModel tree_model, TreeIter iter)
		{
			var counterpartyName = ((CounterpartyInfo)tree_model.GetValue(iter, 0)).Name;
			var pattern = string.Format("\\b{0}", Regex.Escape(yentryClient.Text.ToLower()));
			counterpartyName = Regex.Replace(counterpartyName, pattern, match => $"<b>{match.Value}</b>", RegexOptions.IgnoreCase);

			((CellRendererText)cell).Markup = counterpartyName;
		}

		private void OnYentryClientChanged(object sender, EventArgs e)
		{
			SelectedCounterparty = null;
			SelectedDeliveryPoint = null;
			CounterpartyEntryFillAutocomplete();
		}

		private bool CompletionMatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			var value = completion.Model.GetValue(iter, 0).ToString().ToLower();
			var isMatch = Regex.IsMatch(value, String.Format("\\b{0}.*", Regex.Escape(yentryClient.Text.ToLower())));
			CompletionFullMatchFunc(completion, key, iter);
			return isMatch;
		}

		private bool CompletionFullMatchFunc(EntryCompletion completion, string key, TreeIter iter)
		{
			var enteredCounterpartyNameValue = (CounterpartyInfo)completion.Model.GetValue(iter, 0);
			var value = enteredCounterpartyNameValue.Name.ToLower();
			bool isMatch = key == value;

			if(isMatch)
			{
				SelectedCounterparty = enteredCounterpartyNameValue;
			}

			return isMatch;
		}

		[GLib.ConnectBefore]
		void OnCompletionMatchSelected(object o, MatchSelectedArgs args)
		{
			SelectedCounterparty = (CounterpartyInfo)args.Model.GetValue(args.Iter, 0);
			yentryClient.Text = SelectedCounterparty.Name;
			args.RetVal = true;
		}

		private void GetAllCounterpartyAdresses()
		{
			comboboxAddress.SetRenderTextFunc<DeliveryPointInfo>(d => d.Address);

			if(SelectedCounterparty != null)
			{
				if (_earchiveUpdServiceClient.IsConnectionActive)
				{
					var deliveryPoints = _earchiveUpdServiceClient
					.GetDeliveryPoints(SelectedCounterparty)
					.Select(d => d)
					.ToList();
					comboboxAddress.ItemsList = deliveryPoints;
					return;
				}
				else
				{
					ShowGrpcServiceUnavailableMessage();
				}
					
			}
			comboboxAddress.ItemsList = null;
		}

		private void OnComboboxAddressItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			SelectedDeliveryPoint = (DeliveryPointInfo)comboboxAddress.SelectedItem;
		}

		private bool GetUpdDocs()
		{
			if (SelectedDocumentTypeId != 5 || SelectedCounterparty == null || SelectedDeliveryPoint == null)
			{
				return false;
			}

			var udpCodes = new List<long>();

			if (_earchiveUpdServiceClient.IsConnectionActive)
			{
				udpCodes = _earchiveUpdServiceClient
					.GetUpdCodes(SelectedCounterparty.Id, SelectedDeliveryPoint.Id, DateTime.Now.AddYears(-30), DateTime.Now)
					.Select(c => c.Id)
					.ToList();

				UpdateDocs(udpCodes);
				return true;
			}

			ShowGrpcServiceUnavailableMessage();

			return true;
		}

		private void ClearComboboxAddresses()
		{
			comboboxAddress.ItemsList = null;
		}
		#endregion

		private void UpdateDocs(List<long> documentsCodes)
		{
			if (CurDocType == null || documentsCodes.Count < 1)
			{
				return;
			}

			logger.Info("Запрос группы документов в базе...");

			DocsListStore.Clear();

			string sqlExtra = "";
			if (CurDocType.DBTableExsist)
			{
				sqlExtra = "LEFT JOIN extra_" 
							+ CurDocType.DBTableName 
							+ " ON extra_" 
							+ CurDocType.DBTableName 
							+ ".doc_id = docs.id ";
			}
			
			string sql = "SELECT * FROM docs " + sqlExtra + " WHERE docs.type_id = @typeId AND FIND_IN_SET(number, @documentsCodesList) ";

			if (!selectperiodDocs.IsAllTime)
			{
				sql += " AND date BETWEEN @startDate AND @endDate";
			}

			QSMain.CheckConnectionAlive();

			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);

			if (comboDocType.GetActiveIter(out TreeIter iter))
			{
				cmd.Parameters.AddWithValue("@typeId", comboDocType.Model.GetValue(iter, 1));
			}

			var documentsCodesParameterValue = string.Join(",", documentsCodes);
			cmd.Parameters.AddWithValue("@documentsCodesList", documentsCodesParameterValue);

			cmd.Parameters.AddWithValue("@startDate", selectperiodDocs.DateBegin);
			cmd.Parameters.AddWithValue("@endDate", selectperiodDocs.DateEnd);

			MySqlDataReader rdr = cmd.ExecuteReader();

			while (rdr.Read())
			{
				object[] Values = new object[4 + UsedExtraFields];
				Values[0] = rdr.GetInt32("id");
				if (rdr["number"] != DBNull.Value)
					Values[1] = rdr.GetString("number");
				else
					Values[1] = "";
				if (rdr["date"] != DBNull.Value)
					Values[2] = string.Format("{0:d}", rdr.GetDateTime("date"));
				else
					Values[2] = "";
				Values[3] = string.Format("{0}", rdr.GetDateTime("create_date"));

				if (CurDocType.FieldsList != null)
				{
					foreach (DocFieldInfo item in CurDocType.FieldsList)
					{
						if (!item.Display && !item.Search)
							continue;
						switch (item.Type)
						{
							case "varchar":
								if (rdr[item.DBName] != DBNull.Value)
									Values[item.ListStoreColumn] = rdr.GetString(item.DBName);
								else
									Values[item.ListStoreColumn] = "";
								break;
						}
					}
				}

				DocsListStore.AppendValues(Values);
			}

			rdr.Close();

			OnTreeviewDocsCursorChanged(null, null);

			int totaldoc = DocsListStore.IterNChildren();

			logger.Info(NumberToTextRus.FormatCase(totaldoc,
				"Получен {0} документ.",
				"Получено {0} документа.",
				"Получено {0} документов."));
		}

		void UpdateDocs()
		{
			if (CurDocType == null)
				return;
			logger.Info("Запрос документов в базе...");
			DocsListStore.Clear();

			string sqlExtra = "";
			if (CurDocType.DBTableExsist)
				sqlExtra = "LEFT JOIN extra_" + CurDocType.DBTableName + " ON extra_" + CurDocType.DBTableName +
					".doc_id = docs.id ";
			string sql = "SELECT * FROM docs " + sqlExtra + "WHERE docs.type_id = @type_id";
			if (!selectperiodDocs.IsAllTime)
				sql += " AND date BETWEEN @startdate AND @enddate";
			if (entryDocNumber.Text.Length > 0)
				sql += string.Format(" AND number LIKE '%{0}%' ", entryDocNumber.Text);
			QSMain.CheckConnectionAlive();
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			if (comboDocType.GetActiveIter(out TreeIter iter)) {
				cmd.Parameters.AddWithValue("@type_id", comboDocType.Model.GetValue(iter, 1));
			}
			cmd.Parameters.AddWithValue("@startdate", selectperiodDocs.DateBegin);
			cmd.Parameters.AddWithValue("@enddate", selectperiodDocs.DateEnd);
			MySqlDataReader rdr = cmd.ExecuteReader();

			while (rdr.Read()) {
				object[] Values = new object[4 + UsedExtraFields];
				Values[0] = rdr.GetInt32("id");
				if (rdr["number"] != DBNull.Value)
					Values[1] = rdr.GetString("number");
				else
					Values[1] = "";
				if (rdr["date"] != DBNull.Value)
					Values[2] = string.Format("{0:d}", rdr.GetDateTime("date"));
				else
					Values[2] = "";
				Values[3] = string.Format("{0}", rdr.GetDateTime("create_date"));

				if (CurDocType.FieldsList != null) {
					foreach (DocFieldInfo item in CurDocType.FieldsList) {
						if (!item.Display && !item.Search)
							continue;
						switch (item.Type) {
							case "varchar":
								if (rdr[item.DBName] != DBNull.Value)
									Values[item.ListStoreColumn] = rdr.GetString(item.DBName);
								else
									Values[item.ListStoreColumn] = "";
								break;
						}
					}
				}

				DocsListStore.AppendValues(Values);
			}
			rdr.Close();
			OnTreeviewDocsCursorChanged(null, null);
			int totaldoc = DocsListStore.IterNChildren();
			logger.Info(NumberToTextRus.FormatCase(totaldoc,
				"Получен {0} документ.",
				"Получено {0} документа.",
				"Получено {0} документов."));
		}

		protected void OnSelectperiodDocsDatesChanged(object sender, EventArgs e)
		{
			if(GetUpdDocs())
			{
				return;
			}

			if (selectperiodDocs.IsAllTime && entryDocNumber.Text.Length < 2)
				entryDocNumber.GrabFocus();
			else
				UpdateDocs();
		}

		protected void OnButtonInputClicked(object sender, EventArgs e)
		{
			if (InputDocsWin == null) {
				InputDocsWin = new InputDocs();
				InputDocsWin.DeleteEvent += OnDeleteInputDocsEvent;
				Console.WriteLine("new");
			}
			InputDocsWin.Maximize();
			InputDocsWin.Show();
		}

		protected void OnDeleteInputDocsEvent(object s, DeleteEventArgs arg)
		{
			InputDocsWin.Destroy();
			InputDocsWin = null;
		}

		protected void OnButtonOpenClicked(object sender, EventArgs e)
		{
			treeviewDocs.Selection.GetSelected(out TreeIter iter);
			int ItemId = (int)DocsListStore.GetValue(iter, 0);
			ViewDoc win = new ViewDoc();
			win.Fill(ItemId);
			win.Show();
			if ((ResponseType)win.Run() == ResponseType.Ok)
			{
				if (GetUpdDocs())
				{
					return;
				}
				UpdateDocs();
			}
			win.Destroy();
		}

		protected void OnButtonOpenAllClicked(object sender, EventArgs e)
		{
			if(DocsListStore.IterNChildren() < 1)
			{
				return;
			}

			var docIds = new List<int>();

			foreach(object[] item in DocsListStore)
			{
				if(item.Length > 0 && item[0] is int code)
				{
					docIds.Add(code);
				}
			}

			ViewDoc win = new ViewDoc();
			win.Fill(docIds);
			win.Show();
			if ((ResponseType)win.Run() == ResponseType.Ok)
			{
				if(GetUpdDocs())
				{
					return;
				}
				UpdateDocs();
			}
			win.Destroy();
		}

		protected void OnTreeviewDocsCursorChanged(object sender, EventArgs e)
		{
			bool RowSelected = treeviewDocs.Selection.CountSelectedRows() == 1;
			buttonOpen.Sensitive = RowSelected;
			ybuttonOpenAll.Sensitive = true;
			buttonDelete.Sensitive = RowSelected && QSMain.User.Permissions["can_edit"];
		}

		protected void OnTreeviewDocsRowActivated(object o, RowActivatedArgs args)
		{
			OnButtonOpenClicked(null, null);
		}

		protected void OnButtonDeleteClicked(object sender, EventArgs e)
		{
			treeviewDocs.Selection.GetSelected(out TreeIter iter);
			int itemid = (int)DocsListStore.GetValue(iter, 0);

			string sql = "DELETE FROM docs WHERE id = @id";
			QSMain.CheckConnectionAlive();
			MySqlCommand cmd = new MySqlCommand(sql, QSMain.connectionDB);
			cmd.Parameters.AddWithValue("@id", itemid);
			cmd.ExecuteNonQuery();
			UpdateDocs();
		}

		protected void OnButtonRefreshClicked(object sender, EventArgs e)
		{
			if (GetUpdDocs())
			{
				return;
			}

			UpdateDocs();
		}

		protected void OnAction3Activated(object sender, EventArgs e)
		{
			//throw new System.NotImplementedException ();
			Statistics stat = new Statistics();
			stat.Show();
			stat.Run();
			stat.Destroy();
		}

		protected void OnAboutActionActivated(object sender, EventArgs e)
		{			
			IProductService productService = new ProductService();
			AboutViewModel aboutViewModel = new AboutViewModel(applicationInfo, productService);
			AboutView aboutView = new AboutView(aboutViewModel);
			aboutView.Run();
			aboutView.Destroy();
		}

		protected void OnQuitActionActivated(object sender, EventArgs e)
		{
			Application.Quit();
		}

		protected void OnButtonSearchClicked(object sender, EventArgs e)
		{
			if(string.IsNullOrEmpty(entryDocNumber.Text))
			{
				return;
			}

			SelectedCounterparty = null;
			SelectedDeliveryPoint = null;

			yentryClient.Text = string.Empty;
			ClearComboboxAddresses();

			UpdateDocs();

		}

		protected void OnEntryDocNumberActivated(object sender, EventArgs e)
		{
			buttonSearch.Click();
		}

		protected void OnAction5Activated(object sender, EventArgs e)
		{
			QSMain.RunChangeLogDlg(this);
		}

		protected void OnActionUpdateActivated(object sender, EventArgs e)
		{
			//нужно разобраться как работает новый апдейтер
			//CheckUpdate.StartCheckUpdateThread (UpdaterFlags.ShowAnyway);
		}

		private void ShowGrpcServiceUnavailableMessage()
		{
			MessageDialogHelper.RunErrorDialog("Служба получения кодов не доступна");
		}
	}
}
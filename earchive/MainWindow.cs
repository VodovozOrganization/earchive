using earchive.UpdGrpc;
using EarchiveApi;
using Gamma.Binding.Core;
using Gtk;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using NHibernate.Criterion;
using NHibernate.Mapping;
using NLog;
using QS.Dialog.Gtk;
using QS.DomainModel.UoW;
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
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using ZXing.PDF417.Internal;

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

			//Настройка контролов поиска кодов УПД
            yentryClient.Completion = new EntryCompletion();
            yentryClient.Completion.Model = new ListStore(typeof(CounterpartyInfo));
            yentryClient.Completion.MinimumKeyLength = 2;
            yentryClient.Completion.TextColumn = 0;
            yentryClient.Completion.Complete();
            //var cell = new CellRendererText();
            //yentryClient.Completion.PackStart(cell, true);
            //yentryClient.Completion.SetCellDataFunc(cell, OnCellLayoutDataFunc);

            yentryClient.Completion.MatchSelected += OnCompletionMatchSelected;
            yentryClient.Completion.MatchFunc = CompletionMatchFunc;
            yentryClient.Changed += OnYentryClientChanged;
            yentryClient.FocusInEvent += OnYentryClientFocusInEvent;

			_earchiveUpdServiceClient = new EarchiveUpdServiceClient();
        }

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
				SetUpdSearchControlsSettings(CurrentTypeId);
				PrepareDocsTable();
				UpdateDocs();
			}
		}

        #region Поиск УПД
        private void SetUpdSearchControlsSettings(int currentDocementId)
        {
            yentryClient.Text = string.Empty;
            comboboxentryAddress.Clear();
            var selectedUpdDocumentType = currentDocementId == 5;

            if (selectedUpdDocumentType)
			{
                yentryClient.Visible = true;
                yentryClient.Sensitive = true;

                comboboxentryAddress.Visible = true;
                comboboxentryAddress.Sensitive = true;

				return;
            }

            yentryClient.Visible = false;
			yentryClient.Sensitive = false;

            comboboxentryAddress.Visible = false;
            comboboxentryAddress.Sensitive = false;
        }

		private void CounterpartyEntryFillAutocomplete()
        {
            var completionListStore = new ListStore(typeof(CounterpartyInfo));

            var items = _earchiveUpdServiceClient
				.GetCounterparties(yentryClient.Text.ToLower())
				.Select(c => c)
				.ToList();

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
            var streetName = (CounterpartyInfo)tree_model.GetValue(iter, 1);
            //var streetTypeName = (string)tree_model.GetValue(iter, (int)columns.StreetTypeName);
            //var streetDistrict = (string)tree_model.GetValue(iter, (int)columns.StreeDistrict);
            //var pattern = string.Format("\\b{0}", Regex.Escape(Text.ToLower()));
            //streetName = Regex.Replace(streetName, pattern, match => $"<b>{match.Value}</b>", RegexOptions.IgnoreCase);

            //if (!string.IsNullOrWhiteSpace(streetDistrict))
            //{
            //    streetDistrict = $"({streetDistrict})";
            //}

            //((CellRendererText)cell).Markup =
            //    string.IsNullOrWhiteSpace(streetDistrict) ? $"{streetTypeName.ToLower()} {streetName}" :
            //        $"{streetTypeName.ToLower()} {streetName} {streetDistrict}";
        }

        private void OnYentryClientChanged(object sender, EventArgs e)
        {
			CounterpartyEntryFillAutocomplete();
        }

        private void OnYentryClientFocusInEvent(object o, FocusInEventArgs args)
        {
            //CounterpartyEntryFillAutocomplete();
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
			var counterpartyInfo = (CounterpartyInfo)completion.Model.GetValue(iter, 0);
            var value = counterpartyInfo.Name.ToLower();
			bool isMatch = key == value;

			if(isMatch)
			{
				GetAllCounterpartyAdresses(counterpartyInfo);
            }

			return isMatch;
		}

        [GLib.ConnectBefore]
        void OnCompletionMatchSelected(object o, MatchSelectedArgs args)
        {
			var selectedCounterparty = (CounterpartyInfo)args.Model.GetValue(args.Iter, 0);
            yentryClient.Text = selectedCounterparty.Name;
			GetAllCounterpartyAdresses(selectedCounterparty);
            args.RetVal = true;
        }

		private void GetAllCounterpartyAdresses(CounterpartyInfo selectedCounterparty)
		{
            var deliveryPoints = _earchiveUpdServiceClient
                .GetDeliveryPoints(selectedCounterparty)
                .Select(d => d)
                .ToList();
        }
        #endregion

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
				UpdateDocs();
			win.Destroy();
		}

		protected void OnButtonOpenAllClicked(object sender, EventArgs e)
		{
			treeviewDocs.Selection.GetSelected(out TreeIter iter);
			int ItemId = (int)DocsListStore.GetValue(iter, 0);
			ViewDoc win = new ViewDoc();
			win.Fill(ItemId);
			win.Show();
			if ((ResponseType)win.Run() == ResponseType.Ok)
				UpdateDocs();
			win.Destroy();
		}

		protected void OnTreeviewDocsCursorChanged(object sender, EventArgs e)
		{
			bool RowSelected = treeviewDocs.Selection.CountSelectedRows() == 1;
			buttonOpen.Sensitive = RowSelected;
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
	}
}

// This file has been generated by the GUI designer. Do not modify.
namespace earchive
{
	public partial class MainWindow
	{
		private global::Gtk.UIManager UIManager;
		private global::Gtk.Action Action;
		private global::Gtk.Action dialogAuthenticationAction;
		private global::Gtk.Action UsersAction;
		private global::Gtk.Action quitAction;
		private global::Gtk.Action Action1;
		private global::Gtk.Action ActionDocTypes;
		private global::Gtk.Action Action3;
		private global::Gtk.Action Action4;
		private global::Gtk.Action aboutAction;
		private global::Gtk.VBox vbox2;
		private global::Gtk.MenuBar menubar1;
		private global::Gtk.HBox hbox2;
		private global::Gtk.Table table1;
		private global::Gtk.ComboBox comboDocType;
		private global::Gtk.Entry entryDocNumber;
		private global::Gtk.Label label1;
		private global::Gtk.Label label2;
		private global::QSWidgetLib.SelectPeriod selectperiodDocs;
		private global::Gtk.Button buttonInput;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TreeView treeviewDocs;
		private global::Gtk.HBox hbox7;
		private global::Gtk.Button buttonOpen;
		private global::Gtk.Button buttonDelete;
		private global::Gtk.Button buttonRefresh;
		private global::Gtk.Statusbar statusbar1;
		private global::Gtk.Label labelUser;
		private global::Gtk.Label labelStatus;

		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget earchive.MainWindow
			this.UIManager = new global::Gtk.UIManager ();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup ("Default");
			this.Action = new global::Gtk.Action ("Action", global::Mono.Unix.Catalog.GetString ("Файл"), null, null);
			this.Action.ShortLabel = global::Mono.Unix.Catalog.GetString ("Файл");
			w1.Add (this.Action, null);
			this.dialogAuthenticationAction = new global::Gtk.Action ("dialogAuthenticationAction", global::Mono.Unix.Catalog.GetString ("Изменить пароль"), null, "gtk-dialog-authentication");
			this.dialogAuthenticationAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Изменить пароль");
			w1.Add (this.dialogAuthenticationAction, null);
			this.UsersAction = new global::Gtk.Action ("UsersAction", global::Mono.Unix.Catalog.GetString ("Пользователи"), null, "gtk-properties");
			this.UsersAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("Пользователи");
			w1.Add (this.UsersAction, null);
			this.quitAction = new global::Gtk.Action ("quitAction", global::Mono.Unix.Catalog.GetString ("В_ыход"), null, "gtk-quit");
			this.quitAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("В_ыход");
			w1.Add (this.quitAction, null);
			this.Action1 = new global::Gtk.Action ("Action1", global::Mono.Unix.Catalog.GetString ("Настройка"), null, null);
			this.Action1.ShortLabel = global::Mono.Unix.Catalog.GetString ("Настройка");
			w1.Add (this.Action1, null);
			this.ActionDocTypes = new global::Gtk.Action ("ActionDocTypes", global::Mono.Unix.Catalog.GetString ("Типы документов"), null, null);
			this.ActionDocTypes.ShortLabel = global::Mono.Unix.Catalog.GetString ("Типы документов");
			w1.Add (this.ActionDocTypes, null);
			this.Action3 = new global::Gtk.Action ("Action3", global::Mono.Unix.Catalog.GetString ("Статистика"), null, null);
			this.Action3.ShortLabel = global::Mono.Unix.Catalog.GetString ("Статистика");
			w1.Add (this.Action3, null);
			this.Action4 = new global::Gtk.Action ("Action4", global::Mono.Unix.Catalog.GetString ("Справка"), null, null);
			this.Action4.ShortLabel = global::Mono.Unix.Catalog.GetString ("Справка");
			w1.Add (this.Action4, null);
			this.aboutAction = new global::Gtk.Action ("aboutAction", global::Mono.Unix.Catalog.GetString ("_О программе"), null, "gtk-about");
			this.aboutAction.ShortLabel = global::Mono.Unix.Catalog.GetString ("_О программе");
			w1.Add (this.aboutAction, null);
			this.UIManager.InsertActionGroup (w1, 0);
			this.AddAccelGroup (this.UIManager.AccelGroup);
			this.Name = "earchive.MainWindow";
			this.Title = global::Mono.Unix.Catalog.GetString ("QS: Электронный архив");
			this.Icon = global::Gdk.Pixbuf.LoadFromResource ("earchive.icons.logo.png");
			this.WindowPosition = ((global::Gtk.WindowPosition)(1));
			// Container child earchive.MainWindow.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox ();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString ("<ui><menubar name='menubar1'><menu name='Action' action='Action'><menuitem name='dialogAuthenticationAction' action='dialogAuthenticationAction'/><menuitem name='UsersAction' action='UsersAction'/><separator/><menuitem name='quitAction' action='quitAction'/></menu><menu name='Action1' action='Action1'><menuitem name='ActionDocTypes' action='ActionDocTypes'/><menuitem name='Action3' action='Action3'/></menu><menu name='Action4' action='Action4'><menuitem name='aboutAction' action='aboutAction'/></menu></menubar></ui>");
			this.menubar1 = ((global::Gtk.MenuBar)(this.UIManager.GetWidget ("/menubar1")));
			this.menubar1.Name = "menubar1";
			this.vbox2.Add (this.menubar1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.menubar1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox ();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			this.hbox2.BorderWidth = ((uint)(6));
			// Container child hbox2.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table (((uint)(3)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboDocType = new global::Gtk.ComboBox ();
			this.comboDocType.Name = "comboDocType";
			this.table1.Add (this.comboDocType);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1 [this.comboDocType]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.XOptions = ((global::Gtk.AttachOptions)(4));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryDocNumber = new global::Gtk.Entry ();
			this.entryDocNumber.WidthRequest = 80;
			this.entryDocNumber.CanFocus = true;
			this.entryDocNumber.Name = "entryDocNumber";
			this.entryDocNumber.IsEditable = true;
			this.entryDocNumber.InvisibleChar = '●';
			this.table1.Add (this.entryDocNumber);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1 [this.entryDocNumber]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.XOptions = ((global::Gtk.AttachOptions)(4));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label ();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString ("Тип документа:");
			this.table1.Add (this.label1);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1 [this.label1]));
			w5.XOptions = ((global::Gtk.AttachOptions)(4));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label ();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString ("Номер:");
			this.table1.Add (this.label2);
			global::Gtk.Table.TableChild w6 = ((global::Gtk.Table.TableChild)(this.table1 [this.label2]));
			w6.TopAttach = ((uint)(1));
			w6.BottomAttach = ((uint)(2));
			w6.XOptions = ((global::Gtk.AttachOptions)(4));
			w6.YOptions = ((global::Gtk.AttachOptions)(4));
			this.hbox2.Add (this.table1);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.table1]));
			w7.Position = 0;
			w7.Expand = false;
			w7.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.selectperiodDocs = new global::QSWidgetLib.SelectPeriod ();
			this.selectperiodDocs.Events = ((global::Gdk.EventMask)(256));
			this.selectperiodDocs.Name = "selectperiodDocs";
			this.selectperiodDocs.DateBegin = new global::System.DateTime (0);
			this.selectperiodDocs.DateEnd = new global::System.DateTime (0);
			this.selectperiodDocs.AutoDateSeparation = true;
			this.selectperiodDocs.ShowToday = true;
			this.selectperiodDocs.ShowWeek = true;
			this.selectperiodDocs.ShowMonth = true;
			this.selectperiodDocs.Show3Month = true;
			this.selectperiodDocs.Show6Month = true;
			this.selectperiodDocs.ShowYear = false;
			this.selectperiodDocs.ShowAllTime = true;
			this.selectperiodDocs.ShowCurWeek = false;
			this.selectperiodDocs.ShowCurMonth = false;
			this.selectperiodDocs.ShowCurQuarter = false;
			this.selectperiodDocs.ShowCurYear = false;
			this.hbox2.Add (this.selectperiodDocs);
			global::Gtk.Box.BoxChild w8 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.selectperiodDocs]));
			w8.Position = 1;
			w8.Expand = false;
			w8.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.buttonInput = new global::Gtk.Button ();
			this.buttonInput.CanFocus = true;
			this.buttonInput.Name = "buttonInput";
			this.buttonInput.UseUnderline = true;
			this.buttonInput.Label = global::Mono.Unix.Catalog.GetString ("Ввод документов");
			global::Gtk.Image w9 = new global::Gtk.Image ();
			w9.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-new", global::Gtk.IconSize.LargeToolbar);
			this.buttonInput.Image = w9;
			this.hbox2.Add (this.buttonInput);
			global::Gtk.Box.BoxChild w10 = ((global::Gtk.Box.BoxChild)(this.hbox2 [this.buttonInput]));
			w10.PackType = ((global::Gtk.PackType)(1));
			w10.Position = 2;
			w10.Expand = false;
			w10.Fill = false;
			this.vbox2.Add (this.hbox2);
			global::Gtk.Box.BoxChild w11 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox2]));
			w11.Position = 1;
			w11.Expand = false;
			w11.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewDocs = new global::Gtk.TreeView ();
			this.treeviewDocs.CanFocus = true;
			this.treeviewDocs.Name = "treeviewDocs";
			this.GtkScrolledWindow.Add (this.treeviewDocs);
			this.vbox2.Add (this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.GtkScrolledWindow]));
			w13.Position = 2;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox7 = new global::Gtk.HBox ();
			this.hbox7.Name = "hbox7";
			this.hbox7.Spacing = 6;
			this.hbox7.BorderWidth = ((uint)(3));
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonOpen = new global::Gtk.Button ();
			this.buttonOpen.Sensitive = false;
			this.buttonOpen.CanFocus = true;
			this.buttonOpen.Name = "buttonOpen";
			this.buttonOpen.UseUnderline = true;
			this.buttonOpen.Label = global::Mono.Unix.Catalog.GetString ("Открыть");
			global::Gtk.Image w14 = new global::Gtk.Image ();
			w14.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-edit", global::Gtk.IconSize.SmallToolbar);
			this.buttonOpen.Image = w14;
			this.hbox7.Add (this.buttonOpen);
			global::Gtk.Box.BoxChild w15 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonOpen]));
			w15.Position = 0;
			w15.Expand = false;
			w15.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonDelete = new global::Gtk.Button ();
			this.buttonDelete.Sensitive = false;
			this.buttonDelete.CanFocus = true;
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.UseUnderline = true;
			this.buttonDelete.Label = global::Mono.Unix.Catalog.GetString ("Удалить");
			global::Gtk.Image w16 = new global::Gtk.Image ();
			w16.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-delete", global::Gtk.IconSize.SmallToolbar);
			this.buttonDelete.Image = w16;
			this.hbox7.Add (this.buttonDelete);
			global::Gtk.Box.BoxChild w17 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonDelete]));
			w17.Position = 1;
			w17.Expand = false;
			w17.Fill = false;
			// Container child hbox7.Gtk.Box+BoxChild
			this.buttonRefresh = new global::Gtk.Button ();
			this.buttonRefresh.Sensitive = false;
			this.buttonRefresh.CanFocus = true;
			this.buttonRefresh.Name = "buttonRefresh";
			this.buttonRefresh.UseUnderline = true;
			this.buttonRefresh.Label = global::Mono.Unix.Catalog.GetString ("Обновить");
			global::Gtk.Image w18 = new global::Gtk.Image ();
			w18.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-refresh", global::Gtk.IconSize.SmallToolbar);
			this.buttonRefresh.Image = w18;
			this.hbox7.Add (this.buttonRefresh);
			global::Gtk.Box.BoxChild w19 = ((global::Gtk.Box.BoxChild)(this.hbox7 [this.buttonRefresh]));
			w19.PackType = ((global::Gtk.PackType)(1));
			w19.Position = 2;
			w19.Expand = false;
			w19.Fill = false;
			this.vbox2.Add (this.hbox7);
			global::Gtk.Box.BoxChild w20 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.hbox7]));
			w20.Position = 3;
			w20.Expand = false;
			w20.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.statusbar1 = new global::Gtk.Statusbar ();
			this.statusbar1.Name = "statusbar1";
			this.statusbar1.Spacing = 6;
			// Container child statusbar1.Gtk.Box+BoxChild
			this.labelUser = new global::Gtk.Label ();
			this.labelUser.Name = "labelUser";
			this.labelUser.LabelProp = global::Mono.Unix.Catalog.GetString ("Пользователь");
			this.statusbar1.Add (this.labelUser);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(this.statusbar1 [this.labelUser]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			w21.Padding = ((uint)(4));
			// Container child statusbar1.Gtk.Box+BoxChild
			this.labelStatus = new global::Gtk.Label ();
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.LabelProp = global::Mono.Unix.Catalog.GetString ("OK");
			this.statusbar1.Add (this.labelStatus);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(this.statusbar1 [this.labelStatus]));
			w22.Position = 3;
			w22.Expand = false;
			w22.Fill = false;
			this.vbox2.Add (this.statusbar1);
			global::Gtk.Box.BoxChild w23 = ((global::Gtk.Box.BoxChild)(this.vbox2 [this.statusbar1]));
			w23.Position = 4;
			w23.Expand = false;
			w23.Fill = false;
			this.Add (this.vbox2);
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 771;
			this.DefaultHeight = 457;
			this.Show ();
			this.DeleteEvent += new global::Gtk.DeleteEventHandler (this.OnDeleteEvent);
			this.dialogAuthenticationAction.Activated += new global::System.EventHandler (this.OnDialogAuthenticationActionActivated);
			this.UsersAction.Activated += new global::System.EventHandler (this.OnUsersActionActivated);
			this.ActionDocTypes.Activated += new global::System.EventHandler (this.OnAction2Activated);
			this.Action3.Activated += new global::System.EventHandler (this.OnAction3Activated);
			this.aboutAction.Activated += new global::System.EventHandler (this.OnAboutActionActivated);
			this.entryDocNumber.Changed += new global::System.EventHandler (this.OnEntryDocNumberChanged);
			this.comboDocType.Changed += new global::System.EventHandler (this.OnComboDocTypeChanged);
			this.selectperiodDocs.DatesChanged += new global::System.EventHandler (this.OnSelectperiodDocsDatesChanged);
			this.buttonInput.Clicked += new global::System.EventHandler (this.OnButtonInputClicked);
			this.treeviewDocs.CursorChanged += new global::System.EventHandler (this.OnTreeviewDocsCursorChanged);
			this.treeviewDocs.RowActivated += new global::Gtk.RowActivatedHandler (this.OnTreeviewDocsRowActivated);
			this.buttonOpen.Clicked += new global::System.EventHandler (this.OnButtonOpenClicked);
			this.buttonDelete.Clicked += new global::System.EventHandler (this.OnButtonDeleteClicked);
			this.buttonRefresh.Clicked += new global::System.EventHandler (this.OnButtonRefreshClicked);
		}
	}
}

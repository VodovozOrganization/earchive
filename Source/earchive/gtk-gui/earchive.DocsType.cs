
// This file has been generated by the GUI designer. Do not modify.
namespace earchive
{
	public partial class DocsType
	{
		private global::Gtk.UIManager UIManager;

		private global::Gtk.Action recognizeAction;

		private global::Gtk.Action openAction;

		private global::Gtk.Action saveAsAction;

		private global::Gtk.Action clearAction;

		private global::Gtk.Table table1;

		private global::Gtk.Entry entryID;

		private global::Gtk.Entry entryName;

		private global::Gtk.Entry entryType;

		private global::Gtk.Frame frame1;

		private global::Gtk.Alignment GtkAlignment6;

		private global::Gtk.VBox vbox2;

		private global::Gtk.Label labelTemplateName;

		private global::Gtk.Toolbar toolbarTemplate;

		private global::Gtk.Label GtkLabel6;

		private global::Gtk.ScrolledWindow GtkScrolledWindow1;

		private global::Gtk.TextView textviewDescription;

		private global::Gtk.HBox hbox2;

		private global::Gtk.Label label5;

		private global::Gtk.Entry entryDBTable;

		private global::Gtk.Label label1;

		private global::Gtk.Label label2;

		private global::Gtk.Label label3;

		private global::Gtk.Label label6;

		private global::Gtk.Label label7;

		private global::Gtk.Label label4;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.TreeView treeviewFields;

		private global::Gtk.HBox hbox1;

		private global::Gtk.Button buttonAdd;

		private global::Gtk.Button buttonEdit;

		private global::Gtk.Button buttonDelete;

		private global::Gtk.Button buttonApply;

		private global::Gtk.Button buttonCancel;

		private global::Gtk.Button buttonOk;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget earchive.DocsType
			this.UIManager = new global::Gtk.UIManager();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup("Default");
			this.recognizeAction = new global::Gtk.Action("recognizeAction", null, null, "recognize");
			w1.Add(this.recognizeAction, null);
			this.openAction = new global::Gtk.Action("openAction", null, null, "gtk-open");
			w1.Add(this.openAction, null);
			this.saveAsAction = new global::Gtk.Action("saveAsAction", null, null, "gtk-save-as");
			w1.Add(this.saveAsAction, null);
			this.clearAction = new global::Gtk.Action("clearAction", null, null, "gtk-clear");
			w1.Add(this.clearAction, null);
			this.UIManager.InsertActionGroup(w1, 0);
			this.AddAccelGroup(this.UIManager.AccelGroup);
			this.Name = "earchive.DocsType";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child earchive.DocsType.VBox
			global::Gtk.VBox w2 = this.VBox;
			w2.Name = "dialog1_VBox";
			w2.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(5)), ((uint)(3)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			this.table1.BorderWidth = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.entryID = new global::Gtk.Entry();
			this.entryID.Sensitive = false;
			this.entryID.CanFocus = true;
			this.entryID.Name = "entryID";
			this.entryID.IsEditable = true;
			this.entryID.InvisibleChar = '●';
			this.table1.Add(this.entryID);
			global::Gtk.Table.TableChild w3 = ((global::Gtk.Table.TableChild)(this.table1[this.entryID]));
			w3.LeftAttach = ((uint)(1));
			w3.RightAttach = ((uint)(2));
			w3.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryName = new global::Gtk.Entry();
			this.entryName.CanFocus = true;
			this.entryName.Name = "entryName";
			this.entryName.IsEditable = true;
			this.entryName.InvisibleChar = '●';
			this.table1.Add(this.entryName);
			global::Gtk.Table.TableChild w4 = ((global::Gtk.Table.TableChild)(this.table1[this.entryName]));
			w4.TopAttach = ((uint)(1));
			w4.BottomAttach = ((uint)(2));
			w4.LeftAttach = ((uint)(1));
			w4.RightAttach = ((uint)(2));
			w4.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.entryType = new global::Gtk.Entry();
			this.entryType.CanFocus = true;
			this.entryType.Name = "entryType";
			this.entryType.IsEditable = true;
			this.entryType.InvisibleChar = '●';
			this.table1.Add(this.entryType);
			global::Gtk.Table.TableChild w5 = ((global::Gtk.Table.TableChild)(this.table1[this.entryType]));
			w5.TopAttach = ((uint)(2));
			w5.BottomAttach = ((uint)(3));
			w5.LeftAttach = ((uint)(1));
			w5.RightAttach = ((uint)(2));
			w5.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.frame1 = new global::Gtk.Frame();
			this.frame1.Name = "frame1";
			// Container child frame1.Gtk.Container+ContainerChild
			this.GtkAlignment6 = new global::Gtk.Alignment(0F, 0F, 1F, 1F);
			this.GtkAlignment6.Name = "GtkAlignment6";
			this.GtkAlignment6.LeftPadding = ((uint)(12));
			// Container child GtkAlignment6.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.labelTemplateName = new global::Gtk.Label();
			this.labelTemplateName.Name = "labelTemplateName";
			this.labelTemplateName.LabelProp = global::Mono.Unix.Catalog.GetString("Отсутствует");
			this.labelTemplateName.Wrap = true;
			this.vbox2.Add(this.labelTemplateName);
			global::Gtk.Box.BoxChild w6 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.labelTemplateName]));
			w6.Position = 0;
			w6.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString(@"<ui><toolbar name='toolbarTemplate'><toolitem name='recognizeAction' action='recognizeAction'/><toolitem name='openAction' action='openAction'/><toolitem name='saveAsAction' action='saveAsAction'/><toolitem name='clearAction' action='clearAction'/></toolbar></ui>");
			this.toolbarTemplate = ((global::Gtk.Toolbar)(this.UIManager.GetWidget("/toolbarTemplate")));
			this.toolbarTemplate.Name = "toolbarTemplate";
			this.toolbarTemplate.ShowArrow = false;
			this.toolbarTemplate.ToolbarStyle = ((global::Gtk.ToolbarStyle)(0));
			this.toolbarTemplate.IconSize = ((global::Gtk.IconSize)(4));
			this.vbox2.Add(this.toolbarTemplate);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.toolbarTemplate]));
			w7.Position = 1;
			w7.Fill = false;
			this.GtkAlignment6.Add(this.vbox2);
			this.frame1.Add(this.GtkAlignment6);
			this.GtkLabel6 = new global::Gtk.Label();
			this.GtkLabel6.Name = "GtkLabel6";
			this.GtkLabel6.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Шаблон распознования</b>");
			this.GtkLabel6.UseMarkup = true;
			this.frame1.LabelWidget = this.GtkLabel6;
			this.table1.Add(this.frame1);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.frame1]));
			w10.BottomAttach = ((uint)(4));
			w10.LeftAttach = ((uint)(2));
			w10.RightAttach = ((uint)(3));
			w10.XOptions = ((global::Gtk.AttachOptions)(4));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.GtkScrolledWindow1 = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow1.Name = "GtkScrolledWindow1";
			this.GtkScrolledWindow1.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow1.Gtk.Container+ContainerChild
			this.textviewDescription = new global::Gtk.TextView();
			this.textviewDescription.CanFocus = true;
			this.textviewDescription.Name = "textviewDescription";
			this.textviewDescription.WrapMode = ((global::Gtk.WrapMode)(2));
			this.GtkScrolledWindow1.Add(this.textviewDescription);
			this.table1.Add(this.GtkScrolledWindow1);
			global::Gtk.Table.TableChild w12 = ((global::Gtk.Table.TableChild)(this.table1[this.GtkScrolledWindow1]));
			w12.TopAttach = ((uint)(4));
			w12.BottomAttach = ((uint)(5));
			w12.LeftAttach = ((uint)(1));
			w12.RightAttach = ((uint)(3));
			w12.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.label5 = new global::Gtk.Label();
			this.label5.Name = "label5";
			this.label5.LabelProp = global::Mono.Unix.Catalog.GetString("<b>extra_</b>");
			this.label5.UseMarkup = true;
			this.hbox2.Add(this.label5);
			global::Gtk.Box.BoxChild w13 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.label5]));
			w13.Position = 0;
			w13.Expand = false;
			w13.Fill = false;
			// Container child hbox2.Gtk.Box+BoxChild
			this.entryDBTable = new global::Gtk.Entry();
			this.entryDBTable.CanFocus = true;
			this.entryDBTable.Name = "entryDBTable";
			this.entryDBTable.IsEditable = true;
			this.entryDBTable.MaxLength = 10;
			this.entryDBTable.InvisibleChar = '●';
			this.hbox2.Add(this.entryDBTable);
			global::Gtk.Box.BoxChild w14 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.entryDBTable]));
			w14.Position = 1;
			this.table1.Add(this.hbox2);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.table1[this.hbox2]));
			w15.TopAttach = ((uint)(3));
			w15.BottomAttach = ((uint)(4));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.Xalign = 1F;
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("Код:");
			this.table1.Add(this.label1);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.table1[this.label1]));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.Xalign = 1F;
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Название<span foreground=\"red\">*</span>:");
			this.label2.UseMarkup = true;
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w17 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w17.TopAttach = ((uint)(1));
			w17.BottomAttach = ((uint)(2));
			w17.XOptions = ((global::Gtk.AttachOptions)(4));
			w17.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label3 = new global::Gtk.Label();
			this.label3.Name = "label3";
			this.label3.Xalign = 1F;
			this.label3.LabelProp = global::Mono.Unix.Catalog.GetString("Имя таблицы БД:");
			this.label3.UseMarkup = true;
			this.table1.Add(this.label3);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.table1[this.label3]));
			w18.TopAttach = ((uint)(3));
			w18.BottomAttach = ((uint)(4));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label6 = new global::Gtk.Label();
			this.label6.Name = "label6";
			this.label6.Xalign = 1F;
			this.label6.Yalign = 0F;
			this.label6.LabelProp = global::Mono.Unix.Catalog.GetString("Описание:");
			this.table1.Add(this.label6);
			global::Gtk.Table.TableChild w19 = ((global::Gtk.Table.TableChild)(this.table1[this.label6]));
			w19.TopAttach = ((uint)(4));
			w19.BottomAttach = ((uint)(5));
			w19.XOptions = ((global::Gtk.AttachOptions)(4));
			w19.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label7 = new global::Gtk.Label();
			this.label7.Name = "label7";
			this.label7.Xalign = 1F;
			this.label7.LabelProp = global::Mono.Unix.Catalog.GetString("Идентификатор типа:");
			this.table1.Add(this.label7);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.table1[this.label7]));
			w20.TopAttach = ((uint)(2));
			w20.BottomAttach = ((uint)(3));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			w2.Add(this.table1);
			global::Gtk.Box.BoxChild w21 = ((global::Gtk.Box.BoxChild)(w2[this.table1]));
			w21.Position = 0;
			w21.Expand = false;
			w21.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.label4 = new global::Gtk.Label();
			this.label4.Name = "label4";
			this.label4.Xalign = 0F;
			this.label4.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Дополнительные поля документа</b>");
			this.label4.UseMarkup = true;
			w2.Add(this.label4);
			global::Gtk.Box.BoxChild w22 = ((global::Gtk.Box.BoxChild)(w2[this.label4]));
			w22.Position = 1;
			w22.Expand = false;
			w22.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			this.GtkScrolledWindow.BorderWidth = ((uint)(3));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewFields = new global::Gtk.TreeView();
			this.treeviewFields.CanFocus = true;
			this.treeviewFields.Name = "treeviewFields";
			this.GtkScrolledWindow.Add(this.treeviewFields);
			w2.Add(this.GtkScrolledWindow);
			global::Gtk.Box.BoxChild w24 = ((global::Gtk.Box.BoxChild)(w2[this.GtkScrolledWindow]));
			w24.Position = 2;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			this.hbox1.BorderWidth = ((uint)(3));
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonAdd = new global::Gtk.Button();
			this.buttonAdd.Sensitive = false;
			this.buttonAdd.CanFocus = true;
			this.buttonAdd.Name = "buttonAdd";
			this.buttonAdd.UseUnderline = true;
			this.buttonAdd.Label = global::Mono.Unix.Catalog.GetString("Добавить");
			global::Gtk.Image w25 = new global::Gtk.Image();
			w25.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-add", global::Gtk.IconSize.Menu);
			this.buttonAdd.Image = w25;
			this.hbox1.Add(this.buttonAdd);
			global::Gtk.Box.BoxChild w26 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonAdd]));
			w26.Position = 0;
			w26.Expand = false;
			w26.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonEdit = new global::Gtk.Button();
			this.buttonEdit.Sensitive = false;
			this.buttonEdit.CanFocus = true;
			this.buttonEdit.Name = "buttonEdit";
			this.buttonEdit.UseUnderline = true;
			this.buttonEdit.Label = global::Mono.Unix.Catalog.GetString("Изменить");
			global::Gtk.Image w27 = new global::Gtk.Image();
			w27.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-edit", global::Gtk.IconSize.Menu);
			this.buttonEdit.Image = w27;
			this.hbox1.Add(this.buttonEdit);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonEdit]));
			w28.Position = 1;
			w28.Expand = false;
			w28.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.buttonDelete = new global::Gtk.Button();
			this.buttonDelete.Sensitive = false;
			this.buttonDelete.CanFocus = true;
			this.buttonDelete.Name = "buttonDelete";
			this.buttonDelete.UseUnderline = true;
			this.buttonDelete.Label = global::Mono.Unix.Catalog.GetString("Удалить");
			global::Gtk.Image w29 = new global::Gtk.Image();
			w29.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-remove", global::Gtk.IconSize.Menu);
			this.buttonDelete.Image = w29;
			this.hbox1.Add(this.buttonDelete);
			global::Gtk.Box.BoxChild w30 = ((global::Gtk.Box.BoxChild)(this.hbox1[this.buttonDelete]));
			w30.Position = 2;
			w30.Expand = false;
			w30.Fill = false;
			w2.Add(this.hbox1);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(w2[this.hbox1]));
			w31.Position = 3;
			w31.Expand = false;
			w31.Fill = false;
			// Internal child earchive.DocsType.ActionArea
			global::Gtk.HButtonBox w32 = this.ActionArea;
			w32.Name = "dialog1_ActionArea";
			w32.Spacing = 10;
			w32.BorderWidth = ((uint)(5));
			w32.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonApply = new global::Gtk.Button();
			this.buttonApply.Sensitive = false;
			this.buttonApply.CanFocus = true;
			this.buttonApply.Name = "buttonApply";
			this.buttonApply.UseUnderline = true;
			this.buttonApply.Label = global::Mono.Unix.Catalog.GetString("Применить");
			global::Gtk.Image w33 = new global::Gtk.Image();
			w33.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-apply", global::Gtk.IconSize.Menu);
			this.buttonApply.Image = w33;
			w32.Add(this.buttonApply);
			global::Gtk.ButtonBox.ButtonBoxChild w34 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w32[this.buttonApply]));
			w34.Expand = false;
			w34.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = global::Mono.Unix.Catalog.GetString("О_тменить");
			global::Gtk.Image w35 = new global::Gtk.Image();
			w35.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-cancel", global::Gtk.IconSize.Menu);
			this.buttonCancel.Image = w35;
			this.AddActionWidget(this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w36 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w32[this.buttonCancel]));
			w36.Position = 1;
			w36.Expand = false;
			w36.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button();
			this.buttonOk.Sensitive = false;
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = global::Mono.Unix.Catalog.GetString("_OK");
			global::Gtk.Image w37 = new global::Gtk.Image();
			w37.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-ok", global::Gtk.IconSize.Menu);
			this.buttonOk.Image = w37;
			this.AddActionWidget(this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w38 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w32[this.buttonOk]));
			w38.Position = 2;
			w38.Expand = false;
			w38.Fill = false;
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 669;
			this.DefaultHeight = 400;
			this.Show();
			this.recognizeAction.Activated += new global::System.EventHandler(this.OnRecognizeActionActivated);
			this.openAction.Activated += new global::System.EventHandler(this.OnOpenActionActivated);
			this.saveAsAction.Activated += new global::System.EventHandler(this.OnSaveAsActionActivated);
			this.clearAction.Activated += new global::System.EventHandler(this.OnClearActionActivated);
			this.entryDBTable.Changed += new global::System.EventHandler(this.OnEntryDBTableChanged);
			this.entryName.Changed += new global::System.EventHandler(this.OnEntryNameChanged);
			this.treeviewFields.CursorChanged += new global::System.EventHandler(this.OnTreeviewFieldsCursorChanged);
			this.treeviewFields.RowActivated += new global::Gtk.RowActivatedHandler(this.OnTreeviewFieldsRowActivated);
			this.buttonAdd.Clicked += new global::System.EventHandler(this.OnButtonAddClicked);
			this.buttonEdit.Clicked += new global::System.EventHandler(this.OnButtonEditClicked);
			this.buttonDelete.Clicked += new global::System.EventHandler(this.OnButtonDeleteClicked);
			this.buttonApply.Clicked += new global::System.EventHandler(this.OnButtonApplyClicked);
			this.buttonOk.Clicked += new global::System.EventHandler(this.OnButtonOkClicked);
		}
	}
}

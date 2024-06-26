
// This file has been generated by the GUI designer. Do not modify.
namespace earchive
{
	public partial class InputDocs
	{
		private global::Gtk.UIManager UIManager;

		private global::Gtk.Action openAction;

		private global::Gtk.Action ScanAction;

		private global::Gtk.Action clearAction;

		private global::Gtk.Action saveAction;

		private global::Gtk.Action zoom100Action;

		private global::Gtk.Action zoomFitAction;

		private global::Gtk.Action zoomInAction;

		private global::Gtk.Action zoomOutAction;

		private global::Gtk.Action Rotate90Action;

		private global::Gtk.Action Rotate180Action;

		private global::Gtk.Action Rotate270Action;

		private global::Gtk.Action ActionRecognize;

		private global::Gtk.VBox vbox2;

		private global::Gtk.Toolbar toolbar1;

		private global::Gtk.HBox hbox2;

		private global::Gtk.HPaned hpaned1;

		private global::Gtk.ScrolledWindow GtkScrolledWindow;

		private global::Gtk.TreeView treeviewImages;

		private global::Gtk.ScrolledWindow scrolledImage;

		private global::Gtk.Image imageDoc;

		private global::Gtk.VBox vbox3;

		private global::Gtk.Table table1;

		private global::Gamma.Widgets.yListComboBox comboScaner;

		private global::Gtk.Label label2;

		private global::Gtk.Table tableFieldWidgets;

		private global::Gtk.ComboBox comboType;

		private global::QSWidgetLib.DatePicker dateDoc;

		private global::Gamma.GtkWidgets.yEntry entryInn;

		private global::Gtk.Entry entryNumber;

		private global::Gtk.EventBox eventboxDateIcon;

		private global::Gtk.Image IconDate;

		private global::Gtk.EventBox eventboxNumberIcon;

		private global::Gtk.Image IconNumber;

		private global::Gamma.GtkWidgets.yImage IconInn;

		private global::Gtk.Label label1;

		private global::Gtk.Label label10;

		private global::Gtk.Label label11;

		private global::Gtk.Label label9;

		private global::Gamma.GtkWidgets.yLabel labelInn;

		private global::Gamma.GtkWidgets.yButton ybuttonApplyToAllScans;

		private global::Gtk.CheckButton checkDiagnostic;

		private global::Gtk.Button buttonLog;

		private global::Gtk.ProgressBar progresswork;

		protected virtual void Build()
		{
			global::Stetic.Gui.Initialize(this);
			// Widget earchive.InputDocs
			this.UIManager = new global::Gtk.UIManager();
			global::Gtk.ActionGroup w1 = new global::Gtk.ActionGroup("Default");
			this.openAction = new global::Gtk.Action("openAction", global::Mono.Unix.Catalog.GetString("Из файла"), null, "gtk-open");
			this.openAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Из файла");
			w1.Add(this.openAction, null);
			this.ScanAction = new global::Gtk.Action("ScanAction", global::Mono.Unix.Catalog.GetString("Сканировать"), global::Mono.Unix.Catalog.GetString("Добавить изображения со сканера"), "scanner");
			this.ScanAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Сканировать");
			w1.Add(this.ScanAction, null);
			this.clearAction = new global::Gtk.Action("clearAction", null, global::Mono.Unix.Catalog.GetString("Очистить список изображений"), "gtk-clear");
			this.clearAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Очистить");
			w1.Add(this.clearAction, null);
			this.saveAction = new global::Gtk.Action("saveAction", null, global::Mono.Unix.Catalog.GetString("Сохранить в базу"), "gtk-save");
			this.saveAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Сохранить");
			w1.Add(this.saveAction, null);
			this.zoom100Action = new global::Gtk.Action("zoom100Action", global::Mono.Unix.Catalog.GetString("1:1"), null, "gtk-zoom-100");
			this.zoom100Action.ShortLabel = global::Mono.Unix.Catalog.GetString("1:1");
			w1.Add(this.zoom100Action, null);
			this.zoomFitAction = new global::Gtk.Action("zoomFitAction", global::Mono.Unix.Catalog.GetString("Вписать"), null, "gtk-zoom-fit");
			this.zoomFitAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Вписать");
			w1.Add(this.zoomFitAction, null);
			this.zoomInAction = new global::Gtk.Action("zoomInAction", null, null, "gtk-zoom-in");
			this.zoomInAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Увеличить");
			w1.Add(this.zoomInAction, null);
			this.zoomOutAction = new global::Gtk.Action("zoomOutAction", null, null, "gtk-zoom-out");
			this.zoomOutAction.ShortLabel = global::Mono.Unix.Catalog.GetString("Уменьшить");
			w1.Add(this.zoomOutAction, null);
			this.Rotate90Action = new global::Gtk.Action("Rotate90Action", global::Mono.Unix.Catalog.GetString("90"), null, "rotate-90");
			this.Rotate90Action.ShortLabel = global::Mono.Unix.Catalog.GetString("90°");
			w1.Add(this.Rotate90Action, null);
			this.Rotate180Action = new global::Gtk.Action("Rotate180Action", global::Mono.Unix.Catalog.GetString("180"), null, "rotate-180");
			this.Rotate180Action.ShortLabel = global::Mono.Unix.Catalog.GetString("180°");
			w1.Add(this.Rotate180Action, null);
			this.Rotate270Action = new global::Gtk.Action("Rotate270Action", global::Mono.Unix.Catalog.GetString("270"), null, "rotate-270");
			this.Rotate270Action.ShortLabel = global::Mono.Unix.Catalog.GetString("270°");
			w1.Add(this.Rotate270Action, null);
			this.ActionRecognize = new global::Gtk.Action("ActionRecognize", global::Mono.Unix.Catalog.GetString("Распознать"), null, "recognize");
			this.ActionRecognize.ShortLabel = global::Mono.Unix.Catalog.GetString("Распознать");
			w1.Add(this.ActionRecognize, null);
			this.UIManager.InsertActionGroup(w1, 0);
			this.AddAccelGroup(this.UIManager.AccelGroup);
			this.Name = "earchive.InputDocs";
			this.Title = global::Mono.Unix.Catalog.GetString("Ввод документов");
			this.Icon = global::Gdk.Pixbuf.LoadFromResource("earchive.icons.scanner.png");
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Container child earchive.InputDocs.Gtk.Container+ContainerChild
			this.vbox2 = new global::Gtk.VBox();
			this.vbox2.Name = "vbox2";
			this.vbox2.Spacing = 6;
			// Container child vbox2.Gtk.Box+BoxChild
			this.UIManager.AddUiFromString(@"<ui><toolbar name='toolbar1'><toolitem name='clearAction' action='clearAction'/><toolitem name='openAction' action='openAction'/><toolitem name='ScanAction' action='ScanAction'/><toolitem name='ActionRecognize' action='ActionRecognize'/><toolitem name='saveAction' action='saveAction'/><separator/><toolitem name='zoom100Action' action='zoom100Action'/><toolitem name='zoomFitAction' action='zoomFitAction'/><toolitem name='zoomInAction' action='zoomInAction'/><toolitem name='zoomOutAction' action='zoomOutAction'/><separator/><toolitem name='Rotate90Action' action='Rotate90Action'/><toolitem name='Rotate180Action' action='Rotate180Action'/><toolitem name='Rotate270Action' action='Rotate270Action'/></toolbar></ui>");
			this.toolbar1 = ((global::Gtk.Toolbar)(this.UIManager.GetWidget("/toolbar1")));
			this.toolbar1.Name = "toolbar1";
			this.toolbar1.ShowArrow = false;
			this.toolbar1.ToolbarStyle = ((global::Gtk.ToolbarStyle)(2));
			this.toolbar1.IconSize = ((global::Gtk.IconSize)(6));
			this.vbox2.Add(this.toolbar1);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.toolbar1]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child vbox2.Gtk.Box+BoxChild
			this.hbox2 = new global::Gtk.HBox();
			this.hbox2.Name = "hbox2";
			this.hbox2.Spacing = 6;
			// Container child hbox2.Gtk.Box+BoxChild
			this.hpaned1 = new global::Gtk.HPaned();
			this.hpaned1.CanFocus = true;
			this.hpaned1.Name = "hpaned1";
			this.hpaned1.Position = 206;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.treeviewImages = new global::Gtk.TreeView();
			this.treeviewImages.CanFocus = true;
			this.treeviewImages.Name = "treeviewImages";
			this.treeviewImages.HeadersVisible = false;
			this.treeviewImages.Reorderable = true;
			this.GtkScrolledWindow.Add(this.treeviewImages);
			this.hpaned1.Add(this.GtkScrolledWindow);
			global::Gtk.Paned.PanedChild w4 = ((global::Gtk.Paned.PanedChild)(this.hpaned1[this.GtkScrolledWindow]));
			w4.Resize = false;
			// Container child hpaned1.Gtk.Paned+PanedChild
			this.scrolledImage = new global::Gtk.ScrolledWindow();
			this.scrolledImage.Name = "scrolledImage";
			this.scrolledImage.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child scrolledImage.Gtk.Container+ContainerChild
			global::Gtk.Viewport w5 = new global::Gtk.Viewport();
			w5.ShadowType = ((global::Gtk.ShadowType)(0));
			// Container child GtkViewport.Gtk.Container+ContainerChild
			this.imageDoc = new global::Gtk.Image();
			this.imageDoc.Name = "imageDoc";
			w5.Add(this.imageDoc);
			this.scrolledImage.Add(w5);
			this.hpaned1.Add(this.scrolledImage);
			this.hbox2.Add(this.hpaned1);
			global::Gtk.Box.BoxChild w9 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.hpaned1]));
			w9.Position = 0;
			// Container child hbox2.Gtk.Box+BoxChild
			this.vbox3 = new global::Gtk.VBox();
			this.vbox3.Name = "vbox3";
			this.vbox3.Spacing = 6;
			// Container child vbox3.Gtk.Box+BoxChild
			this.table1 = new global::Gtk.Table(((uint)(1)), ((uint)(2)), false);
			this.table1.Name = "table1";
			this.table1.RowSpacing = ((uint)(6));
			this.table1.ColumnSpacing = ((uint)(6));
			// Container child table1.Gtk.Table+TableChild
			this.comboScaner = new global::Gamma.Widgets.yListComboBox();
			this.comboScaner.Name = "comboScaner";
			this.comboScaner.AddIfNotExist = false;
			this.comboScaner.DefaultFirst = false;
			this.table1.Add(this.comboScaner);
			global::Gtk.Table.TableChild w10 = ((global::Gtk.Table.TableChild)(this.table1[this.comboScaner]));
			w10.LeftAttach = ((uint)(1));
			w10.RightAttach = ((uint)(2));
			w10.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child table1.Gtk.Table+TableChild
			this.label2 = new global::Gtk.Label();
			this.label2.Name = "label2";
			this.label2.LabelProp = global::Mono.Unix.Catalog.GetString("Сканер:");
			this.table1.Add(this.label2);
			global::Gtk.Table.TableChild w11 = ((global::Gtk.Table.TableChild)(this.table1[this.label2]));
			w11.XOptions = ((global::Gtk.AttachOptions)(4));
			w11.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox3.Add(this.table1);
			global::Gtk.Box.BoxChild w12 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.table1]));
			w12.Position = 0;
			w12.Expand = false;
			w12.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.tableFieldWidgets = new global::Gtk.Table(((uint)(7)), ((uint)(3)), false);
			this.tableFieldWidgets.Name = "tableFieldWidgets";
			this.tableFieldWidgets.RowSpacing = ((uint)(6));
			this.tableFieldWidgets.ColumnSpacing = ((uint)(6));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.comboType = global::Gtk.ComboBox.NewText();
			this.comboType.Name = "comboType";
			this.tableFieldWidgets.Add(this.comboType);
			global::Gtk.Table.TableChild w13 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.comboType]));
			w13.TopAttach = ((uint)(1));
			w13.BottomAttach = ((uint)(2));
			w13.LeftAttach = ((uint)(1));
			w13.RightAttach = ((uint)(2));
			w13.XOptions = ((global::Gtk.AttachOptions)(4));
			w13.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.dateDoc = new global::QSWidgetLib.DatePicker();
			this.dateDoc.Events = ((global::Gdk.EventMask)(256));
			this.dateDoc.Name = "dateDoc";
			this.dateDoc.WithTime = false;
			this.dateDoc.Date = new global::System.DateTime(0);
			this.dateDoc.IsEditable = true;
			this.dateDoc.AutoSeparation = true;
			this.tableFieldWidgets.Add(this.dateDoc);
			global::Gtk.Table.TableChild w14 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.dateDoc]));
			w14.TopAttach = ((uint)(3));
			w14.BottomAttach = ((uint)(4));
			w14.LeftAttach = ((uint)(1));
			w14.RightAttach = ((uint)(2));
			w14.XOptions = ((global::Gtk.AttachOptions)(4));
			w14.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.entryInn = new global::Gamma.GtkWidgets.yEntry();
			this.entryInn.CanFocus = true;
			this.entryInn.Name = "entryInn";
			this.entryInn.IsEditable = true;
			this.entryInn.InvisibleChar = '•';
			this.tableFieldWidgets.Add(this.entryInn);
			global::Gtk.Table.TableChild w15 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.entryInn]));
			w15.TopAttach = ((uint)(4));
			w15.BottomAttach = ((uint)(5));
			w15.LeftAttach = ((uint)(1));
			w15.RightAttach = ((uint)(2));
			w15.XOptions = ((global::Gtk.AttachOptions)(4));
			w15.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.entryNumber = new global::Gtk.Entry();
			this.entryNumber.CanFocus = true;
			this.entryNumber.Name = "entryNumber";
			this.entryNumber.IsEditable = true;
			this.entryNumber.InvisibleChar = '●';
			this.tableFieldWidgets.Add(this.entryNumber);
			global::Gtk.Table.TableChild w16 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.entryNumber]));
			w16.TopAttach = ((uint)(2));
			w16.BottomAttach = ((uint)(3));
			w16.LeftAttach = ((uint)(1));
			w16.RightAttach = ((uint)(2));
			w16.XOptions = ((global::Gtk.AttachOptions)(4));
			w16.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.eventboxDateIcon = new global::Gtk.EventBox();
			this.eventboxDateIcon.Name = "eventboxDateIcon";
			// Container child eventboxDateIcon.Gtk.Container+ContainerChild
			this.IconDate = new global::Gtk.Image();
			this.IconDate.Name = "IconDate";
			this.IconDate.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-dialog-info", global::Gtk.IconSize.Button);
			this.eventboxDateIcon.Add(this.IconDate);
			this.tableFieldWidgets.Add(this.eventboxDateIcon);
			global::Gtk.Table.TableChild w18 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.eventboxDateIcon]));
			w18.TopAttach = ((uint)(3));
			w18.BottomAttach = ((uint)(4));
			w18.LeftAttach = ((uint)(2));
			w18.RightAttach = ((uint)(3));
			w18.XOptions = ((global::Gtk.AttachOptions)(4));
			w18.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.eventboxNumberIcon = new global::Gtk.EventBox();
			this.eventboxNumberIcon.Name = "eventboxNumberIcon";
			// Container child eventboxNumberIcon.Gtk.Container+ContainerChild
			this.IconNumber = new global::Gtk.Image();
			this.IconNumber.Name = "IconNumber";
			this.IconNumber.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-dialog-info", global::Gtk.IconSize.Button);
			this.eventboxNumberIcon.Add(this.IconNumber);
			this.tableFieldWidgets.Add(this.eventboxNumberIcon);
			global::Gtk.Table.TableChild w20 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.eventboxNumberIcon]));
			w20.TopAttach = ((uint)(2));
			w20.BottomAttach = ((uint)(3));
			w20.LeftAttach = ((uint)(2));
			w20.RightAttach = ((uint)(3));
			w20.XOptions = ((global::Gtk.AttachOptions)(4));
			w20.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.IconInn = new global::Gamma.GtkWidgets.yImage();
			this.IconInn.Name = "IconInn";
			this.IconInn.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-dialog-info", global::Gtk.IconSize.Menu);
			this.tableFieldWidgets.Add(this.IconInn);
			global::Gtk.Table.TableChild w21 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.IconInn]));
			w21.TopAttach = ((uint)(4));
			w21.BottomAttach = ((uint)(5));
			w21.LeftAttach = ((uint)(2));
			w21.RightAttach = ((uint)(3));
			w21.XOptions = ((global::Gtk.AttachOptions)(4));
			w21.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.label1 = new global::Gtk.Label();
			this.label1.Name = "label1";
			this.label1.LabelProp = global::Mono.Unix.Catalog.GetString("<b>Поля текущего документа</b>");
			this.label1.UseMarkup = true;
			this.label1.Justify = ((global::Gtk.Justification)(2));
			this.tableFieldWidgets.Add(this.label1);
			global::Gtk.Table.TableChild w22 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.label1]));
			w22.RightAttach = ((uint)(3));
			w22.XOptions = ((global::Gtk.AttachOptions)(4));
			w22.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.label10 = new global::Gtk.Label();
			this.label10.Name = "label10";
			this.label10.Xalign = 1F;
			this.label10.LabelProp = global::Mono.Unix.Catalog.GetString("Дата<span foreground=\"red\">*</span>:");
			this.label10.UseMarkup = true;
			this.tableFieldWidgets.Add(this.label10);
			global::Gtk.Table.TableChild w23 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.label10]));
			w23.TopAttach = ((uint)(3));
			w23.BottomAttach = ((uint)(4));
			w23.XOptions = ((global::Gtk.AttachOptions)(4));
			w23.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.label11 = new global::Gtk.Label();
			this.label11.Name = "label11";
			this.label11.Xalign = 1F;
			this.label11.LabelProp = global::Mono.Unix.Catalog.GetString("Тип документа<span foreground=\"red\">*</span>:");
			this.label11.UseMarkup = true;
			this.tableFieldWidgets.Add(this.label11);
			global::Gtk.Table.TableChild w24 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.label11]));
			w24.TopAttach = ((uint)(1));
			w24.BottomAttach = ((uint)(2));
			w24.XOptions = ((global::Gtk.AttachOptions)(4));
			w24.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.label9 = new global::Gtk.Label();
			this.label9.Name = "label9";
			this.label9.Xalign = 1F;
			this.label9.LabelProp = global::Mono.Unix.Catalog.GetString("Номер<span foreground=\"red\">*</span>:");
			this.label9.UseMarkup = true;
			this.tableFieldWidgets.Add(this.label9);
			global::Gtk.Table.TableChild w25 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.label9]));
			w25.TopAttach = ((uint)(2));
			w25.BottomAttach = ((uint)(3));
			w25.XOptions = ((global::Gtk.AttachOptions)(4));
			w25.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.labelInn = new global::Gamma.GtkWidgets.yLabel();
			this.labelInn.Name = "labelInn";
			this.labelInn.Xalign = 1F;
			this.labelInn.LabelProp = global::Mono.Unix.Catalog.GetString("ИНН<span foreground=\"red\">*</span>:");
			this.labelInn.UseMarkup = true;
			this.tableFieldWidgets.Add(this.labelInn);
			global::Gtk.Table.TableChild w26 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.labelInn]));
			w26.TopAttach = ((uint)(4));
			w26.BottomAttach = ((uint)(5));
			w26.XOptions = ((global::Gtk.AttachOptions)(4));
			w26.YOptions = ((global::Gtk.AttachOptions)(4));
			// Container child tableFieldWidgets.Gtk.Table+TableChild
			this.ybuttonApplyToAllScans = new global::Gamma.GtkWidgets.yButton();
			this.ybuttonApplyToAllScans.CanFocus = true;
			this.ybuttonApplyToAllScans.Name = "ybuttonApplyToAllScans";
			this.ybuttonApplyToAllScans.UseUnderline = true;
			this.ybuttonApplyToAllScans.Label = global::Mono.Unix.Catalog.GetString("Применить ко всем\n      изображениям");
			this.tableFieldWidgets.Add(this.ybuttonApplyToAllScans);
			global::Gtk.Table.TableChild w27 = ((global::Gtk.Table.TableChild)(this.tableFieldWidgets[this.ybuttonApplyToAllScans]));
			w27.TopAttach = ((uint)(5));
			w27.BottomAttach = ((uint)(6));
			w27.LeftAttach = ((uint)(1));
			w27.RightAttach = ((uint)(2));
			w27.XOptions = ((global::Gtk.AttachOptions)(4));
			w27.YOptions = ((global::Gtk.AttachOptions)(4));
			this.vbox3.Add(this.tableFieldWidgets);
			global::Gtk.Box.BoxChild w28 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.tableFieldWidgets]));
			w28.Position = 1;
			// Container child vbox3.Gtk.Box+BoxChild
			this.checkDiagnostic = new global::Gtk.CheckButton();
			this.checkDiagnostic.CanFocus = true;
			this.checkDiagnostic.Name = "checkDiagnostic";
			this.checkDiagnostic.Label = global::Mono.Unix.Catalog.GetString("Диагностика распознования");
			this.checkDiagnostic.DrawIndicator = true;
			this.checkDiagnostic.UseUnderline = true;
			this.vbox3.Add(this.checkDiagnostic);
			global::Gtk.Box.BoxChild w29 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.checkDiagnostic]));
			w29.Position = 2;
			w29.Expand = false;
			w29.Fill = false;
			// Container child vbox3.Gtk.Box+BoxChild
			this.buttonLog = new global::Gtk.Button();
			this.buttonLog.CanFocus = true;
			this.buttonLog.Name = "buttonLog";
			this.buttonLog.UseUnderline = true;
			this.buttonLog.Label = global::Mono.Unix.Catalog.GetString("Показать журнал распознования.");
			global::Gtk.Image w30 = new global::Gtk.Image();
			w30.Pixbuf = global::Stetic.IconLoader.LoadIcon(this, "gtk-execute", global::Gtk.IconSize.Button);
			this.buttonLog.Image = w30;
			this.vbox3.Add(this.buttonLog);
			global::Gtk.Box.BoxChild w31 = ((global::Gtk.Box.BoxChild)(this.vbox3[this.buttonLog]));
			w31.Position = 3;
			w31.Expand = false;
			w31.Fill = false;
			this.hbox2.Add(this.vbox3);
			global::Gtk.Box.BoxChild w32 = ((global::Gtk.Box.BoxChild)(this.hbox2[this.vbox3]));
			w32.Position = 1;
			w32.Expand = false;
			w32.Fill = false;
			this.vbox2.Add(this.hbox2);
			global::Gtk.Box.BoxChild w33 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.hbox2]));
			w33.Position = 1;
			// Container child vbox2.Gtk.Box+BoxChild
			this.progresswork = new global::Gtk.ProgressBar();
			this.progresswork.Name = "progresswork";
			this.vbox2.Add(this.progresswork);
			global::Gtk.Box.BoxChild w34 = ((global::Gtk.Box.BoxChild)(this.vbox2[this.progresswork]));
			w34.Position = 2;
			w34.Expand = false;
			w34.Fill = false;
			this.Add(this.vbox2);
			if ((this.Child != null))
			{
				this.Child.ShowAll();
			}
			this.DefaultWidth = 1053;
			this.DefaultHeight = 478;
			this.Show();
			this.openAction.Activated += new global::System.EventHandler(this.OnOpenActionActivated);
			this.ScanAction.Activated += new global::System.EventHandler(this.OnScanActionActivated);
			this.clearAction.Activated += new global::System.EventHandler(this.OnClearActionActivated);
			this.saveAction.Activated += new global::System.EventHandler(this.OnSaveActionActivated);
			this.zoom100Action.Activated += new global::System.EventHandler(this.OnZoom100ActionActivated);
			this.zoomFitAction.Activated += new global::System.EventHandler(this.OnZoomFitActionActivated);
			this.zoomInAction.Activated += new global::System.EventHandler(this.OnZoomInActionActivated);
			this.zoomOutAction.Activated += new global::System.EventHandler(this.OnZoomOutActionActivated);
			this.Rotate90Action.Activated += new global::System.EventHandler(this.OnRotate90ActionActivated);
			this.Rotate180Action.Activated += new global::System.EventHandler(this.OnRotate180ActionActivated);
			this.Rotate270Action.Activated += new global::System.EventHandler(this.OnRotate270ActionActivated);
			this.ActionRecognize.Activated += new global::System.EventHandler(this.OnActionRecognizeActivated);
			this.treeviewImages.ButtonReleaseEvent += new global::Gtk.ButtonReleaseEventHandler(this.OnTreeviewImagesButtonReleaseEvent);
			this.imageDoc.DragMotion += new global::Gtk.DragMotionHandler(this.OnImageDocDragMotion);
			this.comboScaner.Changed += new global::System.EventHandler(this.OnComboScanerChanged);
			this.eventboxNumberIcon.ButtonPressEvent += new global::Gtk.ButtonPressEventHandler(this.OnEventboxNumberIconButtonPressEvent);
			this.eventboxDateIcon.ButtonPressEvent += new global::Gtk.ButtonPressEventHandler(this.OnEventboxDateIconButtonPressEvent);
			this.entryNumber.Changed += new global::System.EventHandler(this.OnEntryNumberChanged);
			this.dateDoc.DateChanged += new global::System.EventHandler(this.OnDateDocDateChanged);
			this.comboType.Changed += new global::System.EventHandler(this.OnComboTypeChanged);
			this.buttonLog.Clicked += new global::System.EventHandler(this.OnButtonLogClicked);
		}
	}
}

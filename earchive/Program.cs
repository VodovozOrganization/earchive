using System;
using System.Collections.Generic;
using Gtk;
using QSProjectsLib;

namespace earchive
{
	class MainClass
	{
		public static MainWindow MainWin;

		public static void Main (string[] args)
		{
			Application.Init ();
			QSMain.SubscribeToUnhadledExceptions ();
			QSMain.GuiThread = System.Threading.Thread.CurrentThread;
			CreateProjectParam();

			// Создаем окно входа
			Login LoginDialog = new QSProjectsLib.Login ();
			LoginDialog.Logo = Gdk.Pixbuf.LoadFromResource ("earchive.icons.logo.png");
			LoginDialog.SetDefaultNames ("earchive");
			LoginDialog.DefaultLogin = "admin";
			LoginDialog.DefaultServer = "localhost";
			LoginDialog.UpdateFromGConf ();

			ResponseType LoginResult;
			LoginResult = (ResponseType) LoginDialog.Run();
			if (LoginResult == ResponseType.DeleteEvent || LoginResult == ResponseType.Cancel)
				return;

			LoginDialog.Destroy ();

			//Запускаем программу
			MainWin = new MainWindow ();
			if(QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();
		}

		static void CreateProjectParam()
		{
			QSMain.ProjectPermission = new Dictionary<string, UserPermission>();
			QSMain.ProjectPermission.Add ("can_edit", new UserPermission("can_edit", "Изменение документов",
			                                                      "Пользователь может изменять и добавлять документы"));
			QSMain.ProjectPermission.Add ("edit_db", new UserPermission("edit_db", "Изменение БД",
			                                                             "Пользователь может изменять структуру базы данных"));
		}

		public static void WaitRedraw()
		{
			while (Application.EventsPending ())
			{
				Gtk.Main.Iteration ();
			}
		}
	}
}

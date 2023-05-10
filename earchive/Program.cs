using System;
using System.Collections.Generic;
using Gtk;
using QS.BaseParameters;
using QS.Dialog;
using QS.Project.Services;
using QS.Project.Services.GtkUI;
using QS.Project.Versioning;
using QSProjectsLib;

namespace earchive
{
	class MainClass
	{
		public static MainWindow MainWin;

		[STAThread]
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

			IApplicationInfo applicationInfo = new ApplicationVersionInfo();
			var baseVersionChecker = new CheckBaseVersion(applicationInfo, new ParametersService(QSMain.ConnectionDB));
			QS.Project.Repositories.UserRepository.GetCurrentUserId = () => QSMain.User.Id;
			if(baseVersionChecker.Check())
			{
				ServicesConfig.CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, baseVersionChecker.TextMessage, "Несовпадение версии");
				return;
			}

			if(QSMain.User.Login == "root")
			{
				string Message = "Вы зашли в программу под администратором базы данных. У вас есть только возможность создавать других пользователей.";
				MessageDialog md = new MessageDialog(null, DialogFlags.DestroyWithParent,
													  MessageType.Info,
													  ButtonsType.Ok,
													  Message);
				md.Run();
				md.Destroy();
				OnUsersActionActivated(null, null);
				return;
			}

			//Запускаем программу
			MainWin = new MainWindow ();
			if(QSMain.User.Login == "root")
				return;
			MainWin.Show ();
			Application.Run ();
		}

		private static void OnUsersActionActivated(object sender, EventArgs e)
		{
			Users WinUser = new Users();
			WinUser.Show();
			WinUser.Run();
			WinUser.Destroy();
		}

		static void CreateProjectParam()
		{
			QSMain.ProjectPermission = new Dictionary<string, UserPermission>();
			QSMain.ProjectPermission.Add ("can_edit", new UserPermission("can_edit", "Изменение документов",
			                                                      "Пользователь может изменять и добавлять документы"));
			QSMain.ProjectPermission.Add ("edit_db", new UserPermission("edit_db", "Изменение БД",
			                                                             "Пользователь может изменять структуру базы данных"));

			//ServicesConfig.InteractiveService = new GtkInteractiveService();
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

using System;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает точку входа приложения
	/// </summary>
	public static class UniNotifierProgram
		{
		/// <summary>
		/// Главная точка входа для приложения
		/// </summary>
		[STAThread]
		public static void Main (string[] args)
			{
			// Инициализация
			Application.EnableVisualStyles ();
			Application.SetCompatibleTextRenderingDefault (false);

			// Язык интерфейса и контроль XPUN
			if (!RDLocale.IsXPUNClassAcceptable)
				return;

			// Проверка запуска единственной копии
			if (!RDGenerics.IsAppInstanceUnique (true))
				return;

			// Контроль прав
			if (!RDGenerics.AppHasAccessRights (true, false))
				return;

			// Отображение справки и запроса на принятие Политики
			if (!RDGenerics.AcceptEULA ())
				return;
			RDGenerics.ShowAbout (true);

			/*Bitmap b = GMJPicture.CreateGMJPicture ("- Telegram > Grammar must joy • №4142–1957 -",
				"— Хочу футболку с QR-кодом какого-нибудь жуткого вредоносного вируса. Чтобы при попытке сфотографировать меня в публичном месте телефон автоматически сканировал код и превращался в бесполезный кирпич.\n\n"+
				"— Медуза Горгона, версия для зумеров");
			if (b == null)
				{
				RDGenerics.MessageBox (RDMessageTypes.Warning_Center, "Текст слишком длинный", 2000);
				return;
				}

			b.Save ("C:\\Test.png", ImageFormat.Png);
			RDGenerics.RunURL ("C:\\Test.png");
			return;*/

			// Запуск
			Application.Run (new GrammarMustJoyForm ((args.Length > 0) && (args[0] == "-h")));
			}
		}
	}

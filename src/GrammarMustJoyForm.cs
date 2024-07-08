using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает главную форму приложения
	/// </summary>
	public partial class GrammarMustJoyForm: Form
		{
		// Переменные
		private NotifyIcon ni = new NotifyIcon ();
		private bool allowExit = false;
		private bool hideWindow;

		/// <summary>
		/// Конструктор. Настраивает главную форму приложения
		/// </summary>
		public GrammarMustJoyForm (bool HideWindow)
			{
			// Инициализация
			InitializeComponent ();

			this.Text = ProgramDescription.AssemblyVisibleName;
			this.CancelButton = BClose;
			MainText.Font = new Font ("Calibri", 13);
			if (!RDGenerics.AppHasAccessRights (false, false))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);
			hideWindow = HideWindow;

			// Принудительные параметры
			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			if (GMJ.EnablePostSubscription)
				GMJ.EnablePostSubscription = false;

			/*ReloadNotificationsList ();
			if TGT
			GetGMJ.Visible = false;
			else
			GetGMJ.Visible = RDLocale.IsCurrentLanguageRuRu;
			endif*/

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);
			ReadMode.Checked = RDGenerics.GetSettings (readPar, false);
			/*callWindowOnUrgents = RDGenerics.GetSettings (callWindowOnUrgentsPar, false);
			*/
			try
				{
				FontSizeField.Value = RDGenerics.GetSettings (fontSizePar, 130) / 10.0m;
				}
			catch { }

			// Настройка иконки в трее
			ni.Icon = Properties.GrammarMustJoy.GMJNotifier16;
			ni.Text = ProgramDescription.AssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenu = new ContextMenu ();

			/*ni.ContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetText ("MainMenuOption02"), ShowSettings));
			ni.ContextMenu.MenuItems[0].Enabled = RDGenerics.AppHasAccessRights (false, true);*/

			ni.ContextMenu.MenuItems.Add (new MenuItem (
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout), AboutService));
			ni.ContextMenu.MenuItems.Add (new MenuItem (
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit), CloseService));

			ni.MouseDown += ShowHideFullText;
			ni.ContextMenu.MenuItems[1].DefaultItem = true;
			}

		private void GrammarMustJoyForm_Shown (object sender, EventArgs e)
			{
			// Скрытие окна настроек
			GrammarMustJoyForm_Resize (null, null);
			if (hideWindow)
				this.Hide ();

			/*// Запуск
			MainTimer.Interval = (int)ProgramDescription.MasterFrameLength * 4;
			MainTimer.Enabled = true;*/
			}

		// Завершение работы службы
		private void CloseService (object sender, EventArgs e)
			{
			allowExit = true;
			this.Close ();
			}

		private void GrammarMustJoyForm_FormClosing (object sender, FormClosingEventArgs e)
			{
			// Остановка службы
			if (allowExit)
				{
				// Остановка
				ni.Visible = false;
				/*MainTimer.Enabled = false;

				// Освобождение ресурсов
				ns.Dispose ();*/
				}

			// Скрытие окна просмотра
			else
				{
				this.Hide ();
				e.Cancel = true;
				}
			}

		// О приложении
		private void AboutService (object sender, EventArgs e)
			{
			RDGenerics.ShowAbout (false);
			}

		// Отображение / скрытие полного списка оповещений
		private void ShowHideFullText (object sender, MouseEventArgs e)
			{
			// Работа только с левой кнопкой мыши
			if (e.Button != MouseButtons.Left)
				return;

			/*// Отмена состояния сообщений
			ns.HasUrgentNotifications = false;*/

			// Обработка состояния
			if (this.Visible)
				{
				this.Close ();
				}
			else
				{
				this.Show ();
				this.TopMost = true;
				this.TopMost = false;
				MainText.ScrollToCaret ();
				}
			}

		// Закрытие окна просмотра
		private void BClose_Click (object sender, EventArgs e)
			{
			/*// Отмена состояния сообщений
			ns.HasUrgentNotifications = false;*/

			this.Close ();
			}

		// Переход в режим чтения и обратно
		private void ReadMode_CheckedChanged (object sender, EventArgs e)
			{
			// Изменение состояния
			if (ReadMode.Checked)
				{
				MainText.ForeColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.LightGrey);
				MainText.BackColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.DefaultText);
				}
			else
				{
				MainText.ForeColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.DefaultText);
				MainText.BackColor = RDGenerics.GetInterfaceColor (RDInterfaceColors.LightGrey);
				}

			// Запоминание
			RDGenerics.SetSettings (readPar, ReadMode.Checked);
			}
		private const string readPar = "Read";

		// Изменение размера формы
		private void GrammarMustJoyForm_Resize (object sender, EventArgs e)
			{
			MainText.Width = this.Width - 38;
			MainText.Height = this.Height - 87;

			ButtonsPanel.Top = MainText.Top + MainText.Height - 1;
			}

		// Сохранение размера формы
		private void GrammarMustJoyForm_ResizeEnd (object sender, EventArgs e)
			{
			RDGenerics.SaveWindowDimensions (this);
			}

		// Запрос сообщения от GMJ
		private void GetGMJExecutor (object sender, DoWorkEventArgs e)
			{
			e.Result = GMJ.GetRandomGMJ ();
			}

		private void GetGMJ_Click (object sender, EventArgs e)
			{
			// Запрос записи
			RDGenerics.RunWork (GetGMJExecutor, null, "Запрос случайной записи...",
				RDRunWorkFlags.CaptionInTheMiddle);
			string s = RDGenerics.WorkResultAsString;
			string item;

			if (s != "")
				item = s;
			else
				item = "GMJ не отвечает на запрос. Проверьте интернет-соединение";

			// Отображение
			// Добавление в главное окно
			if ((MainText.Text.Length + item.Length > ProgramDescription.MasterLogMaxLength) &&
				(MainText.Text.Length > item.Length))   // Бывает и так
				MainText.Text = MainText.Text.Substring (item.Length, MainText.Text.Length - item.Length);
			if (MainText.Text.Length > 0)
				MainText.AppendText (RDLocale.RNRN + RDLocale.RN);

			// Добавление и форматирование
			MainText.AppendText (item.Replace (NotificationsSet.MainLogItemSplitter.ToString (), RDLocale.RN));
			MainText.AppendText (RDLocale.RN);
			}

		// Изменение размера шрифта
		private void FontSizeField_ValueChanged (object sender, EventArgs e)
			{
			MainText.Font = new Font (MainText.Font.FontFamily, (float)FontSizeField.Value);
			RDGenerics.SetSettings (fontSizePar, (uint)(FontSizeField.Value * 10.0m));
			}
		private const string fontSizePar = "FontSize";
		}
	}

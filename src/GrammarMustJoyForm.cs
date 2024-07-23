using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
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

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);
			ReadMode.Checked = RDGenerics.GetSettings (readPar, false);
			try
				{
				FontSizeField.Value = RDGenerics.GetSettings (fontSizePar, 130) / 10.0m;
				GroupCountField.Value = RDGenerics.GetSettings (groupCountPar, 1);
				}
			catch { }

			// Настройка иконки в трее
			ni.Icon = Properties.GrammarMustJoy.GMJNotifier16;
			ni.Text = ProgramDescription.AssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenu = new ContextMenu ();
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
			MainText.Height = this.Height - ButtonsPanel.Height - 53;

			ButtonsPanel.Top = MainText.Top + MainText.Height + 1;
			}

		// Сохранение размера формы
		private void GrammarMustJoyForm_ResizeEnd (object sender, EventArgs e)
			{
			RDGenerics.SaveWindowDimensions (this);
			}

		// Запрос сообщения от GMJ
		private void GetGMJExecutor (object sender, DoWorkEventArgs e)
			{
			uint group = RDGenerics.GetSettings (groupCountPar, 1);
			BackgroundWorker bw = (BackgroundWorker)sender;
			string res = "";
			string sp = groupSplitter[0].ToString ();
			string limit = " из " + group.ToString ();

			for (int i = 0; i < group; i++)
				{
				// Антиспам
				if (i > 0)
					Thread.Sleep (1000);

				res += (GMJ.GetRandomGMJ () + sp);
				bw.ReportProgress ((int)(HardWorkExecutor.ProgressBarSize * (i + 1) / group),
					"Запрошено " + (i + 1).ToString () + limit);
				}

			e.Result = res;
			}

		private void GetGMJ_Click (object sender, EventArgs e)
			{
			// Запрос записи
			RDGenerics.RunWork (GetGMJExecutor, null, "Запрос случайной записи...",
				RDRunWorkFlags.CaptionInTheMiddle);
			string items = "";

			string[] values = RDGenerics.WorkResultAsString.Split (groupSplitter,
				StringSplitOptions.RemoveEmptyEntries);
			bool empty = string.IsNullOrWhiteSpace (MainText.Text);

			if (values.Length > 0)
				{
				for (int i = 0; i < values.Length; i++)
					items += ((empty ? "" : RDLocale.RNRN + RDLocale.RNRN) +
						values[i].Replace (NotificationsSet.MainLogItemSplitter.ToString (),
						RDLocale.RN));
				}
			else
				{
				items = (empty ? "" : RDLocale.RNRN) +
					"GMJ не отвечает на запрос. Проверьте интернет-соединение";
				}

			// Добавление в главное окно
			if ((MainText.Text.Length + items.Length > ProgramDescription.MasterLogMaxLength) &&
				(MainText.Text.Length > items.Length))   // Бывает и так
				MainText.Text = MainText.Text.Substring (items.Length, MainText.Text.Length - items.Length);
			/*if (MainText.Text.Length > 0)
				MainText.AppendText (RDLocale.RNRN + RDLocale.RN);

			// Добавление и форматирование
			MainText.AppendText (item.Replace (NotificationsSet.MainLogItemSplitter.ToString (), RDLocale.RN));
			MainText.AppendText (RDLocale.RN);*/
			MainText.AppendText (items);
			}

		// Изменение размера шрифта
		private void FontSizeField_ValueChanged (object sender, EventArgs e)
			{
			MainText.Font = new Font (MainText.Font.FontFamily, (float)FontSizeField.Value);
			RDGenerics.SetSettings (fontSizePar, (uint)(FontSizeField.Value * 10.0m));
			}
		private const string fontSizePar = "FontSize";

		// Изменение длины группы
		private void GroupCountField_ValueChanged (object sender, EventArgs e)
			{
			RDGenerics.SetSettings (groupCountPar, (uint)GroupCountField.Value);
			}
		private const string groupCountPar = "GroupCount";
		private char[] groupSplitter = new char[] { '\x1' };

		// Вызов справки
		private void BHelp_Click (object sender, EventArgs e)
			{
			RDGenerics.ShowAbout (false);
			}

		// Предложение записей
		private void BAdd_Click (object sender, EventArgs e)
			{
			if (RDGenerics.MessageBox (RDMessageTypes.Question_Center, GMJ.SuggestionMessage,
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) != RDMessageButtons.ButtonOne)
				return;

			AboutForm.AskDeveloper ();
			}
		}
	}

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

		private char[] groupSplitter = new char[] { '\x1' };
		/*private bool csReverse = false;     // Отмена повторной обработки изменения режима цензурирования*/

		/*private ContextMenu bColorContextMenu;
		private ContextMenu bHelpContextMenu;*/
		private ContextMenu textContextMenu;
		private int textContextSender;

		private const string fontFamily = "Calibri";
		private const int transculencyAmount = 15;

		/// <summary>
		/// Конструктор. Настраивает главную форму приложения
		/// </summary>
		public GrammarMustJoyForm (bool HideWindow)
			{
			// Инициализация
			InitializeComponent ();

			this.Text = ProgramDescription.AssemblyVisibleName;
			this.CancelButton = BClose;
			if (!RDGenerics.AppHasAccessRights (false, false))
				this.Text += RDLocale.GetDefaultText (RDLDefaultTexts.Message_LimitedFunctionality);
			hideWindow = HideWindow;

			// Принудительные параметры
			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			/*if (!GMJ.EnableCopySubscription)
				GMJ.EnableCopySubscription = true;*/

			// Получение настроек
			RDGenerics.LoadWindowDimensions (this);

			/*BColor_ItemClicked (null, null);    // Подгрузка настройки*/
			ApplyColorsAndFonts ();
			/*try
				{
				FontSizeField.Value = NotificationsSupport.LogFontSize / 10.0m;
				GroupCountField.Value = NotificationsSupport.GroupSize;
				}
			catch { }*/

			// Настройка иконки в трее
			ni.Icon = Properties.GrammarMustJoy.GMJNotifier16;
			ni.Text = ProgramDescription.AssemblyVisibleName;
			ni.Visible = true;

			ni.ContextMenu = new ContextMenu ();
			ni.ContextMenu.MenuItems.Add (new MenuItem (GMJ.GMJStatsMenuItem, BHelp_ItemClicked));
			ni.ContextMenu.MenuItems.Add (new MenuItem ("Настройки", BHelp_ItemClicked));
			ni.ContextMenu.MenuItems.Add (new MenuItem (
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout), BHelp_ItemClicked));
			ni.ContextMenu.MenuItems.Add (new MenuItem (
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Exit), BHelp_ItemClicked));

			ni.MouseDown += ShowHideFullText;
			/*ni.ContextMenu.MenuItems[1].DefaultItem = true;*/

			// Цензурирование
			if (RDGenerics.StartedFromMSStore)
				{
				if (!GMJ.EnableCensorship)
					GMJ.EnableCensorship = true;
				}

			/*// Цензурирование
			if (RDGenerics.StartedFromMSStore)
				{
				CensorshipFlag.Visible = false;
				if (!GMJ.EnableCensorship)
					GMJ.EnableCensorship = true;
				}
			else
				{
				csReverse = true;
				CensorshipFlag.Checked = GMJ.EnableCensorship;
				csReverse = false;
				CensorshipFlag_CheckedChanged (null, null);
				}*/

			// Контекстное меню журнала
			textContextMenu = new ContextMenu ();
			textContextMenu.MenuItems.Add (new MenuItem ("Копировать текст", TextContext_ItemClicked));
			textContextMenu.MenuItems.Add (new MenuItem ("Сохранить картинку", TextContext_ItemClicked));

			// Окно сохранения картинок
			SFDialog.Title = "Укажите расположение для сохраняемой картинки";
			SFDialog.Filter = "Portable network graphics (*.png)|*.png";
			}

		private void GrammarMustJoyForm_Shown (object sender, EventArgs e)
			{
			// Скрытие окна настроек
			GrammarMustJoyForm_Resize (null, null);
			if (hideWindow)
				this.Hide ();
			}

		/*// Завершение работы службы
		private void CloseService (object sender, EventArgs e)
			{
			allowExit = true;
			this.Close ();
			}*/

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

		/*// О приложении
		private void AboutService (object sender, EventArgs e)
			{
			RDGenerics.ShowAbout (false);
			}*/

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
				ScrollLog ();
				}
			}

		// Метод прокручивает журнал к последней записи
		private void ScrollLog ()
			{
			if (MainLayout.Controls.Count > 0)
				MainLayout.ScrollControlIntoView (MainLayout.Controls[MainLayout.Controls.Count - 1]);
			}

		// Закрытие окна просмотра
		private void BClose_Click (object sender, EventArgs e)
			{
			this.Close ();
			}

		/*// Выбор цвета журнала
		private void BColor_Clicked (object sender, EventArgs e)
			{
			// Создание вызывающего контекстного меню
			if (bColorContextMenu == null)
				{
				bColorContextMenu = new ContextMenu ();

				for (int i = 0; i < NotificationsSupport.LogColors.ColorNames.Length; i++)
					bColorContextMenu.MenuItems.Add (new MenuItem (NotificationsSupport.LogColors.ColorNames[i],
						BColor_ItemClicked));

				bColorContextMenu.MenuItems.Add (new MenuItem ("-"));
				bColorContextMenu.MenuItems.Add (new MenuItem ("Полупрозрачные элементы журнала",
					BColor_ItemClicked));
				bColorContextMenu.MenuItems[bColorContextMenu.MenuItems.Count - 1].Checked =
					NotificationsSupport.TranslucentLogItems;
				}

			// Вызов
			if (sender != null)
				bColorContextMenu.Show (BColor, Point.Empty);
			}

		private void BColor_ItemClicked (object sender, EventArgs e)*/

		// Загрузка параметров после настройки
		private void ApplyColorsAndFonts ()
			{
			// Извлечение индекса
			int idx;
			/*if (sender == null)*/
			idx = (int)NotificationsSupport.LogColor;
			/*else
				idx = bColorContextMenu.MenuItems.IndexOf ((MenuItem)sender);*/

			/*// Сохранение
			if (idx < NotificationsSupport.LogColors.ColorNames.Length)
				{
				NotificationsSupport.LogColor = (uint)idx;
				}
			else
				{
				MenuItem mi = bColorContextMenu.MenuItems[bColorContextMenu.MenuItems.Count - 1];
				mi.Checked = !mi.Checked;
				NotificationsSupport.TranslucentLogItems = mi.Checked;

				FontSizeField_ValueChanged (null, null);
				}*/

			Font fnt = new Font (fontFamily, NotificationsSupport.LogFontSize / 10.0f);
			MainLayout.BackColor = NotificationsSupport.LogColors.CurrentColor.BackColor;
			for (int i = 0; i < MainLayout.Controls.Count; i++)
				{
				Label l = (Label)MainLayout.Controls[i];
				l.Font = fnt;
				l.Margin = LogItemMargin;

				int amount = NotificationsSupport.TranslucentLogItems ? transculencyAmount : 0;
				if (NotificationsSupport.LogColors.CurrentColor.IsBright)
					l.BackColor = Color.FromArgb (amount, 0, 0, 0);
				else
					l.BackColor = Color.FromArgb (amount, 255, 255, 255);

				l.ForeColor = NotificationsSupport.LogColors.CurrentColor.MainTextColor;
				}
			}

		// Изменение размера формы
		private void GrammarMustJoyForm_Resize (object sender, EventArgs e)
			{
			MainLayout.Width = this.Width - 38;
			MainLayout.Height = this.Height - ButtonsPanel.Height - 53;

			ButtonsPanel.Top = MainLayout.Top + MainLayout.Height + 1;
			ButtonsPanel.Left = (this.Width - ButtonsPanel.Width) / 2;
			}

		// Сохранение размера формы
		private void GrammarMustJoyForm_ResizeEnd (object sender, EventArgs e)
			{
			// Сохранение настроек
			RDGenerics.SaveWindowDimensions (this);
			// Пересчёт размеров элементов
			for (int i = 0; i < MainLayout.Controls.Count; i++)
				{
				Label l = (Label)MainLayout.Controls[i];
				l.AutoSize = false;
				l.MaximumSize = l.MinimumSize = LogSizeLimit;
				l.AutoSize = true;
				}
			}

		private Size LogSizeLimit
			{
			get
				{
				return new Size (MainLayout.Width - 6 - 18, 0);
				}
			}

		// Запрос сообщения от GMJ
		private void GetGMJExecutor (object sender, DoWorkEventArgs e)
			{
			uint group = NotificationsSupport.GroupSize;
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
			/*string items = "";*/

			string[] values = RDGenerics.WorkResultAsString.Split (groupSplitter,
				StringSplitOptions.RemoveEmptyEntries);
			/*bool empty = string.IsNullOrWhiteSpace (MainText.Text);

			if (values.Length > 0)
				{
				for (int i = 0; i < values.Length; i++)
					items += ((empty ? "" : RDLocale.RNRN + RDLocale.RNRN) +
						values[i].Replace (NotificationsSupport.MainLogItemSplitter.ToString (),
						RDLocale.RN));
				}
			else
				{
				items = (empty ? "" : RDLocale.RNRN) +
					"GMJ не отвечает на запрос. Проверьте интернет-соединение";
				}

			// Добавление в главное окно
			if ((MainText.Text.Length + items.Length > 40000) &&
				(MainText.Text.Length > items.Length))   // Бывает и так
				MainText.Text = MainText.Text.Substring (items.Length, MainText.Text.Length - items.Length);

			MainText.AppendText (items);*/

			if (values.Length < 1)
				{
				AddTextToLayout (GMJ.NoConnectionPattern);
				}
			else
				{
				for (int i = 0; i < values.Length; i++)
					AddTextToLayout (values[i].Replace (NotificationsSupport.MainLogItemSplitter.ToString (),
						RDLocale.RN));
				}
			}

		// Метод добавляет этемент в MainLayout
		private void AddTextToLayout (string Text)
			{
			// Формирование контрола
			Label l = new Label ();
			l.AutoSize = false;

			int amount = NotificationsSupport.TranslucentLogItems ? transculencyAmount : 0;
			if (NotificationsSupport.LogColors.CurrentColor.IsBright)
				l.BackColor = Color.FromArgb (amount, 0, 0, 0);
			else
				l.BackColor = Color.FromArgb (amount, 255, 255, 255);

			l.Click += TextLabel_Clicked;
			l.Font = new Font (fontFamily, NotificationsSupport.LogFontSize / 10.0f);
			l.ForeColor = NotificationsSupport.LogColors.CurrentColor.MainTextColor;
			l.Text = Text;
			l.Margin = LogItemMargin;

			l.MaximumSize = l.MinimumSize = LogSizeLimit;
			l.AutoSize = true;

			// Добавление
			MainLayout.Controls.Add (l);

			while (MainLayout.Controls.Count > NotificationsSupport.MasterLogMaxItems)
				MainLayout.Controls.RemoveAt (0);

			// Прокрутка
			ScrollLog ();
			}

		// Изменение размера шрифта
		// Нажатие на элемент журнала
		private void TextLabel_Clicked (object sender, EventArgs e)
			{
			Label l = (Label)sender;
			textContextSender = MainLayout.Controls.IndexOf (l);

			textContextMenu.MenuItems[1].Enabled = !l.Text.Contains (GMJ.SourceNoReturnPattern) &&
				!l.Text.Contains (GMJ.NoConnectionPattern) && (GMJPicture.AlignString (l.Text) != null);

			textContextMenu.Show (l, Point.Empty);
			}

		// Выбор варианта в меню
		private void TextContext_ItemClicked (object sender, EventArgs e)
			{
			int idx = textContextMenu.MenuItems.IndexOf ((MenuItem)sender);
			if (textContextSender < 0)
				return;
			Label l = (Label)MainLayout.Controls[textContextSender];

			switch (idx)
				{
				// Копировать в буфер
				case 0:
					string notItem = l.Text;
					string notLink = "";

					int left, right;
					if (((left = notItem.IndexOf (GMJ.NumberStringBeginning)) >= 0) &&
						((right = notItem.IndexOf (GMJ.NumberStringEnding, left)) >= 0))
						{
						left += GMJ.NumberStringBeginning.Length;
						notLink = GMJ.SourceRedirectLink + "/" + notItem.Substring (left, right - left);
						}

					bool add = GMJ.EnableCopySubscription && !string.IsNullOrWhiteSpace (notLink);
					RDGenerics.SendToClipboard (notItem + (add ? (RDLocale.RNRN + notLink) : ""), true);
					break;

				// Сохранить картинку
				case 1:
					// Разделение на компоненты
					int hSize = l.Text.IndexOf (RDLocale.RN);
					string header = l.Text.Substring (0, hSize);
					string text = l.Text.Substring (hSize + RDLocale.RNRN.Length);

					string sub = "";
					if (text.EndsWith ("]"))
						{
						int sSize = text.LastIndexOf (RDLocale.RNRN);
						sub = text.Substring (sSize + RDLocale.RNRN.Length);

						sub = sub.Substring (1, sub.Length - 2);
						text = text.Substring (0, sSize);
						}

					int pbk;
					switch (NotificationsSupport.PicturesBackgroundType)
						{
						case NotificationsSupport.PicturesBackgroundRandom:
							pbk = RDGenerics.RND.Next (NotificationsSupport.PictureColors.ColorNames.Length);
							break;

						default:
							pbk = NotificationsSupport.PicturesBackgroundType;
							break;
						}

					// Создание изображения
					Bitmap b = GMJPicture.CreateGMJPicture (header, text, sub,
						NotificationsSupport.PicturesTextAlignment,
						NotificationsSupport.PictureColors.GetColor ((uint)pbk));

					// Сохранение
					SFDialog.FileName = GMJPicture.GetFileNameFromCode (header);

					if (SFDialog.ShowDialog () == DialogResult.OK)
						GMJPicture.SaveGMJPictureToFile (b, SFDialog.FileName);

					b.Dispose ();
					break;
				}
			}

		/*// Изменение размера шрифта
		private void FontSizeField_ValueChanged (object sender, EventArgs e)
			{
			NotificationsSupport.LogFontSize = (uint)(FontSizeField.Value * 10.0m);
			Font fnt = new Font (fontFamily, (float)FontSizeField.Value);

			for (int i = 0; i < MainLayout.Controls.Count; i++)
				{
				Label l = (Label)MainLayout.Controls[i];
				l.Font = fnt;
				l.Margin = LogItemMargin;
				}
			}*/

		private Padding LogItemMargin
			{
			get
				{
				return new Padding (3, 3, 3, (int)NotificationsSupport.LogFontSize /
					(NotificationsSupport.TranslucentLogItems ? 8 : 4));
				}
			}

		/*// Изменение длины группы
		private void GroupCountField_ValueChanged (object sender, EventArgs e)
			{
			NotificationsSupport.GroupSize = (uint)GroupCountField.Value;
			}

		// Вызов контекстного меню иконки
		private void BHelp_Click (object sender, EventArgs e)
			{
			// Создание вызывающего контекстного меню
			if (bHelpContextMenu == null)
				{
				bHelpContextMenu = new ContextMenu ();

				bHelpContextMenu.MenuItems.Add (new MenuItem (GMJ.GMJStatsMenuItem, BHelp_ItemClicked));
				bHelpContextMenu.MenuItems.Add (new MenuItem ("Настройки", BHelp_ItemClicked));
				bHelpContextMenu.MenuItems.Add (new MenuItem (RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
					BHelp_ItemClicked));
				}

			// Вызов
			if (sender != null)
				bHelpContextMenu.Show (BHelp, Point.Empty);
			}*/

		private void BHelp_ItemClicked (object sender, EventArgs e)
			{
			// Извлечение индекса
			int idx = ni.ContextMenu.MenuItems.IndexOf ((MenuItem)sender);

			// Вызов
			switch (idx)
				{
				case 0:
					RDGenerics.MessageBox (RDMessageTypes.Information_Left, GMJ.GMJStats);
					break;

				case 1:
					BSettings_Click (null, null);
					break;

				case 2:
					RDGenerics.ShowAbout (false);
					break;

				case 3:
					allowExit = true;
					this.Close ();
					break;
				}
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

		// Вызов настроек
		private void BSettings_Click (object sender, EventArgs e)
			{
			GMJSettingsForm gmjsf = new GMJSettingsForm ();
			gmjsf.Dispose ();

			ApplyColorsAndFonts ();
			}

		/*// Изменение режима цензурирования
		private void CensorshipFlag_CheckedChanged (object sender, EventArgs e)
			{
			// Внешняя часть
			CensorshipFlag.BackColor = RDGenerics.GetInterfaceColor (CensorshipFlag.Checked ?
				RDInterfaceColors.SuccessMessage : RDInterfaceColors.ErrorMessage);

			// Защита от множественного входа
			if ((sender == null) || csReverse)
				return;

			// Внутренняя часть
			string msg = CensorshipFlag.Checked ? GMJ.CensorshipEnableMessage2 : GMJ.CensorshipDisableMessage2;
			if (RDGenerics.MessageBox (RDMessageTypes.Warning_Left, msg,
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_YesNoFocus),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) == RDMessageButtons.ButtonOne)
				{
				GMJ.EnableCensorship = CensorshipFlag.Checked;
				}
			else
				{
				csReverse = true;
				CensorshipFlag.Checked = GMJ.EnableCensorship;
				csReverse = false;
				return;
				}

			msg = CensorshipFlag.Checked ? GMJ.CensorshipEnableResetMessage : GMJ.CensorshipDisableResetMessage;
			if (RDGenerics.MessageBox (RDMessageTypes.Warning_Left, msg,
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_YesNoFocus),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)) == RDMessageButtons.ButtonOne)
				{
				GMJ.ResetFreeSet ();
				}
			}*/
		}
	}

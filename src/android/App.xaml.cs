﻿using Microsoft.Maui.Controls;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace RD_AAOW
	{
	/// <summary>
	/// Класс описывает функционал приложения
	/// </summary>
	public partial class App: Application
		{
		#region Общие переменные и константы

		// Флаги прав доступа
		private RDAppStartupFlags flags;

		// Главный журнал приложения
		private List<MainLogItem> masterLog;

		// Управление центральной кнопкой журнала
		private bool centerButtonEnabled = true;
		private const string semaphoreOn = "●";
		private const string semaphoreOff = "○";

		// Флаг завершения прокрутки журнала
		private bool needsScroll = true;

		// Сформированные контекстные меню
		private List<List<string>> tapMenuItems2 = new List<List<string>> ();
		private List<string> specialOptions = new List<string> ();

		// Цветовая схема
		private readonly Color
			logMasterBackColor = Color.FromArgb ("#F0F0F0"),
			logFieldBackColor = Color.FromArgb ("#80808080"),
			logReadModeColor = Color.FromArgb ("#202020"),

			settingsMasterBackColor = Color.FromArgb ("#FFF8F0"),
			settingsFieldBackColor = Color.FromArgb ("#FFE8D0"),

			solutionLockedBackColor = Color.FromArgb ("#F0F0F0"),

			aboutMasterBackColor = Color.FromArgb ("#F0FFF0"),
			aboutFieldBackColor = Color.FromArgb ("#D0FFD0");

		#endregion

		#region Переменные страниц

		private ContentPage settingsPage, aboutPage, logPage;

		private Label aboutLabel, fontSizeFieldLabel, groupSizeFieldLabel, aboutFontSizeField;

		private Switch nightModeSwitch, newsAtTheEndSwitch, keepScreenOnSwitch,
			enablePostSubscriptionSwitch;

		private Button centerButton, scrollUpButton, scrollDownButton, menuButton, addButton,
			pictureBackButton, pTextOnTheLeftButton, censorshipButton;

		private ListView mainLog;

		private List<string> pageVariants = new List<string> ();
		private List<string> pictureBKVariants = new List<string> ();
		private List<string> pictureBKSelectionVariants = new List<string> ();
		private List<string> pictureTAVariants = new List<string> ();
		private List<string> pictureTASelectionVariants = new List<string> ();
		private List<string> censorshipVariants = new List<string> ();

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();
			flags = AndroidSupport.GetAppStartupFlags (RDAppStartupFlags.DisableXPUN | RDAppStartupFlags.CanWriteFiles);

			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			// Общая конструкция страниц приложения
			MainPage = new MasterPage ();

			settingsPage = AndroidSupport.ApplyPageSettings (new SettingsPage (), "SettingsPage",
				"Настройки приложения", settingsMasterBackColor);
			aboutPage = AndroidSupport.ApplyPageSettings (new AboutPage (), "AboutPage",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout), aboutMasterBackColor);
			logPage = AndroidSupport.ApplyPageSettings (new LogPage (), "LogPage",
				"Журнал", logMasterBackColor);

			AndroidSupport.SetMasterPage (MainPage, logPage, logMasterBackColor);

			if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.PolicyTip))
				AndroidSupport.SetCurrentPage (settingsPage, settingsMasterBackColor);

			// Настройки просмотра
			AndroidSupport.ApplyLabelSettings (settingsPage, "AppSettingsLabel",
				"Просмотр", RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (settingsPage, "KeepScreenOnLabel",
				"Запретить переход в спящий режим,\nпока приложение открыто", RDLabelTypes.DefaultLeft);
			keepScreenOnSwitch = AndroidSupport.ApplySwitchSettings (settingsPage, "KeepScreenOnSwitch",
				false, settingsFieldBackColor, KeepScreenOnSwitch_Toggled, NotificationsSupport.KeepScreenOn);

			Label eps = AndroidSupport.ApplyLabelSettings (settingsPage, "EnablePostSubscriptionLabel",
				"Добавлять ссылку на оригинал\nзаписи к тексту при действиях\n«Скопировать» и «Поделиться»",
				RDLabelTypes.DefaultLeft);
			enablePostSubscriptionSwitch = AndroidSupport.ApplySwitchSettings (settingsPage,
				"EnablePostSubscriptionSwitch", false, settingsFieldBackColor,
				EnablePostSubscription_Toggled, GMJ.EnablePostSubscription);

			if (AndroidSupport.IsTV)
				{
				GMJ.EnablePostSubscription = false;
				eps.IsVisible = enablePostSubscriptionSwitch.IsVisible = false;
				}

			#region Страница "О программе"

			aboutLabel = AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click, false);

			Button hlp = AndroidSupport.ApplyButtonSettings (aboutPage, "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click, false);
			hlp.IsVisible = !AndroidSupport.IsTV;

			Image qrImage = (Image)aboutPage.FindByName ("QRImage");
			qrImage.IsVisible = AndroidSupport.IsTV;

			AndroidSupport.ApplyLabelSettings (aboutPage, "GenericSettingsLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_GenericSettings),
				RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (aboutPage, "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.Tip);

			AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_InterfaceFontSize),
				RDLabelTypes.DefaultLeft);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeInc",
				RDDefaultButtons.Increase, aboutFieldBackColor, FontSizeButton_Clicked);
			AndroidSupport.ApplyButtonSettings (aboutPage, "FontSizeDec",
				RDDefaultButtons.Decrease, aboutFieldBackColor, FontSizeButton_Clicked);
			aboutFontSizeField = AndroidSupport.ApplyLabelSettings (aboutPage, "FontSizeField",
				" ", RDLabelTypes.DefaultCenter);

			AndroidSupport.ApplyLabelSettings (aboutPage, "HelpHeaderLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
				RDLabelTypes.HeaderLeft);
			Label htl = AndroidSupport.ApplyLabelSettings (aboutPage, "HelpTextLabel",
				AndroidSupport.GetAppHelpText (), RDLabelTypes.SmallLeft);
			htl.TextType = TextType.Html;

			FontSizeButton_Clicked (null, null);

			#endregion

			// Страница журнала приложения
			mainLog = (ListView)logPage.FindByName ("MainLog");
			mainLog.BackgroundColor = logFieldBackColor;
			mainLog.HasUnevenRows = true;
			mainLog.ItemTapped += MainLog_ItemTapped;
			mainLog.ItemTemplate = new DataTemplate (typeof (NotificationView));
			mainLog.SelectionMode = ListViewSelectionMode.None;
			mainLog.SeparatorVisibility = SeparatorVisibility.None;
			mainLog.ItemAppearing += MainLog_ItemAppearing;
			AndroidSupport.MasterPage.Popped += CurrentPageChanged;

			if (AndroidSupport.IsTV)
				{
				mainLog.VerticalScrollBarVisibility = ScrollBarVisibility.Always;
				mainLog.SelectionMode = ListViewSelectionMode.Single;
				}

			centerButton = AndroidSupport.ApplyButtonSettings (logPage, "CenterButton", " ",
				logFieldBackColor, CenterButton_Click, false);
			centerButton.FontSize += 6;

			scrollUpButton = AndroidSupport.ApplyButtonSettings (logPage, "ScrollUp",
				RDDefaultButtons.Up, logFieldBackColor, ScrollUpButton_Click);
			scrollDownButton = AndroidSupport.ApplyButtonSettings (logPage, "ScrollDown",
				RDDefaultButtons.Down, logFieldBackColor, ScrollDownButton_Click);

			// Режим чтения
			AndroidSupport.ApplyLabelSettings (settingsPage, "ReadModeLabel",
				"Тёмная тема для журнала", RDLabelTypes.DefaultLeft);
			nightModeSwitch = AndroidSupport.ApplySwitchSettings (settingsPage, "ReadModeSwitch",
				false, settingsFieldBackColor, NightModeSwitch_Toggled, NotificationsSupport.LogReadingMode);

			// Расположение новых записей в конце журнала
			AndroidSupport.ApplyLabelSettings (settingsPage, "NewsAtTheEndLabel",
				"Добавлять новые записи в конец журнала", RDLabelTypes.DefaultLeft);
			newsAtTheEndSwitch = AndroidSupport.ApplySwitchSettings (settingsPage, "NewsAtTheEndSwitch",
				false, settingsFieldBackColor, NewsAtTheEndSwitch_Toggled, NotificationsSupport.LogNewsItemsAtTheEnd);

			menuButton = AndroidSupport.ApplyButtonSettings (logPage, "MenuButton",
				RDDefaultButtons.Menu, logFieldBackColor, SelectPage);
			addButton = AndroidSupport.ApplyButtonSettings (logPage, "AddButton",
				RDDefaultButtons.Increase, logFieldBackColor, OfferTheRecord);
			addButton.IsVisible = !AndroidSupport.IsTV;

			AndroidSupport.ApplyLabelSettings (settingsPage, "LogSettingsLabel",
				"Главный журнал", RDLabelTypes.HeaderLeft);

			// Режим чтения
			NightModeSwitch_Toggled (null, null);

			// Размер шрифта
			fontSizeFieldLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "FontSizeFieldLabel",
				"", RDLabelTypes.DefaultLeft);
			fontSizeFieldLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (settingsPage, "FontSizeIncButton",
				RDDefaultButtons.Increase, settingsFieldBackColor, FontSizeChanged);
			AndroidSupport.ApplyButtonSettings (settingsPage, "FontSizeDecButton",
				RDDefaultButtons.Decrease, settingsFieldBackColor, FontSizeChanged);

			FontSizeChanged (null, null);

			// Размер группы запрашиваемых записей
			groupSizeFieldLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "GroupSizeFieldLabel",
				"", RDLabelTypes.DefaultLeft);
			groupSizeFieldLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (settingsPage, "GroupSizeIncButton",
				RDDefaultButtons.Increase, settingsFieldBackColor, GroupSizeChanged);
			AndroidSupport.ApplyButtonSettings (settingsPage, "GroupSizeDecButton",
				RDDefaultButtons.Decrease, settingsFieldBackColor, GroupSizeChanged);

			GroupSizeChanged (null, null);

			AndroidSupport.ApplyLabelSettings (settingsPage, "CensorshipLabel",
				"Цензурирование:", RDLabelTypes.DefaultLeft);
			censorshipButton = AndroidSupport.ApplyButtonSettings (settingsPage, "CensorshipButton",
				" ", settingsFieldBackColor, Censorship_Clicked, false);

			Censorship_Clicked (null, null);

			// Настройки картинок
			Label pictLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "PicturesLabel",
				"Изображения", RDLabelTypes.HeaderLeft);

			Label pictBackLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "PicturesBackLabel",
				"Фон:", RDLabelTypes.DefaultLeft);
			pictureBackButton = AndroidSupport.ApplyButtonSettings (settingsPage, "PicturesBackButton",
				" ", settingsFieldBackColor, PictureBack_Clicked, false);

			Label pictTextLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "PTextLeftLabel",
				"Текст:", RDLabelTypes.DefaultLeft);
			pTextOnTheLeftButton = AndroidSupport.ApplyButtonSettings (settingsPage, "PTextLeftButton",
				" ", settingsFieldBackColor, PTextOnTheLeft_Toggled, false);

			if (AndroidSupport.IsTV)
				{
				pictLabel.IsVisible = pictBackLabel.IsVisible = pictureBackButton.IsVisible =
					pictTextLabel.IsVisible = pTextOnTheLeftButton.IsVisible = false;
				}
			else
				{
				PictureBack_Clicked (null, null);
				PTextOnTheLeft_Toggled (null, null);
				}

			// Запуск цикла обратной связи (без ожидания)
			FinishBackgroundRequest ();

			// Принятие соглашений
			ShowStartupTips ();
			}

		// Исправление для сброса текущей позиции журнала
		private async void CurrentPageChanged (object sender, EventArgs e)
			{
			if (AndroidSupport.MasterPage.CurrentPage != logPage)
				return;

			needsScroll = true;
			await ScrollMainLog (newsAtTheEndSwitch.IsToggled, -1);
			}

		// Цикл обратной связи для загрузки текущего журнала, если фоновая служба не успела завершить работу
		private bool FinishBackgroundRequest ()
			{
			// Ожидание завершения операции
			SetLogState (false);

			UpdateLogButton (true, true);

			// Перезапрос журнала
			if (masterLog != null)
				masterLog.Clear ();
			masterLog = new List<MainLogItem> (NotificationsSupport.GetMasterLog (true));

			needsScroll = true;
			UpdateLog ();

			SetLogState (true);
			return true;
			}

		// Метод отображает подсказки при первом запуске
		private async void ShowStartupTips ()
			{
			// Контроль XPUN
			if (!flags.HasFlag (RDAppStartupFlags.DisableXPUN))
				await AndroidSupport.XPUNLoop ();

			// Требование принятия Политики
			if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.PolicyTip))
				{
				if (!AndroidSupport.IsTV)
					await AndroidSupport.PolicyLoop ();
				NotificationsSupport.TipsState |= NSTipTypes.PolicyTip;
				}

			// Подсказки
			if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.StartupTips))
				{
				await AndroidSupport.ShowMessage ("Добро пожаловать в мини-клиент Grammar must joy!" + RDLocale.RNRN +
					"• На этой странице Вы можете настроить поведение приложения." + RDLocale.RNRN +
					"• Используйте системную кнопку «Назад», чтобы вернуться к журналу записей " +
					"из любого раздела." + RDLocale.RNRN +
					"• Используйте кнопку с семафором для получения случайных записей из сообщества GMJ",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next));

				if (AndroidSupport.IsTV)
					{
					await AndroidSupport.ShowMessage ("Внимание!" + RDLocale.RNRN +
						"• Убедитесь, что данное устройство имеет выход в интернет. Без него " +
						"приложение не сможет продолжить работу." + RDLocale.RNRN +
						"• Ознакомьтесь с описанием проекта в разделе «О приложении» (кнопка ≡). Убедитесь, " +
						"что Вы согласны с Политикой сообщества и Политикой разработки продукта",
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
					}
				else
					{
					await AndroidSupport.ShowMessage ("Внимание!" + RDLocale.RNRN +
						"Некоторые устройства требуют ручного разрешения на доступ в интернет " +
						"(например, если активен режим экономии интернет-трафика). Проверьте его, " +
						"если запросы не будут работать правильно",
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
					}

				NotificationsSupport.TipsState |= NSTipTypes.StartupTips;
				}
			}

		// Метод отображает остальные подсказки
		private async Task<bool> ShowTips (NSTipTypes Type)
			{
			// Подсказки
			string msg = "";
			switch (Type)
				{
				case NSTipTypes.GoToButton:
					msg = "Эта опция позволяет открыть выбранную запись в Telegram или в браузере";
					break;

				case NSTipTypes.ShareTextButton:
					msg = "Эта опция позволяет поделиться текстом записи";
					if (GMJ.EnablePostSubscription)
						msg += ("." + RDLocale.RNRN +
							"Обратите внимание, что приложение добавляет к текстам, которыми Вы делитесь, " +
							"ссылку на сообщество Grammar must joy");
					break;

				case NSTipTypes.ShareImageButton:
					msg = "Эта опция позволяет поделиться записью в виде картинки";
					break;

				case NSTipTypes.MainLogClickMenuTip:
					msg = "Все операции с текстами записей доступны по клику на них в главном журнале";
					break;

				case NSTipTypes.KeepScreenOnTip:
					msg = "Этот переключатель позволяет экрану оставаться активным, пока Вы читаете " +
						"тексты записей (т. е. пока приложение открыто)";
					break;
				}

			await AndroidSupport.ShowMessage (msg, RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			NotificationsSupport.TipsState |= Type;
			return true;
			}

		/// <summary>
		/// Сохранение настроек программы
		/// </summary>
		protected override void OnSleep ()
			{
			AndroidSupport.StopRequested = true;
			NotificationsSupport.SetMasterLog (masterLog);
			AndroidSupport.AppIsRunning = false;
			}

		/// <summary>
		/// Возврат в интерфейс при сворачивании
		/// </summary>
		protected override void OnResume ()
			{
			AndroidSupport.MasterPage.PopToRootAsync (true);

			// Запуск цикла обратной связи (без ожидания, на случай, если приложение было свёрнуто, но не закрыто,
			// а во время ожидания имели место обновления журнала)
			AndroidSupport.AppIsRunning = true;
			FinishBackgroundRequest ();
			}

		/// <summary>
		/// Возврат в интерфейс из статичного оповещения (использует перенаправление в MasterPage)
		/// </summary>
		public void ResumeApp ()
			{
			OnResume ();
			}

		#endregion

		#region Журнал

		// Принудительное обновление лога
		private void UpdateLog ()
			{
			mainLog.ItemsSource = null;
			mainLog.ItemsSource = masterLog;
			}

		// Промотка журнала к нужной позиции
		private async void MainLog_ItemAppearing (object sender, ItemVisibilityEventArgs e)
			{
			await ScrollMainLog (newsAtTheEndSwitch.IsToggled, e.ItemIndex);
			}

		private async Task<bool> ScrollMainLog (bool ToTheEnd, int VisibleItem)
			{
			// Контроль
			if (masterLog == null)
				return false;

			if ((masterLog.Count < 1) || !needsScroll)
				return false;

			// Искусственная задержка
			await Task.Delay (100);

			// Промотка с повторением до достижения нужного участка
			if (VisibleItem < 0)
				needsScroll = false;

			if (ToTheEnd)
				{
				if (VisibleItem > masterLog.Count - 3)
					needsScroll = false;

				mainLog.ScrollTo (masterLog[masterLog.Count - 1], ScrollToPosition.MakeVisible, false);
				}
			else
				{
				if (VisibleItem < 2)
					needsScroll = false;

				mainLog.ScrollTo (masterLog[0], ScrollToPosition.MakeVisible, false);
				}

			return true;
			}

		// Обновление формы кнопки журнала
		private void UpdateLogButton (bool Requesting, bool FinishingBackgroundRequest)
			{
			bool red = Requesting && FinishingBackgroundRequest;
			bool yellow = Requesting && !FinishingBackgroundRequest;
			bool green = !Requesting && !FinishingBackgroundRequest;
			bool dark = nightModeSwitch.IsToggled;

			if (red || yellow || green)
				{
				centerButton.Text = (red ? semaphoreOn : semaphoreOff) + (yellow ? semaphoreOn : semaphoreOff) +
					(green ? semaphoreOn : semaphoreOff);

				if (red)
					centerButton.TextColor = Color.FromArgb (dark ? "#FF4040" : "#D00000");
				else if (yellow)
					centerButton.TextColor = Color.FromArgb (dark ? "#FFFF40" : "#D0D000");
				else
					centerButton.TextColor = Color.FromArgb (dark ? "#40FF40" : "#00D000");
				}
			else
				{
				centerButton.Text = "   ";
				}

			if (AndroidSupport.IsTV)
				centerButton.Focus ();
			}

		// Выбор оповещения для перехода или share
		private async void MainLog_ItemTapped (object sender, ItemTappedEventArgs e)
			{
			// Контроль
			if (AndroidSupport.IsTV)
				return;

			MainLogItem notItem = (MainLogItem)e.Item;
			if (!centerButtonEnabled || (notItem.StringForSaving == ""))  // Признак разделителя
				return;

			// Сброс состояния
			UpdateLogButton (false, false);

			// Извлечение ссылки и номера оповещения
			string notLink = "";
			int l, r;
			if (((l = notItem.Header.IndexOf (GMJ.NumberStringBeginning)) >= 0) &&
				((r = notItem.Header.IndexOf (GMJ.NumberStringEnding, l)) >= 0))
				{
				l += GMJ.NumberStringBeginning.Length;
				notLink = GMJ.SourceRedirectLink + "/" + notItem.Header.Substring (l, r - l);
				}

			// Формирование меню
			const string secondMenuName = "Ещё...";
			if (tapMenuItems2.Count < 1)
				{
				tapMenuItems2.Add (new List<string> {
					"☍\tПоделиться текстом",
					"❏\tСкопировать текст",
					secondMenuName,
					});
				tapMenuItems2.Add (new List<string> {
					"▷\tПерейти к источнику",
					"☍\tПоделиться текстом",
					"❏\tСкопировать текст",
					secondMenuName,
					});
				tapMenuItems2.Add (new List<string> {
					"▷\tПерейти к источнику",
					"☍\tПоделиться текстом",
					"☻\tПоделиться картинкой",
					"❏\tСкопировать текст",
					secondMenuName,
					});
				tapMenuItems2.Add (new List<string> {
					"✕\tУдалить из журнала",
					});
				}

			// Запрос варианта использования
			int menuVariant = 0;
			if (!string.IsNullOrWhiteSpace (notLink))
				{
				menuVariant++;
				if (GMJPicture.AlignString (notItem.Text) != null)
					menuVariant++;
				}

			int menuItem = await AndroidSupport.ShowList ("Выберите действие:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel),
				tapMenuItems2[menuVariant]);

			if (menuItem < 0)
				return;

			bool secondMenu = (tapMenuItems2[menuVariant][menuItem] == secondMenuName);
			menuVariant = menuItem + 10 * (menuVariant + 1);

			// Контроль второго набора
			if (secondMenu)
				{
				menuVariant = 3;
				menuItem = await AndroidSupport.ShowList ("Выберите действие:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), tapMenuItems2[menuVariant]);
				if (menuItem < 0)
					return;

				/*variant += menuItem;*/
				menuVariant = menuItem + 10 * (menuVariant + 1);
				}

			// Обработка (неподходящие варианты будут отброшены)
			switch (menuVariant)
				{
				// Переход по ссылке
				case 20:
				case 30:
					if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.GoToButton))
						await ShowTips (NSTipTypes.GoToButton);

					try
						{
						await Launcher.OpenAsync (notLink);
						}
					catch
						{
						AndroidSupport.ShowBalloon
							(RDLocale.GetDefaultText (RDLDefaultTexts.Message_BrowserNotAvailable), true);
						}
					break;

				// Поделиться
				case 10:
				case 21:
				case 31:
					if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.ShareTextButton))
						await ShowTips (NSTipTypes.ShareTextButton);

					await Share.RequestAsync ((notItem.Header + RDLocale.RNRN + notItem.Text +
						(GMJ.EnablePostSubscription ? (RDLocale.RNRN + notLink) : "")).Replace ("\r", ""),
						ProgramDescription.AssemblyVisibleName);
					break;

				// Скопировать в буфер обмена
				case 11:
				case 22:
				case 33:
					RDGenerics.SendToClipboard ((notItem.Header + RDLocale.RNRN + notItem.Text +
						(GMJ.EnablePostSubscription ? (RDLocale.RNRN + notLink) : "")).Replace ("\r", ""),
						true);
					break;

				// Поделиться картинкой
				case 32:
					if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.ShareImageButton))
						await ShowTips (NSTipTypes.ShareImageButton);

					if (!flags.HasFlag (RDAppStartupFlags.CanWriteFiles))
						{
						if (await AndroidSupport.ShowMessage (
							RDLocale.GetDefaultText (RDLDefaultTexts.Message_ReadWritePermission) + "." +
							RDLocale.RNRN + "Перейти к настройкам разрешений приложения?",
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
							AndroidSupport.CallAppSettings ();
						return;
						}

					GMJPictureTextAlignment pta = NotificationsSupport.PicturesTextAlignment;
					if (pta == GMJPictureTextAlignment.AskUser)
						{
						int res = await AndroidSupport.ShowList ("Выровнять текст:",
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pictureTASelectionVariants);
						if (res < 0)
							return;

						pta = (GMJPictureTextAlignment)res;
						}

					GMJPictureBackground pbk = NotificationsSupport.PicturesBackgroundType;
					if (pbk == GMJPictureBackground.AskUser)
						{
						int res = await AndroidSupport.ShowList ("Использовать фон:",
							RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pictureBKSelectionVariants);
						if (res < 0)
							return;

						pbk = (GMJPictureBackground)(res + 2);
						}

					var pict = GMJPicture.CreateGMJPicture (notItem.Header, notItem.Text, pta, pbk);
					if (pict == null)
						{
						AndroidSupport.ShowBalloon ("Текст записи не позволяет сформировать картинку", true);
						return;
						}

					await GMJPicture.SaveGMJPictureToFile (pict, notItem.Header + ".png");
					break;

				// Удаление из журнала
				case 40:
					masterLog.RemoveAt (e.ItemIndex);
					UpdateLog ();
					break;
				}

			// Завершено
			}

		// Блокировка / разблокировка кнопок
		private void SetLogState (bool State)
			{
			// Переключение состояния кнопок и свичей
			centerButtonEnabled = State;
			menuButton.IsVisible = State;
			addButton.IsVisible = State && !AndroidSupport.IsTV;

			// Обновление статуса
			UpdateLogButton (!State, false);
			}

		// Добавление текста в журнал
		private void AddTextToLog (string Text)
			{
			if (newsAtTheEndSwitch.IsToggled)
				{
				masterLog.Add (new MainLogItem (Text));

				// Удаление верхних строк
				while (masterLog.Count >= ProgramDescription.MasterLogMaxItems)
					masterLog.RemoveAt (0);
				}
			else
				{
				masterLog.Insert (0, new MainLogItem (Text));

				// Удаление нижних строк (здесь требуется, т.к. не выполняется обрезка свойством .MainLog)
				while (masterLog.Count >= ProgramDescription.MasterLogMaxItems)
					masterLog.RemoveAt (masterLog.Count - 1);
				}
			}

		// Действия средней кнопки журнала
		private void CenterButton_Click (object sender, EventArgs e)
			{
			if (!centerButtonEnabled)
				{
				AndroidSupport.ShowBalloon ("Пожалуйста, дождитесь ответа на запрос...", true);
				return;
				}

			GetGMJ ();
			}

		private async void GetGMJ ()
			{
			// Блокировка на время опроса
			SetLogState (false);
			AndroidSupport.ShowBalloon ("Запрос случайной записи...", false);

			// Запуск и разбор
			AndroidSupport.StopRequested = false; // Разблокировка метода GetHTML
			string newText = "";
			uint group = NotificationsSupport.GroupSize;
			bool success = false;

			for (int i = 0; i < group; i++)
				{
				// Антиспам
				if (i > 0)
					Thread.Sleep (1000);

				newText = await Task.Run<string> (GMJ.GetRandomGMJ);
				if (newText == "")
					{
					AndroidSupport.ShowBalloon ("Grammar must joy не ответила на запрос. " +
						"Проверьте интернет-соединение", false);
					}
				else if (newText.Contains (GMJ.SourceNoReturnPattern))
					{
					AndroidSupport.ShowBalloon (newText, false);
					}
				else
					{
					AddTextToLog (newText);
					needsScroll = true;
					UpdateLog ();
					success = true;
					}
				}

			// Разблокировка
			SetLogState (true);
			UpdateLogButton (!success, !success);
			if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.MainLogClickMenuTip))
				await ShowTips (NSTipTypes.MainLogClickMenuTip);
			}

		// Ручная прокрутка
		private async void ScrollUpButton_Click (object sender, EventArgs e)
			{
			needsScroll = true;
			await ScrollMainLog (false, -1);
			}

		private async void ScrollDownButton_Click (object sender, EventArgs e)
			{
			needsScroll = true;
			await ScrollMainLog (true, -1);
			}

		// Выбор текущей страницы
		private async void SelectPage (object sender, EventArgs e)
			{
			// Запрос варианта
			if (pageVariants.Count < 1)
				{
				pageVariants = new List<string> ()
					{
					"Настройки приложения",
					RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout),
					};
				}

			int res = await AndroidSupport.ShowList (RDLocale.GetDefaultText (RDLDefaultTexts.Button_GoTo),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pageVariants);
			if (res < 0)
				return;

			// Вызов
			switch (res)
				{
				case 0:
					AndroidSupport.SetCurrentPage (settingsPage, settingsMasterBackColor);
					break;

				case 1:
					AndroidSupport.SetCurrentPage (aboutPage, aboutMasterBackColor);
					break;
				}
			}

		// Предложение записи сообществу
		private async void OfferTheRecord (object sender, EventArgs e)
			{
			if (!await AndroidSupport.ShowMessage (GMJ.SuggestionMessage,
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
				return;

			await AndroidSupport.AskDeveloper ();
			}

		#endregion

		#region Основные настройки

		// Включение / выключение фиксации экрана
		private async void KeepScreenOnSwitch_Toggled (object sender, ToggledEventArgs e)
			{
			// Подсказки
			if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.KeepScreenOnTip))
				await ShowTips (NSTipTypes.KeepScreenOnTip);

			NotificationsSupport.KeepScreenOn = keepScreenOnSwitch.IsToggled;
			}

		// Включение / выключение добавления новостей с конца журнала
		private void NewsAtTheEndSwitch_Toggled (object sender, ToggledEventArgs e)
			{
			// Обновление журнала
			if (e != null)
				NotificationsSupport.LogNewsItemsAtTheEnd = newsAtTheEndSwitch.IsToggled;

			UpdateLogButton (false, false);
			}

		// Включение / выключение режима чтения для лога
		private void NightModeSwitch_Toggled (object sender, ToggledEventArgs e)
			{
			if (e != null)
				NotificationsSupport.LogReadingMode = nightModeSwitch.IsToggled;

			if (nightModeSwitch.IsToggled)
				{
				logPage.BackgroundColor = mainLog.BackgroundColor = centerButton.BackgroundColor =
					scrollUpButton.BackgroundColor = scrollDownButton.BackgroundColor =
					menuButton.BackgroundColor = addButton.BackgroundColor = logReadModeColor;
				NotificationsSupport.LogFontColor = logMasterBackColor;
				}
			else
				{
				logPage.BackgroundColor = mainLog.BackgroundColor = centerButton.BackgroundColor =
					scrollUpButton.BackgroundColor = scrollDownButton.BackgroundColor =
					menuButton.BackgroundColor = addButton.BackgroundColor = logMasterBackColor;
				NotificationsSupport.LogFontColor = logReadModeColor;
				}
			scrollUpButton.TextColor = scrollDownButton.TextColor = menuButton.TextColor =
				addButton.TextColor = NotificationView.CurrentAntiBackColor;

			// Принудительное обновление (только не при старте)
			if (e != null)
				{
				needsScroll = true;
				UpdateLog ();
				}

			// Цепляет кнопку журнала
			UpdateLogButton (false, false);
			}

		// Изменение размера шрифта лога
		private void FontSizeChanged (object sender, EventArgs e)
			{
			uint fontSize = NotificationsSupport.LogFontSize;

			if (e != null)
				{
				Button b = (Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Increase) &&
					(fontSize < AndroidSupport.MaxFontSize))
					fontSize++;
				else if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Decrease) &&
					(fontSize > AndroidSupport.MinFontSize))
					fontSize--;

				NotificationsSupport.LogFontSize = fontSize;
				}

			// Принудительное обновление
			fontSizeFieldLabel.Text = string.Format ("Размер шрифта в журнале: <b>{0:D}</b>", fontSize.ToString ());

			if (e != null)
				{
				needsScroll = true;
				UpdateLog ();
				}
			}

		// Изменение размера шрифта лога
		private void GroupSizeChanged (object sender, EventArgs e)
			{
			uint groupSize = NotificationsSupport.GroupSize;

			if (e != null)
				{
				Button b = (Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Increase) &&
					(groupSize < 5))
					groupSize++;
				else if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Decrease) &&
					(groupSize > 1))
					groupSize--;

				NotificationsSupport.GroupSize = groupSize;
				}

			// Принудительное обновление
			groupSizeFieldLabel.Text = string.Format ("Число записей, получаемых<br/>по одному нажатию кнопки: " +
				"<b>{0:D}</b>", groupSize.ToString ());
			}

		// Включение / выключение подписи
		private async void EnablePostSubscription_Toggled (object sender, ToggledEventArgs e)
			{
			// Подсказки
			if (!enablePostSubscriptionSwitch.IsToggled)
				await AndroidSupport.ShowMessage ("Мы не имеем ничего против отключения подписей у текстов. " +
					"Честно. Всё-таки юмор – это общественное достояние, не допускающее каких-либо ограничений." +
					RDLocale.RNRN + "Однако мы будем Вам весьма признательны, если Вы упомянете нас в качестве " +
					"источника. Спасибо!",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));

			GMJ.EnablePostSubscription = enablePostSubscriptionSwitch.IsToggled;
			}

		// Выбор фона картинок
		private async void PictureBack_Clicked (object sender, EventArgs e)
			{
			// Запрос варианта
			if (pictureBKVariants.Count < 1)
				{
				pictureBKVariants.Add ("(спрашивать)");
				pictureBKVariants.Add ("(случайный)");
				pictureBKVariants.AddRange (GMJPicture.BackgroundNames);
				pictureBKSelectionVariants.AddRange (GMJPicture.BackgroundNames);
				}

			int res;
			if (sender == null)
				{
				res = (int)NotificationsSupport.PicturesBackgroundType;
				}
			else
				{
				res = await AndroidSupport.ShowList ("Фон картинок",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pictureBKVariants);
				if (res < 0)
					return;

				NotificationsSupport.PicturesBackgroundType = (GMJPictureBackground)res;
				}

			pictureBackButton.Text = pictureBKVariants[res];
			}

		// Выбор режима выравнивания текста картинок
		private async void PTextOnTheLeft_Toggled (object sender, EventArgs e)
			{
			// Запрос варианта
			if (pictureTAVariants.Count < 1)
				{
				pictureTAVariants = new List<string> {
					"Всегда по центру",
					"Всегда по левой стороне",
					"Диалоги по левой стороне",
					"Запрашивать выравнивание",
					};
				pictureTASelectionVariants = new List<string> {
					"По центру",
					"По левой стороне",
					};
				}

			int res;
			if (sender == null)
				{
				res = (int)NotificationsSupport.PicturesTextAlignment;
				}
			else
				{
				res = await AndroidSupport.ShowList ("Выравнивание текста",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pictureTAVariants);
				if (res < 0)
					return;

				NotificationsSupport.PicturesTextAlignment = (GMJPictureTextAlignment)res;
				if (NotificationsSupport.PicturesTextAlignment == GMJPictureTextAlignment.BasedOnDialogues)
					await AndroidSupport.ShowMessage ("Этот вариант предполагает, что тексты, содержащие диалоги, " +
						"будут выравниваться по левой стороне, а остальные – по центру",
						RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				}

			// Сохранение
			pTextOnTheLeftButton.Text = pictureTAVariants[res];
			}

		// Выбор режима цензурирования
		private async void Censorship_Clicked (object sender, EventArgs e)
			{
			// Запрос варианта
			if (censorshipVariants.Count < 1)
				{
				censorshipVariants = new List<string> {
					"Отключено",
					"Действует",
					};
				}

			int res;
			if (sender == null)
				{
				res = GMJ.EnableCensorship ? 1 : 0;
				censorshipButton.Text = censorshipVariants[res];
				return;
				}

			res = await AndroidSupport.ShowList ("Цензурирование",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), censorshipVariants);
			if (res < 0)
				return;

			// Контроль
			string msg = (res > 0) ? GMJ.CensorshipEnableMessage : GMJ.CensorshipDisableMessage;
			if (await AndroidSupport.ShowMessage (msg, RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel)))
				{
				GMJ.EnableCensorship = (res > 0);
				censorshipButton.Text = censorshipVariants[res];
				GMJ.ResetFreeSet ();
				}
			}

		#endregion

		#region О приложении

		// Вызов справочных материалов
		private async void ReferenceButton_Click (object sender, EventArgs e)
			{
			if (AndroidSupport.IsTV)
				{
				await AndroidSupport.ShowMessage ("Для доступа к помощи, поддержке и справочным материалам " +
					"воспользуйтесь QR-кодом, представленным ниже." + RDLocale.RNRN +
					"Далее на этой странице доступно сокращённое описание приложения",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
				return;
				}

			await AndroidSupport.CallHelpMaterials (RDHelpMaterials.ReferenceMaterials);
			}

		private async void HelpButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.CallHelpMaterials (RDHelpMaterials.HelpAndSupport);
			}

		// Изменение размера шрифта интерфейса
		private void FontSizeButton_Clicked (object sender, EventArgs e)
			{
			if (sender != null)
				{
				Button b = (Button)sender;
				if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Increase))
					AndroidSupport.MasterFontSize += 0.5;
				else if (AndroidSupport.IsNameDefault (b.Text, RDDefaultButtons.Decrease))
					AndroidSupport.MasterFontSize -= 0.5;
				}

			aboutFontSizeField.Text = AndroidSupport.MasterFontSize.ToString ("F1");
			aboutFontSizeField.FontSize = AndroidSupport.MasterFontSize;
			}

		#endregion
		}
	}

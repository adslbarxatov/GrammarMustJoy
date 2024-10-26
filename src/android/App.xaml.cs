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

		// Параметры прокрутки журнала
		private bool needsScroll = true;
		private int currentScrollItem;
		private int tvScrollPosition;

		private const int autoScrollMode = -1;
		private const int manualScrollModeUp = -2;
		private const int manualScrollModeDown = -3;

		// Сформированные контекстные меню
		private List<List<string>> tapMenuItems2 = new List<List<string>> ();
		private List<string> specialOptions = new List<string> ();

		// Цветовая схема
		private readonly Color
			logMasterBackColor = Color.FromArgb ("#F0F0F0"),
			logFieldBackColor = Color.FromArgb ("#80808080"),

			settingsMasterBackColor = Color.FromArgb ("#FFF8F0"),
			settingsFieldBackColor = Color.FromArgb ("#FFE8D0"),

			solutionLockedBackColor = Color.FromArgb ("#F0F0F0"),

			aboutMasterBackColor = Color.FromArgb ("#F0FFF0"),
			aboutFieldBackColor = Color.FromArgb ("#D0FFD0");

		private GMJPictureColorsSet pColorsSet = new GMJPictureColorsSet ();

		#endregion

		#region Переменные страниц

		private ContentPage settingsPage, aboutPage, logPage;

		private Label aboutLabel, fontSizeFieldLabel, groupSizeFieldLabel, aboutFontSizeField;

		private Switch newsAtTheEndSwitch, keepScreenOnSwitch, enableCopySubscriptionSwitch;

		private Button centerButton, scrollUpButton, scrollDownButton, menuButton, addButton,
			pictureBackButton, pTextOnTheLeftButton, censorshipButton, logColorButton,
			pSubsButton, logFontFamilyButton;

		private ListView mainLog;

		private List<string> pageVariants = new List<string> ();
		private List<string> pictureBKVariants = new List<string> ();
		private List<string> pictureBKSelectionVariants = new List<string> ();
		private List<string> pictureTAVariants = new List<string> ();
		private List<string> pictureTASelectionVariants = new List<string> ();
		private List<string> censorshipVariants = new List<string> ();
		private List<string> logColorVariants = new List<string> ();
		private List<string> logFontFamilyVariants = new List<string> ();

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

			// Запрет спящего режима
			AndroidSupport.ApplyLabelSettings (settingsPage, "KeepScreenOnLabel",
				"Запрет спящего режима", RDLabelTypes.DefaultLeft);
			keepScreenOnSwitch = AndroidSupport.ApplySwitchSettings (settingsPage, "KeepScreenOnSwitch",
				false, settingsFieldBackColor, KeepScreenOnSwitch_Toggled, NotificationsSupport.KeepScreenOn);
			AndroidSupport.ApplyLabelSettings (settingsPage, "KeepScreenOnTip",
				"Опция запрещает переход устройства в спящий режим, пока приложение открыто, " +
				"позволяя экрану оставаться активным, пока Вы читаете тексты записей",
				RDLabelTypes.TipLeft);

			// Ссылка на оригинал
			Label eps1 = AndroidSupport.ApplyLabelSettings (settingsPage, "EnablePostSubscriptionLabel",
				"Ссылка на оригинал", RDLabelTypes.DefaultLeft);
			enableCopySubscriptionSwitch = AndroidSupport.ApplySwitchSettings (settingsPage,
				"EnablePostSubscriptionSwitch", false, settingsFieldBackColor,
				EnablePostSubscription_Toggled, GMJ.EnableCopySubscription);
			Label eps2 = AndroidSupport.ApplyLabelSettings (settingsPage, "EnablePostSubscriptionTip",
				"Опция обеспечивает добавление ссылки на оригинал записи к тексту при выполнении действий " +
				"«Скопировать» и «Поделиться»", RDLabelTypes.TipLeft);

			if (AndroidSupport.IsTV)
				{
				GMJ.EnableCopySubscription = false;
				eps1.IsVisible = eps2.IsVisible = enableCopySubscriptionSwitch.IsVisible = false;
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

			AndroidSupport.ApplyButtonSettings (aboutPage, "StatsButton",
				GMJ.GMJStatsMenuItem,
				aboutFieldBackColor, StatsButton_Click, false);

			AndroidSupport.ApplyLabelSettings (aboutPage, "GenericSettingsLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_GenericSettings),
				RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (aboutPage, "RestartTipLabel",
				RDLocale.GetDefaultText (RDLDefaultTexts.Message_RestartRequired),
				RDLabelTypes.TipCenter);

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

			centerButton = AndroidSupport.ApplyButtonSettings (logPage, "CenterButton", " ",
				logFieldBackColor, CenterButton_Click, false);
			centerButton.FontSize += 6;

			scrollUpButton = AndroidSupport.ApplyButtonSettings (logPage, "ScrollUp",
				RDDefaultButtons.Up, logFieldBackColor, ScrollUpButton_Click);
			scrollDownButton = AndroidSupport.ApplyButtonSettings (logPage, "ScrollDown",
				RDDefaultButtons.Down, logFieldBackColor, ScrollDownButton_Click);
			centerButton.HeightRequest = centerButton.MaximumHeightRequest = scrollDownButton.HeightRequest;

			// Главный журнал
			AndroidSupport.ApplyLabelSettings (settingsPage, "LogSettingsLabel",
				"Журнал", RDLabelTypes.HeaderLeft);

			// Расположение новых записей в конце журнала
			Label nates1 = AndroidSupport.ApplyLabelSettings (settingsPage, "NewsAtTheEndLabel",
				"Новые записи – снизу", RDLabelTypes.DefaultLeft);
			newsAtTheEndSwitch = AndroidSupport.ApplySwitchSettings (settingsPage, "NewsAtTheEndSwitch",
				false, settingsFieldBackColor, NewsAtTheEndSwitch_Toggled, NotificationsSupport.LogNewsItemsAtTheEnd);
			Label nates2 = AndroidSupport.ApplyLabelSettings (settingsPage, "NewsAtTheEndTip",
				"Опция позволяет добавлять новые записи в конец журнала (снизу). Если выключена, " +
				"записи добавляются в начало журнала (сверху)", RDLabelTypes.TipLeft);

			if (AndroidSupport.IsTV)
				{
				nates1.IsVisible = nates2.IsVisible = newsAtTheEndSwitch.IsVisible = false;
				if (!NotificationsSupport.LogNewsItemsAtTheEnd)
					NotificationsSupport.LogNewsItemsAtTheEnd = true;
				}

			// Цвет фона журнала
			AndroidSupport.ApplyLabelSettings (settingsPage, "LogColorLabel",
				"Цветовая тема:", RDLabelTypes.DefaultLeft);
			logColorButton = AndroidSupport.ApplyButtonSettings (settingsPage, "LogColorButton",
				" ", settingsFieldBackColor, LogColor_Clicked, false);
			AndroidSupport.ApplyLabelSettings (settingsPage, "LogColorTip",
				"Опция задаёт цвета фона и текста в журнале приложения",
				RDLabelTypes.TipLeft);

			// Кнопки меню и предложения в журнале
			menuButton = AndroidSupport.ApplyButtonSettings (logPage, "MenuButton",
				RDDefaultButtons.Menu, logFieldBackColor, SelectPage);
			addButton = AndroidSupport.ApplyButtonSettings (logPage, "AddButton",
				RDDefaultButtons.Increase, logFieldBackColor, OfferTheRecord);
			addButton.IsVisible = !AndroidSupport.IsTV;

			LogColor_Clicked (null, null);

			// Размер шрифта журнала
			fontSizeFieldLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "FontSizeFieldLabel",
				"", RDLabelTypes.DefaultLeft);
			fontSizeFieldLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (settingsPage, "FontSizeIncButton",
				RDDefaultButtons.Increase, settingsFieldBackColor, FontSizeChanged);
			AndroidSupport.ApplyButtonSettings (settingsPage, "FontSizeDecButton",
				RDDefaultButtons.Decrease, settingsFieldBackColor, FontSizeChanged);

			AndroidSupport.ApplyLabelSettings (settingsPage, "FontSizeFieldTip",
				"Настройка задаёт кегль (размер) шрифта текста в журнале приложения",
				RDLabelTypes.TipLeft);

			FontSizeChanged (null, null);

			// Размер группы запрашиваемых записей
			groupSizeFieldLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "GroupSizeFieldLabel",
				"", RDLabelTypes.DefaultLeft);
			groupSizeFieldLabel.TextType = TextType.Html;

			AndroidSupport.ApplyButtonSettings (settingsPage, "GroupSizeIncButton",
				RDDefaultButtons.Increase, settingsFieldBackColor, GroupSizeChanged);
			AndroidSupport.ApplyButtonSettings (settingsPage, "GroupSizeDecButton",
				RDDefaultButtons.Decrease, settingsFieldBackColor, GroupSizeChanged);

			AndroidSupport.ApplyLabelSettings (settingsPage, "GroupSizeFieldTip",
				"Настройка задаёт количество записей, запрашиваемых подряд одним нажатием кнопки",
				RDLabelTypes.TipLeft);

			GroupSizeChanged (null, null);

			// Цензурирование
			AndroidSupport.ApplyLabelSettings (settingsPage, "CensorshipLabel",
				"Цензурирование:", RDLabelTypes.DefaultLeft);
			censorshipButton = AndroidSupport.ApplyButtonSettings (settingsPage, "CensorshipButton",
				" ", settingsFieldBackColor, Censorship_Clicked, false);
			AndroidSupport.ApplyLabelSettings (settingsPage, "CensorshipTip",
				"Опция указывает, будут ли отображаться записи, потенциально неприемлемые " +
				"для лиц младше 18 лет (содержащие ругательства, интимный подтекст и прочее)",
				RDLabelTypes.TipLeft);

			Censorship_Clicked (null, null);

			// Шрифт журнала
			AndroidSupport.ApplyLabelSettings (settingsPage, "LogFontFamilyLabel",
				"Шрифт:", RDLabelTypes.DefaultLeft);
			logFontFamilyButton = AndroidSupport.ApplyButtonSettings (settingsPage, "LogFontFamilyButton",
				" ", settingsFieldBackColor, LogFontFamily_Clicked, false);
			AndroidSupport.ApplyLabelSettings (settingsPage, "LogFontFamilyTip",
				"Опция задаёт шрифт текста в журнале: " +
				"Roboto – без засечек (несколько яркостей), " +
				"Condensed – без засечек узкий (несколько яркостей), " +
				"Noto – с засечками, " +
				"Droid Sans – без засечек моноширинный",
				RDLabelTypes.TipLeft);

			LogFontFamily_Clicked (null, null);

			// Настройки картинок
			Label pictLabel = AndroidSupport.ApplyLabelSettings (settingsPage, "PicturesLabel",
				"Сохраняемые картинки", RDLabelTypes.HeaderLeft);

			// Фон картинок
			Label pictBackLabel1 = AndroidSupport.ApplyLabelSettings (settingsPage, "PicturesBackLabel",
				"Фон:", RDLabelTypes.DefaultLeft);
			pictureBackButton = AndroidSupport.ApplyButtonSettings (settingsPage, "PicturesBackButton",
				" ", settingsFieldBackColor, PictureBack_Clicked, false);
			Label pictBackLabel2 = AndroidSupport.ApplyLabelSettings (settingsPage, "PicturesBackTip",
				"Опция задаёт цвет фона и контрастный оттенок текста для картинок либо указывает " +
				"вариант их интерактивного выбора", RDLabelTypes.TipLeft);

			// Выравнивание текста
			Label pictTextLabel1 = AndroidSupport.ApplyLabelSettings (settingsPage, "PTextLeftLabel",
				"Выравнивание:", RDLabelTypes.DefaultLeft);
			pTextOnTheLeftButton = AndroidSupport.ApplyButtonSettings (settingsPage, "PTextLeftButton",
				" ", settingsFieldBackColor, PTextOnTheLeft_Toggled, false);
			Label pictTextLabel2 = AndroidSupport.ApplyLabelSettings (settingsPage, "PTextLeftTip",
				"Опция задаёт одинаковое выравнивание текста на картинах либо ставит его в зависимость " +
				"от содержимого записи или выбора пользователя", RDLabelTypes.TipLeft);

			// Подпись картинок
			Label pictSubsLabel1 = AndroidSupport.ApplyLabelSettings (settingsPage, "PSubsLabel",
				"Подпись:", RDLabelTypes.DefaultLeft);
			pSubsButton = AndroidSupport.ApplyButtonSettings (settingsPage, "PSubsButton",
				" ", settingsFieldBackColor, PSubs_Clicked, false);
			Label pictSubsLabel2 = AndroidSupport.ApplyLabelSettings (settingsPage, "PSubsTip",
				"Опция задаёт дополнительную текстовую подпись для картинок, создаваемых на этом устройстве. " +
				"Используется в качестве Вашего индивидуального приветствия. Если не заполнена, не добавляется",
				RDLabelTypes.TipLeft);

			if (AndroidSupport.IsTV)
				{
				pictLabel.IsVisible =
					pictBackLabel1.IsVisible = pictBackLabel2.IsVisible = pictureBackButton.IsVisible =
					pictTextLabel1.IsVisible = pictTextLabel2.IsVisible = pTextOnTheLeftButton.IsVisible =
					pictSubsLabel1.IsVisible = pictSubsLabel2.IsVisible = pSubsButton.IsVisible = false;
				}
			else
				{
				PictureBack_Clicked (null, null);
				PTextOnTheLeft_Toggled (null, null);
				PSubs_Clicked (null, null);
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
			await ScrollMainLog (autoScrollMode);
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

			// Настройка и запуск
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
					if (GMJ.EnableCopySubscription)
						msg += ("." + RDLocale.RNRN +
							"Обратите внимание, что приложение добавляет к текстам, которыми Вы делитесь, " +
							"ссылку на сообщество Grammar must joy");
					break;

				case NSTipTypes.ShareImageButton:
					msg = "Эта опция позволяет поделиться записью в виде картинки";
					break;

				case NSTipTypes.MainLogClickMenuTip:
					msg = "Все операции с текстами записей доступны по клику на них в журнале приложения";
					break;

				case NSTipTypes.PostSubscriptions:
					msg = "Мы не имеем ничего против отключения подписей у текстов. " +
						"Честно. Всё-таки юмор – это общественное достояние, не допускающее каких-либо ограничений." +
						RDLocale.RNRN + "Однако мы будем Вам весьма признательны, если Вы упомянете нас в качестве " +
						"источника. Спасибо!";
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
			UpdateLog (-1);
			}

		private void UpdateLog (int ScrollPosition)
			{
			tvScrollPosition = ScrollPosition;
			mainLog.ItemsSource = null;
			mainLog.ItemsSource = masterLog;
			}

		// Промотка журнала к нужной позиции
		private async void MainLog_ItemAppearing (object sender, ItemVisibilityEventArgs e)
			{
			if (tvScrollPosition >= 0)
				await ScrollMainLog (tvScrollPosition);
			else
				await ScrollMainLog (NotificationsSupport.LogNewsItemsAtTheEnd ? masterLog.Count - 1 : 0);
			}

		private async Task<bool> ScrollMainLog (int VisibleItem)
			{
			// Контроль
			if (masterLog == null)
				return false;

			if ((masterLog.Count < 1) || !needsScroll)
				return false;

			// Искусственная задержка
			await Task.Delay (100);

			// Промотка с повторением до достижения нужного участка
			if (VisibleItem <= autoScrollMode)
				needsScroll = false;

			// Определение варианта промотки
			bool toTheEnd = NotificationsSupport.LogNewsItemsAtTheEnd;
			if (VisibleItem > manualScrollModeUp)
				{
				currentScrollItem = VisibleItem;
				if (currentScrollItem < 0)
					currentScrollItem = toTheEnd ? (masterLog.Count - 1) : 0;
				if (currentScrollItem > masterLog.Count - 1)
					currentScrollItem = masterLog.Count - 1;
				}
			else if (VisibleItem > manualScrollModeDown)
				{
				if (currentScrollItem > 0)
					currentScrollItem--;
				}
			else
				{
				if (currentScrollItem < (masterLog.Count - 1))
					currentScrollItem++;
				}

			if (toTheEnd)
				{
				if (VisibleItem > masterLog.Count - 3)
					needsScroll = false;
				}
			else
				{
				if (VisibleItem < 2)
					needsScroll = false;
				}

			try
				{
				mainLog.ScrollTo (masterLog[currentScrollItem], ScrollToPosition.MakeVisible,
					VisibleItem <= manualScrollModeUp);
				}
			catch { }
			return true;
			}

		// Обновление формы кнопки журнала
		private void UpdateLogButton (bool Requesting, bool FinishingBackgroundRequest)
			{
			bool red = Requesting && FinishingBackgroundRequest;
			bool yellow = Requesting && !FinishingBackgroundRequest;
			bool green = !Requesting && !FinishingBackgroundRequest;
			bool dark = !NotificationsSupport.LogColors.CurrentColor.IsBright;

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
					"▒\tПоделиться картинкой",
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
						RDLocale.RNRN + notItem.Separator.Replace (RDLocale.RN, "") +
						(GMJ.EnableCopySubscription ? (RDLocale.RNRN + notLink) : "")).Replace ("\r", ""),
						ProgramDescription.AssemblyVisibleName);
					break;

				// Скопировать в буфер обмена
				case 11:
				case 22:
				case 33:
					RDGenerics.SendToClipboard ((notItem.Header + RDLocale.RNRN + notItem.Text +
						RDLocale.RNRN + notItem.Separator.Replace (RDLocale.RN, "") +
						(GMJ.EnableCopySubscription ? (RDLocale.RNRN + notLink) : "")).Replace ("\r", ""),
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

					int pbk;
					switch (NotificationsSupport.PicturesBackgroundType)
						{
						case NotificationsSupport.PicturesBackgroundAsk:
							int res = await AndroidSupport.ShowList ("Использовать фон:",
								RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pictureBKSelectionVariants);
							if (res < 0)
								return;

							pbk = res;
							break;

						case NotificationsSupport.PicturesBackgroundRandom:
							pbk = RDGenerics.RND.Next (pColorsSet.ColorNames.Length);
							break;

						default:
							pbk = NotificationsSupport.PicturesBackgroundType;
							break;
						}

					var pict = GMJPicture.CreateGMJPicture (notItem.Header, notItem.Text,
						notItem.SeparatorIsSign ? notItem.Separator.Replace (RDLocale.RN, "") : "",
						pta, pColorsSet.GetColor ((uint)pbk));
					if (pict == null)
						{
						AndroidSupport.ShowBalloon ("Текст записи не позволяет сформировать картинку", true);
						return;
						}

					await GMJPicture.SaveGMJPictureToFile (pict, notItem.Header);
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
			menuButton.IsVisible = scrollDownButton.IsVisible = scrollUpButton.IsVisible = State;
			addButton.IsVisible = State && !AndroidSupport.IsTV;

			// Обновление статуса
			UpdateLogButton (!State, false);
			}

		// Добавление текста в журнал
		private void AddTextToLog (string Text)
			{
			if (NotificationsSupport.LogNewsItemsAtTheEnd)
				{
				masterLog.Add (new MainLogItem (Text));

				// Удаление верхних строк
				while (masterLog.Count > NotificationsSupport.MasterLogMaxItems)
					masterLog.RemoveAt (0);
				}
			else
				{
				masterLog.Insert (0, new MainLogItem (Text));

				// Удаление нижних строк (здесь требуется, т.к. не выполняется обрезка свойством .MainLog)
				while (masterLog.Count > NotificationsSupport.MasterLogMaxItems)
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
					// Разбиение на экраны
					if (AndroidSupport.IsTV)
						{
						int left;
						const int linesLimit = 9;

						// NotificationsSupport.LogNewsItemsAtTheEnd is true
						int scrollTo = masterLog.Count;

						int charsLimit = 60;
						if (NotificationsSupport.LogFontSize > 20)
							charsLimit -= 2 * (int)(NotificationsSupport.LogFontSize - 20);
						if (charsLimit < 30)
							charsLimit = 30;

						do
							{
							// Поиск ближайшего подходящего абзаца
							left = -RDLocale.RN.Length;
							for (int l = 0; l < linesLimit; l++)
								{
								left = newText.IndexOf (RDLocale.RN, left + RDLocale.RN.Length);
								if ((left < 0) || (left > charsLimit * linesLimit))
									break;
								}

							// Отделение
							if (left < 0)
								{
								AddTextToLog (newText);
								}
							else
								{
								AddTextToLog (newText.Substring (0, left));
								newText = NotificationsSupport.HeaderBeginning + "(продолжение)" +
									NotificationsSupport.HeaderEnding + MainLogItem.MainLogItemSplitter +
									newText.Substring (left + RDLocale.RN.Length);
								}

							// При достижении конца текста немедленное завершение
							if (left < 0)
								break;
							// (left > 0) обеспечивает обработку последнего фрагмента текста

							// NotificationsSupport.LogNewsItemsAtTheEnd is true
							scrollTo--;
							}
						while ((left > 0) || ((newText.Length - newText.Replace ("\n", "").Length > linesLimit) ||
							(newText.Length > charsLimit * linesLimit)));

						needsScroll = true;

						scrollTo--; // Пока не понятно, почему -1
						if (scrollTo < 0)
							scrollTo = 0;
						UpdateLog (scrollTo);
						}

					// Прямое направление
					else
						{
						AddTextToLog (newText);
						needsScroll = true;
						UpdateLog ();
						}

					// Завершено
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
			await ScrollMainLog (manualScrollModeUp);
			}

		private async void ScrollDownButton_Click (object sender, EventArgs e)
			{
			needsScroll = true;
			await ScrollMainLog (manualScrollModeDown);
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
		private void KeepScreenOnSwitch_Toggled (object sender, ToggledEventArgs e)
			{
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
			fontSizeFieldLabel.Text = string.Format ("Размер шрифта: <b>{0:D}</b>", fontSize.ToString ());

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
			groupSizeFieldLabel.Text = string.Format ("Длина серии: <b>{0:D}</b>", groupSize.ToString ());
			}

		// Включение / выключение подписи
		private async void EnablePostSubscription_Toggled (object sender, ToggledEventArgs e)
			{
			// Подсказки
			if (!NotificationsSupport.TipsState.HasFlag (NSTipTypes.PostSubscriptions))
				await ShowTips (NSTipTypes.PostSubscriptions);

			GMJ.EnableCopySubscription = enableCopySubscriptionSwitch.IsToggled;
			}

		// Выбор фона картинок
		private async void PictureBack_Clicked (object sender, EventArgs e)
			{
			// Запрос варианта
			if (pictureBKVariants.Count < 1)
				{
				pictureBKVariants.Add ("(спрашивать)");
				pictureBKVariants.Add ("(случайный)");
				pictureBKVariants.AddRange (pColorsSet.ColorNames);
				pictureBKSelectionVariants.AddRange (pColorsSet.ColorNames);
				}

			int res;
			if (sender == null)
				{
				res = NotificationsSupport.PicturesBackgroundType - NotificationsSupport.PicturesBackgroundAsk;
				}
			else
				{
				res = await AndroidSupport.ShowList ("Фон картинок",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), pictureBKVariants);
				if (res < 0)
					return;

				NotificationsSupport.PicturesBackgroundType = res + NotificationsSupport.PicturesBackgroundAsk;
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

		// Ввод подписи изображения
		private async void PSubs_Clicked (object sender, EventArgs e)
			{
			// Ввод подписи
			string sub;
			if (sender == null)
				{
				sub = NotificationsSupport.PicturesSubscription;
				}
			else
				{
				sub = await AndroidSupport.ShowInput ("Подпись картинок",
					"Введите подпись, которая будет добавляться на сохраняемые картинки",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Save),
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel),
					20, Keyboard.Text, NotificationsSupport.PicturesSubscription);

				// Если действие не было отменено
				if (sub == null)
					return;

				sub = sub.Replace ("\n", "").Replace ("\r", "").Replace ("\t", "");
				NotificationsSupport.PicturesSubscription = sub;
				}

			// Обработка и сохранение
			pSubsButton.Text = string.IsNullOrWhiteSpace (sub) ? "(нет)" : sub;
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
			string msg = (res > 0) ? GMJ.CensorshipEnableMessage2 : GMJ.CensorshipDisableMessage2;
			bool doReset = false;
			if (await AndroidSupport.ShowMessage (msg, RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel)))
				{
				GMJ.EnableCensorship = (res > 0);
				censorshipButton.Text = censorshipVariants[res];
				doReset = true;
				}

			msg = (res > 0) ? GMJ.CensorshipEnableResetMessage : GMJ.CensorshipDisableResetMessage;
			if (doReset && await AndroidSupport.ShowMessage (msg, RDLocale.GetDefaultText (RDLDefaultTexts.Button_Yes),
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_No)))
				{
				GMJ.ResetFreeSet ();
				}
			}

		// Выбор фона картинок
		private async void LogColor_Clicked (object sender, EventArgs e)
			{
			// Запрос варианта
			if (logColorVariants.Count < 1)
				logColorVariants.AddRange (NotificationsSupport.LogColors.ColorNames);

			int res;
			if (sender == null)
				{
				res = (int)NotificationsSupport.LogColor;
				}
			else
				{
				res = await AndroidSupport.ShowList ("Цветовая тема журнала",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), logColorVariants);
				if (res < 0)
					return;

				NotificationsSupport.LogColor = (uint)res;
				}

			// Установка настроек
			GMJLogColor currentLogColor = NotificationsSupport.LogColors.CurrentColor;
			logColorButton.Text = logColorVariants[res];

			logPage.BackgroundColor = mainLog.BackgroundColor = centerButton.BackgroundColor =
				scrollUpButton.BackgroundColor = scrollDownButton.BackgroundColor =
				menuButton.BackgroundColor = addButton.BackgroundColor = currentLogColor.BackColor;
			scrollUpButton.TextColor = scrollDownButton.TextColor = menuButton.TextColor =
				addButton.TextColor = currentLogColor.MainTextColor;

			NavigationPage np = (NavigationPage)MainPage;
			if (currentLogColor.IsBright)
				{
				np.BarBackgroundColor = currentLogColor.MainTextColor;
				np.BarTextColor = currentLogColor.BackColor;
				}
			else
				{
				np.BarBackgroundColor = currentLogColor.BackColor;
				np.BarTextColor = currentLogColor.MainTextColor;
				}

			// Принудительное обновление (только не при старте)
			if (sender != null)
				{
				needsScroll = true;
				UpdateLog ();
				}

			// Цепляет кнопку журнала
			UpdateLogButton (false, false);
			}

		// Изменение размера шрифта лога
		private async void LogFontFamily_Clicked (object sender, EventArgs e)
			{
			// Запрос варианта
			if (logFontFamilyVariants.Count < 1)
				{
				string[][] fonts = AndroidSupport.AvailableFonts;
				for (int i = 0; i < fonts.Length; i++)
					{
					logFontFamilyVariants.Add (fonts[i][0]);
					logFontFamilyVariants.Add (fonts[i][0] + " Italic");
					}
				}

			int res;
			if (sender == null)
				{
				res = (int)AndroidSupport.LogFontFamily;
				}
			else
				{
				res = await AndroidSupport.ShowList ("Шрифт журнала",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), logFontFamilyVariants);
				if (res < 0)
					return;

				AndroidSupport.LogFontFamily = (uint)res;
				}

			// Сохранение и отображение настройки в интерфейсе
			logFontFamilyButton.Text = logFontFamilyVariants[res];

			string ff;
			FontAttributes fa;
			AndroidSupport.GetCurrentFontFamily (out ff, out fa);

			logFontFamilyButton.FontAttributes = fa;
			logFontFamilyButton.FontFamily = ff;

			// Обновление журнала
			if (e != null)
				{
				needsScroll = true;
				UpdateLog ();
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

		// Отображение статистики архива
		private async void StatsButton_Click (object sender, EventArgs e)
			{
			await AndroidSupport.ShowMessage (GMJ.GMJStats, RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			}

		#endregion
		}
	}

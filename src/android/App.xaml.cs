using Microsoft.Maui.Controls;

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
		private List<List<string>> tapMenuItems = new List<List<string>> ();
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

		private Button centerButton, scrollUpButton, scrollDownButton, menuButton, addButton;

		private ListView mainLog;

		private List<string> pageVariants = new List<string> ();

		#endregion

		#region Запуск и настройка

		/// <summary>
		/// Конструктор. Точка входа приложения
		/// </summary>
		public App ()
			{
			// Инициализация
			InitializeComponent ();
			flags = AndroidSupport.GetAppStartupFlags (RDAppStartupFlags.Huawei | RDAppStartupFlags.CanReadFiles |
				RDAppStartupFlags.CanWriteFiles | RDAppStartupFlags.CanShowNotifications);

			if (!RDLocale.IsCurrentLanguageRuRu)
				RDLocale.CurrentLanguage = RDLanguages.ru_ru;

			// Общая конструкция страниц приложения
			MainPage = new MasterPage ();
			MasterPage.AppEx = this;

			settingsPage = AndroidSupport.ApplyPageSettings (new SettingsPage (), "SettingsPage",
				"Настройки приложения", settingsMasterBackColor);
			aboutPage = AndroidSupport.ApplyPageSettings (new AboutPage (), "AboutPage",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_AppAbout), aboutMasterBackColor);
			logPage = AndroidSupport.ApplyPageSettings (new LogPage (), "LogPage",
				"Журнал", logMasterBackColor);

			AndroidSupport.SetMasterPage (MainPage, logPage, logMasterBackColor);

			if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.PolicyTip))
				AndroidSupport.SetCurrentPage (settingsPage, settingsMasterBackColor);

			// Настройки просмотра
			AndroidSupport.ApplyLabelSettings (settingsPage, "AppSettingsLabel",
				"Просмотр", RDLabelTypes.HeaderLeft);

			AndroidSupport.ApplyLabelSettings (settingsPage, "KeepScreenOnLabel",
				"Запретить переход в спящий режим,\nпока приложение открыто", RDLabelTypes.DefaultLeft);
			keepScreenOnSwitch = AndroidSupport.ApplySwitchSettings (settingsPage, "KeepScreenOnSwitch",
				false, settingsFieldBackColor, KeepScreenOnSwitch_Toggled, NotificationsSupport.KeepScreenOn);

			AndroidSupport.ApplyLabelSettings (settingsPage, "EnablePostSubscriptionLabel",
				"Добавлять ссылку на оригинал\nзаписи к тексту при действиях\n«Скопировать» и «Поделиться»",
				RDLabelTypes.DefaultLeft);
			enablePostSubscriptionSwitch = AndroidSupport.ApplySwitchSettings (settingsPage,
				"EnablePostSubscriptionSwitch", false, settingsFieldBackColor,
				EnablePostSubscription_Toggled, GMJ.EnablePostSubscription);

			#region Страница "О программе"

			aboutLabel = AndroidSupport.ApplyLabelSettings (aboutPage, "AboutLabel",
				RDGenerics.AppAboutLabelText, RDLabelTypes.AppAbout);

			AndroidSupport.ApplyButtonSettings (aboutPage, "ManualsButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_ReferenceMaterials),
				aboutFieldBackColor, ReferenceButton_Click, false);
			AndroidSupport.ApplyButtonSettings (aboutPage, "HelpButton",
				RDLocale.GetDefaultText (RDLDefaultTexts.Control_HelpSupport),
				aboutFieldBackColor, HelpButton_Click, false);
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

			centerButton = AndroidSupport.ApplyButtonSettings (logPage, "CenterButton", " ",
				logFieldBackColor, CenterButton_Click, false);
			centerButton.FontSize += 6;

			scrollUpButton = AndroidSupport.ApplyButtonSettings (logPage, "ScrollUp",
				RDDefaultButtons.Up, logFieldBackColor, ScrollUpButton_Click);
			scrollDownButton = AndroidSupport.ApplyButtonSettings (logPage, "ScrollDown",
				RDDefaultButtons.Down, logFieldBackColor, ScrollDownButton_Click);

			// Режим чтения
			AndroidSupport.ApplyLabelSettings (settingsPage, "ReadModeLabel",
				"Тёмная тема для основного журнала", RDLabelTypes.DefaultLeft);
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
			if (!flags.HasFlag (RDAppStartupFlags.Huawei))
				await AndroidSupport.XPUNLoop ();

			// Требование принятия Политики
			if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.PolicyTip))
				{
				await AndroidSupport.PolicyLoop ();
				NotificationsSet.TipsState |= NSTipTypes.PolicyTip;
				}

			// Подсказки
			if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.StartupTips))
				{
				await AndroidSupport.ShowMessage ("Добро пожаловать в клиент Grammar must joy!" + RDLocale.RNRN +
					"• На этой странице Вы можете настроить поведение приложения." + RDLocale.RNRN +
					"• Используйте системную кнопку «Назад», чтобы вернуться к журналу записей " +
					"из любого раздела." + RDLocale.RNRN +
					"• Используйте кнопку с семафором для получения случайных записей из сообщества GMJ",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Next));

				await AndroidSupport.ShowMessage ("Внимание!" + RDLocale.RNRN +
					"• Некоторые устройства требуют ручного разрешения на доступ в интернет " +
					"(например, если активен режим экономии интернет-трафика). Проверьте его, " +
					"если запросы не будут работать правильно",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));

				NotificationsSet.TipsState |= NSTipTypes.StartupTips;
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
					msg = "Эта опция позволяет открыть источник выбранного уведомления в браузере";
					break;

				case NSTipTypes.ShareButton:
					msg = "Эти опция позволяет поделиться текстом уведомления";
					if (GMJ.EnablePostSubscription)
						msg += ("." + RDLocale.RNRN +
							"Обратите внимание, что приложение добавляет к текстам, которыми Вы делитесь, " +
							"ссылки на источники информации (в целях соблюдения прав авторов). Рекомендуется " +
							"не удалять их при распространении текстов с помощью этой функции");
					break;

				case NSTipTypes.MainLogClickMenuTip:
					msg = "Все операции с текстами уведомлений доступны по клику на них в главном журнале";
					break;

				case NSTipTypes.KeepScreenOnTip:
					msg = "Этот переключатель позволяет экрану оставаться активным, пока Вы читаете " +
						"тексты уведомлений (т. е. пока приложение открыто)";
					break;
				}

			await AndroidSupport.ShowMessage (msg, RDLocale.GetDefaultText (RDLDefaultTexts.Button_OK));
			NotificationsSet.TipsState |= Type;
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
			if (ToTheEnd)
				{
				if ((VisibleItem < 0) || (VisibleItem > masterLog.Count - 3))
					needsScroll = false;

				mainLog.ScrollTo (masterLog[masterLog.Count - 1], ScrollToPosition.MakeVisible, false);
				}
			else
				{
				if ((VisibleItem < 0) || (VisibleItem < 2))
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
			}

		// Выбор оповещения для перехода или share
		private async void MainLog_ItemTapped (object sender, ItemTappedEventArgs e)
			{
			// Контроль
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
			int variant = 0, menuItem;
			if (tapMenuItems.Count < 1)
				{
				tapMenuItems.Add (new List<string> {
					"☍\tПоделиться текстом",
					"❏\tСкопировать текст",
					"Ещё...",
					});
				tapMenuItems.Add (new List<string> {
					"▷\tПерейти к источнику",
					"☍\tПоделиться текстом",
					"❏\tСкопировать текст",
					"Ещё...",
					});
				tapMenuItems.Add (new List<string> {
					"✕\tУдалить из журнала",
					});
				}

			// Запрос варианта использования
			menuItem = (string.IsNullOrWhiteSpace (notLink) ? 0 : 1);
			menuItem = await AndroidSupport.ShowList ("Выберите действие:",
				RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel),
				tapMenuItems[menuItem]);

			if (menuItem < 0)
				return;

			variant = menuItem + 10;
			if (string.IsNullOrWhiteSpace (notLink))
				variant++;

			// Контроль второго набора
			if (variant > 12)
				{
				menuItem = await AndroidSupport.ShowList ("Выберите действие:",
					RDLocale.GetDefaultText (RDLDefaultTexts.Button_Cancel), tapMenuItems[2]);
				if (menuItem < 0)
					return;

				variant += menuItem;
				}

			// Обработка (неподходящие варианты будут отброшены)
			switch (variant)
				{
				// Переход по ссылке
				case 0:
				case 10:
					if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.GoToButton))
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
				case 1:
				case 11:
					if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.ShareButton))
						await ShowTips (NSTipTypes.ShareButton);

					await Share.RequestAsync ((notItem.Header + RDLocale.RNRN + notItem.Text +
						(GMJ.EnablePostSubscription ? (RDLocale.RNRN + notLink) : "")).Replace ("\r", ""),
						ProgramDescription.AssemblyVisibleName);
					break;

				// Скопировать в буфер обмена
				case 2:
				case 12:
					RDGenerics.SendToClipboard ((notItem.Header + RDLocale.RNRN + notItem.Text +
						(GMJ.EnablePostSubscription ? (RDLocale.RNRN + notLink) : "")).Replace ("\r", ""),
						true);
					break;

				// Удаление из журнала
				case 5:
				case 13:
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
			menuButton.IsVisible = addButton.IsVisible = State;

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
					}
				}

			// Разблокировка
			SetLogState (true);
			if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.MainLogClickMenuTip))
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
			if (!NotificationsSet.TipsState.HasFlag (NSTipTypes.KeepScreenOnTip))
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

		#endregion

		#region О приложении

		// Вызов справочных материалов
		private async void ReferenceButton_Click (object sender, EventArgs e)
			{
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

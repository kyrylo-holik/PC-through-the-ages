using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Менеджер интерфейса: показывает и скрывает экраны (панели), выводит тексты,
// переключает язык и управляет бонусным уровнем.
// ВАЖНО: названия полей с [SerializeField] нельзя менять — к ним привязаны объекты в сцене.
public class UI_Manager : MonoBehaviour
{
    // ------------------------------------------------------------------
    // Поля, которые заполняются в Inspector (перетаскиванием объектов из сцены)
    // ------------------------------------------------------------------

    [Header("Panels")]
    [SerializeField] private GameObject Main_Menu_Panel;               // главное меню
    [SerializeField] private GameObject Main_Intro_Panel;              // экран с описанием игры и кнопкой «Начать»
    [SerializeField] private GameObject Level_Intro_And_Theory_Panel;  // экран эпохи: описание до заданий и теория после них
    [SerializeField] private GameObject Gameplay_HUD;                  // экран с вопросом во время игры
    [SerializeField] private GameObject Pop_Up_Hint_Panel;             // всплывающая подсказка
    [SerializeField] private GameObject Bonus_Level_Panel;             // бонусный уровень
    [SerializeField] private GameObject Game_Completed_Panel;          // итоговый экран

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI Title_Text;         // заголовок эпохи
    [SerializeField] private TextMeshProUGUI Intro_Text;         // описание эпохи (или заголовок теории)
    [SerializeField] private TextMeshProUGUI Theory_Text;        // текст теории
    [SerializeField] private TextMeshProUGUI Question_Text;      // текст текущего вопроса
    [SerializeField] private TextMeshProUGUI Hint_Pop_Up_Text;   // текст всплывающей подсказки
    [SerializeField] private TextMeshProUGUI Feedback_Text;      // «Верно!» / «Не тот компонент»
    [SerializeField] private TextMeshProUGUI Score_Result_Text;  // итоги игры

    [Header("Buttons")]
    [SerializeField] private Button Bonus_Level_Button;          // кнопка «Бонус» в главном меню

    [Header("UI_Texts_For_Localization")]
    // Надписи на кнопках и экранах, которые меняются при смене языка.
    [SerializeField] private TextMeshProUGUI New_Game_Button_Text;
    [SerializeField] private TextMeshProUGUI Bonus_Button_Text;
    [SerializeField] private TextMeshProUGUI Language_Button_Text;
    [SerializeField] private TextMeshProUGUI Main_Intro_UI_Text;
    [SerializeField] private TextMeshProUGUI Start_Intro_Button_Text;
    [SerializeField] private TextMeshProUGUI Level_Start_Button_Text;
    [SerializeField] private TextMeshProUGUI Level_Close_Hint_Pop_Up_Text;
    [SerializeField] private TextMeshProUGUI Return_To_Menu_Button_Text;

    [Header("Bonus_Level_Elements")]
    [SerializeField] private GameObject[] Bonus_Brand_Buttons;   // кнопки Intel / AMD / Nvidia
    [SerializeField] private TextMeshProUGUI Bonus_Result_Text;  // текст о выбранном производителе
    [SerializeField] private TextMeshProUGUI Bonus_Title_Text;   // «Выберите производителя:»

    // ------------------------------------------------------------------
    // Внутренние переменные
    // ------------------------------------------------------------------

    private const float Feedback_Duration = 2f;      // сколько секунд висит «Верно!» / «Не тот компонент»
    private const float Bonus_Auto_Close_Delay = 60f; // через сколько секунд бонус сам вернётся в меню

    // Список всех панелей, чтобы скрывать их одной командой.
    private GameObject[] All_Panels;

    // Текущий язык (берётся из менеджера локализации).
    private Language Lang => Localization_Manager.Instance.Current_Language;

    // ------------------------------------------------------------------
    // Запуск
    // ------------------------------------------------------------------

    private void Awake()
    {
        All_Panels = new[]
        {
            Main_Menu_Panel, Main_Intro_Panel, Level_Intro_And_Theory_Panel, Gameplay_HUD,
            Pop_Up_Hint_Panel, Bonus_Level_Panel, Game_Completed_Panel
        };
    }

    private void Start()
    {
        Configure_Text_Layout();
        Update_All_UI_Texts();
        Show_Main_Menu();
    }

    // Включает автоподбор размера шрифта у длинных текстов и ставит текст бонуса в нижнюю часть экрана.
    // Текст стартового описания (Main_Intro_UI_Text) намеренно не трогаем: он настраивается в редакторе.
    private void Configure_Text_Layout()
    {
        Enable_Auto_Size(Intro_Text, 16f);
        Enable_Auto_Size(Theory_Text, 16f);
        Enable_Auto_Size(Bonus_Result_Text, 14f);

        if (Bonus_Result_Text == null)
            return;

        // Текст бонуса занимает нижние ~35% экрана, чтобы не закрывать 3D-модель.
        RectTransform Bonus_Rect = Bonus_Result_Text.rectTransform;
        Bonus_Rect.anchorMin = new Vector2(0.08f, 0.03f);
        Bonus_Rect.anchorMax = new Vector2(0.92f, 0.38f);
        Bonus_Rect.offsetMin = Vector2.zero;
        Bonus_Rect.offsetMax = Vector2.zero;
        Bonus_Result_Text.alignment = TextAlignmentOptions.Center;
    }

    // Если текст не помещается в своё поле, шрифт сам уменьшается (но не меньше Min_Size).
    // Максимум — тот размер шрифта, который задан в редакторе.
    private void Enable_Auto_Size(TextMeshProUGUI Text, float Min_Size)
    {
        if (Text == null)
            return;

        Text.fontSizeMax = Text.fontSize;
        Text.fontSizeMin = Min_Size;
        Text.enableAutoSizing = true;
    }

    // ------------------------------------------------------------------
    // Показ экранов
    // ------------------------------------------------------------------

    // Скрывает все панели. Вызывается перед показом любого нового экрана.
    private void Hide_All_Panels()
    {
        foreach (GameObject Panel in All_Panels)
            Panel.SetActive(false);
    }

    // Главное меню. Кнопка «Бонус» активна, только если бонус уже заработан.
    public void Show_Main_Menu()
    {
        Hide_All_Panels();
        Main_Menu_Panel.SetActive(true);

        Bonus_Level_Button.interactable = Save_Manager.Is_Bonus_Unlocked();
    }

    // Экран с описанием игры и кнопкой «Начать».
    public void Show_Main_Intro()
    {
        Hide_All_Panels();
        Main_Intro_Panel.SetActive(true);
    }

    // Экран эпохи. Сюда передаются три строки:
    //   до заданий — заголовок, описание эпохи и пустая теория;
    //   после заданий — заголовок, надпись «Теперь — теория» и сама теория.
    public void Show_Level_Intro_And_Theory(string Title, string Cutscene, string Theory)
    {
        Hide_All_Panels();
        Level_Intro_And_Theory_Panel.SetActive(true);

        Title_Text.text = Title;
        Intro_Text.text = Cutscene;
        Theory_Text.text = Theory;
    }

    // Нажатие кнопки «Продолжить» на экране эпохи.
    // Если сейчас показана теория — идём дальше (следующая эпоха или итоги).
    // Если это описание перед заданиями — открываем экран с вопросами.
    public void On_Start_Level_Button_Clicked()
    {
        if (Game_Controller.Instance.Is_Theory_Phase)
        {
            Game_Controller.Instance.Finish_Theory_Phase();
            return;
        }

        Hide_All_Panels();
        Gameplay_HUD.SetActive(true);
        Game_Controller.Instance.Begin_Questions_Phase();
    }

    // Итоговый экран: точность ответов и сообщение о бонусе.
    public void Show_Game_Completion_Screen(float Accuracy)
    {
        Hide_All_Panels();
        Game_Completed_Panel.SetActive(true);

        string Bonus_Text = Accuracy >= Save_Manager.Bonus_Accuracy_Percent
            ? Localization_DB.Game_Completion.Bonus_Unlocked.Get(Lang)
            : Localization_DB.Game_Completion.Bonus_Locked.Get(Lang);

        Score_Result_Text.text =
            $"{Localization_DB.Game_Completion.Finished.Get(Lang)}\n" +
            $"{Localization_DB.Game_Completion.Accuracy.Get(Lang)}: {Accuracy:F1}%\n" +
            Bonus_Text;
    }

    // Возврат в главное меню (также убирает 3D-модель бонуса, если она была).
    public void Return_To_Menu()
    {
        if (Game_Controller.Instance != null)
            Game_Controller.Instance.Clear_Bonus_Model();

        Show_Main_Menu();
    }

    // ------------------------------------------------------------------
    // Вопросы, подсказки и обратная связь
    // ------------------------------------------------------------------

    // Показывает текст текущего вопроса.
    public void Show_Question(string Question)
    {
        Question_Text.text = Question;
    }

    // Открывает всплывающую подсказку с заданным текстом.
    public void Show_Hint_Popup(string Hint)
    {
        Pop_Up_Hint_Panel.SetActive(true);
        Hint_Pop_Up_Text.text = Hint;
    }

    // Закрывает всплывающую подсказку (вызывается кнопкой «Закрыть» и после верного ответа).
    public void Close_Hint_Popup()
    {
        Pop_Up_Hint_Panel.SetActive(false);
    }

    // Показывает на пару секунд зелёное «Верно!» или красное сообщение об ошибке.
    public void Show_Feedback(bool Is_Correct, string Message)
    {
        Feedback_Text.gameObject.SetActive(true);
        Feedback_Text.color = Is_Correct ? Color.green : Color.red;
        Feedback_Text.text = Message;

        // Если сообщение уже висело, сбрасываем старый таймер и запускаем новый.
        CancelInvoke(nameof(Hide_Feedback));
        Invoke(nameof(Hide_Feedback), Feedback_Duration);
    }

    private void Hide_Feedback()
    {
        Feedback_Text.gameObject.SetActive(false);
    }

    // ------------------------------------------------------------------
    // Бонусный уровень
    // ------------------------------------------------------------------

    // Открывает бонусный экран: заголовок и три кнопки выбора производителя.
    public void Open_Bonus_Level()
    {
        // Убираем 3D-модель от предыдущего захода, если она осталась.
        if (Game_Controller.Instance != null)
            Game_Controller.Instance.Clear_Bonus_Model();

        Hide_All_Panels();
        Bonus_Level_Panel.SetActive(true);

        Bonus_Title_Text.text = Localization_DB.Bonus.Title.Get(Lang);
        Bonus_Title_Text.gameObject.SetActive(true);

        foreach (GameObject Button in Bonus_Brand_Buttons)
            Button.SetActive(true);

        Bonus_Result_Text.text = string.Empty;
        Bonus_Result_Text.gameObject.SetActive(false);
    }

    // Игрок выбрал производителя ("Intel", "AMD" или "Nvidia"):
    // прячем кнопки, показываем подробный текст и 3D-модель.
    public void On_Bonus_Choice(string Vendor_Name)
    {
        string Comment = Vendor_Name switch
        {
            "Intel" => Localization_DB.Bonus.Intel.Get(Lang),
            "AMD" => Localization_DB.Bonus.AMD.Get(Lang),
            "Nvidia" => Localization_DB.Bonus.Nvidia.Get(Lang),
            _ => string.Empty
        };

        Bonus_Title_Text.gameObject.SetActive(false);

        foreach (GameObject Button in Bonus_Brand_Buttons)
            Button.SetActive(false);

        // К тексту добавляем мелкую подсказку, как вернуться в меню.
        Bonus_Result_Text.gameObject.SetActive(true);
        Bonus_Result_Text.text =
            Comment + "\n\n<size=65%><i>" + Localization_DB.Bonus.Return_Hint.Get(Lang) + "</i></size>";

        if (Game_Controller.Instance != null)
            Game_Controller.Instance.Show_Bonus_Model(Vendor_Name);

        // Запускаем ожидание возврата в меню (если оно уже шло — сначала останавливаем).
        StopAllCoroutines();
        StartCoroutine(Return_To_Menu_After_Bonus(Bonus_Auto_Close_Delay));
    }

    // Ждёт, пока пройдёт время или игрок нажмёт Esc / пробел, и возвращает в главное меню.
    private IEnumerator Return_To_Menu_After_Bonus(float Delay)
    {
        float Elapsed = 0f;

        while (Elapsed < Delay)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
                break;

            Elapsed += Time.deltaTime;
            yield return null;
        }

        Return_To_Menu();
    }

    // ------------------------------------------------------------------
    // Язык интерфейса
    // ------------------------------------------------------------------

    // Подставляет в кнопки и подписи тексты на текущем языке.
    public void Update_All_UI_Texts()
    {
        New_Game_Button_Text.text = Localization_DB.UI.New_Game.Get(Lang);
        Bonus_Button_Text.text = Localization_DB.UI.Bonus.Get(Lang);
        Language_Button_Text.text = Localization_DB.UI.Language.Get(Lang);
        Main_Intro_UI_Text.text = Localization_DB.UI.Main_Intro.Get(Lang);
        Start_Intro_Button_Text.text = Localization_DB.UI.Start_Intro.Get(Lang);
        Level_Start_Button_Text.text = Localization_DB.UI.Continue.Get(Lang);
        Level_Close_Hint_Pop_Up_Text.text = Localization_DB.UI.Close.Get(Lang);
        Return_To_Menu_Button_Text.text = Localization_DB.UI.Return_To_Menu.Get(Lang);
    }

    // Кнопка смены языка: русский <-> английский.
    public void Toggle_Language()
    {
        if (Lang == Language.RU)
            Set_Language_EN();
        else
            Set_Language_RU();
    }

    public void Set_Language_RU()
    {
        Localization_Manager.Instance.Set_Language(Language.RU);
        Update_All_UI_Texts();
    }

    public void Set_Language_EN()
    {
        Localization_Manager.Instance.Set_Language(Language.EN);
        Update_All_UI_Texts();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject Main_Menu_Panel;
    [SerializeField] private GameObject Main_Intro_Panel;
    [SerializeField] private GameObject Level_Intro_And_Theory_Panel;
    [SerializeField] private GameObject Gameplay_HUD;
    [SerializeField] private GameObject Pop_Up_Hint_Panel;
    [SerializeField] private GameObject Bonus_Level_Panel;
    [SerializeField] private GameObject Game_Completed_Panel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI Title_Text;
    [SerializeField] private TextMeshProUGUI Intro_Text;
    [SerializeField] private TextMeshProUGUI Theory_Text;
    [SerializeField] private TextMeshProUGUI Question_Text;
    [SerializeField] private TextMeshProUGUI Hint_Pop_Up_Text;
    [SerializeField] private TextMeshProUGUI Feedback_Text;
    [SerializeField] private TextMeshProUGUI Score_Result_Text;

    [Header("Buttons")]
    [SerializeField] private Button Bonus_Level_Button;

    [Header("UI_Texts_For_Localization")]
    [SerializeField] private TextMeshProUGUI New_Game_Button_Text;
    [SerializeField] private TextMeshProUGUI Bonus_Button_Text;
    [SerializeField] private TextMeshProUGUI Language_Button_Text;
    [SerializeField] private TextMeshProUGUI Main_Intro_UI_Text;
    [SerializeField] private TextMeshProUGUI Start_Intro_Button_Text;
    [SerializeField] private TextMeshProUGUI Level_Start_Button_Text;
    [SerializeField] private TextMeshProUGUI Level_Close_Hint_Pop_Up_Text;
    [SerializeField] private TextMeshProUGUI Return_To_Menu_Button_Text;

    [Header("Bonus_Level_Elements")]
    [SerializeField] private GameObject[] Bonus_Brand_Buttons;
    [SerializeField] private TextMeshProUGUI Bonus_Result_Text;
    [SerializeField] private TextMeshProUGUI Bonus_Title_Text;

    private void Start()
    {
        Configure_Text_Layout();
        Update_All_UI_Texts();
        Show_Main_Menu();
    }

    public void Show_Main_Menu()
    {
        Hide_All_Panels();
        Main_Menu_Panel.SetActive(true);

        Bonus_Level_Button.interactable = Save_Manager.Is_Bonus_Unlocked();
    }

    public void Show_Main_Intro()
    {
        Hide_All_Panels();
        Main_Intro_Panel.SetActive(true);
    }

    public void Show_Level_Intro_And_Theory(string Title, string Cutscene, string Theory)
    {
        Hide_All_Panels();
        Level_Intro_And_Theory_Panel.SetActive(true);

        Title_Text.text = Title;
        Intro_Text.text = Cutscene;
        Theory_Text.text = Theory;
    }

    // Тексты стали длиннее: включаем автоподбор размера шрифта и растягиваем поле стартового описания
    private void Configure_Text_Layout()
    {
        Enable_Auto_Size(Intro_Text, 16f);
        Enable_Auto_Size(Theory_Text, 16f);
        Enable_Auto_Size(Bonus_Result_Text, 14f);

        // Текст бонуса - в нижней части экрана, чтобы не закрывать 3D-модель
        if (Bonus_Result_Text != null)
        {
            RectTransform Bonus_Rect = Bonus_Result_Text.rectTransform;
            Bonus_Rect.anchorMin = new Vector2(0.08f, 0.03f);
            Bonus_Rect.anchorMax = new Vector2(0.92f, 0.38f);
            Bonus_Rect.offsetMin = Vector2.zero;
            Bonus_Rect.offsetMax = Vector2.zero;
            Bonus_Result_Text.alignment = TextAlignmentOptions.Center;
        }

    }

    private void Enable_Auto_Size(TextMeshProUGUI Text, float Min_Size)
    {
        if (Text == null)
            return;

        Text.fontSizeMax = Text.fontSize;
        Text.fontSizeMin = Min_Size;
        Text.enableAutoSizing = true;
    }

    // Одна и та же кнопка «Продолжить»: до заданий открывает вопросы, после заданий - идёт дальше от теории
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

    public void Show_Question(string Question)
    {
        Question_Text.text = Question;
    }

    public void Show_Hint_Popup(string Hint)
    {
        Pop_Up_Hint_Panel.SetActive(true);
        Hint_Pop_Up_Text.text = Hint;
    }

    public void Close_Hint_Popup()
    {
        Pop_Up_Hint_Panel.SetActive(false);
    }

    public void Show_Feedback(bool Is_Correct, string Message)
    {
        Feedback_Text.gameObject.SetActive(true);
        Feedback_Text.color = Is_Correct ? Color.green : Color.red;
        Feedback_Text.text = Message;

        CancelInvoke(nameof(Hide_Feedback));
        Invoke(nameof(Hide_Feedback), 2f);
    }

    private void Hide_Feedback()
    {
        Feedback_Text.gameObject.SetActive(false);
    }

    public void Show_Game_Completion_Screen(float Accuracy)
    {
        Hide_All_Panels();
        Game_Completed_Panel.SetActive(true);

        Language Lang = Localization_Manager.Instance.Current_Language;

        string Finished_Text =
            Localization_DB.Game_Completion.Finished.Get(Lang);

        string Accuracy_Text =
            Localization_DB.Game_Completion.Accuracy.Get(Lang);

        string Bonus_Text =
            Accuracy >= 80f
                ? Localization_DB.Game_Completion.Bonus_Unlocked.Get(Lang)
                : Localization_DB.Game_Completion.Bonus_Locked.Get(Lang);

        Score_Result_Text.text =
            $"{Finished_Text}\n{Accuracy_Text}: {Accuracy:F1}%\n{Bonus_Text}";
    }

    public void Open_Bonus_Level()
    {
        if (Game_Controller.Instance != null)
            Game_Controller.Instance.Clear_Bonus_Model();

        Hide_All_Panels();
        Bonus_Level_Panel.SetActive(true);

        Language Lang = Localization_Manager.Instance.Current_Language;

        Bonus_Title_Text.text = Localization_DB.Bonus.Title.Get(Lang);
        Bonus_Title_Text.gameObject.SetActive(true);

        foreach (GameObject Button in Bonus_Brand_Buttons)
            Button.SetActive(true);

        Bonus_Result_Text.text = string.Empty;
        Bonus_Result_Text.gameObject.SetActive(false);
    }

    public void On_Bonus_Choice(string Vendor_Name)
    {
        Language Lang = Localization_Manager.Instance.Current_Language;

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

        Bonus_Result_Text.gameObject.SetActive(true);
        Bonus_Result_Text.text =
            Comment + "\n\n<size=65%><i>" + Localization_DB.Bonus.Return_Hint.Get(Lang) + "</i></size>";

        if (Game_Controller.Instance != null)
            Game_Controller.Instance.Show_Bonus_Model(Vendor_Name);

        StopAllCoroutines();
        StartCoroutine(Return_To_Menu_After_Bonus_Delay(60f));
    }

    private System.Collections.IEnumerator Return_To_Menu_After_Bonus_Delay(float Delay)
    {
        // Ждём либо окончания таймера, либо нажатия Esc / пробела
        float Elapsed = 0f;

        while (Elapsed < Delay)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space))
                break;

            Elapsed += Time.deltaTime;
            yield return null;
        }

        Bonus_Result_Text.text = string.Empty;
        Bonus_Result_Text.gameObject.SetActive(false);

        Bonus_Title_Text.gameObject.SetActive(true);

        foreach (GameObject Button in Bonus_Brand_Buttons)
            Button.SetActive(true);

        Return_To_Menu();
    }

    private void Hide_All_Panels()
    {
        Main_Menu_Panel.SetActive(false);
        Main_Intro_Panel.SetActive(false);
        Level_Intro_And_Theory_Panel.SetActive(false);
        Gameplay_HUD.SetActive(false);
        Pop_Up_Hint_Panel.SetActive(false);
        Bonus_Level_Panel.SetActive(false);
        Game_Completed_Panel.SetActive(false);
    }

    public void Update_All_UI_Texts()
    {
        Language Lang = Localization_Manager.Instance.Current_Language;

        New_Game_Button_Text.text = Localization_DB.UI.New_Game.Get(Lang);
        Bonus_Button_Text.text = Localization_DB.UI.Bonus.Get(Lang);
        Language_Button_Text.text = Localization_DB.UI.Language.Get(Lang);
        Main_Intro_UI_Text.text = Localization_DB.UI.Main_Intro.Get(Lang);
        Start_Intro_Button_Text.text = Localization_DB.UI.Start_Intro.Get(Lang);
        Level_Start_Button_Text.text = Localization_DB.UI.Continue.Get(Lang);
        Level_Close_Hint_Pop_Up_Text.text = Localization_DB.UI.Close.Get(Lang);
        Return_To_Menu_Button_Text.text = Localization_DB.UI.Return_To_Menu.Get(Lang);
    }

    public void Toggle_Language()
    {
        if (Localization_Manager.Instance.Current_Language == Language.RU)
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

    public void Return_To_Menu()
    {
        if (Game_Controller.Instance != null)
            Game_Controller.Instance.Clear_Bonus_Model();

        Hide_All_Panels();
        Show_Main_Menu();
    }
}
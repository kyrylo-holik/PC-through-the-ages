using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Менеджер пользовательского интерфейса (UI) и переключения панелей
public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject Main_Menu_Panel;
    public GameObject Main_Intro_Panel;
    public GameObject Level_Intro_And_Theory_Panel;
    public GameObject Gameplay_HUD;
    public GameObject Pop_Up_Hint_Panel;
    public GameObject Bonus_Level_Panel;
    public GameObject Game_Completed_Panel;

    [Header("Texts")]
    public TextMeshProUGUI Title_Text;
    public TextMeshProUGUI Intro_Text;
    public TextMeshProUGUI Theory_Text;
    public TextMeshProUGUI Question_Text;
    public TextMeshProUGUI Hint_Pop_Up_Text;
    public TextMeshProUGUI Feedback_Text;
    public TextMeshProUGUI Score_Result_Text;

    [Header("Buttons")]
    public Button Bonus_Level_Button; // Единственная интерактивная кнопка в меню с условием разблокировки

    [Header("UI Texts for Localization")]
    public TextMeshProUGUI New_Game_Button_Text;
    public TextMeshProUGUI Bonus_Button_Text;
    public TextMeshProUGUI Language_Button_Text;
    public TextMeshProUGUI Main_Intro_UI_Text;
    public TextMeshProUGUI Start_Intro_Button_Text;
    public TextMeshProUGUI Level_Start_Button_Text;
    public TextMeshProUGUI Level_Close_Hint_Pop_Up;
    public TextMeshProUGUI Return_To_Menu_Button_Text;

    [Header("Translations")]
    public LocalizedText New_Game_String;
    public LocalizedText Bonus_String;
    public LocalizedText Language_String;
    public LocalizedText Main_Intro_String;
    public LocalizedText Start_Intro_Button_String;
    public LocalizedText Level_Start_Button_String = new LocalizedText { RU = "Продолжить", EN = "Continue" };
    public LocalizedText Level_Close_Hint_Pop_Up_String = new LocalizedText { RU = "Закрыть", EN = "Close" };
    public LocalizedText Return_To_Menu_String = new LocalizedText { RU = "Главное меню", EN = "Main Menu" };

    [Header("Bonus Level Elements")]
    public GameObject[] Bonus_Brand_Buttons;
    public TextMeshProUGUI Bonus_Result_Text;
    public TextMeshProUGUI Bonus_Title_Text;

    [Header("Bonus Level Translations")]
    public LocalizedText Bonus_Title_String = new LocalizedText { RU = "Выберите производителя:", EN = "Choose a manufacturer:" };
    public LocalizedText Intel_Comment = new LocalizedText { RU = "Intel — отличный и надежный выбор для работы и вычислений!", EN = "Intel is an excellent and reliable choice!" };
    public LocalizedText Amd_Comment = new LocalizedText { RU = "AMD — великолепный баланс цены и многопоточной мощности!", EN = "AMD is a great balance of price and power!" };
    public LocalizedText Nvidia_Comment = new LocalizedText { RU = "Nvidia — то что надо для игр, дизайна и ИИ!", EN = "Nvidia is just what you need for gaming and AI!" };

    private void Start()
    {
        Update_All_UI_Texts();
        Show_Main_Menu();
    }

    // Отображение главного меню игры
    public void Show_Main_Menu()
    {
        Hide_All_Panels();
        if (Main_Menu_Panel != null) Main_Menu_Panel.SetActive(true);

        // Проверяем доступность бонусного уровня при входе в меню
        if (Bonus_Level_Button != null)
            Bonus_Level_Button.interactable = SaveManager.Is_Bonus_Unlocked();
    }

    // Показ вступительного экрана перед началом игры
    public void Show_Main_Intro()
    {
        Hide_All_Panels();
        if (Main_Intro_Panel != null) Main_Intro_Panel.SetActive(true);
    }

    // Показ экрана теории и интро для конкретного уровня
    public void Show_Level_Intro_And_Theory(string title, string cutscene, string theory)
    {
        Hide_All_Panels();
        if (Level_Intro_And_Theory_Panel != null) Level_Intro_And_Theory_Panel.SetActive(true);
        if (Title_Text != null) Title_Text.text = title;
        if (Intro_Text != null) Intro_Text.text = cutscene;
        if (Theory_Text != null) Theory_Text.text = theory;
    }

    // Обработка нажатия на кнопку старта уровня (переход к вопросам)
    public void On_Start_Level_Button_Clicked()
    {
        Hide_All_Panels();
        if (Gameplay_HUD != null) Gameplay_HUD.SetActive(true);
        if (GameController.Instance != null) GameController.Instance.Begin_Questions_Phase();
    }

    public void Show_Question(string qText)
    {
        if (Question_Text != null) Question_Text.text = qText;
    }

    public void Show_Hint_Popup(string hint)
    {
        if (Pop_Up_Hint_Panel != null) Pop_Up_Hint_Panel.SetActive(true);
        if (Hint_Pop_Up_Text != null) Hint_Pop_Up_Text.text = hint;
    }

    public void Close_Hint_Popup()
    {
        if (Pop_Up_Hint_Panel != null) Pop_Up_Hint_Panel.SetActive(false);
    }

    public void Show_Feedback(bool isCorrect, string message)
    {
        if (Feedback_Text != null)
        {
            Feedback_Text.gameObject.SetActive(true);
            Feedback_Text.color = isCorrect ? Color.green : Color.red;
            Feedback_Text.text = message;
        }
        CancelInvoke(nameof(Hide_Feedback));
        Invoke(nameof(Hide_Feedback), 2f);
    }

    private void Hide_Feedback()
    {
        if (Feedback_Text != null) Feedback_Text.gameObject.SetActive(false);
    }

    public void Show_Game_Completion_Screen(float accuracy)
    {
        Hide_All_Panels();
        if (Game_Completed_Panel != null) Game_Completed_Panel.SetActive(true);
        if (Score_Result_Text != null)
        {
            Score_Result_Text.text = $"Игра завершена!\nТочность ответов: {accuracy:F1}%\n" +
                (accuracy >= 80f ? "Бонусный уровень разблокирован в Главном Меню!" : "Слишком много ошибок для разблокировки бонуса.");
        }
    }

    // Открытие бонусного уровня
    public void Open_Bonus_Level()
    {
        Hide_All_Panels();
        if (Bonus_Level_Panel != null) Bonus_Level_Panel.SetActive(true);

        if (Bonus_Title_Text != null && LocalizationManager.Instance != null)
        {
            Language currentLang = LocalizationManager.Instance.Current_Language;
            Bonus_Title_Text.text = Bonus_Title_String.Get_Text(currentLang);
            Bonus_Title_Text.gameObject.SetActive(true);
        }

        if (Bonus_Brand_Buttons != null)
        {
            foreach (var button in Bonus_Brand_Buttons)
            {
                if (button != null) button.SetActive(true);
            }
        }

        if (Bonus_Result_Text != null)
        {
            Bonus_Result_Text.text = "";
            Bonus_Result_Text.gameObject.SetActive(false);
        }
    }

    // Выбор бренда в бонусном уровне
    public void On_Bonus_Choice(string vendorName)
    {
        Language currentLang = LocalizationManager.Instance != null ? LocalizationManager.Instance.Current_Language : Language.RU;
        string comment = "";

        switch (vendorName)
        {
            case "Intel": comment = Intel_Comment.Get_Text(currentLang); break;
            case "AMD": comment = Amd_Comment.Get_Text(currentLang); break;
            case "Nvidia": comment = Nvidia_Comment.Get_Text(currentLang); break;
        }

        if (Bonus_Title_Text != null) Bonus_Title_Text.gameObject.SetActive(false);

        if (Bonus_Brand_Buttons != null)
        {
            foreach (var button in Bonus_Brand_Buttons)
            {
                if (button != null) button.SetActive(false);
            }
        }

        if (Bonus_Result_Text != null)
        {
            Bonus_Result_Text.gameObject.SetActive(true);
            Bonus_Result_Text.text = comment;
        }

        StopAllCoroutines();
        StartCoroutine(Return_To_Menu_After_Bonus_Delay(10f));
    }

    private System.Collections.IEnumerator Return_To_Menu_After_Bonus_Delay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (Bonus_Result_Text != null)
        {
            Bonus_Result_Text.text = "";
            Bonus_Result_Text.gameObject.SetActive(false);
        }

        if (Bonus_Title_Text != null) Bonus_Title_Text.gameObject.SetActive(true);

        if (Bonus_Brand_Buttons != null)
        {
            foreach (var button in Bonus_Brand_Buttons)
            {
                if (button != null) button.SetActive(true);
            }
        }

        Return_To_Menu();
    }

    // Скрытие всех игровых панелей интерфейса
    private void Hide_All_Panels()
    {
        if (Main_Menu_Panel != null) Main_Menu_Panel.SetActive(false);
        if (Main_Intro_Panel != null) Main_Intro_Panel.SetActive(false);
        if (Level_Intro_And_Theory_Panel != null) Level_Intro_And_Theory_Panel.SetActive(false);
        if (Gameplay_HUD != null) Gameplay_HUD.SetActive(false);
        if (Pop_Up_Hint_Panel != null) Pop_Up_Hint_Panel.SetActive(false);
        if (Bonus_Level_Panel != null) Bonus_Level_Panel.SetActive(false);
        if (Game_Completed_Panel != null) Game_Completed_Panel.SetActive(false);
    }

    // Обновление текстов интерфейса при смене языка
    public void Update_All_UI_Texts()
    {
        if (LocalizationManager.Instance == null) return;
        Language current = LocalizationManager.Instance.Current_Language;

        if (New_Game_Button_Text != null) New_Game_Button_Text.text = New_Game_String.Get_Text(current);
        if (Bonus_Button_Text != null) Bonus_Button_Text.text = Bonus_String.Get_Text(current);
        if (Language_Button_Text != null) Language_Button_Text.text = Language_String.Get_Text(current);
        if (Main_Intro_UI_Text != null) Main_Intro_UI_Text.text = Main_Intro_String.Get_Text(current);
        if (Start_Intro_Button_Text != null) Start_Intro_Button_Text.text = Start_Intro_Button_String.Get_Text(current);
        if (Level_Start_Button_Text != null) Level_Start_Button_Text.text = Level_Start_Button_String.Get_Text(current);
        if (Level_Close_Hint_Pop_Up != null) Level_Close_Hint_Pop_Up.text = Level_Close_Hint_Pop_Up_String.Get_Text(current);
        if (Return_To_Menu_Button_Text != null) Return_To_Menu_Button_Text.text = Return_To_Menu_String.Get_Text(current);
    }

    public void Toggle_Language()
    {
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.Current_Language == Language.RU)
            Set_Language_EN();
        else
            Set_Language_RU();
    }

    public void Set_Language_RU()
    {
        if (LocalizationManager.Instance != null) LocalizationManager.Instance.Set_Language(Language.RU);
        Update_All_UI_Texts();
    }

    public void Set_Language_EN()
    {
        if (LocalizationManager.Instance != null) LocalizationManager.Instance.Set_Language(Language.EN);
        Update_All_UI_Texts();
    }

    public void Return_To_Menu()
    {
        Hide_All_Panels();
        Show_Main_Menu();
    }
}
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject mainIntroPanel;
    public GameObject levelIntroAndTheoryPanel;
    public GameObject gameplayHUD;
    public GameObject popUpHintPanel;
    public GameObject bonusLevelPanel;
    public GameObject gameCompletedPanel;

    [Header("Texts")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI introText;
    public TextMeshProUGUI theoryText;
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI hintPopUpText;
    public TextMeshProUGUI feedbackText;
    public TextMeshProUGUI scoreResultText;

    [Header("Buttons")]
    public Button loadGameButton;
    public Button bonusLevelButton;

    [Header("UI Texts for Localization")]
    public TextMeshProUGUI newGameBtnText;
    public TextMeshProUGUI loadGameBtnText;
    public TextMeshProUGUI bonusBtnText;
    public TextMeshProUGUI languageBtnText;
    public TextMeshProUGUI mainIntroUIText; // Вступительный текст
    public TextMeshProUGUI startIntroBtnText; // Кнопка "Начать" в интро
    public TextMeshProUGUI levelStartBtnText; // Кнопка "Продолжить" в интро уровня
    public TextMeshProUGUI levelCloseHintPopUp; // Кнопка "Закрыть" подсказку в интро уровня
    public TextMeshProUGUI returnToMenuBtnText; // Кнопка возврата в меню

    [Header("Translations")]
    public LocalizedText newGameString;
    public LocalizedText loadGameString;
    public LocalizedText bonusString;
    public LocalizedText languageString;
    public LocalizedText mainIntroString;
    public LocalizedText startIntroBtnString;
    public LocalizedText level1StartBtnString = new LocalizedText { RU = "Продолжить", EN = "Continue" };
    public LocalizedText level2StartBtnString = new LocalizedText { RU = "Продолжить", EN = "Continue" };
    public LocalizedText level3StartBtnString = new LocalizedText { RU = "Продолжить", EN = "Continue" };
    public LocalizedText level4StartBtnString = new LocalizedText { RU = "Продолжить", EN = "Continue" };
    public LocalizedText level1CloseHintPopUpString = new LocalizedText { RU = "Закрыть", EN = "Close" };
    public LocalizedText level2CloseHintPopUpString = new LocalizedText { RU = "Закрыть", EN = "Close" };
    public LocalizedText level3CloseHintPopUpString = new LocalizedText { RU = "Закрыть", EN = "Close" };
    public LocalizedText level4CloseHintPopUpString = new LocalizedText { RU = "Закрыть", EN = "Close" };
    public LocalizedText returnToMenuString = new LocalizedText { RU = "Главное меню", EN = "Main Menu" };

    [Header("Bonus Level UI Elements")]
    public GameObject bonusButtonsContainer; // Контейнер, в котором лежат три кнопки (Intel, AMD, Nvidia)

    [Header("Bonus Level Elements")]
    public GameObject[] bonusBrandButtons; // Ваши 3 кнопки (Intel, AMD, Nvidia)
    public TextMeshProUGUI bonusResultText;  // Текст для отображения результата выбора в бонусе
    public TextMeshProUGUI bonusTitleText;   // Текст заголовка «Выберите производителя»

    [Header("Bonus Level Translations")]
    public LocalizedText bonusTitleString = new LocalizedText
    {
        RU = "Выберите производителя:",
        EN = "Choose a manufacturer:"
    };
    public LocalizedText intelComment = new LocalizedText
    {
        RU = "Intel — отличный и надежный выбор для работы и вычислений!",
        EN = "Intel is an excellent and reliable choice for work and computing!"
    };
    public LocalizedText amdComment = new LocalizedText
    {
        RU = "AMD — великолепный баланс цены и многопоточной мощности!",
        EN = "AMD is a great balance of price and multi-threaded power!"
    };
    public LocalizedText nvidiaComment = new LocalizedText
    {
        RU = "Nvidia — то что надо для игр, дизайна и ИИ!",
        EN = "Nvidia is just what you need for gaming, design, and AI!"
    };

    private void Start()
    {
        UpdateAllUITexts();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
        
        // Разблокировка кнопки загрузки и бонуса
        loadGameButton.interactable = SaveManager.GetSavedLevel() > 0;
        bonusLevelButton.interactable = SaveManager.IsBonusUnlocked();
    }

    public void ShowMainIntro()
    {
        HideAllPanels();
        mainIntroPanel.SetActive(true);
    }

    public void ShowLevelIntroAndTheory(string title, string cutscene, string theory)
    {
        HideAllPanels();
        levelIntroAndTheoryPanel.SetActive(true);
        titleText.text = title;
        introText.text = cutscene;
        theoryText.text = theory;
    }

    public void OnStartLevelButtonClicked()
    {
        HideAllPanels();
        gameplayHUD.SetActive(true);
        GameController.Instance.BeginQuestionsPhase();
    }

    public void ShowQuestion(string qText)
    {
        questionText.text = qText;
    }

    public void ShowHintPopUp(string hint)
    {
        popUpHintPanel.SetActive(true);
        hintPopUpText.text = hint;
    }

    public void CloseHintPopUp()
    {
        popUpHintPanel.SetActive(false);
    }

    public void ShowFeedback(bool isCorrect, string message)
    {
        feedbackText.gameObject.SetActive(true);
        feedbackText.color = isCorrect ? Color.green : Color.red;
        feedbackText.text = message;
        CancelInvoke(nameof(HideFeedback));
        Invoke(nameof(HideFeedback), 2f);
    }

    private void HideFeedback()
    {
        feedbackText.gameObject.SetActive(false);
    }

    public void ShowGameCompletionScreen(float accuracy)
    {
        HideAllPanels();
        gameCompletedPanel.SetActive(true);
        scoreResultText.text = $"Игра завершена!\nТочность ответов: {accuracy:F1}%\n" +
            (accuracy >= 80f ? "Бонусный уровень разблокирован в Главном Меню!" : "Слишком много ошибок для разблокировки бонуса.");
    }

    public void OpenBonusLevel()
    {
        HideAllPanels();
        bonusLevelPanel.SetActive(true);

        // Обновляем текст заголовка под текущий язык
        if (bonusTitleText != null)
        {
            Language currentLang = LocalizationManager.Instance.CurrentLanguage;
            bonusTitleText.text = bonusTitleString.Get(currentLang);
            bonusTitleText.gameObject.SetActive(true);
        }

        // Убеждаемся, что кнопки видны при входе на уровень
        if (bonusBrandButtons != null)
        {
            foreach (var btn in bonusBrandButtons)
            {
                if (btn != null) btn.SetActive(true);
            }
        }

        // Скрываем прошлый результат, если он остался
        if (bonusResultText != null)
        {
            bonusResultText.text = "";
            bonusResultText.gameObject.SetActive(false);
        }
    }

    public void OnBonusChoice(string vendorName)
    {
        Language currentLang = LocalizationManager.Instance.CurrentLanguage;
        string comment = "";

        switch (vendorName)
        {
            case "Intel": comment = intelComment.Get(currentLang); break;
            case "AMD": comment = amdComment.Get(currentLang); break;
            case "Nvidia": comment = nvidiaComment.Get(currentLang); break;
        }

        // 1. Скрываем заголовок и кнопки брендов
        if (bonusTitleText != null) bonusTitleText.gameObject.SetActive(false);

        if (bonusBrandButtons != null)
        {
            foreach (var btn in bonusBrandButtons)
            {
                if (btn != null) btn.SetActive(false);
            }
        }

        // 2. Показываем текст комментария
        if (bonusResultText != null)
        {
            bonusResultText.gameObject.SetActive(true);
            bonusResultText.text = comment;
        }

        // 3. Запускаем таймер на 10 секунд и возврат в главное меню
        StopAllCoroutines();
        StartCoroutine(ReturnToMenuAfterBonusDelay(10f));
    }

    private System.Collections.IEnumerator ReturnToMenuAfterBonusDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (bonusResultText != null)
        {
            bonusResultText.text = "";
            bonusResultText.gameObject.SetActive(false);
        }

        if (bonusTitleText != null) bonusTitleText.gameObject.SetActive(true);

        if (bonusBrandButtons != null)
        {
            foreach (var btn in bonusBrandButtons)
            {
                if (btn != null) btn.SetActive(true);
            }
        }

        ReturnToMainMenu();
    }

    private void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        mainIntroPanel.SetActive(false);
        levelIntroAndTheoryPanel.SetActive(false);
        gameplayHUD.SetActive(false);
        popUpHintPanel.SetActive(false);
        bonusLevelPanel.SetActive(false);
        gameCompletedPanel.SetActive(false);
    }

    public void UpdateAllUITexts()
    {
        Language current = LocalizationManager.Instance.CurrentLanguage;

        if (newGameBtnText != null) newGameBtnText.text = newGameString.Get(current);
        if (loadGameBtnText != null) loadGameBtnText.text = loadGameString.Get(current);
        if (bonusBtnText != null) bonusBtnText.text = bonusString.Get(current);
        if (languageBtnText != null) languageBtnText.text = languageString.Get(current);
        if (mainIntroUIText != null) mainIntroUIText.text = mainIntroString.Get(current);
        if (startIntroBtnText != null) startIntroBtnText.text = startIntroBtnString.Get(current);
        if (levelStartBtnText != null) levelStartBtnText.text = level1StartBtnString.Get(current);
        if (levelCloseHintPopUp != null) levelCloseHintPopUp.text = level1CloseHintPopUpString.Get(current);
        if (returnToMenuBtnText != null) returnToMenuBtnText.text = returnToMenuString.Get(current);
    }

    public void ToggleLanguage()
    {
        if (LocalizationManager.Instance.CurrentLanguage == Language.RU)
            SetLanguageEN();
        else
            SetLanguageRU();
    }

    public void SetLanguageRU()
    {
        LocalizationManager.Instance.SetLanguage(Language.RU);
        UpdateAllUITexts();
    }

    public void SetLanguageEN()
    {
        LocalizationManager.Instance.SetLanguage(Language.EN);
        UpdateAllUITexts();
    }

    public void ReturnToMainMenu()
    {
        HideAllPanels();
        ShowMainMenu();
    }
}
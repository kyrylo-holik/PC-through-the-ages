using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Translations")]
    public LocalizedText newGameString;
    public LocalizedText loadGameString;
    public LocalizedText bonusString;
    public LocalizedText languageString;
    public LocalizedText mainIntroString;
    public LocalizedText startIntroBtnString;

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
    }

    public void OnBonusChoice(string vendorName)
    {
        string comment = "";
        switch (vendorName)
        {
            case "Intel": comment = "Intel — отличный и надежный выбор для работы и вычислений!"; break;
            case "AMD": comment = "AMD — великолепный баланс цены и многопоточной мощности!"; break;
            case "Nvidia": comment = "Nvidia — то что надо для игр, дизайна и ИИ!"; break;
        }
        ShowHintPopUp(comment);
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
}
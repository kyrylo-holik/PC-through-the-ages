using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private const string LEVEL_KEY = "SavedLevelIndex";
    private const string TOTAL_QUESTIONS_KEY = "TotalQuestions";
    private const string CORRECT_ANSWERS_KEY = "CorrectAnswers";

    public static void SaveProgress(int levelIndex, int totalQuestions, int correctAnswers)
    {
        PlayerPrefs.SetInt(LEVEL_KEY, levelIndex);
        PlayerPrefs.SetInt(TOTAL_QUESTIONS_KEY, totalQuestions);
        PlayerPrefs.SetInt(CORRECT_ANSWERS_KEY, correctAnswers);
        PlayerPrefs.Save();
    }

    public static int GetSavedLevel() => PlayerPrefs.GetInt(LEVEL_KEY, 0);
    
    public static float GetAccuracyPercentage()
    {
        int total = PlayerPrefs.GetInt(TOTAL_QUESTIONS_KEY, 0);
        int correct = PlayerPrefs.GetInt(CORRECT_ANSWERS_KEY, 0);
        if (total == 0) return 0f;
        return ((float)correct / total) * 100f;
    }

    public static bool IsBonusUnlocked()
    {
        // Доступно только после 5 уровня и с точностью >= 80%
        return GetSavedLevel() >= 5 && GetAccuracyPercentage() >= 80f;
    }

    public static void ResetData()
    {
        PlayerPrefs.DeleteAll();
    }
}
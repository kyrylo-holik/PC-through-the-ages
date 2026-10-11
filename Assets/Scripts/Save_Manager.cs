using UnityEngine;

// Менеджер сохранения прогресса и статистики игрока
public class SaveManager : MonoBehaviour
{
    private const string LEVEL_KEY = "SavedLevelIndex";
    private const string TOTAL_QUESTIONS_KEY = "TotalQuestions";
    private const string CORRECT_ANSWERS_KEY = "CorrectAnswers";

    // Сохранение текущего игрового прогресса
    public static void Save_Progress(int levelIndex, int totalQuestions, int correctAnswers)
    {
        PlayerPrefs.SetInt(LEVEL_KEY, levelIndex);
        PlayerPrefs.SetInt(TOTAL_QUESTIONS_KEY, totalQuestions);
        PlayerPrefs.SetInt(CORRECT_ANSWERS_KEY, correctAnswers);
        PlayerPrefs.Save();
    }

    // Получение индекса сохраненного уровня
    public static int Get_Saved_Level() => PlayerPrefs.GetInt(LEVEL_KEY, 0);

    // Расчет процента правильных ответов игрока
    public static float Get_Accuracy_Percentage()
    {
        int total = PlayerPrefs.GetInt(TOTAL_QUESTIONS_KEY, 0);
        int correct = PlayerPrefs.GetInt(CORRECT_ANSWERS_KEY, 0);
        if (total == 0) return 0f;
        return ((float)correct / total) * 100f;
    }

    // Проверка доступности бонусного уровня (пройдены все 5 уровней и точность >= 80%)
    public static bool Is_Bonus_Unlocked()
    {
        return Get_Saved_Level() >= 5 && Get_Accuracy_Percentage() >= 80f;
    }

    // Полный сброс всех сохраненных данных
    public static void Reset_Data()
    {
        PlayerPrefs.DeleteAll();
    }
}
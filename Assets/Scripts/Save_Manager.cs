using UnityEngine;

// Менеджер сохранения прогресса и статистики игрока
public class Save_Manager : MonoBehaviour
{
    private const string Level_Key = "Saved_Level_Index";
    private const string Total_Questions_Key = "Total_Questions";
    private const string Correct_Answers_Key = "Correct_Answers";

    // Сохранение текущего игрового прогресса
    public static void Save_Progress(int Level_Index, int Total_Questions, int Correct_Answers)
    {
        PlayerPrefs.SetInt(Level_Key, Level_Index);
        PlayerPrefs.SetInt(Total_Questions_Key, Total_Questions);
        PlayerPrefs.SetInt(Correct_Answers_Key, Correct_Answers);
        PlayerPrefs.Save();
    }

    // Получение индекса сохраненного уровня
    public static int Get_Saved_Level()
    {
        return PlayerPrefs.GetInt(Level_Key, 0);
    }

    // Расчет процента правильных ответов игрока
    public static float Get_Accuracy_Percentage()
    {
        int Total = PlayerPrefs.GetInt(Total_Questions_Key, 0);
        int Correct = PlayerPrefs.GetInt(Correct_Answers_Key, 0);

        if (Total == 0)
            return 0f;

        return (float)Correct / Total * 100f;
    }

    // Проверка доступности бонусного уровня
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
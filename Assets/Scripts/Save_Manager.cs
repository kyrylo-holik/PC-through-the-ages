using UnityEngine;

// Менеджер сохранения: хранит прогресс и статистику игрока в PlayerPrefs
// (это встроенное хранилище Unity: данные остаются на компьютере между запусками игры).
// Все методы статические, поэтому вызываются просто: Save_Manager.Save_Progress(...).
public class Save_Manager : MonoBehaviour
{
    // Сколько процентов точности нужно набрать, чтобы открылся бонусный уровень.
    public const float Bonus_Accuracy_Percent = 80f;

    // Сколько эпох нужно пройти, чтобы открылся бонусный уровень (в игре всего 5 эпох).
    private const int Levels_Required_For_Bonus = 5;

    // Названия «ячеек» в PlayerPrefs, куда записываются числа.
    private const string Level_Key = "Saved_Level_Index";
    private const string Total_Questions_Key = "Total_Questions";
    private const string Correct_Answers_Key = "Correct_Answers";

    // Сохраняет прогресс: сколько эпох пройдено, сколько всего вопросов и сколько ответов верных.
    public static void Save_Progress(int Level_Index, int Total_Questions, int Correct_Answers)
    {
        PlayerPrefs.SetInt(Level_Key, Level_Index);
        PlayerPrefs.SetInt(Total_Questions_Key, Total_Questions);
        PlayerPrefs.SetInt(Correct_Answers_Key, Correct_Answers);
        PlayerPrefs.Save();
    }

    // Возвращает, сколько эпох пройдено (0, если игрок ещё ничего не проходил).
    public static int Get_Saved_Level()
    {
        return PlayerPrefs.GetInt(Level_Key, 0);
    }

    // Возвращает точность игрока в процентах: доля вопросов, отвеченных верно с первой попытки.
    public static float Get_Accuracy_Percentage()
    {
        int Total = PlayerPrefs.GetInt(Total_Questions_Key, 0);
        int Correct = PlayerPrefs.GetInt(Correct_Answers_Key, 0);

        // Защита от деления на ноль.
        if (Total == 0)
            return 0f;

        return (float)Correct / Total * 100f;
    }

    // Бонус доступен, если пройдены все эпохи и точность не ниже порога.
    public static bool Is_Bonus_Unlocked()
    {
        return Get_Saved_Level() >= Levels_Required_For_Bonus
            && Get_Accuracy_Percentage() >= Bonus_Accuracy_Percent;
    }

    // Полностью стирает сохранённые данные (вызывается при старте новой игры).
    public static void Reset_Data()
    {
        PlayerPrefs.DeleteAll();
    }
}

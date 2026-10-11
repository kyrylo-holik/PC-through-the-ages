using UnityEngine;

// Менеджер локализации для переключения языков в игре
public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;
    public Language Current_Language = Language.RU;

    private void Awake()
    {
        // Инициализация паттерна Singleton с сохранением между сценами
        if (Instance == null)
        {
            Instance = this;
            Object.DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Установка текущего языка интерфейса
    public void Set_Language(Language lang)
    {
        Current_Language = lang;
    }
}
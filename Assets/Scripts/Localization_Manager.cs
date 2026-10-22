using UnityEngine;

// Менеджер локализации для переключения языков в игре
public class Localization_Manager : MonoBehaviour
{
    public static Localization_Manager Instance;

    public Language Current_Language = Language.RU;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Установка текущего языка интерфейса
    public void Set_Language(Language Lang)
    {
        Current_Language = Lang;
    }
}
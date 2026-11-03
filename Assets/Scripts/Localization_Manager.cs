using UnityEngine;

// Менеджер локализации: помнит, на каком языке сейчас показывается игра (русский или английский).
// Существует в единственном экземпляре и не удаляется при смене сцены.
public class Localization_Manager : MonoBehaviour
{
    // Единственный экземпляр менеджера. Другие скрипты обращаются к нему так:
    // Localization_Manager.Instance.Current_Language
    public static Localization_Manager Instance;

    // Текущий язык интерфейса. По умолчанию — русский.
    public Language Current_Language = Language.RU;

    private void Awake()
    {
        if (Instance == null)
        {
            // Это первый менеджер: запоминаем его и не уничтожаем при загрузке новой сцены.
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Менеджер уже существует — лишний дубликат удаляем.
            Destroy(gameObject);
        }
    }

    // Переключает язык интерфейса.
    public void Set_Language(Language Lang)
    {
        Current_Language = Lang;
    }
}

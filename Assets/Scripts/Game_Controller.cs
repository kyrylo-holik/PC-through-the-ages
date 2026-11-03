using System.Collections.Generic;
using UnityEngine;

// Главный контроллер игры. Он:
//   - создаёт 3D-модель компьютера для каждой эпохи;
//   - по очереди задаёт вопросы и проверяет клики игрока;
//   - считает ошибки и точность ответов;
//   - показывает теорию после заданий;
//   - показывает 3D-модель на бонусном уровне.
// Единственный экземпляр доступен из других скриптов через Game_Controller.Instance.
public class Game_Controller : MonoBehaviour
{
    public static Game_Controller Instance;

    // ------------------------------------------------------------------
    // Ссылки на объекты сцены (заполняются в Inspector)
    // ------------------------------------------------------------------

    [Header("Scene_References")]
    [SerializeField] private Transform PC_Spawn_Point;   // точка, в которой появляется 3D-модель
    [SerializeField] private UI_Manager UI_Manager_Ref;  // менеджер интерфейса

    // ------------------------------------------------------------------
    // Настройки игры
    // ------------------------------------------------------------------

    private const float Next_Question_Delay = 1.5f;       // пауза после верного ответа перед следующим вопросом (сек.)
    private const int Hint_Error_Threshold = 3;           // на какой ошибке показывается текстовая подсказка
    private const int Highlight_Error_Threshold = 5;      // с какой ошибки правильная деталь подсвечивается жёлтым
    private const float Bonus_Auto_Rotation_Speed = 20f;  // скорость самовращения бонусной модели (градусов в секунду)

    // Что нужно найти в каждой эпохе: по строке на эпоху, внутри — по порядку вопросов.
    // ВАЖНО: порядок и количество должны совпадать с Localization_DB.Questions.Q и .Hints.
    private static readonly Component_Type[][] Era_Targets =
    {
        // Эпоха I
        new[]
        {
            Component_Type.Vacuum_Tube,
            Component_Type.Switch_Panel,
            Component_Type.Patch_Panel,
            Component_Type.Punch_Card_Reader
        },

        // Эпоха II
        new[]
        {
            Component_Type.Transistor,
            Component_Type.Magnetic_Tape,
            Component_Type.Core_Memory,
            Component_Type.Control_Console
        },

        // Эпоха III
        new[]
        {
            Component_Type.Integrated_Circuit,
            Component_Type.Floppy_Drive,
            Component_Type.Keyboard,
            Component_Type.Monitor_CRT
        },

        // Эпоха IV (разборный ПК: первое задание — открыть крышку)
        new[]
        {
            Component_Type.Case_Cover,
            Component_Type.CPU,
            Component_Type.RAM,
            Component_Type.GPU,
            Component_Type.Hard_Drive
        },

        // Эпоха V (разборный ПК: первое задание — открыть стекло)
        new[]
        {
            Component_Type.Case_Cover,
            Component_Type.Cooler,
            Component_Type.RAM,
            Component_Type.GPU,
            Component_Type.SSD_NVMe
        }
    };

    // ------------------------------------------------------------------
    // Состояние игры
    // ------------------------------------------------------------------

    private int Current_Era_Index;                 // номер текущей эпохи (с нуля)
    private int Current_Question_Index;            // номер текущего вопроса внутри эпохи (с нуля)
    private int Error_Count_On_Current_Question;   // сколько ошибок игрок сделал на текущем вопросе

    // Статистика для точности:
    private int Total_Questions_Asked;   // сколько вопросов пройдено
    private int Total_Correct_Answers;   // на скольких из них ответили верно с первой попытки

    private GameObject Current_PC_Model;       // 3D-модель, которая сейчас на сцене (эпоха или бонус)
    private Era_Level_Data Current_Era_Data;   // данные текущей эпохи
    private List<Era_Level_Data> All_Eras_Data;

    private bool Is_Bonus_Model_Active;        // true, если на сцене бонусная модель (она вращается сама)

    // Можно ли сейчас кликать по деталям (нельзя: на экранах теории и в паузах между вопросами).
    public bool Is_Interaction_Enabled { get; private set; }

    // true, пока на экране теория эпохи (она показывается ПОСЛЕ заданий).
    public bool Is_Theory_Phase { get; private set; }

    // Текущий язык интерфейса.
    private Language Current_Language => Localization_Manager.Instance.Current_Language;

    // ------------------------------------------------------------------
    // Запуск и обновление
    // ------------------------------------------------------------------

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize_Game_Data();
    }

    // Вызывается каждый кадр: вращает модель.
    private void Update()
    {
        if (Current_PC_Model == null)
            return;

        if (Input.GetMouseButton(1))
        {
            // Правая кнопка мыши зажата: вращаем модель вслед за движением мыши по горизонтали.
            float Rotation_X = Input.GetAxis("Mouse X") * 5f;
            Current_PC_Model.transform.Rotate(Vector3.up, -Rotation_X, Space.World);
        }
        else if (Is_Bonus_Model_Active)
        {
            // Кнопка не зажата: бонусная модель медленно вращается сама, как на витрине.
            Current_PC_Model.transform.Rotate(Vector3.up, Bonus_Auto_Rotation_Speed * Time.deltaTime, Space.World);
        }
    }

    // ------------------------------------------------------------------
    // Ход игры
    // ------------------------------------------------------------------

    // Кнопка «Начать игру»: стирает старый прогресс и открывает экран с описанием игры.
    public void Start_New_Game()
    {
        CancelInvoke();
        Clear_Current_PC_Model();

        Save_Manager.Reset_Data();
        Current_Era_Index = 0;
        Total_Questions_Asked = 0;
        Total_Correct_Answers = 0;
        Is_Interaction_Enabled = false;
        Is_Theory_Phase = false;

        UI_Manager_Ref.Show_Main_Intro();
    }

    // Запускает эпоху: создаёт модель компьютера и показывает описание (теория будет позже, после заданий).
    public void Start_Era_Level(int Era_Index)
    {
        CancelInvoke(nameof(Show_Current_Question));

        Current_Era_Index = Era_Index;
        Current_Question_Index = 0;
        Error_Count_On_Current_Question = 0;
        Is_Interaction_Enabled = false;
        Is_Theory_Phase = false;

        // Старую модель убираем, новую создаём в точке появления.
        Clear_Current_PC_Model();
        Current_PC_Model = Procedural_PC_Models.Create_Era_Model(Era_Index, PC_Spawn_Point);
        Current_Era_Data = All_Eras_Data[Era_Index];

        UI_Manager_Ref.Show_Level_Intro_And_Theory(
            Localization_DB.Era.Titles[Era_Index].Get(Current_Language),
            Localization_DB.Era.Intro[Era_Index].Get(Current_Language),
            string.Empty   // теория появится только после заданий
        );
    }

    // Игрок прочитал описание и нажал «Продолжить»: начинаются вопросы.
    public void Begin_Questions_Phase()
    {
        Show_Current_Question();
    }

    // Показывает текущий вопрос. Если вопросы закончились — сохраняет прогресс и показывает теорию.
    private void Show_Current_Question()
    {
        if (Current_Question_Index < Current_Era_Data.Questions.Count)
        {
            Is_Interaction_Enabled = true;

            Question_Data Q_Data = Current_Era_Data.Questions[Current_Question_Index];
            UI_Manager_Ref.Show_Question(Q_Data.Question_Text.Get(Current_Language));
        }
        else
        {
            Is_Interaction_Enabled = false;

            Save_Manager.Save_Progress(Current_Era_Index + 1, Total_Questions_Asked, Total_Correct_Answers);
            Show_Theory_Phase();
        }
    }

    // Показывает теорию эпохи. Она идёт ПОСЛЕ заданий: сначала задача, потом теория.
    private void Show_Theory_Phase()
    {
        Is_Theory_Phase = true;

        UI_Manager_Ref.Show_Level_Intro_And_Theory(
            Localization_DB.Era.Titles[Current_Era_Index].Get(Current_Language),
            Localization_DB.Gameplay.Theory_Header.Get(Current_Language),
            Localization_DB.Era.Theory[Current_Era_Index].Get(Current_Language)
        );
    }

    // Кнопка «Продолжить» на экране теории: переход к следующей эпохе или к итогам игры.
    public void Finish_Theory_Phase()
    {
        Is_Theory_Phase = false;

        if (Current_Era_Index + 1 < All_Eras_Data.Count)
        {
            Start_Era_Level(Current_Era_Index + 1);
        }
        else
        {
            Clear_Current_PC_Model();
            UI_Manager_Ref.Show_Game_Completion_Screen(Save_Manager.Get_Accuracy_Percentage());
        }
    }

    // ------------------------------------------------------------------
    // Клики игрока
    // ------------------------------------------------------------------

    // Вызывается из Interactive_Component, когда игрок кликнул по детали модели.
    public void On_Component_Clicked(Interactive_Component Component)
    {
        if (Current_Era_Data == null || Current_PC_Model == null)
            return;

        bool Has_Question = Current_Question_Index < Current_Era_Data.Questions.Count;
        Question_Data Current_Q_Data = Has_Question ? Current_Era_Data.Questions[Current_Question_Index] : null;

        // Клик по крышке корпуса открывает или закрывает её.
        // Если текущее задание — «открой корпус», то открытие засчитывается как верный ответ.
        if (Component.Component_Type == Component_Type.Case_Cover)
        {
            bool Is_Open_Now = Toggle_Case_Cover_Internal();

            if (Is_Open_Now && Has_Question && Current_Q_Data.Target_Component == Component_Type.Case_Cover)
                Register_Correct_Answer();

            return;
        }

        if (!Has_Question)
            return;

        // Если деталь съёмная (в моделях 4-й и 5-й эпох), найдём её «съёмный» компонент.
        Detachable_Part Part = Component.GetComponentInParent<Detachable_Part>();

        if (Component.Component_Type == Current_Q_Data.Target_Component)
        {
            // Верная деталь всегда выдвигается из корпуса, чтобы её можно было рассмотреть.
            if (Part != null)
                Part.Set_Detached(true);

            Register_Correct_Answer();
        }
        else
        {
            // Неверная деталь тоже выдвигается или возвращается (можно «разбирать» компьютер).
            if (Part != null)
                Part.Toggle();

            Register_Wrong_Answer(Current_Q_Data);
        }
    }

    // Верный ответ: записывает статистику, показывает «Верно!» и через паузу переходит к следующему вопросу.
    private void Register_Correct_Answer()
    {
        // Каждый вопрос считается один раз, но «верным» — только если не было ошибок.
        Total_Questions_Asked++;

        if (Error_Count_On_Current_Question == 0)
            Total_Correct_Answers++;

        Error_Count_On_Current_Question = 0;

        // На время паузы клики блокируются: иначе можно «ответить» на следующий вопрос до его показа.
        Is_Interaction_Enabled = false;

        Set_Highlight(null, false);
        UI_Manager_Ref.Close_Hint_Popup();
        UI_Manager_Ref.Show_Feedback(true, Localization_DB.Gameplay.Correct.Get(Current_Language));

        Current_Question_Index++;
        Invoke(nameof(Show_Current_Question), Next_Question_Delay);
    }

    // Неверный ответ: считает ошибку и, если нужно, выдаёт подсказку или подсвечивает правильную деталь.
    private void Register_Wrong_Answer(Question_Data Current_Q_Data)
    {
        Error_Count_On_Current_Question++;

        UI_Manager_Ref.Show_Feedback(false, Localization_DB.Gameplay.Wrong.Get(Current_Language));

        if (Error_Count_On_Current_Question == Hint_Error_Threshold)
        {
            // На 3-й ошибке — текстовая подсказка.
            UI_Manager_Ref.Show_Hint_Popup(Current_Q_Data.Hint_Text.Get(Current_Language));
        }
        else if (Error_Count_On_Current_Question >= Highlight_Error_Threshold)
        {
            // Начиная с 5-й ошибки — правильная деталь подсвечивается жёлтым.
            Set_Highlight(Current_Q_Data.Target_Component, true);
            UI_Manager_Ref.Show_Hint_Popup(Localization_DB.Gameplay.Highlight.Get(Current_Language));
        }
    }

    // ------------------------------------------------------------------
    // Крышка корпуса и подсветка
    // ------------------------------------------------------------------

    // Открывает или закрывает крышку корпуса. Оставлено публичным на случай,
    // если метод привязан к кнопке в сцене.
    public void Toggle_Case_Cover()
    {
        Toggle_Case_Cover_Internal();
    }

    // Переключает крышку; возвращает true, если после этого она открыта.
    private bool Toggle_Case_Cover_Internal()
    {
        if (Current_PC_Model == null)
            return false;

        Openable_Cover Cover = Current_PC_Model.GetComponentInChildren<Openable_Cover>();

        if (Cover == null)
            return false;

        Cover.Toggle();
        return Cover.Is_Open;
    }

    // Включает или выключает подсветку деталей.
    // Type = null — действие применяется ко всем деталям, иначе только к деталям этого типа.
    private void Set_Highlight(Component_Type? Type, bool State)
    {
        if (Current_PC_Model == null)
            return;

        foreach (Interactive_Component Comp in Current_PC_Model.GetComponentsInChildren<Interactive_Component>())
        {
            if (!Type.HasValue || Comp.Component_Type == Type.Value)
                Comp.Highlight(State);
        }
    }

    // ------------------------------------------------------------------
    // Бонусный уровень
    // ------------------------------------------------------------------

    // Показывает 3D-модель выбранного производителя ("Intel", "AMD", "Nvidia").
    public void Show_Bonus_Model(string Vendor_Name)
    {
        Clear_Current_PC_Model();

        Current_Era_Data = null;
        Is_Interaction_Enabled = false;

        Current_PC_Model = Procedural_PC_Models.Create_Bonus_Model(Vendor_Name, PC_Spawn_Point);
        Is_Bonus_Model_Active = Current_PC_Model != null;
    }

    // Убирает бонусную модель (если она сейчас на сцене).
    public void Clear_Bonus_Model()
    {
        if (Is_Bonus_Model_Active)
            Clear_Current_PC_Model();
    }

    // ------------------------------------------------------------------
    // Служебные методы
    // ------------------------------------------------------------------

    // Удаляет текущую 3D-модель со сцены.
    private void Clear_Current_PC_Model()
    {
        if (Current_PC_Model != null)
        {
            Destroy(Current_PC_Model);
            Current_PC_Model = null;
        }

        Is_Bonus_Model_Active = false;
    }

    // Собирает данные всех эпох: названия, описания, теорию и вопросы с целями и подсказками.
    private void Initialize_Game_Data()
    {
        All_Eras_Data = new List<Era_Level_Data>();

        for (int Era = 0; Era < Era_Targets.Length; Era++)
        {
            Component_Type[] Targets = Era_Targets[Era];
            Localized_Text[] Texts = Localization_DB.Questions.Q[Era];
            Localized_Text[] Hints = Localization_DB.Questions.Hints[Era];

            // Защита от опечаток: число целей, вопросов и подсказок должно совпадать.
            if (Texts.Length != Targets.Length || Hints.Length != Targets.Length)
            {
                Debug.LogError(
                    $"Эпоха {Era}: не совпадает число целей ({Targets.Length}), вопросов ({Texts.Length}) и подсказок ({Hints.Length})."
                );
            }

            Era_Level_Data Data = new Era_Level_Data
            {
                Era_Index = Era,
                Era_Title = Localization_DB.Era.Titles[Era],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[Era],
                Theory_Card_Text = Localization_DB.Era.Theory[Era],
                Questions = new List<Question_Data>()
            };

            int Count = Mathf.Min(Targets.Length, Mathf.Min(Texts.Length, Hints.Length));

            for (int I = 0; I < Count; I++)
            {
                Data.Questions.Add(new Question_Data
                {
                    Question_Text = Texts[I],
                    Target_Component = Targets[I],
                    Hint_Text = Hints[I]
                });
            }

            All_Eras_Data.Add(Data);
        }
    }
}

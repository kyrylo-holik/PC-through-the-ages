using System.Collections.Generic;
using UnityEngine;

public class Game_Controller : MonoBehaviour
{
    public static Game_Controller Instance;

    [Header("Scene_References")]
    [SerializeField] private Transform PC_Spawn_Point;
    [SerializeField] private UI_Manager UI_Manager_Ref;

    private const float Next_Question_Delay = 1.5f;
    private const float Bonus_Auto_Rotation_Speed = 20f;   // градусов в секунду
    private const int Hint_Error_Threshold = 3;
    private const int Highlight_Error_Threshold = 5;

    // Целевые компоненты вопросов по эпохам.
    // Порядок и количество ДОЛЖНЫ совпадать с Localization_DB.Questions.Q и Localization_DB.Questions.Hints.
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

        // Эпоха IV (разборный ПК)
        new[]
        {
            Component_Type.Case_Cover,
            Component_Type.CPU,
            Component_Type.RAM,
            Component_Type.GPU,
            Component_Type.Hard_Drive
        },

        // Эпоха V (разборный ПК)
        new[]
        {
            Component_Type.Case_Cover,
            Component_Type.Cooler,
            Component_Type.RAM,
            Component_Type.GPU,
            Component_Type.SSD_NVMe
        }
    };

    private int Current_Era_Index;
    private int Current_Question_Index;
    private int Error_Count_On_Current_Question;

    // Статистика: Total_Questions_Asked - сколько вопросов игрок прошёл,
    // Total_Correct_Answers - на скольких из них ответил с первой попытки (без ошибок)
    private int Total_Questions_Asked;
    private int Total_Correct_Answers;

    private GameObject Current_PC_Model;
    private Era_Level_Data Current_Era_Data;
    private List<Era_Level_Data> All_Eras_Data;

    private bool Is_Case_Open;
    private bool Is_Bonus_Model_Active;
    private bool Is_Interaction_Active;

    public bool Is_Interaction_Enabled => Is_Interaction_Active;

    // true, пока на экране теория эпохи (она показывается ПОСЛЕ заданий)
    public bool Is_Theory_Phase { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize_Game_Data();
        UI_Manager_Ref.Show_Main_Menu();
    }

    private void Update()
    {
        if (Current_PC_Model == null) return;

        if (Input.GetMouseButton(1))
        {
            float Rotation_X = Input.GetAxis("Mouse X") * 5f;
            Current_PC_Model.transform.Rotate(Vector3.up, -Rotation_X, Space.World);
        }
        else if (Is_Bonus_Model_Active)
        {
            // Бонусная модель медленно вращается сама, как на витрине
            Current_PC_Model.transform.Rotate(Vector3.up, Bonus_Auto_Rotation_Speed * Time.deltaTime, Space.World);
        }
    }

    // Показать 3D-модель выбранного производителя (Intel, AMD, Nvidia) на бонусном уровне
    public void Show_Bonus_Model(string Vendor_Name)
    {
        Clear_Current_PC_Model();

        Current_Era_Data = null;
        Is_Interaction_Active = false;

        Current_PC_Model = Procedural_PC_Models.Create_Bonus_Model(Vendor_Name, PC_Spawn_Point);
        Is_Bonus_Model_Active = Current_PC_Model != null;
    }

    public void Clear_Bonus_Model()
    {
        if (!Is_Bonus_Model_Active)
            return;

        Clear_Current_PC_Model();
    }

    public void Start_New_Game()
    {
        CancelInvoke();
        Clear_Current_PC_Model();

        Save_Manager.Reset_Data();
        Current_Era_Index = 0;
        Total_Questions_Asked = 0;
        Total_Correct_Answers = 0;
        Is_Interaction_Active = false;
        Is_Theory_Phase = false;

        UI_Manager_Ref.Show_Main_Intro();
    }

    public void Start_Era_Level(int Era_Index)
    {
        CancelInvoke(nameof(Show_Current_Question));

        Current_Era_Index = Era_Index;
        Current_Question_Index = 0;
        Error_Count_On_Current_Question = 0;
        Is_Case_Open = false;
        Is_Interaction_Active = false;
        Is_Theory_Phase = false;

        if (Current_PC_Model != null)
            Destroy(Current_PC_Model);

        Current_PC_Model = Procedural_PC_Models.Create_Era_Model(Era_Index, PC_Spawn_Point);
        Current_Era_Data = All_Eras_Data[Era_Index];

        Language Current_Language = Localization_Manager.Instance.Current_Language;

        UI_Manager_Ref.Show_Level_Intro_And_Theory(
            Localization_DB.Era.Titles[Era_Index].Get(Current_Language),
            Localization_DB.Era.Intro[Era_Index].Get(Current_Language),
            string.Empty   // теория появится только после заданий
        );
    }

    public void Begin_Questions_Phase()
    {
        Show_Current_Question();
    }

    private void Clear_Current_PC_Model()
    {
        if (Current_PC_Model != null)
        {
            Destroy(Current_PC_Model);
            Current_PC_Model = null;
        }

        Is_Bonus_Model_Active = false;
    }

    private void Show_Current_Question()
    {
        Language Current_Language = Localization_Manager.Instance.Current_Language;

        if (Current_Question_Index < Current_Era_Data.Questions.Count)
        {
            Is_Interaction_Active = true;

            Question_Data Q_Data = Current_Era_Data.Questions[Current_Question_Index];
            UI_Manager_Ref.Show_Question(Q_Data.Question_Text.Get(Current_Language));
        }
        else
        {
            Is_Interaction_Active = false;

            Save_Manager.Save_Progress(Current_Era_Index + 1, Total_Questions_Asked, Total_Correct_Answers);

            Show_Theory_Phase();
        }
    }

    // Теория эпохи: показывается после того, как ученик выполнил все задания
    private void Show_Theory_Phase()
    {
        Is_Theory_Phase = true;

        Language Current_Language = Localization_Manager.Instance.Current_Language;

        UI_Manager_Ref.Show_Level_Intro_And_Theory(
            Localization_DB.Era.Titles[Current_Era_Index].Get(Current_Language),
            Localization_DB.Gameplay.Theory_Header.Get(Current_Language),
            Localization_DB.Era.Theory[Current_Era_Index].Get(Current_Language)
        );
    }

    // Вызывается кнопкой «Продолжить» на экране теории: переход к следующей эпохе или к итогам
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

    // Оставлено публичным на случай, если метод привязан к кнопке в сцене
    public void Toggle_Case_Cover()
    {
        Toggle_Case_Cover_Internal();
    }

    // Возвращает true, если после переключения крышка открыта
    private bool Toggle_Case_Cover_Internal()
    {
        if (Current_PC_Model == null) return false;

        Openable_Cover Cover = Current_PC_Model.GetComponentInChildren<Openable_Cover>();
        if (Cover == null) return false;

        Cover.Toggle();
        Is_Case_Open = Cover.Is_Open;

        return Cover.Is_Open;
    }

    public void On_Component_Clicked(Interactive_Component Component)
    {
        if (Current_Era_Data == null || Current_PC_Model == null)
            return;

        bool Has_Question = Current_Question_Index < Current_Era_Data.Questions.Count;
        Question_Data Current_Q_Data = Has_Question ? Current_Era_Data.Questions[Current_Question_Index] : null;

        // Клик по крышке корпуса: открыть/закрыть.
        // Если текущее задание - «открой корпус», то открытие засчитывается как верный ответ.
        if (Component.Component_Type == Component_Type.Case_Cover)
        {
            bool Is_Open_Now = Toggle_Case_Cover_Internal();

            if (Is_Open_Now && Has_Question && Current_Q_Data.Target_Component == Component_Type.Case_Cover)
                Register_Correct_Answer();

            return;
        }

        if (!Has_Question)
            return;

        // Съёмная деталь (если у модели есть разборные элементы)
        Detachable_Part Part = Component.GetComponentInParent<Detachable_Part>();

        if (Component.Component_Type == Current_Q_Data.Target_Component)
        {
            if (Part != null)
                Part.Set_Detached(true);

            Register_Correct_Answer();
        }
        else
        {
            if (Part != null)
                Part.Toggle();

            Register_Wrong_Answer(Current_Q_Data);
        }
    }

    private void Register_Correct_Answer()
    {
        Language Current_Language = Localization_Manager.Instance.Current_Language;

        // Каждый вопрос учитывается ровно один раз (в момент верного ответа),
        // но «правильным» засчитывается только ответ с первой попытки
        Total_Questions_Asked++;

        if (Error_Count_On_Current_Question == 0)
            Total_Correct_Answers++;

        Error_Count_On_Current_Question = 0;

        // Блокируем клики на время паузы, иначе можно «ответить» на следующий вопрос до его показа
        Is_Interaction_Active = false;

        Reset_All_Highlights();
        UI_Manager_Ref.Close_Hint_Popup();
        UI_Manager_Ref.Show_Feedback(true, Localization_DB.Gameplay.Correct.Get(Current_Language));

        Current_Question_Index++;
        Invoke(nameof(Show_Current_Question), Next_Question_Delay);
    }

    private void Register_Wrong_Answer(Question_Data Current_Q_Data)
    {
        Language Current_Language = Localization_Manager.Instance.Current_Language;

        Error_Count_On_Current_Question++;

        UI_Manager_Ref.Show_Feedback(false, Localization_DB.Gameplay.Wrong.Get(Current_Language));

        if (Error_Count_On_Current_Question == Hint_Error_Threshold)
        {
            UI_Manager_Ref.Show_Hint_Popup(Current_Q_Data.Hint_Text.Get(Current_Language));
        }
        else if (Error_Count_On_Current_Question >= Highlight_Error_Threshold)
        {
            Highlight_Target_Component(Current_Q_Data.Target_Component);
            UI_Manager_Ref.Show_Hint_Popup(Localization_DB.Gameplay.Highlight.Get(Current_Language));
        }
    }

    private void Highlight_Target_Component(Component_Type Type)
    {
        if (Current_PC_Model == null) return;

        foreach (Interactive_Component Comp in Current_PC_Model.GetComponentsInChildren<Interactive_Component>())
        {
            if (Comp.Component_Type == Type)
                Comp.Highlight(true);
        }
    }

    private void Reset_All_Highlights()
    {
        if (Current_PC_Model == null) return;

        foreach (Interactive_Component Comp in Current_PC_Model.GetComponentsInChildren<Interactive_Component>())
            Comp.Highlight(false);
    }

    private void Initialize_Game_Data()
    {
        All_Eras_Data = new List<Era_Level_Data>();

        for (int Era = 0; Era < Era_Targets.Length; Era++)
        {
            Component_Type[] Targets = Era_Targets[Era];
            Localized_Text[] Texts = Localization_DB.Questions.Q[Era];
            Localized_Text[] Hints = Localization_DB.Questions.Hints[Era];

            if (Texts.Length != Targets.Length || Hints.Length != Targets.Length)
            {
                Debug.LogError(
                    $"Эпоха {Era}: не совпадает число целей ({Targets.Length}), вопросов ({Texts.Length}) и подсказок ({Hints.Length})."
                );
            }

            int Count = Mathf.Min(Targets.Length, Mathf.Min(Texts.Length, Hints.Length));

            Era_Level_Data Data = new Era_Level_Data
            {
                Era_Index = Era,
                Era_Title = Localization_DB.Era.Titles[Era],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[Era],
                Theory_Card_Text = Localization_DB.Era.Theory[Era],
                Questions = new List<Question_Data>()
            };

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

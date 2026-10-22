using System.Collections.Generic;
using UnityEngine;

public class Game_Controller : MonoBehaviour
{
    public static Game_Controller Instance;

    [Header("Scene_References")]
    [SerializeField] private Transform PC_Spawn_Point;
    [SerializeField] private UI_Manager UI_Manager_Ref;

    private int Current_Era_Index;
    private int Current_Question_Index;
    private int Error_Count_On_Current_Question;

    private int Total_Questions_Asked;
    private int Total_Correct_Answers;

    private GameObject Current_PC_Model;
    private Era_Level_Data Current_Era_Data;
    private List<Era_Level_Data> All_Eras_Data;

    private bool Is_Case_Open;
    private bool Is_Interaction_Active;

    public bool Is_Interaction_Enabled => Is_Interaction_Active;

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
    }

    public void Start_New_Game()
    {
        Save_Manager.Reset_Data();
        Current_Era_Index = 0;
        Total_Questions_Asked = 0;
        Total_Correct_Answers = 0;
        UI_Manager_Ref.Show_Main_Intro();
    }

    public void Start_Era_Level(int Era_Index)
    {
        Current_Era_Index = Era_Index;
        Current_Question_Index = 0;
        Error_Count_On_Current_Question = 0;
        Is_Case_Open = false;
        Is_Interaction_Active = false;

        if (Current_PC_Model != null)
            Destroy(Current_PC_Model);

        Current_PC_Model = Procedural_PC_Models.Create_Era_Model(Era_Index, PC_Spawn_Point);
        Current_Era_Data = All_Eras_Data[Era_Index];

        Language Current_Language = Localization_Manager.Instance.Current_Language;

        UI_Manager_Ref.Show_Level_Intro_And_Theory(
            Localization_DB.Era.Titles[Era_Index].Get(Current_Language),
            Localization_DB.Era.Intro[Era_Index].Get(Current_Language),
            Localization_DB.Era.Theory[Era_Index].Get(Current_Language)
        );
    }

    public void Begin_Questions_Phase()
    {
        Is_Interaction_Active = true;
        Show_Current_Question();
    }

    private void Clear_Current_PC_Model()
    {
        if (Current_PC_Model != null)
        {
            Destroy(Current_PC_Model);
            Current_PC_Model = null;
        }
    }

    private void Show_Current_Question()
    {
        Language Current_Language = Localization_Manager.Instance.Current_Language;

        if (Current_Question_Index < Current_Era_Data.Questions.Count)
        {
            Question_Data Q_Data = Current_Era_Data.Questions[Current_Question_Index];
            UI_Manager_Ref.Show_Question(Q_Data.Question_Text.Get(Current_Language));
        }
        else
        {
            Save_Manager.Save_Progress(Current_Era_Index + 1, Total_Questions_Asked, Total_Correct_Answers);

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
    }

    public void Toggle_Case_Cover()
    {
        if (Current_PC_Model == null) return;

        Transform Cover = Current_PC_Model.transform.Find("SideCover");
        if (Cover == null) return;

        Is_Case_Open = !Is_Case_Open;
        Cover.gameObject.SetActive(!Is_Case_Open);
    }

    public void On_Component_Clicked(Interactive_Component Component)
    {
        Language Current_Language = Localization_Manager.Instance.Current_Language;

        if (Component.Component_Type == Component_Type.Case_Cover)
        {
            Toggle_Case_Cover();
            return;
        }

        if (Current_Era_Data == null || Current_Question_Index >= Current_Era_Data.Questions.Count)
            return;

        Question_Data Current_Q_Data = Current_Era_Data.Questions[Current_Question_Index];

        if (Component.Component_Type == Current_Q_Data.Target_Component)
        {
            Total_Questions_Asked++;
            Total_Correct_Answers++;
            Error_Count_On_Current_Question = 0;

            Reset_All_Highlights();

            UI_Manager_Ref.Show_Feedback(true, Localization_DB.Gameplay.Correct.Get(Current_Language));

            Current_Question_Index++;
            Invoke(nameof(Show_Current_Question), 1.5f);
        }
        else
        {
            Error_Count_On_Current_Question++;

            UI_Manager_Ref.Show_Feedback(false, Localization_DB.Gameplay.Wrong.Get(Current_Language));

            if (Error_Count_On_Current_Question == 3)
            {
                UI_Manager_Ref.Show_Hint_Popup(Current_Q_Data.Hint_Text.Get(Current_Language));
            }
            else if (Error_Count_On_Current_Question >= 5)
            {
                Highlight_Target_Component(Current_Q_Data.Target_Component);
                UI_Manager_Ref.Show_Hint_Popup(Localization_DB.Gameplay.Highlight.Get(Current_Language));
            }
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
        All_Eras_Data = new List<Era_Level_Data>
        {
            new Era_Level_Data
            {
                Era_Index = 0,
                Era_Title = Localization_DB.Era.Titles[0],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[0],
                Theory_Card_Text = Localization_DB.Era.Theory[0],
                Questions = new List<Question_Data>
                {
                    new Question_Data
                    {
                        Question_Text = Localization_DB.Questions.Q[0],
                        Target_Component = Component_Type.Vacuum_Tube,
                        Hint_Text = Localization_DB.Questions.Hints[0]
                    }
                }
            },

            new Era_Level_Data
            {
                Era_Index = 1,
                Era_Title = Localization_DB.Era.Titles[1],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[1],
                Theory_Card_Text = Localization_DB.Era.Theory[1],
                Questions = new List<Question_Data>
                {
                    new Question_Data
                    {
                        Question_Text = Localization_DB.Questions.Q[1],
                        Target_Component = Component_Type.Storage,
                        Hint_Text = Localization_DB.Questions.Hints[1]
                    }
                }
            },

            new Era_Level_Data
            {
                Era_Index = 2,
                Era_Title = Localization_DB.Era.Titles[2],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[2],
                Theory_Card_Text = Localization_DB.Era.Theory[2],
                Questions = new List<Question_Data>
                {
                    new Question_Data
                    {
                        Question_Text = Localization_DB.Questions.Q[2],
                        Target_Component = Component_Type.Storage,
                        Hint_Text = Localization_DB.Questions.Hints[2]
                    }
                }
            },

            new Era_Level_Data
            {
                Era_Index = 3,
                Era_Title = Localization_DB.Era.Titles[3],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[3],
                Theory_Card_Text = Localization_DB.Era.Theory[3],
                Questions = new List<Question_Data>
                {
                    new Question_Data
                    {
                        Question_Text = Localization_DB.Questions.Q[3],
                        Target_Component = Component_Type.GPU,
                        Hint_Text = Localization_DB.Questions.Hints[3]
                    }
                }
            },

            new Era_Level_Data
            {
                Era_Index = 4,
                Era_Title = Localization_DB.Era.Titles[4],
                Intro_Cutscene_Text = Localization_DB.Era.Intro[4],
                Theory_Card_Text = Localization_DB.Era.Theory[4],
                Questions = new List<Question_Data>
                {
                    new Question_Data
                    {
                        Question_Text = Localization_DB.Questions.Q[4],
                        Target_Component = Component_Type.Storage,
                        Hint_Text = Localization_DB.Questions.Hints[4]
                    }
                }
            }
        };
    }
}
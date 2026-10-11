using System.Collections.Generic;
using UnityEngine;

// Главный контроллер игровой логики, управления уровнями и вопросами
public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public Transform PC_Spawn_Point;
    public UIManager UI_Manager_Ref;

    private int Current_Era_Index = 0;
    private int Current_Question_Index = 0;
    private int Error_Count_On_Current_Question = 0;

    private int Total_Questions_Asked = 0;
    private int Total_Correct_Answers = 0;

    private GameObject Current_PC_Model;
    private EraLevelData Current_Era_Data;
    private List<EraLevelData> All_Eras_Data;
    private bool Is_Case_Open = false;
    private bool Is_Interaction_Active = false; // Флаг разрешения кликов по деталям

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
        // Вращение 3D-модели ПК с помощью зажатия правой кнопки мыши
        if (Current_PC_Model != null && Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * 5f;
            Current_PC_Model.transform.Rotate(Vector3.up, -rotX, Space.World);
        }
    }

    public void Start_New_Game()
    {
        SaveManager.Reset_Data();
        Current_Era_Index = 0;
        Total_Questions_Asked = 0;
        Total_Correct_Answers = 0;
        UI_Manager_Ref.Show_Main_Intro();
    }

    public void Start_Era_Level(int eraIndex)
    {
        Current_Era_Index = eraIndex;
        Current_Question_Index = 0;
        Error_Count_On_Current_Question = 0;
        Is_Case_Open = false;
        Is_Interaction_Active = false; // Кликать по деталям во время теории нельзя

        if (Current_PC_Model != null) Destroy(Current_PC_Model);
        Current_PC_Model = ProceduralPCModels.Create_Era_Model(eraIndex, PC_Spawn_Point);

        Current_Era_Data = All_Eras_Data[eraIndex];

        UI_Manager_Ref.Show_Level_Intro_And_Theory(
            Current_Era_Data.Era_Title.Get_Text(LocalizationManager.Instance.Current_Language),
            Current_Era_Data.Intro_Cutscene_Text.Get_Text(LocalizationManager.Instance.Current_Language),
            Current_Era_Data.Theory_Card_Text.Get_Text(LocalizationManager.Instance.Current_Language)
        );
    }

    public void Begin_Questions_Phase()
    {
        Is_Interaction_Active = true; // Разрешаем клики после закрытия теории
        Show_Current_Question();
    }

    private void Show_Current_Question()
    {
        if (Current_Question_Index < Current_Era_Data.Questions.Count)
        {
            var q = Current_Era_Data.Questions[Current_Question_Index];
            UI_Manager_Ref.Show_Question(q.Question_Text.Get_Text(LocalizationManager.Instance.Current_Language));
        }
        else
        {
            // Переход к следующему уровню или завершение игры
            SaveManager.Save_Progress(Current_Era_Index + 1, Total_Questions_Asked, Total_Correct_Answers);

            if (Current_Era_Index + 1 < All_Eras_Data.Count)
            {
                Start_Era_Level(Current_Era_Index + 1);
            }
            else
            {
                UI_Manager_Ref.Show_Game_Completion_Screen(SaveManager.Get_Accuracy_Percentage());
            }
        }
    }

    public void Toggle_Case_Cover()
    {
        if (Current_PC_Model == null) return;
        Transform cover = Current_PC_Model.transform.Find("SideCover");
        if (cover != null)
        {
            Is_Case_Open = !Is_Case_Open;
            cover.gameObject.SetActive(!Is_Case_Open);
        }
    }

    public void On_Component_Clicked(InteractiveComponent component)
    {
        // Открытие/закрытие крышки корпуса по клику
        if (component.Component_Type == ComponentType.CaseCover)
        {
            Toggle_Case_Cover();
            return;
        }

        if (Current_Era_Data == null || Current_Question_Index >= Current_Era_Data.Questions.Count) return;

        QuestionData currentQData = Current_Era_Data.Questions[Current_Question_Index];
        Language lang = LocalizationManager.Instance.Current_Language;

        if (component.Component_Type == currentQData.Target_Component)
        {
            // Обработка правильного ответа
            Total_Questions_Asked++;
            Total_Correct_Answers++;
            Error_Count_On_Current_Question = 0;

            Reset_All_Highlights();

            string correctMsg = (lang == Language.RU) ? "Верно!" : "Correct!";
            UI_Manager_Ref.Show_Feedback(true, correctMsg);
            Current_Question_Index++;
            Invoke(nameof(Show_Current_Question), 1.5f);
        }
        else
        {
            // Обработка ошибки
            Error_Count_On_Current_Question++;
            string wrongMsg = (lang == Language.RU) ? "Не тот компонент. Попробуй еще раз." : "Wrong component. Try again.";
            UI_Manager_Ref.Show_Feedback(false, wrongMsg);

            if (Error_Count_On_Current_Question == 3)
            {
                UI_Manager_Ref.Show_Hint_Popup(currentQData.Hint_Text.Get_Text(lang));
            }
            else if (Error_Count_On_Current_Question >= 5)
            {
                Highlight_Target_Component(currentQData.Target_Component);
                string highlightMsg = (lang == Language.RU) ? "Правильный элемент подсвечен желтым цветом!" : "The correct element is highlighted in yellow!";
                UI_Manager_Ref.Show_Hint_Popup(highlightMsg);
            }
        }
    }

    private void Highlight_Target_Component(ComponentType type)
    {
        foreach (var comp in Current_PC_Model.GetComponentsInChildren<InteractiveComponent>())
        {
            if (comp.Component_Type == type)
            {
                comp.Highlight(true);
            }
        }
    }

    private void Reset_All_Highlights()
    {
        if (Current_PC_Model == null) return;
        foreach (var comp in Current_PC_Model.GetComponentsInChildren<InteractiveComponent>())
        {
            comp.Highlight(false);
        }
    }

    private void Initialize_Game_Data()
    {
        // Инициализация базы данных уровней, вопросов и теоретической информации
        All_Eras_Data = new List<EraLevelData>
        {
            new EraLevelData
            {
                Era_Index = 0,
                Era_Title = new LocalizedText { RU = "I Поколение (1943–1955): Ламповые ЭВМ", EN = "Era I (1943–1955): Vacuum Tube Computers" },
                Intro_Cutscene_Text = new LocalizedText { RU = "Добро пожаловать в 1950-е годы! Компьютеры занимают целые комнаты.", EN = "Welcome to the 1950s! Computers occupy whole rooms." },
                Theory_Card_Text = new LocalizedText { RU = "В первых ЭВМ вместо транзисторов использовались электронно-вакуумные лампы.", EN = "First computers used vacuum tubes." },
                Questions = new List<QuestionData>
                {
                    new QuestionData {
                        Question_Text = new LocalizedText { RU = "Найди главный переключающий элемент 1-го поколения ЭВМ.", EN = "Find the main switching element of 1st gen computers." },
                        Target_Component = ComponentType.VacuumTube,
                        Hint_Text = new LocalizedText { RU = "Ищи стеклянные колбы, похожие на лампочки.", EN = "Look for glass bulbs similar to lightbulbs." }
                    }
                }
            },
            new EraLevelData
            {
                Era_Index = 1,
                Era_Title = new LocalizedText { RU = "II Поколение (1955–1965): Транзисторы", EN = "Era II (1955–1965): Transistors" },
                Intro_Cutscene_Text = new LocalizedText { RU = "Эра транзисторов! Компьютеры стали надежнее и меньше.", EN = "The transistor era! Computers became faster and smaller." },
                Theory_Card_Text = new LocalizedText { RU = "Транзисторы заменили лампы. Появились магнитные ленты.", EN = "Transistors replaced vacuum tubes. Magnetic tapes appeared." },
                Questions = new List<QuestionData>
                {
                    new QuestionData {
                        Question_Text = new LocalizedText { RU = "Укажи на устройство для хранения данных на магнитной ленте.", EN = "Point to the magnetic tape storage device." },
                        Target_Component = ComponentType.Storage,
                        Hint_Text = new LocalizedText { RU = "Оно похоже на круглые катушки или бобины.", EN = "It looks like round tape reels." }
                    }
                }
            },
            new EraLevelData
            {
                Era_Index = 2,
                Era_Title = new LocalizedText { RU = "III Поколение (1965–1980): Интегральные схемы", EN = "Era III (1965–1980): Integrated Circuits" },
                Intro_Cutscene_Text = new LocalizedText { RU = "Микросхемы меняют всё. Появляются первые персональные ПК.", EN = "Integrated circuits change everything. Personal microcomputers appear." },
                Theory_Card_Text = new LocalizedText { RU = "Интегральная схема объединяет тысячи транзисторов на кремнии.", EN = "Integrated circuits pack thousands of transistors on silicon." },
                Questions = new List<QuestionData>
                {
                    new QuestionData {
                        Question_Text = new LocalizedText { RU = "Найди дисковод для гибких дискет 5.25\".", EN = "Find the drive for 5.25\" floppy disks." },
                        Target_Component = ComponentType.Storage,
                        Hint_Text = new LocalizedText { RU = "Ищи узкую щель на передней панели корпуса.", EN = "Look for a narrow slot on the front panel." }
                    }
                }
            },
            new EraLevelData
            {
                Era_Index = 3,
                Era_Title = new LocalizedText { RU = "IV Поколение (1980–2000): Эра Микропроцессоров", EN = "Era IV (1980–2000): Microprocessors" },
                Intro_Cutscene_Text = new LocalizedText { RU = "ПК есть в каждом доме! Появились 3D-ускорители.", EN = "PCs in every home! 3D graphics cards are born." },
                Theory_Card_Text = new LocalizedText { RU = "СБИС позволили разместить весь процессор на одном чипе.", EN = "VLSI allowed the whole CPU to fit onto a single microchip." },
                Questions = new List<QuestionData>
                {
                    new QuestionData {
                        Question_Text = new LocalizedText { RU = "Открой корпус и найди плату, отвечающую за 3D-графику.", EN = "Open the case and find the card responsible for 3D graphics." },
                        Target_Component = ComponentType.GPU,
                        Hint_Text = new LocalizedText { RU = "Кликни по планке в слоте AGP/PCI.", EN = "Click the AGP/PCI card." }
                    }
                }
            },
            new EraLevelData
            {
                Era_Index = 4,
                Era_Title = new LocalizedText { RU = "V Поколение (С 2000-х): Современные станции", EN = "Era V (2000s–Present): Modern Workstations" },
                Intro_Cutscene_Text = new LocalizedText { RU = "Многоядерность, прозрачные корпуса и сверхбыстрые накопители.", EN = "Multi-core CPUs, glass cases and high-speed NVMe storage." },
                Theory_Card_Text = new LocalizedText { RU = "Современные ПК используют сверхбыстрые накопители NVMe SSD.", EN = "Modern PCs rely on NVMe SSDs." },
                Questions = new List<QuestionData>
                {
                    new QuestionData {
                        Question_Text = new LocalizedText { RU = "Найди современный сверхбыстрый накопитель M.2 NVMe SSD.", EN = "Find the fast modern M.2 NVMe SSD." },
                        Target_Component = ComponentType.Storage,
                        Hint_Text = new LocalizedText { RU = "Маленькая планочка, прикрученная к материнской плате.", EN = "A small flat stick mounted directly onto the motherboard." }
                    }
                }
            }
        };
    }
}
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    public Transform pcSpawnPoint;
    public UIManager uiManager;

    private int currentEraIndex = 0;
    private int currentQuestionIndex = 0;
    private int errorCountOnCurrentQuestion = 0;
    
    private int totalQuestionsAsked = 0;
    private int totalCorrectAnswers = 0;

    private GameObject currentPCModel;
    private EraLevelData currentEraData;
    private List<EraLevelData> allErasData;
    private bool isCaseOpen = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        InitializeGameData();
        uiManager.ShowMainMenu();
    }

    private void Update()
    {
        // Вращение модели ПК зажатием правой кнопки мыши
        if (currentPCModel != null && Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * 5f;
            currentPCModel.transform.Rotate(Vector3.up, -rotX, Space.World);
        }
    }

    public void StartNewGame()
    {
        SaveManager.ResetData();
        currentEraIndex = 0;
        totalQuestionsAsked = 0;
        totalCorrectAnswers = 0;
        uiManager.ShowMainIntro();
    }

    public void LoadSavedGame()
    {
        currentEraIndex = SaveManager.GetSavedLevel();
        StartEraLevel(currentEraIndex);
    }

    public void StartEraLevel(int eraIndex)
    {
        currentEraIndex = eraIndex;
        currentQuestionIndex = 0;
        errorCountOnCurrentQuestion = 0;
        isCaseOpen = false;

        if (currentPCModel != null) Destroy(currentPCModel);
        currentPCModel = ProceduralPCModels.CreateEraModel(eraIndex, pcSpawnPoint);

        currentEraData = allErasData[eraIndex];
        
        // Сначала показываем интро уровня и теорию
        uiManager.ShowLevelIntroAndTheory(
            currentEraData.eraTitle.Get(LocalizationManager.Instance.CurrentLanguage),
            currentEraData.introCutsceneText.Get(LocalizationManager.Instance.CurrentLanguage),
            currentEraData.theoryCardText.Get(LocalizationManager.Instance.CurrentLanguage)
        );
    }

    public void BeginQuestionsPhase()
    {
        ShowCurrentQuestion();
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex < currentEraData.questions.Count)
        {
            var q = currentEraData.questions[currentQuestionIndex];
            uiManager.ShowQuestion(q.questionText.Get(LocalizationManager.Instance.CurrentLanguage));
        }
        else
        {
            // Уровень пройден!
            SaveManager.SaveProgress(currentEraIndex + 1, totalQuestionsAsked, totalCorrectAnswers);
            
            if (currentEraIndex + 1 < allErasData.Count)
            {
                StartEraLevel(currentEraIndex + 1);
            }
            else
            {
                uiManager.ShowGameCompletionScreen(SaveManager.GetAccuracyPercentage());
            }
        }
    }

    public void ToggleCaseCover()
    {
        if (currentPCModel == null) return;
        Transform cover = currentPCModel.transform.Find("SideCover");
        if (cover != null)
        {
            isCaseOpen = !isCaseOpen;
            cover.gameObject.SetActive(!isCaseOpen);
        }
    }

    public void OnComponentClicked(InteractiveComponent component)
    {
        if (currentEraData == null || currentQuestionIndex >= currentEraData.questions.Count) return;

        QuestionData currentQ = currentEraData.questions[currentQuestionIndex];

        if (component.componentType == currentQ.targetComponent)
        {
            // ПРАВИЛЬНЫЙ ОТВЕТ
            totalQuestionsAsked++;
            totalCorrectAnswers++;
            errorCountOnCurrentQuestion = 0; // Обнуляем счетчик ошибок
            
            // Снимаем подсветки
            ResetAllHighlights();

            uiManager.ShowFeedback(true, "Верно! / Correct!");
            currentQuestionIndex++;
            Invoke(nameof(ShowCurrentQuestion), 1.5f);
        }
        else
        {
            // ОШИБКА
            errorCountOnCurrentQuestion++;
            uiManager.ShowFeedback(false, "Не тот компонент. Попробуй еще раз.");

            // 3 ошибки -> Подсказка
            if (errorCountOnCurrentQuestion == 3)
            {
                uiManager.ShowHintPopUp(currentQ.hintText.Get(LocalizationManager.Instance.CurrentLanguage));
            }
            // 5 ошибок -> Подсветка правильного предмета
            else if (errorCountOnCurrentQuestion >= 5)
            {
                HighlightTargetComponent(currentQ.targetComponent);
                uiManager.ShowHintPopUp("Правильный элемент подсвечен желтым цветом!");
            }
        }
    }

    private void HighlightTargetComponent(ComponentType type)
    {
        foreach (var comp in currentPCModel.GetComponentsInChildren<InteractiveComponent>())
        {
            if (comp.componentType == type)
            {
                comp.Highlight(true);
            }
        }
    }

    private void ResetAllHighlights()
    {
        if (currentPCModel == null) return;
        foreach (var comp in currentPCModel.GetComponentsInChildren<InteractiveComponent>())
        {
            comp.Highlight(false);
        }
    }

    private void InitializeGameData()
    {
        // База данных уровней, вопросов и теории
        allErasData = new List<EraLevelData>
        {
            // ЭПОХА 1
            new EraLevelData
            {
                eraIndex = 0,
                eraTitle = new LocalizedText { RU = "I Поколение (1943–1955): Ламповые ЭВМ", EN = "Era I (1943–1955): Vacuum Tube Computers" },
                introCutsceneText = new LocalizedText { RU = "Добро пожаловать в 1950-е годы! Компьютеры занимают целые комнаты и выделяют много тепла.", EN = "Welcome to the 1950s! Computers occupy whole rooms and emit huge heat." },
                theoryCardText = new LocalizedText { RU = "В первых ЭВМ вместо транзисторов использовались электронно-вакуумные лампы. Они часто перегорали, а данные вводились с помощью перфокарт.", EN = "First computers used vacuum tubes. They burned out frequently, and data was entered via punch cards." },
                questions = new List<QuestionData>
                {
                    new QuestionData {
                        questionText = new LocalizedText { RU = "Найди главный переключающий элемент 1-го поколения ЭВМ.", EN = "Find the main switching element of 1st gen computers." },
                        targetComponent = ComponentType.VacuumTube,
                        hintText = new LocalizedText { RU = "Ищи стеклянные колбы, похожие на лампочки.", EN = "Look for glass bulbs similar to lightbulbs." }
                    }
                }
            },
            // ЭПОХА 2
            new EraLevelData
            {
                eraIndex = 1,
                eraTitle = new LocalizedText { RU = "II Поколение (1955–1965): Транзисторы", EN = "Era II (1955–1965): Transistors" },
                introCutsceneText = new LocalizedText { RU = "Эра транзисторов! Компьютеры стали надежнее, меньше и быстрее.", EN = "The transistor era! Computers became faster and smaller." },
                theoryCardText = new LocalizedText { RU = "Транзисторы заменили лампы. Появились магнитные ленты для длительного хранения информации.", EN = "Transistors replaced vacuum tubes. Magnetic tapes appeared for storage." },
                questions = new List<QuestionData>
                {
                    new QuestionData {
                        questionText = new LocalizedText { RU = "Укажи на устройство, используемое для хранения данных на магнитной ленте.", EN = "Point to the magnetic tape storage device." },
                        targetComponent = ComponentType.Storage,
                        hintText = new LocalizedText { RU = "Оно похоже на круглые катушки или бобины.", EN = "It looks like round tape reels." }
                    }
                }
            },
            // ЭПОХА 3
            new EraLevelData
            {
                eraIndex = 2,
                eraTitle = new LocalizedText { RU = "III Поколение (1965–1980): Интегральные схемы", EN = "Era III (1965–1980): Integrated Circuits" },
                introCutsceneText = new LocalizedText { RU = "Микросхемы меняют всё. Появляются первые персональные ПК для энтузиастов.", EN = "Integrated circuits change everything. Personal microcomputers appear." },
                theoryCardText = new LocalizedText { RU = "Интегральная схема объединяет тысячи транзисторов на одном маленьком кусочке кремния.", EN = "Integrated circuits pack thousands of transistors on a tiny piece of silicon." },
                questions = new List<QuestionData>
                {
                    new QuestionData {
                        questionText = new LocalizedText { RU = "Найди дисковод для сменных гибких дисков (гибких дискет 5.25\").", EN = "Find the drive for 5.25\" floppy disks." },
                        targetComponent = ComponentType.Storage,
                        hintText = new LocalizedText { RU = "Ищи узкую щель на передней панели корпуса.", EN = "Look for a narrow slot on the front panel." }
                    }
                }
            },
            // ЭПОХА 4
            new EraLevelData
            {
                eraIndex = 3,
                eraTitle = new LocalizedText { RU = "IV Поколение (1980–2000): Эра Микропроцессоров", EN = "Era IV (1980–2000): Microprocessors" },
                introCutsceneText = new LocalizedText { RU = "ПК есть в каждом офисе и доме! Появились 3D-ускорители и звуковые карты.", EN = "PCs in every home and office! 3D graphics cards are born." },
                theoryCardText = new LocalizedText { RU = "СБИС (Сверхбольшие интегральные схемы) позволили разместить весь процессор на одном чипе.", EN = "VLSI allowed the whole CPU to fit onto a single microchip." },
                questions = new List<QuestionData>
                {
                    new QuestionData {
                        questionText = new LocalizedText { RU = "Открой корпус и найди отдельную плату, отвечающую за 3D-графику.", EN = "Open the case and find the card responsible for 3D graphics." },
                        targetComponent = ComponentType.GPU,
                        hintText = new LocalizedText { RU = "Нажми на кнопку 'Открыть корпус' и кликни по планке в слоте AGP/PCI.", EN = "Click 'Open Case' button and click the AGP/PCI card." }
                    }
                }
            },
            // ЭПОХА 5
            new EraLevelData
            {
                eraIndex = 4,
                eraTitle = new LocalizedText { RU = "V Поколение (С 2000-х): Современные станции", EN = "Era V (2000s–Present): Modern Workstations" },
                introCutsceneText = new LocalizedText { RU = "Многоядерность, прозрачные корпуса, RGB-подсветка и сверхбыстрые накопители.", EN = "Multi-core CPUs, glass cases, RGB and high-speed NVMe storage." },
                theoryCardText = new LocalizedText { RU = "Современные ПК используют сверхбыстрые накопители NVMe SSD и многоядерные чипы.", EN = "Modern PCs rely on NVMe SSDs and high-performance multi-core CPUs." },
                questions = new List<QuestionData>
                {
                    new QuestionData {
                        questionText = new LocalizedText { RU = "Найди современный сверхбыстрый накопитель M.2 NVMe SSD.", EN = "Find the fast modern M.2 NVMe SSD." },
                        targetComponent = ComponentType.Storage,
                        hintText = new LocalizedText { RU = "Это маленькая плоская планочка, прикрученная прямо к материнской плате.", EN = "It's a small flat stick mounted directly onto the motherboard." }
                    }
                }
            }
        };
    }
}
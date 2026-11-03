// Этот файл хранит ВСЕ данные игры: перечисления, «контейнеры» для данных и базу текстов.
// Если нужно поправить любой текст в игре (вопрос, подсказку, теорию, описание) — это делается здесь.
// Каждый текст хранится сразу на двух языках: русском (RU) и английском (EN).

using System;
using System.Collections.Generic;

// Список языков, на которых умеет работать игра.
// RU — русский, EN — английский. Чтобы добавить язык, нужно добавить его сюда
// и дополнить класс Localized_Text.
public enum Language
{
    RU,
    EN
}

// Типы деталей компьютера, по которым можно кликать.
// Каждой детали 3D-модели присваивается один из этих типов (см. Procedural_PC_Models),
// а у каждого вопроса есть «правильный» тип (см. Era_Targets в Game_Controller).
// Клик засчитывается как верный, если тип детали совпал с типом, который нужно найти.
// Детали с пометкой «отвлекающий элемент» никогда не бывают правильным ответом:
// они нужны, чтобы было из чего выбирать.
public enum Component_Type
{
    // Эпоха I
    Vacuum_Tube,        // Электронные лампы
    Switch_Panel,       // Панель тумблеров
    Patch_Panel,        // Коммутационная панель с кабелями
    Punch_Card_Reader,  // Устройство ввода с перфокарт

    // Эпоха II
    Transistor,         // Платы с транзисторами
    Magnetic_Tape,      // Накопитель на магнитной ленте
    Core_Memory,        // Память на магнитных сердечниках
    Control_Console,    // Пульт оператора

    // Эпоха III
    Integrated_Circuit, // Микросхемы
    Floppy_Drive,       // Дисковод гибких дискет
    Keyboard,           // Клавиатура
    Monitor_CRT,        // ЭЛТ-монитор

    // Эпохи IV–V (разборные ПК)
    CPU,                // Процессор
    RAM,                // Оперативная память
    GPU,                // Видеокарта
    Hard_Drive,         // Жёсткий диск
    SSD_NVMe,           // Накопитель M.2 NVMe
    Cooler,             // Система охлаждения процессора
    CD_Drive,           // Привод компакт-дисков (отвлекающий элемент)
    Sound_Card,         // Звуковая плата (отвлекающий элемент)
    PSU,                // Блок питания (отвлекающий элемент)
    Motherboard,        // Материнская плата и её детали (отвлекающий элемент)
    Case_Cover          // Боковая крышка корпуса
}

// Один текст сразу на двух языках.
// Пример: new Localized_Text("Привет", "Hello").Get(Language.EN) вернёт "Hello".
[Serializable]
public class Localized_Text
{
    public string RU;   // текст на русском
    public string EN;   // текст на английском

    // Создаёт текст сразу с обоими вариантами перевода.
    public Localized_Text(string RU_Text, string EN_Text)
    {
        RU = RU_Text;
        EN = EN_Text;
    }

    // Возвращает вариант текста для выбранного языка.
    public string Get(Language Lang)
    {
        return Lang == Language.RU ? RU : EN;
    }
}

// Данные одного вопроса: что спросить, какую деталь нужно найти и какая подсказка к вопросу.
[Serializable]
public class Question_Data
{
    public Localized_Text Question_Text;      // текст вопроса («Найди…»)
    public Component_Type Target_Component;   // тип детали, по которой нужно кликнуть
    public Localized_Text Hint_Text;          // подсказка, появляется после 3-й ошибки
}

// Данные одной эпохи (уровня): её названия, тексты и список вопросов.
[Serializable]
public class Era_Level_Data
{
    public int Era_Index;                       // номер эпохи (с нуля)
    public Localized_Text Era_Title;            // заголовок эпохи
    public Localized_Text Intro_Cutscene_Text;  // описание эпохи, показывается ДО заданий
    public Localized_Text Theory_Card_Text;     // теория, показывается ПОСЛЕ заданий
    public List<Question_Data> Questions;       // вопросы эпохи по порядку
}

// Главная база текстов игры. Разбита на группы: интерфейс, сообщения во время игры,
// бонус, данные эпох, вопросы и подсказки, итоговый экран.
public static class Localization_DB
{
    // Надписи на кнопках и экранах интерфейса.
    public static class UI
    {
        // Кнопка главного меню «Начать игру».
        public static readonly Localized_Text New_Game =
            new Localized_Text("Начать Игру", "Start Game");

        // Кнопка главного меню «Бонус».
        public static readonly Localized_Text Bonus =
            new Localized_Text("Бонус", "Bonus");

        // Кнопка смены языка: показывает название ДРУГОГО языка (на какой можно переключиться).
        public static readonly Localized_Text Language =
            new Localized_Text("English", "Русский");

        // Текст стартовой вкладки (поддерживает разметку TextMeshPro)
        public static readonly Localized_Text Main_Intro =
            new Localized_Text(
                "<size=130%><b>Компьютер сквозь века</b></size>\n\n" +
                "Информатику можно изучать не только по учебнику. Эта игра — путешествие по пяти поколениям компьютеров: от ламповых машин 1940-х годов до современных ПК.\n\n" +
                "Здесь всё устроено необычно: <b>сначала задача, потом теория</b>. В каждой эпохе ты сам исследуешь компьютер: вращай модель правой кнопкой мыши, кликай по деталям и находи нужные. Только после этого ты узнаешь, что это за устройства и зачем они нужны.\n\n" +
                "Ошибаться не страшно — подсказки помогут. Отвечай точно, и откроется бонусный уровень!",

                "<size=130%><b>PC Through the Ages</b></size>\n\n" +
                "Computer science can be learned not only from a textbook. This game is a journey through five generations of computers, from the vacuum tube machines of the 1940s to modern PCs.\n\n" +
                "Here things work differently: <b>first the task, then the theory</b>. In each era you explore the computer yourself: rotate the model with the right mouse button, click on parts and find the ones you need. Only then will you learn what these devices are and why they matter.\n\n" +
                "Mistakes are fine — hints will help. Answer accurately and a bonus level will unlock!"
            );

        // Кнопка «Начать» на экране с описанием игры.
        public static readonly Localized_Text Start_Intro =
            new Localized_Text("Начать", "Start");

        // Кнопка «Продолжить» на экране эпохи (до заданий и после теории).
        public static readonly Localized_Text Continue =
            new Localized_Text("Продолжить", "Continue");

        // Кнопка «Закрыть» во всплывающей подсказке.
        public static readonly Localized_Text Close =
            new Localized_Text("Закрыть", "Close");

        // Кнопка «Главное меню» на итоговом экране.
        public static readonly Localized_Text Return_To_Menu =
            new Localized_Text("Главное меню", "Main Menu");
    }

    // Сообщения, которые игрок видит во время заданий.
    public static class Gameplay
    {
        // Появляется зелёным цветом после верного ответа.
        public static readonly Localized_Text Correct =
            new Localized_Text("Верно!", "Correct!");

        // Появляется красным цветом после неверного клика.
        public static readonly Localized_Text Wrong =
            new Localized_Text("Не тот компонент. Попробуй еще раз.", "Wrong component. Try again.");

        // Заголовок экрана теории (показывается после выполнения заданий эпохи)
        public static readonly Localized_Text Theory_Header =
            new Localized_Text(
                "Теперь, когда задания выполнены, — теория:",
                "Now that the tasks are done, here is the theory:"
            );

        // Сообщение, когда после 5 ошибок правильная деталь подсвечена жёлтым.
        public static readonly Localized_Text Highlight =
            new Localized_Text(
                "Правильный элемент подсвечен жёлтым цветом!",
                "The correct element is highlighted in yellow!"
            );
    }

    // Тексты бонусного уровня (выбор производителя процессора или видеокарты).
    public static class Bonus
    {
        // Заголовок над кнопками Intel / AMD / Nvidia.
        public static readonly Localized_Text Title =
            new Localized_Text("Выберите производителя:", "Choose a manufacturer:");

        // Развёрнутые тексты бонуса (показываются под 3D-моделью, поддерживают разметку TextMeshPro)
        public static readonly Localized_Text Intel =
            new Localized_Text(
                "<size=130%><b>Intel — процессор Core</b></size>\n" +
                "Intel — отличный и надёжный выбор для работы и вычислений! Процессор — «мозг» компьютера: он выполняет команды программ миллиарды раз в секунду. " +
                "Первый коммерческий микропроцессор Intel 4004 (1971) содержал около 2300 транзисторов, а в современных их десятки миллиардов. " +
                "Покрути модель: на обороте видны сотни золотых контактов, которыми процессор соединяется с материнской платой.",

                "<size=130%><b>Intel — Core processor</b></size>\n" +
                "Intel is an excellent and reliable choice! The processor is the \"brain\" of a computer: it executes program instructions billions of times per second. " +
                "The first commercial microprocessor, the Intel 4004 (1971), had about 2,300 transistors, while modern ones have tens of billions. " +
                "Rotate the model: on the back you can see hundreds of gold contacts that connect the processor to the motherboard."
            );

        public static readonly Localized_Text AMD =
            new Localized_Text(
                "<size=130%><b>AMD — процессор Ryzen</b></size>\n" +
                "AMD — великолепный баланс цены и многопоточной мощности! Ryzen собирают по чиплетной схеме: процессор состоит из нескольких маленьких кристаллов, " +
                "поэтому в нём легко сделать много ядер — у серверных EPYC их больше сотни. " +
                "Чем больше ядер, тем больше задач выполняется одновременно: монтаж видео, игры, трансляции. Покрути модель и рассмотри контакты на обороте.",

                "<size=130%><b>AMD — Ryzen processor</b></size>\n" +
                "AMD is a great balance of price and power! Ryzen is built from chiplets: the processor consists of several small dies, " +
                "which makes it easy to build many cores — server EPYC chips have more than a hundred. " +
                "The more cores, the more tasks run at once: video editing, games, streaming. Rotate the model and look at the contacts on the back."
            );

        public static readonly Localized_Text Nvidia =
            new Localized_Text(
                "<size=130%><b>Nvidia — видеокарта GeForce</b></size>\n" +
                "Nvidia — то что надо для игр, дизайна и ИИ! Графический процессор (GPU) состоит из тысяч небольших ядер, которые считают одновременно, " +
                "поэтому он быстро рисует 3D-картинку и обучает нейросети. Термин GPU популяризировала Nvidia в 1999 году, выпустив GeForce 256. " +
                "Видеокарта сильно греется, поэтому на ней стоят вентиляторы и большой радиатор.",

                "<size=130%><b>Nvidia — GeForce graphics card</b></size>\n" +
                "Nvidia is just what you need for gaming, design and AI! A graphics processor (GPU) consists of thousands of small cores that calculate at the same time, " +
                "so it quickly draws 3D images and trains neural networks. Nvidia popularised the term GPU in 1999 with the GeForce 256. " +
                "A graphics card gets very hot, so it carries fans and a large heatsink."
            );

        // Мелкая подсказка под текстом бонуса о том, как вернуться в меню.
        public static readonly Localized_Text Return_Hint =
            new Localized_Text(
                "Нажми Esc или пробел, чтобы вернуться в меню",
                "Press Esc or Space to return to the menu"
            );
    }

    // Тексты эпох. В каждом списке ровно 5 элементов: по одному на эпоху (индекс 0 — первая эпоха).
    public static class Era
    {
        // Заголовки эпох (показываются сверху экрана эпохи).
        public static readonly Localized_Text[] Titles =
        {
            new Localized_Text(
                "I Поколение (1943–1955): Ламповые ЭВМ",
                "Era I (1943–1955): Vacuum Tube Computers"
            ),

            new Localized_Text(
                "II Поколение (1955–1965): Транзисторы",
                "Era II (1955–1965): Transistors"
            ),

            new Localized_Text(
                "III Поколение (1965–1980): Интегральные схемы",
                "Era III (1965–1980): Integrated Circuits"
            ),

            new Localized_Text(
                "IV Поколение (1980–2000): Эра Микропроцессоров",
                "Era IV (1980–2000): Microprocessors"
            ),

            new Localized_Text(
                "V Поколение (С 2000-х): Современные станции",
                "Era V (2000s–Present): Modern Workstations"
            )
        };

        // Описание эпохи ДО заданий: где ученик оказался и что ему нужно сделать.
        // Теория здесь намеренно не раскрывается — она будет после заданий.
        public static readonly Localized_Text[] Intro =
        {
            new Localized_Text(
                "Ты в лаборатории 1940-х годов. Перед тобой гигантская вычислительная машина размером с комнату. Осмотри её (правая кнопка мыши вращает модель) и выполни задания: найди главные узлы. Не знаешь, что это? Пробуй угадать — теория будет после заданий.",
                "You are in a 1940s laboratory. In front of you is a giant computer the size of a room. Examine the machine (the right mouse button rotates it) and complete the tasks to find its main parts. Not sure what something is? Take a guess — the theory comes after the tasks."
            ),

            new Localized_Text(
                "Конец 1950-х. Лампы уходят в прошлое: перед тобой компьютер нового поколения — быстрее, надёжнее и меньше прежнего. Осмотри его и выполни задания. Теорию узнаешь после них.",
                "The late 1950s. Vacuum tubes are fading away: in front of you is a new generation of computer — faster, more reliable and smaller. Examine it and complete the tasks. The theory comes afterwards."
            ),

            new Localized_Text(
                "Конец 1970-х. Компьютеры выходят из машинных залов: теперь это настольная машина, за которой работает один человек. Осмотри её и найди нужные устройства. Теорию узнаешь после заданий.",
                "The late 1970s. Computers are leaving the machine halls: this is now a desktop machine that one person can use. Examine it and find the devices you need. The theory comes after the tasks."
            ),

            new Localized_Text(
                "1990-е годы: персональный компьютер есть в каждом доме и школе. Его корпус открывается! Сначала открой крышку — при открытом корпусе клик по детали выдвигает её, чтобы ты мог её рассмотреть. Теорию узнаешь после заданий.",
                "The 1990s: a personal computer in every home and school. Its case opens! Start by opening the cover — with the case open, clicking a part slides it out so you can take a closer look. The theory comes after the tasks."
            ),

            new Localized_Text(
                "Наше время. Перед тобой современный ПК с прозрачной боковой панелью и подсветкой. Открой корпус и найди нужные компоненты — многие из них ты уже встречал в прошлых эпохах.",
                "Present day. In front of you is a modern PC with a transparent side panel and lighting. Open the case and find the right components — you have already met many of them in earlier eras."
            )
        };

        // Теория эпохи ПОСЛЕ заданий. Рассчитана на школьников 7 класса и старше.
        // Символ \n внутри строки означает перенос на новый абзац.
        public static readonly Localized_Text[] Theory =
        {
            new Localized_Text(
                "Первые электронные вычислительные машины (ЭВМ), например ENIAC (1945), строились на электронных лампах. Лампа работает как переключатель: пропускает или не пропускает ток — это «1» или «0». Ламп были тысячи, они сильно нагревались и часто перегорали.\n" +
                "Данные вводили с перфокарт — картонных карточек с отверстиями, а программу задавали тумблерами и перестановкой кабелей на коммутационной панели. Перенастройка занимала дни.",
                "The first electronic computers, such as ENIAC (1945), were built on vacuum tubes. A tube works as a switch: it lets current pass or blocks it — that is a \"1\" or a \"0\". There were thousands of tubes, and they got very hot and burned out often.\n" +
                "Data was entered from punched cards — cardboard cards with holes — and the program was set with switches and by re-plugging cables on a patch panel. Reconfiguring the machine took days."
            ),

            new Localized_Text(
                "Лампы заменили транзисторы — маленькие полупроводниковые переключатели. Они почти не греются, потребляют мало энергии и редко выходят из строя, поэтому компьютеры стали меньше, быстрее и дешевле.\n" +
                "Оперативной памятью служили магнитные сердечники — крошечные ферритовые колечки, каждое хранило 1 бит. Для длительного хранения использовали магнитную ленту: доступ к данным на ней последовательный, чтобы найти нужное, ленту приходилось перематывать.",
                "Vacuum tubes were replaced by transistors — small semiconductor switches. They hardly heat up, use little power and rarely fail, so computers became smaller, faster and cheaper.\n" +
                "Main memory was made of magnetic cores — tiny ferrite rings, each storing 1 bit. Magnetic tape was used for long-term storage: data on it is accessed sequentially, so the tape had to be rewound to find what you needed."
            ),

            new Localized_Text(
                "Интегральная схема (микросхема) — это кристалл кремния, на котором размещены сотни и тысячи соединённых транзисторов. Благодаря ей компьютеры стали компактными, и появились первые персональные компьютеры.\n" +
                "Привычные устройства тоже родом из этой эпохи: клавиатура для ввода, монитор на электронно-лучевой трубке для вывода и гибкие дискеты для хранения и переноса данных.",
                "An integrated circuit (microchip) is a silicon crystal carrying hundreds or thousands of connected transistors. Thanks to it, computers became compact and the first personal computers appeared.\n" +
                "Familiar devices come from this era too: the keyboard for input, the cathode-ray tube monitor for output and floppy disks for storing and moving data."
            ),

            new Localized_Text(
                "Все части ПК соединяет материнская плата. Процессор — «мозг» компьютера: он выполняет команды программ. Сверхбольшие интегральные схемы (СБИС) позволили уместить его в одном чипе — микропроцессоре.\n" +
                "Оперативная память (ОЗУ) хранит данные и программы, пока компьютер включён, а при выключении очищается. Жёсткий диск хранит информацию постоянно. Видеокарта (3D-ускоритель) формирует изображение на экране — именно она сделала возможными компьютерные игры.",
                "All parts of a PC are connected by the motherboard. The processor is the \"brain\" of the computer: it executes program instructions. Very large-scale integration (VLSI) made it possible to fit it on a single chip — the microprocessor.\n" +
                "Random-access memory (RAM) holds data and programs while the computer is on and is cleared when it is switched off. The hard drive stores information permanently. The graphics card (3D accelerator) creates the image on the screen — it made computer games possible."
            ),

            new Localized_Text(
                "Современный процессор состоит из нескольких ядер, которые выполняют задачи одновременно. Он сильно нагревается, поэтому ему нужна система охлаждения — воздушная или жидкостная.\n" +
                "Видеокарта с собственным мощным процессором (GPU) нужна играм, графике и ИИ. Вместо жёстких дисков с вращающимися пластинами всё чаще ставят SSD: в них нет движущихся частей. Накопители NVMe формата M.2 подключаются прямо к материнской плате и работают намного быстрее жёстких дисков.",
                "A modern processor has several cores that perform tasks at the same time. It gets very hot, so it needs a cooling system — air or liquid.\n" +
                "A graphics card with its own powerful processor (GPU) is needed for games, graphics and AI. Instead of hard drives with spinning platters, SSDs are used more and more: they have no moving parts. M.2 NVMe drives plug straight into the motherboard and are much faster than hard drives."
            )
        };
    }

    // Вопросы и подсказки. Это «список списков»: Q[2][1] — второй вопрос третьей эпохи.
    // ВАЖНО: порядок и количество вопросов должны совпадать с таблицей Era_Targets в Game_Controller.
    public static class Questions
    {
        // Тексты вопросов («Найди…»).
        public static readonly Localized_Text[][] Q =
        {
            // Эпоха I: лампы, тумблеры, коммутационная панель, перфокарты
            new[]
            {
                new Localized_Text(
                    "Найди главный переключающий элемент 1-го поколения ЭВМ.",
                    "Find the main switching element of 1st gen computers."
                ),
                new Localized_Text(
                    "Найди панель с тумблерами, на которой оператор вручную задавал числа и режимы.",
                    "Find the switch panel where the operator set numbers and modes by hand."
                ),
                new Localized_Text(
                    "Найди коммутационную панель: для смены программы операторы переставляли на ней кабели.",
                    "Find the patch panel: operators changed the program by re-plugging cables on it."
                ),
                new Localized_Text(
                    "Найди устройство ввода, считывающее данные с перфокарт.",
                    "Find the input device that reads data from punched cards."
                )
            },

            // Эпоха II: транзисторы, лента, ферритовая память, пульт
            new[]
            {
                new Localized_Text(
                    "Найди платы с транзисторами — они пришли на смену лампам.",
                    "Find the transistor boards — transistors replaced vacuum tubes."
                ),
                new Localized_Text(
                    "Укажи на устройство для хранения данных на магнитной ленте.",
                    "Point to the magnetic tape storage device."
                ),
                new Localized_Text(
                    "Найди память на магнитных сердечниках.",
                    "Find the magnetic core memory."
                ),
                new Localized_Text(
                    "Найди пульт оператора с лампочками и переключателями.",
                    "Find the operator's console with lamps and switches."
                )
            },

            // Эпоха III: микросхемы, дискета, клавиатура, монитор
            new[]
            {
                new Localized_Text(
                    "Найди интегральную схему (микросхему) на плате.",
                    "Find the integrated circuit (microchip) on the board."
                ),
                new Localized_Text(
                    "Найди дисковод для гибких дискет 5.25\".",
                    "Find the drive for 5.25\" floppy disks."
                ),
                new Localized_Text(
                    "Найди клавиатуру.",
                    "Find the keyboard."
                ),
                new Localized_Text(
                    "Найди монитор с электронно-лучевой трубкой (ЭЛТ).",
                    "Find the CRT monitor."
                )
            },

            // Эпоха IV: разборный ПК — крышка, процессор, ОЗУ, видеокарта, жёсткий диск
            new[]
            {
                new Localized_Text(
                    "Открой корпус: кликни по боковой крышке системного блока.",
                    "Open the case: click the side cover of the system unit."
                ),
                new Localized_Text(
                    "Найди центральный процессор: он закрыт радиатором с вентилятором.",
                    "Find the CPU: it is covered by a heatsink with a fan."
                ),
                new Localized_Text(
                    "Найди модули оперативной памяти.",
                    "Find the RAM modules."
                ),
                new Localized_Text(
                    "Найди плату, отвечающую за 3D-графику.",
                    "Find the card responsible for 3D graphics."
                ),
                new Localized_Text(
                    "Найди жёсткий диск (HDD).",
                    "Find the hard disk drive (HDD)."
                )
            },

            // Эпоха V: разборный ПК — стекло, охлаждение, ОЗУ, видеокарта, NVMe
            new[]
            {
                new Localized_Text(
                    "Открой корпус: кликни по стеклянной боковой панели.",
                    "Open the case: click the glass side panel."
                ),
                new Localized_Text(
                    "Найди систему охлаждения процессора.",
                    "Find the CPU cooling system."
                ),
                new Localized_Text(
                    "Найди модули оперативной памяти с подсветкой.",
                    "Find the RAM modules with lighting."
                ),
                new Localized_Text(
                    "Найди видеокарту.",
                    "Find the graphics card."
                ),
                new Localized_Text(
                    "Найди современный сверхбыстрый накопитель M.2 NVMe SSD.",
                    "Find the fast modern M.2 NVMe SSD."
                )
            }
        };

        // Подсказки к вопросам (показываются после 3-й ошибки). Порядок тот же, что у вопросов.
        public static readonly Localized_Text[][] Hints =
        {
            // Эпоха I
            new[]
            {
                new Localized_Text(
                    "Ищи стеклянные колбы, похожие на лампочки.",
                    "Look for glass bulbs similar to lightbulbs."
                ),
                new Localized_Text(
                    "Ряды маленьких тумблеров на светлой панели в центральном шкафу.",
                    "Rows of small toggle switches on the light panel in the middle cabinet."
                ),
                new Localized_Text(
                    "Тёмная панель с гнёздами и цветными проводами.",
                    "A dark panel with sockets and coloured cords."
                ),
                new Localized_Text(
                    "Приземистый блок справа со стопкой картонных карточек.",
                    "A squat unit on the right with a stack of cardboard cards."
                )
            },

            // Эпоха II
            new[]
            {
                new Localized_Text(
                    "Ряд зелёных плат с маленькими металлическими баночками.",
                    "A row of green boards with small metal cans."
                ),
                new Localized_Text(
                    "Оно похоже на круглые катушки или бобины.",
                    "It looks like round tape reels."
                ),
                new Localized_Text(
                    "Рамка с сеткой крошечных колечек, нанизанных на тонкие провода.",
                    "A frame with a grid of tiny rings strung on thin wires."
                ),
                new Localized_Text(
                    "Наклонная панель внизу центрального шкафа с рядами цветных лампочек.",
                    "A sloped panel at the bottom of the central cabinet with rows of coloured lamps."
                )
            },

            // Эпоха III
            new[]
            {
                new Localized_Text(
                    "Чёрные прямоугольные корпуса с рядами металлических ножек.",
                    "Black rectangular packages with rows of metal pins."
                ),
                new Localized_Text(
                    "Ищи узкую щель на передней панели корпуса.",
                    "Look for a narrow slot on the front panel."
                ),
                new Localized_Text(
                    "Плоская панель с рядами клавиш перед системным блоком.",
                    "A flat panel with rows of keys in front of the main unit."
                ),
                new Localized_Text(
                    "Массивный бежевый корпус с тёмно-зелёным экраном.",
                    "A bulky beige case with a dark green screen."
                )
            },

            // Эпоха IV
            new[]
            {
                new Localized_Text(
                    "Крышка — большая панель на боковой стороне корпуса. Если не видно, поверни модель правой кнопкой мыши.",
                    "The cover is the large panel on the side of the case. If you can't see it, rotate the model with the right mouse button."
                ),
                new Localized_Text(
                    "Квадратный радиатор с маленьким вентилятором на материнской плате.",
                    "A square heatsink with a small fan on the motherboard."
                ),
                new Localized_Text(
                    "Узкие вертикальные планки с чёрными микросхемами рядом с процессором.",
                    "Narrow vertical sticks with black chips next to the processor."
                ),
                new Localized_Text(
                    "Широкая плата с радиатором и маленьким вентилятором — не путай со звуковой.",
                    "A wide card with a heatsink and a small fan — don't confuse it with the sound card."
                ),
                new Localized_Text(
                    "Серебристая металлическая коробка в нижней передней части корпуса.",
                    "A silver metal box in the lower front part of the case."
                )
            },

            // Эпоха V
            new[]
            {
                new Localized_Text(
                    "Панель прозрачная и голубоватая. Если её не видно, поверни модель правой кнопкой мыши.",
                    "The panel is transparent and bluish. If you can't see it, rotate the model with the right mouse button."
                ),
                new Localized_Text(
                    "Круглая помпа с подсветкой и две трубки, идущие к радиатору.",
                    "A round pump with lighting and two tubes leading to the radiator."
                ),
                new Localized_Text(
                    "Две вертикальные планки с яркими светящимися полосами.",
                    "Two vertical sticks with bright glowing strips."
                ),
                new Localized_Text(
                    "Самая крупная плата с тремя вентиляторами.",
                    "The largest card with three fans."
                ),
                new Localized_Text(
                    "Маленькая планочка, прикрученная к материнской плате.",
                    "A small flat stick mounted directly onto the motherboard."
                )
            }
        };
    }

    // Тексты итогового экрана после прохождения всех эпох.
    public static class Game_Completion
    {
        // Первая строка итогов.
        public static readonly Localized_Text Finished =
            new Localized_Text(
                "Игра завершена!",
                "Game completed!"
            );

        // Подпись перед процентом точности («Точность ответов: 85.0%»).
        public static readonly Localized_Text Accuracy =
            new Localized_Text(
                "Точность ответов",
                "Accuracy"
            );

        // Сообщение, если точность достаточна и бонус открыт.
        public static readonly Localized_Text Bonus_Unlocked =
            new Localized_Text(
                "Бонусный уровень разблокирован в Главном Меню!",
                "Bonus level unlocked in Main Menu!"
            );

        // Сообщение, если ошибок было слишком много и бонус не открылся.
        public static readonly Localized_Text Bonus_Locked =
            new Localized_Text(
                "Слишком много ошибок для разблокировки бонуса.",
                "Too many mistakes to unlock the bonus level."
            );
    }
}

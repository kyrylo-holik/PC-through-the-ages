using System;
using System.Collections.Generic;
using UnityEngine;

// Перечисление поддерживаемых языков интерфейса
public enum Language
{
    RU,
    EN
}

// Перечисление типов интерактивных компонентов компьютера
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

// Класс для хранения локализованного текста
[Serializable]
public class Localized_Text
{
    public string RU;
    public string EN;

    public Localized_Text(string RU_Text, string EN_Text)
    {
        RU = RU_Text;
        EN = EN_Text;
    }

    public string Get(Language Lang)
    {
        return Lang == Language.RU ? RU : EN;
    }
}

// Класс данных для отдельного вопроса
[Serializable]
public class Question_Data
{
    public Localized_Text Question_Text;
    public Component_Type Target_Component;
    public Localized_Text Hint_Text;
}

// Класс данных для уровня определенной эпохи
[Serializable]
public class Era_Level_Data
{
    public int Era_Index;
    public Localized_Text Era_Title;
    public Localized_Text Intro_Cutscene_Text;
    public Localized_Text Theory_Card_Text;
    public List<Question_Data> Questions;
}

// Главная база локализации
public static class Localization_DB
{
    // UI элементы
    public static class UI
    {
        public static readonly Localized_Text New_Game =
            new Localized_Text("Начать Игру", "Start Game");

        public static readonly Localized_Text Bonus =
            new Localized_Text("Бонус", "Bonus");

        public static readonly Localized_Text Language =
            new Localized_Text("English", "Русский");

        public static readonly Localized_Text Main_Intro =
            new Localized_Text("Игра", "Game");

        public static readonly Localized_Text Start_Intro =
            new Localized_Text("Начать", "Start");

        public static readonly Localized_Text Continue =
            new Localized_Text("Продолжить", "Continue");

        public static readonly Localized_Text Close =
            new Localized_Text("Закрыть", "Close");

        public static readonly Localized_Text Return_To_Menu =
            new Localized_Text("Главное меню", "Main Menu");
    }

    // Геймплейные сообщения
    public static class Gameplay
    {
        public static readonly Localized_Text Correct =
            new Localized_Text("Верно!", "Correct!");

        public static readonly Localized_Text Wrong =
            new Localized_Text("Не тот компонент. Попробуй еще раз.", "Wrong component. Try again.");

        public static readonly Localized_Text Highlight =
            new Localized_Text(
                "Правильный элемент подсвечен жёлтым цветом!",
                "The correct element is highlighted in yellow!"
            );
    }

    // Бонусный уровень
    public static class Bonus
    {
        public static readonly Localized_Text Title =
            new Localized_Text("Выберите производителя:", "Choose a manufacturer:");

        public static readonly Localized_Text Intel =
            new Localized_Text(
                "Intel — отличный и надёжный выбор для работы и вычислений!",
                "Intel is an excellent and reliable choice!"
            );

        public static readonly Localized_Text AMD =
            new Localized_Text(
                "AMD — великолепный баланс цены и многопоточной мощности!",
                "AMD is a great balance of price and power!"
            );

        public static readonly Localized_Text Nvidia =
            new Localized_Text(
                "Nvidia — то что надо для игр, дизайна и ИИ!",
                "Nvidia is just what you need for gaming and AI!"
            );
    }

    // Данные эпох
    public static class Era
    {
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

        public static readonly Localized_Text[] Intro =
        {
            new Localized_Text(
                "Добро пожаловать в 1950-е годы! Компьютеры занимают целые комнаты.",
                "Welcome to the 1950s! Computers occupy whole rooms."
            ),

            new Localized_Text(
                "Эра транзисторов! Компьютеры стали надёжнее и меньше.",
                "The transistor era! Computers became faster and smaller."
            ),

            new Localized_Text(
                "Микросхемы меняют всё. Появляются первые персональные ПК.",
                "Integrated circuits change everything. Personal microcomputers appear."
            ),

            new Localized_Text(
                "ПК есть в каждом доме! Появились 3D-ускорители.",
                "PCs in every home! 3D graphics cards are born."
            ),

            new Localized_Text(
                "Многоядерность, прозрачные корпуса и сверхбыстрые накопители.",
                "Multi-core CPUs, glass cases and high-speed NVMe storage."
            )
        };

        public static readonly Localized_Text[] Theory =
        {
            new Localized_Text(
                "В первых ЭВМ вместо транзисторов использовались электронно-вакуумные лампы.",
                "First computers used vacuum tubes."
            ),

            new Localized_Text(
                "Транзисторы заменили лампы. Появились магнитные ленты.",
                "Transistors replaced vacuum tubes. Magnetic tapes appeared."
            ),

            new Localized_Text(
                "Интегральная схема объединяет тысячи транзисторов на кремнии.",
                "Integrated circuits pack thousands of transistors on silicon."
            ),

            new Localized_Text(
                "СБИС позволили разместить весь процессор на одном чипе.",
                "VLSI allowed the whole CPU to fit onto a single microchip."
            ),

            new Localized_Text(
                "Современные ПК используют сверхбыстрые накопители NVMe SSD.",
                "Modern PCs rely on NVMe SSDs."
            )
        };
    }

    // Вопросы и подсказки: индекс 1 — номер эпохи, индекс 2 — номер вопроса внутри эпохи.
    // Порядок вопросов должен совпадать с Era_Targets в Game_Controller.
    public static class Questions
    {
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

    // Сообщения о завершении игры
    public static class Game_Completion
    {
        public static readonly Localized_Text Finished =
            new Localized_Text(
                "Игра завершена!",
                "Game completed!"
            );

        public static readonly Localized_Text Accuracy =
            new Localized_Text(
                "Точность ответов",
                "Accuracy"
            );

        public static readonly Localized_Text Bonus_Unlocked =
            new Localized_Text(
                "Бонусный уровень разблокирован в Главном Меню!",
                "Bonus level unlocked in Main Menu!"
            );

        public static readonly Localized_Text Bonus_Locked =
            new Localized_Text(
                "Слишком много ошибок для разблокировки бонуса.",
                "Too many mistakes to unlock the bonus level."
            );
    }
}

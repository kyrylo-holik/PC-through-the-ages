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
    Vacuum_Tube,     // Лампы (Эпоха 1)
    Transistor,      // Транзисторы (Эпоха 2)
    Storage,         // Накопитель (Лента, дискета, SSD)
    CPU,             // Процессор / логический блок
    GPU,             // Видеосистема
    Case_Cover       // Крышка корпуса
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

    // Вопросы и подсказки
    public static class Questions
    {
        public static readonly Localized_Text[] Q =
        {
            new Localized_Text(
                "Найди главный переключающий элемент 1-го поколения ЭВМ.",
                "Find the main switching element of 1st gen computers."
            ),

            new Localized_Text(
                "Укажи на устройство для хранения данных на магнитной ленте.",
                "Point to the magnetic tape storage device."
            ),

            new Localized_Text(
                "Найди дисковод для гибких дискет 5.25\".",
                "Find the drive for 5.25\" floppy disks."
            ),

            new Localized_Text(
                "Открой корпус и найди плату, отвечающую за 3D-графику.",
                "Open the case and find the card responsible for 3D graphics."
            ),

            new Localized_Text(
                "Найди современный сверхбыстрый накопитель M.2 NVMe SSD.",
                "Find the fast modern M.2 NVMe SSD."
            )
        };

        public static readonly Localized_Text[] Hints =
        {
            new Localized_Text(
                "Ищи стеклянные колбы, похожие на лампочки.",
                "Look for glass bulbs similar to lightbulbs."
            ),

            new Localized_Text(
                "Оно похоже на круглые катушки или бобины.",
                "It looks like round tape reels."
            ),

            new Localized_Text(
                "Ищи узкую щель на передней панели корпуса.",
                "Look for a narrow slot on the front panel."
            ),

            new Localized_Text(
                "Кликни по планке в слоте AGP/PCI.",
                "Click the AGP/PCI card."
            ),

            new Localized_Text(
                "Маленькая планочка, прикрученная к материнской плате.",
                "A small flat stick mounted directly onto the motherboard."
            )
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
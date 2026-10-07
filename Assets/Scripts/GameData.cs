using System;
using System.Collections.Generic;
using UnityEngine;

public enum Language { RU, EN }

public enum ComponentType
{
    VacuumTube,    // Лампы (1 эпоха)
    Transistor,    // Транзисторы / Перфокарты (2 эпоха)
    RAM,           // ОЗУ
    Storage,       // Накопитель (Магнитная лента / Дискета / HDD / SSD)
    CPU,           // Процессор / Микропроцессор
    GPU,           // Видеосистема
    PowerSupply,   // Блок питания
    Motherboard,   // Материнская плата
    Peripheral     // Периферия (Клавиатура/Монитор)
}

[Serializable]
public class LocalizedText
{
    [TextArea(2, 5)] public string RU;
    [TextArea(2, 5)] public string EN;

    public string Get(Language lang) => lang == Language.RU ? RU : EN;
}

[Serializable]
public class QuestionData
{
    public LocalizedText questionText;
    public ComponentType targetComponent;
    public LocalizedText hintText;
}

[Serializable]
public class EraLevelData
{
    public int eraIndex;
    public LocalizedText eraTitle;
    public LocalizedText introCutsceneText; // Текст перед уровнем
    public LocalizedText theoryCardText;   // Теоретический блок
    public List<QuestionData> questions;
}
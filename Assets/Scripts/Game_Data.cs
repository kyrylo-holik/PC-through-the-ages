using System;
using System.Collections.Generic;
using UnityEngine;

// Перечисление поддерживаемых языков интерфейса
public enum Language { RU, EN }

// Перечисление типов интерактивных компонентов компьютера (неиспользуемые RAM, Motherboard, PowerSupply удалены)
public enum ComponentType
{
    VacuumTube,    // Лампы (Эпоха 1)
    Transistor,    // Транзисторы (Эпоха 2)
    Storage,       // Накопитель (Лента, дискета, SSD)
    CPU,           // Процессор / логический блок
    GPU,           // Видеосистема
    CaseCover      // Крышка корпуса
}

// Класс для хранения локализованного текста на русском и английском языках
[Serializable]
public class LocalizedText
{
    [TextArea(2, 5)] public string RU;
    [TextArea(2, 5)] public string EN;

    // Метод получения текста для текущего языка
    public string Get_Text(Language lang) => lang == Language.RU ? RU : EN;
}

// Класс данных для отдельного вопроса на уровне
[Serializable]
public class QuestionData
{
    public LocalizedText Question_Text;
    public ComponentType Target_Component;
    public LocalizedText Hint_Text;
}

// Класс данных для уровня определенной исторической эпохи
[Serializable]
public class EraLevelData
{
    public int Era_Index;
    public LocalizedText Era_Title;
    public LocalizedText Intro_Cutscene_Text; // Вступительный текст перед уровнем
    public LocalizedText Theory_Card_Text;   // Теоретическая справка
    public List<QuestionData> Questions;
}
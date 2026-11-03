using System.Collections.Generic;
using UnityEngine;

// Процедурные 3D-модели: все компьютеры собираются прямо из кода из простых фигур
// (кубов, цилиндров и шаров), никаких файлов моделей в проекте нет.
//
// Как читать этот файл:
//   - Вверху — служебные «инструменты» (создать куб, покрасить, сгруппировать).
//   - Дальше — по одному методу на каждую эпоху (Create_Era_1…5) и на каждую бонусную модель.
//   - В методах эпох координаты подписаны так: X — вправо, Y — вверх, Z — «вглубь экрана».
//     Лицевая сторона моделей эпох I–III смотрит на камеру, то есть в сторону -Z.
//
// Все координаты ЛОКАЛЬНЫЕ (относительно корня модели), поэтому положение PC_Spawn_Point,
// например (1,1,1), не ломает внутреннюю сборку модели.
//
// Эпохи IV–V — башенные корпуса: фронтальная панель по -X, открываемая боковая крышка по -Z,
// материнская плата закреплена у дальней стенки (+Z), все платы торчат из неё в сторону -Z.
// Съёмные детали выдвигаются тоже в сторону -Z, то есть наружу через открытую боковину.
public class Procedural_PC_Models : MonoBehaviour
{
    // На сколько единиц выдвигается съёмная деталь (в сторону -Z, к игроку).
    private static readonly Vector3 Part_Detach_Offset = new Vector3(0f, 0f, -0.9f);

    // Куда поднята бонусная модель относительно центра. Поднята, чтобы снизу осталось место под текст.
    private static readonly Vector3 Bonus_Model_Offset = new Vector3(0f, 0.45f, 0f);

    // Сколько материалов с одним и тем же цветом уже создано. Детали одного цвета
    // используют один общий материал — так память и нагрузка на видеокарту меньше.
    private static readonly Dictionary<Color, Material> Material_Cache = new Dictionary<Color, Material>();

    // ======================================================================
    // Главные методы: создание модели целиком
    // ======================================================================

    // Создаёт модель компьютера для эпохи с номером Era_Index (0…4) внутри объекта Parent.
    public static GameObject Create_Era_Model(int Era_Index, Transform Parent)
    {
        GameObject Root = new GameObject($"Computer_Era_{Era_Index}");
        Root.transform.SetParent(Parent, false);

        switch (Era_Index)
        {
            case 0:
                Create_Era_1_ENIAC(Root.transform);
                break;

            case 1:
                Create_Era_2_Mainframe(Root.transform);
                break;

            case 2:
                Create_Era_3_Microcomputer(Root.transform);
                Root.transform.localScale = Vector3.one * 1.15f;   // чуть увеличиваем, модель компактная
                break;

            case 3:
                Create_Era_4_Retro_PC(Root.transform);
                // Разворачиваем корпус, чтобы сразу были видны и фронт, и боковая крышка.
                Root.transform.localRotation = Quaternion.Euler(0f, -30f, 0f);
                break;

            case 4:
                Create_Era_5_Modern_PC(Root.transform);
                Root.transform.localRotation = Quaternion.Euler(0f, -30f, 0f);
                break;
        }

        return Root;
    }

    // Создаёт бонусную модель ("Intel", "AMD" или "Nvidia"). Для другого названия вернёт null.
    public static GameObject Create_Bonus_Model(string Vendor_Name, Transform Parent)
    {
        GameObject Root = new GameObject($"Bonus_Model_{Vendor_Name}");
        Root.transform.SetParent(Parent, false);
        Root.transform.localPosition = Bonus_Model_Offset;
        Root.transform.localScale = Vector3.one * 0.9f;

        switch (Vendor_Name)
        {
            case "Intel":
                Create_Bonus_Intel(Root.transform);
                break;

            case "AMD":
                Create_Bonus_AMD(Root.transform);
                break;

            case "Nvidia":
                Create_Bonus_Nvidia(Root.transform);
                break;

            default:
                Destroy(Root);
                return null;
        }

        return Root;
    }

    // ======================================================================
    // Инструменты для сборки моделей
    // ======================================================================

    // Короткая запись вектора: V(1, 2, 3) вместо new Vector3(1f, 2f, 3f).
    private static Vector3 V(float X, float Y, float Z)
    {
        return new Vector3(X, Y, Z);
    }

    // Короткая запись цвета: Rgb(красный, зелёный, синий) со значениями от 0 до 1.
    // Четвёртое число — прозрачность (1 = непрозрачный, меньше 1 = полупрозрачный).
    private static Color Rgb(float R, float G, float B, float A = 1f)
    {
        return new Color(R, G, B, A);
    }

    // Создаёт пустой объект-«контейнер», в который можно складывать детали одной группы
    // (например, все части одной лампы). Группу можно двигать и поворачивать целиком.
    // Pos — позиция, Rot — поворот в градусах, Name — имя в иерархии (нужно только для удобства).
    private static GameObject Make_Group(Transform Parent, Vector3 Pos, Vector3 Rot = default, string Name = "Group")
    {
        GameObject Group = new GameObject(Name);
        Group.transform.SetParent(Parent, false);
        Group.transform.localPosition = Pos;
        Group.transform.localRotation = Quaternion.Euler(Rot);
        return Group;
    }

    // Создаёт одну деталь заданной формы. Параметры:
    //   Shape  — форма (куб, цилиндр, шар);
    //   Parent — к чему прикрепить деталь;
    //   Pos    — позиция, Scale — размеры, Rot — поворот в градусах, Paint — цвет;
    //   Type   — если задан, деталь становится КЛИКАБЕЛЬНОЙ и участвует в вопросах и подсветке;
    //   Solid  — деталь без типа, но с «твёрдостью» (стенки корпуса: клик по ним не проходит внутрь).
    // Если деталь не кликабельна и не твёрдая, коллайдер удаляется: клик проходит сквозь неё
    // (мелкие украшения не мешают кликать по деталям за ними).
    private static GameObject Make_Part(PrimitiveType Shape, Transform Parent, Vector3 Pos, Vector3 Scale,
        Color Paint, Component_Type? Type, Vector3 Rot, bool Solid)
    {
        GameObject Obj = GameObject.CreatePrimitive(Shape);
        Obj.transform.SetParent(Parent, false);
        Obj.transform.localPosition = Pos;
        Obj.transform.localRotation = Quaternion.Euler(Rot);
        Obj.transform.localScale = Scale;

        Obj.GetComponent<Renderer>().sharedMaterial = Get_Material(Paint);

        if (Type.HasValue)
        {
            // Делаем деталь кликабельной и запоминаем её тип.
            Obj.AddComponent<Interactive_Component>().Component_Type = Type.Value;
        }
        else if (!Solid)
        {
            // Декоративная деталь: убираем коллайдер, чтобы она не перехватывала клики.
            Collider Col = Obj.GetComponent<Collider>();

            if (Col != null)
                Destroy(Col);
        }

        return Obj;
    }

    // Куб. Размеры задаются в Scale: (ширина, высота, толщина).
    private static GameObject Cube(Transform Parent, Vector3 Pos, Vector3 Scale, Color Paint,
        Component_Type? Type = null, Vector3 Rot = default, bool Solid = false)
    {
        return Make_Part(PrimitiveType.Cube, Parent, Pos, Scale, Paint, Type, Rot, Solid);
    }

    // Цилиндр. Его ось по умолчанию направлена вверх (по Y), а полная высота = 2 * Scale.y.
    //   Rot = (90, 0, 0) поворачивает ось вдоль Z — цилиндр превращается в диск, смотрящий на камеру;
    //   Rot = (0, 0, 90) поворачивает ось вдоль X — цилиндр лежит горизонтально.
    private static GameObject Cylinder(Transform Parent, Vector3 Pos, Vector3 Scale, Color Paint,
        Component_Type? Type = null, Vector3 Rot = default)
    {
        return Make_Part(PrimitiveType.Cylinder, Parent, Pos, Scale, Paint, Type, Rot, false);
    }

    // Шар (Scale — диаметр по трём осям).
    private static GameObject Sphere(Transform Parent, Vector3 Pos, Vector3 Scale, Color Paint,
        Component_Type? Type = null)
    {
        return Make_Part(PrimitiveType.Sphere, Parent, Pos, Scale, Paint, Type, default, false);
    }

    // Возвращает материал нужного цвета. Если такой уже создавали — берёт готовый.
    private static Material Get_Material(Color Paint)
    {
        if (Material_Cache.TryGetValue(Paint, out Material Cached) && Cached != null)
            return Cached;

        Material Created = Create_Material(Paint);
        Material_Cache[Paint] = Created;
        return Created;
    }

    // Создаёт новый материал заданного цвета.
    // Обычный материал CreatePrimitive использует шейдер Standard, который в сборке под URP вырезается
    // (из-за этого модели становились розовыми). Поэтому материал создаётся явно:
    //   1) из шаблонов Resources/PC_Lit_Opaque и Resources/PC_Lit_Transparent
    //      (самый надёжный способ: шейдер гарантированно попадает в сборку);
    //   2) если шаблонов нет — через Shader.Find (URP Lit, затем Standard, затем запасной вариант).
    private static Material Create_Material(Color Paint)
    {
        bool Is_Transparent = Paint.a < 0.99f;
        Material Template = Resources.Load<Material>(Is_Transparent ? "PC_Lit_Transparent" : "PC_Lit_Opaque");
        Material Mat;

        if (Template != null)
        {
            Mat = new Material(Template);
        }
        else
        {
            Shader Found = Shader.Find("Universal Render Pipeline/Lit");

            if (Found == null) Found = Shader.Find("Standard");
            if (Found == null) Found = Shader.Find("Sprites/Default");

            Mat = new Material(Found);

            if (Is_Transparent)
                Make_Transparent(Mat);
        }

        // Красим материал. У разных шейдеров цвет называется по-разному, поэтому задаём оба варианта.
        if (Mat.HasProperty("_BaseColor")) Mat.SetColor("_BaseColor", Paint);
        if (Mat.HasProperty("_Color")) Mat.SetColor("_Color", Paint);

        return Mat;
    }

    // Переключает материал, созданный без шаблона, в режим прозрачности (нужно для стекла и ламп).
    private static void Make_Transparent(Material Mat)
    {
        if (Mat.HasProperty("_Surface"))
        {
            // Шейдер URP Lit.
            Mat.SetFloat("_Surface", 1f);
            Mat.SetFloat("_Blend", 0f);
            Mat.SetFloat("_SrcBlend", 5f);
            Mat.SetFloat("_DstBlend", 10f);
            Mat.SetFloat("_ZWrite", 0f);
            Mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            Mat.SetOverrideTag("RenderType", "Transparent");
        }
        else if (Mat.HasProperty("_Mode"))
        {
            // Шейдер Standard (Built-in).
            Mat.SetFloat("_Mode", 3f);
            Mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            Mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            Mat.SetInt("_ZWrite", 0);
            Mat.EnableKeyword("_ALPHABLEND_ON");
        }

        // 3000 — очередь отрисовки прозрачных объектов (после обычных).
        Mat.renderQueue = 3000;
    }

    // Делает группу деталей съёмной: по клику она будет выдвигаться из корпуса.
    // Вызывать нужно ПОСЛЕ того, как группа поставлена на своё место.
    private static void Make_Detachable(GameObject Part)
    {
        Part.AddComponent<Detachable_Part>().Setup(Part_Detach_Offset);
    }

    // Рисует провод между двумя точками (в плоскости XY): тонкий куб и две круглые «вилки» на концах.
    private static void Create_Cable(Transform T, Vector3 A, Vector3 B, Color Paint, Component_Type Kind)
    {
        Vector3 D = B - A;
        float Angle = Mathf.Atan2(D.y, D.x) * Mathf.Rad2Deg;   // угол наклона провода

        Cube(T, (A + B) * 0.5f, V(D.magnitude, 0.03f, 0.03f), Paint, Kind, V(0f, 0f, Angle));
        Sphere(T, A, V(0.07f, 0.07f, 0.07f), Paint, Kind);
        Sphere(T, B, V(0.07f, 0.07f, 0.07f), Paint, Kind);
    }

    // ======================================================================
    // Эпоха I: ламповые ЭВМ (1943–1955)
    // Задания: лампы, панель тумблеров, коммутационная панель, устройство чтения перфокарт.
    // Модель — три шкафа в ряд: слева лампы, в центре тумблеры и коммутатор, справа перфокарты и индикаторы.
    // ======================================================================

    // Положение гнезда коммутационной панели: Row — номер ряда (0…3), Col — номер столбца (0…5).
    private static Vector3 Patch_Socket(int Row, int Col)
    {
        return V(-0.36f + Col * 0.144f, -0.2f - Row * 0.18f, -0.345f);
    }

    // Та же точка, но чуть ближе к камере: здесь заканчиваются провода (они лежат поверх панели).
    private static Vector3 Z_Front(Vector3 P)
    {
        return V(P.x, P.y, -0.385f);
    }

    // Одна электронная лампа: чёрное основание, полупрозрачная стеклянная колба,
    // купол сверху и тонкая светящаяся «нить» внутри. Все части — тип Vacuum_Tube.
    private static void Create_Vacuum_Tube(Transform T, Vector3 Pos)
    {
        Transform G = Make_Group(T, Pos, default, "Vacuum_Tube").transform;
        Color Glass = Rgb(0.55f, 0.92f, 0.95f, 0.6f);
        Component_Type Kind = Component_Type.Vacuum_Tube;

        Cylinder(G, V(0f, -0.13f, 0f), V(0.10f, 0.03f, 0.10f), Rgb(0.08f, 0.08f, 0.08f), Kind);   // основание
        Cylinder(G, V(0f, 0.02f, 0f), V(0.085f, 0.13f, 0.085f), Glass, Kind);                      // колба
        Sphere(G, V(0f, 0.15f, 0f), V(0.085f, 0.085f, 0.085f), Glass, Kind);                       // купол
        Cylinder(G, V(0f, 0f, 0f), V(0.02f, 0.09f, 0.02f), Rgb(1f, 0.65f, 0.2f), Kind);            // нить накала
    }

    private static void Create_Era_1_ENIAC(Transform T)
    {
        Color Steel = Rgb(0.30f, 0.31f, 0.34f);
        Color Dark = Rgb(0.10f, 0.10f, 0.12f);

        // --- Три шкафа (корпус, верхний и нижний карниз) ---
        float[] Cabinet_X = { -1.1f, 0f, 1.1f };

        foreach (float X in Cabinet_X)
        {
            Cube(T, V(X, 0f, 0f), V(1.05f, 2.4f, 0.6f), Steel);
            Cube(T, V(X, 1.24f, 0f), V(1.12f, 0.08f, 0.66f), Dark);
            Cube(T, V(X, -1.24f, 0f), V(1.12f, 0.08f, 0.66f), Dark);
        }

        // --- Левый шкаф: стойки с электронными лампами (4 полки по 5 ламп) ---
        for (int Row = 0; Row < 4; Row++)
        {
            float Y = 0.8f - Row * 0.5f;

            Cube(T, V(-1.1f, Y - 0.17f, -0.37f), V(0.95f, 0.04f, 0.16f), Dark);   // полка

            for (int I = -2; I <= 2; I++)
                Create_Vacuum_Tube(T, V(-1.1f + I * 0.19f, Y, -0.38f));
        }

        // --- Центральный шкаф, верх: панель тумблеров (4 ряда по 6 тумблеров) ---
        Cube(T, V(0f, 0.6f, -0.32f), V(0.95f, 0.85f, 0.04f), Rgb(0.62f, 0.58f, 0.48f), Component_Type.Switch_Panel);

        for (int R = 0; R < 4; R++)
        {
            for (int C = 0; C < 6; C++)
            {
                // Соседние тумблеры наклонены в разные стороны, чтобы выглядело «живо».
                float Tilt = ((R + C) % 2 == 0) ? 25f : -25f;

                Cube(T, V(-0.36f + C * 0.144f, 0.84f - R * 0.19f, -0.36f), V(0.05f, 0.10f, 0.05f),
                    Rgb(0.12f, 0.12f, 0.14f), Component_Type.Switch_Panel, V(Tilt, 0f, 0f));
            }
        }

        // --- Центральный шкаф, низ: коммутационная панель (4 ряда по 6 гнёзд) и провода ---
        Cube(T, V(0f, -0.45f, -0.32f), V(0.95f, 0.8f, 0.04f), Rgb(0.08f, 0.08f, 0.10f), Component_Type.Patch_Panel);

        for (int R = 0; R < 4; R++)
        {
            for (int C = 0; C < 6; C++)
            {
                Cylinder(T, Patch_Socket(R, C), V(0.07f, 0.012f, 0.07f), Rgb(0.78f, 0.78f, 0.82f),
                    Component_Type.Patch_Panel, V(90f, 0f, 0f));
            }
        }

        // Пять разноцветных проводов между гнёздами.
        Component_Type Patch = Component_Type.Patch_Panel;
        Create_Cable(T, Z_Front(Patch_Socket(0, 0)), Z_Front(Patch_Socket(1, 3)), Rgb(0.85f, 0.15f, 0.15f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(0, 1)), Z_Front(Patch_Socket(2, 4)), Rgb(0.95f, 0.80f, 0.15f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(1, 0)), Z_Front(Patch_Socket(3, 2)), Rgb(0.20f, 0.40f, 0.90f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(2, 1)), Z_Front(Patch_Socket(0, 5)), Rgb(0.15f, 0.70f, 0.30f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(3, 3)), Z_Front(Patch_Socket(1, 5)), Rgb(0.95f, 0.50f, 0.10f), Patch);

        // --- Правый шкаф, низ: устройство чтения перфокарт ---
        Component_Type Reader = Component_Type.Punch_Card_Reader;

        Cube(T, V(1.1f, -0.62f, -0.58f), V(0.9f, 0.55f, 0.55f), Rgb(0.55f, 0.50f, 0.40f), Reader);         // корпус
        Cube(T, V(0.95f, -0.30f, -0.58f), V(0.5f, 0.10f, 0.30f), Rgb(0.45f, 0.42f, 0.35f), Reader);        // подставка под карты
        Cube(T, V(0.95f, -0.18f, -0.58f), V(0.42f, 0.14f, 0.24f), Rgb(0.95f, 0.92f, 0.78f), Reader);       // стопка перфокарт
        Cube(T, V(1.35f, -0.32f, -0.58f), V(0.25f, 0.10f, 0.30f), Rgb(0.95f, 0.92f, 0.78f), Reader);       // приёмник карт
        Cube(T, V(1.1f, -0.60f, -0.86f), V(0.5f, 0.05f, 0.02f), Rgb(0.05f, 0.05f, 0.05f), Reader);         // щель
        Sphere(T, V(1.30f, -0.48f, -0.86f), V(0.05f, 0.05f, 0.05f), Rgb(0.9f, 0.1f, 0.1f), Reader);        // красный индикатор
        Sphere(T, V(1.38f, -0.48f, -0.86f), V(0.05f, 0.05f, 0.05f), Rgb(0.1f, 0.9f, 0.2f), Reader);        // зелёный индикатор

        // --- Правый шкаф, верх: индикаторные лампы (просто украшение, не вопрос) ---
        Cube(T, V(1.1f, 0.55f, -0.31f), V(0.95f, 0.9f, 0.03f), Rgb(0.08f, 0.08f, 0.09f));

        for (int R = 0; R < 5; R++)
        {
            for (int C = 0; C < 8; C++)
            {
                // Каждая третья лампа горит ярко, остальные тусклые.
                Color Lamp = ((R * 8 + C) % 3 == 0) ? Rgb(1f, 0.6f, 0.1f) : Rgb(0.35f, 0.25f, 0.10f);
                Sphere(T, V(1.1f + (C - 3.5f) * 0.1f, 0.55f + (R - 2f) * 0.16f, -0.335f), V(0.05f, 0.05f, 0.05f), Lamp);
            }
        }
    }

    // ======================================================================
    // Эпоха II: транзисторные ЭВМ (1955–1965)
    // Задания: платы с транзисторами, магнитная лента, память на сердечниках, пульт оператора.
    // Модель — три шкафа: слева лента, в центре процессор с пультом, справа память на сердечниках.
    // ======================================================================

    // Одна плата с транзисторами: зелёная плата, золотой разъём, три транзистора-«баночки»
    // и три резистора. X, Y — положение платы в стойке. Все части — тип Transistor.
    private static void Create_Transistor_Card(Transform T, float X, float Y)
    {
        Component_Type Kind = Component_Type.Transistor;

        Cube(T, V(X, Y, -0.48f), V(0.14f, 0.55f, 0.03f), Rgb(0.10f, 0.45f, 0.22f), Kind);          // плата
        Cube(T, V(X, Y - 0.29f, -0.48f), V(0.10f, 0.04f, 0.03f), Rgb(0.85f, 0.70f, 0.20f), Kind);  // разъём

        for (int J = 0; J < 3; J++)
        {
            float Part_Y = Y + (J - 1) * 0.15f;

            Cylinder(T, V(X, Part_Y, -0.51f), V(0.05f, 0.025f, 0.05f), Rgb(0.75f, 0.75f, 0.80f), Kind, V(90f, 0f, 0f));  // транзистор
            Cube(T, V(X, Part_Y + 0.07f, -0.505f), V(0.05f, 0.02f, 0.02f), Rgb(0.90f, 0.50f, 0.15f), Kind);              // резистор
        }
    }

    private static void Create_Era_2_Mainframe(Transform T)
    {
        Color Blue_Gray = Rgb(0.58f, 0.64f, 0.70f);
        Color Dark = Rgb(0.10f, 0.11f, 0.13f);

        // --- Центральный шкаф процессора ---
        Cube(T, V(0f, 0f, 0f), V(1.6f, 2.2f, 0.8f), Blue_Gray);
        Cube(T, V(0f, 1.12f, 0f), V(1.66f, 0.06f, 0.86f), Dark);
        Cube(T, V(0f, -1.12f, 0f), V(1.66f, 0.06f, 0.86f), Dark);

        // Стойка с семью транзисторными платами.
        Cube(T, V(0f, 0.65f, -0.43f), V(1.45f, 0.70f, 0.06f), Dark);

        for (int I = 0; I < 7; I++)
            Create_Transistor_Card(T, -0.6f + I * 0.2f, 0.65f);

        // Пульт оператора — наклонная панель с лампочками и тумблерами.
        // Все детали лежат внутри группы, которая наклонена на 20°.
        Component_Type Console_Kind = Component_Type.Control_Console;
        Transform C = Make_Group(T, V(0f, -0.4f, -0.5f), V(20f, 0f, 0f), "Operator_Console").transform;

        Cube(C, V(0f, 0f, 0f), V(1.4f, 0.55f, 0.08f), Rgb(0.14f, 0.15f, 0.18f), Console_Kind);

        // Три ряда по десять цветных лампочек (красные, жёлтые, зелёные по очереди).
        for (int R = 0; R < 3; R++)
        {
            for (int Col = 0; Col < 10; Col++)
            {
                int Pattern = (R * 3 + Col) % 3;
                Color Lamp = Pattern == 0 ? Rgb(0.9f, 0.15f, 0.15f)
                           : Pattern == 1 ? Rgb(1f, 0.75f, 0.15f)
                           : Rgb(0.2f, 0.85f, 0.3f);

                Sphere(C, V(-0.54f + Col * 0.12f, 0.18f - R * 0.10f, -0.05f), V(0.05f, 0.05f, 0.05f), Lamp, Console_Kind);
            }
        }

        // Ряд из десяти тумблеров (соседние наклонены в разные стороны).
        for (int Col = 0; Col < 10; Col++)
        {
            float Tilt = (Col % 2 == 0) ? 15f : -15f;

            Cube(C, V(-0.54f + Col * 0.12f, -0.16f, -0.05f), V(0.04f, 0.07f, 0.04f),
                Rgb(0.90f, 0.90f, 0.85f), Console_Kind, V(Tilt, 0f, 0f));
        }

        // --- Левый шкаф: накопитель на магнитной ленте ---
        Component_Type Tape = Component_Type.Magnetic_Tape;

        Cube(T, V(-1.3f, -0.1f, 0f), V(0.95f, 2.0f, 0.7f), Rgb(0.50f, 0.56f, 0.64f));                      // корпус
        Cube(T, V(-1.3f, 0.5f, -0.365f), V(0.85f, 0.90f, 0.03f), Rgb(0.10f, 0.12f, 0.15f), Tape);          // тёмное окно

        // Две катушки; у каждой — вакуумная колонна и петля ленты.
        foreach (float Reel_X in new[] { -1.5f, -1.1f })
        {
            Cylinder(T, V(Reel_X, 0.65f, -0.395f), V(0.38f, 0.02f, 0.38f), Rgb(0.20f, 0.20f, 0.23f), Tape, V(90f, 0f, 0f));   // катушка
            Cylinder(T, V(Reel_X, 0.65f, -0.415f), V(0.13f, 0.012f, 0.13f), Rgb(0.82f, 0.82f, 0.85f), Tape, V(90f, 0f, 0f)); // втулка
            Cube(T, V(Reel_X, 0.10f, -0.385f), V(0.12f, 0.50f, 0.02f), Rgb(0.25f, 0.35f, 0.45f), Tape);                       // колонна
            Cube(T, V(Reel_X, 0.18f, -0.40f), V(0.012f, 0.38f, 0.012f), Rgb(0.40f, 0.25f, 0.10f), Tape);                      // лента
        }

        // --- Правый шкаф: память на магнитных сердечниках ---
        Component_Type Core = Component_Type.Core_Memory;
        Color Gold = Rgb(0.85f, 0.70f, 0.25f);

        Cube(T, V(1.3f, -0.1f, 0f), V(0.95f, 2.0f, 0.7f), Rgb(0.52f, 0.58f, 0.56f));                       // корпус
        Cube(T, V(1.3f, 0.5f, -0.37f), V(0.75f, 0.75f, 0.04f), Rgb(0.20f, 0.15f, 0.10f), Core);            // рамка

        // Сетка 6×6 крошечных ферритовых колец.
        for (int R = 0; R < 6; R++)
        {
            for (int Col = 0; Col < 6; Col++)
            {
                Cylinder(T, V(1.3f + (Col - 2.5f) * 0.1f, 0.5f + (R - 2.5f) * 0.1f, -0.40f),
                    V(0.055f, 0.012f, 0.055f), Rgb(0.32f, 0.20f, 0.12f), Core, V(90f, 0f, 0f));
            }
        }

        // Золотые провода, нанизанные на кольца: 6 горизонтальных и 6 вертикальных.
        for (int R = 0; R < 6; R++)
            Cube(T, V(1.3f, 0.5f + (R - 2.5f) * 0.1f, -0.41f), V(0.62f, 0.008f, 0.008f), Gold, Core);

        for (int Col = 0; Col < 6; Col++)
            Cube(T, V(1.3f + (Col - 2.5f) * 0.1f, 0.5f, -0.42f), V(0.008f, 0.62f, 0.008f), Gold, Core);

        // Индикаторы под блоком памяти (украшение).
        for (int I = 0; I < 8; I++)
        {
            Color Lamp = (I % 2 == 0) ? Rgb(1f, 0.7f, 0.1f) : Rgb(0.3f, 0.25f, 0.1f);
            Sphere(T, V(1.3f + (I - 3.5f) * 0.1f, -0.5f, -0.36f), V(0.05f, 0.05f, 0.05f), Lamp);
        }
    }

    // ======================================================================
    // Эпоха III: микрокомпьютер (1965–1980)
    // Задания: микросхема, дисковод 5.25", клавиатура, ЭЛТ-монитор.
    // Модель — системный блок с дисководом, клавиатура перед ним, ЭЛТ-монитор сверху
    // и открытая плата с микросхемами на подставке справа.
    // ======================================================================

    // Одна микросхема в корпусе DIP: чёрный корпус и два ряда металлических «ножек».
    // P — к чему прикрепить, Pos — центр, W и H — ширина и высота корпуса.
    private static void Create_DIP_Chip(Transform P, Vector3 Pos, float W, float H)
    {
        Component_Type Kind = Component_Type.Integrated_Circuit;
        Color Pins = Rgb(0.72f, 0.72f, 0.75f);

        Cube(P, Pos, V(W, H, 0.05f), Rgb(0.07f, 0.07f, 0.08f), Kind);                                                    // корпус
        Cube(P, V(Pos.x, Pos.y + H * 0.5f + 0.012f, Pos.z + 0.01f), V(W - 0.02f, 0.025f, 0.02f), Pins, Kind);            // верхние ножки
        Cube(P, V(Pos.x, Pos.y - H * 0.5f - 0.012f, Pos.z + 0.01f), V(W - 0.02f, 0.025f, 0.02f), Pins, Kind);            // нижние ножки
    }

    private static void Create_Era_3_Microcomputer(Transform T)
    {
        Color Beige = Rgb(0.83f, 0.79f, 0.67f);

        // --- Системный блок и вентиляционные прорези на нём ---
        Cube(T, V(-0.5f, -0.6f, 0f), V(1.8f, 0.5f, 1.2f), Beige);

        for (int I = 0; I < 5; I++)
            Cube(T, V(-1.2f + I * 0.07f, -0.6f, -0.605f), V(0.03f, 0.30f, 0.01f), Rgb(0.30f, 0.28f, 0.24f));

        // --- Дисковод 5.25": передняя панель, щель, красный рычаг и индикатор ---
        Component_Type Floppy = Component_Type.Floppy_Drive;
        Transform F = Make_Group(T, V(-0.1f, -0.6f, -0.62f), default, "Floppy_Drive").transform;

        Cube(F, V(0f, 0f, 0f), V(0.70f, 0.30f, 0.04f), Rgb(0.22f, 0.22f, 0.25f), Floppy);
        Cube(F, V(0f, 0.02f, -0.025f), V(0.50f, 0.04f, 0.015f), Rgb(0.02f, 0.02f, 0.02f), Floppy);
        Cube(F, V(0.20f, 0.02f, -0.035f), V(0.07f, 0.12f, 0.02f), Rgb(0.80f, 0.10f, 0.10f), Floppy);
        Cube(F, V(-0.28f, -0.10f, -0.025f), V(0.03f, 0.03f, 0.015f), Rgb(1f, 0.1f, 0.1f), Floppy);

        // --- Клавиатура: основание и 31 клавиша, слегка наклонена к игроку ---
        Component_Type Keys = Component_Type.Keyboard;
        Transform K = Make_Group(T, V(-0.5f, -0.83f, -1.05f), V(-10f, 0f, 0f), "Keyboard").transform;

        Cube(K, V(0f, 0f, 0f), V(1.30f, 0.06f, 0.50f), Rgb(0.78f, 0.74f, 0.64f), Keys);

        // Три ряда по десять клавиш (часть клавиш тёмные).
        for (int R = 0; R < 3; R++)
        {
            for (int C = 0; C < 10; C++)
            {
                Color Key_Paint = ((R + C) % 4 == 0) ? Rgb(0.45f, 0.42f, 0.38f) : Rgb(0.90f, 0.88f, 0.80f);
                Cube(K, V((C - 4.5f) * 0.115f, 0.05f, 0.12f - R * 0.10f), V(0.09f, 0.04f, 0.08f), Key_Paint, Keys);
            }
        }

        Cube(K, V(0f, 0.05f, -0.18f), V(0.60f, 0.04f, 0.07f), Rgb(0.90f, 0.88f, 0.80f), Keys);   // пробел

        // --- ЭЛТ-монитор: рамка, корпус, задняя «колба», тёмно-зелёный экран и строки текста ---
        Component_Type CRT = Component_Type.Monitor_CRT;
        Transform M = Make_Group(T, V(-0.6f, 0.1f, 0f), default, "CRT_Monitor").transform;

        Cube(M, V(0f, 0f, -0.30f), V(1.15f, 0.90f, 0.10f), Rgb(0.85f, 0.80f, 0.68f), CRT);
        Cube(M, V(0f, 0f, 0f), V(1.00f, 0.80f, 0.45f), Rgb(0.80f, 0.76f, 0.64f), CRT);
        Cube(M, V(0f, 0f, 0.38f), V(0.60f, 0.50f, 0.35f), Rgb(0.76f, 0.72f, 0.60f), CRT);
        Cube(M, V(0f, 0f, -0.355f), V(0.92f, 0.72f, 0.03f), Rgb(0.04f, 0.15f, 0.08f), CRT);

        // Пять зелёных «строк текста» разной длины, выровненные по левому краю.
        float[] Line_Width = { 0.50f, 0.70f, 0.35f, 0.60f, 0.25f };
        float[] Line_Y = { 0.25f, 0.14f, 0.03f, -0.08f, -0.19f };

        for (int I = 0; I < Line_Width.Length; I++)
            Cube(M, V(-0.42f + Line_Width[I] * 0.5f, Line_Y[I], -0.375f), V(Line_Width[I], 0.03f, 0.01f),
                Rgb(0.35f, 1f, 0.50f), CRT);

        // --- Открытая плата с микросхемами на подставке (справа) ---
        Cube(T, V(1.0f, -0.82f, -0.15f), V(0.95f, 0.04f, 0.40f), Rgb(0.15f, 0.15f, 0.17f));   // подставка

        Transform B = Make_Group(T, V(1.0f, -0.4f, -0.15f), default, "IC_Board").transform;

        // Сама плата — «отвлекающий» элемент (кликать по ней — ошибка).
        Cube(B, V(0f, 0f, 0f), V(0.95f, 0.80f, 0.04f), Rgb(0.08f, 0.38f, 0.20f), Component_Type.Motherboard);

        // Одна большая 40-выводная микросхема (процессор) и шесть микросхем поменьше.
        Create_DIP_Chip(B, V(0f, 0.20f, -0.045f), 0.55f, 0.14f);

        for (int R = 0; R < 2; R++)
        {
            for (int C = 0; C < 3; C++)
                Create_DIP_Chip(B, V((C - 1) * 0.28f, -0.05f - R * 0.22f, -0.045f), 0.20f, 0.09f);
        }

        // Четыре синих конденсатора (украшение).
        for (int I = 0; I < 4; I++)
            Cylinder(B, V(-0.35f + I * 0.23f, -0.37f, -0.04f), V(0.06f, 0.04f, 0.06f), Rgb(0.10f, 0.20f, 0.70f),
                null, V(90f, 0f, 0f));
    }

    // ======================================================================
    // Общий каркас башенного корпуса (эпохи IV и V)
    // Глубина (фронт–тыл) — по X, высота — по Y, ширина — по Z.
    // Фронт — по -X, открытая сторона — по -Z, дальняя стенка (под плату) — по +Z.
    // Метод строит стенки и боковую крышку. Возвращает крышку, чтобы к ней можно было прикрепить украшения.
    // ======================================================================

    private static Transform Build_Tower_Shell(Transform T, float Depth, float Height, float Width,
        Color Body, Color Front_Paint, Color Cover_Paint)
    {
        const float Wall = 0.05f;   // толщина стенок

        // Стенки «твёрдые»: клик по ним не проходит внутрь корпуса.
        Cube(T, V(Depth / 2f - Wall / 2f, 0f, 0f), V(Wall, Height, Width), Body, Solid: true);             // задняя стенка
        Cube(T, V(0f, Height / 2f - Wall / 2f, 0f), V(Depth, Wall, Width), Body, Solid: true);             // верх
        Cube(T, V(0f, -Height / 2f + Wall / 2f, 0f), V(Depth, Wall, Width), Body, Solid: true);            // низ
        Cube(T, V(0f, 0f, Width / 2f - Wall / 2f), V(Depth, Height, Wall), Body, Solid: true);             // дальняя стенка (+Z)
        Cube(T, V(-Depth / 2f + 0.04f, 0f, 0f), V(0.08f, Height, Width), Front_Paint, Solid: true);        // фронтальная панель

        // Боковая крышка стоит по -Z. При открытии она сдвигается назад (в сторону +X).
        // Сама крышка — дочерний объект «рамки» Pivot, а Openable_Cover двигает именно Pivot.
        GameObject Pivot = Make_Group(T, V(Depth / 2f, 0f, -Width / 2f - 0.025f), default, "SideCover");
        Pivot.AddComponent<Openable_Cover>().Setup(V(Depth + 0.1f, 0f, 0f));

        Cube(Pivot.transform, V(-Depth / 2f, 0f, 0f), V(Depth, Height, 0.05f), Cover_Paint, Component_Type.Case_Cover);

        return Pivot.transform;
    }

    // ======================================================================
    // Эпоха IV: ретро-ПК (1980–2000), разборный
    // Задания: открыть крышку, процессор, ОЗУ, 3D-видеокарта, жёсткий диск.
    // Отвлекающие элементы: дисководы, CD-привод, звуковая плата, блок питания, материнская плата.
    // Съёмные детали: процессор с радиатором, два модуля ОЗУ, видеокарта, звуковая плата, жёсткий диск.
    // ======================================================================

    // Модуль оперативной памяти (старого образца) с разъёмом на плате. X — положение по оси X.
    private static void Create_Retro_RAM(Transform T, float X)
    {
        // Разъём на материнской плате — отвлекающий элемент.
        Cube(T, V(X, 0.10f, 0.40f), V(0.05f, 0.55f, 0.03f), Rgb(0.08f, 0.08f, 0.09f), Component_Type.Motherboard);

        GameObject Stick = Make_Group(T, V(X, 0.10f, 0f), default, "RAM_Stick");
        Transform S = Stick.transform;
        Component_Type Kind = Component_Type.RAM;

        Cube(S, V(0f, 0f, 0.29f), V(0.025f, 0.50f, 0.22f), Rgb(0.12f, 0.45f, 0.25f), Kind);   // плата модуля

        // Восемь чёрных микросхем: два столбца по четыре.
        for (int Col = 0; Col < 2; Col++)
        {
            for (int I = 0; I < 4; I++)
                Cube(S, V(-0.018f, -0.15f + I * 0.10f, 0.34f - Col * 0.09f), V(0.012f, 0.07f, 0.06f),
                    Rgb(0.08f, 0.08f, 0.09f), Kind);
        }

        Make_Detachable(Stick);
    }

    // Отсек на передней панели: дисковод или CD-привод. Y — высота, H — высота отсека,
    // Kind — тип детали, Face_Paint — цвет лицевой панели, Is_Floppy — дисковод (true) или CD-привод (false).
    private static void Create_Front_Drive(Transform T, float Y, float H, Component_Type Kind, Color Face_Paint, bool Is_Floppy)
    {
        const float Z_Size = 0.5f;

        Transform D = Make_Group(T, V(-0.62f, Y, 0f), default, "Front_Drive").transform;

        Cube(D, V(0f, 0f, 0f), V(0.50f, H, Z_Size), Rgb(0.60f, 0.60f, 0.63f), Kind);          // корпус привода
        Cube(D, V(-0.35f, 0f, 0f), V(0.04f, H, Z_Size), Face_Paint, Kind);                     // лицевая панель

        if (Is_Floppy)
        {
            Cube(D, V(-0.372f, 0f, 0f), V(0.01f, 0.03f, Z_Size * 0.8f), Rgb(0.02f, 0.02f, 0.02f), Kind);       // щель для дискеты
            Cube(D, V(-0.375f, 0f, Z_Size * 0.3f), V(0.012f, H * 0.6f, 0.05f), Rgb(0.80f, 0.10f, 0.10f), Kind); // красный рычаг
        }
        else
        {
            Cube(D, V(-0.372f, 0.03f, 0f), V(0.01f, 0.02f, Z_Size * 0.85f), Rgb(0.25f, 0.25f, 0.28f), Kind);    // лоток диска
            Cube(D, V(-0.372f, -0.05f, Z_Size * 0.3f), V(0.01f, 0.03f, 0.06f), Rgb(0.45f, 0.45f, 0.48f), Kind); // кнопка
        }
    }

    private static void Create_Era_4_Retro_PC(Transform T)
    {
        const float Depth = 1.9f;    // глубина корпуса
        const float Height = 2.2f;   // высота
        const float Width = 1.0f;    // ширина

        // Корпус бежевого цвета с боковой крышкой.
        Transform Cover = Build_Tower_Shell(T, Depth, Height, Width,
            Rgb(0.72f, 0.69f, 0.60f), Rgb(0.84f, 0.81f, 0.72f), Rgb(0.78f, 0.75f, 0.66f));

        // Украшения крышки: вентиляционные прорези и табличка.
        for (int I = 0; I < 6; I++)
            Cube(Cover, V(-0.5f, -0.9f + I * 0.07f, -0.03f), V(0.5f, 0.03f, 0.01f), Rgb(0.50f, 0.47f, 0.40f));

        Cube(Cover, V(-1.5f, 0.9f, -0.03f), V(0.25f, 0.08f, 0.01f), Rgb(0.20f, 0.20f, 0.25f));

        // Украшения фронта: кнопка питания и два светодиода.
        Cube(T, V(-0.965f, -0.55f, 0.15f), V(0.03f, 0.10f, 0.10f), Rgb(0.25f, 0.25f, 0.28f));
        Cube(T, V(-0.96f, -0.35f, 0.30f), V(0.02f, 0.04f, 0.04f), Rgb(0.10f, 0.90f, 0.20f));
        Cube(T, V(-0.96f, -0.35f, 0.20f), V(0.02f, 0.04f, 0.04f), Rgb(0.90f, 0.60f, 0.10f));

        // Передние отсеки (не съёмные): дисковод 5.25", CD-привод, дисковод 3.5".
        Create_Front_Drive(T, 0.80f, 0.20f, Component_Type.Floppy_Drive, Rgb(0.80f, 0.77f, 0.68f), true);
        Create_Front_Drive(T, 0.55f, 0.20f, Component_Type.CD_Drive, Rgb(0.82f, 0.79f, 0.70f), false);
        Create_Front_Drive(T, 0.33f, 0.12f, Component_Type.Floppy_Drive, Rgb(0.80f, 0.77f, 0.68f), true);

        // Блок питания в верхней задней части: корпус и решётка вентилятора.
        Cube(T, V(0.55f, 0.79f, 0f), V(0.70f, 0.48f, 0.80f), Rgb(0.55f, 0.55f, 0.58f), Component_Type.PSU);
        Cylinder(T, V(0.55f, 0.79f, -0.403f), V(0.30f, 0.004f, 0.30f), Rgb(0.12f, 0.12f, 0.14f), Component_Type.PSU, V(90f, 0f, 0f));

        // Материнская плата и гнездо процессора (отвлекающие элементы).
        Cube(T, V(0.20f, -0.20f, 0.43f), V(1.25f, 1.40f, 0.03f), Rgb(0.10f, 0.38f, 0.20f), Component_Type.Motherboard);
        Cube(T, V(0.35f, 0.20f, 0.40f), V(0.30f, 0.30f, 0.03f), Rgb(0.85f, 0.85f, 0.80f), Component_Type.Motherboard);

        // --- Процессор с радиатором и вентилятором (съёмный) ---
        // Координаты частей внутри группы заданы в «абсолютных» значениях по Z (группа стоит на Z = 0).
        GameObject CPU_Group = Make_Group(T, V(0.35f, 0.20f, 0f), default, "CPU");
        Transform C = CPU_Group.transform;
        Component_Type CPU_Kind = Component_Type.CPU;

        Cube(C, V(0f, 0f, 0.365f), V(0.24f, 0.24f, 0.04f), Rgb(0.18f, 0.18f, 0.20f), CPU_Kind);   // сам кристалл-процессор
        Cube(C, V(0f, 0f, 0.33f), V(0.26f, 0.26f, 0.03f), Rgb(0.72f, 0.74f, 0.78f), CPU_Kind);    // основание радиатора

        for (int I = 0; I < 5; I++)
            Cube(C, V(0f, -0.10f + I * 0.05f, 0.265f), V(0.26f, 0.025f, 0.10f), Rgb(0.65f, 0.67f, 0.72f), CPU_Kind);   // рёбра радиатора

        Cube(C, V(0f, 0f, 0.19f), V(0.22f, 0.22f, 0.03f), Rgb(0.10f, 0.10f, 0.12f), CPU_Kind);                         // корпус вентилятора
        Cylinder(C, V(0f, 0f, 0.172f), V(0.09f, 0.008f, 0.09f), Rgb(0.50f, 0.50f, 0.55f), CPU_Kind, V(90f, 0f, 0f));  // крыльчатка
        Make_Detachable(CPU_Group);

        // --- Два модуля оперативной памяти (съёмные) ---
        Create_Retro_RAM(T, 0.62f);
        Create_Retro_RAM(T, 0.70f);

        // --- Видеокарта AGP (съёмная) ---
        GameObject GPU_Group = Make_Group(T, V(0f, -0.35f, 0f), default, "GPU");
        Transform G = GPU_Group.transform;
        Component_Type GPU_Kind = Component_Type.GPU;

        Cube(G, V(0.36f, 0f, 0.245f), V(0.82f, 0.03f, 0.34f), Rgb(0.12f, 0.42f, 0.30f), GPU_Kind);      // плата
        Cube(G, V(0.30f, 0f, 0.40f), V(0.40f, 0.03f, 0.03f), Rgb(0.85f, 0.70f, 0.20f), GPU_Kind);       // золотой разъём
        Cube(G, V(0.20f, 0.045f, 0.24f), V(0.22f, 0.06f, 0.20f), Rgb(0.72f, 0.74f, 0.78f), GPU_Kind);   // радиатор
        Cylinder(G, V(0.20f, 0.085f, 0.24f), V(0.16f, 0.01f, 0.16f), Rgb(0.10f, 0.10f, 0.12f), GPU_Kind); // вентилятор

        // Шесть микросхем видеопамяти (3 × 2).
        for (int X = 0; X < 3; X++)
        {
            for (int Z = 0; Z < 2; Z++)
                Cube(G, V(0.45f + X * 0.10f, 0.027f, 0.18f + Z * 0.12f), V(0.06f, 0.025f, 0.05f), Rgb(0.08f, 0.08f, 0.09f), GPU_Kind);
        }

        Cube(G, V(0.78f, 0.03f, 0.245f), V(0.015f, 0.12f, 0.34f), Rgb(0.75f, 0.75f, 0.78f), GPU_Kind);  // планка у задней стенки
        Make_Detachable(GPU_Group);

        // --- Звуковая плата (съёмная, отвлекающий элемент) ---
        GameObject Sound_Group = Make_Group(T, V(0f, -0.62f, 0f), default, "Sound_Card");
        Transform S = Sound_Group.transform;
        Component_Type Sound_Kind = Component_Type.Sound_Card;

        Cube(S, V(0.45f, 0f, 0.275f), V(0.60f, 0.03f, 0.28f), Rgb(0.55f, 0.25f, 0.12f), Sound_Kind);
        Cube(S, V(0.45f, 0f, 0.40f), V(0.30f, 0.03f, 0.03f), Rgb(0.85f, 0.70f, 0.20f), Sound_Kind);
        Cube(S, V(0.40f, 0.027f, 0.28f), V(0.12f, 0.025f, 0.08f), Rgb(0.08f, 0.08f, 0.09f), Sound_Kind);
        Cube(S, V(0.60f, 0.027f, 0.22f), V(0.08f, 0.025f, 0.06f), Rgb(0.08f, 0.08f, 0.09f), Sound_Kind);
        Cube(S, V(0.755f, 0.03f, 0.275f), V(0.015f, 0.10f, 0.28f), Rgb(0.75f, 0.75f, 0.78f), Sound_Kind);
        Make_Detachable(Sound_Group);

        // --- Жёсткий диск (съёмный) ---
        GameObject HDD_Group = Make_Group(T, V(-0.50f, -0.96f, -0.10f), default, "HDD");
        Transform H = HDD_Group.transform;
        Component_Type HDD_Kind = Component_Type.Hard_Drive;

        Cube(H, V(0f, 0f, 0f), V(0.60f, 0.14f, 0.42f), Rgb(0.62f, 0.64f, 0.68f), HDD_Kind);         // металлический корпус
        Cube(H, V(0f, 0.073f, 0f), V(0.40f, 0.006f, 0.26f), Rgb(0.92f, 0.92f, 0.90f), HDD_Kind);    // белая наклейка
        Cube(H, V(0.31f, 0f, 0f), V(0.03f, 0.08f, 0.20f), Rgb(0.15f, 0.15f, 0.15f), HDD_Kind);      // разъём
        Make_Detachable(HDD_Group);
    }

    // ======================================================================
    // Эпоха V: современный ПК (с 2000-х), разборный
    // Задания: открыть стекло, охлаждение процессора, ОЗУ, видеокарта, NVMe SSD.
    // Отвлекающие элементы: блок питания, материнская плата с радиаторами.
    // Съёмные детали: жидкостное охлаждение, два модуля ОЗУ, видеокарта, NVMe SSD.
    // ======================================================================

    // Модуль оперативной памяти с RGB-подсветкой. X — положение по оси X, Glow — цвет подсветки.
    private static void Create_Modern_RAM(Transform T, float X, Color Glow)
    {
        // Разъём на материнской плате — отвлекающий элемент.
        Cube(T, V(X, 0.40f, 0.40f), V(0.05f, 0.60f, 0.03f), Rgb(0.05f, 0.05f, 0.06f), Component_Type.Motherboard);

        GameObject Stick = Make_Group(T, V(X, 0.40f, 0f), default, "RAM_Stick");
        Transform S = Stick.transform;
        Component_Type Kind = Component_Type.RAM;

        Cube(S, V(0f, 0f, 0.305f), V(0.020f, 0.55f, 0.22f), Rgb(0.10f, 0.30f, 0.20f), Kind);      // плата модуля
        Cube(S, V(0f, 0f, 0.300f), V(0.036f, 0.50f, 0.20f), Rgb(0.10f, 0.10f, 0.12f), Kind);      // радиатор-кожух
        Cube(S, V(0f, 0.265f, 0.300f), V(0.040f, 0.03f, 0.21f), Glow, Kind);                      // световая полоса сверху
        Cube(S, V(0f, 0f, 0.195f), V(0.037f, 0.50f, 0.02f), Glow, Kind);                          // световая полоса спереди
        Make_Detachable(Stick);
    }

    private static void Create_Era_5_Modern_PC(Transform T)
    {
        const float Depth = 1.8f;
        const float Height = 2.2f;
        const float Width = 1.0f;

        // Тёмный корпус со стеклянной боковой панелью (последний цвет — полупрозрачный голубой).
        Transform Cover = Build_Tower_Shell(T, Depth, Height, Width,
            Rgb(0.10f, 0.10f, 0.12f), Rgb(0.07f, 0.07f, 0.09f), Rgb(0.45f, 0.70f, 0.95f, 0.28f));

        // Тёмная рамка вокруг стекла: верх, низ, левая и правая стороны.
        Color Frame = Rgb(0.05f, 0.05f, 0.06f);
        Cube(Cover, V(-Depth / 2f, Height / 2f - 0.02f, -0.035f), V(Depth, 0.04f, 0.01f), Frame);
        Cube(Cover, V(-Depth / 2f, -Height / 2f + 0.02f, -0.035f), V(Depth, 0.04f, 0.01f), Frame);
        Cube(Cover, V(-Depth + 0.02f, 0f, -0.035f), V(0.04f, Height, 0.01f), Frame);
        Cube(Cover, V(-0.02f, 0f, -0.035f), V(0.04f, Height, 0.01f), Frame);

        // Кожух блока питания внизу (отвлекающий элемент) и красная полоска-логотип на нём.
        Cube(T, V(0f, -0.85f, 0f), V(1.65f, 0.40f, 0.85f), Rgb(0.05f, 0.05f, 0.06f), Component_Type.PSU);
        Cube(T, V(0.30f, -0.85f, -0.43f), V(0.50f, 0.04f, 0.01f), Rgb(0.80f, 0.10f, 0.10f), Component_Type.PSU);

        // Материнская плата и два радиатора на ней (отвлекающие элементы).
        Cube(T, V(0.10f, 0.20f, 0.43f), V(1.35f, 1.50f, 0.03f), Rgb(0.06f, 0.06f, 0.08f), Component_Type.Motherboard);
        Cube(T, V(-0.45f, 0.55f, 0.38f), V(0.12f, 0.45f, 0.07f), Rgb(0.20f, 0.20f, 0.22f), Component_Type.Motherboard);
        Cube(T, V(0.45f, -0.40f, 0.39f), V(0.22f, 0.20f, 0.05f), Rgb(0.20f, 0.20f, 0.22f), Component_Type.Motherboard);

        // --- Жидкостное охлаждение процессора: помпа, две трубки и радиатор (съёмное целиком) ---
        GameObject Cooler_Group = Make_Group(T, V(0f, 0f, 0f), default, "Cooler");
        Transform K = Cooler_Group.transform;
        Component_Type Cooler_Kind = Component_Type.Cooler;
        Color Tube_Paint = Rgb(0.07f, 0.07f, 0.08f);

        // Помпа: тёмный диск, голубое светящееся кольцо и чёрная крышка.
        Cylinder(K, V(0.05f, 0.50f, 0.34f), V(0.28f, 0.04f, 0.28f), Rgb(0.12f, 0.12f, 0.14f), Cooler_Kind, V(90f, 0f, 0f));
        Cylinder(K, V(0.05f, 0.50f, 0.30f), V(0.30f, 0.012f, 0.30f), Rgb(0.00f, 0.85f, 1.00f), Cooler_Kind, V(90f, 0f, 0f));
        Cylinder(K, V(0.05f, 0.50f, 0.288f), V(0.20f, 0.010f, 0.20f), Rgb(0.05f, 0.05f, 0.07f), Cooler_Kind, V(90f, 0f, 0f));

        // Две трубки от помпы к радиатору (лежат горизонтально).
        Cylinder(K, V(-0.38f, 0.57f, 0.34f), V(0.04f, 0.29f, 0.04f), Tube_Paint, Cooler_Kind, V(0f, 0f, 90f));
        Cylinder(K, V(-0.38f, 0.43f, 0.34f), V(0.04f, 0.29f, 0.04f), Tube_Paint, Cooler_Kind, V(0f, 0f, 90f));

        // Радиатор у передней стенки и его светлые «рёбра».
        Cube(K, V(-0.72f, 0.35f, 0.20f), V(0.10f, 1.00f, 0.36f), Rgb(0.12f, 0.12f, 0.14f), Cooler_Kind);

        for (int I = 0; I < 12; I++)
            Cube(K, V(-0.72f, -0.10f + I * 0.08f, 0.015f), V(0.09f, 0.02f, 0.01f), Rgb(0.40f, 0.40f, 0.45f), Cooler_Kind);

        Make_Detachable(Cooler_Group);

        // --- Два модуля ОЗУ с RGB-подсветкой (съёмные): розовая и голубая ---
        Create_Modern_RAM(T, 0.45f, Rgb(1.00f, 0.20f, 0.80f));
        Create_Modern_RAM(T, 0.53f, Rgb(0.20f, 0.90f, 1.00f));

        // --- NVMe SSD формата M.2 (съёмный): синяя плата, две микросхемы памяти и контроллер ---
        GameObject SSD_Group = Make_Group(T, V(0.10f, 0f, 0f), default, "NVMe_SSD");
        Transform N = SSD_Group.transform;
        Component_Type SSD_Kind = Component_Type.SSD_NVMe;

        Cube(N, V(0f, 0f, 0.405f), V(0.34f, 0.08f, 0.012f), Rgb(0.10f, 0.28f, 0.65f), SSD_Kind);
        Cube(N, V(-0.07f, 0f, 0.395f), V(0.09f, 0.065f, 0.01f), Rgb(0.10f, 0.10f, 0.11f), SSD_Kind);
        Cube(N, V(0.04f, 0f, 0.395f), V(0.09f, 0.065f, 0.01f), Rgb(0.10f, 0.10f, 0.11f), SSD_Kind);
        Cube(N, V(0.12f, 0f, 0.395f), V(0.05f, 0.05f, 0.01f), Rgb(0.35f, 0.35f, 0.38f), SSD_Kind);
        Make_Detachable(SSD_Group);

        // --- Видеокарта с тремя вентиляторами (съёмная) ---
        GameObject GPU_Group = Make_Group(T, V(0f, -0.15f, 0f), default, "GPU");
        Transform G = GPU_Group.transform;
        Component_Type GPU_Kind = Component_Type.GPU;

        Cube(G, V(0.10f, 0f, 0.20f), V(1.25f, 0.20f, 0.34f), Rgb(0.08f, 0.08f, 0.10f), GPU_Kind);       // кожух
        Cube(G, V(0.15f, 0f, 0.392f), V(0.80f, 0.05f, 0.045f), Rgb(0.15f, 0.15f, 0.17f), GPU_Kind);     // разъём в материнскую плату
        Cube(G, V(0.10f, 0.11f, 0.20f), V(1.25f, 0.02f, 0.34f), Rgb(0.20f, 0.20f, 0.22f), GPU_Kind);    // задняя пластина
        Cube(G, V(0.60f, 0.12f, 0.30f), V(0.15f, 0.04f, 0.05f), Rgb(0.05f, 0.05f, 0.06f), GPU_Kind);    // разъём питания
        Cube(G, V(0.735f, 0f, 0.20f), V(0.015f, 0.20f, 0.34f), Rgb(0.75f, 0.75f, 0.78f), GPU_Kind);     // планка у задней стенки

        // Три вентилятора: голубое кольцо, тёмный диск и серая втулка.
        foreach (float Fan_X in new[] { -0.2f, 0.1f, 0.4f })
        {
            Cylinder(G, V(Fan_X, 0f, 0.027f), V(0.19f, 0.004f, 0.19f), Rgb(0.00f, 0.85f, 1.00f), GPU_Kind, V(90f, 0f, 0f));
            Cylinder(G, V(Fan_X, 0f, 0.021f), V(0.16f, 0.004f, 0.16f), Rgb(0.10f, 0.10f, 0.12f), GPU_Kind, V(90f, 0f, 0f));
            Cylinder(G, V(Fan_X, 0f, 0.015f), V(0.05f, 0.004f, 0.05f), Rgb(0.55f, 0.55f, 0.60f), GPU_Kind, V(90f, 0f, 0f));
        }

        Make_Detachable(GPU_Group);
    }

    // ======================================================================
    // Бонусный уровень: 3D-модели Intel (процессор), AMD (процессор), Nvidia (видеокарта).
    // Модели неинтерактивны: они стоят на подставке-«витрине» и вращаются сами.
    // ======================================================================

    // Подставка: основание, стойка и цветное кольцо фирменного цвета.
    // Post_Top_Y — на какой высоте заканчивается стойка (там начинается сама деталь).
    private static void Create_Showcase_Stand(Transform T, Color Accent, float Post_Top_Y)
    {
        Cylinder(T, V(0f, -0.90f, 0f), V(0.60f, 0.03f, 0.60f), Rgb(0.12f, 0.12f, 0.14f));   // основание
        Cylinder(T, V(0f, -0.925f, 0f), V(0.64f, 0.008f, 0.64f), Accent);                    // цветное кольцо

        float Post_Height = Post_Top_Y + 0.87f;
        Cube(T, V(0f, -0.87f + Post_Height * 0.5f, 0f), V(0.08f, Post_Height, 0.08f), Rgb(0.30f, 0.30f, 0.34f));   // стойка
    }

    // Сетка золотых контактов на обратной стороне процессора. В центре контактов нет (как на настоящих).
    // Cols и Rows — число столбцов и рядов, Spacing — расстояние между контактами, Pos — центр сетки.
    private static void Create_Pad_Grid(Transform T, int Cols, int Rows, float Spacing, Vector3 Pos)
    {
        Color Gold = Rgb(0.85f, 0.70f, 0.25f);

        for (int R = 0; R < Rows; R++)
        {
            for (int C = 0; C < Cols; C++)
            {
                // Смещение контакта от центра сетки.
                float Dx = C - (Cols - 1) / 2f;
                float Dy = R - (Rows - 1) / 2f;

                // В центре — пустое место.
                if (Mathf.Abs(Dx) < 1.5f && Mathf.Abs(Dy) < 2f)
                    continue;

                Cube(T, V(Pos.x + Dx * Spacing, Pos.y + Dy * Spacing, Pos.z), V(0.05f, 0.05f, 0.012f), Gold);
            }
        }
    }

    // Процессор Intel: зелёная подложка, металлическая крышка с синей табличкой,
    // конденсаторы по краям, метка ключа в углу и золотые контакты на обороте.
    private static void Create_Bonus_Intel(Transform T)
    {
        Color Gold = Rgb(0.85f, 0.70f, 0.25f);
        Color Cap = Rgb(0.45f, 0.33f, 0.18f);
        Color White = Rgb(0.95f, 0.95f, 0.97f);

        Transform Chip = Make_Group(T, V(0f, 0.15f, 0f), default, "Intel_CPU").transform;

        // Подложка и теплораспределитель (металлическая крышка процессора).
        Cube(Chip, V(0f, 0f, 0f), V(1.25f, 1.50f, 0.06f), Rgb(0.08f, 0.42f, 0.26f));
        Cube(Chip, V(0f, 0.02f, -0.07f), V(0.95f, 1.05f, 0.08f), Rgb(0.76f, 0.78f, 0.82f));
        Cube(Chip, V(0f, 0.02f, -0.115f), V(0.85f, 0.95f, 0.015f), Rgb(0.86f, 0.88f, 0.91f));

        // Синяя табличка с белыми «буквами» и серые полоски гравировки.
        Cube(Chip, V(0f, 0.30f, -0.126f), V(0.55f, 0.20f, 0.01f), Rgb(0.00f, 0.44f, 0.78f));

        for (int I = 0; I < 5; I++)
            Cube(Chip, V(-0.2f + I * 0.1f, 0.30f, -0.133f), V(0.06f, 0.09f, 0.006f), White);

        for (int I = 0; I < 4; I++)
            Cube(Chip, V(0f, -0.05f - I * 0.12f, -0.126f), V(0.6f - I * 0.1f, 0.025f, 0.006f), Rgb(0.55f, 0.57f, 0.62f));

        // Конденсаторы вдоль верхнего и нижнего края подложки.
        for (int I = 0; I < 8; I++)
        {
            float X = -0.49f + I * 0.14f;
            Cube(Chip, V(X, 0.65f, -0.045f), V(0.07f, 0.04f, 0.03f), Cap);
            Cube(Chip, V(X, -0.63f, -0.045f), V(0.07f, 0.04f, 0.03f), Cap);
        }

        // Золотой ромбик — метка ключа (показывает, как вставлять процессор).
        Cube(Chip, V(-0.52f, -0.68f, -0.035f), V(0.09f, 0.09f, 0.01f), Gold, null, V(0f, 0f, 45f));

        // Обратная сторона: контакты.
        Create_Pad_Grid(Chip, 10, 12, 0.09f, V(0f, 0f, 0.045f));

        Create_Showcase_Stand(T, Rgb(0.00f, 0.44f, 0.78f), -0.60f);
    }

    // Процессор AMD: квадратная подложка, крышка-«осьминог» с восемью лапками,
    // оранжевая табличка, конденсаторы между лапками и контакты на обороте.
    private static void Create_Bonus_AMD(Transform T)
    {
        Color Gold = Rgb(0.85f, 0.70f, 0.25f);
        Color Cap = Rgb(0.45f, 0.33f, 0.18f);
        Color Metal = Rgb(0.78f, 0.80f, 0.84f);
        Color Orange = Rgb(0.93f, 0.30f, 0.08f);

        Transform Chip = Make_Group(T, V(0f, 0.15f, 0f), default, "AMD_CPU").transform;

        // Подложка.
        Cube(Chip, V(0f, 0f, 0f), V(1.30f, 1.30f, 0.06f), Rgb(0.08f, 0.42f, 0.26f));

        // Крышка: основное тело и восемь лапок (по две с каждой стороны).
        Cube(Chip, V(0f, 0f, -0.07f), V(0.80f, 0.80f, 0.08f), Metal);

        foreach (float Sign in new[] { -1f, 1f })
        {
            foreach (float Shift in new[] { -0.27f, 0.27f })
            {
                Cube(Chip, V(Sign * 0.49f, Shift, -0.07f), V(0.18f, 0.20f, 0.08f), Metal);   // лапки слева и справа
                Cube(Chip, V(Shift, Sign * 0.49f, -0.07f), V(0.20f, 0.18f, 0.08f), Metal);   // лапки сверху и снизу
            }
        }

        Cube(Chip, V(0f, 0f, -0.115f), V(0.70f, 0.70f, 0.015f), Rgb(0.87f, 0.89f, 0.92f));

        // Конденсаторы между лапками.
        Cube(Chip, V(-0.52f, 0f, -0.045f), V(0.08f, 0.14f, 0.03f), Cap);
        Cube(Chip, V(0.52f, 0f, -0.045f), V(0.08f, 0.14f, 0.03f), Cap);
        Cube(Chip, V(0f, 0.52f, -0.045f), V(0.14f, 0.08f, 0.03f), Cap);
        Cube(Chip, V(0f, -0.52f, -0.045f), V(0.14f, 0.08f, 0.03f), Cap);

        // Оранжевая табличка с белыми «буквами» и полоски гравировки.
        Cube(Chip, V(0f, 0.20f, -0.126f), V(0.50f, 0.14f, 0.01f), Orange);

        for (int I = 0; I < 4; I++)
            Cube(Chip, V(-0.15f + I * 0.1f, 0.20f, -0.133f), V(0.06f, 0.07f, 0.006f), Rgb(0.98f, 0.98f, 0.98f));

        for (int I = 0; I < 3; I++)
            Cube(Chip, V(0f, -0.05f - I * 0.12f, -0.126f), V(0.5f - I * 0.1f, 0.025f, 0.006f), Rgb(0.55f, 0.57f, 0.62f));

        // Метка ключа и контакты на обороте.
        Cube(Chip, V(-0.57f, -0.57f, -0.035f), V(0.09f, 0.09f, 0.01f), Gold, null, V(0f, 0f, 45f));
        Create_Pad_Grid(Chip, 12, 12, 0.085f, V(0f, 0f, 0.045f));

        Create_Showcase_Stand(T, Orange, -0.50f);
    }

    // Видеокарта Nvidia: тёмный кожух, два вентилятора по 9 лопастей, зелёные акценты,
    // разъём питания, планка и золотые контакты PCIe.
    private static void Create_Bonus_Nvidia(Transform T)
    {
        Color Green = Rgb(0.46f, 0.73f, 0.00f);
        Color Dark = Rgb(0.09f, 0.09f, 0.10f);

        Transform Card = Make_Group(T, V(0f, 0.15f, 0f), default, "Nvidia_GPU").transform;

        // Кожух, лицевая вставка и задняя пластина с зелёной полоской.
        Cube(Card, V(0f, 0f, 0f), V(1.90f, 0.80f, 0.14f), Dark);
        Cube(Card, V(0f, 0f, -0.08f), V(1.80f, 0.70f, 0.02f), Rgb(0.12f, 0.12f, 0.14f));
        Cube(Card, V(0f, 0f, 0.08f), V(1.90f, 0.80f, 0.02f), Rgb(0.18f, 0.18f, 0.20f));
        Cube(Card, V(0f, 0f, 0.092f), V(1.00f, 0.05f, 0.01f), Green);

        // Два вентилятора: тёмное кольцо, диск, девять лопастей, втулка и зелёная точка.
        foreach (float Fan_X in new[] { -0.5f, 0.5f })
        {
            Cylinder(Card, V(Fan_X, 0f, -0.095f), V(0.62f, 0.01f, 0.62f), Rgb(0.05f, 0.05f, 0.06f), null, V(90f, 0f, 0f));
            Cylinder(Card, V(Fan_X, 0f, -0.10f), V(0.56f, 0.01f, 0.56f), Rgb(0.15f, 0.15f, 0.17f), null, V(90f, 0f, 0f));

            // Лопасти расставлены по кругу через каждые 40° и чуть довернуты, как у настоящего вентилятора.
            for (int I = 0; I < 9; I++)
            {
                float Angle = I * 40f;
                float Rad = Angle * Mathf.Deg2Rad;

                Cube(Card, V(Fan_X + Mathf.Cos(Rad) * 0.18f, Mathf.Sin(Rad) * 0.18f, -0.108f),
                    V(0.22f, 0.07f, 0.01f), Rgb(0.22f, 0.22f, 0.25f), null, V(0f, 0f, Angle + 35f));
            }

            Cylinder(Card, V(Fan_X, 0f, -0.12f), V(0.16f, 0.015f, 0.16f), Rgb(0.08f, 0.08f, 0.09f), null, V(90f, 0f, 0f));
            Cylinder(Card, V(Fan_X, 0f, -0.132f), V(0.08f, 0.01f, 0.08f), Green, null, V(90f, 0f, 0f));
        }

        // Зелёные акценты: логотип между вентиляторами, полоса снизу и светящаяся линия сверху.
        Cube(Card, V(0f, 0f, -0.092f), V(0.20f, 0.20f, 0.012f), Green);
        Cube(Card, V(0f, 0f, -0.100f), V(0.12f, 0.04f, 0.008f), Dark);
        Cube(Card, V(0f, -0.33f, -0.092f), V(1.60f, 0.03f, 0.01f), Green);
        Cube(Card, V(0f, 0.402f, 0f), V(0.60f, 0.012f, 0.04f), Green);

        // Разъём питания, металлическая планка и золотые контакты PCIe.
        Cube(Card, V(0.55f, 0.43f, 0f), V(0.30f, 0.06f, 0.12f), Rgb(0.05f, 0.05f, 0.06f));
        Cube(Card, V(-0.965f, 0f, 0f), V(0.025f, 0.85f, 0.18f), Rgb(0.75f, 0.75f, 0.78f));
        Cube(Card, V(-0.10f, -0.445f, 0f), V(1.00f, 0.09f, 0.03f), Rgb(0.85f, 0.70f, 0.25f));

        Create_Showcase_Stand(T, Green, -0.49f);
    }
}

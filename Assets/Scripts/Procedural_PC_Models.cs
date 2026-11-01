using UnityEngine;

// Процедурная генерация 3D-моделей компьютеров пяти эпох.
//
// Все координаты — ЛОКАЛЬНЫЕ (относительно корня модели), поэтому положение PC_Spawn_Point (1,1,1)
// не смещает отдельные части модели друг относительно друга.
//
// Эпохи I–III: лицевая сторона обращена к камере (в сторону -Z).
// Эпохи IV–V (башенные корпуса): фронтальная панель по -X, открываемая боковая крышка по -Z,
// материнская плата закреплена у дальней стенки (+Z), все платы выступают из неё в сторону -Z.
// Съёмные детали выдвигаются в сторону -Z (через открытую боковину).
public class Procedural_PC_Models : MonoBehaviour
{
    // Насколько выдвигается съёмная деталь
    private static readonly Vector3 Part_Detach_Offset = new Vector3(0f, 0f, -0.9f);

    // Главный метод создания модели ПК для конкретной эпохи
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
                Root.transform.localScale = Vector3.one * 1.15f;
                break;

            case 3:
                Create_Era_4_Retro_PC(Root.transform);
                // Поворачиваем, чтобы сразу были видны и фронт, и боковая крышка
                Root.transform.localRotation = Quaternion.Euler(0f, -30f, 0f);
                break;

            case 4:
                Create_Era_5_Modern_PC(Root.transform);
                Root.transform.localRotation = Quaternion.Euler(0f, -30f, 0f);
                break;
        }

        return Root;
    }

    // ======================================================================
    // Вспомогательные методы
    // ======================================================================

    private static Vector3 V(float X, float Y, float Z)
    {
        return new Vector3(X, Y, Z);
    }

    private static Color Rgb(float R, float G, float B, float A = 1f)
    {
        return new Color(R, G, B, A);
    }

    // Пустой объект-контейнер для группы деталей
    private static GameObject Make_Group(Transform Parent, Vector3 Pos, Vector3 Rot = default, string Name = "Group")
    {
        GameObject Group = new GameObject(Name);
        Group.transform.SetParent(Parent, false);
        Group.transform.localPosition = Pos;
        Group.transform.localRotation = Quaternion.Euler(Rot);
        return Group;
    }

    // Создание одной детали.
    // Type != null  -> деталь интерактивна (кликабельна, участвует в вопросах и подсветке)
    // Solid == true -> деталь без типа, но с коллайдером (стенки корпуса: блокируют клики по внутренностям)
    // иначе коллайдер удаляется, и клик проходит сквозь декоративную деталь
    private static GameObject Make_Part(PrimitiveType Shape, Transform Parent, Vector3 Pos, Vector3 Scale,
        Color Paint, Component_Type? Type, Vector3 Rot, bool Solid)
    {
        GameObject Obj = GameObject.CreatePrimitive(Shape);
        Obj.transform.SetParent(Parent, false);
        Obj.transform.localPosition = Pos;
        Obj.transform.localRotation = Quaternion.Euler(Rot);
        Obj.transform.localScale = Scale;

        Renderer Rend = Obj.GetComponent<Renderer>();
        Rend.material.color = Paint;

        if (Paint.a < 0.99f)
            Make_Transparent(Rend.material);

        if (Type.HasValue)
        {
            Interactive_Component Comp = Obj.AddComponent<Interactive_Component>();
            Comp.Component_Type = Type.Value;
        }
        else if (!Solid)
        {
            Collider Col = Obj.GetComponent<Collider>();
            if (Col != null)
                Destroy(Col);
        }

        return Obj;
    }

    private static GameObject Cube(Transform Parent, Vector3 Pos, Vector3 Scale, Color Paint,
        Component_Type? Type = null, Vector3 Rot = default, bool Solid = false)
    {
        return Make_Part(PrimitiveType.Cube, Parent, Pos, Scale, Paint, Type, Rot, Solid);
    }

    // Ось цилиндра по умолчанию — Y, полная высота = 2 * Scale.y.
    // Rot = (90,0,0) направляет ось вдоль Z (диск смотрит на камеру), Rot = (0,0,90) — вдоль X.
    private static GameObject Cylinder(Transform Parent, Vector3 Pos, Vector3 Scale, Color Paint,
        Component_Type? Type = null, Vector3 Rot = default, bool Solid = false)
    {
        return Make_Part(PrimitiveType.Cylinder, Parent, Pos, Scale, Paint, Type, Rot, Solid);
    }

    private static GameObject Sphere(Transform Parent, Vector3 Pos, Vector3 Scale, Color Paint,
        Component_Type? Type = null)
    {
        return Make_Part(PrimitiveType.Sphere, Parent, Pos, Scale, Paint, Type, default, false);
    }

    // Режим Transparent для стандартного шейдера (Built-in Render Pipeline)
    private static void Make_Transparent(Material Mat)
    {
        Mat.SetFloat("_Mode", 3f);
        Mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        Mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        Mat.SetInt("_ZWrite", 0);
        Mat.DisableKeyword("_ALPHATEST_ON");
        Mat.EnableKeyword("_ALPHABLEND_ON");
        Mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        Mat.renderQueue = 3000;
    }

    // Сделать группу деталей съёмной (вызывать после установки позиции группы)
    private static void Make_Detachable(GameObject Part)
    {
        Detachable_Part Comp = Part.AddComponent<Detachable_Part>();
        Comp.Setup(Part_Detach_Offset);
    }

    // Провод между двумя точками (в плоскости XY) с вилками на концах
    private static void Create_Cable(Transform T, Vector3 A, Vector3 B, Color Paint, Component_Type Kind)
    {
        Vector3 D = B - A;
        float Angle = Mathf.Atan2(D.y, D.x) * Mathf.Rad2Deg;

        Cube(T, (A + B) * 0.5f, V(D.magnitude, 0.03f, 0.03f), Paint, Kind, V(0f, 0f, Angle));
        Sphere(T, A, V(0.07f, 0.07f, 0.07f), Paint, Kind);
        Sphere(T, B, V(0.07f, 0.07f, 0.07f), Paint, Kind);
    }

    // ======================================================================
    // Эпоха I: ламповые ЭВМ (1943–1955)
    // Задания: лампы, панель тумблеров, коммутационная панель, устройство чтения перфокарт
    // ======================================================================

    private static Vector3 Patch_Socket(int Row, int Col)
    {
        return V(-0.36f + Col * 0.144f, -0.2f - Row * 0.18f, -0.345f);
    }

    private static void Create_Vacuum_Tube(Transform T, Vector3 Pos)
    {
        Transform G = Make_Group(T, Pos, default, "Vacuum_Tube").transform;
        Color Glass = Rgb(0.55f, 0.92f, 0.95f, 0.6f);
        Component_Type Kind = Component_Type.Vacuum_Tube;

        Cylinder(G, V(0f, -0.13f, 0f), V(0.10f, 0.03f, 0.10f), Rgb(0.08f, 0.08f, 0.08f), Kind);
        Cylinder(G, V(0f, 0.02f, 0f), V(0.085f, 0.13f, 0.085f), Glass, Kind);
        Sphere(G, V(0f, 0.15f, 0f), V(0.085f, 0.085f, 0.085f), Glass, Kind);
        Cylinder(G, V(0f, 0f, 0f), V(0.02f, 0.09f, 0.02f), Rgb(1f, 0.65f, 0.2f), Kind);
    }

    private static void Create_Era_1_ENIAC(Transform T)
    {
        Color Steel = Rgb(0.30f, 0.31f, 0.34f);
        Color Dark = Rgb(0.10f, 0.10f, 0.12f);

        // Три шкафа: лампы | тумблеры и коммутатор | перфокарты и индикаторы
        float[] Cabinet_X = { -1.1f, 0f, 1.1f };

        foreach (float X in Cabinet_X)
        {
            Cube(T, V(X, 0f, 0f), V(1.05f, 2.4f, 0.6f), Steel);
            Cube(T, V(X, 1.24f, 0f), V(1.12f, 0.08f, 0.66f), Dark);
            Cube(T, V(X, -1.24f, 0f), V(1.12f, 0.08f, 0.66f), Dark);
        }

        // --- Левый шкаф: стойки с электронными лампами (4 ряда по 5 ламп)
        for (int Row = 0; Row < 4; Row++)
        {
            float Y = 0.8f - Row * 0.5f;

            Cube(T, V(-1.1f, Y - 0.17f, -0.37f), V(0.95f, 0.04f, 0.16f), Dark);

            for (int I = -2; I <= 2; I++)
                Create_Vacuum_Tube(T, V(-1.1f + I * 0.19f, Y, -0.38f));
        }

        // --- Центральный шкаф, верх: панель тумблеров (4 ряда по 6)
        Cube(T, V(0f, 0.6f, -0.32f), V(0.95f, 0.85f, 0.04f), Rgb(0.62f, 0.58f, 0.48f), Component_Type.Switch_Panel);

        for (int R = 0; R < 4; R++)
        {
            for (int C = 0; C < 6; C++)
            {
                float Tilt = ((R + C) % 2 == 0) ? 25f : -25f;

                Cube(T, V(-0.36f + C * 0.144f, 0.84f - R * 0.19f, -0.36f), V(0.05f, 0.10f, 0.05f),
                    Rgb(0.12f, 0.12f, 0.14f), Component_Type.Switch_Panel, V(Tilt, 0f, 0f));
            }
        }

        // --- Центральный шкаф, низ: коммутационная панель с гнёздами и проводами
        Cube(T, V(0f, -0.45f, -0.32f), V(0.95f, 0.8f, 0.04f), Rgb(0.08f, 0.08f, 0.10f), Component_Type.Patch_Panel);

        for (int R = 0; R < 4; R++)
        {
            for (int C = 0; C < 6; C++)
            {
                Cylinder(T, Patch_Socket(R, C), V(0.07f, 0.012f, 0.07f), Rgb(0.78f, 0.78f, 0.82f),
                    Component_Type.Patch_Panel, V(90f, 0f, 0f));
            }
        }

        Component_Type Patch = Component_Type.Patch_Panel;
        Create_Cable(T, Z_Front(Patch_Socket(0, 0)), Z_Front(Patch_Socket(1, 3)), Rgb(0.85f, 0.15f, 0.15f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(0, 1)), Z_Front(Patch_Socket(2, 4)), Rgb(0.95f, 0.80f, 0.15f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(1, 0)), Z_Front(Patch_Socket(3, 2)), Rgb(0.20f, 0.40f, 0.90f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(2, 1)), Z_Front(Patch_Socket(0, 5)), Rgb(0.15f, 0.70f, 0.30f), Patch);
        Create_Cable(T, Z_Front(Patch_Socket(3, 3)), Z_Front(Patch_Socket(1, 5)), Rgb(0.95f, 0.50f, 0.10f), Patch);

        // --- Правый шкаф, низ: устройство чтения перфокарт
        Component_Type Reader = Component_Type.Punch_Card_Reader;

        Cube(T, V(1.1f, -0.62f, -0.58f), V(0.9f, 0.55f, 0.55f), Rgb(0.55f, 0.50f, 0.40f), Reader);
        Cube(T, V(0.95f, -0.30f, -0.58f), V(0.5f, 0.10f, 0.30f), Rgb(0.45f, 0.42f, 0.35f), Reader);
        Cube(T, V(0.95f, -0.18f, -0.58f), V(0.42f, 0.14f, 0.24f), Rgb(0.95f, 0.92f, 0.78f), Reader);
        Cube(T, V(1.35f, -0.32f, -0.58f), V(0.25f, 0.10f, 0.30f), Rgb(0.95f, 0.92f, 0.78f), Reader);
        Cube(T, V(1.1f, -0.60f, -0.86f), V(0.5f, 0.05f, 0.02f), Rgb(0.05f, 0.05f, 0.05f), Reader);
        Sphere(T, V(1.30f, -0.48f, -0.86f), V(0.05f, 0.05f, 0.05f), Rgb(0.9f, 0.1f, 0.1f), Reader);
        Sphere(T, V(1.38f, -0.48f, -0.86f), V(0.05f, 0.05f, 0.05f), Rgb(0.1f, 0.9f, 0.2f), Reader);

        // --- Правый шкаф, верх: декоративные индикаторные лампы (не вопрос)
        Cube(T, V(1.1f, 0.55f, -0.31f), V(0.95f, 0.9f, 0.03f), Rgb(0.08f, 0.08f, 0.09f));

        for (int R = 0; R < 5; R++)
        {
            for (int C = 0; C < 8; C++)
            {
                Color Lamp = ((R * 8 + C) % 3 == 0) ? Rgb(1f, 0.6f, 0.1f) : Rgb(0.35f, 0.25f, 0.10f);
                Sphere(T, V(1.1f + (C - 3.5f) * 0.1f, 0.55f + (R - 2f) * 0.16f, -0.335f), V(0.05f, 0.05f, 0.05f), Lamp);
            }
        }
    }

    // Выносит точку чуть вперёд (для проводов поверх панели)
    private static Vector3 Z_Front(Vector3 P)
    {
        return V(P.x, P.y, -0.385f);
    }

    // ======================================================================
    // Эпоха II: транзисторные ЭВМ (1955–1965)
    // Задания: платы с транзисторами, магнитная лента, память на сердечниках, пульт оператора
    // ======================================================================

    private static void Create_Transistor_Card(Transform T, float X, float Y)
    {
        Component_Type Kind = Component_Type.Transistor;

        Cube(T, V(X, Y, -0.48f), V(0.14f, 0.55f, 0.03f), Rgb(0.10f, 0.45f, 0.22f), Kind);
        Cube(T, V(X, Y - 0.29f, -0.48f), V(0.10f, 0.04f, 0.03f), Rgb(0.85f, 0.70f, 0.20f), Kind);

        for (int J = 0; J < 3; J++)
        {
            float Part_Y = Y + (J - 1) * 0.15f;

            Cylinder(T, V(X, Part_Y, -0.51f), V(0.05f, 0.025f, 0.05f), Rgb(0.75f, 0.75f, 0.80f), Kind, V(90f, 0f, 0f));
            Cube(T, V(X, Part_Y + 0.07f, -0.505f), V(0.05f, 0.02f, 0.02f), Rgb(0.90f, 0.50f, 0.15f), Kind);
        }
    }

    private static void Create_Era_2_Mainframe(Transform T)
    {
        Color Blue_Gray = Rgb(0.58f, 0.64f, 0.70f);
        Color Dark = Rgb(0.10f, 0.11f, 0.13f);

        // --- Центральный шкаф процессора
        Cube(T, V(0f, 0f, 0f), V(1.6f, 2.2f, 0.8f), Blue_Gray);
        Cube(T, V(0f, 1.12f, 0f), V(1.66f, 0.06f, 0.86f), Dark);
        Cube(T, V(0f, -1.12f, 0f), V(1.66f, 0.06f, 0.86f), Dark);

        // Стойка с транзисторными платами (7 плат)
        Cube(T, V(0f, 0.65f, -0.43f), V(1.45f, 0.70f, 0.06f), Dark);

        for (int I = 0; I < 7; I++)
            Create_Transistor_Card(T, -0.6f + I * 0.2f, 0.65f);

        // Пульт оператора: наклонная панель с лампами и тумблерами
        Component_Type Console_Kind = Component_Type.Control_Console;
        Transform C = Make_Group(T, V(0f, -0.4f, -0.5f), V(20f, 0f, 0f), "Operator_Console").transform;

        Cube(C, V(0f, 0f, 0f), V(1.4f, 0.55f, 0.08f), Rgb(0.14f, 0.15f, 0.18f), Console_Kind);

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

        for (int Col = 0; Col < 10; Col++)
        {
            float Tilt = (Col % 2 == 0) ? 15f : -15f;

            Cube(C, V(-0.54f + Col * 0.12f, -0.16f, -0.05f), V(0.04f, 0.07f, 0.04f),
                Rgb(0.90f, 0.90f, 0.85f), Console_Kind, V(Tilt, 0f, 0f));
        }

        // --- Левый шкаф: накопитель на магнитной ленте
        Component_Type Tape = Component_Type.Magnetic_Tape;

        Cube(T, V(-1.3f, -0.1f, 0f), V(0.95f, 2.0f, 0.7f), Rgb(0.50f, 0.56f, 0.64f));
        Cube(T, V(-1.3f, 0.5f, -0.365f), V(0.85f, 0.90f, 0.03f), Rgb(0.10f, 0.12f, 0.15f), Tape);

        foreach (float Reel_X in new[] { -1.5f, -1.1f })
        {
            Cylinder(T, V(Reel_X, 0.65f, -0.395f), V(0.38f, 0.02f, 0.38f), Rgb(0.20f, 0.20f, 0.23f), Tape, V(90f, 0f, 0f));
            Cylinder(T, V(Reel_X, 0.65f, -0.415f), V(0.13f, 0.012f, 0.13f), Rgb(0.82f, 0.82f, 0.85f), Tape, V(90f, 0f, 0f));
            Cube(T, V(Reel_X, 0.10f, -0.385f), V(0.12f, 0.50f, 0.02f), Rgb(0.25f, 0.35f, 0.45f), Tape);
            Cube(T, V(Reel_X, 0.18f, -0.40f), V(0.012f, 0.38f, 0.012f), Rgb(0.40f, 0.25f, 0.10f), Tape);
        }

        // --- Правый шкаф: память на магнитных сердечниках
        Component_Type Core = Component_Type.Core_Memory;
        Color Gold = Rgb(0.85f, 0.70f, 0.25f);

        Cube(T, V(1.3f, -0.1f, 0f), V(0.95f, 2.0f, 0.7f), Rgb(0.52f, 0.58f, 0.56f));
        Cube(T, V(1.3f, 0.5f, -0.37f), V(0.75f, 0.75f, 0.04f), Rgb(0.20f, 0.15f, 0.10f), Core);

        for (int R = 0; R < 6; R++)
        {
            for (int Col = 0; Col < 6; Col++)
            {
                Cylinder(T, V(1.3f + (Col - 2.5f) * 0.1f, 0.5f + (R - 2.5f) * 0.1f, -0.40f),
                    V(0.055f, 0.012f, 0.055f), Rgb(0.32f, 0.20f, 0.12f), Core, V(90f, 0f, 0f));
            }
        }

        for (int R = 0; R < 6; R++)
            Cube(T, V(1.3f, 0.5f + (R - 2.5f) * 0.1f, -0.41f), V(0.62f, 0.008f, 0.008f), Gold, Core);

        for (int Col = 0; Col < 6; Col++)
            Cube(T, V(1.3f + (Col - 2.5f) * 0.1f, 0.5f, -0.42f), V(0.008f, 0.62f, 0.008f), Gold, Core);

        // Декоративные индикаторы под блоком памяти
        for (int I = 0; I < 8; I++)
        {
            Color Lamp = (I % 2 == 0) ? Rgb(1f, 0.7f, 0.1f) : Rgb(0.3f, 0.25f, 0.1f);
            Sphere(T, V(1.3f + (I - 3.5f) * 0.1f, -0.5f, -0.36f), V(0.05f, 0.05f, 0.05f), Lamp);
        }
    }

    // ======================================================================
    // Эпоха III: микрокомпьютер (1965–1980)
    // Задания: микросхема, дисковод 5.25", клавиатура, ЭЛТ-монитор
    // ======================================================================

    private static void Create_DIP_Chip(Transform P, Vector3 Pos, float W, float H)
    {
        Component_Type Kind = Component_Type.Integrated_Circuit;
        Color Pins = Rgb(0.72f, 0.72f, 0.75f);

        Cube(P, Pos, V(W, H, 0.05f), Rgb(0.07f, 0.07f, 0.08f), Kind);
        Cube(P, V(Pos.x, Pos.y + H * 0.5f + 0.012f, Pos.z + 0.01f), V(W - 0.02f, 0.025f, 0.02f), Pins, Kind);
        Cube(P, V(Pos.x, Pos.y - H * 0.5f - 0.012f, Pos.z + 0.01f), V(W - 0.02f, 0.025f, 0.02f), Pins, Kind);
    }

    private static void Create_Era_3_Microcomputer(Transform T)
    {
        Color Beige = Rgb(0.83f, 0.79f, 0.67f);

        // --- Системный блок
        Cube(T, V(-0.5f, -0.6f, 0f), V(1.8f, 0.5f, 1.2f), Beige);

        for (int I = 0; I < 5; I++)
            Cube(T, V(-1.2f + I * 0.07f, -0.6f, -0.605f), V(0.03f, 0.30f, 0.01f), Rgb(0.30f, 0.28f, 0.24f));

        // --- Дисковод 5.25"
        Component_Type Floppy = Component_Type.Floppy_Drive;
        Transform F = Make_Group(T, V(-0.1f, -0.6f, -0.62f), default, "Floppy_Drive").transform;

        Cube(F, V(0f, 0f, 0f), V(0.70f, 0.30f, 0.04f), Rgb(0.22f, 0.22f, 0.25f), Floppy);
        Cube(F, V(0f, 0.02f, -0.025f), V(0.50f, 0.04f, 0.015f), Rgb(0.02f, 0.02f, 0.02f), Floppy);
        Cube(F, V(0.20f, 0.02f, -0.035f), V(0.07f, 0.12f, 0.02f), Rgb(0.80f, 0.10f, 0.10f), Floppy);
        Cube(F, V(-0.28f, -0.10f, -0.025f), V(0.03f, 0.03f, 0.015f), Rgb(1f, 0.1f, 0.1f), Floppy);

        // --- Клавиатура (слегка наклонена)
        Component_Type Keys = Component_Type.Keyboard;
        Transform K = Make_Group(T, V(-0.5f, -0.83f, -1.05f), V(-10f, 0f, 0f), "Keyboard").transform;

        Cube(K, V(0f, 0f, 0f), V(1.30f, 0.06f, 0.50f), Rgb(0.78f, 0.74f, 0.64f), Keys);

        for (int R = 0; R < 3; R++)
        {
            for (int C = 0; C < 10; C++)
            {
                Color Key_Paint = ((R + C) % 4 == 0) ? Rgb(0.45f, 0.42f, 0.38f) : Rgb(0.90f, 0.88f, 0.80f);
                Cube(K, V((C - 4.5f) * 0.115f, 0.05f, 0.12f - R * 0.10f), V(0.09f, 0.04f, 0.08f), Key_Paint, Keys);
            }
        }

        Cube(K, V(0f, 0.05f, -0.18f), V(0.60f, 0.04f, 0.07f), Rgb(0.90f, 0.88f, 0.80f), Keys);

        // --- ЭЛТ-монитор на системном блоке
        Component_Type CRT = Component_Type.Monitor_CRT;
        Transform M = Make_Group(T, V(-0.6f, 0.1f, 0f), default, "CRT_Monitor").transform;

        Cube(M, V(0f, 0f, -0.30f), V(1.15f, 0.90f, 0.10f), Rgb(0.85f, 0.80f, 0.68f), CRT);
        Cube(M, V(0f, 0f, 0f), V(1.00f, 0.80f, 0.45f), Rgb(0.80f, 0.76f, 0.64f), CRT);
        Cube(M, V(0f, 0f, 0.38f), V(0.60f, 0.50f, 0.35f), Rgb(0.76f, 0.72f, 0.60f), CRT);
        Cube(M, V(0f, 0f, -0.355f), V(0.92f, 0.72f, 0.03f), Rgb(0.04f, 0.15f, 0.08f), CRT);

        float[] Line_Width = { 0.50f, 0.70f, 0.35f, 0.60f, 0.25f };
        float[] Line_Y = { 0.25f, 0.14f, 0.03f, -0.08f, -0.19f };

        for (int I = 0; I < Line_Width.Length; I++)
            Cube(M, V(-0.42f + Line_Width[I] * 0.5f, Line_Y[I], -0.375f), V(Line_Width[I], 0.03f, 0.01f),
                Rgb(0.35f, 1f, 0.50f), CRT);

        // --- Открытая плата с микросхемами на подставке
        Cube(T, V(1.0f, -0.82f, -0.15f), V(0.95f, 0.04f, 0.40f), Rgb(0.15f, 0.15f, 0.17f));

        Transform B = Make_Group(T, V(1.0f, -0.4f, -0.15f), default, "IC_Board").transform;

        // Сама плата — отвлекающий элемент
        Cube(B, V(0f, 0f, 0f), V(0.95f, 0.80f, 0.04f), Rgb(0.08f, 0.38f, 0.20f), Component_Type.Motherboard);

        // Большая 40-выводная микросхема (процессор)
        Create_DIP_Chip(B, V(0f, 0.20f, -0.045f), 0.55f, 0.14f);

        // Шесть микросхем поменьше
        for (int R = 0; R < 2; R++)
        {
            for (int C = 0; C < 3; C++)
                Create_DIP_Chip(B, V((C - 1) * 0.28f, -0.05f - R * 0.22f, -0.045f), 0.20f, 0.09f);
        }

        // Конденсаторы — декор
        for (int I = 0; I < 4; I++)
            Cylinder(B, V(-0.35f + I * 0.23f, -0.37f, -0.04f), V(0.06f, 0.04f, 0.06f), Rgb(0.10f, 0.20f, 0.70f),
                null, V(90f, 0f, 0f));
    }

    // ======================================================================
    // Общий каркас башенного корпуса (эпохи IV и V)
    // Глубина (фронт-тыл) — по X, высота — по Y, ширина — по Z.
    // Фронт — по -X, открытая сторона — по -Z, дальняя стенка (под плату) — по +Z.
    // Возвращает Transform крышки, к которому можно докрепить декор.
    // ======================================================================

    private static Transform Build_Tower_Shell(Transform T, float Depth, float Height, float Width,
        Color Body, Color Front_Paint, Color Cover_Paint)
    {
        const float Wall = 0.05f;

        Cube(T, V(Depth / 2f - Wall / 2f, 0f, 0f), V(Wall, Height, Width), Body, Solid: true);                 // задняя стенка
        Cube(T, V(0f, Height / 2f - Wall / 2f, 0f), V(Depth, Wall, Width), Body, Solid: true);                 // верх
        Cube(T, V(0f, -Height / 2f + Wall / 2f, 0f), V(Depth, Wall, Width), Body, Solid: true);                // низ
        Cube(T, V(0f, 0f, Width / 2f - Wall / 2f), V(Depth, Height, Wall), Body, Solid: true);                 // дальняя стенка (+Z)
        Cube(T, V(-Depth / 2f + 0.04f, 0f, 0f), V(0.08f, Height, Width), Front_Paint, Solid: true);            // фронтальная панель

        // Боковая крышка: стоит по -Z и при открытии сдвигается назад (+X)
        GameObject Pivot = Make_Group(T, V(Depth / 2f, 0f, -Width / 2f - 0.025f), default, "SideCover");
        Openable_Cover Cover = Pivot.AddComponent<Openable_Cover>();
        Cover.Setup(V(Depth + 0.1f, 0f, 0f));

        Cube(Pivot.transform, V(-Depth / 2f, 0f, 0f), V(Depth, Height, 0.05f), Cover_Paint, Component_Type.Case_Cover);

        return Pivot.transform;
    }

    // ======================================================================
    // Эпоха IV: ретро-ПК (1980–2000), разборный
    // Задания: открыть крышку, процессор, ОЗУ, 3D-видеокарта, жёсткий диск
    // Отвлекающие элементы: дисководы, CD-привод, звуковая плата, блок питания, материнская плата
    // Съёмные детали: процессор с радиатором, 2 модуля ОЗУ, видеокарта, звуковая плата, жёсткий диск
    // ======================================================================

    private static void Create_Retro_RAM(Transform T, float X)
    {
        // Разъём на плате (отвлекающий элемент)
        Cube(T, V(X, 0.10f, 0.40f), V(0.05f, 0.55f, 0.03f), Rgb(0.08f, 0.08f, 0.09f), Component_Type.Motherboard);

        GameObject Stick = Make_Group(T, V(X, 0.10f, 0f), default, "RAM_Stick");
        Transform S = Stick.transform;
        Component_Type Kind = Component_Type.RAM;

        Cube(S, V(0f, 0f, 0.29f), V(0.025f, 0.50f, 0.22f), Rgb(0.12f, 0.45f, 0.25f), Kind);

        for (int Col = 0; Col < 2; Col++)
        {
            for (int I = 0; I < 4; I++)
                Cube(S, V(-0.018f, -0.15f + I * 0.10f, 0.34f - Col * 0.09f), V(0.012f, 0.07f, 0.06f),
                    Rgb(0.08f, 0.08f, 0.09f), Kind);
        }

        Make_Detachable(Stick);
    }

    private static void Create_Front_Drive(Transform T, float Y, float H, Component_Type Kind, Color Face_Paint, bool Is_Floppy)
    {
        const float Z_Size = 0.5f;

        Transform D = Make_Group(T, V(-0.62f, Y, 0f), default, "Front_Drive").transform;

        Cube(D, V(0f, 0f, 0f), V(0.50f, H, Z_Size), Rgb(0.60f, 0.60f, 0.63f), Kind);
        Cube(D, V(-0.35f, 0f, 0f), V(0.04f, H, Z_Size), Face_Paint, Kind);

        if (Is_Floppy)
        {
            Cube(D, V(-0.372f, 0f, 0f), V(0.01f, 0.03f, Z_Size * 0.8f), Rgb(0.02f, 0.02f, 0.02f), Kind);
            Cube(D, V(-0.375f, 0f, Z_Size * 0.3f), V(0.012f, H * 0.6f, 0.05f), Rgb(0.80f, 0.10f, 0.10f), Kind);
        }
        else
        {
            Cube(D, V(-0.372f, 0.03f, 0f), V(0.01f, 0.02f, Z_Size * 0.85f), Rgb(0.25f, 0.25f, 0.28f), Kind);
            Cube(D, V(-0.372f, -0.05f, Z_Size * 0.3f), V(0.01f, 0.03f, 0.06f), Rgb(0.45f, 0.45f, 0.48f), Kind);
        }
    }

    private static void Create_Era_4_Retro_PC(Transform T)
    {
        const float Depth = 1.9f;
        const float Height = 2.2f;
        const float Width = 1.0f;

        Transform Cover = Build_Tower_Shell(T, Depth, Height, Width,
            Rgb(0.72f, 0.69f, 0.60f), Rgb(0.84f, 0.81f, 0.72f), Rgb(0.78f, 0.75f, 0.66f));

        // Декор крышки: вентиляционные прорези и табличка
        for (int I = 0; I < 6; I++)
            Cube(Cover, V(-0.5f, -0.9f + I * 0.07f, -0.03f), V(0.5f, 0.03f, 0.01f), Rgb(0.50f, 0.47f, 0.40f));

        Cube(Cover, V(-1.5f, 0.9f, -0.03f), V(0.25f, 0.08f, 0.01f), Rgb(0.20f, 0.20f, 0.25f));

        // Декор фронта: кнопка питания и светодиоды
        Cube(T, V(-0.965f, -0.55f, 0.15f), V(0.03f, 0.10f, 0.10f), Rgb(0.25f, 0.25f, 0.28f));
        Cube(T, V(-0.96f, -0.35f, 0.30f), V(0.02f, 0.04f, 0.04f), Rgb(0.10f, 0.90f, 0.20f));
        Cube(T, V(-0.96f, -0.35f, 0.20f), V(0.02f, 0.04f, 0.04f), Rgb(0.90f, 0.60f, 0.10f));

        // Передние отсеки: дисковод 5.25", CD-привод, дисковод 3.5" (не съёмные)
        Create_Front_Drive(T, 0.80f, 0.20f, Component_Type.Floppy_Drive, Rgb(0.80f, 0.77f, 0.68f), true);
        Create_Front_Drive(T, 0.55f, 0.20f, Component_Type.CD_Drive, Rgb(0.82f, 0.79f, 0.70f), false);
        Create_Front_Drive(T, 0.33f, 0.12f, Component_Type.Floppy_Drive, Rgb(0.80f, 0.77f, 0.68f), true);

        // Блок питания в верхней задней части
        Cube(T, V(0.55f, 0.79f, 0f), V(0.70f, 0.48f, 0.80f), Rgb(0.55f, 0.55f, 0.58f), Component_Type.PSU);
        Cylinder(T, V(0.55f, 0.79f, -0.403f), V(0.30f, 0.004f, 0.30f), Rgb(0.12f, 0.12f, 0.14f), Component_Type.PSU, V(90f, 0f, 0f));

        // Материнская плата и гнездо процессора
        Cube(T, V(0.20f, -0.20f, 0.43f), V(1.25f, 1.40f, 0.03f), Rgb(0.10f, 0.38f, 0.20f), Component_Type.Motherboard);
        Cube(T, V(0.35f, 0.20f, 0.40f), V(0.30f, 0.30f, 0.03f), Rgb(0.85f, 0.85f, 0.80f), Component_Type.Motherboard);

        // Процессор с радиатором и вентилятором (съёмный)
        GameObject CPU_Group = Make_Group(T, V(0.35f, 0.20f, 0f), default, "CPU");
        Transform C = CPU_Group.transform;
        Component_Type CPU_Kind = Component_Type.CPU;

        Cube(C, V(0f, 0f, 0.365f), V(0.24f, 0.24f, 0.04f), Rgb(0.18f, 0.18f, 0.20f), CPU_Kind);
        Cube(C, V(0f, 0f, 0.33f), V(0.26f, 0.26f, 0.03f), Rgb(0.72f, 0.74f, 0.78f), CPU_Kind);

        for (int I = 0; I < 5; I++)
            Cube(C, V(0f, -0.10f + I * 0.05f, 0.265f), V(0.26f, 0.025f, 0.10f), Rgb(0.65f, 0.67f, 0.72f), CPU_Kind);

        Cube(C, V(0f, 0f, 0.19f), V(0.22f, 0.22f, 0.03f), Rgb(0.10f, 0.10f, 0.12f), CPU_Kind);
        Cylinder(C, V(0f, 0f, 0.172f), V(0.09f, 0.008f, 0.09f), Rgb(0.50f, 0.50f, 0.55f), CPU_Kind, V(90f, 0f, 0f));
        Make_Detachable(CPU_Group);

        // Два модуля оперативной памяти (съёмные)
        Create_Retro_RAM(T, 0.62f);
        Create_Retro_RAM(T, 0.70f);

        // Видеокарта AGP (съёмная)
        GameObject GPU_Group = Make_Group(T, V(0f, -0.35f, 0f), default, "GPU");
        Transform G = GPU_Group.transform;
        Component_Type GPU_Kind = Component_Type.GPU;

        Cube(G, V(0.36f, 0f, 0.245f), V(0.82f, 0.03f, 0.34f), Rgb(0.12f, 0.42f, 0.30f), GPU_Kind);
        Cube(G, V(0.30f, 0f, 0.40f), V(0.40f, 0.03f, 0.03f), Rgb(0.85f, 0.70f, 0.20f), GPU_Kind);
        Cube(G, V(0.20f, 0.045f, 0.24f), V(0.22f, 0.06f, 0.20f), Rgb(0.72f, 0.74f, 0.78f), GPU_Kind);
        Cylinder(G, V(0.20f, 0.085f, 0.24f), V(0.16f, 0.01f, 0.16f), Rgb(0.10f, 0.10f, 0.12f), GPU_Kind);

        for (int X = 0; X < 3; X++)
        {
            for (int Z = 0; Z < 2; Z++)
                Cube(G, V(0.45f + X * 0.10f, 0.027f, 0.18f + Z * 0.12f), V(0.06f, 0.025f, 0.05f), Rgb(0.08f, 0.08f, 0.09f), GPU_Kind);
        }

        Cube(G, V(0.78f, 0.03f, 0.245f), V(0.015f, 0.12f, 0.34f), Rgb(0.75f, 0.75f, 0.78f), GPU_Kind);
        Make_Detachable(GPU_Group);

        // Звуковая плата (съёмная, отвлекающий элемент)
        GameObject Sound_Group = Make_Group(T, V(0f, -0.62f, 0f), default, "Sound_Card");
        Transform S = Sound_Group.transform;
        Component_Type Sound_Kind = Component_Type.Sound_Card;

        Cube(S, V(0.45f, 0f, 0.275f), V(0.60f, 0.03f, 0.28f), Rgb(0.55f, 0.25f, 0.12f), Sound_Kind);
        Cube(S, V(0.45f, 0f, 0.40f), V(0.30f, 0.03f, 0.03f), Rgb(0.85f, 0.70f, 0.20f), Sound_Kind);
        Cube(S, V(0.40f, 0.027f, 0.28f), V(0.12f, 0.025f, 0.08f), Rgb(0.08f, 0.08f, 0.09f), Sound_Kind);
        Cube(S, V(0.60f, 0.027f, 0.22f), V(0.08f, 0.025f, 0.06f), Rgb(0.08f, 0.08f, 0.09f), Sound_Kind);
        Cube(S, V(0.755f, 0.03f, 0.275f), V(0.015f, 0.10f, 0.28f), Rgb(0.75f, 0.75f, 0.78f), Sound_Kind);
        Make_Detachable(Sound_Group);

        // Жёсткий диск (съёмный)
        GameObject HDD_Group = Make_Group(T, V(-0.50f, -0.96f, -0.10f), default, "HDD");
        Transform H = HDD_Group.transform;
        Component_Type HDD_Kind = Component_Type.Hard_Drive;

        Cube(H, V(0f, 0f, 0f), V(0.60f, 0.14f, 0.42f), Rgb(0.62f, 0.64f, 0.68f), HDD_Kind);
        Cube(H, V(0f, 0.073f, 0f), V(0.40f, 0.006f, 0.26f), Rgb(0.92f, 0.92f, 0.90f), HDD_Kind);
        Cube(H, V(0.31f, 0f, 0f), V(0.03f, 0.08f, 0.20f), Rgb(0.15f, 0.15f, 0.15f), HDD_Kind);
        Make_Detachable(HDD_Group);
    }

    // ======================================================================
    // Эпоха V: современный ПК (с 2000-х), разборный
    // Задания: открыть стекло, охлаждение процессора, ОЗУ, видеокарта, NVMe SSD
    // Отвлекающие элементы: блок питания, материнская плата
    // Съёмные детали: СЖО, 2 модуля ОЗУ, видеокарта, NVMe SSD
    // ======================================================================

    private static void Create_Modern_RAM(Transform T, float X, Color Glow)
    {
        // Разъём на плате (отвлекающий элемент)
        Cube(T, V(X, 0.40f, 0.40f), V(0.05f, 0.60f, 0.03f), Rgb(0.05f, 0.05f, 0.06f), Component_Type.Motherboard);

        GameObject Stick = Make_Group(T, V(X, 0.40f, 0f), default, "RAM_Stick");
        Transform S = Stick.transform;
        Component_Type Kind = Component_Type.RAM;

        Cube(S, V(0f, 0f, 0.305f), V(0.020f, 0.55f, 0.22f), Rgb(0.10f, 0.30f, 0.20f), Kind);
        Cube(S, V(0f, 0f, 0.300f), V(0.036f, 0.50f, 0.20f), Rgb(0.10f, 0.10f, 0.12f), Kind);
        Cube(S, V(0f, 0.265f, 0.300f), V(0.040f, 0.03f, 0.21f), Glow, Kind);
        Cube(S, V(0f, 0f, 0.195f), V(0.037f, 0.50f, 0.02f), Glow, Kind);
        Make_Detachable(Stick);
    }

    private static void Create_Era_5_Modern_PC(Transform T)
    {
        const float Depth = 1.8f;
        const float Height = 2.2f;
        const float Width = 1.0f;

        // Тёмный корпус со стеклянной боковой панелью
        Transform Cover = Build_Tower_Shell(T, Depth, Height, Width,
            Rgb(0.10f, 0.10f, 0.12f), Rgb(0.07f, 0.07f, 0.09f), Rgb(0.45f, 0.70f, 0.95f, 0.28f));

        // Рамка стекла
        Color Frame = Rgb(0.05f, 0.05f, 0.06f);
        Cube(Cover, V(-Depth / 2f, Height / 2f - 0.02f, -0.035f), V(Depth, 0.04f, 0.01f), Frame);
        Cube(Cover, V(-Depth / 2f, -Height / 2f + 0.02f, -0.035f), V(Depth, 0.04f, 0.01f), Frame);
        Cube(Cover, V(-Depth + 0.02f, 0f, -0.035f), V(0.04f, Height, 0.01f), Frame);
        Cube(Cover, V(-0.02f, 0f, -0.035f), V(0.04f, Height, 0.01f), Frame);

        // Кожух блока питания внизу (отвлекающий элемент)
        Cube(T, V(0f, -0.85f, 0f), V(1.65f, 0.40f, 0.85f), Rgb(0.05f, 0.05f, 0.06f), Component_Type.PSU);
        Cube(T, V(0.30f, -0.85f, -0.43f), V(0.50f, 0.04f, 0.01f), Rgb(0.80f, 0.10f, 0.10f), Component_Type.PSU);

        // Материнская плата и радиаторы на ней (отвлекающие элементы)
        Cube(T, V(0.10f, 0.20f, 0.43f), V(1.35f, 1.50f, 0.03f), Rgb(0.06f, 0.06f, 0.08f), Component_Type.Motherboard);
        Cube(T, V(-0.45f, 0.55f, 0.38f), V(0.12f, 0.45f, 0.07f), Rgb(0.20f, 0.20f, 0.22f), Component_Type.Motherboard);
        Cube(T, V(0.45f, -0.40f, 0.39f), V(0.22f, 0.20f, 0.05f), Rgb(0.20f, 0.20f, 0.22f), Component_Type.Motherboard);

        // Жидкостное охлаждение процессора: помпа, трубки и радиатор (съёмное целиком)
        GameObject Cooler_Group = Make_Group(T, V(0f, 0f, 0f), default, "Cooler");
        Transform K = Cooler_Group.transform;
        Component_Type Cooler_Kind = Component_Type.Cooler;
        Color Tube_Paint = Rgb(0.07f, 0.07f, 0.08f);

        Cylinder(K, V(0.05f, 0.50f, 0.34f), V(0.28f, 0.04f, 0.28f), Rgb(0.12f, 0.12f, 0.14f), Cooler_Kind, V(90f, 0f, 0f));
        Cylinder(K, V(0.05f, 0.50f, 0.30f), V(0.30f, 0.012f, 0.30f), Rgb(0.00f, 0.85f, 1.00f), Cooler_Kind, V(90f, 0f, 0f));
        Cylinder(K, V(0.05f, 0.50f, 0.288f), V(0.20f, 0.010f, 0.20f), Rgb(0.05f, 0.05f, 0.07f), Cooler_Kind, V(90f, 0f, 0f));
        Cylinder(K, V(-0.38f, 0.57f, 0.34f), V(0.04f, 0.29f, 0.04f), Tube_Paint, Cooler_Kind, V(0f, 0f, 90f));
        Cylinder(K, V(-0.38f, 0.43f, 0.34f), V(0.04f, 0.29f, 0.04f), Tube_Paint, Cooler_Kind, V(0f, 0f, 90f));
        Cube(K, V(-0.72f, 0.35f, 0.20f), V(0.10f, 1.00f, 0.36f), Rgb(0.12f, 0.12f, 0.14f), Cooler_Kind);

        for (int I = 0; I < 12; I++)
            Cube(K, V(-0.72f, -0.10f + I * 0.08f, 0.015f), V(0.09f, 0.02f, 0.01f), Rgb(0.40f, 0.40f, 0.45f), Cooler_Kind);

        Make_Detachable(Cooler_Group);

        // Два модуля ОЗУ с RGB-подсветкой (съёмные)
        Create_Modern_RAM(T, 0.45f, Rgb(1.00f, 0.20f, 0.80f));
        Create_Modern_RAM(T, 0.53f, Rgb(0.20f, 0.90f, 1.00f));

        // NVMe SSD формата M.2 (съёмный)
        GameObject SSD_Group = Make_Group(T, V(0.10f, 0f, 0f), default, "NVMe_SSD");
        Transform N = SSD_Group.transform;
        Component_Type SSD_Kind = Component_Type.SSD_NVMe;

        Cube(N, V(0f, 0f, 0.405f), V(0.34f, 0.08f, 0.012f), Rgb(0.10f, 0.28f, 0.65f), SSD_Kind);
        Cube(N, V(-0.07f, 0f, 0.395f), V(0.09f, 0.065f, 0.01f), Rgb(0.10f, 0.10f, 0.11f), SSD_Kind);
        Cube(N, V(0.04f, 0f, 0.395f), V(0.09f, 0.065f, 0.01f), Rgb(0.10f, 0.10f, 0.11f), SSD_Kind);
        Cube(N, V(0.12f, 0f, 0.395f), V(0.05f, 0.05f, 0.01f), Rgb(0.35f, 0.35f, 0.38f), SSD_Kind);
        Make_Detachable(SSD_Group);

        // Видеокарта с тремя вентиляторами (съёмная)
        GameObject GPU_Group = Make_Group(T, V(0f, -0.15f, 0f), default, "GPU");
        Transform G = GPU_Group.transform;
        Component_Type GPU_Kind = Component_Type.GPU;

        Cube(G, V(0.10f, 0f, 0.20f), V(1.25f, 0.20f, 0.34f), Rgb(0.08f, 0.08f, 0.10f), GPU_Kind);
        Cube(G, V(0.15f, 0f, 0.392f), V(0.80f, 0.05f, 0.045f), Rgb(0.15f, 0.15f, 0.17f), GPU_Kind);
        Cube(G, V(0.10f, 0.11f, 0.20f), V(1.25f, 0.02f, 0.34f), Rgb(0.20f, 0.20f, 0.22f), GPU_Kind);
        Cube(G, V(0.60f, 0.12f, 0.30f), V(0.15f, 0.04f, 0.05f), Rgb(0.05f, 0.05f, 0.06f), GPU_Kind);
        Cube(G, V(0.735f, 0f, 0.20f), V(0.015f, 0.20f, 0.34f), Rgb(0.75f, 0.75f, 0.78f), GPU_Kind);

        foreach (float Fan_X in new[] { -0.2f, 0.1f, 0.4f })
        {
            Cylinder(G, V(Fan_X, 0f, 0.027f), V(0.19f, 0.004f, 0.19f), Rgb(0.00f, 0.85f, 1.00f), GPU_Kind, V(90f, 0f, 0f));
            Cylinder(G, V(Fan_X, 0f, 0.021f), V(0.16f, 0.004f, 0.16f), Rgb(0.10f, 0.10f, 0.12f), GPU_Kind, V(90f, 0f, 0f));
            Cylinder(G, V(Fan_X, 0f, 0.015f), V(0.05f, 0.004f, 0.05f), Rgb(0.55f, 0.55f, 0.60f), GPU_Kind, V(90f, 0f, 0f));
        }

        Make_Detachable(GPU_Group);
    }
}

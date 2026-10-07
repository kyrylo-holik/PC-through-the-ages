using UnityEngine;

public class ProceduralPCModels : MonoBehaviour
{
    public static GameObject CreateEraModel(int eraIndex, Transform parent)
    {
        GameObject root = new GameObject("Computer_Era_" + eraIndex);
        root.transform.SetParent(parent);

        switch (eraIndex)
        {
            case 0: CreateEra1_ENIAC(root); break;
            case 1: CreateEra2_Mainframe(root); break;
            case 2: CreateEra3_Microcomputer(root); break;
            case 3: CreateEra4_RetroPC(root); break;
            case 4: CreateEra5_ModernPC(root); break;
        }
        return root;
    }

    private static void AttachComponent(GameObject obj, ComponentType type, string nameRU, string nameEN)
    {
        InteractiveComponent comp = obj.AddComponent<InteractiveComponent>();
        comp.componentType = type;
        comp.componentNameRU = nameRU;
        comp.componentNameEN = nameEN;
        if (!obj.GetComponent<Collider>()) obj.AddComponent<BoxCollider>();
    }

    // Эпоха 1: 1940-1950 Ламповые шкафы
    private static void CreateEra1_ENIAC(GameObject parent)
    {
        GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinet.transform.SetParent(parent.transform);
        cabinet.transform.localScale = new Vector3(3f, 2.5f, 0.8f);
        cabinet.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        // Электронные лампы
        for (int i = -2; i <= 2; i++)
        {
            GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tube.transform.SetParent(parent.transform);
            tube.transform.position = new Vector3(i * 0.5f, 0.5f, -0.45f);
            tube.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            tube.GetComponent<Renderer>().material.color = Color.cyan;
            AttachComponent(tube, ComponentType.VacuumTube, "Электронная лампа", "Vacuum Tube");
        }

        // Переключатели / Логика
        GameObject switches = GameObject.CreatePrimitive(PrimitiveType.Cube);
        switches.transform.SetParent(parent.transform);
        switches.transform.position = new Vector3(0, -0.5f, -0.45f);
        switches.transform.localScale = new Vector3(2f, 0.6f, 0.1f);
        switches.GetComponent<Renderer>().material.color = Color.gray;
        AttachComponent(switches, ComponentType.CPU, "Ламповый вычислетель", "Tube Logic Circuit");
    }

    // Эпоха 2: 1960-е Транзисторы и магнитофоны
    private static void CreateEra2_Mainframe(GameObject parent)
    {
        GameObject mainFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainFrame.transform.SetParent(parent.transform);
        mainFrame.transform.localScale = new Vector3(2f, 2f, 1f);
        mainFrame.GetComponent<Renderer>().material.color = Color.darkGray;

        // Магнитные бобины (Накопитель)
        GameObject tape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tape.transform.SetParent(parent.transform);
        tape.transform.position = new Vector3(0, 0.4f, -0.55f);
        tape.transform.rotation = Quaternion.Euler(90, 0, 0);
        tape.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
        tape.GetComponent<Renderer>().material.color = Color.black;
        AttachComponent(tape, ComponentType.Storage, "Ленточный накопитель", "Magnetic Tape Storage");

        // Плата с транзисторами
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.transform.SetParent(parent.transform);
        board.transform.position = new Vector3(0, -0.4f, -0.55f);
        board.transform.localScale = new Vector3(1.5f, 0.5f, 0.1f);
        board.GetComponent<Renderer>().material.color = Color.green;
        AttachComponent(board, ComponentType.Transistor, "Транзисторный модуль", "Transistor Board");
    }

    // Эпоха 3: 1970-1980 Первые моноблоки / Микро-ЭВМ
    private static void CreateEra3_Microcomputer(GameObject parent)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(parent.transform);
        body.transform.localScale = new Vector3(1.5f, 0.4f, 1.2f);
        body.GetComponent<Renderer>().material.color = new Color(0.8f, 0.75f, 0.65f); // Бежевый

        // Дисковод
        GameObject floppyDrive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floppyDrive.transform.SetParent(parent.transform);
        floppyDrive.transform.position = new Vector3(0.3f, 0f, -0.61f);
        floppyDrive.transform.localScale = new Vector3(0.5f, 0.15f, 0.05f);
        floppyDrive.GetComponent<Renderer>().material.color = Color.black;
        AttachComponent(floppyDrive, ComponentType.Storage, "Дисковод 5.25\"", "5.25 Floppy Drive");

        // Клавиатура в корпусе
        GameObject kb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        kb.transform.SetParent(parent.transform);
        kb.transform.position = new Vector3(0f, 0.1f, 0.2f);
        kb.transform.localScale = new Vector3(1.2f, 0.1f, 0.5f);
        kb.GetComponent<Renderer>().material.color = Color.gray;
        AttachComponent(kb, ComponentType.Peripheral, "Встроенная клавиатура", "Built-in Keyboard");
    }

    // Эпоха 4: 1990-2000 ПК классика
    private static void CreateEra4_RetroPC(GameObject parent)
    {
        // Корпус
        GameObject caseBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        caseBody.transform.SetParent(parent.transform);
        caseBody.transform.localScale = new Vector3(0.8f, 1.2f, 1.2f);
        caseBody.GetComponent<Renderer>().material.color = new Color(0.85f, 0.82f, 0.75f);

        // Открывающаяся боковая крышка
        GameObject sideCover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sideCover.name = "SideCover";
        sideCover.transform.SetParent(parent.transform);
        sideCover.transform.position = new Vector3(0.41f, 0f, 0f);
        sideCover.transform.localScale = new Vector3(0.02f, 1.18f, 1.18f);
        sideCover.GetComponent<Renderer>().material.color = new Color(0.75f, 0.72f, 0.65f);

        // Процессор внутри
        GameObject cpu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cpu.transform.SetParent(parent.transform);
        cpu.transform.position = new Vector3(0f, 0.2f, 0f);
        cpu.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);
        cpu.GetComponent<Renderer>().material.color = Color.black;
        AttachComponent(cpu, ComponentType.CPU, "Процессор Pentium", "Pentium CPU");

        // Видеокарта AGP
        GameObject gpu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gpu.transform.SetParent(parent.transform);
        gpu.transform.position = new Vector3(0f, -0.2f, -0.2f);
        gpu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
        gpu.GetComponent<Renderer>().material.color = Color.green;
        AttachComponent(gpu, ComponentType.GPU, "Видеокарта AGP", "AGP Graphics Card");
    }

    // Эпоха 5: Современный Игровой ПК
    private static void CreateEra5_ModernPC(GameObject parent)
    {
        GameObject caseBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
        caseBody.transform.SetParent(parent.transform);
        caseBody.transform.localScale = new Vector3(0.9f, 1.4f, 1.3f);
        caseBody.GetComponent<Renderer>().material.color = new Color(0.1f, 0.1f, 0.1f);

        // Прозрачная стеклянная панель
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glass.name = "SideCover";
        glass.transform.SetParent(parent.transform);
        glass.transform.position = new Vector3(0.46f, 0f, 0f);
        glass.transform.localScale = new Vector3(0.02f, 1.35f, 1.25f);
        Material glassMat = glass.GetComponent<Renderer>().material;
        glassMat.color = new Color(0f, 0.5f, 1f, 0.4f);

        // Мощная видеокарта RTX
        GameObject gpu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gpu.transform.SetParent(parent.transform);
        gpu.transform.position = new Vector3(0f, -0.1f, 0f);
        gpu.transform.localScale = new Vector3(0.3f, 0.25f, 0.7f);
        gpu.GetComponent<Renderer>().material.color = Color.magenta;
        AttachComponent(gpu, ComponentType.GPU, "Современная Видеокарта", "Modern GPU");

        // NVMe SSD
        GameObject ssd = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ssd.transform.SetParent(parent.transform);
        ssd.transform.position = new Vector3(0f, -0.4f, 0.2f);
        ssd.transform.localScale = new Vector3(0.15f, 0.02f, 0.3f);
        ssd.GetComponent<Renderer>().material.color = Color.blue;
        AttachComponent(ssd, ComponentType.Storage, "M.2 NVMe SSD", "M.2 NVMe SSD");
    }
}
using UnityEngine;

// Класс для процедурной генерации 3D-моделей компьютеров различных эпох
public class ProceduralPCModels : MonoBehaviour
{
    // Главный метод создания модели ПК для конкретной эпохи
    public static GameObject Create_Era_Model(int eraIndex, Transform parent)
    {
        GameObject root = new GameObject("Computer_Era_" + eraIndex);
        root.transform.SetParent(parent);

        switch (eraIndex)
        {
            case 0: Create_Era_1_ENIAC(root); break;
            case 1: Create_Era_2_Mainframe(root); break;
            case 2: Create_Era_3_Microcomputer(root); break;
            case 3: Create_Era_4_Retro_PC(root); break;
            case 4: Create_Era_5_Modern_PC(root); break;
        }
        return root;
    }

    // Вспомогательный метод для привязки интерактивного компонента к объекту
    private static void Attach_Component(GameObject obj, ComponentType type, string nameRU, string nameEN)
    {
        InteractiveComponent comp = obj.AddComponent<InteractiveComponent>();
        comp.Component_Type = type;
        comp.Component_Name_RU = nameRU;
        comp.Component_Name_EN = nameEN;
        if (!obj.GetComponent<Collider>()) obj.AddComponent<BoxCollider>();
    }

    // Эпоха 1: Ламповые ЭВМ (1943–1955)
    private static void Create_Era_1_ENIAC(GameObject parent)
    {
        GameObject cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cabinet.transform.SetParent(parent.transform);
        cabinet.transform.localScale = new Vector3(3f, 2.5f, 0.8f);
        cabinet.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        // Создание электронно-вакуумных ламп
        for (int i = -2; i <= 2; i++)
        {
            GameObject tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tube.transform.SetParent(parent.transform);
            tube.transform.position = new Vector3(i * 0.5f, 0.5f, -0.45f);
            tube.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            tube.GetComponent<Renderer>().material.color = Color.cyan;
            Attach_Component(tube, ComponentType.VacuumTube, "Электронная лампа", "Vacuum Tube");
        }

        // Логический модуль
        GameObject switches = GameObject.CreatePrimitive(PrimitiveType.Cube);
        switches.transform.SetParent(parent.transform);
        switches.transform.position = new Vector3(0, -0.5f, -0.45f);
        switches.transform.localScale = new Vector3(2f, 0.6f, 0.1f);
        switches.GetComponent<Renderer>().material.color = Color.gray;
        Attach_Component(switches, ComponentType.CPU, "Ламповый вычислетель", "Tube Logic Circuit");
    }

    // Эпоха 2: Транзисторные ЭВМ (1955–1965)
    private static void Create_Era_2_Mainframe(GameObject parent)
    {
        GameObject mainFrame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mainFrame.transform.SetParent(parent.transform);
        mainFrame.transform.localScale = new Vector3(2f, 2f, 1f);
        mainFrame.GetComponent<Renderer>().material.color = Color.darkGray;

        // Накопитель на магнитной ленте
        GameObject tape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tape.transform.SetParent(parent.transform);
        tape.transform.position = new Vector3(0, 0.4f, -0.55f);
        tape.transform.rotation = Quaternion.Euler(90, 0, 0);
        tape.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
        tape.GetComponent<Renderer>().material.color = Color.black;
        Attach_Component(tape, ComponentType.Storage, "Ленточный накопитель", "Magnetic Tape Storage");

        // Транзисторная плата
        GameObject board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        board.transform.SetParent(parent.transform);
        board.transform.position = new Vector3(0, -0.4f, -0.55f);
        board.transform.localScale = new Vector3(1.5f, 0.5f, 0.1f);
        board.GetComponent<Renderer>().material.color = Color.green;
        Attach_Component(board, ComponentType.Transistor, "Транзисторный модуль", "Transistor Board");
    }

    // Эпоха 3: Интегральные схемы (1965–1980)
    private static void Create_Era_3_Microcomputer(GameObject parent)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.transform.SetParent(parent.transform);
        body.transform.localScale = new Vector3(1.5f, 0.4f, 1.2f);
        body.GetComponent<Renderer>().material.color = new Color(0.8f, 0.75f, 0.65f);

        // Дисковод для гибких дискет
        GameObject floppyDrive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floppyDrive.transform.SetParent(parent.transform);
        floppyDrive.transform.position = new Vector3(0.3f, 0f, -0.61f);
        floppyDrive.transform.localScale = new Vector3(0.5f, 0.15f, 0.05f);
        floppyDrive.GetComponent<Renderer>().material.color = Color.black;
        Attach_Component(floppyDrive, ComponentType.Storage, "Дисковод 5.25\"", "5.25 Floppy Drive");
    }

    // Эпоха 4: Эра Микропроцессоров (1980–2000)
    private static void Create_Era_4_Retro_PC(GameObject parent)
    {
        GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chassis.transform.SetParent(parent.transform);
        chassis.transform.position = new Vector3(-0.1f, 0f, 0f);
        chassis.transform.localScale = new Vector3(0.6f, 1.2f, 1.2f);
        chassis.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);

        GameObject frontPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontPanel.transform.SetParent(parent.transform);
        frontPanel.transform.position = new Vector3(-0.45f, 0f, 0f);
        frontPanel.transform.localScale = new Vector3(0.1f, 1.22f, 1.22f);
        frontPanel.GetComponent<Renderer>().material.color = new Color(0.85f, 0.82f, 0.75f);

        // Открывающаяся боковая крышка корпуса
        GameObject sideCover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sideCover.name = "SideCover";
        sideCover.transform.SetParent(parent.transform);
        sideCover.transform.position = new Vector3(0.25f, 0f, 0f);
        sideCover.transform.localScale = new Vector3(0.05f, 1.18f, 1.18f);
        sideCover.GetComponent<Renderer>().material.color = new Color(0.75f, 0.72f, 0.65f);
        Attach_Component(sideCover, ComponentType.CaseCover, "Боковая крышка", "Side Cover");

        // Центральный процессор
        GameObject cpu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cpu.transform.SetParent(parent.transform);
        cpu.transform.position = new Vector3(-0.1f, 0.2f, 0f);
        cpu.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);
        cpu.GetComponent<Renderer>().material.color = Color.black;
        Attach_Component(cpu, ComponentType.CPU, "Процессор Pentium", "Pentium CPU");

        // Видеокарта AGP
        GameObject gpu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gpu.transform.SetParent(parent.transform);
        gpu.transform.position = new Vector3(0.05f, -0.2f, 0f);
        gpu.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
        gpu.GetComponent<Renderer>().material.color = Color.green;
        Attach_Component(gpu, ComponentType.GPU, "Видеокарта AGP", "AGP Graphics Card");
    }

    // Эпоха 5: Современные станции (с 2000-х)
    private static void Create_Era_5_Modern_PC(GameObject parent)
    {
        GameObject chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chassis.transform.SetParent(parent.transform);
        chassis.transform.position = new Vector3(0f, 0f, 0.2f);
        chassis.transform.localScale = new Vector3(1.2f, 1.3f, 0.2f);
        chassis.GetComponent<Renderer>().material.color = new Color(0.12f, 0.12f, 0.12f);

        // Видеокарта
        GameObject gpu = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gpu.transform.SetParent(parent.transform);
        gpu.transform.position = new Vector3(0f, 0.05f, 0f);
        gpu.transform.localScale = new Vector3(0.8f, 0.2f, 0.6f);
        gpu.GetComponent<Renderer>().material.color = Color.magenta;
        Attach_Component(gpu, ComponentType.GPU, "Современная Видеокарта", "Modern GPU");

        // Накопитель NVMe SSD
        GameObject ssd = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ssd.transform.SetParent(parent.transform);
        ssd.transform.position = new Vector3(-0.2f, 0.35f, 0.05f);
        ssd.transform.localScale = new Vector3(0.25f, 0.04f, 0.4f);
        ssd.GetComponent<Renderer>().material.color = Color.cyan;
        Attach_Component(ssd, ComponentType.Storage, "M.2 NVMe SSD", "M.2 NVMe SSD");

        // Стеклянная крышка корпуса
        GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        glass.name = "SideCover";
        glass.transform.SetParent(parent.transform);
        glass.transform.position = new Vector3(0f, 0f, -0.3f);
        glass.transform.localScale = new Vector3(1.25f, 1.35f, 0.05f);
        Material glassMat = glass.GetComponent<Renderer>().material;
        glassMat.color = new Color(0f, 0.5f, 1f, 0.3f);
        Attach_Component(glass, ComponentType.CaseCover, "Стеклянная крышка", "Glass Cover");
    }
}
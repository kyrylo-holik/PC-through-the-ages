using UnityEngine;

// Класс для процедурной генерации 3D-моделей компьютеров различных эпох
public class Procedural_PC_Models : MonoBehaviour
{
    // Главный метод создания модели ПК для конкретной эпохи
    public static GameObject Create_Era_Model(int Era_Index, Transform Parent)
    {
        GameObject Root = new GameObject($"Computer_Era_{Era_Index}");
        Root.transform.SetParent(Parent, false);

        switch (Era_Index)
        {
            case 0:
                Create_Era_1_ENIAC(Root);
                break;

            case 1:
                Create_Era_2_Mainframe(Root);
                break;

            case 2:
                Create_Era_3_Microcomputer(Root);
                break;

            case 3:
                Create_Era_4_Retro_PC(Root);
                break;

            case 4:
                Create_Era_5_Modern_PC(Root);
                break;
        }

        return Root;
    }

    // Вспомогательный метод для привязки интерактивного компонента к объекту
    private static void Attach_Component(GameObject Obj, Component_Type Type)
    {
        Interactive_Component Comp = Obj.AddComponent<Interactive_Component>();
        Comp.Component_Type = Type;

        if (!Obj.TryGetComponent<Collider>(out _))
            Obj.AddComponent<BoxCollider>();
    }

    // Эпоха 1: Ламповые ЭВМ (1943–1955)
    private static void Create_Era_1_ENIAC(GameObject Parent)
    {
        GameObject Cabinet = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Cabinet.transform.SetParent(Parent.transform, false);
        Cabinet.transform.localScale = new Vector3(3f, 2.5f, 0.8f);
        Cabinet.GetComponent<Renderer>().material.color = new Color(0.2f, 0.2f, 0.2f);

        // Электронно-вакуумные лампы
        for (int I = -2; I <= 2; I++)
        {
            GameObject Tube = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Tube.transform.SetParent(Parent.transform, false);
            Tube.transform.position = new Vector3(I * 0.5f, 0.5f, -0.45f);
            Tube.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            Tube.GetComponent<Renderer>().material.color = Color.cyan;

            Attach_Component(Tube, Component_Type.Vacuum_Tube);
        }

        // Логический модуль
        GameObject Switches = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Switches.transform.SetParent(Parent.transform, false);
        Switches.transform.position = new Vector3(0f, -0.5f, -0.45f);
        Switches.transform.localScale = new Vector3(2f, 0.6f, 0.1f);
        Switches.GetComponent<Renderer>().material.color = Color.gray;

        Attach_Component(Switches, Component_Type.CPU);
    }

    // Эпоха 2: Транзисторные ЭВМ (1955–1965)
    private static void Create_Era_2_Mainframe(GameObject Parent)
    {
        GameObject Mainframe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Mainframe.transform.SetParent(Parent.transform, false);
        Mainframe.transform.localScale = new Vector3(2f, 2f, 1f);
        Mainframe.GetComponent<Renderer>().material.color = Color.gray;

        // Накопитель на магнитной ленте
        GameObject Tape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Tape.transform.SetParent(Parent.transform, false);
        Tape.transform.position = new Vector3(0f, 0.4f, -0.55f);
        Tape.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        Tape.transform.localScale = new Vector3(0.8f, 0.05f, 0.8f);
        Tape.GetComponent<Renderer>().material.color = Color.black;

        Attach_Component(Tape, Component_Type.Storage);

        // Транзисторная плата
        GameObject Board = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Board.transform.SetParent(Parent.transform, false);
        Board.transform.position = new Vector3(0f, -0.4f, -0.55f);
        Board.transform.localScale = new Vector3(1.5f, 0.5f, 0.1f);
        Board.GetComponent<Renderer>().material.color = Color.green;

        Attach_Component(Board, Component_Type.Transistor);
    }

    // Эпоха 3: Интегральные схемы (1965–1980)
    private static void Create_Era_3_Microcomputer(GameObject Parent)
    {
        GameObject Body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Body.transform.SetParent(Parent.transform, false);
        Body.transform.localScale = new Vector3(1.5f, 0.4f, 1.2f);
        Body.GetComponent<Renderer>().material.color = new Color(0.8f, 0.75f, 0.65f);

        // Дисковод для гибких дискет
        GameObject Floppy_Drive = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Floppy_Drive.transform.SetParent(Parent.transform, false);
        Floppy_Drive.transform.position = new Vector3(0.3f, 0f, -0.61f);
        Floppy_Drive.transform.localScale = new Vector3(0.5f, 0.15f, 0.05f);
        Floppy_Drive.GetComponent<Renderer>().material.color = Color.black;

        Attach_Component(Floppy_Drive, Component_Type.Storage);
    }

    // Эпоха 4: Эра Микропроцессоров (1980–2000)
    private static void Create_Era_4_Retro_PC(GameObject Parent)
    {
        GameObject Chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Chassis.transform.SetParent(Parent.transform, false);
        Chassis.transform.position = new Vector3(-0.1f, 0f, 0f);
        Chassis.transform.localScale = new Vector3(0.6f, 1.2f, 1.2f);
        Chassis.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);

        GameObject Front_Panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Front_Panel.transform.SetParent(Parent.transform, false);
        Front_Panel.transform.position = new Vector3(-0.45f, 0f, 0f);
        Front_Panel.transform.localScale = new Vector3(0.1f, 1.22f, 1.22f);
        Front_Panel.GetComponent<Renderer>().material.color = new Color(0.85f, 0.82f, 0.75f);

        // Открывающаяся боковая крышка корпуса
        GameObject Side_Cover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Side_Cover.name = "SideCover";
        Side_Cover.transform.SetParent(Parent.transform, false);
        Side_Cover.transform.position = new Vector3(0.25f, 0f, 0f);
        Side_Cover.transform.localScale = new Vector3(0.05f, 1.18f, 1.18f);
        Side_Cover.GetComponent<Renderer>().material.color = new Color(0.75f, 0.72f, 0.65f);

        Attach_Component(Side_Cover, Component_Type.Case_Cover);

        // Центральный процессор
        GameObject CPU_Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        CPU_Obj.transform.SetParent(Parent.transform, false);
        CPU_Obj.transform.position = new Vector3(-0.1f, 0.2f, 0f);
        CPU_Obj.transform.localScale = new Vector3(0.2f, 0.2f, 0.05f);
        CPU_Obj.GetComponent<Renderer>().material.color = Color.black;

        Attach_Component(CPU_Obj, Component_Type.CPU);

        // Видеокарта AGP
        GameObject GPU_Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GPU_Obj.transform.SetParent(Parent.transform, false);
        GPU_Obj.transform.position = new Vector3(0.05f, -0.2f, 0f);
        GPU_Obj.transform.localScale = new Vector3(0.4f, 0.1f, 0.4f);
        GPU_Obj.GetComponent<Renderer>().material.color = Color.green;

        Attach_Component(GPU_Obj, Component_Type.GPU);
    }

    // Эпоха 5: Современные станции (с 2000-х)
    private static void Create_Era_5_Modern_PC(GameObject Parent)
    {
        GameObject Chassis = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Chassis.transform.SetParent(Parent.transform, false);
        Chassis.transform.position = new Vector3(0f, 0f, 0.2f);
        Chassis.transform.localScale = new Vector3(1.2f, 1.3f, 0.2f);
        Chassis.GetComponent<Renderer>().material.color = new Color(0.12f, 0.12f, 0.12f);

        // Видеокарта
        GameObject GPU_Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GPU_Obj.transform.SetParent(Parent.transform, false);
        GPU_Obj.transform.position = new Vector3(0f, 0.05f, 0f);
        GPU_Obj.transform.localScale = new Vector3(0.8f, 0.2f, 0.6f);
        GPU_Obj.GetComponent<Renderer>().material.color = Color.magenta;

        Attach_Component(GPU_Obj, Component_Type.GPU);

        // Накопитель NVMe SSD
        GameObject SSD_Obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        SSD_Obj.transform.SetParent(Parent.transform, false);
        SSD_Obj.transform.position = new Vector3(-0.2f, 0.35f, 0.05f);
        SSD_Obj.transform.localScale = new Vector3(0.25f, 0.04f, 0.4f);
        SSD_Obj.GetComponent<Renderer>().material.color = Color.cyan;

        Attach_Component(SSD_Obj, Component_Type.Storage);

        // Стеклянная крышка корпуса
        GameObject Glass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Glass.name = "SideCover";
        Glass.transform.SetParent(Parent.transform, false);
        Glass.transform.position = new Vector3(0f, 0f, -0.3f);
        Glass.transform.localScale = new Vector3(1.25f, 1.35f, 0.05f);

        Material Glass_Material = Glass.GetComponent<Renderer>().material;
        Glass_Material.color = new Color(0f, 0.5f, 1f, 0.3f);

        Attach_Component(Glass, Component_Type.Case_Cover);
    }
}
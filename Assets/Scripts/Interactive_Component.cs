using UnityEngine;

// Компонент, закрепляемый за интерактивными элементами 3D-моделей ПК
public class Interactive_Component : MonoBehaviour
{
    public Component_Type Component_Type;

    private Renderer Mesh_Renderer;
    private Color Original_Color;
    private bool Is_Cached;

    // Исходный цвет запоминается лениво (при первом обращении), а не в Awake:
    // так не важно, в каком порядке процедурная модель красит объект и вешает компонент
    private void Cache_Original()
    {
        if (Is_Cached)
            return;

        Mesh_Renderer = GetComponent<Renderer>();

        if (Mesh_Renderer != null)
            Original_Color = Mesh_Renderer.material.color;

        Is_Cached = true;
    }

    // Управление подсветкой компонента (включение/выключение)
    public void Highlight(bool State)
    {
        Cache_Original();

        if (Mesh_Renderer == null)
            return;

        // Для прозрачных деталей (стекло) сохраняем полупрозрачность
        Color Highlight_Color = Color.yellow;
        Highlight_Color.a = Mathf.Max(Original_Color.a, 0.5f);

        Mesh_Renderer.material.color = State ? Highlight_Color : Original_Color;
    }

    private void OnMouseDown()
    {
        if (Game_Controller.Instance == null)
            return;

        // Если игра на этапе теории или идёт пауза между вопросами - клики заблокированы
        if (!Game_Controller.Instance.Is_Interaction_Enabled)
            return;

        // Передаем информацию о клике в главный контроллер игры
        Game_Controller.Instance.On_Component_Clicked(this);
    }
}

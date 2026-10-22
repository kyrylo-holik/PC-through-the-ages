using UnityEngine;

// Компонент, закрепляемый за интерактивными элементами 3D-моделей ПК
public class Interactive_Component : MonoBehaviour
{
    public Component_Type Component_Type;

    private Renderer Mesh_Renderer;
    private Color Original_Color;

    private void Awake()
    {
        Mesh_Renderer = GetComponent<Renderer>();

        if (Mesh_Renderer != null)
        {
            Original_Color = Mesh_Renderer.material.color;
        }
    }

    // Управление подсветкой компонента (включение/выключение)
    public void Highlight(bool State)
    {
        if (Mesh_Renderer == null)
            return;

        Mesh_Renderer.material.color = State ? Color.yellow : Original_Color;
    }

    private void OnMouseDown()
    {
        if (Game_Controller.Instance == null)
            return;

        // Если игра на этапе теории и клики заблокированы — ничего не делаем
        if (!Game_Controller.Instance.Is_Interaction_Enabled)
            return;

        // Передаем информацию о клике в главный контроллер игры
        Game_Controller.Instance.On_Component_Clicked(this);
    }
}
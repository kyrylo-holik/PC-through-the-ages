using UnityEngine;

// Компонент, закрепляемый за интерактивными элементами 3D-моделей ПК
public class InteractiveComponent : MonoBehaviour
{
    public ComponentType Component_Type;
    public string Component_Name_RU;
    public string Component_Name_EN;

    private Renderer Mesh_Renderer;
    private Color Original_Color;
    private bool Is_Highlighted = false;

    private void Awake()
    {
        Mesh_Renderer = GetComponent<Renderer>();
        if (Mesh_Renderer != null)
        {
            Original_Color = Mesh_Renderer.material.color;
        }
    }

    // Управление подсветкой компонента (включение/выключение)
    public void Highlight(bool state)
    {
        Is_Highlighted = state;
        if (Mesh_Renderer != null)
        {
            Mesh_Renderer.material.color = state ? Color.yellow : Original_Color;
        }
    }

    private void OnMouseDown()
    {
        // Если игра на этапе теории и клики заблокированы — ничего не делаем
        if (GameController.Instance != null && !GameController.Instance.Is_Interaction_Enabled)
        {
            return;
        }

        // Передаем информацию о клике в главный контроллер игры
        if (GameController.Instance != null)
        {
            GameController.Instance.On_Component_Clicked(this);
        }
    }
}
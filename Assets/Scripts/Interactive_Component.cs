using UnityEngine;

public class InteractiveComponent : MonoBehaviour
{
    public ComponentType componentType;
    public string componentNameRU;
    public string componentNameEN;

    private Renderer meshRenderer;
    private Color originalColor;
    private bool isHighlighted = false;

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    public void Highlight(bool state)
    {
        isHighlighted = state;
        if (meshRenderer != null)
        {
            meshRenderer.material.color = state ? Color.yellow : originalColor;
        }
    }

    private void OnMouseDown()
    {
        // Если игра еще на этапе теории и клики заблокированы — ничего не делаем
        if (GameController.Instance != null && !GameController.Instance.IsInteractionEnabled)
        {
            return;
        }

        // Передаем клик в главный контроллер, если фаза вопросов активна
        if (GameController.Instance != null)
        {
            GameController.Instance.OnComponentClicked(this);
        }
    }
}
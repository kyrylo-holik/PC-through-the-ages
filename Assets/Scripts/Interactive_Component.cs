using UnityEngine;

// Этот компонент вешается на каждую «кликабельную» деталь 3D-модели
// (лампу, процессор, видеокарту и т.д.).
// Он делает две вещи:
//   1) сообщает главному контроллеру игры, что по детали кликнули;
//   2) умеет подсвечивать деталь жёлтым цветом (подсказка после 5 ошибок).
public class Interactive_Component : MonoBehaviour
{
    // Что это за деталь. Контроллер сравнивает этот тип с ответом на текущий вопрос.
    public Component_Type Component_Type;

    // Собственная копия материала детали. Создаётся только тогда, когда деталь
    // действительно нужно подсветить (так мы не тратим память на сотни лишних копий).
    private Material Own_Material;

    // Цвет детали до подсветки, чтобы вернуть его обратно.
    private Color Original_Color;

    // Включена ли подсветка прямо сейчас.
    private bool Is_Highlighted;

    // Включает (State = true) или выключает (State = false) жёлтую подсветку детали.
    public void Highlight(bool State)
    {
        // Если деталь уже в нужном состоянии — ничего делать не нужно.
        if (State == Is_Highlighted)
            return;

        // При первой подсветке делаем личную копию материала и запоминаем исходный цвет.
        if (Own_Material == null)
        {
            Renderer Mesh_Renderer = GetComponent<Renderer>();

            if (Mesh_Renderer == null)
                return;

            Own_Material = Mesh_Renderer.material;
            Original_Color = Own_Material.color;
        }

        Is_Highlighted = State;

        // Жёлтый цвет, но с прозрачностью исходной детали (чтобы стекло оставалось полупрозрачным).
        Color Highlight_Color = Color.yellow;
        Highlight_Color.a = Mathf.Max(Original_Color.a, 0.5f);

        Own_Material.color = State ? Highlight_Color : Original_Color;
    }

    // Unity вызывает этот метод сам, когда игрок нажимает левой кнопкой мыши на деталь.
    private void OnMouseDown()
    {
        // Если контроллера нет, или сейчас клики запрещены (идёт теория или пауза между вопросами) — выходим.
        if (Game_Controller.Instance == null || !Game_Controller.Instance.Is_Interaction_Enabled)
            return;

        // Сообщаем контроллеру, по какой детали кликнули.
        Game_Controller.Instance.On_Component_Clicked(this);
    }

    // Когда деталь удаляется вместе с моделью, удаляем и личную копию материала, чтобы не копился мусор.
    private void OnDestroy()
    {
        if (Own_Material != null)
            Destroy(Own_Material);
    }
}

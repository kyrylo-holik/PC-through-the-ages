using System.Collections;
using UnityEngine;

// Боковая крышка корпуса (в 4-й эпохе — металлическая, в 5-й — стеклянная).
// Открываясь, она плавно сдвигается назад. Крышка остаётся в сцене и по ней
// можно кликнуть ещё раз, чтобы закрыть корпус.
public class Openable_Cover : MonoBehaviour
{
    // Сколько секунд занимает открытие или закрытие.
    public float Move_Duration = 0.5f;

    private Vector3 Closed_Position;   // позиция закрытой крышки
    private Vector3 Open_Position;     // позиция открытой крышки
    private Coroutine Move_Routine;    // текущая анимация

    // true — крышка открыта (или открывается).
    public bool Is_Open { get; private set; }

    // Запоминает позицию закрытой крышки и на сколько её нужно сдвинуть при открытии.
    // Вызывается один раз, когда крышка уже стоит на своём месте.
    public void Setup(Vector3 Open_Offset)
    {
        Closed_Position = transform.localPosition;
        Open_Position = Closed_Position + Open_Offset;
        Is_Open = false;
    }

    // Открыть закрытую крышку или закрыть открытую.
    public void Toggle()
    {
        Set_Open(!Is_Open);
    }

    // Принудительно открыть (true) или закрыть (false) крышку.
    public void Set_Open(bool State)
    {
        // Крышка уже в нужном состоянии.
        if (Is_Open == State)
            return;

        Is_Open = State;

        // Останавливаем предыдущую анимацию, если она ещё идёт.
        if (Move_Routine != null)
            StopCoroutine(Move_Routine);

        Move_Routine = StartCoroutine(Move_To(State ? Open_Position : Closed_Position));
    }

    // Плавно двигает крышку из текущей позиции в целевую.
    private IEnumerator Move_To(Vector3 Target_Position)
    {
        Vector3 From_Position = transform.localPosition;
        float Time_Passed = 0f;

        while (Time_Passed < Move_Duration)
        {
            Time_Passed += Time.deltaTime;

            // SmoothStep делает движение мягким: медленный старт и плавная остановка.
            float Progress = Mathf.SmoothStep(0f, 1f, Time_Passed / Move_Duration);
            transform.localPosition = Vector3.Lerp(From_Position, Target_Position, Progress);

            yield return null;   // ждём следующий кадр
        }

        transform.localPosition = Target_Position;
        Move_Routine = null;
    }
}

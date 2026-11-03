using System.Collections;
using UnityEngine;

// Съёмная деталь корпуса (оперативная память, видеокарта, диск и т.д.).
// По клику плавно выезжает из корпуса, а по повторному клику возвращается на место.
public class Detachable_Part : MonoBehaviour
{
    // Сколько секунд занимает выдвижение или возврат детали.
    public float Move_Duration = 0.35f;

    private Vector3 Attached_Local_Position;   // позиция детали, когда она стоит на месте
    private Vector3 Detached_Local_Position;   // позиция детали, когда она выдвинута
    private Coroutine Move_Routine;            // текущая анимация (чтобы остановить её, если она ещё идёт)

    // true — деталь сейчас выдвинута (или едет наружу).
    public bool Is_Detached { get; private set; }

    // Запоминает «родную» позицию детали и то, на сколько её нужно выдвигать.
    // Вызывается один раз, сразу после того как деталь поставлена на своё место.
    public void Setup(Vector3 Detach_Offset)
    {
        Attached_Local_Position = transform.localPosition;
        Detached_Local_Position = Attached_Local_Position + Detach_Offset;
        Is_Detached = false;
    }

    // Выдвинуть деталь, если она на месте, и вернуть, если она выдвинута.
    public void Toggle()
    {
        Set_Detached(!Is_Detached);
    }

    // Принудительно выдвинуть (true) или вернуть (false) деталь.
    public void Set_Detached(bool State)
    {
        // Деталь уже в нужном состоянии.
        if (Is_Detached == State)
            return;

        Is_Detached = State;

        // Если предыдущая анимация ещё идёт — останавливаем её, чтобы они не мешали друг другу.
        if (Move_Routine != null)
            StopCoroutine(Move_Routine);

        Move_Routine = StartCoroutine(Move_To(State ? Detached_Local_Position : Attached_Local_Position));
    }

    // Плавно двигает деталь из текущей позиции в целевую.
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

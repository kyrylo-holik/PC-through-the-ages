using System.Collections;
using UnityEngine;

// Съёмная деталь корпуса (ОЗУ, видеокарта, диск и т.д.).
// По клику плавно выдвигается из корпуса и возвращается на место.
public class Detachable_Part : MonoBehaviour
{
    public float Move_Duration = 0.35f;

    private Vector3 Attached_Local_Position;
    private Vector3 Detached_Local_Position;
    private Coroutine Move_Routine;

    public bool Is_Detached { get; private set; }

    // Вызывается сразу после того, как деталь расставлена на своё место
    public void Setup(Vector3 Detach_Offset)
    {
        Attached_Local_Position = transform.localPosition;
        Detached_Local_Position = Attached_Local_Position + Detach_Offset;
        Is_Detached = false;
    }

    public void Toggle()
    {
        Set_Detached(!Is_Detached);
    }

    public void Set_Detached(bool State)
    {
        if (Is_Detached == State)
            return;

        Is_Detached = State;

        if (Move_Routine != null)
            StopCoroutine(Move_Routine);

        Move_Routine = StartCoroutine(Move_To(State ? Detached_Local_Position : Attached_Local_Position));
    }

    private IEnumerator Move_To(Vector3 Target_Position)
    {
        Vector3 From_Position = transform.localPosition;
        float Time_Passed = 0f;

        while (Time_Passed < Move_Duration)
        {
            Time_Passed += Time.deltaTime;
            float T = Mathf.SmoothStep(0f, 1f, Time_Passed / Move_Duration);
            transform.localPosition = Vector3.Lerp(From_Position, Target_Position, T);
            yield return null;
        }

        transform.localPosition = Target_Position;
        Move_Routine = null;
    }
}

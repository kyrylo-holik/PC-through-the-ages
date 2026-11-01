using System.Collections;
using UnityEngine;

// Боковая крышка корпуса. Сдвигается (и при необходимости поворачивается) в открытое положение.
// В отличие от старого SetActive(false), крышка остаётся в сцене и по ней можно кликнуть снова, чтобы закрыть корпус.
public class Openable_Cover : MonoBehaviour
{
    public Vector3 Open_Euler = Vector3.zero;
    public float Move_Duration = 0.5f;

    private Vector3 Closed_Position;
    private Vector3 Open_Position;
    private Quaternion Closed_Rotation;
    private Quaternion Open_Rotation;
    private Coroutine Move_Routine;

    public bool Is_Open { get; private set; }

    // Вызывается после того, как крышка поставлена на своё место в закрытом положении
    public void Setup(Vector3 Open_Offset)
    {
        Closed_Position = transform.localPosition;
        Open_Position = Closed_Position + Open_Offset;
        Closed_Rotation = transform.localRotation;
        Open_Rotation = Closed_Rotation * Quaternion.Euler(Open_Euler);
        Is_Open = false;
    }

    public void Toggle()
    {
        Set_Open(!Is_Open);
    }

    public void Set_Open(bool State)
    {
        if (Is_Open == State)
            return;

        Is_Open = State;

        if (Move_Routine != null)
            StopCoroutine(Move_Routine);

        Move_Routine = StartCoroutine(Move_To(
            State ? Open_Position : Closed_Position,
            State ? Open_Rotation : Closed_Rotation));
    }

    private IEnumerator Move_To(Vector3 Target_Position, Quaternion Target_Rotation)
    {
        Vector3 From_Position = transform.localPosition;
        Quaternion From_Rotation = transform.localRotation;
        float Time_Passed = 0f;

        while (Time_Passed < Move_Duration)
        {
            Time_Passed += Time.deltaTime;
            float T = Mathf.SmoothStep(0f, 1f, Time_Passed / Move_Duration);
            transform.localPosition = Vector3.Lerp(From_Position, Target_Position, T);
            transform.localRotation = Quaternion.Slerp(From_Rotation, Target_Rotation, T);
            yield return null;
        }

        transform.localPosition = Target_Position;
        transform.localRotation = Target_Rotation;
        Move_Routine = null;
    }
}

using UnityEngine;
using UnityEngine.Events;

public class GeneralEvent : MonoBehaviour
{
    public UnityEvent generalEvent;

    public void GeneralEventInvoker()
    {
        generalEvent?.Invoke();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EventClickProxy : MonoBehaviour, IPointerClickHandler {
    public UnityEvent callback;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        callback.Invoke();
    }
}

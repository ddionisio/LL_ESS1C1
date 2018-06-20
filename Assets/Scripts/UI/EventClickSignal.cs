using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventClickSignal : MonoBehaviour, IPointerClickHandler {

    public M8.Signal signal;

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
        if(signal)
            signal.Invoke();
    }
}
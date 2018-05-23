using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventDropProxy : MonoBehaviour, IDropHandler {
    public System.Action<PointerEventData> callback;

    void IDropHandler.OnDrop(PointerEventData eventData) {
        if(callback != null)
            callback(eventData);
    }
}

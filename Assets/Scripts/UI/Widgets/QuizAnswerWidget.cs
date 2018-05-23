using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class QuizAnswerWidget : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {


    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
        Debug.Log("Begin Drag: " + name);
    }

    void IDragHandler.OnDrag(PointerEventData eventData) {
        
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData) {
        Debug.Log("End Drag: " + name);
    }
}

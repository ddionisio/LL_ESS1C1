using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModalQuiz : M8.UIModal.Controller {
    public UIEventDropProxy[] dropSlots;

    //public QuizAnswerWidget 

    [Header("Signals")]
    public M8.Signal signalNext;

    public void Next() {
        if(signalNext)
            signalNext.Invoke();
    }

    void OnDestroy() {
        for(int i = 0; i < dropSlots.Length; i++) {
            if(dropSlots[i])
                dropSlots[i].callback -= OnDropSlot;
        }
    }

    void Awake() {
        //initialize drop slots
        for(int i = 0; i < dropSlots.Length; i++) {
            dropSlots[i].callback += OnDropSlot;
        }
    }

    void OnDropSlot(PointerEventData eventData) {
        Debug.Log("event: " + eventData.pointerDrag.name);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalLevelResults : M8.UIModal.Controller {

    [Header("Signals")]
    public M8.Signal signalNext;

    public void Next() {
        if(signalNext)
            signalNext.Invoke();
    }
}

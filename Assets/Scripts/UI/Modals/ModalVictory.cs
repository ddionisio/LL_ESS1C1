using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.UIModal.Controller {
    //TODO: info stuff

    public void Proceed() {
        Close();

        GameData.instance.Progress();
    }
}

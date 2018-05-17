using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper to open notebook
/// </summary>
public class ModalNotebookProxy : MonoBehaviour {
    public int startPageIndex = 0;

    public void Open() {
        ModalNotebook.Open(startPageIndex);
    }
}

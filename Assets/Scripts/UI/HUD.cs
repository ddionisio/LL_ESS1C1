using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.PrefabFromResource("UI")]
public class HUD : M8.SingletonBehaviour<HUD> {
    public enum Mode {
        None,
        Lesson,
        Game,
    }

    public RectTransform root;

    public GameObject gameRootGO; //gameplay related hud elements

    public GameObject lessonRootGO;
    public Transform notebookIconRoot;

    [Header("Signals")]
    public SignalBool signalGameActiveUpdate;

    public Mode mode {
        get { return mMode; }
        set {
            if(mMode != value) {
                mMode = value;

                ApplyCurrentMode();
            }
        }
    }

    private Mode mMode = Mode.None;

    protected override void OnInstanceInit() {
        ApplyCurrentMode();
    }

    private void ApplyCurrentMode() {
        bool gameActive = false;
        bool lessonActive = false;

        switch(mMode) {
            case Mode.Game:
                gameActive = true;
                break;
            case Mode.Lesson:
                lessonActive = true;
                break;
        }

        if(gameRootGO) gameRootGO.SetActive(gameActive);
        if(lessonRootGO) lessonRootGO.SetActive(lessonActive);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[M8.PrefabFromResource("UI")]
public class HUD : M8.SingletonBehaviour<HUD> {
    public RectTransform root;

    public GameObject gameRootGO; //gameplay related hud elements
    public GameObject launchDialogGO;

    [Header("Signals")]
    public SignalBool signalGameActiveUpdate;

    /// <summary>
    /// Show/Hide gameplay related hud elements
    /// </summary>
    public bool isGameActive {
        get { return gameRootGO && gameRootGO.activeSelf; }
        set {
            if(gameRootGO) {
                if(gameRootGO.activeSelf != value) {
                    gameRootGO.SetActive(value);

                    if(signalGameActiveUpdate)
                        signalGameActiveUpdate.Invoke(value);
                }
            }
        }
    }

    protected override void OnInstanceInit() {
        if(gameRootGO) gameRootGO.SetActive(false);
    }
}

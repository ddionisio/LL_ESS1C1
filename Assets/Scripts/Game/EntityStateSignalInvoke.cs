using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EntityStateSignalInvoke {
    public EntityState state;
    public M8.Signal[] signals;

    public void Invoke() {
        for(int i = 0; i < signals.Length; i++) {
            var signal = signals[i];
            if(signal)
                signal.Invoke();
        }
    }
}

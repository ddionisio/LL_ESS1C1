using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        var player = target as Player;

        //Information Display
        M8.EditorExt.Utility.DrawSeparator();

        //runtime information
        if(Application.isPlaying) {
            GUILayout.Label(string.Format("Is Grounded: {0}", player.isGrounded));
            GUILayout.Label(string.Format("Speed: {0}", player.physicsBody.velocity.magnitude));
        }
        //

        //Debug Stuff
    }
}

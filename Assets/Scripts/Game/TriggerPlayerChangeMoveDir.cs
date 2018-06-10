using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerPlayerChangeMoveDir : MonoBehaviour {
    public const float gizmoAlpha = 0.2f;

    public enum DirType {
        Left,
        Right
    }

    public DirType dir;

    private void OnTriggerEnter2D(Collider2D collision) {
        //assume it is the player
        switch(dir) {
            case DirType.Left:
                GameMapController.instance.player.ChangeGroundMoveDir(-1f);
                break;
            case DirType.Right:
                GameMapController.instance.player.ChangeGroundMoveDir(1f);
                break;
        }
    }

    void OnDrawGizmos() {
        var boxColl = GetComponent<BoxCollider2D>();
        if(!boxColl)
            return;

        switch(dir) {
            case DirType.Left:
                Gizmos.color = new Color(0.3f, 1f, 0.3f, gizmoAlpha);
                break;
            case DirType.Right:
                Gizmos.color = new Color(1f, 0.3f, 0.3f, gizmoAlpha);
                break;
        }

        Gizmos.DrawCube(transform.localToWorldMatrix.MultiplyPoint3x4(boxColl.offset), boxColl.size);
    }
}

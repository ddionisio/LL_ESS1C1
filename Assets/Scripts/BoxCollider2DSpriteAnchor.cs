using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BoxCollider2DSpriteAnchor : MonoBehaviour {
    public SpriteRenderer spriteRenderer;
    public BoxCollider2D boxCollider;

    public Vector2 offsetMin;
    public Vector2 offsetMax;

    public void Apply() {
        if(!spriteRenderer || !boxCollider)
            return;

        Transform t = spriteRenderer.transform;

        Vector2 size;
                        
        switch(spriteRenderer.drawMode) {
            case SpriteDrawMode.Sliced:
            case SpriteDrawMode.Tiled:
                size = spriteRenderer.size;
                size *= t.localScale;
                break;
            default:
                size = t.localScale;
                break;
        }

        size /= boxCollider.transform.localScale;

        Vector2 pivot;

        if(spriteRenderer.sprite)
            pivot = spriteRenderer.sprite.pivot / spriteRenderer.sprite.pixelsPerUnit;
        else
            pivot = new Vector2(0.5f, 0.5f);



        Rect r = new Rect();
        r.center = boxCollider.transform.worldToLocalMatrix.MultiplyPoint3x4(t.position);
        r.size = size;

        r.center -= size * pivot;

        r.min += offsetMin;
        r.max += offsetMax;

        boxCollider.offset = r.center;
        boxCollider.size = r.size;
    }

    void Awake() {
        Apply();
    }

#if UNITY_EDITOR
    void Update() {
        Apply();
    }
#endif
}

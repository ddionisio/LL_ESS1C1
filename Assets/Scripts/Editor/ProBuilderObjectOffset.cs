using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using ProBuilder.Core;

public class ProBuilderObjectOffset {

    [MenuItem("Game/ProBuilder/Object/Apply Unity Position")]
    static void ObjectApplyUnityPosition() {
        var gos = Selection.gameObjects;

        for(int i = 0; i < gos.Length; i++) {
            pb_Object obj = gos[i].GetComponent<pb_Object>();
            if(obj) {
                var pos = gos[i].transform.localPosition;

                Undo.RecordObject(gos[i].transform, "Reset Local Position");
                gos[i].transform.localPosition = Vector3.zero;

                Undo.RecordObject(obj, "Apply Vertices");
                                
                var vtx = obj.vertices;
                for(int vI = 0; vI < vtx.Length; vI++) {
                    vtx[vI] += pos;
                }

                obj.SetVertices(vtx);
                obj.Refresh();
            }
        }
    }
}

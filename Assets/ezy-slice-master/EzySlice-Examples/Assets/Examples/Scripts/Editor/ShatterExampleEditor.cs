using UnityEngine;
using System.Collections;
using UnityEditor;
using EzySlice;

/**
 * This is a simple Editor helper script for rapid testing/prototyping! 
 */
[CustomEditor(typeof(ShatterExample))]
public class ShatterExampleEditor : Editor {
    public GameObject source;
    public Material crossMat;
    public int slice;

    public override void OnInspectorGUI() {
        ShatterExample plane = (ShatterExample)target;

        source = (GameObject)EditorGUILayout.ObjectField(source, typeof(GameObject), true);

        if (source == null) {
            EditorGUILayout.LabelField("Add a GameObject to Shatter.");

            return;
        }

        if (!source.activeInHierarchy) {
            EditorGUILayout.LabelField("Object is Hidden. Cannot Slice.");

            return;
        }

        if (source.GetComponent<MeshFilter>() == null) {
            EditorGUILayout.LabelField("GameObject must have a MeshFilter.");

            return;
        }

        crossMat = (Material)EditorGUILayout.ObjectField(crossMat, typeof(Material), true);
        slice = EditorGUILayout.IntSlider(slice, 1, 20);

        if (GUILayout.Button("Shatter Object")) {
            if (plane.ShatterObject(source, slice, crossMat)) {
                source.SetActive(false);
            }
        }
    }
}


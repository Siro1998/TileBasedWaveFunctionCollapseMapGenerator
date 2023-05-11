using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilesGenerator3D))]

public class TilesGeneratorEditor3D : Editor
{
    TilesGenerator3D tilesGenerator;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Analyze")){
            tilesGenerator.Analyze();
        }
        if(GUILayout.Button("Generate")){
            tilesGenerator.WFC();
        }
        if(GUILayout.Button("Save")){
            tilesGenerator.SaveMap();
        }
    }

    private void OnEnable(){
        tilesGenerator = (TilesGenerator3D)target;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilesGenerator3DS))]

public class TilesGeneratorEditor3DS : Editor
{
    TilesGenerator3DS tilesGenerator;
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
        tilesGenerator = (TilesGenerator3DS)target;
    }
}

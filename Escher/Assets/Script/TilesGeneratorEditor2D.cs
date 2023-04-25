using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TilesGenerator2D))]

public class TilesGeneratorEditor2D : Editor
{
    TilesGenerator2D tilesGenerator;
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Analyze")){
            tilesGenerator.Analyze();
        }
        if(GUILayout.Button("Generate")){
            tilesGenerator.WFC();
        }
        // if(GUILayout.Button("Check")){
        //     tilesGenerator.Generate();
        // }
        if(GUILayout.Button("Save")){
            tilesGenerator.SaveMap();
        }
    }

    private void OnEnable(){
        tilesGenerator = (TilesGenerator2D)target;
    }
}

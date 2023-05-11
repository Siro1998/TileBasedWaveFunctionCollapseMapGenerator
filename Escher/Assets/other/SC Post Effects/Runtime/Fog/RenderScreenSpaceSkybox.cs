using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SCPE
{
    public class RenderScreenSpaceSkybox : MonoBehaviour { }

#if UNITY_EDITOR
    [CustomEditor(typeof(RenderScreenSpaceSkybox))]
    public class RenderScreenSpaceSkyboxInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This script is now obsolete and can be removed.", MessageType.Warning);
        }
    }
#endif
}

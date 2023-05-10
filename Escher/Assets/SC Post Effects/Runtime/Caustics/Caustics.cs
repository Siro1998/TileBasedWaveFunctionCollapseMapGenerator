using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using TextureParameter = UnityEngine.Rendering.PostProcessing.TextureParameter;
using BoolParameter = UnityEngine.Rendering.PostProcessing.BoolParameter;
using FloatParameter = UnityEngine.Rendering.PostProcessing.FloatParameter;
using IntParameter = UnityEngine.Rendering.PostProcessing.IntParameter;
using ColorParameter = UnityEngine.Rendering.PostProcessing.ColorParameter;
using Vector2Parameter = UnityEngine.Rendering.PostProcessing.Vector2Parameter;
using MinAttribute = UnityEngine.Rendering.PostProcessing.MinAttribute;

namespace SCPE
{
    [PostProcess(typeof(CausticsRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Environment/Caustics")]
    [Serializable]
    public sealed class Caustics : PostProcessEffectSettings
    {
        [Range(0f, 1f)]
        public FloatParameter intensity = new FloatParameter { value = 0f };
        
        public TextureParameter causticsTexture = new TextureParameter { value = null };
        
        public FloatParameter brightness = new FloatParameter { value = 1f };

        [Tooltip("Draws the caustics on pixels brighter than this threshold, useful to hide the caustics in shadows")]
        [Range(0f, 2f)]
        public FloatParameter luminanceThreshold = new FloatParameter { value = 0f };
        public BoolParameter projectFromSun = new BoolParameter { value = false};
        
        [Space]
        
        public FloatParameter minHeight = new FloatParameter { value = -5f };
        [Range(0f, 1f)]
        public FloatParameter minHeightFalloff = new FloatParameter { value = 1f };

        public FloatParameter maxHeight = new FloatParameter { value = 0f };
        [Range(0f, 1f)]
        public FloatParameter maxHeightFalloff = new FloatParameter { value = 1f };
        
        [Space]

        [Range(0.1f, 3f)]
        public FloatParameter size = new FloatParameter { value = 0.5f };
        [Range(0f, 1f)]
        public FloatParameter speed = new FloatParameter { value = 0.2f };
        
        [Space]

        public BoolParameter distanceFade = new BoolParameter { value = false };
        public FloatParameter startFadeDistance = new FloatParameter { value = 0f };
        public FloatParameter endFadeDistance = new FloatParameter { value = 200f };
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return (enabled.value && intensity > 0 && causticsTexture.value != null);
        }
        
        [SerializeField]
        public Shader shader;

        private void Reset()
        {
            SerializeShader();
            
            #if UNITY_EDITOR
            //Auto assign default texture
            if (causticsTexture.value == null)
            {
                causticsTexture.overrideState = true;
                causticsTexture.value = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath("c04a5fb5ba4f3ee4489d7a234aba5d3c"));
            }
            #endif
        }
        
        private bool SerializeShader()
        {
            bool wasSerialized = !shader;
            shader = Shader.Find(ShaderNames.Caustics);

            return wasSerialized;
        }
    }
}
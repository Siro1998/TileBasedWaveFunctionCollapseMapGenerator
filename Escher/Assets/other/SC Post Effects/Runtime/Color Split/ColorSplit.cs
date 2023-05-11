using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using FloatParameter = UnityEngine.Rendering.PostProcessing.FloatParameter;

namespace SCPE
{
    [PostProcess(typeof(ColorSplitRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Retro/Color Split", true)]
    [Serializable]
    public sealed class ColorSplit : PostProcessEffectSettings
    {
        public enum SplitMode
        {
            [InspectorName("Horizontal")]
            Single,
            [InspectorName("Horizontal + Vertical")]
            Double
        }

        [Serializable]
        public sealed class SplitModeParam : ParameterOverride<SplitMode> { }

        [DisplayName("Method"), Tooltip("Box filtered methods provide a subtle blur effect and are less efficient")]
        public SplitModeParam mode = new SplitModeParam { value = SplitMode.Single };

        [Range(0f, 1f), Tooltip("The amount by which the color channels offset")]
        public FloatParameter offset = new FloatParameter { value = 0f };
        
        [Range(0f, 1f), Tooltip("0=Full screen. 1=Limit to screen edges")]
        public FloatParameter edgeMasking = new FloatParameter { value = 0f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (offset == 0) { return false; }
                return true;
            }

            return false;
        }
        
        [SerializeField]
        public Shader shader;
        
        private void Reset()
        {
            SerializeShader();
        }
        
        private bool SerializeShader()
        {
            bool wasSerialized = !shader;
            shader = Shader.Find(ShaderNames.ColorSplit);

            return wasSerialized;
        }
    }
}
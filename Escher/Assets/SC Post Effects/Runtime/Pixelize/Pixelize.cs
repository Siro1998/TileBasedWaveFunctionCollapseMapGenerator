using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    [PostProcess(typeof(PixelizeRenderer), PostProcessEvent.BeforeStack, "SC Post Effects/Retro/Pixelize", true)]
    [Serializable]
    public sealed class Pixelize : PostProcessEffectSettings
    {
        [Range(0f, 1f), Tooltip("Amount")]
        public UnityEngine.Rendering.PostProcessing.FloatParameter amount = new UnityEngine.Rendering.PostProcessing.FloatParameter { value = 0f };
        [UnityEngine.Rendering.PostProcessing.Min(0f)]
        public UnityEngine.Rendering.PostProcessing.IntParameter resolution = new UnityEngine.Rendering.PostProcessing.IntParameter { value = 240 };

        public enum Resolution
        {
            Custom = 1,
            [InspectorName("600p")]
            Sixhundred = 600,
            [InspectorName("480p")]
            FourEighty = 480,
            [InspectorName("240p")]
            TwoFourty = 240,
            [InspectorName("200p")]
            TwoHundred = 200,
            [InspectorName("160p")]
            HundredSixty = 160
        }
        
        [Serializable]
        public sealed class ResolutionPreset : ParameterOverride<Resolution> { }
        
        public ResolutionPreset resolutionPreset = new ResolutionPreset { value = Resolution.Custom };

        [Tooltip("When disabled, pixels will retain a square aspect ratio")]
        public UnityEngine.Rendering.PostProcessing.BoolParameter preserveAspectRatio = new UnityEngine.Rendering.PostProcessing.BoolParameter { value = false };
        [Tooltip("When enabled, pixels are shifted by half. Mostly has a visible effect on extremely low resolutions")]
        public UnityEngine.Rendering.PostProcessing.BoolParameter centerPixel = new UnityEngine.Rendering.PostProcessing.BoolParameter { value = true };
        
        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0f) { return false; }
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
            shader = Shader.Find(ShaderNames.Pixelize);

            return wasSerialized;
        }
    }
}
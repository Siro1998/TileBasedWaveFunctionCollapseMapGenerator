using UnityEngine.Rendering.PostProcessing;
using UnityEngine;
using UnityEngine.Rendering;

namespace SCPE
{
    public sealed class SunshaftsRenderer : PostProcessEffectRenderer<Sunshafts>
    {
        Shader shader;
        private int sunshaftBufferID = Shader.PropertyToID("_SunshaftBuffer");
        int blurredID = Shader.PropertyToID("_Temp1");
        int blurredID2 = Shader.PropertyToID("_Temp2");
        
        enum Pass
        {
            SkySource,
            RadialBlur,
            Blend
        }

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.Sunshafts);
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

    #region Parameters
            float sunIntensity = (settings.useCasterIntensity && RenderSettings.sun) ? RenderSettings.sun.intensity : settings.sunShaftIntensity.value;

            sheet.properties.SetVector("_SunPosition",-RenderSettings.sun.transform.forward * 1E10f);
            
            sheet.properties.SetFloat("_BlendMode", (int)settings.blendMode.value);
            sheet.properties.SetColor("_SunColor", settings.useCasterColor && RenderSettings.sun ? RenderSettings.sun.color : settings.sunColor.value);
            sheet.properties.SetColor("_SunThreshold", settings.sunThreshold);
            
            sheet.properties.SetVector(ShaderParameters.Params, new Vector4(sunIntensity, settings.falloff.value, 0, 0));
    #endregion

            int res = (int)settings.resolution.value;

            cmd.GetTemporaryRT(blurredID, context.width / res, context.height / res, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.width / res, context.height / res, 0, FilterMode.Bilinear);
            
            //Create skybox mask
            context.command.BlitFullscreenTriangle(context.source, blurredID, sheet, (int)Pass.SkySource);

            //Blur buffer
    #region Blur
            cmd.BeginSample("Sunshafts blur");
            
            float offset = settings.length * (1.0f / 768.0f);

            int iterations = (settings.highQuality) ? 2 : 1;
            float blurAmount = (settings.highQuality) ? settings.length / 3f : settings.length;
            
            for (int i = 0; i < iterations; i++)
            {
                context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, (int)Pass.RadialBlur);
                offset = blurAmount * (((i * 2.0f + 1.0f) * 6.0f)) / context.screenWidth;
                sheet.properties.SetFloat(ShaderParameters.BlurRadius, offset);

                context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, (int)Pass.RadialBlur);
                offset = blurAmount * (((i * 2.0f + 2.0f) * 6.0f)) / context.screenWidth;
                sheet.properties.SetFloat(ShaderParameters.BlurRadius, offset);

            }
            cmd.EndSample("Sunshafts blur");

            cmd.SetGlobalTexture(sunshaftBufferID, blurredID);
    #endregion

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)Pass.Blend);

            cmd.ReleaseTemporaryRT(blurredID);
            cmd.ReleaseTemporaryRT(blurredID2);
        }
    }
}

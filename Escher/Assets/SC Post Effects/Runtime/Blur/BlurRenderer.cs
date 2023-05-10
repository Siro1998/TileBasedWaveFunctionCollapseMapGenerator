using UnityEngine.Rendering;
using UnityEngine;

using UnityEngine.Rendering.PostProcessing;
using TextureParameter = UnityEngine.Rendering.PostProcessing.TextureParameter;
using BoolParameter = UnityEngine.Rendering.PostProcessing.BoolParameter;
using FloatParameter = UnityEngine.Rendering.PostProcessing.FloatParameter;
using IntParameter = UnityEngine.Rendering.PostProcessing.IntParameter;
using ColorParameter = UnityEngine.Rendering.PostProcessing.ColorParameter;
using MinAttribute = UnityEngine.Rendering.PostProcessing.MinAttribute;

namespace SCPE 
{
    public sealed class BlurRenderer : PostProcessEffectRenderer<Blur>
    {
        Shader shader;
        int blurredID = Shader.PropertyToID("_Temp1");
        int blurredID2 = Shader.PropertyToID("_Temp2");

        enum Pass
        {
            Blend,
            BlendDepthFade,
            Gaussian,
            Box
        }

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.Blur);
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;
            
            //Swap buffer
            cmd.GetTemporaryRT(blurredID, context.screenWidth / settings.downscaling, context.screenHeight / settings.downscaling, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2, context.screenWidth / settings.downscaling, context.screenHeight / settings.downscaling, 0, FilterMode.Bilinear);

            //Color copy
            cmd.BlitFullscreenTriangle(context.source, blurredID);

            int blurPass = (settings.mode == Blur.BlurMethod.Gaussian) ? (int)Pass.Gaussian : (int)Pass.Box;

            for (int i = 0; i < settings.iterations; i++)
            {
                // horizontal blur
                cmd.SetGlobalVector(ShaderParameters.BlurOffsets, new Vector4(settings.amount / context.screenWidth, 0, 0, 0));
                cmd.BlitFullscreenTriangle(blurredID, blurredID2, sheet, blurPass);

                // vertical blur
                cmd.SetGlobalVector(ShaderParameters.BlurOffsets, new Vector4(0, settings.amount / context.screenHeight, 0, 0));
                cmd.BlitFullscreenTriangle(blurredID2, blurredID, sheet, blurPass);

                //Double blur
                if (settings.highQuality)
                {
                    // horizontal blur
                    cmd.SetGlobalVector(ShaderParameters.BlurOffsets, new Vector4(settings.amount / context.screenWidth, 0, 0, 0));
                    context.command.BlitFullscreenTriangle(blurredID, blurredID2, sheet, blurPass);

                    // vertical blur
                    cmd.SetGlobalVector(ShaderParameters.BlurOffsets, new Vector4(0, settings.amount / context.screenHeight, 0, 0));
                    context.command.BlitFullscreenTriangle(blurredID2, blurredID, sheet, blurPass);
                }
            }

            cmd.SetGlobalTexture("_BlurredTex", blurredID);
            
            if( settings.distanceFade.value) cmd.SetGlobalVector(ShaderParameters.FadeParams, new Vector4(settings.startFadeDistance.value, settings.endFadeDistance.value, 0, 0));
            
            // Render blurred texture in blend pass
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, settings.distanceFade.value ? (int)Pass.BlendDepthFade : (int)Pass.Blend);

            // release
            cmd.ReleaseTemporaryRT(blurredID);
            cmd.ReleaseTemporaryRT(blurredID2);
        }
    }
}
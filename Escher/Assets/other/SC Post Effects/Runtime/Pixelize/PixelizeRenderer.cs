using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    public sealed class PixelizeRenderer : PostProcessEffectRenderer<Pixelize>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.Pixelize);
        }
        
        private static readonly int _PixelizeParams = Shader.PropertyToID("_PixelizeParams");
        private static Vector4 pixelizeParams;
        
        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            var resolution = settings.resolutionPreset.value == Pixelize.Resolution.Custom ? settings.resolution.value : (int)settings.resolutionPreset.value;

            pixelizeParams.x = (settings.preserveAspectRatio.value ? context.screenWidth : context.screenHeight) / (float)resolution;
            pixelizeParams.y = context.screenHeight / (float)resolution;
            pixelizeParams.z = settings.amount.value;
            pixelizeParams.w = settings.centerPixel.value ? 1 : 0;
            
            sheet.properties.SetVector(_PixelizeParams, pixelizeParams);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
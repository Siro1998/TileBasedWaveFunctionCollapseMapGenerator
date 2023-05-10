using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    public sealed class ColorSplitRenderer : PostProcessEffectRenderer<ColorSplit>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.ColorSplit);
        }

        public override void Release()
        {
            base.Release();
        }

        public override void Render(PostProcessRenderContext context)
        {
            var sheet = context.propertySheets.Get(shader);

            sheet.properties.SetVector(ShaderParameters.Params, new Vector4(settings.offset.value / 100, settings.edgeMasking.value, 0, 0));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, (int)settings.mode.value);
        }
    }
}
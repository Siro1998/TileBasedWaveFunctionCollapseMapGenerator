using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

namespace SCPE
{
    public sealed class TiltShiftRenderer : PostProcessEffectRenderer<TiltShift>
    {
        Shader shader;

        public override void Init()
        {
            shader = Shader.Find(ShaderNames.TiltShift);
        }

        enum Pass
        {
            FragHorizontal,
            FragHorizontalHQ,
            FragRadial,
            FragRadialHQ,
            FragDebug
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            sheet.properties.SetVector(ShaderParameters.Params, new Vector4(settings.areaSize.value, settings.areaFalloff.value, settings.amount.value, (int)settings.mode.value));
            sheet.properties.SetFloat("_Offset", settings.offset.value);
            sheet.properties.SetFloat("_Angle", settings.angle.value);

            //Copy screen contents
            int pass = (int)settings.mode.value + (int)settings.quality.value;

            switch ((int)settings.mode.value)
            {
                case 0:
                    pass = 0 + (int)settings.quality.value;
                    break;
                case 1:
                    pass = 2 + (int)settings.quality.value;
                    break;
            }

            // Render blurred texture in blend pass
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, TiltShift.debug ? (int)Pass.FragDebug : pass);
        }
    }
}
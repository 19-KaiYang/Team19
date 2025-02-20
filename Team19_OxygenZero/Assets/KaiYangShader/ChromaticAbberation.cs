using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Custom Post-Processing/Chromatic Aberration", typeof(UniversalRenderPipeline))]
public class ChromaticAberration : VolumeComponent, IPostProcessComponent
{
    [Tooltip("Intensity of the chromatic aberration effect")]
    public ClampedFloatParameter intensity = new ClampedFloatParameter(0.0f, 0.0f, 1.0f);

    public bool IsActive() => intensity.value > 0.0f && active;

    public bool IsTileCompatible() => true;
}



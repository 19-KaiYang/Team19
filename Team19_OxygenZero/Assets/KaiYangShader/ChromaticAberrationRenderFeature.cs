using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationRenderFeature : ScriptableRendererFeature
{
    private ChromaticAberrationPass chromaticAberrationPass;

    public override void Create()
    {
        chromaticAberrationPass = new ChromaticAberrationPass();
        name = "Chromatic Aberration";
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(chromaticAberrationPass);
    }
}


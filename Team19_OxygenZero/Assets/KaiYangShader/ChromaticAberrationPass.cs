using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationPass : ScriptableRenderPass
{
    private Material material;
    private ChromaticAberration volumeComponent;
    private RenderTargetIdentifier src;
    private int tempTexID;

    public ChromaticAberrationPass()
    {
        if (!material)
        {
            material = CoreUtils.CreateEngineMaterial("Custom Post-Processing/ChromaticAberration");
        }
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        volumeComponent = VolumeManager.instance.stack.GetComponent<ChromaticAberration>();
        src = renderingData.cameraData.renderer.cameraColorTargetHandle;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (volumeComponent == null || !volumeComponent.IsActive())
            return;

        tempTexID = Shader.PropertyToID("_TempTex");
        cmd.GetTemporaryRT(tempTexID, cameraTextureDescriptor);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (volumeComponent == null || !volumeComponent.IsActive())
            return;

        CommandBuffer cmd = CommandBufferPool.Get("Custom Post-Processing/Chromatic Aberration");

        material.SetFloat("_Intensity", volumeComponent.intensity.value);

        cmd.Blit(src, tempTexID, material, 0);
        cmd.Blit(tempTexID, src);

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }
}



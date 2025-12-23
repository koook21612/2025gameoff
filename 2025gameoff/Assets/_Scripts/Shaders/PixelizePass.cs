using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PixelizePass : ScriptableRenderPass
{
    private PixelizeFeature.CustomPassSettings settings;

    private RTHandle colorBuffer;
    private RTHandle pixelBuffer;

    // 使用RTHandle引用
    private const string pixelBufferName = "_PixelBuffer";

    private Material material;
    private int pixelScreenHeight, pixelScreenWidth;

    public PixelizePass(PixelizeFeature.CustomPassSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = settings.renderPassEvent;

        // 创建材质
        if (material == null)
        {
            if (Shader.Find("Hidden/Pixelize") != null)
                material = CoreUtils.CreateEngineMaterial("Hidden/Pixelize");
            else
                Debug.LogWarning("Pixelize shader not found");
        }

        // 配置输入
        ConfigureInput(ScriptableRenderPassInput.Color);
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // 获取相机的颜色缓冲区句柄
        colorBuffer = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // 计算像素化分辨率
        pixelScreenHeight = settings.screenHeight;
        pixelScreenWidth = (int)(pixelScreenHeight * renderingData.cameraData.camera.aspect + 0.5f);

        // 设置材质属性
        if (material != null)
        {
            material.SetVector("_BlockCount", new Vector2(pixelScreenWidth, pixelScreenHeight));
            material.SetVector("_BlockSize", new Vector2(1.0f / pixelScreenWidth, 1.0f / pixelScreenHeight));
            material.SetVector("_HalfBlockSize", new Vector2(0.5f / pixelScreenWidth, 0.5f / pixelScreenHeight));
        }

        // 创建像素化缓冲区描述符
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.height = pixelScreenHeight;
        descriptor.width = pixelScreenWidth;
        descriptor.msaaSamples = 1;
        descriptor.depthBufferBits = 0; // 不需要深度缓冲区

        // 分配RTHandle
        RenderingUtils.ReAllocateIfNeeded(ref pixelBuffer, descriptor,
            FilterMode.Point, TextureWrapMode.Clamp,
            name: pixelBufferName);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (material == null)
        {
            Debug.LogError("Pixelize material is null");
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, new ProfilingSampler("Pixelize Pass")))
        {
            // 使用新的Blit API
            Blitter.BlitCameraTexture(cmd, colorBuffer, pixelBuffer, material, 0);
            Blitter.BlitCameraTexture(cmd, pixelBuffer, colorBuffer);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // 释放RTHandle（如果需要
    }

    // 清理资源
    public void Dispose()
    {
        pixelBuffer?.Release();
        CoreUtils.Destroy(material);
    }
}
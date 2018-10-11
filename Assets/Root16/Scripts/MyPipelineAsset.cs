using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class MyPipeline : RenderPipeline 
{
	private CullResults cull;
	CommandBuffer cameraBuffer = new CommandBuffer()
	{
		name = "Render Camera"
	};

	public override void Render(ScriptableRenderContext rc, Camera[] cameras)
	{
		base.Render(rc, cameras);

		foreach(var thisCamera in cameras)
		{
			Render(rc, thisCamera);
		}
	}

	void Render(ScriptableRenderContext rc, Camera camera)
	{
		// Get camera culling parameters
		ScriptableCullingParameters cullingParameters;
		if(!CullResults.GetCullingParameters(camera, out cullingParameters))
			return;
		
		// perform culling, CullResults stores information on what is visible
		CullResults.Cull(ref cullingParameters, rc, ref cull);

		// Setup camera properties
		rc.SetupCameraProperties(camera);

		CameraClearFlags clearFlags = camera.clearFlags;
		cameraBuffer.ClearRenderTarget(
			(clearFlags & CameraClearFlags.Depth) != 0,
			(clearFlags & CameraClearFlags.Color) != 0,
			camera.backgroundColor
		);

		// Execute command buffer
		rc.ExecuteCommandBuffer(cameraBuffer);

		// release it!
		cameraBuffer.Clear();

		// DRAW!!!!
		var drawSettings = new DrawRendererSettings(camera, new ShaderPassName("SRPDefaultUnlit"));
		var filterSettings = new FilterRenderersSettings(true);

		// First draw opaque objects
		drawSettings.sorting.flags = SortFlags.CommonOpaque;
		filterSettings.renderQueueRange = RenderQueueRange.opaque;
		rc.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

		rc.DrawSkybox(camera);

		// draw transparent objects after skybox is drawn!
		drawSettings.sorting.flags = SortFlags.CommonTransparent;
		filterSettings.renderQueueRange = RenderQueueRange.transparent;
		rc.DrawRenderers(cull.visibleRenderers, ref drawSettings, filterSettings);

		rc.Submit();
	}
	
}

[CreateAssetMenu(menuName = "Rendering/My Pipeline")]
public class MyPipelineAsset : RenderPipelineAsset
{
	protected override IRenderPipeline InternalCreatePipeline()
	{
		return new MyPipeline();
	}
}

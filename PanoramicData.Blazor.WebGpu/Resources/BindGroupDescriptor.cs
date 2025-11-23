namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Descriptor for creating a bind group.
/// </summary>
public class BindGroupDescriptor
{
	/// <summary>
	/// Gets or sets the bind group layout (optional, can be inferred from pipeline).
	/// </summary>
	public object? Layout { get; set; }

	/// <summary>
	/// Gets or sets the bind group entries.
	/// </summary>
	public BindGroupEntry[]? Entries { get; set; }
}

/// <summary>
/// Bind group entry configuration.
/// </summary>
public class BindGroupEntry
{
	/// <summary>
	/// Gets or sets the binding index.
	/// </summary>
	public uint Binding { get; set; }

	/// <summary>
	/// Gets or sets the resource to bind (buffer, texture, or sampler).
	/// </summary>
	public object? Resource { get; set; }

	/// <summary>
	/// Gets or sets the buffer offset (if binding a buffer).
	/// </summary>
	public ulong Offset { get; set; }

	/// <summary>
	/// Gets or sets the buffer size (if binding a buffer, 0 means entire buffer).
	/// </summary>
	public ulong Size { get; set; }
}

/// <summary>
/// Color attachment configuration for render passes.
/// </summary>
public class ColorAttachment
{
	/// <summary>
	/// Gets or sets the texture view to render to.
	/// </summary>
	public object? View { get; set; }

	/// <summary>
	/// Gets or sets the resolve target (for MSAA).
	/// </summary>
	public object? ResolveTarget { get; set; }

	/// <summary>
	/// Gets or sets the load operation (load or clear).
	/// </summary>
	public string LoadOp { get; set; } = "clear";

	/// <summary>
	/// Gets or sets the store operation (store or discard).
	/// </summary>
	public string StoreOp { get; set; } = "store";

	/// <summary>
	/// Gets or sets the clear value (RGBA, used if LoadOp is clear).
	/// </summary>
	public System.Numerics.Vector4 ClearValue { get; set; } = new(0, 0, 0, 1);
}

/// <summary>
/// Depth/stencil attachment configuration for render passes.
/// </summary>
public class DepthStencilAttachment
{
	/// <summary>
	/// Gets or sets the depth/stencil texture view.
	/// </summary>
	public object? View { get; set; }

	/// <summary>
	/// Gets or sets the depth load operation.
	/// </summary>
	public string DepthLoadOp { get; set; } = "clear";

	/// <summary>
	/// Gets or sets the depth store operation.
	/// </summary>
	public string DepthStoreOp { get; set; } = "store";

	/// <summary>
	/// Gets or sets the depth clear value.
	/// </summary>
	public float DepthClearValue { get; set; } = 1.0f;

	/// <summary>
	/// Gets or sets whether depth is read-only.
	/// </summary>
	public bool DepthReadOnly { get; set; }

	/// <summary>
	/// Gets or sets the stencil load operation.
	/// </summary>
	public string StencilLoadOp { get; set; } = "clear";

	/// <summary>
	/// Gets or sets the stencil store operation.
	/// </summary>
	public string StencilStoreOp { get; set; } = "store";

	/// <summary>
	/// Gets or sets the stencil clear value.
	/// </summary>
	public uint StencilClearValue { get; set; }

	/// <summary>
	/// Gets or sets whether stencil is read-only.
	/// </summary>
	public bool StencilReadOnly { get; set; }
}

/// <summary>
/// Render pass descriptor configuration.
/// </summary>
public class RenderPassDescriptor
{
	/// <summary>
	/// Gets or sets the color attachments.
	/// </summary>
	public ColorAttachment[]? ColorAttachments { get; set; }

	/// <summary>
	/// Gets or sets the depth/stencil attachment.
	/// </summary>
	public DepthStencilAttachment? DepthStencilAttachment { get; set; }

	/// <summary>
	/// Gets or sets the occlusion query set.
	/// </summary>
	public object? OcclusionQuerySet { get; set; }

	/// <summary>
	/// Gets or sets the timestamp writes.
	/// </summary>
	public object? TimestampWrites { get; set; }
}

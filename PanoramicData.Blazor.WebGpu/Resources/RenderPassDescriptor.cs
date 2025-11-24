namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Descriptor for creating a render pass.
/// </summary>
public class RenderPassDescriptor
{
	/// <summary>
	/// Gets or sets the optional label for debugging.
	/// </summary>
	public string? Label { get; set; }

	/// <summary>
	/// Gets or sets the color attachments.
	/// </summary>
	public ColorAttachment[]? ColorAttachments { get; set; }

	/// <summary>
	/// Gets or sets the depth/stencil attachment.
	/// </summary>
	public DepthStencilAttachment? DepthStencilAttachment { get; set; }
}

/// <summary>
/// Color attachment configuration for a render pass.
/// </summary>
public class ColorAttachment
{
	/// <summary>
	/// Gets or sets the texture view resource ID.
	/// </summary>
	public int? ViewId { get; set; }

	/// <summary>
	/// Gets or sets the resolve target texture view resource ID (for multisampling).
	/// </summary>
	public int? ResolveTargetId { get; set; }

	/// <summary>
	/// Gets or sets the load operation ("load" or "clear").
	/// </summary>
	public string LoadOp { get; set; } = "clear";

	/// <summary>
	/// Gets or sets the store operation ("store" or "discard").
	/// </summary>
	public string StoreOp { get; set; } = "store";

	/// <summary>
	/// Gets or sets the clear color value.
	/// </summary>
	public ClearColor? ClearValue { get; set; }
}

/// <summary>
/// Depth/stencil attachment configuration for a render pass.
/// </summary>
public class DepthStencilAttachment
{
	/// <summary>
	/// Gets or sets the texture view resource ID.
	/// </summary>
	public int ViewId { get; set; }

	/// <summary>
	/// Gets or sets the depth load operation.
	/// </summary>
	public string? DepthLoadOp { get; set; }

	/// <summary>
	/// Gets or sets the depth store operation.
	/// </summary>
	public string? DepthStoreOp { get; set; }

	/// <summary>
	/// Gets or sets the depth clear value.
	/// </summary>
	public float? DepthClearValue { get; set; }

	/// <summary>
	/// Gets or sets the stencil load operation.
	/// </summary>
	public string? StencilLoadOp { get; set; }

	/// <summary>
	/// Gets or sets the stencil store operation.
	/// </summary>
	public string? StencilStoreOp { get; set; }

	/// <summary>
	/// Gets or sets the stencil clear value.
	/// </summary>
	public uint? StencilClearValue { get; set; }
}

/// <summary>
/// Clear color value.
/// </summary>
public class ClearColor
{
	/// <summary>
	/// Gets or sets the red component (0.0 to 1.0).
	/// </summary>
	public double R { get; set; }

	/// <summary>
	/// Gets or sets the green component (0.0 to 1.0).
	/// </summary>
	public double G { get; set; }

	/// <summary>
	/// Gets or sets the blue component (0.0 to 1.0).
	/// </summary>
	public double B { get; set; }

	/// <summary>
	/// Gets or sets the alpha component (0.0 to 1.0).
	/// </summary>
	public double A { get; set; } = 1.0;

	/// <summary>
	/// Creates a new clear color.
	/// </summary>
	public ClearColor(double r, double g, double b, double a = 1.0)
	{
		R = r;
		G = g;
		B = b;
		A = a;
	}

	/// <summary>
	/// Creates a black clear color.
	/// </summary>
	public static ClearColor Black => new(0, 0, 0, 1);

	/// <summary>
	/// Creates a white clear color.
	/// </summary>
	public static ClearColor White => new(1, 1, 1, 1);

	/// <summary>
	/// Creates a transparent clear color.
	/// </summary>
	public static ClearColor Transparent => new(0, 0, 0, 0);
}

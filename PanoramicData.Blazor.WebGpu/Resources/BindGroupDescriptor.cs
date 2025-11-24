namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Descriptor for creating a bind group.
/// </summary>
public class BindGroupDescriptor
{
	/// <summary>
	/// Gets or sets the bind group layout resource ID (optional, can be inferred from pipeline).
	/// </summary>
	public int? LayoutId { get; set; }

	/// <summary>
	/// Gets or sets the pipeline resource ID to get the layout from (alternative to LayoutId).
	/// </summary>
	public int? PipelineId { get; set; }

	/// <summary>
	/// Gets or sets the bind group index when getting layout from pipeline (default: 0).
	/// </summary>
	public int GroupIndex { get; set; } = 0;

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
	/// Gets or sets the resource ID to bind (buffer, texture, or sampler).
	/// </summary>
	public int ResourceId { get; set; }

	/// <summary>
	/// Gets or sets the resource type ("buffer", "texture", "sampler").
	/// </summary>
	public string ResourceType { get; set; } = "buffer";

	/// <summary>
	/// Gets or sets the buffer offset (if binding a buffer).
	/// </summary>
	public ulong Offset { get; set; }

	/// <summary>
	/// Gets or sets the buffer size (if binding a buffer, 0 means entire buffer).
	/// </summary>
	public ulong Size { get; set; }
}

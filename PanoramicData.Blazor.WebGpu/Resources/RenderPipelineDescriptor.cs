namespace PanoramicData.Blazor.WebGpu.Resources;

/// <summary>
/// Descriptor for creating a render pipeline.
/// </summary>
public class RenderPipelineDescriptor
{
	/// <summary>
	/// Gets or sets the vertex shader configuration.
	/// </summary>
	public VertexState? Vertex { get; set; }

	/// <summary>
	/// Gets or sets the fragment shader configuration.
	/// </summary>
	public FragmentState? Fragment { get; set; }

	/// <summary>
	/// Gets or sets the primitive assembly configuration.
	/// </summary>
	public PrimitiveState? Primitive { get; set; }

	/// <summary>
	/// Gets or sets the depth/stencil configuration.
	/// </summary>
	public DepthStencilState? DepthStencil { get; set; }

	/// <summary>
	/// Gets or sets the multisample configuration.
	/// </summary>
	public MultisampleState? Multisample { get; set; }
}

/// <summary>
/// Vertex shader stage configuration.
/// </summary>
public class VertexState
{
	/// <summary>
	/// Gets or sets the shader module.
	/// </summary>
	public PDWebGpuShader? Shader { get; set; }

	/// <summary>
	/// Gets or sets the entry point function name.
	/// </summary>
	public string? EntryPoint { get; set; } = "main";

	/// <summary>
	/// Gets or sets the vertex buffer layouts.
	/// </summary>
	public VertexBufferLayout[]? Buffers { get; set; }
}

/// <summary>
/// Fragment shader stage configuration.
/// </summary>
public class FragmentState
{
	/// <summary>
	/// Gets or sets the shader module.
	/// </summary>
	public PDWebGpuShader? Shader { get; set; }

	/// <summary>
	/// Gets or sets the entry point function name.
	/// </summary>
	public string? EntryPoint { get; set; } = "main";

	/// <summary>
	/// Gets or sets the color target states.
	/// </summary>
	public ColorTargetState[]? Targets { get; set; }
}

/// <summary>
/// Vertex buffer layout configuration.
/// </summary>
public class VertexBufferLayout
{
	/// <summary>
	/// Gets or sets the stride in bytes between vertex data.
	/// </summary>
	public ulong ArrayStride { get; set; }

	/// <summary>
	/// Gets or sets the step mode (vertex or instance).
	/// </summary>
	public string StepMode { get; set; } = "vertex";

	/// <summary>
	/// Gets or sets the vertex attributes.
	/// </summary>
	public VertexAttribute[]? Attributes { get; set; }
}

/// <summary>
/// Vertex attribute configuration.
/// </summary>
public class VertexAttribute
{
	/// <summary>
	/// Gets or sets the vertex format (e.g., float32x3, float32x2).
	/// </summary>
	public string Format { get; set; } = "float32x3";

	/// <summary>
	/// Gets or sets the byte offset from the start of vertex data.
	/// </summary>
	public ulong Offset { get; set; }

	/// <summary>
	/// Gets or sets the shader location (attribute index).
	/// </summary>
	public uint ShaderLocation { get; set; }
}

/// <summary>
/// Color target state configuration.
/// </summary>
public class ColorTargetState
{
	/// <summary>
	/// Gets or sets the texture format.
	/// </summary>
	public string Format { get; set; } = "bgra8unorm";

	/// <summary>
	/// Gets or sets the blend state (optional).
	/// </summary>
	public BlendState? Blend { get; set; }

	/// <summary>
	/// Gets or sets the write mask.
	/// </summary>
	public uint WriteMask { get; set; } = 0xF; // All channels
}

/// <summary>
/// Blend state configuration.
/// </summary>
public class BlendState
{
	/// <summary>
	/// Gets or sets the color blend component.
	/// </summary>
	public BlendComponent? Color { get; set; }

	/// <summary>
	/// Gets or sets the alpha blend component.
	/// </summary>
	public BlendComponent? Alpha { get; set; }
}

/// <summary>
/// Blend component configuration.
/// </summary>
public class BlendComponent
{
	/// <summary>
	/// Gets or sets the source factor.
	/// </summary>
	public string SrcFactor { get; set; } = "one";

	/// <summary>
	/// Gets or sets the destination factor.
	/// </summary>
	public string DstFactor { get; set; } = "zero";

	/// <summary>
	/// Gets or sets the blend operation.
	/// </summary>
	public string Operation { get; set; } = "add";
}

/// <summary>
/// Primitive assembly state configuration.
/// </summary>
public class PrimitiveState
{
	/// <summary>
	/// Gets or sets the primitive topology (triangle-list, line-list, point-list).
	/// </summary>
	public string Topology { get; set; } = "triangle-list";

	/// <summary>
	/// Gets or sets the strip index format (optional, for strip topologies).
	/// </summary>
	public string? StripIndexFormat { get; set; }

	/// <summary>
	/// Gets or sets the front face winding (ccw or cw).
	/// </summary>
	public string FrontFace { get; set; } = "ccw";

	/// <summary>
	/// Gets or sets the cull mode (none, front, back).
	/// </summary>
	public string CullMode { get; set; } = "none";
}

/// <summary>
/// Depth/stencil state configuration.
/// </summary>
public class DepthStencilState
{
	/// <summary>
	/// Gets or sets the depth/stencil texture format.
	/// </summary>
	public string Format { get; set; } = "depth24plus";

	/// <summary>
	/// Gets or sets whether depth writes are enabled.
	/// </summary>
	public bool DepthWriteEnabled { get; set; } = true;

	/// <summary>
	/// Gets or sets the depth compare function.
	/// </summary>
	public string DepthCompare { get; set; } = "less";

	/// <summary>
	/// Gets or sets the stencil front face configuration.
	/// </summary>
	public StencilFaceState? StencilFront { get; set; }

	/// <summary>
	/// Gets or sets the stencil back face configuration.
	/// </summary>
	public StencilFaceState? StencilBack { get; set; }

	/// <summary>
	/// Gets or sets the stencil read mask.
	/// </summary>
	public uint StencilReadMask { get; set; } = 0xFFFFFFFF;

	/// <summary>
	/// Gets or sets the stencil write mask.
	/// </summary>
	public uint StencilWriteMask { get; set; } = 0xFFFFFFFF;

	/// <summary>
	/// Gets or sets the depth bias.
	/// </summary>
	public int DepthBias { get; set; }

	/// <summary>
	/// Gets or sets the depth bias slope scale.
	/// </summary>
	public float DepthBiasSlopeScale { get; set; }

	/// <summary>
	/// Gets or sets the depth bias clamp.
	/// </summary>
	public float DepthBiasClamp { get; set; }
}

/// <summary>
/// Stencil face state configuration.
/// </summary>
public class StencilFaceState
{
	/// <summary>
	/// Gets or sets the compare function.
	/// </summary>
	public string Compare { get; set; } = "always";

	/// <summary>
	/// Gets or sets the stencil fail operation.
	/// </summary>
	public string FailOp { get; set; } = "keep";

	/// <summary>
	/// Gets or sets the depth fail operation.
	/// </summary>
	public string DepthFailOp { get; set; } = "keep";

	/// <summary>
	/// Gets or sets the pass operation.
	/// </summary>
	public string PassOp { get; set; } = "keep";
}

/// <summary>
/// Multisample state configuration.
/// </summary>
public class MultisampleState
{
	/// <summary>
	/// Gets or sets the sample count.
	/// </summary>
	public uint Count { get; set; } = 1;

	/// <summary>
	/// Gets or sets the sample mask.
	/// </summary>
	public uint Mask { get; set; } = 0xFFFFFFFF;

	/// <summary>
	/// Gets or sets whether alpha to coverage is enabled.
	/// </summary>
	public bool AlphaToCoverageEnabled { get; set; }
}

namespace PanoramicData.Blazor.WebGpu.Tests.Infrastructure.Utilities;

/// <summary>
/// Provides test data and sample resources for WebGPU tests.
/// </summary>
public static class TestData
{
	/// <summary>
	/// Simple vertex shader in WGSL format for testing.
	/// </summary>
	public const string SimpleVertexShader = @"
@vertex
fn main(@location(0) position: vec3<f32>) -> @builtin(position) vec4<f32> {
    return vec4<f32>(position, 1.0);
}
";

	/// <summary>
	/// Simple fragment shader in WGSL format for testing.
	/// </summary>
	public const string SimpleFragmentShader = @"
@fragment
fn main() -> @location(0) vec4<f32> {
    return vec4<f32>(1.0, 0.0, 0.0, 1.0);
}
";

	/// <summary>
	/// Invalid WGSL shader that should cause compilation error.
	/// </summary>
	public const string InvalidShader = @"
@vertex
fn main() -> invalid_type {
    return something_wrong;
}
";

	/// <summary>
	/// Sample vertex data for a triangle.
	/// </summary>
	public static readonly float[] TriangleVertices =
	[
		 0.0f,  0.5f, 0.0f,  // Top vertex
		-0.5f, -0.5f, 0.0f,  // Bottom left
		 0.5f, -0.5f, 0.0f   // Bottom right
	];

	/// <summary>
	/// Sample vertex data for a cube.
	/// </summary>
	public static readonly float[] CubeVertices =
	[
		// Front face
		-0.5f, -0.5f,  0.5f,
		 0.5f, -0.5f,  0.5f,
		 0.5f,  0.5f,  0.5f,
		-0.5f,  0.5f,  0.5f,
		// Back face
		-0.5f, -0.5f, -0.5f,
		-0.5f,  0.5f, -0.5f,
		 0.5f,  0.5f, -0.5f,
		 0.5f, -0.5f, -0.5f,
	];

	/// <summary>
	/// Sample indices for a cube.
	/// </summary>
	public static readonly ushort[] CubeIndices =
	[
		0, 1, 2, 0, 2, 3,    // Front face
		4, 5, 6, 4, 6, 7,    // Back face
		4, 0, 3, 4, 3, 5,    // Left face
		1, 7, 6, 1, 6, 2,    // Right face
		3, 2, 6, 3, 6, 5,    // Top face
		4, 7, 1, 4, 1, 0     // Bottom face
	];
}

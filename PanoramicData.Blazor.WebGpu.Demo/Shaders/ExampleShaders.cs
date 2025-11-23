namespace PanoramicData.Blazor.WebGpu.Demo.Shaders;

/// <summary>
/// Collection of example WGSL shaders for demonstration purposes.
/// </summary>
public static class ExampleShaders
{
	/// <summary>
	/// Simple triangle vertex shader.
	/// </summary>
	public const string SimpleTriangleVertex = @"
@vertex
fn main(@builtin(vertex_index) vertexIndex: u32) -> @builtin(position) vec4<f32> {
    var positions = array<vec2<f32>, 3>(
        vec2<f32>(0.0, 0.5),
        vec2<f32>(-0.5, -0.5),
        vec2<f32>(0.5, -0.5)
    );
    
    return vec4<f32>(positions[vertexIndex], 0.0, 1.0);
}";

	/// <summary>
	/// Simple colored fragment shader.
	/// </summary>
	public const string SimpleColoredFragment = @"
@fragment
fn main() -> @location(0) vec4<f32> {
    return vec4<f32>(1.0, 0.5, 0.2, 1.0); // Orange color
}";

	/// <summary>
	/// Rotating cube vertex shader with model-view-projection matrix.
	/// </summary>
	public const string RotatingCubeVertex = @"
struct Uniforms {
    modelViewProjection: mat4x4<f32>,
}

@group(0) @binding(0) var<uniform> uniforms: Uniforms;

struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) color: vec3<f32>,
}

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) color: vec3<f32>,
}

@vertex
fn main(input: VertexInput) -> VertexOutput {
    var output: VertexOutput;
    output.position = uniforms.modelViewProjection * vec4<f32>(input.position, 1.0);
    output.color = input.color;
    return output;
}";

	/// <summary>
	/// Simple pass-through fragment shader with color interpolation.
	/// </summary>
	public const string ColoredFragment = @"
@fragment
fn main(@location(0) color: vec3<f32>) -> @location(0) vec4<f32> {
    return vec4<f32>(color, 1.0);
}";

	/// <summary>
	/// Gradient background vertex shader (fullscreen quad).
	/// </summary>
	public const string GradientVertex = @"
@vertex
fn main(@builtin(vertex_index) vertexIndex: u32) -> @builtin(position) vec4<f32> {
    // Fullscreen quad
    var positions = array<vec2<f32>, 6>(
        vec2<f32>(-1.0, -1.0),
        vec2<f32>(1.0, -1.0),
        vec2<f32>(-1.0, 1.0),
        vec2<f32>(-1.0, 1.0),
        vec2<f32>(1.0, -1.0),
        vec2<f32>(1.0, 1.0)
    );
    
    return vec4<f32>(positions[vertexIndex], 0.0, 1.0);
}";

	/// <summary>
	/// Animated gradient fragment shader with time uniform.
	/// </summary>
	public const string AnimatedGradientFragment = @"
struct TimeUniform {
    time: f32,
}

@group(0) @binding(0) var<uniform> timeData: TimeUniform;

@fragment
fn main(@builtin(position) fragCoord: vec4<f32>) -> @location(0) vec4<f32> {
    let resolution = vec2<f32>(800.0, 600.0);
    let uv = fragCoord.xy / resolution;
    
    let r = 0.5 + 0.5 * sin(timeData.time + uv.x * 3.14159);
    let g = 0.5 + 0.5 * sin(timeData.time + uv.y * 3.14159 + 2.0);
    let b = 0.5 + 0.5 * sin(timeData.time + (uv.x + uv.y) * 3.14159 + 4.0);
    
    return vec4<f32>(r, g, b, 1.0);
}";

	/// <summary>
	/// Phong lighting vertex shader.
	/// </summary>
	public const string PhongVertex = @"
struct Uniforms {
    modelMatrix: mat4x4<f32>,
    viewMatrix: mat4x4<f32>,
    projectionMatrix: mat4x4<f32>,
    normalMatrix: mat4x4<f32>,
}

@group(0) @binding(0) var<uniform> uniforms: Uniforms;

struct VertexInput {
    @location(0) position: vec3<f32>,
    @location(1) normal: vec3<f32>,
}

struct VertexOutput {
    @builtin(position) position: vec4<f32>,
    @location(0) worldPosition: vec3<f32>,
    @location(1) normal: vec3<f32>,
}

@vertex
fn main(input: VertexInput) -> VertexOutput {
    var output: VertexOutput;
    
    let worldPos = uniforms.modelMatrix * vec4<f32>(input.position, 1.0);
    output.worldPosition = worldPos.xyz;
    output.position = uniforms.projectionMatrix * uniforms.viewMatrix * worldPos;
    output.normal = (uniforms.normalMatrix * vec4<f32>(input.normal, 0.0)).xyz;
    
    return output;
}";

	/// <summary>
	/// Phong lighting fragment shader.
	/// </summary>
	public const string PhongFragment = @"
struct LightUniforms {
    lightPosition: vec3<f32>,
    lightColor: vec3<f32>,
    ambientColor: vec3<f32>,
    cameraPosition: vec3<f32>,
}

@group(1) @binding(0) var<uniform> light: LightUniforms;

@fragment
fn main(
    @location(0) worldPosition: vec3<f32>,
    @location(1) normal: vec3<f32>
) -> @location(0) vec4<f32> {
    let objectColor = vec3<f32>(0.8, 0.2, 0.3);
    
    // Ambient
    let ambient = light.ambientColor * objectColor;
    
    // Diffuse
    let norm = normalize(normal);
    let lightDir = normalize(light.lightPosition - worldPosition);
    let diff = max(dot(norm, lightDir), 0.0);
    let diffuse = light.lightColor * diff * objectColor;
    
    // Specular
    let viewDir = normalize(light.cameraPosition - worldPosition);
    let reflectDir = reflect(-lightDir, norm);
    let spec = pow(max(dot(viewDir, reflectDir), 0.0), 32.0);
    let specular = light.lightColor * spec;
    
    let result = ambient + diffuse + specular;
    return vec4<f32>(result, 1.0);
}";

	/// <summary>
	/// Gets a shader example by name.
	/// </summary>
	public static string GetShader(string name) => name.ToLowerInvariant() switch
	{
		"simple-triangle-vertex" => SimpleTriangleVertex,
		"simple-colored-fragment" => SimpleColoredFragment,
		"rotating-cube-vertex" => RotatingCubeVertex,
		"colored-fragment" => ColoredFragment,
		"gradient-vertex" => GradientVertex,
		"animated-gradient-fragment" => AnimatedGradientFragment,
		"phong-vertex" => PhongVertex,
		"phong-fragment" => PhongFragment,
		_ => SimpleTriangleVertex
	};

	/// <summary>
	/// Gets all available shader examples.
	/// </summary>
	public static Dictionary<string, string> GetAllShaders() => new()
	{
		{ "Simple Triangle (Vertex)", SimpleTriangleVertex },
		{ "Simple Color (Fragment)", SimpleColoredFragment },
		{ "Rotating Cube (Vertex)", RotatingCubeVertex },
		{ "Colored (Fragment)", ColoredFragment },
		{ "Gradient (Vertex)", GradientVertex },
		{ "Animated Gradient (Fragment)", AnimatedGradientFragment },
		{ "Phong Lighting (Vertex)", PhongVertex },
		{ "Phong Lighting (Fragment)", PhongFragment }
	};
}

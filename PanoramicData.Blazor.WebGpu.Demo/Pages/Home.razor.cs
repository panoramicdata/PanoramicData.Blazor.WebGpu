using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor.WebGpu.Camera;
using PanoramicData.Blazor.WebGpu.Components;
using PanoramicData.Blazor.WebGpu.Demo.Shaders;
using PanoramicData.Blazor.WebGpu.Performance;
using PanoramicData.Blazor.WebGpu.Resources;
using PanoramicData.Blazor.WebGpu.Services;
using System.Numerics;
using System.Runtime.InteropServices;

namespace PanoramicData.Blazor.WebGpu.Demo.Pages;

/// <summary>
/// Main shader editor demo page with split-view layout and rotating cube rendering.
/// </summary>
public partial class Home : IDisposable
{
	private PDWebGpuContainer? _container;
	private PDWebGpuCameraBase? _activeCamera;
	private PDWebGpuOrbitCamera? _orbitCamera;
	private PDWebGpuFirstPersonCamera? _firstPersonCamera;
	private PDWebGpuOrthographicCamera? _orthoCamera;

	// Rendering resources
	private PDWebGpuBuffer? _vertexBuffer;
	private PDWebGpuBuffer? _indexBuffer;
	private PDWebGpuBuffer? _uniformBuffer;
	private PDWebGpuShader? _vertexShader;
	private PDWebGpuShader? _fragmentShader;
	private PDWebGpuPipeline? _pipeline;
	private PDWebGpuBindGroup? _bindGroup;
	private bool _resourcesInitialized;
	private string? _canvasContextId; // Cache the canvas context ID
	private float _elapsedTime; // Track elapsed time for animated shaders

	private string _currentShaderCode = ExampleShaders.RotatingCubeVertex;
	private string? _compilationError;
	private bool _compilationSuccess;
	private bool _isCompiling;
	private bool _showPerformance;
	private bool? _webGpuSupported;
	private string? _deviceError;
	private CameraMode _cameraMode = CameraMode.Orbit;
	private ShaderType _currentShaderType = ShaderType.Vertex; // Track current shader type

	private readonly Dictionary<string, string> _exampleShaders = ExampleShaders.GetAllShaders();

	private readonly PDWebGpuPerformanceDisplayOptions _performanceOptions = new()
	{
		ShowFPS = true,
		ShowFrameTime = true,
		ShowFrameTimeUsage = true,
		ShowDrawCalls = true,
		ShowTriangleCount = true,
		Position = CornerPosition.TopRight,
		UpdateIntervalMs = 500
	};

	[Inject]
	private IPDWebGpuService WebGpuService { get; set; } = default!;

	// Cube geometry data
	private static readonly float[] CubeVertexData =
	[
		// Position (x,y,z)     Color (r,g,b)
		// Front face (red)
		-0.5f, -0.5f,  0.5f,    1.0f, 0.0f, 0.0f,
		 0.5f, -0.5f,  0.5f,    1.0f, 0.0f, 0.0f,
		 0.5f,  0.5f,  0.5f,    1.0f, 0.0f, 0.0f,
		-0.5f,  0.5f,  0.5f,    1.0f, 0.0f, 0.0f,

		// Back face (green)
		-0.5f, -0.5f, -0.5f,    0.0f, 1.0f, 0.0f,
		-0.5f,  0.5f, -0.5f,    0.0f, 1.0f, 0.0f,
		 0.5f,  0.5f, -0.5f,    0.0f, 1.0f, 0.0f,
		 0.5f, -0.5f, -0.5f,    0.0f, 1.0f, 0.0f,

		// Top face (blue)
		-0.5f,  0.5f, -0.5f,    0.0f, 0.0f, 1.0f,
		-0.5f,  0.5f,  0.5f,    0.0f, 0.0f, 1.0f,
		 0.5f,  0.5f,  0.5f,    0.0f, 0.0f, 1.0f,
		 0.5f,  0.5f, -0.5f,    0.0f, 0.0f, 1.0f,

		// Bottom face (yellow)
		-0.5f, -0.5f, -0.5f,    1.0f, 1.0f, 0.0f,
		 0.5f, -0.5f, -0.5f,    1.0f, 1.0f, 0.0f,
		 0.5f, -0.5f,  0.5f,    1.0f, 1.0f, 0.0f,
		-0.5f, -0.5f,  0.5f,    1.0f, 1.0f, 0.0f,

		// Right face (magenta)
		 0.5f, -0.5f, -0.5f,    1.0f, 0.0f, 1.0f,
		 0.5f,  0.5f, -0.5f,    1.0f, 0.0f, 1.0f,
		 0.5f,  0.5f,  0.5f,    1.0f, 0.0f, 1.0f,
		 0.5f, -0.5f,  0.5f,    1.0f, 0.0f, 1.0f,

		// Left face (cyan)
		-0.5f, -0.5f, -0.5f,    0.0f, 1.0f, 1.0f,
		-0.5f, -0.5f,  0.5f,    0.0f, 1.0f, 1.0f,
		-0.5f,  0.5f,  0.5f,    0.0f, 1.0f, 1.0f,
		-0.5f,  0.5f, -0.5f,    0.0f, 1.0f, 1.0f,
	];

	private static readonly ushort[] CubeIndexData =
	[
		0,  1,  2,  0,  2,  3,   // front
		4,  5,  6,  4,  6,  7,   // back
		8,  9, 10,  8, 10, 11,   // top
		12, 13, 14, 12, 14, 15,  // bottom
		16, 17, 18, 16, 18, 19,  // right
		20, 21, 22, 20, 22, 23,  // left
	];

	protected override async Task OnInitializedAsync()
	{
		Console.WriteLine("[OnInitializedAsync] Initializing Home component...");
		
		// Initialize cameras
		_orbitCamera = new PDWebGpuOrbitCamera
		{
			Target = Vector3.Zero,
			Distance = 10.0f,          // Double the distance to make cube appear smaller
			Pitch = 0.3f,              // Look down slightly
			Yaw = 0.5f,                // Rotate a bit
			FieldOfView = MathF.PI / 4f // 45 degrees - standard FOV
		};

		_firstPersonCamera = new PDWebGpuFirstPersonCamera
		{
			Position = new Vector3(0, 0, 10),  // Match distance
			MoveSpeed = 2.0f,
			FieldOfView = MathF.PI / 4f  // Match FOV
		};

		_orthoCamera = new PDWebGpuOrthographicCamera
		{
			Left = -4.0f,    // Wider bounds to match perspective
			Right = 4.0f,
			Bottom = -4.0f,
			Top = 4.0f
		};

		Console.WriteLine("[OnInitializedAsync] Cameras initialized");
		SetCameraMode(CameraMode.Orbit);

		// Check WebGPU support
		_webGpuSupported = await WebGpuService.IsSupportedAsync();
		Console.WriteLine($"[OnInitializedAsync] WebGPU supported: {_webGpuSupported}");
	}

	private async Task OnGpuReady(EventArgs args)
	{
		Console.WriteLine("[OnGpuReady] GPU is ready, initializing rendering resources...");
		
		// GPU is initialized and ready for rendering
		// Get and cache the canvas context ID
		if (_container?.Canvas != null)
		{
			_canvasContextId = _container.Canvas.ContextId;
			Console.WriteLine($"[OnGpuReady] Canvas context ID: {_canvasContextId}");
		}

		await InitializeRenderingResourcesAsync();
		await InvokeAsync(StateHasChanged);
		
		Console.WriteLine("[OnGpuReady] Rendering resources initialized successfully");
	}

	private async Task OnError(PDWebGpuErrorEventArgs args)
	{
		Console.WriteLine($"[OnError] WebGPU error occurred: {args.Message}");
		_deviceError = args.Message;
		await InvokeAsync(StateHasChanged);
	}

	private async Task InitializeRenderingResourcesAsync()
	{
		if (_resourcesInitialized)
		{
			return;
		}

		try
		{
			// Create vertex buffer
			_vertexBuffer = await WebGpuService.CreateBufferAsync(CubeVertexData, BufferType.Vertex, "CubeVertexBuffer");

			// Create index buffer
			_indexBuffer = await WebGpuService.CreateBufferAsync(CubeIndexData, BufferType.Index, "CubeIndexBuffer");

			// Create uniform buffer (64 bytes for 4x4 matrix)
			var uniformData = new byte[64];
			_uniformBuffer = await WebGpuService.CreateBufferAsync(uniformData, BufferType.Uniform, "UniformBuffer");

			// Create shaders
			_vertexShader = await WebGpuService.CreateShaderAsync(ExampleShaders.RotatingCubeVertex, "CubeVertexShader");
			_fragmentShader = await WebGpuService.CreateShaderAsync(ExampleShaders.ColoredFragment, "CubeFragmentShader");

			// Create render pipeline
			var pipelineDescriptor = new RenderPipelineDescriptor
			{
				Vertex = new VertexState
				{
					Shader = _vertexShader,
					EntryPoint = "main",
					Buffers =
					[
						new VertexBufferLayout
						{
							ArrayStride = 24, // 6 floats * 4 bytes = 24 bytes per vertex
							StepMode = "vertex",
							Attributes =
							[
								new VertexAttribute { Format = "float32x3", Offset = 0, ShaderLocation = 0 },  // position
								new VertexAttribute { Format = "float32x3", Offset = 12, ShaderLocation = 1 }, // color
							]
						}
					]
				},
				Fragment = new FragmentState
				{
					Shader = _fragmentShader,
					EntryPoint = "main",
					Targets =
					[
						new ColorTargetState
						{
							Format = "bgra8unorm",
							WriteMask = 0xF
						}
					]
				},
				Primitive = new PrimitiveState
				{
					Topology = "triangle-list",
					CullMode = "none", // Disable culling temporarily
					FrontFace = "ccw"
				}
			};

			_pipeline = await WebGpuService.CreateRenderPipelineAsync(pipelineDescriptor, "CubePipeline");

			// Create bind group using the pipeline's implicit layout
			var bindGroupDescriptor = new BindGroupDescriptor
			{
				PipelineId = _pipeline.ResourceId,  // Get layout from pipeline
				GroupIndex = 0,                      // First bind group
				Entries =
				[
					new BindGroupEntry
					{
						Binding = 0,
						ResourceId = _uniformBuffer.ResourceId,
						ResourceType = "buffer"
					}
				]
			};

			_bindGroup = await WebGpuService.CreateBindGroupAsync(bindGroupDescriptor, "UniformBindGroup");

			_resourcesInitialized = true;
		}
		catch (Exception ex)
		{
			_deviceError = $"Failed to initialize rendering resources: {ex.Message}";
		}
	}

	private async Task OnFrame(PDWebGpuFrameEventArgs args)
	{
		if (!_resourcesInitialized || _activeCamera == null || _bindGroup == null || string.IsNullOrEmpty(_canvasContextId))
		{
			return;
		}

		try
		{
			// Update elapsed time for animated shaders
			_elapsedTime += (float)args.DeltaTime * 0.001f; // Convert ms to seconds

			// Debug: Log camera info for first few frames
			if (args.FrameNumber <= 3 && _activeCamera is PDWebGpuOrbitCamera orbitDebug)
			{
				Console.WriteLine($"=== FRAME {args.FrameNumber} DEBUG ===");
				Console.WriteLine($"Camera Distance: {orbitDebug.Distance}");
				Console.WriteLine($"Camera FOV: {orbitDebug.FieldOfView * 180 / MathF.PI:F1} degrees");
				Console.WriteLine($"Camera Position: {orbitDebug.Position}");
				Console.WriteLine($"Camera Yaw: {orbitDebug.Yaw:F2}, Pitch: {orbitDebug.Pitch:F2}");
				Console.WriteLine($"Camera AspectRatio: {orbitDebug.AspectRatio:F2}");

				// Log projection matrix values
				var proj = orbitDebug.ProjectionMatrix;
				Console.WriteLine($"Projection Matrix:");
				Console.WriteLine($"  M11 (X scale): {proj.M11:F4}");
				Console.WriteLine($"  M22 (Y scale): {proj.M22:F4}");
				Console.WriteLine($"  M33 (Z scale): {proj.M33:F4}");
				Console.WriteLine($"  M34 (Z offset): {proj.M34:F4}");
			}

			// Log every 60th frame to show render loop is running
			if (args.FrameNumber % 60 == 0)
			{
				Console.WriteLine($"[OnFrame] Frame {args.FrameNumber}, FPS: ~{1000.0 / args.DeltaTime:F0}");
			}

			// Update camera
			if (_cameraMode == CameraMode.Orbit && _activeCamera is PDWebGpuOrbitCamera orbitCam)
			{
				orbitCam.Rotate((float)args.DeltaTime * 0.0005f, 0);
			}

			// Determine if we're using the gradient shaders (which use time uniform)
			// Check if the vertex shader or fragment shader contains "Gradient" or "timeData"
			var isGradientShader = (_vertexShader?.WgslCode.Contains("Gradient", StringComparison.OrdinalIgnoreCase) ?? false) ||
			                       (_fragmentShader?.WgslCode.Contains("timeData", StringComparison.OrdinalIgnoreCase) ?? false);

			// Update uniform buffer based on shader type
			if (isGradientShader)
			{
				// Update with time uniform for gradient shader
				var timeData = new[] { _elapsedTime };
				var timeBytes = MemoryMarshal.AsBytes(timeData.AsSpan()).ToArray();
				await _uniformBuffer!.UpdateAsync(timeBytes);
			}
			else
			{
				// Update with MVP matrix for 3D shaders
				var viewMatrix = _activeCamera.ViewMatrix;
				var projMatrix = _activeCamera.ProjectionMatrix;
				
				// Standard 3D graphics: MVP = Projection * View * Model
				// Since we have no model matrix (cube at origin), it's just Projection * View
				var mvpMatrix = projMatrix * viewMatrix;

				// Transpose for WGSL column-major format
				// .NET Matrix4x4 is row-major, WGSL mat4x4 is column-major
				mvpMatrix = Matrix4x4.Transpose(mvpMatrix);

				var matrixBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref mvpMatrix, 1)).ToArray();
				await _uniformBuffer!.UpdateAsync(matrixBytes);
			}

			// Get current texture using cached context ID
			var textureId = await WebGpuService.GetCurrentTextureAsync(_canvasContextId);
			var viewId = await WebGpuService.CreateTextureViewAsync(textureId);

			// Create command encoder
			var encoder = await WebGpuService.CreateCommandEncoderAsync("FrameCommandEncoder");

			// Begin render pass
			var renderPassDesc = new RenderPassDescriptor
			{
				ColorAttachments =
				[
					new ColorAttachment
					{
						ViewId = viewId,
						LoadOp = "clear",
						StoreOp = "store",
						ClearValue = new ClearColor(0.1, 0.1, 0.15, 1.0) // Dark blue background
					}
				]
			};

			var passEncoderId = await encoder.BeginRenderPassAsync(renderPassDesc);

			// Set pipeline and bind group
			await encoder.SetPipelineAsync(passEncoderId, _pipeline!.ResourceId);
			await encoder.SetBindGroupAsync(passEncoderId, 0, _bindGroup!.ResourceId);

			// For gradient shader, draw a fullscreen quad without vertex/index buffers
			if (isGradientShader)
			{
				// Draw 6 vertices for fullscreen quad (2 triangles)
				await encoder.DrawAsync(passEncoderId, 6, 1, 0, 0);
			}
			else
			{
				// Set vertex and index buffers for 3D cube
				await encoder.SetVertexBufferAsync(passEncoderId, 0, _vertexBuffer!.ResourceId);
				await encoder.SetIndexBufferAsync(passEncoderId, _indexBuffer!.ResourceId, "uint16");

				// Draw the cube
				await encoder.DrawIndexedAsync(passEncoderId, CubeIndexData.Length, 1, 0, 0, 0);
			}

			// End render pass
			await encoder.EndRenderPassAsync(passEncoderId);

			// Finish and submit
			var commandBufferId = await encoder.FinishAsync();
			await WebGpuService.SubmitCommandBuffersAsync(commandBufferId);

			// Update performance metrics for draw calls and triangles
			if (_container?.PerformanceDisplay != null)
			{
				_container.PerformanceDisplay.SetDrawCalls(1); // One draw call per frame
				
				if (isGradientShader)
				{
					_container.PerformanceDisplay.SetTriangleCount(2); // Fullscreen quad has 2 triangles
				}
				else
				{
					_container.PerformanceDisplay.SetTriangleCount(12); // Cube has 12 triangles (6 faces * 2 triangles each)
				}
			}
		}
		catch (Exception ex)
		{
			_deviceError = $"Rendering error: {ex.Message}";
			await InvokeAsync(StateHasChanged);
		}
	}

	private async Task CompileShader()
	{
		Console.WriteLine($"[CompileShader] Button clicked, compiling {_currentShaderType} shader...");
		_isCompiling = true;
		_compilationError = null;
		_compilationSuccess = false;
		StateHasChanged();

		try
		{
			// Validate shader first
			var validation = PDWebGpuShader.Validate(_currentShaderCode);
			if (!validation.Success)
			{
				_compilationError = validation.ErrorMessage ?? "Unknown validation error";
				Console.WriteLine($"[CompileShader] Validation failed: {_compilationError}");
				return;
			}

			Console.WriteLine($"[CompileShader] Validation passed, creating shader module...");
			
			// Create the new shader
			var newShader = await WebGpuService.CreateShaderAsync(_currentShaderCode, 
				_currentShaderType == ShaderType.Vertex ? "NewVertexShader" : "NewFragmentShader");

			// Update the appropriate shader reference
			if (_currentShaderType == ShaderType.Vertex)
			{
				Console.WriteLine($"[CompileShader] Replacing vertex shader...");
				if (_vertexShader != null)
				{
					await _vertexShader.DisposeAsync(); // Use async dispose
				}
				_vertexShader = newShader;
			}
			else
			{
				Console.WriteLine($"[CompileShader] Replacing fragment shader...");
				if (_fragmentShader != null)
				{
					await _fragmentShader.DisposeAsync(); // Use async dispose
				}
				_fragmentShader = newShader;
			}

			// Rebuild the render pipeline with the new shaders
			Console.WriteLine($"[CompileShader] Rebuilding render pipeline...");
			await RebuildPipelineAsync();

			Console.WriteLine($"[CompileShader] Pipeline rebuilt successfully!");
			_compilationSuccess = true;
			await Task.Delay(3000); // Show success message for 3 seconds
			_compilationSuccess = false;
		}
		catch (Exception ex)
		{
			_compilationError = ex.Message;
			Console.WriteLine($"[CompileShader] Exception: {ex.Message}");
		}
		finally
		{
			_isCompiling = false;
			StateHasChanged();
		}
	}

	private async Task RebuildPipelineAsync()
	{
		if (_vertexShader == null || _fragmentShader == null)
		{
			Console.WriteLine("[RebuildPipeline] Cannot rebuild - missing vertex or fragment shader");
			return;
		}

		// Dispose old pipeline and bind group using async dispose
		if (_pipeline != null)
		{
			await _pipeline.DisposeAsync();
		}
		if (_bindGroup != null)
		{
			await _bindGroup.DisposeAsync();
		}

		Console.WriteLine("[RebuildPipeline] Creating new render pipeline...");

		// Detect if we're using gradient shaders (which don't use vertex buffers)
		var isGradientShader = (_vertexShader.WgslCode.Contains("Gradient", StringComparison.OrdinalIgnoreCase)) ||
		                       (_fragmentShader.WgslCode.Contains("timeData", StringComparison.OrdinalIgnoreCase));

		// Create pipeline descriptor based on shader type
		RenderPipelineDescriptor pipelineDescriptor;

		if (isGradientShader)
		{
			// Gradient shader: No vertex buffers, no vertex attributes
			pipelineDescriptor = new RenderPipelineDescriptor
			{
				Vertex = new VertexState
				{
					Shader = _vertexShader,
					EntryPoint = "main",
					Buffers = [] // No vertex buffers for fullscreen quad
				},
				Fragment = new FragmentState
				{
					Shader = _fragmentShader,
					EntryPoint = "main",
					Targets =
					[
						new ColorTargetState
						{
							Format = "bgra8unorm",
							WriteMask = 0xF
						}
					]
				},
				Primitive = new PrimitiveState
				{
					Topology = "triangle-list",
					CullMode = "none",
					FrontFace = "ccw"
				}
			};
		}
		else
		{
			// 3D cube shader: Use vertex buffers with position and color attributes
			pipelineDescriptor = new RenderPipelineDescriptor
			{
				Vertex = new VertexState
				{
					Shader = _vertexShader,
					EntryPoint = "main",
					Buffers =
					[
						new VertexBufferLayout
						{
							ArrayStride = 24, // 6 floats * 4 bytes = 24 bytes per vertex
							StepMode = "vertex",
							Attributes =
							[
								new VertexAttribute { Format = "float32x3", Offset = 0, ShaderLocation = 0 },  // position
								new VertexAttribute { Format = "float32x3", Offset = 12, ShaderLocation = 1 }, // color
							]
						}
					]
				},
				Fragment = new FragmentState
				{
					Shader = _fragmentShader,
					EntryPoint = "main",
					Targets =
					[
						new ColorTargetState
						{
							Format = "bgra8unorm",
							WriteMask = 0xF
						}
					]
				},
				Primitive = new PrimitiveState
				{
					Topology = "triangle-list",
					CullMode = "none",
					FrontFace = "ccw"
				}
			};
		}

		_pipeline = await WebGpuService.CreateRenderPipelineAsync(pipelineDescriptor, "DynamicPipeline");

		// Recreate bind group with new pipeline
		var bindGroupDescriptor = new BindGroupDescriptor
		{
			PipelineId = _pipeline.ResourceId,
			GroupIndex = 0,
			Entries =
			[
				new BindGroupEntry
				{
					Binding = 0,
					ResourceId = _uniformBuffer!.ResourceId,
					ResourceType = "buffer"
				}
			]
		};

		_bindGroup = await WebGpuService.CreateBindGroupAsync(bindGroupDescriptor, "DynamicBindGroup");
		
		Console.WriteLine($"[RebuildPipeline] Pipeline rebuilt with resource ID: {_pipeline.ResourceId} (IsGradient: {isGradientShader})");
	}
	
	private void TogglePerformanceMetrics()
	{
		_showPerformance = !_showPerformance;
		Console.WriteLine($"[TogglePerformanceMetrics] Performance metrics now {(_showPerformance ? "visible" : "hidden")}");
	}

	private void LoadExampleShader(ChangeEventArgs args)
	{
		var selectedExample = args.Value?.ToString();
		Console.WriteLine($"[LoadExampleShader] Dropdown changed to: {selectedExample}");
		if (!string.IsNullOrEmpty(selectedExample) && _exampleShaders.TryGetValue(selectedExample, out var value))
		{
			_currentShaderCode = value;
			
			// Auto-detect shader type based on shader name
			if (selectedExample.Contains("Vertex", StringComparison.OrdinalIgnoreCase))
			{
				_currentShaderType = ShaderType.Vertex;
				Console.WriteLine($"[LoadExampleShader] Detected vertex shader");
				
				// Auto-load compatible fragment shader
				var compatibleFragment = selectedExample switch
				{
					"Simple Triangle (Vertex)" => "Simple Color (Fragment)",
					"Rotating Cube (Vertex)" => "Colored (Fragment)",
					"Gradient (Vertex)" => "Animated Gradient (Fragment)",
					"Phong Lighting (Vertex)" => "Phong Lighting (Fragment)",
					_ => null
				};
				
				if (compatibleFragment != null)
				{
					Console.WriteLine($"[LoadExampleShader] Auto-compiling compatible fragment shader: {compatibleFragment}");
					_ = Task.Run(async () =>
					{
						try
						{
							// Create the VERTEX shader first (from the current code that was just loaded)
							var vertexCode = value;
							var newVertexShader = await WebGpuService.CreateShaderAsync(vertexCode, "AutoLoadedVertexShader");
							
							// Then create the FRAGMENT shader
							var fragmentCode = _exampleShaders[compatibleFragment];
							var newFragmentShader = await WebGpuService.CreateShaderAsync(fragmentCode, "AutoLoadedFragmentShader");
							
							// Dispose old shaders
							if (_vertexShader != null)
							{
								await _vertexShader.DisposeAsync();
							}
							if (_fragmentShader != null)
							{
								await _fragmentShader.DisposeAsync();
							}
							
							// Assign BOTH shaders BEFORE rebuilding
							_vertexShader = newVertexShader;
							_fragmentShader = newFragmentShader;
							
							// Now rebuild pipeline after both shaders are assigned
							await RebuildPipelineAsync();
							Console.WriteLine($"[LoadExampleShader] Auto-loaded both shaders successfully");
						}
						catch (Exception ex)
						{
							Console.WriteLine($"[LoadExampleShader] Failed to auto-load shaders: {ex.Message}");
						}
					});
				}
			}
			else if (selectedExample.Contains("Fragment", StringComparison.OrdinalIgnoreCase))
			{
				_currentShaderType = ShaderType.Fragment;
				Console.WriteLine($"[LoadExampleShader] Detected fragment shader");
				
				// Auto-load compatible vertex shader
				var compatibleVertex = selectedExample switch
				{
					"Simple Color (Fragment)" => "Simple Triangle (Vertex)",
					"Colored (Fragment)" => "Rotating Cube (Vertex)",
					"Animated Gradient (Fragment)" => "Gradient (Vertex)",
					"Phong Lighting (Fragment)" => "Phong Lighting (Vertex)",
					_ => null
				};
				
				if (compatibleVertex != null)
				{
					Console.WriteLine($"[LoadExampleShader] Auto-compiling compatible vertex shader: {compatibleVertex}");
					_ = Task.Run(async () =>
					{
						try
						{
							// Create the FRAGMENT shader first (from the current code that was just loaded)
							var fragmentCode = value;
							var newFragmentShader = await WebGpuService.CreateShaderAsync(fragmentCode, "AutoLoadedFragmentShader");
							
							// Then create the VERTEX shader
							var vertexCode = _exampleShaders[compatibleVertex];
							var newVertexShader = await WebGpuService.CreateShaderAsync(vertexCode, "AutoLoadedVertexShader");
							
							// Dispose old shaders
							if (_fragmentShader != null)
							{
								await _fragmentShader.DisposeAsync();
							}
							if (_vertexShader != null)
							{
								await _vertexShader.DisposeAsync();
							}
							
							// Assign BOTH shaders BEFORE rebuilding
							_fragmentShader = newFragmentShader;
							_vertexShader = newVertexShader;
							
							// Now rebuild pipeline after both shaders are assigned
							await RebuildPipelineAsync();
							Console.WriteLine($"[LoadExampleShader] Auto-loaded both shaders successfully");
						}
						catch (Exception ex)
						{
							Console.WriteLine($"[LoadExampleShader] Failed to auto-load shaders: {ex.Message}");
						}
					});
				}
			}
			
			Console.WriteLine($"[LoadExampleShader] Loaded {_currentShaderType} shader, length: {value.Length} characters");
		}
	}

	private void SetCameraMode(CameraMode mode)
	{
		Console.WriteLine($"[SetCameraMode] Switching camera mode to: {mode}");
		_cameraMode = mode;
		_activeCamera = mode switch
		{
			CameraMode.Orbit => _orbitCamera,
			CameraMode.FirstPerson => _firstPersonCamera,
			CameraMode.Orthographic => _orthoCamera,
			_ => _orbitCamera
		};

		// Update aspect ratio for all cameras
		var aspectRatio = 16f / 9f; // Default aspect ratio
		if (_orbitCamera != null) _orbitCamera.AspectRatio = aspectRatio;
		if (_firstPersonCamera != null) _firstPersonCamera.AspectRatio = aspectRatio;
		if (_orthoCamera != null)
		{
			_orthoCamera.Left = -2.0f * aspectRatio;
			_orthoCamera.Right = 2.0f * aspectRatio;
		}
		
		Console.WriteLine($"[SetCameraMode] Camera switched successfully");
	}

	public void Dispose()
	{
		_vertexBuffer?.Dispose();
		_indexBuffer?.Dispose();
		_uniformBuffer?.Dispose();
		_vertexShader?.Dispose();
		_fragmentShader?.Dispose();
		_pipeline?.Dispose();
		_bindGroup?.Dispose();
	}
}

/// <summary>
/// Camera modes for the demo.
/// </summary>
public enum CameraMode
{
	Orbit,
	FirstPerson,
	Orthographic
}

/// <summary>
/// Shader types for hot-reloading.
/// </summary>
public enum ShaderType
{
	Vertex,
	Fragment
}

using Microsoft.AspNetCore.Components;
using PanoramicData.Blazor.WebGpu.Services;

namespace PanoramicData.Blazor.WebGpu.Components;

/// <summary>
/// Main container component that provides layered rendering with WebGPU canvas background and HTML foreground.
/// </summary>
public partial class PDWebGpuContainer : PDWebGpuComponentBase
{
	private PDWebGpuCanvas? _canvas;
	private bool _isRunning;
	private System.Threading.Timer? _renderTimer;
	private long _frameNumber;
	private double _lastFrameTime;
	private double _totalTime;

	[Inject]
	private IPDWebGpuService WebGpuService { get; set; } = default!;

	/// <summary>
	/// Gets or sets the canvas ID.
	/// </summary>
	[Parameter]
	public string CanvasId { get; set; } = $"pdwebgpu-canvas-{Guid.NewGuid():N}";

	/// <summary>
	/// Gets or sets the child content to render in the foreground layer.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	/// <summary>
	/// Gets or sets the frame rate mode.
	/// </summary>
	[Parameter]
	public FrameRateMode FrameRateMode { get; set; } = FrameRateMode.Variable;

	/// <summary>
	/// Gets or sets the target frame rate (only used when FrameRateMode is Fixed).
	/// </summary>
	[Parameter]
	public int TargetFrameRate { get; set; } = 60;

	/// <summary>
	/// Gets or sets whether to pause rendering when the tab/window is inactive.
	/// </summary>
	[Parameter]
	public bool PauseWhenInactive { get; set; } = true;

	/// <summary>
	/// Gets or sets whether the render loop should automatically start.
	/// </summary>
	[Parameter]
	public bool AutoStart { get; set; } = true;

	/// <inheritdoc/>
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender && AutoStart)
		{
			await StartRenderLoopAsync();
		}
	}

	/// <summary>
	/// Starts the render loop.
	/// </summary>
	public async Task StartRenderLoopAsync()
	{
		if (_isRunning)
		{
			return;
		}

		_isRunning = true;
		_frameNumber = 0;
		_lastFrameTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		_totalTime = 0;

		if (FrameRateMode == FrameRateMode.Fixed)
		{
			var intervalMs = 1000.0 / TargetFrameRate;
			_renderTimer = new System.Threading.Timer(async _ => await RenderFrameAsync(), null, 0, (int)intervalMs);
		}
		else
		{
			// Variable frame rate - use requestAnimationFrame equivalent
			_ = Task.Run(async () =>
			{
				while (_isRunning)
				{
					await RenderFrameAsync();
					await Task.Delay(1); // Yield to prevent CPU saturation
				}
			});
		}

		await Task.CompletedTask;
	}

	/// <summary>
	/// Stops the render loop.
	/// </summary>
	public void StopRenderLoop()
	{
		_isRunning = false;
		_renderTimer?.Dispose();
		_renderTimer = null;
	}

	private async Task RenderFrameAsync()
	{
		if (!_isRunning)
		{
			return;
		}

		try
		{
			var currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			var deltaTime = currentTime - _lastFrameTime;
			_totalTime += deltaTime;
			_lastFrameTime = currentTime;

			var frameArgs = new PDWebGpuFrameEventArgs
			{
				DeltaTime = deltaTime,
				TotalTime = _totalTime,
				FrameNumber = ++_frameNumber
			};

			await RaiseFrameAsync(frameArgs);
		}
		catch (Exception ex)
		{
			await RaiseErrorAsync(new PDWebGpuErrorEventArgs(ex));
		}
	}

	private async Task HandleGpuReadyAsync(EventArgs args)
	{
		await RaiseGpuReadyAsync();
	}

	private async Task HandleErrorAsync(PDWebGpuErrorEventArgs args)
	{
		await RaiseErrorAsync(args);
	}

	private async Task HandleResizeAsync(PDWebGpuResizeEventArgs args)
	{
		await RaiseResizeAsync(args);
	}

	/// <inheritdoc/>
	protected override async ValueTask DisposeAsyncCore()
	{
		StopRenderLoop();
		await base.DisposeAsyncCore();
	}
}

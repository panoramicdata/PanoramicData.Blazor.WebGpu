using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PanoramicData.Blazor.WebGpu.Services;

namespace PanoramicData.Blazor.WebGpu.Components;

/// <summary>
/// WebGPU canvas component that manages the canvas element and WebGPU context.
/// </summary>
public partial class PDWebGpuCanvas : PDWebGpuComponentBase
{
	private ElementReference _canvasRef;
	private string? _contextId;
	private int _width;
	private int _height;
	private bool _isInitialized;

	[Inject]
	private IPDWebGpuService WebGpuService { get; set; } = default!;

	[Inject]
	private IJSRuntime JSRuntime { get; set; } = default!;

	/// <summary>
	/// Gets or sets the canvas ID.
	/// </summary>
	[Parameter, EditorRequired]
	public string CanvasId { get; set; } = string.Empty;

	/// <summary>
	/// Gets the canvas context ID.
	/// </summary>
	public string? ContextId => _contextId;

	/// <summary>
	/// Gets the canvas width in pixels.
	/// </summary>
	public int Width => _width;

	/// <summary>
	/// Gets the canvas height in pixels.
	/// </summary>
	public int Height => _height;

	/// <summary>
	/// Gets whether the canvas is initialized.
	/// </summary>
	public bool IsInitialized => _isInitialized;

	/// <inheritdoc/>
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await InitializeCanvasAsync();
		}
	}

	private async Task InitializeCanvasAsync()
	{
		try
		{
			// Ensure WebGPU service is initialized
			await WebGpuService.EnsureInitializedAsync();

			// Get the canvas context
			_contextId = await WebGpuService.GetCanvasContextAsync(CanvasId);

			// Configure the canvas context
			await WebGpuService.ConfigureCanvasContextAsync(_contextId);

			// Get canvas size
			await UpdateCanvasSizeAsync();

			_isInitialized = true;

			// Raise GPU ready event
			await RaiseGpuReadyAsync();
		}
		catch (Exception ex)
		{
			await RaiseErrorAsync(new PDWebGpuErrorEventArgs(ex));
		}
	}

	private async Task UpdateCanvasSizeAsync()
	{
		try
		{
			// Get canvas bounding rect from JavaScript
			var size = await JSRuntime.InvokeAsync<CanvasSize>("eval",
				$"(function() {{ var c = document.getElementById('{CanvasId}'); return {{ width: c.clientWidth, height: c.clientHeight }}; }})()");

			var oldWidth = _width;
			var oldHeight = _height;

			_width = size.Width;
			_height = size.Height;

			if (oldWidth != _width || oldHeight != _height)
			{
				await RaiseResizeAsync(new PDWebGpuResizeEventArgs
				{
					Width = _width,
					Height = _height,
					OldWidth = oldWidth,
					OldHeight = oldHeight
				});
			}
		}
		catch (Exception ex)
		{
			await RaiseErrorAsync(new PDWebGpuErrorEventArgs(ex));
		}
	}

	/// <summary>
	/// Resizes the canvas to the specified dimensions.
	/// </summary>
	/// <param name="width">The new width in pixels.</param>
	/// <param name="height">The new height in pixels.</param>
	public async Task ResizeAsync(int width, int height)
	{
		var oldWidth = _width;
		var oldHeight = _height;

		_width = width;
		_height = height;

		await RaiseResizeAsync(new PDWebGpuResizeEventArgs
		{
			Width = width,
			Height = height,
			OldWidth = oldWidth,
			OldHeight = oldHeight
		});
	}

	private class CanvasSize
	{
		public int Width { get; set; }
		public int Height { get; set; }
	}
}

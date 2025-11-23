using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PanoramicData.Blazor.WebGpu.Services;

namespace PanoramicData.Blazor.WebGpu;

/// <summary>
/// Abstract base class for all PDWebGpu components.
/// Provides common functionality for component lifecycle, events, and input handling.
/// </summary>
public abstract class PDWebGpuComponentBase : ComponentBase, IDisposable, IAsyncDisposable
{
	private bool _disposed;

	/// <summary>
	/// Gets or sets the CSS class for the component.
	/// </summary>
	[Parameter]
	public string? CssClass { get; set; }

	/// <summary>
	/// Gets or sets additional HTML attributes for the component.
	/// </summary>
	[Parameter(CaptureUnmatchedValues = true)]
	public Dictionary<string, object>? AdditionalAttributes { get; set; }

	/// <summary>
	/// Event callback invoked when a frame should be rendered.
	/// </summary>
	[Parameter]
	public EventCallback<PDWebGpuFrameEventArgs> OnFrame { get; set; }

	/// <summary>
	/// Event callback invoked when the component is resized.
	/// </summary>
	[Parameter]
	public EventCallback<PDWebGpuResizeEventArgs> OnResize { get; set; }

	/// <summary>
	/// Event callback invoked when the GPU is ready.
	/// </summary>
	[Parameter]
	public EventCallback<EventArgs> OnGpuReady { get; set; }

	/// <summary>
	/// Event callback invoked when an error occurs.
	/// </summary>
	[Parameter]
	public EventCallback<PDWebGpuErrorEventArgs> OnError { get; set; }

	/// <summary>
	/// Gets or sets whether mouse events should be handled.
	/// </summary>
	[Parameter]
	public bool HandleMouseEvents { get; set; }

	/// <summary>
	/// Event callback for mouse down events.
	/// </summary>
	[Parameter]
	public EventCallback<MouseEventArgs> OnMouseDown { get; set; }

	/// <summary>
	/// Event callback for mouse up events.
	/// </summary>
	[Parameter]
	public EventCallback<MouseEventArgs> OnMouseUp { get; set; }

	/// <summary>
	/// Event callback for mouse move events.
	/// </summary>
	[Parameter]
	public EventCallback<MouseEventArgs> OnMouseMove { get; set; }

	/// <summary>
	/// Event callback for mouse wheel events.
	/// </summary>
	[Parameter]
	public EventCallback<WheelEventArgs> OnMouseWheel { get; set; }

	/// <summary>
	/// Gets or sets whether touch events should be handled.
	/// </summary>
	[Parameter]
	public bool HandleTouchEvents { get; set; }

	/// <summary>
	/// Event callback for touch start events.
	/// </summary>
	[Parameter]
	public EventCallback<TouchEventArgs> OnTouchStart { get; set; }

	/// <summary>
	/// Event callback for touch move events.
	/// </summary>
	[Parameter]
	public EventCallback<TouchEventArgs> OnTouchMove { get; set; }

	/// <summary>
	/// Event callback for touch end events.
	/// </summary>
	[Parameter]
	public EventCallback<TouchEventArgs> OnTouchEnd { get; set; }

	/// <summary>
	/// Raises the OnFrame event.
	/// </summary>
	/// <param name="args">Frame event arguments.</param>
	protected virtual async Task RaiseFrameAsync(PDWebGpuFrameEventArgs args)
	{
		if (OnFrame.HasDelegate)
		{
			await OnFrame.InvokeAsync(args);
		}
	}

	/// <summary>
	/// Raises the OnResize event.
	/// </summary>
	/// <param name="args">Resize event arguments.</param>
	protected virtual async Task RaiseResizeAsync(PDWebGpuResizeEventArgs args)
	{
		if (OnResize.HasDelegate)
		{
			await OnResize.InvokeAsync(args);
		}
	}

	/// <summary>
	/// Raises the OnGpuReady event.
	/// </summary>
	protected virtual async Task RaiseGpuReadyAsync()
	{
		if (OnGpuReady.HasDelegate)
		{
			await OnGpuReady.InvokeAsync(EventArgs.Empty);
		}
	}

	/// <summary>
	/// Raises the OnError event.
	/// </summary>
	/// <param name="args">Error event arguments.</param>
	protected virtual async Task RaiseErrorAsync(PDWebGpuErrorEventArgs args)
	{
		if (OnError.HasDelegate)
		{
			await OnError.InvokeAsync(args);
		}
	}

	/// <summary>
	/// Handles mouse down events.
	/// </summary>
	protected virtual async Task HandleMouseDownAsync(MouseEventArgs e)
	{
		if (HandleMouseEvents && OnMouseDown.HasDelegate)
		{
			await OnMouseDown.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Handles mouse up events.
	/// </summary>
	protected virtual async Task HandleMouseUpAsync(MouseEventArgs e)
	{
		if (HandleMouseEvents && OnMouseUp.HasDelegate)
		{
			await OnMouseUp.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Handles mouse move events.
	/// </summary>
	protected virtual async Task HandleMouseMoveAsync(MouseEventArgs e)
	{
		if (HandleMouseEvents && OnMouseMove.HasDelegate)
		{
			await OnMouseMove.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Handles mouse wheel events.
	/// </summary>
	protected virtual async Task HandleMouseWheelAsync(WheelEventArgs e)
	{
		if (HandleMouseEvents && OnMouseWheel.HasDelegate)
		{
			await OnMouseWheel.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Handles touch start events.
	/// </summary>
	protected virtual async Task HandleTouchStartAsync(TouchEventArgs e)
	{
		if (HandleTouchEvents && OnTouchStart.HasDelegate)
		{
			await OnTouchStart.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Handles touch move events.
	/// </summary>
	protected virtual async Task HandleTouchMoveAsync(TouchEventArgs e)
	{
		if (HandleTouchEvents && OnTouchMove.HasDelegate)
		{
			await OnTouchMove.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Handles touch end events.
	/// </summary>
	protected virtual async Task HandleTouchEndAsync(TouchEventArgs e)
	{
		if (HandleTouchEvents && OnTouchEnd.HasDelegate)
		{
			await OnTouchEnd.InvokeAsync(e);
		}
	}

	/// <summary>
	/// Disposes the component synchronously.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes the component asynchronously.
	/// </summary>
	public async ValueTask DisposeAsync()
	{
		await DisposeAsyncCore();
		Dispose(false);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Disposes managed and unmanaged resources.
	/// </summary>
	/// <param name="disposing">True if called from Dispose(), false if called from finalizer.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				// Dispose managed resources
			}

			_disposed = true;
		}
	}

	/// <summary>
	/// Override this method to dispose resources asynchronously.
	/// </summary>
	protected virtual ValueTask DisposeAsyncCore()
	{
		return ValueTask.CompletedTask;
	}
}

/// <summary>
/// Event args for frame rendering events.
/// </summary>
public class PDWebGpuFrameEventArgs : EventArgs
{
	/// <summary>
	/// Gets or sets the time elapsed since the last frame in milliseconds.
	/// </summary>
	public double DeltaTime { get; set; }

	/// <summary>
	/// Gets or sets the total time since application start in milliseconds.
	/// </summary>
	public double TotalTime { get; set; }

	/// <summary>
	/// Gets or sets the current frame number.
	/// </summary>
	public long FrameNumber { get; set; }
}

/// <summary>
/// Event args for resize events.
/// </summary>
public class PDWebGpuResizeEventArgs : EventArgs
{
	/// <summary>
	/// Gets or sets the new width in pixels.
	/// </summary>
	public int Width { get; set; }

	/// <summary>
	/// Gets or sets the new height in pixels.
	/// </summary>
	public int Height { get; set; }

	/// <summary>
	/// Gets or sets the previous width in pixels.
	/// </summary>
	public int OldWidth { get; set; }

	/// <summary>
	/// Gets or sets the previous height in pixels.
	/// </summary>
	public int OldHeight { get; set; }
}

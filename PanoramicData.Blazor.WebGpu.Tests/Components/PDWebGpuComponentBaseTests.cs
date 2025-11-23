using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PanoramicData.Blazor.WebGpu.Components;
using PanoramicData.Blazor.WebGpu.Services;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Components;

/// <summary>
/// Tests for PDWebGpuComponentBase.
/// </summary>
public class PDWebGpuComponentBaseTests : TestBase
{
	// Create a test implementation of the abstract base class
	private class TestComponent : PDWebGpuComponentBase
	{
		public Task TestRaiseFrameAsync(PDWebGpuFrameEventArgs args) => RaiseFrameAsync(args);
		public Task TestRaiseResizeAsync(PDWebGpuResizeEventArgs args) => RaiseResizeAsync(args);
		public Task TestRaiseGpuReadyAsync() => RaiseGpuReadyAsync();
		public Task TestRaiseErrorAsync(PDWebGpuErrorEventArgs args) => RaiseErrorAsync(args);
		public Task TestHandleMouseDownAsync(MouseEventArgs e) => HandleMouseDownAsync(e);
		public Task TestHandleMouseMoveAsync(MouseEventArgs e) => HandleMouseMoveAsync(e);
		public Task TestHandleTouchStartAsync(TouchEventArgs e) => HandleTouchStartAsync(e);
	}

	[Fact]
	public void Component_Should_BeCreatable()
	{
		// Act
		var component = new TestComponent();

		// Assert
		component.Should().NotBeNull();
		component.CssClass.Should().BeNull();
		component.AdditionalAttributes.Should().BeNull();
	}

	[Fact]
	public void Component_Should_AcceptCssClass()
	{
		// Arrange
		var component = new TestComponent();

		// Act
		component.CssClass = "test-class";

		// Assert
		component.CssClass.Should().Be("test-class");
	}

	[Fact]
	public void Component_Should_AcceptAdditionalAttributes()
	{
		// Arrange
		var component = new TestComponent();
		var attributes = new Dictionary<string, object>
		{
			["data-test"] = "value"
		};

		// Act
		component.AdditionalAttributes = attributes;

		// Assert
		component.AdditionalAttributes.Should().BeSameAs(attributes);
	}

	[Fact]
	public async Task RaiseFrameAsync_Should_InvokeCallback()
	{
		// Arrange
		var component = new TestComponent();
		PDWebGpuFrameEventArgs? receivedArgs = null;

		component.OnFrame = EventCallback.Factory.Create<PDWebGpuFrameEventArgs>(
			this,
			args => receivedArgs = args);

		var frameArgs = new PDWebGpuFrameEventArgs
		{
			DeltaTime = 16.7,
			TotalTime = 1000,
			FrameNumber = 60
		};

		// Act
		await component.TestRaiseFrameAsync(frameArgs);

		// Assert
		receivedArgs.Should().NotBeNull();
		receivedArgs!.DeltaTime.Should().Be(16.7);
		receivedArgs.TotalTime.Should().Be(1000);
		receivedArgs.FrameNumber.Should().Be(60);
	}

	[Fact]
	public async Task RaiseResizeAsync_Should_InvokeCallback()
	{
		// Arrange
		var component = new TestComponent();
		PDWebGpuResizeEventArgs? receivedArgs = null;

		component.OnResize = EventCallback.Factory.Create<PDWebGpuResizeEventArgs>(
			this,
			args => receivedArgs = args);

		var resizeArgs = new PDWebGpuResizeEventArgs
		{
			Width = 1920,
			Height = 1080,
			OldWidth = 1280,
			OldHeight = 720
		};

		// Act
		await component.TestRaiseResizeAsync(resizeArgs);

		// Assert
		receivedArgs.Should().NotBeNull();
		receivedArgs!.Width.Should().Be(1920);
		receivedArgs.Height.Should().Be(1080);
		receivedArgs.OldWidth.Should().Be(1280);
		receivedArgs.OldHeight.Should().Be(720);
	}

	[Fact]
	public async Task RaiseGpuReadyAsync_Should_InvokeCallback()
	{
		// Arrange
		var component = new TestComponent();
		var callbackInvoked = false;

		component.OnGpuReady = EventCallback.Factory.Create<EventArgs>(
			this,
			_ => callbackInvoked = true);

		// Act
		await component.TestRaiseGpuReadyAsync();

		// Assert
		callbackInvoked.Should().BeTrue();
	}

	[Fact]
	public async Task RaiseErrorAsync_Should_InvokeCallback()
	{
		// Arrange
		var component = new TestComponent();
		PDWebGpuErrorEventArgs? receivedArgs = null;

		component.OnError = EventCallback.Factory.Create<PDWebGpuErrorEventArgs>(
			this,
			args => receivedArgs = args);

		var exception = new PDWebGpuException("Test error");
		var errorArgs = new PDWebGpuErrorEventArgs(exception);

		// Act
		await component.TestRaiseErrorAsync(errorArgs);

		// Assert
		receivedArgs.Should().NotBeNull();
		receivedArgs!.Exception.Should().Be(exception);
		receivedArgs.Message.Should().Be("Test error");
	}

	[Fact]
	public async Task HandleMouseDownAsync_Should_InvokeCallback_When_HandleMouseEventsEnabled()
	{
		// Arrange
		var component = new TestComponent();
		component.HandleMouseEvents = true;
		MouseEventArgs? receivedArgs = null;

		component.OnMouseDown = EventCallback.Factory.Create<MouseEventArgs>(
			this,
			args => receivedArgs = args);

		var mouseArgs = new MouseEventArgs { Button = 0, ClientX = 100, ClientY = 200 };

		// Act
		await component.TestHandleMouseDownAsync(mouseArgs);

		// Assert
		receivedArgs.Should().NotBeNull();
		receivedArgs!.ClientX.Should().Be(100);
		receivedArgs.ClientY.Should().Be(200);
	}

	[Fact]
	public async Task HandleMouseDownAsync_Should_NotInvokeCallback_When_HandleMouseEventsDisabled()
	{
		// Arrange
		var component = new TestComponent();
		component.HandleMouseEvents = false;
		var callbackInvoked = false;

		component.OnMouseDown = EventCallback.Factory.Create<MouseEventArgs>(
			this,
			_ => callbackInvoked = true);

		var mouseArgs = new MouseEventArgs { Button = 0 };

		// Act
		await component.TestHandleMouseDownAsync(mouseArgs);

		// Assert
		callbackInvoked.Should().BeFalse();
	}

	[Fact]
	public async Task HandleTouchStartAsync_Should_InvokeCallback_When_HandleTouchEventsEnabled()
	{
		// Arrange
		var component = new TestComponent();
		component.HandleTouchEvents = true;
		TouchEventArgs? receivedArgs = null;

		component.OnTouchStart = EventCallback.Factory.Create<TouchEventArgs>(
			this,
			args => receivedArgs = args);

		var touchArgs = new TouchEventArgs { };

		// Act
		await component.TestHandleTouchStartAsync(touchArgs);

		// Assert
		receivedArgs.Should().NotBeNull();
	}

	[Fact]
	public void Dispose_Should_SetDisposedFlag()
	{
		// Arrange
		var component = new TestComponent();

		// Act
		component.Dispose();

		// Assert - second dispose should not throw
		var act = () => component.Dispose();
		act.Should().NotThrow();
	}

	[Fact]
	public async Task DisposeAsync_Should_SetDisposedFlag()
	{
		// Arrange
		var component = new TestComponent();

		// Act
		await component.DisposeAsync();

		// Assert - second dispose should not throw
		var act = async () => await component.DisposeAsync();
		await act.Should().NotThrowAsync();
	}

	[Fact]
	public void PDWebGpuFrameEventArgs_Should_HaveProperties()
	{
		// Act
		var args = new PDWebGpuFrameEventArgs
		{
			DeltaTime = 16.7,
			TotalTime = 5000,
			FrameNumber = 300
		};

		// Assert
		args.DeltaTime.Should().Be(16.7);
		args.TotalTime.Should().Be(5000);
		args.FrameNumber.Should().Be(300);
	}

	[Fact]
	public void PDWebGpuResizeEventArgs_Should_HaveProperties()
	{
		// Act
		var args = new PDWebGpuResizeEventArgs
		{
			Width = 800,
			Height = 600,
			OldWidth = 640,
			OldHeight = 480
		};

		// Assert
		args.Width.Should().Be(800);
		args.Height.Should().Be(600);
		args.OldWidth.Should().Be(640);
		args.OldHeight.Should().Be(480);
	}
}

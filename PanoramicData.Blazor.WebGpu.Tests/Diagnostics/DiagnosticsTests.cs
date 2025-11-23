using PanoramicData.Blazor.WebGpu.Diagnostics;
using PanoramicData.Blazor.WebGpu.Interop;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Diagnostics;

/// <summary>
/// Tests for error handling and diagnostics functionality.
/// </summary>
public class DiagnosticsTests : TestBase
{
	#region WebGpuCompatibilityInfo Tests

	[Fact]
	public void CompatibilityInfo_Should_StoreProperties()
	{
		// Act
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = true,
			UserAgent = "Mozilla/5.0...",
			Vendor = "Google Inc.",
			Platform = "Win32",
			BrowserName = "Chrome",
			BrowserVersion = "120",
			SupportsWithFlags = true
		};

		// Assert
		info.IsSupported.Should().BeTrue();
		info.UserAgent.Should().Be("Mozilla/5.0...");
		info.Vendor.Should().Be("Google Inc.");
		info.Platform.Should().Be("Win32");
		info.BrowserName.Should().Be("Chrome");
		info.BrowserVersion.Should().Be("120");
		info.SupportsWithFlags.Should().BeTrue();
	}

	[Fact]
	public void CompatibilityInfo_Should_DefaultToEmptyStrings()
	{
		// Act
		var info = new WebGpuCompatibilityInfo();

		// Assert
		info.UserAgent.Should().BeEmpty();
		info.Vendor.Should().BeEmpty();
		info.Platform.Should().BeEmpty();
		info.IsSupported.Should().BeFalse();
	}

	#endregion

	#region PDWebGpuNotSupportedException Tests

	[Fact]
	public void NotSupportedException_Should_HaveDefaultMessage()
	{
		// Act
		var ex = new PDWebGpuNotSupportedException();

		// Assert
		ex.Message.Should().Contain("not supported");
	}

	[Fact]
	public void NotSupportedException_Should_StoreCompatibilityInfo()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "Firefox",
			BrowserVersion = "100",
			ErrorMessage = "WebGPU not available"
		};

		// Act
		var ex = new PDWebGpuNotSupportedException(info);

		// Assert
		ex.CompatibilityInfo.Should().NotBeNull();
		ex.CompatibilityInfo!.BrowserName.Should().Be("Firefox");
		ex.Message.Should().Contain("not available");
	}

	[Fact]
	public void NotSupportedException_Should_ProvideSuggestion()
	{
		// Arrange
		var ex = new PDWebGpuNotSupportedException();

		// Act
		var suggestion = PDWebGpuNotSupportedException.Suggestion;

		// Assert
		suggestion.Should().Contain("Chrome");
		suggestion.Should().Contain("Edge");
		suggestion.Should().Contain("113");
	}

	[Fact]
	public void NotSupportedException_Should_ProvideDetailedMessage()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			UserAgent = "Mozilla/5.0 Firefox/100.0",
			Vendor = "Mozilla",
			Platform = "Win32",
			BrowserName = "Firefox",
			BrowserVersion = "100"
		};
		var ex = new PDWebGpuNotSupportedException(info);

		// Act
		var detailedMessage = ex.GetDetailedMessage();

		// Assert
		detailedMessage.Should().Contain("Firefox");
		detailedMessage.Should().Contain("100");
		detailedMessage.Should().Contain("Win32");
		detailedMessage.Should().Contain("Chrome");
	}

	#endregion

	#region PDWebGpuDeviceException Tests

	[Fact]
	public void DeviceException_Should_HaveRecoverySuggestion()
	{
		// Act
		var ex = new PDWebGpuDeviceException("Device lost");

		// Assert
		PDWebGpuDeviceException.RecoverySuggestion.Should().Contain("refresh");
		PDWebGpuDeviceException.RecoverySuggestion.Should().Contain("GPU drivers");
	}

	[Fact]
	public void DeviceException_Should_StoreMessage()
	{
		// Act
		var ex = new PDWebGpuDeviceException("Custom error message");

		// Assert
		ex.Message.Should().Be("Custom error message");
	}

	[Fact]
	public void DeviceException_Should_StoreInnerException()
	{
		// Arrange
		var innerEx = new InvalidOperationException("Inner error");

		// Act
		var ex = new PDWebGpuDeviceException("Outer error", innerEx);

		// Assert
		ex.InnerException.Should().Be(innerEx);
		ex.Message.Should().Be("Outer error");
	}

	#endregion

	#region DiagnosticHelper Tests

	[Fact]
	public void DiagnosticHelper_Should_FormatNotSupportedMessage()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "Firefox",
			BrowserVersion = "100",
			SupportsWithFlags = true
		};

		// Act
		var message = DiagnosticHelper.GetNotSupportedMessage(info);

		// Assert
		message.Should().Contain("Firefox");
		message.Should().Contain("100");
		message.Should().Contain("Chrome 113");
		message.Should().Contain("about:config");
	}

	[Fact]
	public void DiagnosticHelper_Should_FormatDeviceErrorMessage()
	{
		// Arrange
		var ex = new Exception("Device initialization failed");

		// Act
		var message = DiagnosticHelper.GetDeviceErrorMessage(ex);

		// Assert
		message.Should().Contain("Device Error");
		message.Should().Contain("initialization failed");
		message.Should().Contain("Refresh the page");
		message.Should().Contain("GPU drivers");
	}

	[Fact]
	public void DiagnosticHelper_Should_FormatShaderErrorMessage()
	{
		// Arrange
		var shaderCode = "@vertex fn main() -> vec4<f32> {\n  return vec4<f32>(1.0);\n}";
		var ex = new PDWebGpuShaderCompilationException("Shader compilation failed", shaderCode, new Exception("Line 2: Syntax error"));

		// Act
		var message = DiagnosticHelper.GetShaderErrorMessage(ex);

		// Assert
		message.Should().Contain("Shader Compilation Error");
		message.Should().Contain("Syntax error");
	}

	[Fact]
	public void DiagnosticHelper_Should_FormatNumbers()
	{
		// Act & Assert
		DiagnosticHelper.FormatNumber(999).Should().Be("999");
		DiagnosticHelper.FormatNumber(1_500).Should().Be("1.5K");
		DiagnosticHelper.FormatNumber(1_500_000).Should().Be("1.5M");
		DiagnosticHelper.FormatNumber(1_500_000_000).Should().Be("1.5B");
	}

	[Fact]
	public void DiagnosticHelper_Should_FormatBrowserCapabilities()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = true,
			BrowserName = "Chrome",
			BrowserVersion = "120",
			Platform = "Win32",
			Vendor = "Google Inc."
		};

		// Act
		var capabilities = DiagnosticHelper.GetBrowserCapabilities(info);

		// Assert
		capabilities.Should().Contain("Chrome");
		capabilities.Should().Contain("120");
		capabilities.Should().Contain("Win32");
		capabilities.Should().Contain("✓ Yes");
	}

	[Fact]
	public void DiagnosticHelper_Should_IndicateUnsupportedBrowser()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "IE",
			BrowserVersion = "11"
		};

		// Act
		var capabilities = DiagnosticHelper.GetBrowserCapabilities(info);

		// Assert
		capabilities.Should().Contain("IE");
		capabilities.Should().Contain("✗ No");
	}

	[Fact]
	public void DiagnosticHelper_Should_IndicateSupportsWithFlags()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "Firefox",
			BrowserVersion = "110",
			SupportsWithFlags = true
		};

		// Act
		var capabilities = DiagnosticHelper.GetBrowserCapabilities(info);

		// Assert
		capabilities.Should().Contain("Supports with Flags");
		capabilities.Should().Contain("configuration required");
	}

	#endregion

	#region Browser Detection Message Tests

	[Fact]
	public void DiagnosticHelper_Should_ProvideSafariSetupInstructions()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "Safari",
			BrowserVersion = "16",
			SupportsWithFlags = true
		};

		// Act
		var message = DiagnosticHelper.GetNotSupportedMessage(info);

		// Assert
		message.Should().Contain("Safari");
		message.Should().Contain("Technology Preview");
		message.Should().Contain("Experimental Features");
	}

	[Fact]
	public void DiagnosticHelper_Should_ProvideFirefoxSetupInstructions()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "Firefox",
			BrowserVersion = "110",
			SupportsWithFlags = true
		};

		// Act
		var message = DiagnosticHelper.GetNotSupportedMessage(info);

		// Assert
		message.Should().Contain("Firefox");
		message.Should().Contain("about:config");
		message.Should().Contain("dom.webgpu.enabled");
	}

	[Fact]
	public void DiagnosticHelper_Should_RecommendChromeForUnsupportedBrowser()
	{
		// Arrange
		var info = new WebGpuCompatibilityInfo
		{
			IsSupported = false,
			BrowserName = "IE",
			BrowserVersion = "11",
			SupportsWithFlags = false
		};

		// Act
		var message = DiagnosticHelper.GetNotSupportedMessage(info);

		// Assert
		message.Should().Contain("Chrome");
		message.Should().Contain("Edge");
		message.Should().NotContain("about:config");
		message.Should().NotContain("Technology Preview");
	}

	#endregion
}

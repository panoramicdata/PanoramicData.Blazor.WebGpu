using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Exceptions;

/// <summary>
/// Tests for PDWebGpu exception classes.
/// </summary>
public class ExceptionTests : TestBase
{
	[Fact]
	public void PDWebGpuException_Should_BeCreatable()
	{
		// Act
		var exception = new PDWebGpuException("Test error");

		// Assert
		exception.Should().NotBeNull();
		exception.Message.Should().Be("Test error");
	}

	[Fact]
	public void PDWebGpuException_Should_WrapInnerException()
	{
		// Arrange
		var inner = new InvalidOperationException("Inner error");

		// Act
		var exception = new PDWebGpuException("Outer error", inner);

		// Assert
		exception.Should().NotBeNull();
		exception.Message.Should().Be("Outer error");
		exception.InnerException.Should().Be(inner);
	}

	[Fact]
	public void PDWebGpuDeviceException_Should_ProvideRecoverySuggestion()
	{
		// Act
		var exception = new PDWebGpuDeviceException("Device lost");

		// Assert
		exception.Should().NotBeNull();
		exception.RecoverySuggestion.Should().NotBeNullOrEmpty();
		exception.RecoverySuggestion.Should().Contain("refresh");
	}

	[Fact]
	public void PDWebGpuShaderCompilationException_Should_ParseLineNumber()
	{
		// Arrange
		var shaderSource = @"
@vertex
fn main() -> invalid_type {
    return something_wrong;
}";
		var inner = new Exception("Shader compilation failed:\nLine 3: expected valid return type");

		// Act
		var exception = new PDWebGpuShaderCompilationException("Compilation failed", shaderSource, inner);

		// Assert
		exception.Should().NotBeNull();
		exception.ShaderSource.Should().Be(shaderSource);
		exception.LineNumber.Should().Be(3);
		exception.CompilationError.Should().Contain("expected valid return type");
	}

	[Fact]
	public void PDWebGpuShaderCompilationException_Should_HandleMissingLineNumber()
	{
		// Arrange
		var shaderSource = "invalid shader";
		var inner = new Exception("Generic compilation error");

		// Act
		var exception = new PDWebGpuShaderCompilationException("Compilation failed", shaderSource, inner);

		// Assert
		exception.Should().NotBeNull();
		exception.LineNumber.Should().BeNull();
		exception.CompilationError.Should().Be("Generic compilation error");
	}

	[Fact]
	public void PDWebGpuShaderCompilationException_Should_FormatErrorWithContext()
	{
		// Arrange
		var shaderSource = @"@vertex
fn main(pos: vec3<f32>) -> @builtin(position) vec4<f32> {
    return invalid_value;
}";
		var inner = new Exception("Line 3: invalid_value not defined");

		// Act
		var exception = new PDWebGpuShaderCompilationException("Compilation failed", shaderSource, inner);
		var formatted = exception.GetFormattedError();

		// Assert
		formatted.Should().Contain("line 3");
		formatted.Should().Contain("invalid_value");
		formatted.Should().Contain(">>>");  // Error marker
	}

	[Fact]
	public void PDWebGpuNotSupportedException_Should_ProvideDefaultMessage()
	{
		// Act
		var exception = new PDWebGpuNotSupportedException();

		// Assert
		exception.Should().NotBeNull();
		exception.Message.Should().Contain("not supported");
		exception.Suggestion.Should().Contain("Chrome");
		exception.Suggestion.Should().Contain("Edge");
	}

	[Fact]
	public void PDWebGpuNotSupportedException_Should_IncludeCompatibilityInfo()
	{
		// Arrange
		var compatInfo = new PanoramicData.Blazor.WebGpu.Interop.WebGpuCompatibilityInfo
		{
			IsSupported = false,
			UserAgent = "Mozilla/5.0 (Test Browser)",
			Vendor = "Test Vendor",
			Platform = "Test Platform",
			ErrorMessage = "WebGPU not available"
		};

		// Act
		var exception = new PDWebGpuNotSupportedException(compatInfo);

		// Assert
		exception.Should().NotBeNull();
		exception.CompatibilityInfo.Should().Be(compatInfo);
		exception.Message.Should().Be("WebGPU not available");
	}

	[Fact]
	public void PDWebGpuNotSupportedException_Should_FormatDetailedMessage()
	{
		// Arrange
		var compatInfo = new PanoramicData.Blazor.WebGpu.Interop.WebGpuCompatibilityInfo
		{
			IsSupported = false,
			UserAgent = "Mozilla/5.0 (Test Browser)",
			Vendor = "Test Vendor",
			Platform = "Windows"
		};
		var exception = new PDWebGpuNotSupportedException(compatInfo);

		// Act
		var detailed = exception.GetDetailedMessage();

		// Assert
		detailed.Should().Contain("Test Browser");
		detailed.Should().Contain("Test Vendor");
		detailed.Should().Contain("Windows");
		detailed.Should().Contain("Chrome 113");
	}

	[Fact]
	public void ExceptionTypes_Should_BeAssignableToBaseException()
	{
		// This test verifies the exception hierarchy
		var deviceException = new PDWebGpuDeviceException();
		var shaderException = new PDWebGpuShaderCompilationException();
		var notSupportedException = new PDWebGpuNotSupportedException();

		deviceException.Should().BeAssignableTo<PDWebGpuException>();
		shaderException.Should().BeAssignableTo<PDWebGpuException>();
		notSupportedException.Should().BeAssignableTo<PDWebGpuException>();
	}
}

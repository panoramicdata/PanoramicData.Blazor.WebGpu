using Microsoft.JSInterop;
using Moq;

namespace PanoramicData.Blazor.WebGpu.Tests.Infrastructure.Mocks;

/// <summary>
/// Mock implementation of IJSRuntime for testing WebGPU JavaScript interop.
/// </summary>
public class MockJSRuntime : Mock<IJSRuntime>
{
	private readonly Dictionary<string, object?> _invocations = [];

	public MockJSRuntime()
	{
		SetupDefaultBehavior();
	}

	private void SetupDefaultBehavior()
	{
		// Setup default responses for common WebGPU interop calls
		Setup(x => x.InvokeAsync<bool>(
			"webGpuInterop.isSupported",
			It.IsAny<object[]>()))
			.ReturnsAsync(true);

		Setup(x => x.InvokeAsync<IJSObjectReference>(
			It.IsAny<string>(),
			It.IsAny<object[]>()))
			.ReturnsAsync(new Mock<IJSObjectReference>().Object);
	}

	/// <summary>
	/// Records an invocation for later verification.
	/// </summary>
	public void RecordInvocation(string identifier, params object[] args)
	{
		_invocations[identifier] = args;
	}

	/// <summary>
	/// Verifies that a specific method was called with the given identifier.
	/// </summary>
	public bool WasInvoked(string identifier) => _invocations.ContainsKey(identifier);

	/// <summary>
	/// Clears all recorded invocations.
	/// </summary>
	public void ClearInvocations() => _invocations.Clear();
}

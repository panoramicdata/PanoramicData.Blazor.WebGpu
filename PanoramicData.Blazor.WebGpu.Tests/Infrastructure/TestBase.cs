namespace PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

/// <summary>
/// Base class for all unit tests in the WebGPU framework.
/// Provides common test utilities and setup/teardown logic.
/// </summary>
public abstract class TestBase : IDisposable
{
	private bool _disposed;

	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected TestBase()
	{
		// Common setup for all tests
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

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
}

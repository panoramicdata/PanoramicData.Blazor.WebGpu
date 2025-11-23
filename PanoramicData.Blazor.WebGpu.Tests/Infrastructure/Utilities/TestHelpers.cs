namespace PanoramicData.Blazor.WebGpu.Tests.Infrastructure.Utilities;

/// <summary>
/// Helper methods for common test operations.
/// </summary>
public static class TestHelpers
{
	/// <summary>
	/// Creates a temporary test file with the specified content.
	/// </summary>
	public static async Task<string> CreateTempFileAsync(string content, string extension = ".wgsl")
	{
		var tempPath = Path.Combine(Path.GetTempPath(), $"webgpu_test_{Guid.NewGuid()}{extension}");
		await File.WriteAllTextAsync(tempPath, content);
		return tempPath;
	}

	/// <summary>
	/// Deletes a temporary test file if it exists.
	/// </summary>
	public static void DeleteTempFile(string path)
	{
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	/// <summary>
	/// Waits for a condition to be true within a timeout period.
	/// </summary>
	public static async Task<bool> WaitForConditionAsync(
		Func<bool> condition,
		TimeSpan timeout,
		TimeSpan? pollInterval = null)
	{
		var interval = pollInterval ?? TimeSpan.FromMilliseconds(10);
		var endTime = DateTime.UtcNow + timeout;

		while (DateTime.UtcNow < endTime)
		{
			if (condition())
			{
				return true;
			}

			await Task.Delay(interval);
		}

		return false;
	}

	/// <summary>
	/// Compares two floating-point arrays with tolerance.
	/// </summary>
	public static bool ArraysEqual(float[] a, float[] b, float tolerance = 0.0001f)
	{
		if (a.Length != b.Length)
		{
			return false;
		}

		for (var i = 0; i < a.Length; i++)
		{
			if (Math.Abs(a[i] - b[i]) > tolerance)
			{
				return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Creates a 4x4 identity matrix for testing.
	/// </summary>
	public static float[] CreateIdentityMatrix()
	{
		return
		[
			1, 0, 0, 0,
			0, 1, 0, 0,
			0, 0, 1, 0,
			0, 0, 0, 1
		];
	}
}

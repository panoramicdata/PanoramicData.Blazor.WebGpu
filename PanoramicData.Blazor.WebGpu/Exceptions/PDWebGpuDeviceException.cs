namespace PanoramicData.Blazor.WebGpu;

/// <summary>
/// Exception thrown when WebGPU device initialization or operation fails.
/// </summary>
public class PDWebGpuDeviceException : PDWebGpuException
{
	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuDeviceException"/> class.
	/// </summary>
	public PDWebGpuDeviceException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuDeviceException"/> class with a specified error message.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public PDWebGpuDeviceException(string message) : base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PDWebGpuDeviceException"/> class with a specified error message
	/// and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception.</param>
	public PDWebGpuDeviceException(string message, Exception innerException) : base(message, innerException)
	{
	}

	/// <summary>
	/// Gets suggested recovery actions for device errors.
	/// </summary>
	public static string RecoverySuggestion => "Try refreshing the page or restarting your browser. " +
		"If the problem persists, check if your GPU drivers are up to date.";
}

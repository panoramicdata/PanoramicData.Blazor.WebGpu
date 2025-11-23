using System.Numerics;
using PanoramicData.Blazor.WebGpu.Camera;
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests.Camera;

/// <summary>
/// Tests for camera system functionality.
/// </summary>
public class CameraTests : TestBase
{
	#region PDWebGpuOrbitCamera Tests

	[Fact]
	public void OrbitCamera_Should_HaveDefaultValues()
	{
		// Act
		var camera = new PDWebGpuOrbitCamera();

		// Assert
		camera.Target.Should().Be(Vector3.Zero);
		camera.Distance.Should().Be(10f);
		camera.Yaw.Should().Be(0f);
		camera.Pitch.Should().Be(0f);
		camera.AspectRatio.Should().BeApproximately(16f / 9f, 0.01f);
	}

	[Fact]
	public void OrbitCamera_Should_CalculatePosition()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera
		{
			Target = Vector3.Zero,
			Distance = 10f,
			Yaw = 0f,
			Pitch = 0f
		};

		// Act
		var position = camera.Position;

		// Assert
		position.X.Should().BeApproximately(10f, 0.01f);
		position.Y.Should().BeApproximately(0f, 0.01f);
		position.Z.Should().BeApproximately(0f, 0.01f);
	}

	[Fact]
	public void OrbitCamera_Should_Rotate()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();
		var initialYaw = camera.Yaw;
		var initialPitch = camera.Pitch;

		// Act
		camera.Rotate(MathF.PI / 4f, MathF.PI / 6f);

		// Assert
		camera.Yaw.Should().Be(initialYaw + MathF.PI / 4f);
		camera.Pitch.Should().Be(initialPitch + MathF.PI / 6f);
	}

	[Fact]
	public void OrbitCamera_Should_Zoom()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera { Distance = 10f };

		// Act
		camera.Zoom(-2f);

		// Assert
		camera.Distance.Should().Be(8f);
	}

	[Fact]
	public void OrbitCamera_Should_ClampDistance()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera
		{
			MinDistance = 1f,
			MaxDistance = 100f
		};

		// Act & Assert - Below minimum
		camera.Distance = 0.5f;
		camera.Distance.Should().Be(1f);

		// Act & Assert - Above maximum
		camera.Distance = 150f;
		camera.Distance.Should().Be(100f);
	}

	[Fact]
	public void OrbitCamera_Should_ClampPitch()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();

		// Act & Assert - Maximum pitch
		camera.Pitch = MathF.PI;
		camera.Pitch.Should().BeLessThan(MathF.PI / 2f);

		// Act & Assert - Minimum pitch
		camera.Pitch = -MathF.PI;
		camera.Pitch.Should().BeGreaterThan(-MathF.PI / 2f);
	}

	[Fact]
	public void OrbitCamera_Should_CalculateViewMatrix()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera
		{
			Target = Vector3.Zero,
			Distance = 10f
		};

		// Act
		var viewMatrix = camera.ViewMatrix;

		// Assert
		viewMatrix.Should().NotBe(Matrix4x4.Identity);
	}

	[Fact]
	public void OrbitCamera_Should_CalculateProjectionMatrix()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera
		{
			FieldOfView = MathF.PI / 4f,
			AspectRatio = 16f / 9f,
			NearPlane = 0.1f,
			FarPlane = 1000f
		};

		// Act
		var projectionMatrix = camera.ProjectionMatrix;

		// Assert
		projectionMatrix.Should().NotBe(Matrix4x4.Identity);
	}

	#endregion

	#region PDWebGpuFirstPersonCamera Tests

	[Fact]
	public void FirstPersonCamera_Should_HaveDefaultValues()
	{
		// Act
		var camera = new PDWebGpuFirstPersonCamera();

		// Assert
		camera.Position.Y.Should().Be(1.7f); // Default eye height
		camera.Yaw.Should().Be(0f);
		camera.Pitch.Should().Be(0f);
		camera.MoveSpeed.Should().Be(5f);
	}

	[Fact]
	public void FirstPersonCamera_Should_CalculateForwardVector()
	{
		// Arrange
		var camera = new PDWebGpuFirstPersonCamera
		{
			Yaw = 0f,
			Pitch = 0f
		};

		// Act
		var forward = camera.Forward;

		// Assert
		forward.X.Should().BeApproximately(1f, 0.01f);
		forward.Y.Should().BeApproximately(0f, 0.01f);
		forward.Z.Should().BeApproximately(0f, 0.01f);
	}

	[Fact]
	public void FirstPersonCamera_Should_CalculateRightVector()
	{
		// Arrange
		var camera = new PDWebGpuFirstPersonCamera
		{
			Yaw = 0f,
			Pitch = 0f
		};

		// Act
		var right = camera.Right;

		// Assert - When looking along +X axis, right vector points toward +Z
		right.Z.Should().BeApproximately(1f, 0.01f);
	}

	[Fact]
	public void FirstPersonCamera_Should_Look()
	{
		// Arrange
		var camera = new PDWebGpuFirstPersonCamera();
		var initialYaw = camera.Yaw;
		var initialPitch = camera.Pitch;

		// Act
		camera.Look(100f, 50f); // Mouse delta in pixels

		// Assert
		camera.Yaw.Should().NotBe(initialYaw);
		camera.Pitch.Should().NotBe(initialPitch);
	}

	[Fact]
	public void FirstPersonCamera_Should_MoveForward()
	{
		// Arrange
		var camera = new PDWebGpuFirstPersonCamera
		{
			Position = Vector3.Zero,
			Yaw = 0f
		};
		var initialPosition = camera.Position;

		// Act
		camera.MoveForward(1f, 0.1f); // Move forward for 0.1 seconds

		// Assert
		camera.Position.X.Should().BeGreaterThan(initialPosition.X);
	}

	[Fact]
	public void FirstPersonCamera_Should_MoveRight()
	{
		// Arrange
		var camera = new PDWebGpuFirstPersonCamera
		{
			Position = Vector3.Zero,
			Yaw = 0f
		};
		var initialPosition = camera.Position;

		// Act
		camera.MoveRight(1f, 0.1f); // Move right for 0.1 seconds

		// Assert
		camera.Position.Should().NotBe(initialPosition);
	}

	[Fact]
	public void FirstPersonCamera_Should_MoveUp()
	{
		// Arrange
		var camera = new PDWebGpuFirstPersonCamera
		{
			Position = Vector3.Zero
		};

		// Act
		camera.MoveUp(1f, 0.1f); // Move up for 0.1 seconds

		// Assert
		camera.Position.Y.Should().BeGreaterThan(0f);
	}

	#endregion

	#region PDWebGpuOrthographicCamera Tests

	[Fact]
	public void OrthographicCamera_Should_HaveDefaultValues()
	{
		// Act
		var camera = new PDWebGpuOrthographicCamera();

		// Assert
		camera.Position.Z.Should().Be(10f);
		camera.Target.Should().Be(Vector3.Zero);
		camera.Left.Should().Be(-10f);
		camera.Right.Should().Be(10f);
		camera.Bottom.Should().Be(-10f);
		camera.Top.Should().Be(10f);
		camera.Zoom.Should().Be(1f);
	}

	[Fact]
	public void OrthographicCamera_Should_SetBounds()
	{
		// Arrange
		var camera = new PDWebGpuOrthographicCamera();

		// Act
		camera.SetBounds(20f, 10f);

		// Assert
		camera.Left.Should().Be(-10f);
		camera.Right.Should().Be(10f);
		camera.Bottom.Should().Be(-5f);
		camera.Top.Should().Be(5f);
	}

	[Fact]
	public void OrthographicCamera_Should_SetBoundsFromHeight()
	{
		// Arrange
		var camera = new PDWebGpuOrthographicCamera
		{
			AspectRatio = 2f // 2:1 aspect ratio
		};

		// Act
		camera.SetBoundsFromHeight(10f);

		// Assert
		camera.Bottom.Should().Be(-5f);
		camera.Top.Should().Be(5f);
		camera.Left.Should().Be(-10f);
		camera.Right.Should().Be(10f);
	}

	[Fact]
	public void OrthographicCamera_Should_Zoom()
	{
		// Arrange
		var camera = new PDWebGpuOrthographicCamera
		{
			Zoom = 1f
		};

		// Act
		camera.Zoom = 2f;

		// Assert
		camera.Zoom.Should().Be(2f);
	}

	[Fact]
	public void OrthographicCamera_Should_Pan()
	{
		// Arrange
		var camera = new PDWebGpuOrthographicCamera
		{
			Position = new Vector3(0, 0, 10),
			Target = Vector3.Zero
		};
		var initialPosition = camera.Position;
		var initialTarget = camera.Target;

		// Act
		camera.Pan(1f, 1f);

		// Assert
		camera.Position.Should().NotBe(initialPosition);
		camera.Target.Should().NotBe(initialTarget);
	}

	[Fact]
	public void OrthographicCamera_Should_CalculateProjectionMatrix()
	{
		// Arrange
		var camera = new PDWebGpuOrthographicCamera
		{
			Left = -10f,
			Right = 10f,
			Bottom = -10f,
			Top = 10f,
			Zoom = 1f
		};

		// Act
		var projectionMatrix = camera.ProjectionMatrix;

		// Assert
		projectionMatrix.Should().NotBe(Matrix4x4.Identity);
	}

	#endregion

	#region Camera Base Tests

	[Fact]
	public void Camera_Should_CacheViewMatrix()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();

		// Act
		var viewMatrix1 = camera.ViewMatrix;
		var viewMatrix2 = camera.ViewMatrix;

		// Assert
		viewMatrix1.Should().Be(viewMatrix2);
	}

	[Fact]
	public void Camera_Should_InvalidateViewMatrixOnChange()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();
		var initialMatrix = camera.ViewMatrix;

		// Act
		camera.Distance = 20f;
		var newMatrix = camera.ViewMatrix;

		// Assert
		newMatrix.Should().NotBe(initialMatrix);
	}

	[Fact]
	public void Camera_Should_CacheProjectionMatrix()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();

		// Act
		var projectionMatrix1 = camera.ProjectionMatrix;
		var projectionMatrix2 = camera.ProjectionMatrix;

		// Assert
		projectionMatrix1.Should().Be(projectionMatrix2);
	}

	[Fact]
	public void Camera_Should_InvalidateProjectionMatrixOnChange()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();
		var initialMatrix = camera.ProjectionMatrix;

		// Act
		camera.AspectRatio = 4f / 3f;
		var newMatrix = camera.ProjectionMatrix;

		// Assert
		newMatrix.Should().NotBe(initialMatrix);
	}

	[Fact]
	public void Camera_Should_CalculateViewProjectionMatrix()
	{
		// Arrange
		var camera = new PDWebGpuOrbitCamera();

		// Act
		var viewProjectionMatrix = camera.ViewProjectionMatrix;

		// Assert
		viewProjectionMatrix.Should().NotBe(Matrix4x4.Identity);
	}

	#endregion
}

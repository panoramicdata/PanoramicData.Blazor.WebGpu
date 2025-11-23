using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

namespace PanoramicData.Blazor.WebGpu.Tests;

/// <summary>
/// Smoke tests to verify basic test infrastructure is working.
/// </summary>
public class SmokeTests : TestBase
{
	[Fact]
	public void Framework_Should_Load()
	{
		// This test verifies that the test project can reference the main library
		var assembly = typeof(PanoramicData.Blazor.WebGpu.PDWebGpuComponentBase).Assembly;
		assembly.Should().NotBeNull();
		assembly.GetName().Name.Should().Be("PanoramicData.Blazor.WebGpu");
	}

	[Fact]
	public void TestInfrastructure_Should_BeAccessible()
	{
		// This test verifies that test infrastructure classes are accessible
		var testBase = this;
		testBase.Should().NotBeNull();
		testBase.Should().BeOfType<SmokeTests>();
	}

	[Fact]
	public void FluentAssertions_Should_Work()
	{
		// This test verifies that FluentAssertions is working
		var value = 42;
		value.Should().Be(42);
		value.Should().NotBe(0);
		value.Should().BeGreaterThan(0);
	}

	[Fact]
	public void MockJSRuntime_Should_BeCreatable()
	{
		// This test verifies that the mock JSRuntime can be instantiated
		var mockJSRuntime = new Infrastructure.Mocks.MockJSRuntime();
		mockJSRuntime.Should().NotBeNull();
		mockJSRuntime.Object.Should().NotBeNull();
	}

	[Fact]
	public void TestData_Should_ProvideShaders()
	{
		// This test verifies that test data is available
		Infrastructure.Utilities.TestData.SimpleVertexShader.Should().NotBeNullOrEmpty();
		Infrastructure.Utilities.TestData.SimpleFragmentShader.Should().NotBeNullOrEmpty();
		Infrastructure.Utilities.TestData.InvalidShader.Should().NotBeNullOrEmpty();
	}

	[Fact]
	public void TestData_Should_ProvideGeometry()
	{
		// This test verifies that test geometry data is available
		Infrastructure.Utilities.TestData.TriangleVertices.Should().NotBeNull();
		Infrastructure.Utilities.TestData.TriangleVertices.Length.Should().Be(9); // 3 vertices * 3 components

		Infrastructure.Utilities.TestData.CubeVertices.Should().NotBeNull();
		Infrastructure.Utilities.TestData.CubeIndices.Should().NotBeNull();
	}

	[Fact]
	public void TestHelpers_ArraysEqual_Should_Work()
	{
		// This test verifies that the array comparison helper works
		var a = new float[] { 1.0f, 2.0f, 3.0f };
		var b = new float[] { 1.0f, 2.0f, 3.0f };
		var c = new float[] { 1.0f, 2.0f, 3.1f };

		Infrastructure.Utilities.TestHelpers.ArraysEqual(a, b).Should().BeTrue();
		Infrastructure.Utilities.TestHelpers.ArraysEqual(a, c).Should().BeFalse();
	}

	[Fact]
	public void TestHelpers_IdentityMatrix_Should_BeCorrect()
	{
		// This test verifies that the identity matrix helper works
		var identity = Infrastructure.Utilities.TestHelpers.CreateIdentityMatrix();
		identity.Should().NotBeNull();
		identity.Length.Should().Be(16); // 4x4 matrix
		identity[0].Should().Be(1.0f);   // [0,0]
		identity[5].Should().Be(1.0f);   // [1,1]
		identity[10].Should().Be(1.0f);  // [2,2]
		identity[15].Should().Be(1.0f);  // [3,3]
	}

	[Fact]
	public async Task TestHelpers_WaitForCondition_Should_Work()
	{
		// This test verifies that the async waiting helper works
		var flag = false;
		var task = Task.Run(async () =>
		{
			await Task.Delay(50);
			flag = true;
		},
		CancellationToken);

		var result = await Infrastructure.Utilities.TestHelpers.WaitForConditionAsync(
			() => flag,
			TimeSpan.FromSeconds(1));

		result.Should().BeTrue();
		flag.Should().BeTrue();
		await task;
	}

	[Fact]
	public async Task TestHelpers_CreateTempFile_Should_Work()
	{
		// This test verifies that temp file creation works
		var content = "test content";
		var path = await Infrastructure.Utilities.TestHelpers.CreateTempFileAsync(content);

		try
		{
			File.Exists(path).Should().BeTrue();
			var readContent = await File.ReadAllTextAsync(path,
			CancellationToken);
			readContent.Should().Be(content);
		}
		finally
		{
			Infrastructure.Utilities.TestHelpers.DeleteTempFile(path);
			File.Exists(path).Should().BeFalse();
		}
	}
}

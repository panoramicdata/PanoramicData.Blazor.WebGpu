# PanoramicData.Blazor.WebGpu.Tests

This project contains unit and integration tests for the PanoramicData.Blazor.WebGpu framework.

## Test Framework

- **xUnit 2.9**: Modern testing framework with excellent async support
- **FluentAssertions**: Fluent assertion library for readable test code
- **Moq**: Mocking library for creating test doubles
- **Coverlet**: Code coverage collection

## Running Tests

### Visual Studio
- Open Test Explorer (Test > Test Explorer)
- Click "Run All" to execute all tests

### Command Line
```bash
# Run all tests
dotnet test

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Run specific test
dotnet test --filter "FullyQualifiedName~SmokeTests"
```

## Code Coverage

Code coverage is configured using Coverlet. Target coverage is >80%.

### Generate Coverage Report
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura
```

### View Coverage in Visual Studio
1. Install the "Fine Code Coverage" extension
2. Run tests with coverage enabled
3. View coverage results in the Coverage window

## Test Infrastructure

### Infrastructure Components

- **TestBase**: Base class for all tests, provides common setup/teardown
- **MockJSRuntime**: Mock JavaScript runtime for WebGPU interop testing
- **MockWebGpuDevice**: Mock WebGPU device for testing without GPU hardware
- **TestData**: Provides sample shaders, geometry, and test resources
- **TestHelpers**: Common utility methods for testing

### Mock Strategy

The test infrastructure provides mocks for:
- JavaScript interop (IJSRuntime)
- WebGPU device and adapter
- Canvas context
- Shader compilation and validation

This allows testing without requiring:
- Actual GPU hardware
- Browser environment
- WebGPU API availability

## Test Organization

Tests are organized by feature area:

- **SmokeTests**: Basic infrastructure validation
- **Component Tests**: Tests for individual components (to be added)
- **Service Tests**: Tests for services and dependency injection (to be added)
- **Resource Tests**: Tests for GPU resource wrappers (to be added)
- **Integration Tests**: End-to-end tests with mocked WebGPU (to be added)

## Writing Tests

### Example Test
```csharp
using PanoramicData.Blazor.WebGpu.Tests.Infrastructure;

public class MyComponentTests : TestBase
{
    [Fact]
    public void MyComponent_Should_InitializeCorrectly()
    {
        // Arrange
        var component = new MyComponent();

        // Act
        component.Initialize();

        // Assert
        component.IsInitialized.Should().BeTrue();
    }
}
```

### Using Mock JSRuntime
```csharp
[Fact]
public async Task Service_Should_CallJavaScript()
{
    // Arrange
    using var mockJS = new MockJSRuntime();
    var service = new MyService(mockJS.Object);

    // Act
    await service.InitializeAsync();

    // Assert
    mockJS.Verify(x => x.InvokeAsync<bool>(
        "webGpuInterop.isSupported",
        It.IsAny<object[]>()), Times.Once);
}
```

## Best Practices

1. **Use FluentAssertions**: Prefer `value.Should().Be(expected)` over `Assert.Equal(expected, value)`
2. **Inherit from TestBase**: All test classes should inherit from `TestBase`
3. **Dispose Resources**: Use `using` statements or dispose in teardown
4. **Async Tests**: Use `async Task` for tests that perform async operations
5. **Clear Test Names**: Use descriptive names like `ComponentName_Should_DoSomething_When_Condition`
6. **Arrange-Act-Assert**: Structure tests with clear AAA pattern
7. **Mock External Dependencies**: Use mocks for all external dependencies (JS interop, file system, etc.)

## Continuous Integration

Tests are run automatically on:
- Every commit
- Pull requests
- Before release builds

CI enforces:
- All tests must pass
- Code coverage must be >80%
- No compilation warnings

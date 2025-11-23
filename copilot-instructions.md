# Copilot Instructions for PanoramicData.Blazor.WebGpu

## Purpose
This file provides instructions for GitHub Copilot and other AI coding assistants when working on the PanoramicData.Blazor.WebGpu framework.

## Critical Rules
1. **No Additional Markdown Files**: NEVER create additional markdown files unless explicitly requested by the developer
2. **Update Master Plan**: DO update MASTER_PLAN.md after significant progress
3. **Phase Management**: You MAY add, insert, or renumber phases in the Master Plan as needed

## Naming Conventions
- All components: `PDWebGpu<Xxx>` pattern
  - Example: `PDWebGpuContainer`, `PDWebGpuCanvas`, `PDWebGpuOrbitCamera`
- All services: `IPDWebGpu<Xxx>Service` pattern
  - Example: `IPDWebGpuService`
- All exception types: `PDWebGpu<Xxx>Exception` pattern
  - Example: `PDWebGpuShaderCompilationException`, `PDWebGpuDeviceException`
- All event args: `PDWebGpu<Xxx>EventArgs` pattern
  - Example: `PDWebGpuFrameEventArgs`, `PDWebGpuResizeEventArgs`

## Code Style
- Follow .editorconfig from Jira.Api project (already copied to this project)
- Use C# 12 features (collection expressions, primary constructors, etc.)
- Prefer async/await patterns for all I/O operations
- Use file-scoped namespaces
- Prefer expression-bodied members where appropriate
- Use `var` for local variable declarations
- Private fields use underscore prefix: `_fieldName`
- XML documentation required on all public APIs

## Architecture Patterns
- **Component-based design**: All components inherit from `PDWebGpuComponentBase`
- **Service injection**: Use dependency injection for cross-cutting concerns
- **Resource wrappers**: All WebGPU objects wrapped in C# classes with `PDWebGpu` prefix
- **Automatic disposal**: Implement `IDisposable` and `IAsyncDisposable` for resource cleanup
- **Event-driven**: Use EventCallback and custom event args for component communication

## JavaScript Interop
- Minimize JS code; keep everything in single file: `webgpu-interop.js`
- Use `IJSRuntime` for all interop calls
- Wrap all JS calls in try-catch with meaningful C# exceptions
- Never expose raw JavaScript objects to C# developers
- All WebGPU operations must go through C# wrapper classes

## Testing Strategy
- Unit tests for all framework components
- Integration tests for WebGPU interop
- Mock/stub WebGPU API for unit testing
- Target > 80% code coverage
- Use xUnit as testing framework
- Test project: `PanoramicData.Blazor.WebGpu.Tests`

## Performance Optimizations
- Minimize allocations in render loop
- Use object pooling for frequently allocated objects
- Batch WebGPU commands where possible
- Avoid blocking calls on main thread
- Use `ValueTask` for hot paths

## Common Pitfalls
- Don't forget to dispose GPU resources
- Don't block the UI thread with long-running operations
- Don't expose JavaScript objects directly to consumers
- Always handle WebGPU device lost scenarios
- Remember to update shader validation on hot-reload

## Development Workflow
1. Read MASTER_PLAN.md before starting work
2. Update phase checkboxes as tasks are completed
3. Add new discoveries to this file (copilot-instructions.md)
4. Keep all code comments concise and meaningful
5. Ensure XML documentation on public APIs
6. Run tests before committing changes
7. Update MASTER_PLAN.md version history when making significant changes

## Integration with PanoramicData.Blazor
- Reference existing `PDMonaco` component for shader editing
- Use existing keyboard handling services
- Follow established component patterns from PD.Blazor
- Maintain consistency with existing PD.Blazor conventions
- Reuse existing UI components where applicable

## WebGPU-Specific Patterns
- Canvas element managed by `PDWebGpuCanvas` component
- Render loop coordinated by `IPDWebGpuService`
- Shaders always in WGSL text format (no binary support)
- Support both fixed and variable frame rate modes
- Configurable pause behavior when tab/window inactive
- Hot-reload support for WGSL shaders in development

## Error Handling
- All WebGPU errors wrapped in C# exceptions
- Shader compilation errors include line numbers
- Device errors include recovery suggestions
- Graceful degradation when WebGPU unavailable
- User-friendly error messages for all failure modes

## Documentation Standards
- XML documentation on all public members
- Include example code in XML comments where helpful
- Document exceptions that can be thrown
- Document thread safety characteristics
- Keep documentation concise and accurate

## Project Structure
- **PanoramicData.Blazor.WebGpu**: Framework library (NuGet package)
- **PanoramicData.Blazor.WebGpu.Tests**: Unit and integration tests
- **PanoramicData.Blazor.WebGpu.Template**: Visual Studio template (.vstemplate)
- **PanoramicData.Blazor.WebGpu.Demo**: Demo application with Monaco editor

## Version Information
- Target Framework: .NET 10
- Solution Format: .slnx (Visual Studio 2026)
- License: MIT
- Copyright: Panoramic Data Limited 2025

# WebGPU Blazor Framework - Master Plan

## Project Overview

**Project Name**: PanoramicData.Blazor.WebGpu  
**Location**: `C:\Users\DavidBond\source\repos\panoramicdata\WebGpu`  
**Solution Format**: `.slnx` (Visual Studio 2026)  
**Target Framework**: .NET 10  
**License**: MIT  
**Copyright**: Panoramic Data Limited 2025  
**Purpose**: A WebGPU framework for Blazor WebAssembly that enables developers to create GPU-accelerated applications using pure C#, with no JavaScript knowledge required.

## Vision Statement

Create a Visual Studio template and NuGet package that allows developers to build WebGPU applications in Blazor WebAssembly using only C#. The framework handles all WebGPU complexity, screen resizing, render loops, and provides a component-based layered rendering system with minimal JavaScript involvement.

## Key Design Principles

1. **C# First**: Developers interact exclusively with C# APIs
2. **Minimal JavaScript**: Single `.js` file for unavoidable WebGPU interop only
3. **Component-Based**: All components use `PDWebGpu<Xxx>` naming convention and inherit from `PDWebGpuComponentBase`
4. **Layered Architecture**: Component-based WebGPU canvas on background Z-index with foreground HTML container div
5. **Hot-Reload Friendly**: WGSL shaders can be reloaded during development with validation and error display
6. **Performance First**: Built-in optional performance metrics, variable and fixed frame rate support
7. **Extensible**: Builds upon existing `PanoramicData.Blazor` library capabilities

## Development Directives

**CRITICAL DEVELOPMENT RULE**: While developing this project, NEVER create additional markdown files unless explicitly requested. This may be unusual, as AI assistants typically create markdown files frequently, but it's tiring to continually delete them. Conversely, it IS acceptable and encouraged to update this Master Plan after every action if significant progress has been made. It's also acceptable to add new phases to the plan or insert new phases, providing existing ones are renumbered accordingly.

## Project Structure

### Solution Projects

1. **PanoramicData.Blazor.WebGpu** (Framework Library)
   - Core WebGPU wrapper classes
   - Component base classes
   - Services and dependency injection
   - Camera systems
   - Resource management (buffers, textures, pipelines)
   - NuGet package output

2. **PanoramicData.Blazor.WebGpu.Tests** (Test Project)
   - Unit tests for framework components
   - Integration tests for WebGPU interop
   - Test helpers and utilities

3. **PanoramicData.Blazor.WebGpu.Template** (Visual Studio Template)
   - Template configuration for VS Marketplace
   - Template parameters
   - Empty app scaffolding (rotating cube with simple lighting)

4. **PanoramicData.Blazor.WebGpu.Demo** (Demo Application)
   - Split-view demo: Monaco editor (left) + WebGPU output (right)
   - WGSL shader hot-reload demonstration
   - Performance metrics toggle
   - Example usage of all major framework features

### Dependencies

- **PanoramicData.Blazor**: Existing library dependency (NuGet package - latest version)
  - Use existing `PDMonaco` component for shader editing
  - Configure WGSL language support via custom language file
  - Use existing global keyboard handling services
  - Follow established component patterns and conventions

### Configuration Files

- **.editorconfig**: Copy from `C:\Users\DavidBond\source\repos\panoramicdata\Jira.Api\.editorconfig`
- **copilot-instructions.md**: Development instructions for AI assistants (see Copilot Instructions section below)

## Architecture Specification

### Component Hierarchy

```
PDWebGpuComponentBase (abstract base class)
├── PDWebGpuContainer (main container with layered rendering)
├── PDWebGpuCanvas (WebGPU rendering surface)
├── PDWebGpuPerformanceDisplay (optional metrics overlay)
└── Camera Components
    ├── PDWebGpuOrbitCamera
    ├── PDWebGpuFirstPersonCamera
    └── PDWebGpuOrthographicCamera
```

### Core Services

```
IPDWebGpuService (primary service interface)
├── Initialization & lifecycle management
├── Device and adapter access
├── Resource creation and management
└── Render loop coordination
```

### C# Wrapper Classes

All GPU resources exposed via C# wrappers with `PDWebGpu` prefix:

- `PDWebGpuBuffer` (vertex, uniform, storage buffers)
- `PDWebGpuTexture`
- `PDWebGpuSampler`
- `PDWebGpuPipeline` (render and compute pipelines)
- `PDWebGpuShader` (WGSL shader management)
- `PDWebGpuBindGroup`
- `PDWebGpuCommandEncoder`

### Layered Rendering System

**PDWebGpuContainer** is a component-based container that provides two layers:

1. **Background Layer** (Z-Index: 0)
   - WebGPU canvas (created and managed by PDWebGpuCanvas component)
   - GPU-accelerated rendering
   - Handles screen resizing automatically

2. **Foreground Layer** (Z-Index: 1)
   - HTML container div
   - Standard Blazor components
   - UI overlays, controls, text, etc.

**Canvas Management**:
- Canvas element is created by the PDWebGpuCanvas component
- PDWebGpuContainer instantiates and manages PDWebGpuCanvas
- No direct JavaScript canvas manipulation required

### Render Loop

**Event Model**:
```csharp
- OnFrame(PDWebGpuFrameEventArgs)
- OnResize(PDWebGpuResizeEventArgs)
- OnGpuReady(EventArgs)
- OnError(PDWebGpuErrorEventArgs)
```

**Frame Rate Modes**:
- **Variable Frame Rate**: Render as fast as possible (VSync limited)
- **Fixed Frame Rate**: Configurable target FPS (e.g., 60, 30, 144)

**Pause Behavior**:
- **Configurable**: Determine render loop behavior when tab loses focus or window is minimized
- **Options**: Continue rendering in background OR pause when not visible
- **Property**: `PauseWhenInactive` (bool, default: true)

**Configuration**: Set via properties on `PDWebGpuContainer`:
```csharp
<PDWebGpuContainer 
    FrameRateMode="@FrameRateMode.Fixed" 
    TargetFrameRate="60"
    PauseWhenInactive="true">
</PDWebGpuContainer>
```

### Shader Management

**Supported Formats**:
1. **WGSL Text**: Compiled at runtime via WebGPU API (only format supported)

**Development Features**:
- Hot-reload support during development
- Validation on load
- Error reporting with line numbers
- Errors thrown as C# exceptions with detailed messages

**Demo Application Shader Editor**:
- PDMonaco editor (left pane) with WGSL syntax highlighting (via custom language file)
- Live WebGPU output (right pane)
- Real-time error display on compilation failure

### Input Handling

**Mouse & Touch**:
- Available as optional features on `PDWebGpuComponentBase`
- Coordinates automatically translated to WebGPU canvas space
- Events: OnMouseDown, OnMouseMove, OnMouseUp, OnTouchStart, etc.

**Keyboard**:
- Use existing `PanoramicData.Blazor` global keyboard handling services
- Components register interest in key combinations
- Support for modifier keys (Ctrl, Alt, Shift)

### Camera System

**Single Eye Rendering** (stereoscopic reserved for future):

**PDWebGpuOrbitCamera**:
- Target-based camera
- Configurable distance, angles
- Mouse interaction for rotation/zoom

**PDWebGpuFirstPersonCamera**:
- FPS-style camera
- WASD movement, mouse look
- Configurable speed and sensitivity

**PDWebGpuOrthographicCamera**:
- Parallel projection
- 2D rendering and UI
- Configurable bounds

**Common Features**:
- Automatic view/projection matrix calculation
- C# property-based configuration
- Optional automatic input handling (opt-in)

### Performance Metrics

**PDWebGpuPerformanceDisplay Component**:

**Available Metrics**:
- FPS (Frames Per Second)
- Frame Time (milliseconds)
- Frame Time Usage % (for fixed frame intervals)
- Draw Calls per frame
- Triangle Count
- Custom user-defined metrics

**Configuration**:
```csharp
<PDWebGpuPerformanceDisplay Options="@perfOptions" />

perfOptions = new PDWebGpuPerformanceDisplayOptions 
{
    ShowFPS = true,
    ShowFrameTime = true,
    ShowDrawCalls = false,
    Position = CornerPosition.TopRight
};
```

**Demo App**: Toggle-able performance display to demonstrate functionality

### Error Handling

**Shader Compilation Errors**:
- Thrown as `PDWebGpuShaderCompilationException`
- Include line number and error message
- Display in HTML layer for user-friendly debugging

**WebGPU Device Errors**:
- Thrown as `PDWebGpuDeviceException`
- Includes error category and recovery suggestions

**Fallback Behavior**:
- Detect WebGPU unavailability
- Throw `PDWebGpuNotSupportedException` with browser compatibility info
- Suggest enabling flags or upgrading browser

### JavaScript Interop

**Single File**: `webgpu-interop.js` in wwwroot

**Minimal Responsibilities**:
1. WebGPU adapter/device initialization
2. Canvas context acquisition
3. Command buffer submission
4. Shader compilation (via WebGPU API, not custom compiler)
5. Resource creation forwarding to WebGPU API

**Production Build**:
- Bundled and minified
- ES6 module format
- No external dependencies

## Template Specification

### Visual Studio Template Configuration

**Template Type**: Project Template (.NET 10 Blazor WebAssembly)

**Template Parameters**:
- Project Name (standard)
- Include Sample Shaders (checkbox, default: true)

**Generated Project Contents**:
1. Empty Blazor WebAssembly app
2. PDWebGpuContainer configured with basic scene
3. Rotating cube with simple lighting
4. Basic orbit camera
5. Example WGSL shaders (vertex + fragment)
6. Performance display (disabled by default, commented code to enable)

**NuGet Reference**: Automatically includes `PanoramicData.Blazor.WebGpu` package

### Marketplace Publishing Requirements

**Package Requirements**:
- Icon (512x512 PNG) using the standard PanoramicData.png file from the PanoramicData.Blazor library
- Preview images/screenshots
- README with getting started guide
- License file (MIT)
- Version numbering (semantic versioning)

**Marketplace Metadata**:
- Category: Templates > Blazor
- Tags: WebGPU, Blazor, Graphics, 3D, GPU, Rendering
- Supported VS versions: Visual Studio 2026+
- Target audience: Graphics programmers, game developers, visualization developers

**Template Format**: Visual Studio template format (.vstemplate)

## Implementation Phases

### Phase 1: Project Setup ✓
- [x] Create solution structure (.slnx)
- [x] Create four projects (Library, Tests, Template, Demo)
- [x] Copy .editorconfig from Jira.Api
- [x] Create copilot-instructions.md
- [x] Configure project references and dependencies
- [x] Add PanoramicData.Blazor NuGet reference (latest version)
- [x] Initialize Git repository (if not already initialized)
- [x] Create initial commit with project structure

### Phase 1.5: Test Infrastructure Setup ✓
- [x] Configure test project with xUnit and FluentAssertions (Note: xUnit 2.9.3 used instead of 3.0, FluentAssertions used instead of AwesomeAssertions due to .NET 10 compatibility)
- [x] Set up test helpers and utilities
- [x] Create mock/stub infrastructure for WebGPU interop testing
- [x] Configure code coverage tooling (Coverlet)
- [x] Create initial smoke tests (10 passing tests)

### Phase 2: Core Framework - JavaScript Interop ✓
- [x] Create webgpu-interop.js with minimal WebGPU initialization
- [x] Implement adapter/device acquisition
- [x] Implement canvas context setup
- [x] Add command buffer submission
- [x] Add shader compilation forwarding
- [x] Create C# interop wrapper (WebGpuJsInterop)
- [x] Create exception classes (PDWebGpuException, PDWebGpuDeviceException, PDWebGpuShaderCompilationException, PDWebGpuNotSupportedException)
- [x] Add comprehensive tests for exceptions and interop (21 new tests, all passing)

### Phase 3: Core Framework - C# Services ✓
- [x] Create IPDWebGpuService interface
- [x] Implement PDWebGpuService with DI registration
- [x] Add WebGPU device lifecycle management
- [x] Implement initialization and error handling
- [x] Add event system (DeviceReady, DeviceLost, Error)
- [x] Create dependency injection extension methods
- [x] Add comprehensive service tests (22 new tests, all passing)

### Phase 4: Core Framework - Base Components ✓
- [x] Create PDWebGpuComponentBase abstract class with event support
- [x] Implement OnFrame, OnResize, OnGpuReady, OnError events
- [x] Add input handling hooks (mouse, touch)
- [x] Create PDWebGpuContainer component with layered rendering
- [x] Create PDWebGpuCanvas component
- [x] Implement automatic screen resize handling
- [x] Add FrameRateMode enum (Variable, Fixed)
- [x] Implement render loop system with configurable frame rates
- [x] Add comprehensive component tests (17 new tests, all passing)

### Phase 5: Resource Management - Wrapper Classes ✓
- [x] Implement PDWebGpuBuffer (vertex, uniform, storage) with BufferType enum
- [x] Implement PDWebGpuTexture with TextureFormat enum (RGBA8Unorm, BGRA8Unorm, Depth24PlusStencil8, Depth32Float)
- [x] Implement PDWebGpuSampler with FilterMode and AddressMode enums
- [x] Implement PDWebGpuShader (WGSL text only)
- [x] Implement PDWebGpuPipeline (render + compute) with PipelineType enum
- [x] Implement PDWebGpuBindGroup
- [x] Implement PDWebGpuCommandEncoder with Finish() method
- [x] Add resource disposal and lifecycle management (IDisposable + IAsyncDisposable)
- [x] Add CreateShaderAsync method to service interface
- [x] Add comprehensive resource tests (27 new tests, all passing)

### Phase 6: Render Loop System ✓
- [x] Implement variable frame rate mode (render as fast as possible with VSync limit)
- [x] Implement fixed frame rate mode with configurable target FPS
- [x] Add accurate frame timing calculations (delta time, total time, frame number)
- [x] Implement render loop events dispatching (OnFrame with timing data)
- [x] Add pause behavior when tab inactive/window minimized (PauseWhenInactive property)
- [x] Add page visibility API integration via JavaScript interop
- [x] Create IVisibilityCallback interface for visibility change notifications
- [x] Add manual pause/resume methods (PauseRenderLoop, ResumeRenderLoop)
- [x] Add render loop status properties (IsRunning, IsPaused, CurrentFPS)
- [x] Implement automatic timing reset on resume to prevent large delta spikes
- [x] Add comprehensive render loop tests (11 new tests, all passing)

### Phase 7: Shader Management ✓
- [x] Implement WGSL text shader loading via service
- [x] Add shader validation with PDWebGpuShader.Validate() static method
- [x] Enhance PDWebGpuShader with ShaderCompilationInfo class
- [x] Create ShaderLoader utility class for shader management
- [x] Implement hot-reload support with ShaderReloaded event
- [x] Add shader name tracking and retrieval
- [x] Create WGSL language definition file for Monaco editor (wgsl.monarch.json)
- [x] Implement shader source code caching
- [x] Add comprehensive shader tests (18 new tests, all passing)

### Phase 8: Camera System ✓
- [x] Create PDWebGpuCameraBase abstract class with matrix calculation
- [x] Implement PDWebGpuOrbitCamera with target-based rotation
- [x] Implement PDWebGpuFirstPersonCamera with WASD movement and mouse look
- [x] Implement PDWebGpuOrthographicCamera with parallel projection for 2D
- [x] Add view/projection matrix calculation with caching
- [x] Add matrix dirty flags for efficient recalculation
- [x] Implement camera-specific movement methods (Rotate, Zoom, Pan, Move, Look)
- [x] Add configurable camera properties (FOV, speed, sensitivity, bounds)
- [x] Add comprehensive camera tests (26 new tests, all passing)

### Phase 9: Performance Metrics ✓
- [x] Create PDWebGpuPerformanceDisplayOptions with configuration properties
- [x] Create CornerPosition enum for overlay positioning
- [x] Implement PerformanceMetrics class for metric tracking and calculation
- [x] Implement PDWebGpuPerformanceDisplay component with overlay UI
- [x] Add FPS calculation with rolling average
- [x] Add frame time measurement in milliseconds
- [x] Add frame time usage % for fixed frame rate intervals
- [x] Add draw call counter support
- [x] Add triangle count tracking support
- [x] Support custom user-defined metrics via dictionary
- [x] Add periodic UI refresh with configurable interval
- [x] Add color-coded warnings (green/yellow/red for frame usage)
- [x] Add number formatting for large values (K/M suffixes)
- [x] Add comprehensive performance tests (21 new tests, all passing)

### Phase 10: Error Handling & Diagnostics
- [ ] Implement PDWebGpuDeviceException
- [ ] Implement PDWebGpuNotSupportedException
- [ ] Add WebGPU availability detection
- [ ] Create user-friendly error messages
- [ ] Add browser compatibility detection

### Phase 11: Demo Application
- [ ] Create Blazor WebAssembly demo app
- [ ] Integrate PDMonaco for WGSL editing (left pane)
- [ ] Configure PDMonaco with WGSL language definition
- [ ] Integrate PDWebGpuContainer for output (right pane)
- [ ] Implement split-view layout
- [ ] Add example WGSL shaders
- [ ] Implement shader hot-reload demonstration
- [ ] Add performance metrics toggle
- [ ] Create example scenes showcasing framework features

### Phase 12: Template Project
- [ ] Create Visual Studio project template structure (.vstemplate)
- [ ] Configure template metadata and parameters
- [ ] Create rotating cube + lighting example
- [ ] Add basic orbit camera setup
- [ ] Include example WGSL shaders
- [ ] Add commented performance display code
- [ ] Test template installation and project generation

### Phase 13: NuGet Package
- [ ] Configure NuGet package metadata
- [ ] Add package icon and README
- [ ] Set version to 1.0.0
- [ ] Configure package dependencies
- [ ] Build and test package locally
- [ ] Validate package contents

### Phase 14: Documentation
- [ ] Create README.md for repository
- [ ] Document API reference (XML comments)
- [ ] Create getting started guide
- [ ] Document camera usage
- [ ] Document shader management
- [ ] Create troubleshooting guide
- [ ] Add code examples for common scenarios

### Phase 15: Testing & Validation
- [ ] Run all unit tests and verify passing
- [ ] Run all integration tests and verify passing
- [ ] Verify code coverage meets targets
- [ ] Test on Chrome (WebGPU enabled)
- [ ] Test on Edge (WebGPU enabled)
- [ ] Test WebGPU unavailable scenarios
- [ ] Test shader compilation errors
- [ ] Test screen resizing
- [ ] Test both frame rate modes
- [ ] Test pause behavior (tab inactive/window minimized)
- [ ] Test all camera types
- [ ] Test performance metrics accuracy
- [ ] Test hot-reload functionality

### Phase 16: Marketplace Preparation
- [ ] Create 512x512 icon
- [ ] Capture screenshots/preview images
- [ ] Write marketplace description
- [ ] Prepare marketplace README
- [ ] Test template installation from VSIX
- [ ] Create release notes

### Phase 17: Publishing
- [ ] Publish NuGet package to nuget.org
- [ ] Create VSIX package for VS Marketplace
- [ ] Submit to Visual Studio Marketplace
- [ ] Monitor marketplace approval process
- [ ] Announce release

### Phase 18: Post-Launch
- [ ] Monitor feedback and issues
- [ ] Create GitHub repository (if applicable)
- [ ] Plan future enhancements
- [ ] Consider stereoscopic rendering for future version

## Technical Specifications

### .NET 10 & Blazor WebAssembly

**Runtime**: Browser WebAssembly runtime  
**AOT Compilation**: Recommended for production (optional)  
**Trimming**: Enabled for smaller bundle size  

### WebGPU API Coverage

**Minimum Features**:
- Render pipelines (vertex + fragment shaders)
- Compute pipelines
- Vertex, index, uniform, and storage buffers
- 2D textures and samplers
- Bind groups and layouts
- Command encoders and queues

**Future Considerations**:
- 3D textures
- Cube map textures
- Multi-sampling
- Depth/stencil buffers

### Browser Compatibility

**Supported Browsers** (WebGPU enabled):
- Chrome 113+
- Edge 113+
- Opera 99+

**Experimental Support**:
- Firefox (behind flag)
- Safari Technology Preview (behind flag)

**Graceful Degradation**:
- Detect WebGPU availability
- Provide clear error messages
- Suggest browser upgrades or flag enabling

### Performance Targets

**Initialization**: < 500ms to first frame  
**Frame Rate**: Maintain 60 FPS for moderate complexity scenes  
**Memory**: Efficient resource cleanup, no memory leaks  
**Bundle Size**: < 500KB for framework library (after compression)  

## Copilot Instructions

The `copilot-instructions.md` file should contain the following guidance for AI assistants working on this project:

### Purpose
This file provides instructions for GitHub Copilot and other AI coding assistants when working on the PanoramicData.Blazor.WebGpu framework.

### Critical Rules
1. **No Additional Markdown Files**: NEVER create additional markdown files unless explicitly requested by the developer
2. **Update Master Plan**: DO update MASTER_PLAN.md after significant progress
3. **Phase Management**: You MAY add, insert, or renumber phases in the Master Plan as needed

### Important Memories & Preferences to Update Into copilot-instructions.md

When you discover important patterns, conventions, or decisions during development, update this section:

**Naming Conventions**:
- All components: `PDWebGpu<Xxx>` pattern
- All services: `IPDWebGpu<Xxx>Service` pattern
- All exception types: `PDWebGpu<Xxx>Exception` pattern

**Code Style**:
- Follow .editorconfig from Jira.Api project
- Use C# 12 features (collection expressions, primary constructors, etc.)
- Prefer async/await patterns for all I/O operations

**Architecture Patterns**:
- Component-based design inheriting from PDWebGpuComponentBase
- Service injection for cross-cutting concerns
- Resource wrapper classes for all WebGPU objects
- Automatic disposal via IDisposable/IAsyncDisposable

**JavaScript Interop**:
- Minimize JS code; keep everything in single file
- Use IJSRuntime for all interop calls
- Wrap all JS calls in try-catch with meaningful C# exceptions

**Testing Strategy**:
- [To be determined during development]

**Performance Optimizations**:
- [To be discovered and documented during development]

**Common Pitfalls**:
- [To be documented as discovered during development]

### Development Workflow
1. Read MASTER_PLAN.md before starting work
2. Update phase checkboxes as tasks are completed
3. Add new discoveries to copilot-instructions.md
4. Keep all code comments concise and meaningful
5. Ensure XML documentation on public APIs

### Integration with PanoramicData.Blazor
- Reference existing PDMonaco component for shader editing
- Use existing keyboard handling services
- Follow established component patterns
- Maintain consistency with existing PD.Blazor conventions

## Success Criteria

### Minimum Viable Product (MVP)
- [ ] Developer can create WebGPU app using only C#
- [ ] Shaders can be provided as WGSL text or binary
- [ ] Full-screen layered rendering works correctly
- [ ] Screen resizing handled automatically
- [ ] Basic render loop with OnFrame events
- [ ] At least one camera type functional
- [ ] Performance metrics display available
- [ ] Template generates working project
- [ ] NuGet package installable and functional

### Quality Benchmarks
- [ ] Zero JavaScript knowledge required for developers
- [ ] Clear error messages for all failure modes
- [ ] 60 FPS performance for demo app
- [ ] < 2 second load time for demo app
- [ ] Hot-reload works reliably in development
- [ ] Comprehensive XML documentation
- [ ] Working examples for all major features
- [ ] Unit test coverage > 80%
- [ ] All integration tests passing

### Marketplace Launch Criteria
- [ ] Professional icon and screenshots
- [ ] Complete getting started guide
- [ ] 3+ working example projects
- [ ] Tested on Windows, macOS, Linux (Chrome/Edge)
- [ ] All Phase 1-17 tasks completed
- [ ] Positive feedback from beta testers (if applicable)

## Future Enhancements (Post-V1)

### Potential Features
- Stereoscopic rendering (VR/AR)
- Compute shader examples and utilities
- Post-processing effects framework
- Particle system utilities
- Physics integration helpers
- Texture loading utilities (common formats)
- 3D model loading (glTF, OBJ)
- Scene graph system
- Advanced lighting models
- Shadow mapping utilities
- Reflection/refraction helpers

### Community Feedback
[To be populated based on user feedback after launch]

---

## Document Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0.0   | 2025-01-23 | Initial master plan created | AI Assistant |
| 1.1.0   | 2025-01-23 | Updated based on clarifications: removed binary shader support, added test project, clarified canvas management, added pause behavior, updated copyright to 2025, removed GPU memory metrics, updated template format to .vstemplate, added WGSL language file support | AI Assistant |
| 1.2.0   | 2025-01-23 | Phase 1 completed: Solution structure created with 4 projects, .editorconfig copied, copilot-instructions.md created, project references configured, PanoramicData.Blazor v9.0.328 added, Git repository initialized | AI Assistant |
| 1.3.0   | 2025-01-23 | Phase 1.5 completed: Test infrastructure setup with xUnit 2.9.3, FluentAssertions 8.8.0, Moq 4.20.72, Coverlet code coverage, created TestBase, MockJSRuntime, MockWebGpuDevice, TestData, TestHelpers, and 10 passing smoke tests. Created PDWebGpuComponentBase placeholder class in main library | AI Assistant |
| 1.4.0   | 2025-01-23 | Phase 2 completed: JavaScript interop layer created with webgpu-interop.js (346 lines), WebGpuJsInterop C# wrapper, 4 exception classes with detailed error handling (PDWebGpuException, PDWebGpuDeviceException, PDWebGpuShaderCompilationException, PDWebGpuNotSupportedException). Added 21 comprehensive tests. Total: 31 tests passing. Branch renamed from master to main | AI Assistant |
| 1.5.0   | 2025-01-23 | Phase 3 completed: Core service layer created with IPDWebGpuService interface and PDWebGpuService implementation. Features include device lifecycle management, initialization with error handling, event system (DeviceReady, DeviceLost, Error), dependency injection extensions (AddPDWebGpu). Added 22 comprehensive tests covering all service functionality. Total: 53 tests passing | AI Assistant |
| 1.6.0   | 2025-01-23 | Phase 4 completed: Core component infrastructure created with enhanced PDWebGpuComponentBase (event callbacks, input handling), PDWebGpuContainer (layered rendering with background WebGPU canvas and foreground HTML), PDWebGpuCanvas component, FrameRateMode enum, render loop system with Variable/Fixed frame rates. Added 17 comprehensive tests. Total: 70 tests passing | AI Assistant |
| 1.6.1   | 2025-01-23 | Refactoring: Converted all Razor components to code-behind pattern. Created PDWebGpuContainer.razor.cs and PDWebGpuCanvas.razor.cs with all logic, parameters, and services. Updated copilot-instructions.md with code-behind pattern guidelines. All 70 tests still passing | AI Assistant |
| 1.7.0   | 2025-01-23 | Phase 5 completed: Resource management wrapper classes created - PDWebGpuBuffer (with BufferType enum), PDWebGpuTexture (with TextureFormat enum), PDWebGpuSampler (with FilterMode/AddressMode enums), PDWebGpuShader, PDWebGpuPipeline (with PipelineType enum), PDWebGpuBindGroup, PDWebGpuCommandEncoder. All resources implement IDisposable + IAsyncDisposable. Added CreateShaderAsync to service. Added 27 comprehensive tests. Total: 97 tests passing | AI Assistant |
| 1.8.0   | 2025-01-23 | Phase 6 completed: Enhanced render loop system with page visibility API integration, IVisibilityCallback interface for visibility change notifications, PauseWhenInactive functionality with automatic pause/resume based on tab visibility, manual pause/resume methods (PauseRenderLoop, ResumeRenderLoop), render loop status properties (IsRunning, IsPaused, CurrentFPS), automatic timing reset on resume to prevent large delta spikes. Enhanced webgpu-interop.js with visibility change listeners. Added 11 comprehensive render loop tests. Total: 108 tests passing | AI Assistant |
| 1.9.0   | 2025-01-23 | Phase 7 completed: Shader management system created with ShaderCompilationInfo class for validation metadata, enhanced PDWebGpuShader with Validate() static method for WGSL syntax checking, ShaderLoader utility class for hot-reload support with ShaderReloaded event, shader name tracking and source code caching, WGSL language definition for Monaco editor (wgsl.monarch.json with complete syntax highlighting including keywords, types, attributes, built-ins, and functions). Added 18 comprehensive shader management tests. Total: 126 tests passing | AI Assistant |
| 1.10.0  | 2025-01-23 | Phase 8 completed: Camera system created with PDWebGpuCameraBase abstract class providing view/projection matrix calculation with caching and dirty flags. Three camera types implemented: PDWebGpuOrbitCamera (target-based with rotation/zoom), PDWebGpuFirstPersonCamera (WASD movement with mouse look), PDWebGpuOrthographicCamera (parallel projection for 2D with pan/zoom). All cameras support configurable properties (FOV, aspect ratio, clipping planes, speed, sensitivity, bounds). Added 26 comprehensive camera tests. Total: 152 tests passing | AI Assistant |
| 1.11.0  | 2025-01-23 | Phase 9 completed: Performance metrics system created with PDWebGpuPerformanceDisplayOptions (configurable overlay settings), CornerPosition enum (4 positions), PerformanceMetrics class (tracks FPS, frame time, draw calls, triangles with rolling average and usage calculations), PDWebGpuPerformanceDisplay component (real-time overlay with color-coded warnings, custom metrics support, number formatting). Metrics include FPS calculation, average frame time, frame time usage %, draw calls, triangle count, and custom user metrics. Added 21 comprehensive tests. Total: 173 tests passing | AI Assistant |

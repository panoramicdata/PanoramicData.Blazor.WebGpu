using System.Diagnostics.CodeAnalysis;

// Suppress BL0005 warnings in test project - we intentionally set component parameters directly in tests
[assembly: SuppressMessage("Usage", "BL0005:Component parameter should not be set outside of its component", Justification = "Test code intentionally sets parameters directly")]

global using Xunit;

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1000:Test classes must be public", Justification = "In some cases we need to test the discovery process with real samples represented as private test classes.")]
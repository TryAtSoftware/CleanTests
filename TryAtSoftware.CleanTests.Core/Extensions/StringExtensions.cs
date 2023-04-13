namespace TryAtSoftware.CleanTests.Core.Extensions;

using TryAtSoftware.Extensions.Collections;

internal static class StringExtensions
{
    internal static string SurroundWith(this string text, string prefix, string suffix) => string.Join(string.Empty, new[] { prefix, text, suffix }.IgnoreNullOrWhitespaceValues());
}
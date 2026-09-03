namespace Scase.Application;

internal static class Against
{
    internal static void Contains(string value, IEnumerable<string> collection, string paramName)
    {
        if (!collection.Contains(value))
        {
            throw new ArgumentException($"Expected '{value}' to be in the collection.", paramName);
        }
    }
}

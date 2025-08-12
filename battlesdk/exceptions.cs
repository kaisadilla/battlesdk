namespace battlesdk;

public class InitializationException (string message, Exception? exception = null)
    : Exception(message, exception);

public class RendererException (string message, Exception? exception = null)
    : Exception(message, exception);

public class RegistryException (string message, Exception? exception = null)
    : Exception(message, exception);

public static class Exceptions {
    public static RegistryException InvalidRegistryIndex<T> (
        Collection<T> col, int index
    ) where T : IIdentifiable {
        return new RegistryException(
            $"Index '{index}' is outside the bounds of the collection, which" +
            $"contains {col.Count} elements."
        );
    }
}
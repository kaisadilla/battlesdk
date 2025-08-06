namespace battlesdk;

public class InitializationException (string message, Exception? exception = null)
    : Exception(message, exception);
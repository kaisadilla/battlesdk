namespace battlesdk.input;
public interface IInputListener {
    /// <summary>
    /// If true, while this listener is active, input listeners with a lower
    /// priority will be ignored.
    /// </summary>
    bool BlockOtherInput { get; }

    /// <summary>
    /// Processes input received this frame.
    /// </summary>
    void HandleInput();

    /// <summary>
    /// This method is called when a newer input source blocks this input,
    /// if this input wasn't blocked until now.
    /// </summary>
    void OnInputBlocked();
}

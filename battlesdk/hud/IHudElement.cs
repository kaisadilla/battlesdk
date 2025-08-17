namespace battlesdk.hud;
public interface IHudElement {
    /// <summary>
    /// True if this element has been closed already.
    /// </summary>
    bool IsClosed { get; }
    /// <summary>
    /// Triggers when this element is closed.
    /// </summary>
    event EventHandler<EventArgs>? OnClose;
    
    void TakeControl();
    void Update();
    void Draw();
    void HandleInput();
    void Close();
}

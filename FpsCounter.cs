namespace battlesdk;

public class FpsCounter {
    /// <summary>
    /// True if this counter was updated in the last frame.
    /// </summary>
    public bool UpdatedLastFrame { get; private set; } = false;
    /// <summary>
    /// The number of frames per second (FPS) currently registered.
    /// </summary>
    public int Fps { get; private set; } = 0;
    /// <summary>
    /// The latency (in seconds) currently registered. Latency is equal to the
    /// amount of time that has passed between the last two frames.
    /// </summary>
    public float Latency { get; private set; } = 0.0f;

    private float _updateTime = 0.0f;

    private float _timeSinceLastUpdate = 0.0f;
    private int _internalFpsCounter = 0;

    /// <summary>
    /// Sets the time it takes for this counter to update its information.
    /// A longer time means the counter is updated less often, but its numbers
    /// are more accurate and fluctuate less.
    /// </summary>
    /// <param name="updateTime">The delay (in seconds) before updates for this
    /// counter.</param>
    public void SetUpdateTime (float updateTime) {
        _updateTime = updateTime;
    }

    /// <summary>
    /// Update this counter. This should be called every frame.
    /// </summary>
    public void Count () {
        _timeSinceLastUpdate += Time.DeltaTime;
        _internalFpsCounter++;

        if (_timeSinceLastUpdate > _updateTime) {
            Fps = (int)(_internalFpsCounter / _timeSinceLastUpdate);
            Latency = Time.DeltaTime / _internalFpsCounter;

            _timeSinceLastUpdate -= _updateTime;
            _timeSinceLastUpdate %= 0.1f;
            _internalFpsCounter = 0;

            UpdatedLastFrame = true;
        }
        else {
            UpdatedLastFrame = false;
        }
    }
}

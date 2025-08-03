namespace battlesdk.animations;

public class UpDownAnimation {
    private int _start;
    private int _end;
    private float _duration;
    private float _progress = 0;

    public int Value {
        get {
            float interp = _progress < 0.5f ? (_progress * 2) : ((1f - _progress) * 2);
            return (int)MathF.Round(_start.Lerp(_end, interp));
        }
    }

    public UpDownAnimation (int start, int end, float duration) {
        _start = start;
        _end = end;
        _duration = duration;
    }

    public void Update () {
        _progress += Time.DeltaTime / _duration;

        _progress %= 1f;
    }
}

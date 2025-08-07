global using CoroutineTask = System.Collections.Generic.IEnumerator<battlesdk.WaitInstruction?>;

namespace battlesdk;
public static class CoroutineRuntime {
    private static readonly List<Coroutine> _coroutines = [];

    public static void Add (Coroutine coroutine) {
        _coroutines.Add(coroutine);
    }

    public static void Update () {
        for (int i = _coroutines.Count - 1; i >= 0; --i) {
            var c = _coroutines[i];

            bool ended = c.Update();

            if (ended) _coroutines.RemoveAt(i);
        }
    }
}

public class Coroutine {
    private TaskCompletionSource _tcs = new();

    private CoroutineTask _routine;
    private WaitInstruction? _wait;

    /// <summary>
    /// Creates a new coroutine. This will NOT start the coroutine. To start
    /// a coroutine right away, use <see cref="Start(CoroutineTask)"/> instead.
    /// </summary>
    /// <param name="routine"></param>
    public Coroutine (CoroutineTask routine) {
        _routine = routine;
    }

    /// <summary>
    /// Starts a new coroutine with the routine given, and returns said
    /// coroutine.
    /// </summary>
    /// <param name="routine">The routine to execute.</param>
    /// <returns>The coroutine that is being ran.</returns>
    public static Coroutine Start (CoroutineTask routine) {
        var cor = new Coroutine(routine);

        CoroutineRuntime.Add(cor);

        return cor;
    }

    /// <summary>
    /// Executes this coroutine. Calling this when this coroutine has already
    /// been executed will result in undefined behavior.
    /// </summary>
    public void Execute () {
        CoroutineRuntime.Add(this);
    }

    /// <summary>
    /// Advances the coroutine, and returns true if the coroutine has ended.
    /// </summary>
    /// <returns></returns>
    public bool Update () {
        if (_wait is not null) {
            if (_wait.IsComplete(Time.DeltaTime) == false) return false;
        }

        bool hasNext = _routine.MoveNext();
        if (hasNext == false) {
            _tcs.TrySetResult();
            return true;
        }

        var yield = _routine.Current;
        if (yield is WaitInstruction wait) {
            _wait = wait;
        }

        return false;
    }

    /// <summary>
    /// Returns a Task that will be completed when this coroutine is finished.
    /// </summary>
    public Task WaitComplete () {
        return _tcs.Task;
    }
}

public abstract class WaitInstruction {
    public abstract bool IsComplete (float deltaTime);
}

public class WaitForSeconds (float seconds) : WaitInstruction {
    /// <summary>
    /// The amount of time, in seconds, until this Wait object is complete.
    /// </summary>
    private float _remaining = seconds;

    /// <summary>
    /// Returns true if this Wait is completed.
    /// </summary>
    /// <param name="deltaTime">The amount of time since the last check.</param>
    /// <returns></returns>
    public override bool IsComplete (float deltaTime) {
        _remaining -= deltaTime;
        return _remaining <= 0;
    }
}
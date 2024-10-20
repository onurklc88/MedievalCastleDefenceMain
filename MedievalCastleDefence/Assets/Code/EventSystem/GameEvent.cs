using System;

public class GameEvent
{
    private event Action _action = delegate { };

    public void Invoke()
    {
        _action.Invoke();
    }

    public void AddListener(Action listener)
    {
        _action += listener;
    }

    public void RemoveListener(Action listener)
    {
        _action -= listener;
    }
}
public class GameEvent<T>
{
    private event Action<T> _action = delegate { };

    public void Invoke(T param)
    {
        _action.Invoke(param);
    }

    public void AddListener(Action<T> listener)
    {
        _action += listener;
    }

    public void RemoveListener(Action<T> listener)
    {
        _action -= listener;
    }
}
 public class GameEvent<T1, T2>
 {
    private event Action<T1, T2> _action = delegate { };

    public void Invoke(T1 param1, T2 param2)
    {
        _action.Invoke(param1, param2);
    }

    public void AddListener(Action<T1, T2> listener)
    {
        _action += listener;
    }

    public void RemoveListener(Action<T1, T2> listener)
    {
        _action -= listener;
    }
 }





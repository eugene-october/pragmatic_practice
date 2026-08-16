class Blender
{
    // Design by contract (DBC) implementation
    //
    // The contract:
    // 10 speed settings. 0 means off. 10 operating speeds + off in total
    // If empty - can't operate
    // You can set speed only one unit at time (that is, 0 -> 1, 1->2, not 0->2)

    private int _speed;
    private bool _full;

    public int GetSpeed()
    {
        return _speed;
    }

    public void SetSpeed(int speed)
    {
        if (speed < 0 || speed > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(speed), speed, "should be within range [0..10]");
        }

        if (Math.Abs(_speed - speed) > 1)
        {
            throw new ArgumentException("should not differ from the Speed more than 1 unit", nameof(speed));
        }

        if (speed > 0 && !IsFull())
        {
            throw new InvalidOperationException("Cannot start with empty blender. Call `.Fill()` method first");
        }

        _speed = speed;
    }

    public bool IsFull()
    {
        return _full;
    }

    public void Fill()
    {
        if (IsFull())
        {
            throw new InvalidOperationException("Already full");
        }

        _full = true;
    }

    public void Empty()
    {
        if (!IsFull())
        {
            throw new InvalidOperationException("Already empty");
        }

        if (_speed > 0)
        {
            throw new InvalidOperationException("Can't operate if empty. Turn off first");
        }

        _full = false;
    }
}

namespace Runtime;

public unsafe struct GCFrame
{
    public readonly int Count;
    public readonly GCFrame* Previous;

    private readonly RuntimeObject** _roots;

    public readonly RuntimeObject*** Roots
    {
        get
        {
            fixed (RuntimeObject*** ptr = &_roots)
                return ptr;
        }
    }
}

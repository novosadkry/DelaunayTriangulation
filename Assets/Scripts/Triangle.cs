using System;

[Serializable]
public struct Triangle
{
    public Triangle(Point a, Point b, Point c)
    {
        this.a = a;
        this.b = b;
        this.c = c;
    }

    public Point a;
    public Point b;
    public Point c;

    public Point this[int idx] => idx switch
    {
        0 => a,
        1 => b,
        2 => c,
        _ => throw new IndexOutOfRangeException()
    };

    public Point[] ToArray()
    {
        return new[] { a, b, c };
    }
}

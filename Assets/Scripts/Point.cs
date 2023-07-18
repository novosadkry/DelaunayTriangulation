using System;
using UnityEngine;

[Serializable]
public struct Point
{
    public int Index { get; set; }
    public Vector3 Position { get; set; }

    public Point(int index, Vector3 position)
    {
        Index = index;
        Position = position;
    }

    public bool Equals(Point other)
    {
        return Position.Equals(other.Position);
    }

    public override bool Equals(object obj)
    {
        return obj is Point other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}

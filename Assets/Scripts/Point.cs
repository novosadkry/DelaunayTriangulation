using System;
using UnityEngine;

[Serializable]
public struct Point
{
    public int Index { get; set; }
    public Vector3 Position { get; set; }

    public Point(Vector3 position, int index = -1)
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

    public static implicit operator Vector3(Point p)
    {
        return p.Position;
    }

    public static implicit operator Vector2(Point p)
    {
        return p.Position;
    }

    public static Vector3 operator +(Point p1, Point p2)
    {
        return ((Vector3)p1) + ((Vector3)p2);
    }

    public static Vector3 operator -(Point p1, Point p2)
    {
        return ((Vector3)p1) - ((Vector3)p2);
    }
}

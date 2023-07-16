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
}

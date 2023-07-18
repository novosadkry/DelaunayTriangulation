using System;
using System.Linq;
using UnityEngine;

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

    public Edge[] GetEdges()
    {
        return new[]
        {
            new Edge(a, b),
            new Edge(c, b),
            new Edge(c, a),
        };
    }

    public void CorrectWinding()
    {
        if (IsCounterClockwise())
            (b, c) = (c, b);
    }

    private bool IsCounterClockwise()
    {
        float result = (b.Position.x - a.Position.x) * (c.Position.y - a.Position.y) - (c.Position.x - a.Position.x) * (b.Position.y - a.Position.y);
        return result > 0;
    }

    public bool ContainsEdge(Edge edge)
    {
        return GetEdges().Contains(edge);
    }

    public Vector2 ComputeCircumCentre()
    {
        // Given that all verticies on a triangle must touch the outside of the CircumCircle.
        // We can deduce that DA = DB = DC (Distances from each vertex to the center are equal).
        // Formulae reference - https://en.wikipedia.org/wiki/Circumscribed_circle#Circumcircle_equations

        Vector2 A = a.Position;
        Vector2 B = b.Position;
        Vector2 C = c.Position;
        Vector2 SqrA = new Vector2(Mathf.Pow(A.x, 2f), Mathf.Pow(A.y, 2f));
        Vector2 SqrB = new Vector2(Mathf.Pow(B.x, 2f), Mathf.Pow(B.y, 2f));
        Vector2 SqrC = new Vector2(Mathf.Pow(C.x, 2f), Mathf.Pow(C.y, 2f));

        float D = (A.x * (B.y - C.y) + B.x * (C.y - A.y) + C.x * (A.y - B.y)) * 2f;
        float x =
            ((SqrA.x + SqrA.y) * (B.y - C.y) + (SqrB.x + SqrB.y) * (C.y - A.y) + (SqrC.x + SqrC.y) * (A.y - B.y)) / D;
        float y =
            ((SqrA.x + SqrA.y) * (C.x - B.x) + (SqrB.x + SqrB.y) * (A.x - C.x) + (SqrC.x + SqrC.y) * (B.x - A.x)) / D;
        return new Vector2(x, y);
    }

    public float ComputeCircumRadius()
    {
        // Radius is the distance from any vertex to the CircumCentre
        Vector2 circumCentre = ComputeCircumCentre();
        return Vector2.Distance(circumCentre, a.Position);
    }
}

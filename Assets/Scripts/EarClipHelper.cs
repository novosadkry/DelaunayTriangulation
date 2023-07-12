using System.Collections.Generic;
using UnityEngine;

public class EarClipHelper
{
    // Source: https://stackoverflow.com/a/9755252
    public static bool IsInsideTriangle(Triangle triangle, Point point)
    {
        Vector2 p = point.transform.position;
        Vector2 a = triangle.a.transform.position;
        Vector2 b = triangle.b.transform.position;
        Vector2 c = triangle.c.transform.position;

        if (p == a || p == b || p == c)
            return false;

        var as_x = p.x - a.x;
        var as_y = p.y - a.y;

        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab)
            return false;

        if ((c.x - b.x) * (p.y - b.y) - (c.y - b.y) * (p.x - b.x) > 0 != s_ab)
            return false;

        return true;
    }

    public static bool IsConvex(LinkedListNode<Point> b, LinkedList<Point> polygon)
    {
        var a = b.Previous ?? polygon.Last;
        var c = b.Next ?? polygon.First;

        var ba = (Vector2) a.Value.transform.position - (Vector2) b.Value.transform.position;
        var bc = (Vector2) c.Value.transform.position - (Vector2) b.Value.transform.position;

        var inner = Vector2.SignedAngle(ba, bc);
        if (inner < 0) inner += 360;

        return inner < Mathf.PI * Mathf.Rad2Deg;
    }

    public static bool TriangleContainsPoints(Triangle triangle, LinkedList<Point> polygon)
    {
        var b = polygon.First;

        while (b != null)
        {
            if (!IsConvex(b, polygon))
            {
                if (IsInsideTriangle(triangle, b.Value))
                    return true;
            }

            b = b.Next;
        }

        return false;
    }

    public static List<int> Solve(List<Point> points)
    {
        var result = new List<int>();
        var polygon = new LinkedList<Point>(points);

        while (polygon.Count >= 3)
        {
            var b = polygon.First;
            bool hasRemovedEar = false;

            for (int i = 0; i < polygon.Count; i++)
            {
                var a = b.Previous ?? polygon.Last;
                var c = b.Next ?? polygon.First;

                var triangle = new Triangle(a.Value, b.Value, c.Value);

                if (IsConvex(b, polygon))
                {
                    if (!TriangleContainsPoints(triangle, polygon))
                    {
                        polygon.Remove(b);
                        result.Add(a.Value.Index);
                        result.Add(b.Value.Index);
                        result.Add(c.Value.Index);

                        hasRemovedEar = true;
                        break;
                    }
                }

                b = c; // Move to next vertex
            }

            if (!hasRemovedEar)
            {
                Debug.LogError("Triangulation error");
                return result;
            }
        }

        return result;
    }
}

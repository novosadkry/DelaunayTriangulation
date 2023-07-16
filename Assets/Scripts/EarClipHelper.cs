using System.Collections.Generic;
using UnityEngine;

public class EarClipHelper
{
    // Source: https://stackoverflow.com/a/9755252
    private static bool IsInsideTriangle(Triangle triangle, Point point)
    {
        Vector2 p = point.Position;
        Vector2 a = triangle.a.Position;
        Vector2 b = triangle.b.Position;
        Vector2 c = triangle.c.Position;

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

    private static bool IsConvex(LinkedListNode<Point> b, LinkedList<Point> polygon)
    {
        var a = b.Previous ?? polygon.Last;
        var c = b.Next ?? polygon.First;

        var ba = (Vector2) a.Value.Position - (Vector2) b.Value.Position;
        var bc = (Vector2) c.Value.Position - (Vector2) b.Value.Position;

        var inner = Vector2.SignedAngle(ba, bc);
        if (inner < 0) inner += 360;

        return inner < Mathf.PI * Mathf.Rad2Deg;
    }

    private static LinkedList<Point> ProjectPolygon2D(LinkedList<Point> polygon)
    {
        var projected = new LinkedList<Point>();
        var bNode = polygon.First;

        for (int i = 0; i < polygon.Count; i++)
        {
            var aNode = bNode.Previous ?? polygon.Last;
            var cNode = bNode.Next ?? polygon.First;

            var a = aNode.Value.Position;
            var b = bNode.Value.Position;
            var c = cNode.Value.Position;

            var n = Vector3.Cross(a - b, c - b);
            n *= IsConvex(bNode, polygon) ? 1 : -1;

            var q = Quaternion.FromToRotation(n, Vector3.forward);

            var bProjected = (Point)bNode.Value.Clone();
            bProjected.Position = q * b;
            projected.AddLast(bProjected);

            bNode = cNode;
        }

        return projected;
    }

    private static bool TriangleContainsPoints(Triangle triangle, LinkedList<Point> polygon)
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

    private static LinkedList<Point> CombineHullWithHole(LinkedList<Point> hull, LinkedList<Point> hole)
    {
        var hullProjected = ProjectPolygon2D(hull);
        var holeProjected = ProjectPolygon2D(hole);

        var polygon = new LinkedList<Point>(hullProjected);

        var bridgeNode = GetHoleBridgePoint(polygon.Last.Value, hole);
        for (int i = 0; i <= hole.Count; i++)
        {
            polygon.AddLast(bridgeNode!.Value);
            bridgeNode = bridgeNode.Previous ?? holeProjected.Last;
        }

        polygon.AddLast(hullProjected.Last.Value);

        return polygon;
    }

    private static LinkedListNode<Point> GetHoleBridgePoint(Point hullPoint, LinkedList<Point> hole)
    {
        var minDist = float.MaxValue;
        LinkedListNode<Point> closest = null;

        var node = hole.First;
        for (int i = 0; i < hole.Count; i++)
        {
            var holePoint = node!.Value;
            var dist = (holePoint.Position - hullPoint.Position).sqrMagnitude;

            if (dist < minDist)
            {
                minDist = dist;
                closest = node;
            }

            node = node.Next;
        }

        return closest;
    }

    public static List<int> Solve(List<Point> hull, List<Point> hole)
    {
        var result = new List<int>();
        var polygon = CombineHullWithHole(
            new LinkedList<Point>(hull),
            new LinkedList<Point>(hole));

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

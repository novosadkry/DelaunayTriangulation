using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EarClipHelper
{
    // Source: https://stackoverflow.com/a/9755252
    private static bool IsInsideTriangle(Triangle triangle, Point point)
    {
        Vector2 p = point;
        Vector2 a = triangle.a;
        Vector2 b = triangle.b;
        Vector2 c = triangle.c;

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

    private static bool IsConvex(Triangle triangle)
    {
        var ba = triangle.a - triangle.b;
        var bc = triangle.c - triangle.b;

        var inner = Vector2.SignedAngle(ba, bc);
        if (inner < 0) inner += 360;

        return inner < Mathf.PI * Mathf.Rad2Deg;
    }

    private static LinkedList<Point> ProjectPolygon2D(LinkedList<Point> polygon, Vector3 normal)
    {
        var projected = new LinkedList<Point>();

        var bNode = polygon.First;
        for (int i = 0; i < polygon.Count; i++)
        {
            var b = bNode.Value.Position;
            var q = Quaternion.FromToRotation(normal, Vector3.back);

            var bProjected = bNode.Value;
            bProjected.Position = q * b;
            projected.AddLast(bProjected);

            bNode = bNode.Next ?? polygon.First;
        }

        return projected;
    }

    private static Vector3 GetPolygonNormal(LinkedList<Point> polygon)
    {
        var normal = Vector3.zero;
        var points = polygon.ToArray();

        for (var i = 0; i < points.Length; i++)
        {
            var j = (i + 1) % points.Length;

            var a = points[i].Position;
            var b = points[j].Position;

            normal.x += (a.y - b.y) * (a.z + b.z);
            normal.y += (a.z - b.z) * (a.x + b.x);
            normal.z += (a.x - b.x) * (a.y + b.y);
        }

        return normal;
    }

    private static bool TriangleContainsPoints(Triangle triangle, LinkedList<Point> polygon)
    {
        var b = polygon.First;

        while (b != null)
        {
            var a = b.Previous ?? polygon.Last;
            var c = b.Next ?? polygon.First;
            var tri = new Triangle(a.Value, b.Value, c.Value);

            if (!IsConvex(tri))
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
        var polygon = new LinkedList<Point>(hull);
        var normal = GetPolygonNormal(polygon);

        foreach (var point in hole)
            polygon.AddLast(point);

        PreprocessPoints(polygon);

        var projected = ProjectPolygon2D(polygon, normal);
        polygon.Clear();

        // Add only projected hull points
        var hullNode = projected.First;
        for (int i = 0; i < hull.Count; i++)
        {
            polygon.AddLast(hullNode!.Value);
            hullNode = hullNode.Next;
        }

        // At this point, hullNode contains first hole node
        // so we take a step back
        hullNode = hullNode!.Previous!;

        // Get best bridge node and connect hole vertices
        var bridgeNode = GetHoleBridgePoint(hullNode);
        for (int i = 0; i <= hole.Count; i++)
        {
            polygon.AddLast(bridgeNode!.Value);
            bridgeNode = bridgeNode.Next ?? hullNode.Next;
        }

        polygon.AddLast(hullNode.Value);

        return polygon;
    }

    private static LinkedListNode<Point> GetHoleBridgePoint(LinkedListNode<Point> hullNode)
    {
        var minDist = float.MaxValue;
        LinkedListNode<Point> closest = null;

        var holeNode = hullNode.Next;
        while (holeNode != null)
        {
            var dist = (holeNode.Value - hullNode.Value).sqrMagnitude;

            if (dist < minDist)
            {
                minDist = dist;
                closest = holeNode;
            }

            holeNode = holeNode.Next;
        }

        return closest;
    }

    private static void PreprocessPoints(LinkedList<Point> polygon)
    {
        int index = 0;

        var node = polygon.First;
        while (node != null)
        {
            var p = node.Value;

            p.Index = index++;
            node.Value = p;

            node = node.Next;
        }
    }

    public static TriangulationResult Solve(List<Point> hull, List<Point> hole)
    {
        var polygon = CombineHullWithHole(
            new LinkedList<Point>(hull),
            new LinkedList<Point>(hole));

        var vertices = hull.Concat(hole)
            .Select(x => x.Position)
            .ToArray();

        var uvs = polygon
            .Select(x => (Vector2)x)
            .Distinct()
            .ToArray();

        var triangles = new List<int>();

        while (polygon.Count >= 3)
        {
            var b = polygon.First;
            bool hasRemovedEar = false;

            for (int i = 0; i < polygon.Count; i++)
            {
                var a = b.Previous ?? polygon.Last;
                var c = b.Next ?? polygon.First;
                var tri = new Triangle(a.Value, b.Value, c.Value);

                if (IsConvex(tri))
                {
                    if (!TriangleContainsPoints(tri, polygon))
                    {
                        polygon.Remove(b);

                        triangles.Add(a.Value.Index);
                        triangles.Add(b.Value.Index);
                        triangles.Add(c.Value.Index);

                        hasRemovedEar = true;
                        break;
                    }
                }

                b = c; // Move to next vertex
            }

            if (!hasRemovedEar)
            {
                Debug.LogError("Triangulation error");
                break;
            }
        }

        return new TriangulationResult(vertices, uvs, triangles.ToArray());
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EarClipHelper
{
    // Source: https://stackoverflow.com/a/9755252
    private static bool IsInsideTriangle(Triangle triangle, Point point)
    {
        var p = (Vector2) point;
        var a = (Vector2) triangle.a;
        var b = (Vector2) triangle.b;
        var c = (Vector2) triangle.c;

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

    private static List<Point> ProjectPolygon2D(List<Point> polygon, Vector3 normal)
    {
        var projected = new List<Point>(polygon.Count);
        var q = Quaternion.FromToRotation(normal, Vector3.back);

        for (int i = 0; i < polygon.Count; i++)
        {
            var p = polygon[i];
            p.Position = q * p.Position;
            projected.Add(p);
        }

        return projected;
    }

    private static Vector3 GetPolygonNormal(List<Point> polygon)
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

    private static bool TriangleContainsPoints(Triangle triangle, List<Point> polygon)
    {
        for (int i = polygon.Count - 1; i >= 0; i--)
        {
            var a = polygon[i];
            var b = polygon[(i + 1) % polygon.Count];
            var c = polygon[(i + 2) % polygon.Count];

            var tri = new Triangle(a, b, c);

            if (!IsConvex(tri))
            {
                if (IsInsideTriangle(triangle, b))
                    return true;
            }
        }

        return false;
    }

    private static void PreprocessPoints(List<Point> polygon)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            var p = polygon[i];
            p.Index = i;
            polygon[i] = p;
        }
    }

    private static List<Point> CombineHullWithHole(List<Point> hull, List<Point> hole)
    {
        var polygon = new List<Point>(hull);
        var normal = GetPolygonNormal(polygon);

        foreach (var point in hole)
            polygon.Add(point);

        PreprocessPoints(polygon);

        var projected = ProjectPolygon2D(polygon, normal);
        polygon.Clear();

        // Add only projected hull points
        for (int i = 0; i < hull.Count; i++)
            polygon.Add(projected[i]);

        // Skip bridging if there is no hole
        if (hole.Count == 0)
            return polygon;

        // Get best bridge node and connect hole vertices
        var bridgeIndex = GetHoleBridgePoint(projected[hull.Count - 1], projected, hull.Count, hole.Count);

        for (int i = 0; i <= hole.Count; i++)
        {
            polygon.Add(projected[bridgeIndex++]);
            bridgeIndex = bridgeIndex < projected.Count
                ? bridgeIndex : projected.Count - hole.Count;
        }

        polygon.Add(projected[hull.Count - 1]);
        Debug.Log(string.Join(",", polygon.Select(x => x.Index)));
        return polygon;
    }

    private static int GetHoleBridgePoint(Point fromPoint, List<Point> polygon, int polygonStart, int polygonCount)
    {
        int closest = -1;
        var minDist = float.MaxValue;

        for (int i = polygonStart; i < polygonStart + polygonCount; i++)
        {
            var toPoint = polygon[i];
            var dist = (toPoint - fromPoint).sqrMagnitude;

            if (dist < minDist)
            {
                minDist = dist;
                closest = i;
            }
        }

        return closest;
    }

    public static TriangulationResult Solve(List<Point> hull, List<Point> hole)
    {
        var polygon = CombineHullWithHole(hull, hole);
        var triangles = new List<int>();

        var vertices = hull.Concat(hole)
            .Select(x => x.Position)
            .ToArray();

        var uvs = polygon
            .Select(x => (Vector2)x)
            .Distinct()
            .ToArray();

        while (polygon.Count >= 3)
        {
            bool hasRemovedEar = false;

            for (int i = polygon.Count - 1; i >= 0; i--)
            {
                var a = polygon[i];
                var b = polygon[(i + 1) % polygon.Count];
                var c = polygon[(i + 2) % polygon.Count];

                var tri = new Triangle(a, b, c);

                if (IsConvex(tri))
                {
                    if (!TriangleContainsPoints(tri, polygon))
                    {
                        polygon.RemoveAt((i + 1) % polygon.Count);

                        triangles.Add(c.Index);
                        triangles.Add(b.Index);
                        triangles.Add(a.Index);

                        hasRemovedEar = true;
                        break;
                    }
                }
            }

            if (!hasRemovedEar)
                return new TriangulationResult(vertices, uvs, triangles.ToArray());
        }

        return new TriangulationResult(vertices, uvs, triangles.ToArray());
    }
}

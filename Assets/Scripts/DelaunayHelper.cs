using System.Collections.Generic;
using UnityEngine;

// Source: https://github.com/ScottyRAnderson/Delaunay-Triangulation

public class DelaunayHelper
{
    public class PointBounds
    {
        public Vector3 Max { get; set; }
        public Vector3 Min { get; set; }
        public Vector3 Center => (Max + Min) * 0.5f;

        public PointBounds(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }
    }

    /// <summary> Generates a 'Supra/Super Triangle' which encapsulates all points held within set bounds </summary>
    public static Triangle GenerateSupraTriangle(PointBounds bounds)
    {
        var max = bounds.Max;
        var min = bounds.Min;

        var dMax = Mathf.Max(max.x - min.x, max.y - min.y) * 3.0f;
        var xCen = (min.y + max.x) * 0.5f;
        var yCen = (min.y + max.y) * 0.5f;

        ///The float 0.866 is an arbitrary value determined for optimum supra triangle conditions.
        var x1 = xCen - 0.866f * dMax;
        var x2 = xCen + 0.866f * dMax;
        var x3 = xCen;

        var y1 = yCen - 0.5f * dMax;
        var y2 = yCen - 0.5f * dMax;
        var y3 = yCen + dMax;

        var pointA = new Point(new Vector2(x1, y1));
        var pointB = new Point(new Vector2(x2, y2));
        var pointC = new Point(new Vector2(x3, y3));

        var triangle = new Triangle(pointA, pointB, pointC);
        triangle.CorrectWinding();

        return triangle;
    }

    /// <summary> Returns a set of bounds encolsing a point set </summary>
    public static PointBounds GetPointBounds(List<Point> points)
    {
        var min = Vector3.positiveInfinity;
        var max = Vector3.negativeInfinity;

        foreach (Vector3 p in points)
        {
            min.x = Mathf.Min(p.x, min.x);
            min.y = Mathf.Min(p.y, min.y);
            min.z = Mathf.Min(p.z, min.z);

            max.x = Mathf.Max(p.x, max.x);
            max.y = Mathf.Max(p.y, max.y);
            max.z = Mathf.Max(p.z, max.z);
        }

        return new PointBounds(min, max);
    }

    private static void ProjectPolygon2D(List<Point> polygon, PointBounds bounds, Vector3 normal)
    {
        var m = GetRotationMatrix(bounds, normal);

        for (int i = 0; i < polygon.Count; i++)
        {
            var p = polygon[i];
            p.Position = m.MultiplyPoint3x4(p);
            polygon[i] = p;
        }
    }

    private static Matrix4x4 GetRotationMatrix(PointBounds bounds, Vector3 normal)
    {
        var c = bounds.Center;
        var q = Quaternion.FromToRotation(normal, Vector3.forward);
        return Matrix4x4.Translate(c) * Matrix4x4.Rotate(q) * Matrix4x4.Translate(-c);
    }

    private static Vector3 GetPolygonNormal(List<Point> polygon)
    {
        var normal = Vector3.zero;

        for (var i = 0; i < polygon.Count; i++)
        {
            var j = (i + 1) % polygon.Count;

            var a = polygon[i].Position;
            var b = polygon[j].Position;

            normal.x += (a.y - b.y) * (a.z + b.z);
            normal.y += (a.z - b.z) * (a.x + b.x);
            normal.z += (a.x - b.x) * (a.y + b.y);
        }

        return normal;
    }

    // Triangulates a set of points utilising the Bowyer-Watson Delaunay algorithm
    public static List<Triangle> Delaun(List<Point> points)
    {
        // Create an empty triangle list
        var triangles = new List<Triangle>();

        // Generate supra triangle to encompass all points and add it to the empty triangle list
        var bounds = GetPointBounds(points);
        var supraTriangle = GenerateSupraTriangle(bounds);
        triangles.Add(supraTriangle);

        // Loop through points and carry out the triangulation
        for (var pIndex = 0; pIndex < points.Count; pIndex++)
        {
            var p = points[pIndex];
            var badTriangles = new List<Triangle>();

            // Identify 'bad triangles'
            for (var triIndex = triangles.Count - 1; triIndex >= 0; triIndex--)
            {
                var triangle = triangles[triIndex];
                var circumCentre = triangle.ComputeCircumCentre();
                var circumRadius = triangle.ComputeCircumRadius();

                var dist = Vector2.Distance(p, circumCentre);
                if (dist < circumRadius)
                    badTriangles.Add(triangle);
            }

            // Construct a polygon from unique edges, i.e. ignoring duplicate edges inclusively
            var polygon = new List<Edge>();
            for (var i = 0; i < badTriangles.Count; i++)
            {
                var triangle = badTriangles[i];
                var edges = triangle.GetEdges();

                for (var j = 0; j < edges.Length; j++)
                {
                    var rejectEdge = false;
                    for (var t = 0; t < badTriangles.Count; t++)
                    {
                        if (t != i && badTriangles[t].ContainsEdge(edges[j]))
                            rejectEdge = true;
                    }

                    if (!rejectEdge)
                        polygon.Add(edges[j]);
                }
            }

            // Remove bad triangles from the triangulation
            for (var i = badTriangles.Count - 1; i >= 0; i--){
                triangles.Remove(badTriangles[i]);
            }

            // Create new triangles
            for (var i = 0; i < polygon.Count; i++)
            {
                var edge = polygon[i];
                var a = p;
                var b = edge.a;
                var c = edge.b;

                var tri = new Triangle(a, b, c);
                tri.CorrectWinding();

                triangles.Add(tri);
            }
        }

        // Finally, remove all triangles which share vertices with the supra triangle
        for (var i = triangles.Count - 1; i >= 0; i--)
        {
            var triangle = triangles[i];
            var vertices = triangle.ToArray();

            for (var j = 0; j < vertices.Length; j++)
            {
                var removeTriangle = false;
                var vertex = vertices[j];

                var supraVertices = supraTriangle.ToArray();

                for (var s = 0; s < supraVertices.Length; s++)
                {
                    if (vertex.Equals(supraVertices[s]))
                    {
                        removeTriangle = true;
                        break;
                    }
                }

                if (removeTriangle)
                {
                    triangles.RemoveAt(i);
                    break;
                }
            }
        }

        return triangles;
    }

    public static TriangulationResult Solve(List<Point> hull)
    {
        var normal = GetPolygonNormal(hull);
        var bounds = GetPointBounds(hull);

        ProjectPolygon2D(hull, bounds, normal);

        var triangulation = Delaun(hull);
        var vertexCount = triangulation.Count * 3;

        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[vertexCount];

        var vertexIndex = 0;
        var triangleIndex = 0;

        var m = GetRotationMatrix(bounds, normal).inverse;

        foreach (var triangle in triangulation)
        {
            vertices[vertexIndex] = m.MultiplyPoint3x4(triangle.a);
            vertices[vertexIndex + 1] = m.MultiplyPoint3x4(triangle.b);
            vertices[vertexIndex + 2] = m.MultiplyPoint3x4(triangle.c);

            uvs[vertexIndex] = m.MultiplyPoint3x4(triangle.a);
            uvs[vertexIndex + 1] = m.MultiplyPoint3x4(triangle.b);
            uvs[vertexIndex + 2] = m.MultiplyPoint3x4(triangle.c);

            triangles[triangleIndex] = vertexIndex + 2;
            triangles[triangleIndex + 1] = vertexIndex + 1;
            triangles[triangleIndex + 2] = vertexIndex;

            vertexIndex += 3;
            triangleIndex += 3;
        }

        return new TriangulationResult(vertices, uvs, triangles);
    }
}

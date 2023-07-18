using System.Collections.Generic;
using UnityEngine;

// Source: https://github.com/ScottyRAnderson/Delaunay-Triangulation

public class DelaunayHelper
{
    public class PointBounds
    {
        public float maxX;
        public float minX;
        public float maxY;
        public float minY;

        public PointBounds(float maxX, float minX, float maxY, float minY)
        {
            this.maxX = maxX;
            this.minX = minX;
            this.maxY = maxY;
            this.minY = minY;
        }
    }

    /// <summary> Generates a 'Supra/Super Triangle' which encapsulates all points held within set bounds </summary>
    public static Triangle GenerateSupraTriangle(PointBounds bounds)
    {
        var dMax = Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY) * 3.0f;
        var xCen = (bounds.minX + bounds.maxX) * 0.5f;
        var yCen = (bounds.minY + bounds.maxY) * 0.5f;

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
        var minX = Mathf.Infinity;
        var minY = Mathf.Infinity;
        var maxX = Mathf.NegativeInfinity;
        var maxY = Mathf.NegativeInfinity;

        foreach (Vector3 p in points)
        {
            minX = Mathf.Min(p.x, minX);
            minY = Mathf.Min(p.y, minY);
            maxX = Mathf.Max(p.x, maxX);
            maxY = Mathf.Max(p.y, maxY);
        }

        return new PointBounds(minX, minY, maxX, maxY);
    }

    private static List<Point> ProjectPolygon2D(List<Point> polygon, Vector3 normal)
    {
        var projected = new List<Point>();

        foreach (var point in polygon)
        {
            var a = point.Position;
            var q = Quaternion.FromToRotation(normal, Vector3.forward);

            var p = point;
            p.Position = q * a;

            projected.Add(p);
        }

        return projected;
    }

    // Triangulates a set of points utilising the Bowyer Watson Delaunay technique
    public static List<Triangle> Delaun(List<Point> points, Vector3 normal)
    {
        points = ProjectPolygon2D(points, normal);

        // Create an empty triangle list
        var triangles = new List<Triangle>();

        // Generate supra triangle to ecompass all points and add it to the empty triangle list
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
                if (dist < circumRadius) {
                    badTriangles.Add(triangle);
                }
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
                        if (t != i && badTriangles[t].ContainsEdge(edges[j])){
                            rejectEdge = true;
                        }
                    }

                    if (!rejectEdge) {
                        polygon.Add(edges[j]);
                    }
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
        var triangulation = Delaun(hull, Vector3.forward);
        var vertexCount = triangulation.Count * 3;

        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[vertexCount];

        var vertexIndex = 0;
        var triangleIndex = 0;
        for (var i = 0; i < triangulation.Count; i++)
        {
            var triangle = triangulation[i];

            vertices[vertexIndex] = triangle.a;
            vertices[vertexIndex + 1] = triangle.b;
            vertices[vertexIndex + 2] = triangle.c;

            uvs[vertexIndex] = triangle.a;
            uvs[vertexIndex + 1] = triangle.b;
            uvs[vertexIndex + 2] = triangle.c;

            triangles[triangleIndex] = vertexIndex + 2;
            triangles[triangleIndex + 1] = vertexIndex + 1;
            triangles[triangleIndex + 2] = vertexIndex;

            vertexIndex += 3;
            triangleIndex += 3;
        }

        return new TriangulationResult(vertices, uvs, triangles);
    }
}

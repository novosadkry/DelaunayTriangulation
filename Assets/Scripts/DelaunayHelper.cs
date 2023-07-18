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
        float dMax = Mathf.Max(bounds.maxX - bounds.minX, bounds.maxY - bounds.minY) * 3.0f;
        float xCen = (bounds.minX + bounds.maxX) * 0.5f;
        float yCen = (bounds.minY + bounds.maxY) * 0.5f;

        ///The float 0.866 is an arbitrary value determined for optimum supra triangle conditions.
        float x1 = xCen - 0.866f * dMax;
        float x2 = xCen + 0.866f * dMax;
        float x3 = xCen;

        float y1 = yCen - 0.5f * dMax;
        float y2 = yCen - 0.5f * dMax;
        float y3 = yCen + dMax;

        var pointA = new Point(0, new Vector2(x1, y1));
        var pointB = new Point(0, new Vector2(x2, y2));
        var pointC = new Point(0, new Vector2(x3, y3));

        var triangle = new Triangle(pointA, pointB, pointC);
        triangle.CorrectWinding();

        return triangle;
    }

    /// <summary> Returns a set of bounds encolsing a point set </summary>
    public static PointBounds GetPointBounds(List<Point> points)
    {
        float minX = Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxX = Mathf.NegativeInfinity;
        float maxY = Mathf.NegativeInfinity;

        for (int i = 0; i < points.Count; i++)
        {
            Point p = points[i];
            if (minX > p.Position.x){
                minX = p.Position.x;
            }
            if (minY > p.Position.y){
                minY = p.Position.y;
            }
            if (maxX < p.Position.x){
                maxX = p.Position.x;
            }
            if (maxY < p.Position.y){
                maxY = p.Position.y;
            }
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
        List<Triangle> triangles = new List<Triangle>();

        // Generate supra triangle to ecompass all points and add it to the empty triangle list
        PointBounds bounds = GetPointBounds(points);
        Triangle supraTriangle = GenerateSupraTriangle(bounds);
        triangles.Add(supraTriangle);

        // Loop through points and carry out the triangulation
        for (int pIndex = 0; pIndex < points.Count; pIndex++)
        {
            Point p = points[pIndex];
            List<Triangle> badTriangles = new List<Triangle>();

            // Identify 'bad triangles'
            for (int triIndex = triangles.Count - 1; triIndex >= 0; triIndex--)
            {
                Triangle triangle = triangles[triIndex];
                var circumCentre = triangle.ComputeCircumCentre();
                var circumRadius = triangle.ComputeCircumRadius();

                float dist = Vector2.Distance(p.Position, circumCentre);
                if (dist < circumRadius) {
                    badTriangles.Add(triangle);
                }
            }

            // Construct a polygon from unique edges, i.e. ignoring duplicate edges inclusively
            List<Edge> polygon = new List<Edge>();
            for (int i = 0; i < badTriangles.Count; i++)
            {
                Triangle triangle = badTriangles[i];
                Edge[] edges = triangle.GetEdges();

                for (int j = 0; j < edges.Length; j++)
                {
                    bool rejectEdge = false;
                    for (int t = 0; t < badTriangles.Count; t++)
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
            for (int i = badTriangles.Count - 1; i >= 0; i--){
                triangles.Remove(badTriangles[i]);
            }

            // Create new triangles
            for (int i = 0; i < polygon.Count; i++)
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
        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            Triangle triangle = triangles[i];
            var vertices = triangle.ToArray();

            for (int j = 0; j < vertices.Length; j++)
            {
                bool removeTriangle = false;
                Point vertex = vertices[j];

                var supraVertices = supraTriangle.ToArray();

                for (int s = 0; s < supraVertices.Length; s++)
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

    public static (Vector3[], Vector2[], int[]) Solve(List<Point> hull)
    {
        var triangulation = Delaun(hull, Vector3.forward);
        int vertexCount = triangulation.Count * 3;

        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[vertexCount];

        int vertexIndex = 0;
        int triangleIndex = 0;
        for (int i = 0; i < triangulation.Count; i++)
        {
            Triangle triangle = triangulation[i];

            vertices[vertexIndex] = triangle.a.Position;
            vertices[vertexIndex + 1] = triangle.b.Position;
            vertices[vertexIndex + 2] = triangle.c.Position;

            uvs[vertexIndex] = triangle.a.Position;
            uvs[vertexIndex + 1] = triangle.b.Position;
            uvs[vertexIndex + 2] = triangle.c.Position;

            triangles[triangleIndex] = vertexIndex + 2;
            triangles[triangleIndex + 1] = vertexIndex + 1;
            triangles[triangleIndex + 2] = vertexIndex;

            vertexIndex += 3;
            triangleIndex += 3;
        }

        return (vertices, uvs, triangles);
    }
}

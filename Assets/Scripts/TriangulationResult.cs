using UnityEngine;

public struct TriangulationResult
{
    public Vector3[] vertices;
    public Vector2[] uvs;
    public int[] triangles;

    public TriangulationResult(Vector3[] vertices, Vector2[] uvs, int[] triangles)
    {
        this.vertices = vertices;
        this.uvs = uvs;
        this.triangles = triangles;
    }
}

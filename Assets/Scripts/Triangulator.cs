using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangulator : MonoBehaviour
{
    public List<PointHandler> hullHandlers;
    public List<PointHandler> holeHandlers;
    public MeshFilter meshFilter;

    private Mesh _mesh;

    void Start()
    {
        _mesh = new Mesh();
        meshFilter.mesh = _mesh;
    }

    void Update()
    {
        var hull = hullHandlers
            .Select(x => x.point)
            .ToList();

        var hole = holeHandlers
            .Select(x => x.point)
            .ToList();

        var result = DelaunayHelper.Solve(hull);
        _mesh.vertices = result.vertices;
        _mesh.triangles = result.triangles;
        _mesh.uv = result.uvs;
    }
}

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
        int index = 0;

        foreach (var handler in hullHandlers)
            handler.point.Index = index++;

        foreach (var handler in holeHandlers)
            handler.point.Index = index++;

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

        _mesh.vertices = hull.Concat(hole)
            .Select(x => x.Position)
            .ToArray();

        _mesh.triangles = EarClipHelper
            .Solve(hull, hole)
            .ToArray();
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Triangulator : MonoBehaviour
{
    public List<Point> points;
    public MeshFilter meshFilter;

    private Mesh _mesh;

    void Start()
    {
        if (points.Count == 0)
        {
            points = FindObjectsOfType<Point>()
                .OrderBy(p => p.name)
                .ToList();
        }

        for (int i = 0; i < points.Count; i++)
            points[i].Index = i;

        _mesh = new Mesh();
        meshFilter.mesh = _mesh;
    }

    void Update()
    {
        _mesh.vertices = points.Select(x => x.transform.position).ToArray();
        _mesh.triangles = EarClipHelper.Solve(points).ToArray();
    }
}

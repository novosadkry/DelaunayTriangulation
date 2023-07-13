using System.Collections.Generic;
using UnityEngine;

public class TriangleRenderer : MonoBehaviour
{
    public List<Triangle> triangles;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        var positionCount = triangles.Count * 3;
        var positions = new Vector3[positionCount];

        for (var i = 0; i < triangles.Count; i++)
        {
            var triangle = triangles[i];
            positions[i * 3 + 0] = triangle[0].Position;
            positions[i * 3 + 1] = triangle[1].Position;
            positions[i * 3 + 2] = triangle[2].Position;
        }

        _lineRenderer.positionCount = positionCount;
        _lineRenderer.SetPositions(positions);
    }
}

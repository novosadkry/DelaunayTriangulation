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
        var positionCount = triangles.Count * 4;
        var positions = new Vector3[positionCount];

        for (var i = 0; i < triangles.Count; i++)
        {
            var triangle = triangles[i];
            positions[i * 4 + 0] = triangle[0].transform.position;
            positions[i * 4 + 1] = triangle[1].transform.position;
            positions[i * 4 + 2] = triangle[2].transform.position;
            positions[i * 4 + 3] = triangle[0].transform.position;
        }

        _lineRenderer.positionCount = positionCount;
        _lineRenderer.SetPositions(positions);
    }
}

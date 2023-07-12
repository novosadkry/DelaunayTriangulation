using System.Collections.Generic;
using UnityEngine;

public class Triangulator : MonoBehaviour
{
    public List<Point> points;
    public TriangleRenderer renderer;

    void Start()
    {
        points = new List<Point>(FindObjectsOfType<Point>());
    }

    void Update()
    {
        renderer.triangles = EarClipHelper.Solve(points);
    }
}

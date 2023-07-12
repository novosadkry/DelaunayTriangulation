using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EarClipHelper
{
    // Source: https://stackoverflow.com/a/9755252
    public static bool IsInsideTriangle(Triangle triangle, Point point)
    {
        var p = point.transform.position;
        var a = triangle.a.transform.position;
        var b = triangle.b.transform.position;
        var c = triangle.c.transform.position;

        var as_x = p.x - a.x;
        var as_y = p.y - a.y;

        bool s_ab = (b.x - a.x) * as_y - (b.y - a.y) * as_x > 0;

        if ((c.x - a.x) * as_y - (c.y - a.y) * as_x > 0 == s_ab)
            return false;

        if ((c.x - b.x) * (p.y - b.y) - (c.y - b.y) * (p.x - b.x) > 0 != s_ab)
            return false;

        return true;
    }

    public static bool IsConvex(LinkedListNode<Point> v, LinkedList<Point> linked)
    {
        var a = v.Previous ?? linked.Last;
        var b = v;
        var c = v.Next ?? linked.First;

        var ba = (Vector2) a.Value.transform.position - (Vector2) b.Value.transform.position;
        var bc = (Vector2) c.Value.transform.position - (Vector2) b.Value.transform.position;

        var inner = Vector2.SignedAngle(ba, bc);
        if (inner < 0) inner += 360;

        return inner < Mathf.PI * Mathf.Rad2Deg;
    }

    public static List<Triangle> Solve(List<Point> points)
    {
        var result = new List<Triangle>();

        var linked = new LinkedList<Point>(points);
        var convex = new List<LinkedListNode<Point>>();
        var concave = new List<LinkedListNode<Point>>();

        {
            var p = linked.First;
            while (p != null)
            {
                if (IsConvex(p, linked))
                    convex.Add(p);
                else
                    concave.Add(p);

                p = p.Next;
            }
        }

        var ears = new LinkedList<LinkedListNode<Point>>();

        foreach (var p in convex)
        {
            if (IsEar(p, concave, linked))
                ears.AddLast(p);
        }

        while (ears.Count > 0)
        {
            var ear = ears.First.Value;
            ears.RemoveFirst();

            var a = ear.Previous ?? linked.Last;
            var b = ear;
            var c = ear.Next ?? linked.First;

            result.Add(new Triangle(a.Value, b.Value, c.Value));

            convex.Remove(ear);
            linked.Remove(ear);

            if (linked.Count < 3)
                break;

            ReconfigureAdjacentVertex(a, convex, concave, ears, linked);
            ReconfigureAdjacentVertex(c, convex, concave, ears, linked);
        }

        return result;
    }

    private static void ReconfigureAdjacentVertex(LinkedListNode<Point> v, List<LinkedListNode<Point>> convex, List<LinkedListNode<Point>> concave, LinkedList<LinkedListNode<Point>> ears, LinkedList<Point> linked)
    {
        var a = v.Previous ?? linked.Last;
        var b = v;
        var c = v.Next ?? linked.First;

        //If the adjacent vertex was reflect...
        if (concave.Contains(v))
        {
            //it may now be convex...
            if (IsConvex(v, linked))
            {
                concave.Remove(v);
                convex.Add(v);

                //and possible a new ear
                if (IsEar(v, concave, linked))
                    ears.AddLast(v);
            }
        }
        //If an adjacent vertex was convex, it will always still be convex
        else
        {
            bool isEar = IsEar(v, concave, linked);

            //This vertex was an ear but is no longer an ear
            if (ears.Contains(v) && !isEar)
            {
                ears.Remove(v);
            }
            //This vertex wasn't an ear but has now become an ear
            else if (isEar)
            {
                ears.AddLast(v);
            }
        }
    }

    private static bool IsEar(LinkedListNode<Point> p, List<LinkedListNode<Point>> concave, LinkedList<Point> linked)
    {
        var a = p.Previous ?? linked.Last;
        var b = p;
        var c = p.Next ?? linked.First;

        var triangle = new Triangle(a.Value, b.Value, c.Value);

        foreach (var other in concave)
        {
            if (other.Value.Equals(a.Value) || other.Value.Equals(b.Value) || other.Value.Equals(c.Value))
                continue;

            if (IsInsideTriangle(triangle, other.Value))
                return false;
        }

        return true;
    }

    public static int Modulo(int n, int m)
    {
        return (n % m + m) % m;
    }
}

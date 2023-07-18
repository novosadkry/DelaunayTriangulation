public class Edge
{
    public Point a;
    public Point b;

    public Edge(Point a, Point b)
    {
        this.a = a;
        this.b = b;
    }

    private bool Equals(Edge other)
    {
        return (a.Equals(other.a) && b.Equals(other.b)) ||
               (b.Equals(other.a) && a.Equals(other.b));
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Edge)obj);
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using UnityEngine;

class Utils
{
    public struct Edge
    {
        public int from, to;
        public float dist;

        public Edge(int f, int t, float d)
        {
            this.from = f;
            this.to = t;
            this.dist = d;
        }
    }

    public static List<int> bf(int N, int s, int t, List<Edge> edges)
    {
        List<int> ans = new List<int>();
        float[] dist = new float[N];
        int[] pre = new int[N];
        for (int i = 0; i < N; i++) pre[i] = -1;
        for (int i = 0; i < N; i++) dist[i] = Mathf.Infinity;

        dist[s] = 0;
        for (int i = 0; i < N; i++)
        {
            foreach (Edge edge in edges)
            {
                if (dist[edge.from] + edge.dist < dist[edge.to])
                {
                    dist[edge.to] = dist[edge.from] + edge.dist;
                    pre[edge.to] = edge.from;
                }
            }
        }

        int cur = t;
        while (cur != s)
        {
            ans.Add(cur);
            cur = pre[cur];
        }
        ans.Add(s);
        ans.Reverse();
        Debug.Log("ans length " + ans.Count);
        return ans;
    }

    public static bool FasterLineSegmentIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        Vector2 a = p2 - p1;
        Vector2 b = p3 - p4;
        Vector2 c = p1 - p3;

        float alphaNumerator = b.y * c.x - b.x * c.y;
        float alphaDenominator = a.y * b.x - a.x * b.y;
        float betaNumerator = a.x * c.y - a.y * c.x;
        float betaDenominator = alphaDenominator; /*2013/07/05, fix by Deniz*/

        bool doIntersect = true;

        if (alphaDenominator == 0 || betaDenominator == 0)
        {
            doIntersect = false;
        }
        else
        {
            if (alphaDenominator > 0)
            {
                if (alphaNumerator < 0 || alphaNumerator > alphaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (alphaNumerator > 0 || alphaNumerator < alphaDenominator)
            {
                doIntersect = false;
            }

            if (doIntersect && betaDenominator > 0)
            {
                if (betaNumerator < 0 || betaNumerator > betaDenominator)
                {
                    doIntersect = false;
                }
            }
            else if (betaNumerator > 0 || betaNumerator < betaDenominator)
            {
                doIntersect = false;
            }
        }

        return doIntersect;
    }

    /*static bool ContainsPoint(Vector2[] polyPoints, Vector2 p)
        {
            var j = polyPoints.Length - 1;
            var inside = false;
            for (var i = 0; i < polyPoints.Length; j = i++)
            {
                if (((polyPoints[i].y <= p.y && p.y < polyPoints[j].y) || (polyPoints[j].y <= p.y && p.y < polyPoints[i].y)) &&
                   (p.x < (polyPoints[j].x - polyPoints[i].x) * (p.y - polyPoints[i].y) / (polyPoints[j].y - polyPoints[i].y) + polyPoints[i].x))
                    inside = !inside;
            }
            return inside;
        }*/

    public static bool ContainsPoint(Vector2[] p, Vector2 v)
    {
        int j = p.Length - 1;
        bool c = false;
        for (int i = 0; i < p.Length; j = i++)
            c ^= p[i].y > v.y ^ p[j].y > v.y && v.x < (p[j].x - p[i].x) * (v.y - p[i].y) / (p[j].y - p[i].y) + p[i].x;
        return c;
    }

    public static float gaussianRandom(float mean, float stdDev)
    {
        float u1 = Random.Range(0.0F, 1.0F); //these are uniform(0,1) random doubles
        float u2 = Random.Range(0.0F, 1.0F);
        float randStdNormal = Mathf.Sqrt(-2.0F * Mathf.Log(u1)) *
                              Mathf.Sin(2.0F * Mathf.PI * u2); //random normal(0,1)
        float randNormal =
            mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
        return randNormal;
    }

    public static void backtrackPath(List<TreeNode> nodes, TreeNode startNode, TreeNode current, List<TreeNode> path)
    {
        if (current.parent != null) backtrackPath(nodes, startNode, current.parent, path);
        path.Add(current);
    }

    public static void backtrackPath(List<DubinNode> nodes, DubinNode startNode, DubinNode current, List<DubinNode> path)
    {
        if (current.parent != null) backtrackPath(nodes, startNode, current.parent, path);
        path.Add(current);
    }

    public static void SaveFilledPath(List<Node> path)
    {
        string json = JsonConvert.SerializeObject(path, new Vector2Converter(), new NodeConverter());
        System.IO.File.WriteAllText(@"output.json", json);
    }


    public static void TopoSort(DubinNode root, List<Pair<DubinNode, float>> sorted, float dt)
    {
        foreach (var child in root.children)
        {
            TopoSort(child, sorted, child.time - root.time);
        }
        sorted.Add(new Pair<DubinNode, float>(root, dt));
        if (root.parent == null)
        {
            sorted.Reverse();
        }
    }


    public static List<Node> LoadFilledPath()
    {
        List<Node> path = null;
        try
        {
            var json = System.IO.File.ReadAllText(@"output.json");
            path =
                (List<Node>)
                JsonConvert.DeserializeObject(json, typeof(List<Node>), new Vector2Converter(), new NodeConverter());
        }
        catch
        {
        }
        return path;
    }

    static float minX = Mathf.Infinity,
        minY = Mathf.Infinity,
        maxX = Mathf.NegativeInfinity,
        maxY = Mathf.NegativeInfinity;


    //Generates a random point, then checks to see if it fits the boundary
    public static Vector2 randomPoint(Node startNode, Node goalNode, Vector2[] boundaryPolygon, Vector2[][] polygons)
    {
        if (minX == Mathf.Infinity)
        {
            foreach (Vector2 p in boundaryPolygon)
            {
                minX = Mathf.Min(minX, p.x);
                minY = Mathf.Min(minY, p.y);
                maxX = Mathf.Max(maxX, p.x);
                maxY = Mathf.Max(maxY, p.y);
            }
        }

        float x, y;
        do
        {
            Vector2 vertex;

            var rand = Random.Range(0.0F, 1.0F);
            if (rand < 0.05)
            {
                vertex = startNode.pos;
            }
            else if (rand < 0.1)
            {
                vertex = goalNode.pos;
            }
            else
            {
                // our optimization
                var polygonId = Random.Range(0, polygons.Length);
                var vertexId = Random.Range(0, polygons[polygonId].Length);
                //bias to obstacle vertex
                vertex = polygons[polygonId][vertexId];
            }

            const float GAUSSIAN_SIZE = 3.0F;
            x = Utils.gaussianRandom(vertex.x, GAUSSIAN_SIZE);
            y = Utils.gaussianRandom(vertex.y, GAUSSIAN_SIZE);
            //            x = Random.Range(minX, maxX);
            //            y = Random.Range(minY, maxY);
        } while (!(x > minX && x < maxX && y > minY && y < maxY) ||
                 !Cheetah.instance.IsBounded(new Vector2(x, y)));
        return new Vector2(x, y);
    }
}
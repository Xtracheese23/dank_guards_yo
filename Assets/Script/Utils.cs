﻿using System.Collections.Generic;
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

    public struct Set
    {
        public List<int> set;
        public int score;
        public Vector2 center; 

        public Set(List<int> s, int si,Vector2 c)
        {
            this.set = s;
            this.score = si;
            this.center = c;
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

    //returns indexes of visible points from interestPoints. This corresponds to a subset of all the interestPoints.
    public static Set pointsInSight(Vector2 c, float r,Vector2[] interestPoints,Vector2[][] polygons)
    {
        List<int> visiblePoints = new List<int>();
        int l = 0;
        for (int i = 0; i < interestPoints.Length; i++)
        {
            float dist = Vector2.Distance(c, interestPoints[i]);
            if (dist > r) continue;

            bool intersect = false;
            foreach (var polygon in polygons)
            {
                for (int k = 0; k < polygon.Length; k++)
                {
                    Vector2 p3 = polygon[k], p4 = polygon[(k + 1) % polygon.Length];
                    intersect = intersect || Utils.FasterLineSegmentIntersection(c, interestPoints[i], p3, p4);
                }
            }
            if (!intersect)
            {
                visiblePoints.Add(i);
                l++;
                Debug.Log("(" + c[0] + ", " + c[1] + "),Can see (" + interestPoints[i][0] + "," + interestPoints[i][1] + "), d = " + dist);
            }
        }
        return new Set(visiblePoints, l,c);
    }

    // Use Greedy algorithm to find N best subsets to cover the interestPoints. Needs to be improved to run multiple Greedies in parallel.
    // I wanna test this first but I'm not sure how.
    public static int[] findBestSetsUsingGreedy(List<Set> subsets,int N)
    {
        int[] bestIndex = new int[N];
        int[] bestScore = new int[N];
        Set s,bestS;
        for (int n=0;n<N; n++)
        {
            for (int i = 0; i < subsets.Count; i++)
            {
                s = subsets[i];
                if (s.score > bestScore[n])
                {
                    bestIndex[n] = i;
                    bestScore[n] = s.score;
                }
            }

            bestS = subsets[bestIndex[n]];
            subsets.RemoveAt(bestIndex[n]);
            for (int i = 0; i < subsets.Count; i++)
            {
                s = subsets[i];
                foreach(int j in s.set)
                    if (bestS.set.Contains(j)) s.score--;
            }
            Debug.Log("Score: " +bestScore+ "Center: (" + bestS.center[0]+","+ bestS.center[1]+")");
        }
        return bestIndex;
    }


    // Useless for now (Can be modified to calulate intersection point between an obstacle and a circle later if needed)
    public static bool LineSegmentCircleIntersection(Vector2 p1, Vector2 p2, Vector2 center, float r)
    {
        Vector2 d = p2 - p1;
        Vector2 f = p1 - center;

        float a = Vector2.Dot(d,d);
        float b = 2 * Vector2.Dot(f,d);
        float c = Vector2.Dot(f,f) - r * r;
        float discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            // no intersection
            return false;
        }
        else
        {
            // ray didn't totally miss sphere,
            // so there is a solution to
            // the equation.

            discriminant = (float)System.Math.Sqrt(discriminant);

            // either solution may be on or off the ray so need to test both
            // t1 is always the smaller value, because BOTH discriminant and
            // a are nonnegative.
            float t1 = (-b - discriminant) / (2 * a);
            float t2 = (-b + discriminant) / (2 * a);

            // 3x HIT cases:
            //          -o->             --|-->  |            |  --|->
            // Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 

            // 3x MISS cases:
            //       ->  o                     o ->              | -> |
            // FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)

            if (t1 >= 0 && t1 <= 1)
            {
                // t1 is the intersection, and it's closer than t2
                // (since t1 uses -b - discriminant)
                // Impale, Poke
                return true;
            }

            // here t1 didn't intersect so we are either started
            // inside the sphere or completely past it
            if (t2 >= 0 && t2 <= 1)
            {
                // ExitWound
                return true;
            }

            // no intn: FallShort, Past, CompletelyInside
            return false;
        }
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
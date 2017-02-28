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

    public struct GreedyNode
    {
        public Set set;
        public int nbCandidates;
        public int depth;
        public GreedyNode[] bestCandidates;

        public GreedyNode(Set s, int N,int d)
        {
            this.set = s;
            this.nbCandidates = 0;
            this.bestCandidates = new GreedyNode[N];
            this.depth=d;
        }

        public void addCandidate(GreedyNode gn)
        {
            this.bestCandidates[nbCandidates] = gn;
            this.nbCandidates++;
        }
    }

    public struct GreedyTree
    {
        public GreedyNode root;

        public GreedyTree(GreedyNode root)
        {
            this.root = root;
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
                //Debug.Log("(" + c[0] + ", " + c[1] + "),Can see (" + interestPoints[i][0] + "," + interestPoints[i][1] + "), d = " + dist);
            }
        }
        return new Set(visiblePoints, l,c);
    }

    //returns indexes of visible points from interestPoints. This corresponds to a subset of all the interestPoints.
    public static Set pointsInSight(Vector2 c, float r, Vector2[] interestPoints)
    {
        List<int> visiblePoints = new List<int>();
        int l = 0;
        for (int i = 0; i < interestPoints.Length; i++)
        {
            float dist = Vector2.Distance(c, interestPoints[i]);
            if (dist > r) continue;
            visiblePoints.Add(i);
            l++;
            //Debug.Log("(" + c[0] + ", " + c[1] + "),Can see (" + interestPoints[i][0] + "," + interestPoints[i][1] + "), d = " + dist);
        }
        Set set = new Set(visiblePoints, l, c);
        //Debug.Log("(" + c[0] + ", " + c[1] + "),score "+set.score);
        return set;
    }



    // Use Greedy algorithm to find N best subsets to cover the interestPoints. Needs to be improved to run multiple Greedies in parallel.
    // I wanna test this first but I'm not sure how.
    public static int[] findBestSetsUsingGreedy(List<Set> subsets,int N)
    {
        int[] bestIndex = new int[N];
        int[] bestScore = new int[N];
        Set s, bestS, tempS;
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
                tempS = s;
                Debug.Log("center"+s.center[0]+","+s.center[1]);
                List<int> elementsToRemove = new List<int>();
                foreach (int j in s.set)
                {
                    Debug.Log(j);
                    if (bestS.set.Contains(j))
                    {
                        //tempS.set.Remove(j);
                        elementsToRemove.Add(j);
                        tempS.score--;
                    }
                }
                foreach (int j in elementsToRemove)
                {
                    tempS.set.Remove(j);
                }
                subsets[i] = tempS;
            }
            Debug.Log("Score: " +bestScore[n]+ "Center: (" + bestS.center[0]+","+ bestS.center[1]+")");
        }
        Debug.Log("Total covered:" + bestScore.Sum()/subsets.Count);
        return bestIndex;
        
    }

    public static float[][] getStartPositions(float[][] items, int numberofGuards,Map map)
    {
        float r = 5;
        float[][] start_positions = new float[numberofGuards][];
        Vector2[] interestPoints = new Vector2[items.Length];
        List<Set> subsets = new List<Set>();
        for(int i=0;i<items.Length;i++)
        {
            interestPoints[i] = new Vector2(items[i][0], items[i][1]);
        }
        foreach(var c in interestPoints)
        {
            Set s = pointsInSight(c, r, interestPoints, map.polygons);
            subsets.Add(s);
        }
        int[] bestIndexes = findBestSetsUsingGreedy(subsets, numberofGuards);

        for(int i = 0; i < numberofGuards; i++)
        {
            start_positions[i] = new float[] { subsets[bestIndexes[i]].center[0], subsets[bestIndexes[i]].center[1] };
        }
        return start_positions;
    }

    public static Set[] findNBestSetsUsingGreedy2(List<Set> subsets, int N)
    {
        Set[] bestSubsets = new Set[N];
        int[] bestScore = new int[N];
        Set s;
        for (int i = 0; i < subsets.Count; i++)
        {
            s = subsets[i];
            for(int j = 0; j < bestSubsets.Length; j++)
            {
                if (s.score > bestScore[j])
                {
                    bestSubsets[j] = s;
                    bestScore[j] = s.score;
                }
            }
        }
        return bestSubsets;
    }

    public static GreedyTree findBestSetsUsingImprovedGreedy(List<Set> subsets, int N,int Ncandidates)
    {
        GreedyNode start = new GreedyNode(new Set(),Ncandidates,0);
        GreedyTree gt = new GreedyTree(start);
        GreedyNode parent;
        parent = start;
        improvedGreedyReccursive(subsets, start, 0, N, Ncandidates);
        return gt;
    }

    public static void improvedGreedyReccursive(List<Set> subsets, GreedyNode parent,int depth,int maxDepth,int Ncandidates)
    {
        if (parent.depth == maxDepth)
        {
            return;
        }
        List<Set> tempSubsets = subsets;
        GreedyNode child;
        Set[] s = findNBestSetsUsingGreedy2(tempSubsets, Ncandidates);
        for (int j = 0; j < Ncandidates; j++)
        {
            tempSubsets = subsets;
            child = new GreedyNode(s[j], Ncandidates, depth+1);
            parent.addCandidate(child);
            tempSubsets.Remove(s[j]);
            improvedGreedyReccursive(tempSubsets, child, depth, maxDepth, Ncandidates);
        }

        return;
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
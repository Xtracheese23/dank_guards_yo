using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public Point[] point;
    public Set[] guardSets;
    public Map map;
    public Color[] colors;
    public GameObject ai;
    public GameObject mapai;
    public GameObject camera;
    public int task = 2;

    public Datastruct data;
    public bool useSaved;
    public string problem;
    private int numberofGuards;
    private int itemcount;

    private void OnDrawGizmos()
    {
        
        /*
        for (int i = 0; i < itemcount; i++)
        {
            if (map)
            {
                //Gizmos.color = Color.yellow;
                //Gizmos.DrawSphere(new Vector3(map.items[i][0], map.items[i][1], 19), 0.5F);
                Gizmos.DrawIcon(new Vector3(map.items[i][0], map.items[i][1], 1), "doughnut.tif", true);
            }
        }
        */
        if (task == 1)
        {
            for (int i = 0; i < numberofGuards; i++)
            {
                if (point[i])
                {
                    Gizmos.color = Color.green;
                    //Gizmos.DrawCube(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), new Vector3(0.5F, 0.5F, 0));
                    //Gizmos.color = new Color(1, 0, 0, (float)0.5);
                    Gizmos.DrawWireSphere(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), 30);
                    //Gizmos.DrawSphere(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), 10);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(new Vector3(point[i].goalPos[0], point[i].goalPos[1], 2), new Vector3(0.5F, 0.5F, 0));
                    List<float[]> unseenitems = new List<float[]>();
                    unseenitems.AddRange(map.items);
                    for (int j = 0; j < numberofGuards; j++)
                    {
                        Gizmos.color = colors[j % colors.Length];
                        foreach (var g in guardSets[j].set)
                        {
                            if (j < colors.Length)
                                Gizmos.DrawCube(new Vector3(map.items[g][0], map.items[g][1], 0), new Vector3(0.5F, 0.5F, 0));
                            else
                                Gizmos.DrawSphere(new Vector3(map.items[g][0], map.items[g][1], 0), 1);
                            unseenitems.Remove(map.items[g]);
                        }
                    }
                    for (int j = 0; j < unseenitems.Count; j++)
                    {
                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(new Vector3(unseenitems[j][0], unseenitems[j][1], 0), new Vector3(0.5F, 0.5F, 0));
                    }

                }
            }
        }
        else if (task == 2)
        {
            for (int i = 0; i < numberofGuards; i++)
            {
                if (point[i])
                {
                    Gizmos.color = Color.green;
                    //Gizmos.DrawCube(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), new Vector3(0.5F, 0.5F, 0));
                    //Gizmos.color = new Color(1, 0, 0, (float)0.5);
                    Gizmos.DrawWireSphere(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), 3);
                    //Gizmos.DrawSphere(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), 10);
                    Gizmos.color = Color.blue;
                    Gizmos.DrawCube(new Vector3(point[i].goalPos[0], point[i].goalPos[1], 2), new Vector3(0.5F, 0.5F, 0));
                    List<float[]> unseenitems = new List<float[]>();
                    unseenitems.AddRange(map.items);

                }
            }
            for (int i = 0; i < numberofGuards; i++)
            {
                Gizmos.color = colors[i % colors.Length];
                //Gizmos.DrawCube(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), new Vector3(0.5F, 0.5F, 0));
                //Gizmos.color = new Color(1, 0, 0, (float)0.5);
                for(int j=0;j<data.waypoints[i].Count-1;j++)
                {
                    Waypoint w1 = data.waypoints[i][j];
                    Waypoint w2 = data.waypoints[i][j+1];
                    Debug.DrawLine(new Vector3(w1.point[0], w1.point[1], 2), new Vector3(w2.point[0], w2.point[1], 2), colors[i % colors.Length]);
                }
                //Gizmos.DrawSphere(new Vector3(point[i].startPos[0], point[i].startPos[1], 2), 10);
                Gizmos.color = Color.blue;
                Gizmos.DrawCube(new Vector3(point[i].goalPos[0], point[i].goalPos[1], 2), new Vector3(0.5F, 0.5F, 0));
                List<float[]> unseenitems = new List<float[]>();
                unseenitems.AddRange(map.items);
            }

            foreach (var s in data.sets)
            {
                if(s.closestGuard>0)
                    Gizmos.color = colors[s.closestGuard % colors.Length];
                else
                    Gizmos.color = Color.black;
                foreach (var g in s.set)
                {
                    Gizmos.DrawCube(new Vector3(map.items[g][0], map.items[g][1], 0), new Vector3(0.5F, 0.5F, 0));
                }
            }
        }

    }

    Point CreateAI()
    {

        var player = Instantiate(ai, transform.position, transform.rotation);   //why can I not just set transform.position to what I want?
        Point point = null;
        point = point ? point : player.GetComponent<DDrive>();
        point = point ? point : player.GetComponent<DynamicPoint>();
        point = point ? point : player.GetComponent<KinematicPoint>();
        point = point ? point : player.GetComponent<StaticGuard>();
        point = point ? point : player.GetComponent<KinematicGuard>();
        return point;
    }

    Point CreateAI2()
    {

        var player = Instantiate(ai, new Vector3(2, 2, -9), new Quaternion(0, 0, 0, 1));   //why can I not just set transform.position to what I want?
        Point point = null;
        point = point ? point : player.GetComponent<DDrive>();
        point = point ? point : player.GetComponent<DynamicPoint>();
        point = point ? point : player.GetComponent<KinematicPoint>();
        point = point ? point : player.GetComponent<StaticGuard>();
        point = point ? point : player.GetComponent<KinematicGuard>();
        return point;
    }

    Map CreateMap()
    {

        //var player = Instantiate(mapai, new Vector3 ( 0, 2, -9 ), new Quaternion (0,0,0,1) );//transform.position, transform.rotation);
        var player = Instantiate(mapai, transform.position, transform.rotation);
        Map field = null;
        field = field ? field : player.GetComponent<MapAI>();
        return field;
    }

    void Awake()
    {
        if (Cheetah.instance == null)
        {
            Cheetah.instance = new Cheetah();
        }
    }

    // Use this for initialization
    void Start ()
    {
        colors = new Color[4];
        colors[0] = Color.green;
        colors[1] = Color.cyan;
        colors[2] = Color.yellow;
        colors[3] = Color.magenta;

        string json = "";
        using (StreamReader r = new StreamReader(problem))
        {
            json = r.ReadToEnd();
            // List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
        }
        Input2 input = JsonConvert.DeserializeObject<Input2>(json);

        map = CreateMap();
        Vector2[] boundaryPolygon = new Vector2[input.boundary_polygon.Length];
        for (int i = 0; i < input.boundary_polygon.Length; i++) boundaryPolygon[i] = new Vector2(input.boundary_polygon[i][0], input.boundary_polygon[i][1]);
        map.boundaryPolygon = boundaryPolygon;

        for (int i = 0; i < input.boundary_polygon.Length; i++)
        {
            var next = (i + 1) % input.boundary_polygon.Length;
            //boundaryPolygon[i] = new Vector2(input.boundary_polygon[i][0], input.boundary_polygon[i][1]);
            Debug.DrawLine(new Vector3(input.boundary_polygon[i][0], input.boundary_polygon[i][1], 1), new Vector3(input.boundary_polygon[next][0], input.boundary_polygon[next][1], 1), Color.red, Mathf.Infinity);
        }


        int polygonCnt = 0;
        numberofGuards = 0;
        Debug.Log(input.a_max);
        //Debug.Log(input.polygon["polygon0"] is JArray);

        //gets length to intialize each array (I know this is incredibly bad practice, but won't be too taxing in the end)
        int polycount = 0;
        this.itemcount = 0;
        foreach (var pair in input.polygon)
        {
            if (pair.Key.StartsWith("polygon")) {
                polycount++;
            } else if (pair.Key.StartsWith("start_pos")){
                numberofGuards++;
            } else if (pair.Key.StartsWith("item_")){
                itemcount++;
            }
        }

        Vector2[][] inputPolygon = new Vector2[polycount][];
            //gets each polygon opject
        foreach (var pair in input.polygon)
        {
            if (pair.Key.StartsWith("polygon"))         //checks if name is polygon
            {
                Debug.Log(pair.Key);
                float[][] polygon = pair.Value.ToObject<float[][]>();       //extracts float object
                Vector2[] vertices2D = new Vector2[polygon.Length];
                for (int i = 0; i < polygon.Length; i++)
                {
                    vertices2D[i] = new Vector2(polygon[i][0], polygon[i][1]);      //puts float value to vertex
                }
                Triangulator tr = new Triangulator(vertices2D);
                int[] indices = tr.Triangulate();

                // Create the Vector3 vertices
                Vector3[] vertices = new Vector3[vertices2D.Length];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 1);
                }

                // Create the mesh
                Mesh msh = new Mesh();
                msh.vertices = vertices;
                msh.triangles = indices;
                msh.RecalculateNormals();
                msh.RecalculateBounds();

                GameObject obj = new GameObject();

                // Set up game object with mesh;
                obj.AddComponent(typeof(MeshRenderer));
                MeshFilter filter = obj.AddComponent(typeof(MeshFilter)) as MeshFilter;
                filter.mesh = msh;

                obj.transform.parent = camera.transform;

                inputPolygon[polygonCnt++] = vertices2D;
            } 
        }
        map.polygons = inputPolygon;

        //get start positions of guards. Assumes:
                        //Number of guards = number of start pos
                        //Guard start & end pos arrive in same order
                        //Can throw this into above forloop later (but it's nice to have seperate)
        float[][] start_positions = new float[numberofGuards][];
        int grdID = 0;
        foreach (var pair in input.polygon)
        {
            if (pair.Key.StartsWith("start_pos"))
            {
 //               Debug.Log(pair.Key + " Number of Guards: " + grdID);
                var start_pos = pair.Value.ToObject<float[]>();
                start_positions[grdID] = start_pos;
                grdID++;
            }
        }

        float[][] end_positions = new float[numberofGuards][];
        grdID = 0;
        foreach (var pair in input.polygon)
        {
            if (pair.Key.StartsWith("goal_pos"))
            {
  //              Debug.Log(pair.Key + " Number of Guards: " + grdID);
                var end_pos = pair.Value.ToObject<float[]>();
                end_positions[grdID] = end_pos;
                grdID++;
            }
        }

        float[][] items = new float[itemcount][];
        grdID = 0;
        foreach (var pair in input.polygon)
        {
            if (pair.Key.StartsWith("item_"))
            {
 //               Debug.Log(pair.Key + " Number of Guards: " + grdID);
                var item_pos = pair.Value.ToObject<float[]>();
                items[grdID] = item_pos;
                grdID++;
            }
        }
        map.items = items;

        //calculate new positions
        //guardSets = Utils.getStartPositions(items, numberofGuards, map);

        //Run task 2
        data = Utils.getPositionsT2(items,numberofGuards,map, 3, start_positions);
        for (int i = 0; i < numberofGuards; i++)
        {
            //start_positions[i] = new float[] { guardSets[i].center[0], guardSets[i].center[1] };
            //start_positions[i] = new float[] {data.waypoints[i].point[0], data.waypoints[i].point[1] };
        }

        //Create players

        this.point = new Point[numberofGuards];
        for (int i = 0; i < numberofGuards; i++)
        {            
            Debug.Log("Guard Number: " + i);
            point[i] = CreateAI();
            
            //point[i].useSaved = useSaved;

            //point[i].startPos = input.start_pos; //need to update for multiple
            point[i].startPos = start_positions[i];
            point[i].transform.position = new Vector3(start_positions[i][0], start_positions[i][1], 20);
            point[i].goalPos = end_positions[i];
            point[i].startVel = input.start_vel;
            point[i].goalVel = input.goal_vel;

            point[i].MAX_SPEED = input.v_max;
            point[i].MAX_ACCEL = input.a_max;
            point[i].MAX_OMEGA = input.omega_max;
            point[i].MAX_PHI = input.phi_max;
            point[i].L_CAR = input.L_car;
            point[i].K_FRICTION = input.k_friction;
            point[i].guardID = i;

            point[i].polygons = inputPolygon;

            if (task == 2)
            {
                point[i].waypoint = data.waypoints[i];
            }
        }


        // Power of Cheetah

        //Cheetah.instance.CreateOrLoad(problem, boundaryPolygon, inputPolygon);

    }
	
	// Update is called once per frame
	void Update () {
        

    }
}

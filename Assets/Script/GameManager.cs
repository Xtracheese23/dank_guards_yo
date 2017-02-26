using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameManager : MonoBehaviour {
    public Point point;
    public Point point2;
    public Map map;
    public GameObject ai;
    public GameObject mapai;
    public GameObject camera;
    public bool useSaved;
    public string problem;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (point)
        {
            Gizmos.DrawCube(new Vector3(point.startPos[0], point.startPos[1], 20), new Vector3(0.5F, 0.5F, 0));
            Gizmos.DrawCube(new Vector3(point.goalPos[0], point.goalPos[1], 20), new Vector3(0.5F, 0.5F, 0));
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
        string json = "";
        using (StreamReader r = new StreamReader(problem))
        {
            json = r.ReadToEnd();
            // List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
        }
        Input input = JsonConvert.DeserializeObject<Input>(json);

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
        
        Vector2[][] inputPolygon = new Vector2[input.polygon.Count][];
        int polygonCnt = 0;
        Debug.Log(input.a_max);
        //Debug.Log(input.polygon["polygon0"] is JArray);
        foreach (var pair in input.polygon)
        {
            float[][] polygon = pair.Value.ToObject<float[][]>();
            Vector2[] vertices2D = new Vector2[polygon.Length];
            for (int i = 0; i < polygon.Length; i++)
            {
                vertices2D[i] = new Vector2(polygon[i][0], polygon[i][1]);
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
        map.polygons = inputPolygon;

        //Create players
        //List<Point> guards = new List<Point>();
        //for (int i = 0; i < 4; i++)
        //{
        int guardNumber = 0; 
        Debug.Log("Guard Number: " + guardNumber);
            point = CreateAI();
            //guards.Add(point);

            point.useSaved = useSaved;

            point.startPos = input.start_pos; //need to update for multiple
            point.goalPos = input.goal_pos;
            point.startVel = input.start_vel;
            point.goalVel = input.goal_vel;

            point.MAX_SPEED = input.v_max;
            point.MAX_ACCEL = input.a_max;
            point.MAX_OMEGA = input.omega_max;
            point.MAX_PHI = input.phi_max;
            point.L_CAR = input.L_car;
            point.K_FRICTION = input.k_friction;
            point.guardID = guardNumber;

            point.polygons = inputPolygon;
        //}

        guardNumber++;

        Debug.Log("Guard Number: " + guardNumber);

        point2 = CreateAI();

        point2.useSaved = useSaved;

        //point2.startPos = input.start_pos; //need to update for multiple
        //point2.startPos = input.start_pos;
        //point2.startPos[0] = guardNumber;
        //point2.startPos[1] = guardNumber;
        point2.startPos = new float[2] {guardNumber, guardNumber };
        point2.transform.position = new Vector3(guardNumber, guardNumber, 1);//;guardNumber;
        //point2.transform.position.x = guardNumber;
        point2.goalPos = input.goal_pos;
        point2.startVel = input.start_vel;
        point2.goalVel = input.goal_vel;

        point2.MAX_SPEED = input.v_max;
        point2.MAX_ACCEL = input.a_max;
        point2.MAX_OMEGA = input.omega_max;
        point2.MAX_PHI = input.phi_max;
        point2.L_CAR = input.L_car;
        point2.K_FRICTION = input.k_friction;
        point2.guardID = guardNumber;

        point2.polygons = inputPolygon;


        // Power of Cheetah

        //Cheetah.instance.CreateOrLoad(problem, boundaryPolygon, inputPolygon);

    }
	
	// Update is called once per frame
	void Update () {
        

    }
}

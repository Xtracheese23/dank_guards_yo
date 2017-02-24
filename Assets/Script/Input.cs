using System;
using Newtonsoft.Json;
using System.Collections.Generic;

[Serializable]
public class Input
{
    public float L_car;
    public float a_max;
    public float[][] boundary_polygon;
    public float[] goal_pos, goal_vel, start_pos, start_vel;
    public float k_friction, omega_max, phi_max, v_max;

    [JsonExtensionData]
    public IDictionary<string, Newtonsoft.Json.Linq.JToken> polygon;
}
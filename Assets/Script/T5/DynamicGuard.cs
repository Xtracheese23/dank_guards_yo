﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KDTreeDLL;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class DynamicGuard : Point
{
    private const float DELTA_T = 0.01F;
    public Vector2 guardPos = new Vector2(0F,0F);
    private bool inFinalInput = false;
    private Vector2 acc;
    private float vox, voy;
    private PID[] pid;
    private bool collision = false;
    private float t_run = 0F;
    private float t_dur = 0F;
    private Vector2 coll_acc;
    private bool initialRush = true;

    private float[][] integral;
    private float[][] prev_error;

    /*private Vector2 acc = new Vector2();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;  //why the hell do they have the American spelling of color with the English spelling of grey?
        Gizmos.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + acc.x*100, transform.position.y + acc.y * 100, 20));
    }*/

    Vector2 ObstacleAvoid()
    {
        var avoid = new Vector2(0F,0F);
        foreach (var poly in this.polygons)     
        {
            float dist = Mathf.Infinity;
            var dirn = new Vector2(Mathf.Infinity, Mathf.Infinity);
            for (int i = 0; i < poly.Length; i++)   //each poly defines a new polygon, only need to return closest side
            {  
                int j = i+1;
                if (j >= poly.Length)    
                {
                    j = 0;  //hax
                }
                var closestpnt = ClosestPointOnLine(new Vector3(poly[i][0], poly[i][1],0F), new Vector3(poly[j][0], poly[j][1], 0F), transform.position);
                var dist2 = Vector2.Distance(transform.position, closestpnt);
                if (dist2 < dist)
                {
                    dist = dist2;
                    if (closestpnt == new Vector3(poly[i][0], poly[i][1], 0F))
                    {
                        dirn = new Vector2(this.transform.position.x - poly[i][0], this.transform.position.y - poly[i][1]);
                    }
                    else if (closestpnt == new Vector3(poly[i][0], poly[i][1], 0F))
                    {
                        dirn = new Vector2(this.transform.position.x - poly[j][0], this.transform.position.y - poly[j][1]);
                    }
                    else
                    {
                        dirn = new Vector2(poly[j][0] - poly[i][0], poly[i][1] - poly[j][1]);        //if the guard decides to suicide into the wall, this is the reason
                        dirn.Normalize();                       // ^ p2x - p1x, p1y - p2y
                        dirn *= dist2;
                    }
                }                                 
            }
            if (dirn.x !=0)
                dirn.x = Mathf.Pow(5 / dirn.x, 2); 

            if (dirn.y != 0)
                dirn.y = Mathf.Pow(5 / dirn.y, 2);
            avoid += dirn;
        }
        return avoid;
    }

    Vector2 AvoidWalls()
    {
        float dist = Mathf.Infinity;
        var avoid = new Vector2(0F, 0F);
        for (int i = 0; i < boundaryPolygon.Length; i++)    //for each wall segment
        {
            int j = (i + 1) % (boundaryPolygon.Length);
            var dirn = new Vector2(Mathf.Infinity, Mathf.Infinity);
            var closestpnt = ClosestPointOnLine(new Vector3(boundaryPolygon[i][0], boundaryPolygon[i][1], 0F), new Vector3(boundaryPolygon[j][0], boundaryPolygon[j][1], 0F), transform.position);
            var dist2 = Vector2.Distance(transform.position, closestpnt);
            if (dist2 < dist)
            {
                dist = dist2;
                dirn = new Vector2(boundaryPolygon[i][1] - boundaryPolygon[j][1], boundaryPolygon[j][0] - boundaryPolygon[i][0]);        //if the guard decides to suicide into the wall, this is the reason
                dirn.Normalize();                       // ^ p2x - p1x, p1y - p2y
                dirn *= dist2;
            }
            if (dirn.x != 0)
                dirn.x = Mathf.Pow(20 / (dirn.x*10), 2);

            if (dirn.y != 0)
                dirn.y = Mathf.Pow(20 / (dirn.y * 10), 2);
            avoid += dirn;
        }
        return avoid;
    }

    Vector2 GoalComponent()
    {
        var x = this.goalPos[0] - this.transform.position.x;
        var y = this.goalPos[1] - this.transform.position.y;
        return new Vector2 (x,y);
    }


    Vector2 FormationComponent(bool rush)
    {
        float x = 0, y = 0;
        int j = 0, iteration = 0;
        for (int i = 0; i < this.formation.Count+1; i++) //9 max guards
        {
            if(i == this.guardID)
            {
                j++;    //hax
                continue;
            }
            var gObj = GameObject.Find("Guard" + i);
            if (gObj)
            {
                var pos = gObj.transform.position;
                var xerr = (pos.x - this.transform.position.x) - this.formation[i-j].x;
                var yerr = (pos.y - this.transform.position.y) - this.formation[i-j].y;
                x += PIDs(xerr, iteration, 0, rush);  //i = 0-4, need 0-6 -> doesn't matter, so long as consistent
                y += PIDs(yerr, iteration, 1, rush);


                if (float.IsNaN(x) || float.IsNaN(y))
                {
                    Debug.Log("Fak");
                }
                //PID fubar = new PID();
                //fubar.error;
                //Debug.Log("x Error between " + this.guardID + " and " + i + " is " + x);
                //Debug.Log("y Error between " + this.guardID + " and " + i + " is " + y);
            }
            iteration++;
        }
        this.formationError = new Vector2(x, y);
        return formationError;
    }

    float PIDs(float error, int connection, int xy, bool rush) //0 = x, 1 = y
    {
        if (!rush)
        {
            if (error < 1)
                integral[connection][xy] = 0F;
            integral[connection][xy] += integral[connection][xy] + (error * Time.deltaTime);
        }
        var derivative = (error - prev_error[connection][xy]) / Time.deltaTime;
        var output = Kp * error + Ki * integral[connection][xy] + Kd * derivative;
        prev_error[connection][xy] = error;
        return output;
    }


    void InitiatePIDs()
    {
        //int j = 0;
        integral = new float[formation.Count][];
        prev_error = new float[formation.Count][];
        for (int i = 0; i < integral.Length; i++)
        {
            integral[i] = new float[] { 0, 0 };
            prev_error[i] = new float[] { 0, 0 };
        }
    }

    Vector3 GetInput()
    {
        //var pid: PID;    // Set values in the inspector.
        //var correction = pid.Update(setSpeed, actualSpeed, Time.deltaTime);

        var goalcomp = GoalComponent();
        var formcomp = FormationComponent(false);
        var obsavoid = ObstacleAvoid();
        var edgeavoid = AvoidWalls();
        //Debug.Log("Guard ID: " + guardID + ", Error: " + Mathf.Sqrt(Mathf.Pow(obsavoid.x,2) + Mathf.Pow(obsavoid.y,2)));
        //Debug.Log("Guard: "+guardID+" Edge: (" + edgeavoid.x +", "+edgeavoid.y +")");

        var x = goalcomp.x + formcomp.x + obsavoid.x + edgeavoid.x;
        var y = goalcomp.y + formcomp.y + obsavoid.y + edgeavoid.y;

        var acc = new Vector2(x, y);
        if (acc.magnitude > MAX_ACCEL)
        {
            acc.Normalize();
            acc *= MAX_ACCEL;
        }

        var dt = Time.deltaTime;
        vel += acc * dt;
        if (vel.magnitude > MAX_SPEED)
        {
            vel.Normalize();
            vel *= MAX_SPEED;
        }

        //we're feeding it position + acceleration componenent, which is wrong
        Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + vel.x, transform.position.y + vel.y, 20), Color.blue);
        Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + acc.x, transform.position.y + acc.y, 20), Color.black);

        return transform.position + new Vector3(vel.x, vel.y, 0F) * dt;
    }

    Vector3 GetInitialInput()
    {
        var formcomp = FormationComponent(true);
        var obsavoid = ObstacleAvoid();
        var edgeavoid = AvoidWalls();

        var x = formcomp.x + edgeavoid.x; // + obsavoid.x;// + edgeavoid.x;
        var y = formcomp.y + edgeavoid.y;// + obsavoid.y;// + edgeavoid.y;

        var acc = new Vector2(x, y);
        if (acc.magnitude > MAX_ACCEL)
        {
            acc.Normalize();
            acc *= MAX_ACCEL;
        }

        var dt = Time.deltaTime;
        vel += acc * dt;
        if (vel.magnitude > MAX_SPEED)
        {
            vel.Normalize();
            vel *= MAX_SPEED;
        }

        //we're feeding it position + acceleration componenent, which is wrong
        Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + vel.x, transform.position.y + vel.y, 20), Color.green);
        Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + acc.x, transform.position.y + acc.y, 20), Color.black);
        return transform.position + new Vector3(vel.x, vel.y, 0F) * dt;
    }

    void Formcheck()
    {
        float x = 0, y = 0;
        int j = 0, iteration = 0;
        for (int i = 0; i < this.formation.Count + 1; i++) //9 max guards
        {
            if (i == this.guardID)
            {
                j++;    //hax
                continue;
            }
            var gObj = GameObject.Find("Guard" + i);
            if (gObj)
            {
                var pos = gObj.transform.position;
                x += (pos.x - this.transform.position.x) - this.formation[i - j].x;
                y += (pos.y - this.transform.position.y) - this.formation[i - j].y;

                if (float.IsNaN(x) || float.IsNaN(y))
                {
                    Debug.Log("Fak");
                }
            }
        }
        var error = new Vector2(x, y).magnitude;
        if (error < 1) //arbitrary
            initialRush = false;
    }

    // Use this for initialization
    void Start()
    {
        vel = new Vector2(startVel[0], startVel[1]);
        InitiatePIDs();
    }

    // Update is called once per frame
    private float totalTime = 0F;
    void Update()
    {
        totalTime += Time.deltaTime;
        UpdatePosition();
    }


    void UpdatePosition()
    {
        float time = totalTime;
        Vector3 input = new Vector3();
        if (inFinalInput == true || Vector3.Distance(transform.position, new Vector3(goalPos[0], goalPos[1], transform.position.z)) < GameManager.endRange)
        {
            input = getFinalInput();    //Final Input
        }
        else
        {
            input = CollisionImminant();
            if (!collision)
            {
                if(initialRush)
                {
                    input = GetInitialInput();
                    Formcheck();   //not inform, in form
                }
                else
                {
                    input = GetInput();     //Regurlar Input
                }
            }
            
        }
        

        transform.position = input;
    }


    Vector3 CollisionImminant()
    {
        //bool collision = false;
        var dt = Time.deltaTime;

        var t_col = this.vel.magnitude / MAX_ACCEL;
        var d = vel.magnitude * dt - MAX_ACCEL * dt * dt * 0.5;

        float dist = Mathf.Infinity;
        if (!collision)
        { 
            var dirn = new Vector2(Mathf.Infinity, Mathf.Infinity);
            foreach (var poly in this.polygons)
            {
                for (int i = 0; i < poly.Length; i++)   //each poly defines a new polygon, only need to return closest side
                {
                    int j = (i + 1)% poly.Length;
                    //if (j >= poly.Length)
                    //{
                    //    j = 0;  //hax
                    //}
                    var closestpnt = ClosestPointOnLine(new Vector3(poly[i][0], poly[i][1], 0F), new Vector3(poly[j][0], poly[j][1], 0F), transform.position);
                    var dist2 = Vector2.Distance(transform.position, closestpnt);
                    if (dist2 < dist)
                    {
                        dist = dist2;
                        if (closestpnt == new Vector3(poly[i][0], poly[i][1], 0F))  //if closest point is the vertices, it is not between the vertices
                        {
                            dirn = new Vector2(this.transform.position.x - poly[i][0], this.transform.position.y - poly[i][1]);
                        }
                        else if (closestpnt == new Vector3(poly[i][0], poly[i][1], 0F))  //if closest point is the vertices, it is not between the vertices
                        {
                            dirn = new Vector2(this.transform.position.x - poly[j][0], this.transform.position.y - poly[j][1]); // might need to swap one of these
                        }
                        else
                        {
                            dirn = new Vector2(poly[j][1] - poly[i][1], poly[i][0] - poly[j][0]); //This is backwards. It works and I don't know why
                            dirn.Normalize();
                            dirn *= MAX_ACCEL;
                        }
                    }
                }

            }
            if (dist < d + 1)
            {
                collision = true;
                t_dur = t_col;
                t_run = 0;
                coll_acc = dirn;
                Debug.Log("COLLISION IMMINANT: GUARD " + guardID);
            }
        }
        if (t_run > t_dur)
            collision = false;
        if (collision) //can't do else, since we need to check this after the first point
        {
            vel += coll_acc * dt;
            t_run += dt;
            Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + vel.x, transform.position.y + vel.y, 20), Color.black);
            Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + coll_acc.x, transform.position.y + coll_acc.y, 20), Color.red);

            return transform.position + new Vector3(vel.x, vel.y, 0F) * dt;
        }
        return new Vector3(0F, 0F, 0F);
    }







    Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
    {
        var vVector1 = vPoint - vA;
        var vVector2 = (vB - vA).normalized;

        var d = Vector3.Distance(vA, vB);
        var t = Vector3.Dot(vVector2, vVector1);
        if (t <= 0)
            return vA;

        if (t >= d)
            return vB;

        var vVector3 = vVector2 * t;

        var vClosestPoint = vA + vVector3;

        return vClosestPoint;
    }


    Vector3 getFinalInput()
    {
     
        if (inFinalInput == false)
        {
            //double tx = prev.pos.x, ty = prev.pos.y, tvx = prev.vel.x, tvy = prev.vel.y;
            var goalpos = new Vector3(goalPos[0], goalPos[1], 0F);
            var goalvel = new Vector2(goalVel[0], goalVel[1]);

            //float t = 2 * Vector3.Distance(goalpos, transform.position) / (goalvel + vel).magnitude;
            float timex = 2 * (goalPos[0] - transform.position.x) / (goalvel[0] + vel.x);
            float timey = 2 * (goalPos[1] - transform.position.y) / (goalvel[1] + vel.y);
            float t = Mathf.Max(timex, timey);
            Debug.Log("Time to dest: " + t);
            var accel = 2 * (goalpos - transform.position - new Vector3(vel[0], vel[1], 0F) * t) / (t * t);

            acc = new Vector2(accel.x, accel.y);
            if (acc.magnitude > MAX_ACCEL)
            {
                acc.Normalize();
                acc *= MAX_ACCEL;
            }
            vox = vel.x;//goalVel[0];//vel.x;
            voy = vel.y; //goalVel[1];//vel.y;
            inFinalInput = true;
        }
        //var dt = Time.deltaTime;
        var dt = Time.deltaTime;
        float tx = transform.position.x, ty = transform.position.y;
        tx = tx + vel.x * dt + acc.x * dt * dt * 0.5F;
        ty = ty + vel.y * dt + acc.y * dt * dt * 0.5F;

        vel += acc * dt;
        if (vel.magnitude > MAX_SPEED)
        {
            vel.Normalize();
            vel *= MAX_SPEED;
        }

        //we're feeding it position + acceleration componenent, which is wrong
        Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + vel.x, transform.position.y + vel.y, 20), Color.red);
        Debug.DrawLine(new Vector3(transform.position.x, transform.position.y, 20), new Vector3(transform.position.x + acc.x, transform.position.y + acc.y, 20), Color.black);

        return new Vector3(tx, ty, 0F);// * Time.deltaTime;
    }
}


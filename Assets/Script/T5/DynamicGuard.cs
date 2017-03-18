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
                dirn.x = Mathf.Pow(20 / dirn.x, 2); 

            if (dirn.y != 0)
                dirn.y = Mathf.Pow(20 / dirn.y, 2);
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


    Vector2 FormationComponent()
    {
        float x = 0, y = 0;
        int j = 0;
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
                x += (pos.x - this.transform.position.x) - this.formation[i-j].x;
                y += (pos.y - this.transform.position.y) - this.formation[i-j].y;

                //Debug.Log("x Error between " + this.guardID + " and " + i + " is " + x);
                //Debug.Log("y Error between " + this.guardID + " and " + i + " is " + y);
            }
        }
        this.formationError = new Vector2(x, y);
        return formationError;
    }


    Vector3 GetInput()
    {
        //var pid: PID;    // Set values in the inspector.
        //var correction = pid.Update(setSpeed, actualSpeed, Time.deltaTime);

        var goalcomp = GoalComponent();
        var formcomp = FormationComponent();
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


    // Use this for initialization
    void Start()
    {
        vel = new Vector2(startVel[0], startVel[1]);
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
            input = getFinalInput();
        }
        else
        {
            input = GetInput();
        }
        

        transform.position = input;
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



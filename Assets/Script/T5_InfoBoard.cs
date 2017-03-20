using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class T5_InfoBoard : MonoBehaviour
{
    public Text info;

    private float totalTime = 0F;
    private float totalError = 0F;
    private float totalcost = 0F;
    private int numberofGuards = 0;
    private bool finished = false;

    string ParseFloat(float f)
    {
        return ((float)((int)(f * 100 + 0.000001)) / 100).ToString("F2");
    }


    string ParseVector(Vector2 v, string heading, bool mag)
    {
        return string.Format("{0}: ({1}, {2})", heading, ParseFloat(v.x), ParseFloat(v.y)) + (mag ? (", " + ParseFloat(v.magnitude)) : "");
    }

    
    float FormationError()
    {
        float err = 0F;
        int numberFin = 0;
        var pos = new Vector3[numberofGuards];
        var goalpos = new float[numberofGuards][];
        //var distances = new float[numberofGuards];
        for (int i = 0; i < numberofGuards; i++)
        {
            var gObj = GameObject.Find("Guard" + i);
            if (gObj)
            {
                pos[i] = gObj.transform.position;
                goalpos[i] = gObj.GetComponent<DynamicGuard>().goalPos;
                if (Vector2.Distance(new Vector2(pos[i][0], pos[i][1]), new Vector2(goalpos[i][0], goalpos[i][1])) < 0.01F)
                    numberFin++;
            }
        }
        if (numberFin >= numberofGuards)
            finished = true;

        float[] errarray = new float[numberofGuards];
        for (int i = 0; i < numberofGuards; i++)        //pos[i] = 0-3 (in order)
        {
            for (int j = 0; j < numberofGuards; j++)        //pos[i] = 0-3 (in order)
            {
                //int j = (i + 1) % (numberofGuards);
                if (i == j)
                {
                    continue;
                }
                var terror = Vector3.Distance(pos[j], pos[i]);    //terror = temp error
                Debug.DrawLine(new Vector3(pos[i][0], pos[i][1], 0F), new Vector3(pos[j][0], pos[j][1], 0F));
                var idealdist = Vector2.Distance(new Vector2(goalpos[j][0], goalpos[j][1]), new Vector2(goalpos[i][0], goalpos[i][1]));
                //need to subtract the ideal distance
                terror = Mathf.Abs(terror) - idealdist;
                terror = Mathf.Pow(terror, 2) * Time.deltaTime;
                //Debug.Log("terror: " + terror);
                err += terror;
            }
        }

        return err;
    }

    // Use this for initialization
    void Start()
    {
        numberofGuards = GameManager.numberofGuards;
        //finished = new bool[numberofGuards];
    }

    void Update()
    {
        if (!finished)
        {
            totalTime += Time.deltaTime;
            totalError += FormationError();
            totalcost = totalTime + totalError;
        }
        
        info.text = ("t: " + ParseFloat(totalTime));
        
        info.text += ("\ne: " + ParseFloat(totalError));

        
        info.text += ("\nC: " + ParseFloat(totalcost));
    }

}


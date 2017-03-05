using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using KDTreeDLL;

public class Test1 : MonoBehaviour
{

    void test()
    {
        Vector2[] interestPoints = new Vector2[25];
        List <Set> sets = new List<Set>();
        int j = 0;
        for(int i=0;i<interestPoints.Length;i++)
        {
            interestPoints[i] = new Vector2(i%5, j);
            if (i%5 == 4)
                j++;
        }

        for (int i = 0; i < interestPoints.Length; i++)
        {
            sets.Add(Utils.pointsInSight(interestPoints[i], (float)1.5, interestPoints));
            //Debug.Log("("+interestPoints[i][0]+","+ interestPoints[i][1]+") Score=" + sets[i].score);
        }
        Set[] bestSets = Utils.findBestSetsUsingGreedy(sets, 3);
    }

    void test2()
    {
        Vector2[] interestPoints = new Vector2[25];
        List<Set> sets = new List<Set>();
        int j = 0;
        for (int i = 0; i < interestPoints.Length; i++)
        {
            interestPoints[i] = new Vector2(i % 5, j);
            if (i % 3 == 2)
                j++;
        }

        for (int i = 0; i < interestPoints.Length; i++)
        {
            sets.Add(Utils.pointsInSight(interestPoints[i], (float)1.5, interestPoints));
            //Debug.Log("("+interestPoints[i][0]+","+ interestPoints[i][1]+") Score=" + sets[i].score);
        }
        Set[] bestSets = Utils.findBestSetsUsingGreedy(sets, 3);
    }


    void testWeights()
    {
        Vector2[] interestPoints = new Vector2[25];
        List<Set> sets = new List<Set>();
        int j = 0;
        for (int i = 0; i < interestPoints.Length; i++)
        {
            interestPoints[i] = new Vector2(i % 5, j);
            if (i % 5 == 4)
                j++;
        }

        for (int i = 0; i < interestPoints.Length; i++)
        {
            sets.Add(Utils.pointsInSight(interestPoints[i], (float)1.5, interestPoints));
            //Debug.Log("("+interestPoints[i][0]+","+ interestPoints[i][1]+") Score=" + sets[i].score);
        }
        Set[] bestSets = Utils.findBestSetsUsingGreedy(sets, 3);
    }


    void Awake()
    {
    }

    // Use this for initialization
    void Start()
    {

        test2();
        //Debug.Log(C.Inverse());
    }

    // Update is called once per frame
    void Update()
    {

    }
}

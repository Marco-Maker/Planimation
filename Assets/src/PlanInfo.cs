using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class PlanInfo
{
    private static PlanInfo instance;
    private static readonly object lockObject = new object();

    private static string pathProblem = "";

    private List<ObjectToAdd> objects;
    private PlanInfo(){}

    public static PlanInfo GetInstance()
    {
        if (instance == null)
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = new PlanInfo();
                }
            }
        }
        return instance;
    }

    public void SetObjects(List<ObjectToAdd> objects)
    {
        this.objects = objects;
    }

    public List<ObjectToAdd> GetObjects()
    {
        return objects;
    }

}
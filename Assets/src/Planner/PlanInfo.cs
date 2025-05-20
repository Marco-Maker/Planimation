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

    private float domainType; // 0 = logistic, 1 = robot, 2 = elevator, with variant 0 = normal, 1 = numeric, 2 = temporal, 3 = event. Example 0.0 --> logistic normal
    private string domainName;

    private List<ObjectToAdd> objects;
    private List<PredicateToAdd> predicates;
    private List<GoalToAdd> goals;

    private List<string> plan;

    public string command = "";
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

    public void SetPredicates(List<PredicateToAdd> predicates)
    {
        this.predicates = predicates;
    }
    public List<PredicateToAdd> GetPredicates()
    {
        return predicates;
    }
    public void SetGoals(List<GoalToAdd> goals)
    {
        this.goals = goals;
    }
    public List<GoalToAdd> GetGoals()
    {
        return goals;
    }

    public void SetPlan(List<string> plan)
    {
        this.plan = plan;
    }
    public List<string> GetPlan()
    {
        return plan;
    }

    public void SetDomainType(float type)
    {
        this.domainType = type;
        domainName = type switch
        {
            0.0f => Const.DOMAIN_LOGISTIC + Const.DOMAIN_NAME_LOGISTIC_NORMAL,
            0.1f => Const.DOMAIN_LOGISTIC + Const.DOMAIN_NAME_LOGISTIC_2_1,
            0.3f => Const.DOMAIN_LOGISTIC + Const.DOMAIN_NAME_LOGISTIC_PLUS,
            1.0f => Const.DOMAIN_ROBOT + Const.DOMAIN_NAME_ROBOT_NORMAL,
            1.2f => Const.DOMAIN_ROBOT + Const.DOMAIN_NAME_ROBOT_2_1,
            1.3f => Const.DOMAIN_ROBOT + Const.DOMAIN_NAME_ROBOT_PLUS,
            2.0f => Const.DOMAIN_ELEVATOR + Const.DOMAIN_NAME_ELEVATOR_NORMAL,
            2.1f => Const.DOMAIN_ELEVATOR + Const.DOMAIN_NAME_ELEVATOR_2_1,
            2.3f => Const.DOMAIN_ELEVATOR + Const.DOMAIN_NAME_ELEVATOR_PLUS,
            _ => throw new ArgumentOutOfRangeException(nameof(type), "Invalid domain type")
        };
    }

    public float GetDomainType()
    {
        return domainType;
    }

    public string GetDomainName()
    {
        return domainName;
    }
}
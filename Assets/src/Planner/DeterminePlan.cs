using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeterminePlan : MonoBehaviour
{
    void Awake()
    {
        switch(PlanInfo.GetInstance().GetDomainType())
        {
            case 0.0f:
                GetComponent<LogisticPlanExecutor>().enabled = true;
                break;
            case 0.1f:
                //GetComponent<LogisticPlanExecutorNumeric>().enabled = true; //Missing domain
                break;
            case 0.2f:
                //GetComponent<LogisticPlanExecutorEvent>().enabled = true; //Missing domain
                break;
            case 1.0f:
                GetComponent<RobotPlanExecutor>().enabled = true;
                break;
            case 1.1f:
                GetComponent<RobotPlanExecutorTemporal>().enabled = true;
                break;
            case 1.2f:
                GetComponent<RobotPlanExecutorEvent>().enabled = true;
                break;
            case 2.0f:
                GetComponent<ElevatorPlanExecutor>().enabled = true;
                break;
            case 2.1f:
                GetComponent<ElevatorPlanExecutorNumeric>().enabled = true;
                break;
            case 2.2f:
                GetComponent<ElevatorPlanExecutorEvent>().enabled = true;
                break;
        }
        ;
}

}

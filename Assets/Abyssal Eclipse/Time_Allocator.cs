using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Time_Allocator : MonoBehaviour
{
    [SerializeField] const int TargetFPS = 60;
    [SerializeField] const int MinFPS = 50;


    public enum Actions
    {
        NPC_Pathfinding,
        NPC_Tasks,
        World_Loading_Unloading
    }

    Dictionary<Actions, List<System.Action>> requestedActions = new Dictionary<Actions, List<System.Action>>();


    float targetTime;
    float minTime;


    // Start is called before the first frame update
    void Start()
    {
        targetTime = 1000 / TargetFPS;
        minTime = 1000 / MinFPS;
        timeLast = Time.realtimeSinceStartup;
    }

    float timeLast;
    public void LateUpdate()
    {
        //time left before fps drops below 60
        float leftOverTime = (1000 / TargetFPS) - (((Time.realtimeSinceStartup) - timeLast) * 1000);
        timeLast = Time.realtimeSinceStartup;
        Debug.Log(leftOverTime);

        while (leftOverTime > 0 && requestedActions.Count > 0)
        {
            float startTime = Time.realtimeSinceStartup;

            //handle Actions


            //end handle Actions

            leftOverTime -= (Time.realtimeSinceStartup - startTime) * 1000;
            if (leftOverTime <= 0)
            {
                leftOverTime += targetTime - minTime;
            }
        }
    }
}

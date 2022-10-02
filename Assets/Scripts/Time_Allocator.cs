using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Time_Allocator;

public class Time_Allocator : MonoBehaviour
{
    [SerializeField] const int TargetFPS = 60;
    [SerializeField] const int MinFPS = 50;


    Queue<System.Action> requestedTasks = new Queue<System.Action>();


    public void AddTask(System.Action task)
    {
        requestedTasks.Enqueue(task);
    }

    public void AddTaskRange(List<System.Action> taskRange)
    {
        foreach (System.Action act in taskRange)
        {
            AddTask(act);
        }
    }






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
        float leftOverTime = (1000 / MinFPS) - (((Time.realtimeSinceStartup) - timeLast) * 1000);
        timeLast = Time.realtimeSinceStartup;
        Debug.Log(leftOverTime);




        while (leftOverTime > 0 && requestedTasks.Count > 0)
        {
            float startTime = Time.realtimeSinceStartup;

            //handle Actions

            requestedTasks.Dequeue().Invoke();

            //end handle Actions

            leftOverTime -= (Time.realtimeSinceStartup - startTime) * 1000;
        }
    }
}

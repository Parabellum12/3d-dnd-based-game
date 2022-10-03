using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Time_Allocator;

public class Time_Allocator : MonoBehaviour
{
    [SerializeField] const int TargetFPS = 60;
    [SerializeField] const int MinFPS = 50;


    Queue<System.Action> highPriorityTask = new Queue<System.Action>();
    Queue<System.Action> mediumPriorityTask = new Queue<System.Action>();
    Queue<System.Action> lowPriorityTask = new Queue<System.Action>();


    public void LowAddTask(System.Action task)
    {
        mediumPriorityTask.Enqueue(task);
    }

    public void LowAddTaskRange(List<System.Action> taskRange)
    {
        foreach (System.Action act in taskRange)
        {
            LowAddTask(act);
        }
    }


    public void MediumAddTask(System.Action task)
    {
        mediumPriorityTask.Enqueue(task);
    }

    public void MediumAddTaskRange(List<System.Action> taskRange)
    {
        foreach (System.Action act in taskRange)
        {
            MediumAddTask(act);
        }
    }
    public void HighPriorityAddTask(System.Action task)
    {
        highPriorityTask.Enqueue(task);
    }

    public void HighPriorityAddTaskRange(List<System.Action> taskRange)
    {
        foreach (System.Action act in taskRange)
        {
            HighPriorityAddTask (act);
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




        while (leftOverTime > 0 && (mediumPriorityTask.Count > 0 || highPriorityTask.Count > 0))
        {
            float startTime = Time.realtimeSinceStartup;

            //handle Actions

            if (highPriorityTask.Count > 0)
            {
                highPriorityTask.Dequeue().Invoke();
            }
            else if (mediumPriorityTask.Count > 0)
            {
                mediumPriorityTask.Dequeue().Invoke();
            }
            else if (lowPriorityTask.Count > 0)
            {
                lowPriorityTask.Dequeue().Invoke();
            }

            //end handle Actions

            leftOverTime -= (Time.realtimeSinceStartup - startTime) * 1000;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public Color LineColor = Color.yellow;
    public Color PointColor = Color.white;
    public Color FirstPointColor = Color.grey;
    public Color LastPointColor = Color.green;

    public List<Transform> Points = new List<Transform>();
    public bool Looping;
    
    public bool SelectPointAfterAdd;

    private void OnDrawGizmos()
    {
        for (int i = 0; i < Points.Count; i++)
        {
            Gizmos.color = LineColor;
            if (i < Points.Count - 1  && Points[i] != null && Points[i+1] != null)
            {
                Gizmos.DrawLine(Points[i].transform.position, Points[i+1].transform.position);
            }

            if (i == Points.Count - 1 && Looping && Points[i] != null && Points[0] != null)
            {
                Gizmos.DrawLine(Points[i].transform.position, Points[0].transform.position);
            }
            
            Gizmos.color = PointColor;
            if(i == 0) Gizmos.color = FirstPointColor;
            if(i == Points.Count - 1) Gizmos.color = LastPointColor;
            if (Points[i] != null)
            {
                Gizmos.DrawSphere(Points[i].transform.position, 0.4f);
            }
        }
    }
}

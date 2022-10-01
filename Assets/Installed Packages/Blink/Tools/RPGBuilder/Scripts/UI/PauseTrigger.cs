using UnityEngine;

public class PauseTrigger : MonoBehaviour
{
   public void StartPause()
   {
      GameEvents.Instance.OnPauseStart();
   }
   
   public void EndPause()
   {
      GameEvents.Instance.OnPauseEnd();
   }
}

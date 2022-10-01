using BLINK.RPGBuilder.LogicMono;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BLINK.RPGBuilder.Managers
{
    public class PlayerInfoDisplayManager : MonoBehaviour
    {
        public void TargetPlayer()
        {
            GameState.playerEntity.SetTarget(GameState.playerEntity);
        }

    }
}
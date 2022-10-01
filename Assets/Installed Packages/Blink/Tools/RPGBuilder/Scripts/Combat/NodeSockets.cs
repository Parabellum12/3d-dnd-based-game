using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeSockets : MonoBehaviour
{
    [System.Serializable]
    public class NodeSocket
    {
        public RPGBNodeSocket nodeSocket;
        public Transform socketTransform;
    }

    public List<NodeSocket> sockets = new List<NodeSocket>();

    public Transform GetSocketTransform(RPGBNodeSocket nodeSocket)
    {
        return (from socket in sockets where socket.nodeSocket == nodeSocket select socket.socketTransform).FirstOrDefault();
    }
}

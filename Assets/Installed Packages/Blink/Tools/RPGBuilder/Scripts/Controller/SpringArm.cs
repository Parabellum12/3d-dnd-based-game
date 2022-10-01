using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.Controller
{
    public class SpringArm : MonoBehaviour
    {
        public float TargetLength = 3.0f;
        public float playerYOffset = 1.75f;
        public float collisionDistance = 1;
        public float distanceBehindCamera = 1.5f;
        public float SpeedDamp = 0.0f;
        public Transform CollisionSocket;
        public LayerMask CollisionMask = 0;
        public Camera Camera;

        private Vector3 _socketVelocity;

        private void LateUpdate()
        {
            if (Camera == null || GameState.playerEntity == null) return;
            Vector3 cameraDir = Camera.transform.rotation * Vector3.back;
            Vector3 behindCamPos = Camera.transform.position + cameraDir * distanceBehindCamera;
            if (Physics.Linecast(new Vector3(GameState.playerEntity.transform.position.x,
                GameState.playerEntity.transform.position.y+playerYOffset,
                GameState.playerEntity.transform.position.z), behindCamPos, out var hit, CollisionMask)
                && Vector3.Distance(transform.position, CollisionSocket.transform.position) <= TargetLength)
            {
                Vector3 dir = CollisionSocket.transform.rotation * Vector3.forward;
                Vector3 newPos = hit.point + dir * collisionDistance;
                CollisionSocket.position = Vector3.SmoothDamp(CollisionSocket.position, newPos, ref _socketVelocity, SpeedDamp * Time.deltaTime);
            }
            else
            {
                Vector3 newSocketLocalPosition = -Vector3.forward * TargetLength;
                CollisionSocket.localPosition = Vector3.SmoothDamp(CollisionSocket.localPosition, newSocketLocalPosition, ref _socketVelocity, SpeedDamp * Time.deltaTime);
            }
        }
    }
}
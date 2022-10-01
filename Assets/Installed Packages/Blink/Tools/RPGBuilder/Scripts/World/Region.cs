using BLINK.RPGBuilder.Managers;
using BLINK.RPGBuilder.Templates;
using UnityEngine;

namespace BLINK.RPGBuilder._THMSV.RPGBuilder.Scripts.World
{
    public class Region : MonoBehaviour
    {
        public WorldData.RegionShapeType shapeType;
        public RegionTemplate RegionTemplate;
        [SerializeField] private  Color sceneColor = Color.green;
        [SerializeField] private  bool showGizmo = true;
    
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject != GameState.playerEntity.gameObject) return;
            WorldEvents.Instance.OnRegionEntered(RegionTemplate);
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject != GameState.playerEntity.gameObject) return;
            RegionManager.Instance.ExitRegion(RegionTemplate);
        }

        private void OnDrawGizmos()
        {
            if (!showGizmo) return;
            Gizmos.color = sceneColor;
            var transform1 = transform;
            if (shapeType == WorldData.RegionShapeType.Cube) DrawCube(transform1.position, transform1.rotation, transform1.localScale);
            else Gizmos.DrawWireSphere(transform1.position, transform1.localScale.y);
        }

        private static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Matrix4x4 cubeTransform = Matrix4x4.TRS(position, rotation, scale);
            Matrix4x4 oldGizmosMatrix = Gizmos.matrix;
 
            Gizmos.matrix *= cubeTransform;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldGizmosMatrix;
        }
    }
}

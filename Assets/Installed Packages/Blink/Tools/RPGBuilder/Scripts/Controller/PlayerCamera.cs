using BLINK.RPGBuilder.Managers;
using UnityEngine;

namespace BLINK.Controller
{
    public class PlayerCamera : MonoBehaviour
    {
        public LayerMask CollisionLayers;
        public float StartDistance = 3.0f;
        public float MinDistance = 1.0f;
        public float MaxDistance = 15.0f;
        public float ZoomInputSensitivity = 15.0f;
        public float DistanceSmoothTime = 0.7f;

        private Camera _camera;
        private RPGBThirdPersonController _controller;
        private Vector3 _pivotOffset;
        private Vector3 _pivotPosition;
        private Vector3 _targetPosition;
        private float _rotationXSmooth;
        private float _rotationYSmooth;
        private float _rotationXCurrentVelocity;
        private float _rotationYCurrentVelocity;
        private float _desiredDistance;
        private float _distanceSmooth;
        private float _distanceCurrentVelocity;
        private float _scrollInput;

        private void Awake()
        {
            _camera = Camera.main;
            _controller = GetComponent<RPGBThirdPersonController>();
            _desiredDistance = StartDistance;
        }

        public void SetScrollInput(float scrollInput)
        {
            _scrollInput = scrollInput;
        }

        public void SetControlRotation(Vector2 controlRotation)
        {
            if (_camera == null || GameState.playerEntity == null) return;
			
            Quaternion rotYaxis = Quaternion.AngleAxis(controlRotation.y, Vector3.up);
            Vector3 desiredPivotPosition = transform.position + rotYaxis * _pivotOffset;
            Vector3 temp = transform.position;
            temp.y = desiredPivotPosition.y;
            Vector3 rayDirection = desiredPivotPosition - temp;

            float closestPivotDistance = Mathf.Infinity;
            if (rayDirection.magnitude != 0)
            {
                float halfFieldOfView = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float halfHeight = _camera.nearClipPlane * Mathf.Tan(halfFieldOfView);
                float halfWidth = halfHeight * _camera.aspect + 0.2f;
                halfHeight += 0.2f;

                Vector3 boxHalfExtents = new Vector3(halfWidth, halfHeight, _camera.nearClipPlane * 0.5f);
                Vector3 boxCenter = temp;
                Quaternion boxOrientation = Quaternion.LookRotation(rayDirection);
                Debug.DrawRay(boxCenter, rayDirection.normalized * (rayDirection.magnitude), Color.magenta);
                RaycastHit[] hitArray = Physics.BoxCastAll(boxCenter, boxHalfExtents, rayDirection, boxOrientation, rayDirection.magnitude, CollisionLayers);
                foreach (RaycastHit hit in hitArray)
                {
                    if (hit.distance < closestPivotDistance)
                    {
                        closestPivotDistance = hit.distance;
                    }
                }
            }

            if (closestPivotDistance != Mathf.Infinity)
            {
                if (closestPivotDistance < 0)
                {
                    closestPivotDistance = 0;
                }
                _pivotPosition = temp + rayDirection.normalized * closestPivotDistance;
            }
            else
            {
                _pivotPosition = desiredPivotPosition;
            }

            _rotationXSmooth = Mathf.SmoothDamp(_rotationXSmooth, controlRotation.y, ref _rotationXCurrentVelocity, 0);
            _rotationYSmooth = Mathf.SmoothDamp(_rotationYSmooth, controlRotation.x, ref _rotationYCurrentVelocity, 0);
            _desiredDistance -= _scrollInput * ZoomInputSensitivity;
            _desiredDistance = Mathf.Clamp(_desiredDistance, MinDistance, MaxDistance);

            _targetPosition = GetPosition(_rotationYSmooth, _rotationXSmooth, _desiredDistance);

            float closestDistance = Mathf.Infinity;
            if (closestPivotDistance != Mathf.Infinity)
            {
                closestDistance = 0;
            }
            else
            {
                closestDistance = CheckForCollision();
            }

            if (closestDistance != Mathf.Infinity)
            {
                closestDistance -= 2.0f * _camera.nearClipPlane;
                if (closestDistance < 0)
                {
                    closestDistance = 0;
                }

                if (_distanceSmooth < closestDistance)
                {
                    _distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, closestDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
                }
                else
                {
                    _distanceSmooth = closestDistance;
                }
            }
            else
            {
                _distanceSmooth = Mathf.SmoothDamp(_distanceSmooth, _desiredDistance, ref _distanceCurrentVelocity, DistanceSmoothTime);
            }

            _camera.transform.position = GetPosition(_rotationYSmooth, _rotationXSmooth, _distanceSmooth);
            if (_distanceSmooth > 0.1f)
            {
                _camera.transform.LookAt(_pivotPosition);
            }
            else
            {
                _camera.transform.rotation = Quaternion.Euler(new Vector3(_rotationYSmooth, _rotationXSmooth, 0));
            }
        }

        private Vector3 GetPosition(float xAxisDegrees, float yAxisDegrees, float distance)
        {
            Vector3 offset = -Vector3.forward * distance;

            Quaternion rotXaxis = Quaternion.AngleAxis(xAxisDegrees, Vector3.right);
            Quaternion rotYaxis = Quaternion.AngleAxis(yAxisDegrees, Vector3.up);

            return _pivotPosition + rotYaxis * rotXaxis * offset;
        }

        private float CheckForCollision()
        {
            float closestDistance = Mathf.Infinity;

            Vector3 rayDirection = _targetPosition - _pivotPosition;
            float rayDistance = rayDirection.magnitude;
            Vector3 targetDirection = rayDirection.normalized;

            if (rayDistance == 0)
            {
                return closestDistance;
            }

            float halfFieldOfView = _camera.fieldOfView * 0.5f * Mathf.Deg2Rad;
            float halfHeight = _camera.nearClipPlane * Mathf.Tan(halfFieldOfView);
            float halfWidth = halfHeight * _camera.aspect + 0.2f;
            halfHeight += 0.2f;

            Vector3 boxHalfExtents = new Vector3(halfWidth, halfHeight, _camera.nearClipPlane * 0.5f);
            Vector3 boxCenter = _pivotPosition;
            Quaternion boxOrientation = Quaternion.LookRotation(rayDirection);
            RaycastHit[] hitArray = Physics.BoxCastAll(boxCenter, boxHalfExtents, rayDirection, boxOrientation, rayDistance - 4.0f * boxHalfExtents.z, CollisionLayers);
            foreach (RaycastHit hit in hitArray)
            {
                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                }
            }

            return closestDistance;
        }

        public void InitCameraPosition(Vector2 controlRotation)
        {
            _rotationXSmooth = controlRotation.y;
            _rotationYSmooth = controlRotation.x;
            _pivotOffset = _controller.CameraSettings.normalPivot;
        }

        public Vector3 GetPivotOffset()
        {
            return _pivotOffset;
        }

        public void SetPivotOffset(Vector3 offset)
        {
            _pivotOffset = offset;
        }

        public Camera GetCamera()
        {
            return _camera;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(_pivotPosition, 0.2f);
        }
    }
}

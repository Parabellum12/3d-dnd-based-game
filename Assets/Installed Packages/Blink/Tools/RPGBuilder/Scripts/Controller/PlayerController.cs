using BLINK.RPGBuilder.Characters;
using UnityEngine;

namespace BLINK.Controller
{
	public class PlayerController : Controller
	{
		public float ControlRotationSensitivity = 3.0f;

		private PlayerInput _playerInput;
		private PlayerCamera _playerCamera;

		public override void Init()
		{
			_playerInput = FindObjectOfType<PlayerInput>();
			_playerCamera = FindObjectOfType<PlayerCamera>();
		}

		public override void OnCharacterUpdate()
		{
			if (!RpgbThirdPersonController.ControllerEssentials.HasMovementRestrictions())
			{
				_playerInput.UpdateInput();
			}
			else
			{
				_playerInput.JumpInput = false;
				RpgbThirdPersonController.SetJumpInput(false);
			}

			if (RpgbThirdPersonController.cameraCanRotate &&
			    !RpgbThirdPersonController.ControllerEssentials.HasRotationRestrictions())
			{
				UpdateControlRotation();
			}

			if (!RpgbThirdPersonController.ControllerEssentials.HasMovementRestrictions())
			{
				RpgbThirdPersonController.SetMovementInput(GetMovementInput());
				RpgbThirdPersonController.SetJumpInput(_playerInput.JumpInput);
			}
		}

		public override void OnCharacterFixedUpdate()
		{
			//if (!RpgbThirdPersonController.cameraCanRotate) return;
			_playerCamera.SetControlRotation(RpgbThirdPersonController.GetControlRotation());
			_playerCamera.SetScrollInput(_playerInput.ScrollInput);
		}

		private void UpdateControlRotation()
		{
			Vector2 camInput = _playerInput.CameraInput;
			Vector2 controlRotation = RpgbThirdPersonController.GetControlRotation();

			// Adjust the pitch angle (X Rotation)
			float pitchAngle = controlRotation.x;
			pitchAngle -= camInput.y * ControlRotationSensitivity;

			// Adjust the yaw angle (Y Rotation)
			float yawAngle = controlRotation.y;
			yawAngle += camInput.x * ControlRotationSensitivity;

			controlRotation = new Vector2(pitchAngle, yawAngle);
			RpgbThirdPersonController.SetControlRotation(controlRotation);
		}

		private Vector3 GetMovementInput()
		{
			Vector3 forward = _playerCamera.GetCamera().transform.forward;;
			Vector3 right = _playerCamera.GetCamera().transform.right;
			if (!RpgbThirdPersonController.isFlying)
			{
				forward.y = 0;
				forward.Normalize();
			}
			
			Vector3 movementInput = (forward * _playerInput.MoveInput.y + right * _playerInput.MoveInput.x);

			if (movementInput.sqrMagnitude > 1f)
			{
				movementInput.Normalize();
			}

			return movementInput;
		}
	}
}
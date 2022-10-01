using UnityEngine;

namespace BLINK.Controller
{
	public class PlayerInput : MonoBehaviour
	{
		[HideInInspector] public float MoveAxisDeadZone = 0.2f;
		[HideInInspector] public bool useNewKeys;
		[HideInInspector] public float smoothTime = 1, dampenTime = 0.2f;
		[HideInInspector] public float smoothInput = 1;

		public Vector2 MoveInput { get; private set; }
		public Vector2 LastMoveInput { get; private set; }
		public Vector2 CameraInput { get; private set; }
		public float ScrollInput { get; private set; }
		public bool JumpInput { get; set; }
		public bool HasMoveInput { get; private set; }
		[HideInInspector] public Vector2 lastMoveInput;

		public void UpdateInput()
		{
			// Update MoveInput
			Vector2 moveInput = new Vector2(0, 0);
			if (!useNewKeys)
			{
				moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
			}
			else
			{
				KeyCode moveForwardKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveForward");
				KeyCode moveBackwardKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveBackward");
				KeyCode moveLeftKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveLeft");
				KeyCode moveRightKey = RPGBuilderUtilities.GetCurrentKeyByActionKeyName("MoveRight");

				if (Input.GetKey(moveForwardKey))
				{
					moveInput.y = 1;
				}

				if (Input.GetKey(moveBackwardKey))
				{
					moveInput.y = -1;
				}

				if (Input.GetKey(moveRightKey))
				{
					moveInput.x = 1;
				}

				if (Input.GetKey(moveLeftKey))
				{
					moveInput.x = -1;
				}
			}

			if (Mathf.Abs(moveInput.x) < MoveAxisDeadZone)
			{
				moveInput.x = 0.0f;
			}

			if (Mathf.Abs(moveInput.y) < MoveAxisDeadZone)
			{
				moveInput.y = 0.0f;
			}

			if (useNewKeys && moveInput.sqrMagnitude > 0.0f)
			{
				moveInput = Vector2.Lerp(lastMoveInput, moveInput, Time.deltaTime * smoothInput);
			}

			lastMoveInput = moveInput;

			bool hasMoveInput = moveInput.sqrMagnitude > 0.0f;

			if (HasMoveInput && !hasMoveInput)
			{
				LastMoveInput = MoveInput;
			}

			if (!useNewKeys)
			{
				GameState.playerEntity.controllerEssentials.anim.SetFloat("MoveDirectionX", moveInput.x);
				GameState.playerEntity.controllerEssentials.anim.SetFloat("MoveDirectionY", moveInput.y);
				if (GameState.playerEntity.IsMounted())
				{
					GameState.playerEntity.GetMountAnimator().SetFloat("MoveDirectionX", moveInput.x);
					GameState.playerEntity.GetMountAnimator().SetFloat("MoveDirectionY", moveInput.y);
				}
			}
			else
			{
				GameState.playerEntity.controllerEssentials.anim.SetFloat("MoveDirectionX", moveInput.x,
					dampenTime, Time.deltaTime * smoothTime);
				GameState.playerEntity.controllerEssentials.anim.SetFloat("MoveDirectionY", moveInput.y,
					dampenTime, Time.deltaTime * smoothTime);
				if (GameState.playerEntity.IsMounted())
				{
					GameState.playerEntity.GetMountAnimator().SetFloat("MoveDirectionX", moveInput.x,
						dampenTime, Time.deltaTime * smoothTime);
					GameState.playerEntity.GetMountAnimator().SetFloat("MoveDirectionY", moveInput.y,
						dampenTime, Time.deltaTime * smoothTime);
				}
			}

			MoveInput = moveInput;
			HasMoveInput = hasMoveInput;

			// Update other inputs
			CameraInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			if (!UIEvents.Instance.CursorHoverUI)
			{
				ScrollInput = Input.GetAxis("Mouse ScrollWheel");
			}

			JumpInput = Input.GetKey(RPGBuilderUtilities.GetCurrentKeyByActionKeyName("Jump"));
		}
	}
}

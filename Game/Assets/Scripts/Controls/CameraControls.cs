using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControls : MonoBehaviour {

	[Header ("Input Actions")]
	public InputAction rightClickIA;
	public InputAction rotationIA;
	public InputAction movementIA;
	public InputAction scrollIA;
	bool rightMouseDown = false;

	[Header ("Anchors")]
	public Transform anchor;
	public Transform zoomAnchor;
	public Transform camTransform;

	[Header ("Paremeters")]
	public float rotationSpeed = 0.1f;
	public float moveSpeed = 4.0f;
	float zoomStep = 1.0f;
	int zoomStepAmount = 16;
	float zoomTime = 0.25f;
	int zoomState = 10;
	Vector3 zoomDirection;
	Vector3 moveDirection;
	Vector2 maxLocation = new Vector2 ();

	void Awake () {

		rightClickIA.started += ctx => {
			rightMouseDown = true;
		};

		rightClickIA.canceled += ctx => {
			rightMouseDown = false;
		};

		rotationIA.performed += ctx => {
			UpdateRotation (ctx.ReadValue<Vector2> ());
		};

		scrollIA.performed += ctx => {
			UpdateZoomTarget (ctx.ReadValue<float> ());
		};
		maxLocation.x = maxLocation.y = Constants.Gameplay.WORLD_SIZE / 2;
		zoomDirection = (zoomAnchor.position - anchor.position).normalized;
		zoomAnchor.localPosition = zoomDirection * zoomState * zoomStep;
	}

	void OnEnable () {
		rightClickIA.Enable ();
		rotationIA.Enable ();
		movementIA.Enable ();
		scrollIA.Enable ();
	}

	void OnDisable () {
		rightClickIA.Disable ();
		rotationIA.Disable ();
		movementIA.Disable ();
		scrollIA.Disable ();
	}

	void Update () {
		UpdateMovement (movementIA.ReadValue<Vector2> ());
		UpadteZoom ();
	}

	void UpdateRotation (Vector2 delta) {
		if (rightMouseDown) {
			float dYRot = delta.x * rotationSpeed;
			anchor.Rotate (0, dYRot, 0);

			float dXRot = -1 * delta.y * rotationSpeed;
			camTransform.Rotate (dXRot, 0, 0);
		}
	}

	void UpdateMovement (Vector2 move) {
		moveDirection = move.x * anchor.right + move.y * anchor.forward;
		anchor.Translate (moveDirection.normalized * moveSpeed * zoomState * Time.deltaTime, Space.World);
		Vector3 finalPosition = anchor.position;
		if (anchor.position.x > maxLocation.x) {
			finalPosition.x = maxLocation.x;
		}
		if (anchor.position.z > maxLocation.y) {
			finalPosition.z = maxLocation.y;
		}
		if (anchor.position.x < -maxLocation.x) {
			finalPosition.x = -maxLocation.x;
		}
		if (anchor.position.z < -maxLocation.y) {
			finalPosition.z = -maxLocation.y;
		}
		anchor.position = finalPosition;
	}

	void UpdateZoomTarget (float direction) {
		direction /= Mathf.Abs (direction);
		zoomState -= (int) direction;
		zoomState = Mathf.Clamp (zoomState, 1, zoomStepAmount);
		zoomAnchor.localPosition = zoomDirection * zoomState * zoomStep;
	}

	void UpadteZoom () {
		Vector3 offset = zoomAnchor.localPosition - camTransform.localPosition;
		float velocity = Vector3.Distance (zoomAnchor.localPosition, camTransform.localPosition) / zoomTime;
		offset = offset.normalized * Time.deltaTime * velocity;
		Vector3 finalPosition = camTransform.localPosition + offset;
		camTransform.localPosition = finalPosition;
	}
}
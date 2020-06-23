using UnityEngine;
using UnityEngine.EventSystems;

public class EntityControls : MonoBehaviour {

	public Managers managers;
	public EventSystem eventSystem;
	public Camera camera;

	public Mode mode;

	void Update () {
		if (false) {
			if (managers.networkManager.connected) {
				return;
			}
			if (eventSystem.IsPointerOverGameObject ()) {
				return;
			}
			RaycastHit hit;
			Ray ray = camera.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				Location clickLocation = managers.entityManager.HitPointToLocation (hit.point);
				if (mode.Equals (Mode.CREATE)) {
					managers.entityManager.RequestEntityCreation (clickLocation);
				} else if (mode.Equals (Mode.CREATE)) {
					managers.entityManager.RequestEntityDeletion (clickLocation);
				}
			}
		}
	}

	public enum Mode {
		INFO,
		CREATE,
		DELETE
	}
}
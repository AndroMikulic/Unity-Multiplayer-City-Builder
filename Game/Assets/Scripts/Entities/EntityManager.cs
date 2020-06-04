using UnityEngine;

public class EntityManager : MonoBehaviour {

	public Managers managers;

	public Mode mode;
	public Camera camera;
	public Location clickPos = new Location (-1, -1);
	public int worldOffset = 256;

	public Entity selectedEntity;
	public int selectedSize = 0;

	public EntitySpawner spawner;

	void Start(){
		Entity e = new Entity();
		e.entityType = EntityType.BUILDING;
		selectedEntity = e;
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			RaycastHit hit;
			Ray ray = camera.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				HitPointToLocation (hit.point);
				if (mode.Equals (Mode.CREATE)) {
					CreateEntity (clickPos);
				}
			}
		}
	}

	void HitPointToLocation (Vector3 point) {
		clickPos.x = (int) point.x + worldOffset;
		clickPos.y = (int) point.z + worldOffset;
	}

	public void CreateEntity (Location location) {
		if (selectedEntity.entityType.Equals (EntityType.BUILDING)) {
			Building building = new Building ();
			building.location = location;
			building.size = selectedSize;
			building.type = (int) Constants.Gameplay.BuildingType.RESIDENTIAL; 
			building.resource = 10;
			building.name = "Kafana";
			building.entityType = EntityType.BUILDING;

			Packet packet = new Packet(Constants.Networking.PacketTypes.ENTITY_CREATE, building);
			managers.networkManager.outboundPackets.Add(packet);
		}
	}

	public enum Mode {
		NONE,
		CREATE,
		DELETE
	}
}
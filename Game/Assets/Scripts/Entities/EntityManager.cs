using System;
using System.Collections.Concurrent;
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
	public EntityRemover remover;
	public Transform entityCollection;
	public long[, ] tileTimestamps = new long[Constants.Gameplay.WORLD_SIZE, Constants.Gameplay.WORLD_SIZE];

	void Start () {
		Entity e = new Entity ();
		e.entityType = EntityType.BUILDING;
		selectedEntity = e;
	}

	void Update () {
		if (Input.GetMouseButtonDown (0)) {
			if (!managers.networkManager.connected) {
				return;
			}
			RaycastHit hit;
			Ray ray = camera.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				HitPointToLocation (hit.point);
				if (mode.Equals (Mode.CREATE)) {
					RequestEntityCreation (clickPos);
				}
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			if (!managers.networkManager.connected) {
				return;
			}
			RaycastHit hit;
			Ray ray = camera.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				HitPointToLocation (hit.point);
				if (mode.Equals (Mode.CREATE)) {
					RequestEntityDeletion (clickPos);
				}
			}
		}
	}

	void HitPointToLocation (Vector3 point) {
		clickPos.x = (int) point.x + worldOffset;
		clickPos.y = (int) point.z + worldOffset;
	}

	public void RequestEntityCreation (Location location) {
		if (selectedEntity.entityType.Equals (EntityType.BUILDING)) {
			Building building = new Building ();
			building.location = location;
			building.size = selectedSize;
			building.type = (int) Constants.Gameplay.BuildingType.RESIDENTIAL;
			building.resource = 10;
			building.name = "Unity HQ";
			building.entityType = EntityType.BUILDING;

			Packet packet = new Packet (Constants.Networking.PacketTypes.ENTITY_CREATE, building);
			managers.networkManager.outboundPackets.Add (packet);
		}
	}

	public void RequestEntityDeletion (Location location) {
		Packet packet = new Packet (Constants.Networking.PacketTypes.ENTITY_DELETE, location);
		managers.networkManager.outboundPackets.Add (packet);
	}

	public bool ValidateTimestamp (long timestamp, Location location) {
		lock (tileTimestamps) {
			int X = location.x - 1;
			int Y = location.y - 1;
			if (tileTimestamps[X, Y] < timestamp) {
				tileTimestamps[X, Y] = timestamp;
				return true;
			}
			return false;
		}
	}

	public enum Mode {
		NONE,
		CREATE,
		DELETE
	}
}
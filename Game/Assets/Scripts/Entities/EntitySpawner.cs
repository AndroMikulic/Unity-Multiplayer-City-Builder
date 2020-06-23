using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour {

	public EntityManager entityManager;

	public BlockingCollection<dynamic> spawnQueue = new BlockingCollection<dynamic> ();
	public int queueCount = 0;

	public Transform referenceFrame;
	public float positionOffset = -0.5f;

	public GameObject[] buildings_size_1;
	public GameObject[] buildings_size_2;
	public GameObject[] buildings_size_3;

	void Start () {
		StartCoroutine (QueueProcessor ());
	}

	IEnumerator QueueProcessor () {
		while (true) {
			if (queueCount > 0) {
				queueCount--;
				dynamic obj = spawnQueue.Take ();
				SpawnEntity (obj);
			}
			yield return null;
		}
	}

	public void AddEntity (dynamic obj) {
		spawnQueue.Add (obj);
		queueCount++;
	}

	void SpawnEntity (dynamic obj) {
		Entity e = Entity.ParseToEntity (obj);
		if (!entityManager.ValidateTimestamp (e.tileTimestamp, e.location)) {
			return;
		}
		if (e.entityType.Equals (EntityType.BUILDING)) {
			SpawnBuilding (Building.ParseToBuilding (obj));
		} else if (e.entityType.Equals (EntityType.ROAD)) {
			SpawnRoad (RoadTile.ParseToRoadTile (e));
		}
	}

	void SpawnBuilding (Building building) {
		Vector3 position = new Vector3 (0, 1, 0);
		position.x = building.location.x + positionOffset;
		position.z = building.location.y + positionOffset;
		GameObject prefab;
		if (building.size == 1 || true) {
			int item = UnityEngine.Random.Range (0, buildings_size_1.Length);
			prefab = Instantiate (buildings_size_1[item], new Vector3 (0, -64, 0), Quaternion.identity);
		}
		prefab.transform.parent = entityManager.entityCollection;
		prefab.GetComponent<BuildingPrefab> ().building = building;
		prefab.transform.localPosition = position;
		prefab.name = building.location.ToString ();
	}

	void SpawnRoad (RoadTile roadTile) {

	}
}
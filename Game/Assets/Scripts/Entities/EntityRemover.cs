using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class EntityRemover : MonoBehaviour {
	public EntityManager entityManager;

	public BlockingCollection<dynamic> removeQueue = new BlockingCollection<dynamic> ();
	public int queueCount = 0;

	void Start () {
		StartCoroutine (QueueProcessor ());
	}

	IEnumerator QueueProcessor () {
		while (true) {
			if (queueCount > 0) {
				queueCount--;
				dynamic obj = removeQueue.Take ();
				DestroyEntity (Entity.ParseToEntity (obj));
			}
			yield return null;
		}
	}

	public void RemoveEntity (dynamic obj) {
		removeQueue.Add (obj);
		queueCount++;
	}

	public void DestroyEntity (Entity entity) {
		if (entityManager.ValidateTimestamp (entity.tileTimestamp, entity.location)) {
			GameObject obj = entityManager.entityCollection.Find (entity.location.ToString ()).gameObject;
			Destroy (obj);
		}
	}
}
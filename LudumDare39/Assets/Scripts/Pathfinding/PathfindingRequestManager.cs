using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingRequestManager : MonoBehaviour {

	public List<Pathfinding> pathfinders;
	int pathfinderIndex;

	void Start () {
		GameObject[] entities = GameObject.FindGameObjectsWithTag("Entity");
		if (entities.Length > 0) {
			for (int i = 0; i < entities.Length; i++) {
				pathfinders.Add(entities[i].GetComponent<Pathfinding>());
			}
		}

		StartCoroutine(UpdatePathfinders());
	}

	IEnumerator UpdatePathfinders () {
		while (true) {
			if (pathfinders.Count > 0) {
				pathfinderIndex = (pathfinderIndex >= pathfinders.Count - 1 ? 0 : pathfinderIndex + 1);
				if (pathfinders[pathfinderIndex] == null) {
					pathfinders.RemoveAt(pathfinderIndex);
					continue;
				} else {
					if (pathfinders[pathfinderIndex].grid != null && pathfinders[pathfinderIndex].targetPosition != null) {
						if (Vector2.Distance(pathfinders[pathfinderIndex].transform.position, pathfinders[pathfinderIndex].targetPosition) <= pathfinders[pathfinderIndex].pathfindingDistanceThreshold) {
							pathfinders[pathfinderIndex].FindPath(pathfinders[pathfinderIndex].transform.position, pathfinders[pathfinderIndex].targetPosition);
						}
					}
				}
			}
			yield return new WaitForSeconds(0.01f);
		}
	}

}

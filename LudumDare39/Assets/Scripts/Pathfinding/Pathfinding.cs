using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour {

	public Vector2 targetPosition;
	public Transform targetTransform;

	public Grid grid;

	public List<Node> path;

	public float pathfindingDistanceThreshold;

	bool doPathfind = true;

	void Awake () {
		grid = GameObject.Find("World Grid").GetComponent<Grid>();
	}

	public void FindPath (Vector3 startPos, Vector3 targetPos) {

		Node startNode = grid.GetNodeFromWorldPoint(startPos);
		Node targetNode = grid.GetNodeFromWorldPoint(targetPos);

		Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
		HashSet<Node> closedSet = new HashSet<Node>();

		openSet.Add(startNode);

		while (openSet.Count > 0) {

			Node currentNode = openSet.RemoveFirst(); ;
			closedSet.Add(currentNode);

			if (currentNode == targetNode) {
				RetracePath(startNode, targetNode);
				return;
			}

			foreach (Node neighbour in grid.GetNeighbours(currentNode)) {
				if (!neighbour.walkable || closedSet.Contains(neighbour)) {
					continue;
				}

				int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
					neighbour.gCost = newMovementCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour)) {
						openSet.Add(neighbour);
					}
				}
			}
		}
	}

	void RetracePath (Node startNode, Node endNode) {
		List<Node> newPath = new List<Node>();
		Node currentNode = endNode;

		while (currentNode != startNode) {
			newPath.Add(currentNode);
			currentNode = currentNode.parent;
		}

		newPath.Reverse();

		path = newPath;
	}

	int GetDistance (Node nodeA, Node nodeB) {
		int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

		if (distX > distY) {
			return (14 * distY) + (10 * (distX - distY));
		} else {
			return (14 * distX) + (10 * (distY - distX));
		}
	}

}

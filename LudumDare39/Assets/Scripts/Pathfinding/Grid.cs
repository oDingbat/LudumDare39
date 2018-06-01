using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {

	public Transform player;

	public LayerMask unwalkableMask;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public Node[,] grid;

	float nodeDiameter;
	int gridSizeX;
	int gridSizeY;

	void Awake () {
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
	}

	public int MaxSize {
		get {
			return gridSizeX * gridSizeY;
		}
	}

	public void CreateGrid () {
		grid = new Node[gridSizeX, gridSizeY];
		Vector2 worldBottomLeft = (Vector2)transform.position - (Vector2.right * gridWorldSize.x/2) - (Vector2.up * gridWorldSize.y/2);

		for (int x = 0; x < gridSizeX; x++) {
			for (int y = 0; y < gridSizeY; y++) {
				Vector2 worldPoint = worldBottomLeft + (Vector2.right * (x * nodeDiameter + nodeRadius)) + (Vector2.up * (y * nodeDiameter + nodeRadius));
				bool walkable = !Physics2D.OverlapCircle(worldPoint, nodeRadius * 0.95f, unwalkableMask);
				grid[x, y] = new Node(walkable, worldPoint, x, y);
			}
		}
	}

	public List<Node> GetNeighbours(Node node) {
		List<Node> neighbours = new List<Node>();

		/*
		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if (x == 0 && y == 0) {
					continue;
				} else {
					int checkX = node.gridX + x;
					int checkY = node.gridY + y;

					if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
						neighbours.Add(grid[checkX, checkY]);
					}
				}
			}
		}
		*/

		for (int x = -1; x <= 1; x++) {
			for (int y = -1; y <= 1; y++) {
				if ((x == 0 && y == 0) || (x != 0 && y != 0)) {
					continue;
				} else {
					int checkX = node.gridX + x;
					int checkY = node.gridY + y;

					if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY) {
						neighbours.Add(grid[checkX, checkY]);
					}
				}
			}
		}

		return neighbours;
	}

	public Node GetNodeFromWorldPoint(Vector3 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		return grid[x, y];
	}

	public Node GetClosestWalkableNode (Vector2 worldPosition) {
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.y + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);

		Node walkableNode = null;

		if (grid[x, y].walkable == true) {
			return grid[x, y];
		} else {
			for (int r = 1; r <= 3; r++) {
				for (int i = -1; i <= 1; i++) {
					for (int j = -1; j <= 1; j++) {
						if (i == 0 && j == 0) {
							continue;
						} else {
							if ((x + (i * r)) >= 0 && (x + (i * r)) < gridSizeX && (y + (j * r)) >= 0 && (y + (j * r)) < gridSizeY) {
								if (grid[x + (i * r), y + (j * r)].walkable == true) {
									return grid[x + (i * r), y + (j * r)];
								}
							}
						}
					}
				}
			}
		}

		return walkableNode;

	}

	void OnDrawGizmos () {
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, gridWorldSize.y, 0));

		if (grid != null) {
			Node playerNode = GetNodeFromWorldPoint(player.position);
			foreach (Node n in grid) {
				if (playerNode == n) {
					Gizmos.color = Color.cyan;
				} else {
					Gizmos.color = (n.walkable ? Color.white : Color.red);
				}

				//Gizmos.DrawWireCube(n.worldPosition, nodeDiameter * Vector3.one * 0.95f);
			}
		}
	}

}

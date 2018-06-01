using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public Entity player;
	public List<Entity> demons;
	public List<Entity> humans;

	public AudioManager audioManager;
	public PathfindingRequestManager PRM;
	public Grid grid;

	public GameObject[] buildings;

	float buildingProbability = 0.5f;

	int waveNumber;
	float timeBetweenWaves = 15;
	int initialHumans = 10;

	int score;

	public Transform spikePrefab;

	public GameObject enemyDemon;
	public GameObject enemySuicider;
	public GameObject human;
	public GameObject priest;

	public AudioClip clipThankYou;

	public NumberLabel scoreCounter;

	void Start () {
		GetInitialEntities();
		GenerateBounds();
		GenerateBuildings();

		scoreCounter.UpdateLabel(score);
		grid.CreateGrid();
		audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

		StartCoroutine(StartGame());
	}

	void GetInitialEntities () {
		GameObject[] allEntities = GameObject.FindGameObjectsWithTag("Entity");

		foreach (GameObject ent in allEntities) {
			if (ent.layer == LayerMask.NameToLayer("Human")) {
				humans.Add(ent.GetComponent<Entity>());
			} else if (ent.layer == LayerMask.NameToLayer("Demon")) {
				demons.Add(ent.GetComponent<Entity>());
			} else if (ent.layer == LayerMask.NameToLayer("Player")) {
				player = ent.GetComponent<Entity>();
			}
		}
	}

	void GenerateBounds() {
		for (float x = 0; x <= grid.gridWorldSize.x + 0.5f; x += 0.5f) {
			for (float y = 0; y <= grid.gridWorldSize.y + 0.5f; y+=0.5f) {
				if (x == 0 || y == 0 || x == grid.gridWorldSize.x + 0.5f || y == grid.gridWorldSize.y + 0.5f) {
					Transform newSpike = Instantiate(spikePrefab, new Vector3((x - grid.gridWorldSize.x / 2) - 0.25f, (y - grid.gridWorldSize.y / 2) - 0.25f), Quaternion.identity);
					newSpike.transform.parent = transform;
				} else {
					continue;
				}
			}
		}
	}

	void GenerateBuildings () {
		int BuildingTilesX = (int)Mathf.Floor(grid.gridWorldSize.x / 10);
		int BuildingTilesY = (int)Mathf.Floor(grid.gridWorldSize.y / 10);

		for (float x = 0; x < BuildingTilesX; x++) {
			for (float y = 0; y < BuildingTilesY; y++) {
				if (Random.Range(0f, 1f) <= buildingProbability) {
					GameObject newBuilding = Instantiate(buildings[Random.Range(0, buildings.Length)], new Vector3(((x * 10) - grid.gridWorldSize.x / 2) + 5, ((y * 10) - grid.gridWorldSize.y / 2) + 5), Quaternion.identity);
					newBuilding.transform.parent = transform;
				}
			}
		}
	}

	IEnumerator StartGame () {
		SpawnHumans();

		yield return new WaitForSeconds(3);

		while (true) {
			waveNumber++;
			SpawnEnemies();
			yield return new WaitForSeconds(timeBetweenWaves + Mathf.Sqrt(waveNumber * 2));
			ReproduceHumans();
			yield return new WaitForSeconds(2f);
		}
	}

	void SpawnHumans () {
		for (int i = 0; i < initialHumans; i++) {
			int randomX = Random.Range(-2, 2);
			int randomY = Random.Range(-2, 2);
		
			GameObject newHuman = Instantiate((i == 0 ? priest : human), new Vector2((randomX * 10) + 5, (randomY * 10) + 5), Quaternion.identity);
			PRM.pathfinders.Add(newHuman.GetComponent<Pathfinding>());
			humans.Add(newHuman.GetComponent<Entity>());
		}
	}

	void SpawnEnemies () {
		for (int i = 0; i < Mathf.Clamp(Mathf.Log(waveNumber, 1.5f) * 2, 1, 100) + (waveNumber * 1.5f); i++) {
			float suiciderProbability = Random.Range(0f, 1f);

			GameObject newEnemy = null;

			if (waveNumber > 3) {
				if (suiciderProbability > 0.9f || (waveNumber % 5 == 0 && suiciderProbability > 0.2f)) {
					newEnemy = Instantiate(enemySuicider, Vector3.zero, Quaternion.identity);
				} else {
					newEnemy = Instantiate(enemyDemon, Vector3.zero, Quaternion.identity);
				}
			} else {
				newEnemy = Instantiate(enemyDemon, Vector3.zero, Quaternion.identity);
			}

			PRM.pathfinders.Add(newEnemy.GetComponent<Pathfinding>());
			demons.Add(newEnemy.GetComponent<Entity>());

			int k = Random.Range(0, 2);

			if (k == 0) {
				newEnemy.transform.position = new Vector3((grid.gridWorldSize.x / 2) * Mathf.Sign(Random.Range(-1f, 1f)), grid.gridWorldSize.y * Random.Range(-0.5f, 0.5f), 0);
			} else {
				newEnemy.transform.position = new Vector3(grid.gridWorldSize.x * Random.Range(-0.5f, 0.5f), (grid.gridWorldSize.y / 2) * Mathf.Sign(Random.Range(-1f, 1f)), 0);
			}

		}
	}

	void ReproduceHumans () {
		int childrenCount = (int)Mathf.Ceil((float)humans.Count / 2);

		for (int i = 0; i < humans.Count; i++) {
			if (childrenCount == 0) {
				break;
			} else {
				childrenCount--;
				GameObject newHuman = Instantiate(((i % 3 == 0) ? priest : human), humans[Random.Range(0, humans.Count)].transform.position + (Vector3.up * 0.1f), Quaternion.identity);
				PRM.pathfinders.Add(newHuman.GetComponent<Pathfinding>());
				humans.Add(newHuman.GetComponent<Entity>());
				audioManager.PlayJukeboxAtPoint(clipThankYou, newHuman.transform.position, 0.75f);
			}
		}
	}

	public Entity GetClosestDemon (Vector3 point) {
		Entity closestDemon = null;
		float closestDistance = Mathf.Infinity;

		for (int i = 0; i < demons.Count; i++) {
			if (demons.Count > 0) {
				if (demons[i] == null) {
					demons.RemoveAt(i);
					i--;
					continue;
				} else {
					if (Vector2.Distance(point, demons[i].transform.position) < closestDistance) {
						closestDistance = Vector2.Distance(point, demons[i].transform.position);
						closestDemon = demons[i];
					}
				}
			}
		}

		return closestDemon;
	}

	public Entity GetClosestHumanOrPlayer(Vector3 point) {

		Entity closestHumanOrPlayer = null;
		float closestDistance = Mathf.Infinity;

		if (Vector2.Distance(point, player.transform.position) < 7) {
			closestDistance = Vector2.Distance(point, player.transform.position);
			closestHumanOrPlayer = player;
		} else {
			if (humans.Count > 0) {
				for (int i = 0; i < humans.Count; i++) {
					if (humans[i] == null) {
						humans.Remove(humans[i]);
						i--;
						continue;
					} else {
						if (Vector2.Distance(point, humans[i].transform.position) < closestDistance) {
							closestDistance = Vector2.Distance(point, humans[i].transform.position);
							closestHumanOrPlayer = humans[i];
						}
					}
				}
			}

			if (Vector2.Distance(point, player.transform.position) <= closestDistance) {
				closestDistance = Vector2.Distance(point, player.transform.position);
				closestHumanOrPlayer = player;
			}
		}

		return closestHumanOrPlayer;
	}

	public void UpdateScore () {
		score++;
		scoreCounter.UpdateLabel(score);
	}

}

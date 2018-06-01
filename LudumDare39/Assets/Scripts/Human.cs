using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), (typeof(Entity)))]
public class Human : MonoBehaviour {

	Rigidbody2D rigidbody;
	Entity entity;
	Pathfinding pathfinding;

	public GameManager gameManager;
	public BackgroundController bgController;
	public AudioManager audioManager;

	public GameObject feet;
	Animator feetAnimator;
	Animator bodyAnimator;

	public GameObject soulPrefab;

	public Vector2 velocityDesired;
	public Vector2 velocityCurrent;

	public GameObject bloodSplatterPrefab;

	public GameObject bodyPrefab;
	public Sprite[] bodies;

	public AudioClip clipHit;
	public AudioClip clipDeath;

	float speed = 6.5f;
	public int soulCount = 1;
	public bool isPriest = false;

	void Start() {
		rigidbody = GetComponent<Rigidbody2D>();
		entity = GetComponent<Entity>();
		pathfinding = GetComponent<Pathfinding>();
		feetAnimator = feet.GetComponent<Animator>();
		bodyAnimator = GetComponent<Animator>();
		gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
		bgController = GameObject.Find("Background").GetComponent<BackgroundController>();

		rigidbody.mass = Random.Range(75, 125);

		audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

		entity.deathEvent += OnEntityDeath;
		entity.damageEvent += OnEntityDamage;
	}

	void Update() {

		Entity closestDemon = gameManager.GetClosestDemon(transform.position);
		if (closestDemon != null && Vector2.Distance(transform.position, gameManager.player.transform.position) < 20 && Vector2.Distance(transform.position, closestDemon.transform.position) < 12) {
			pathfinding.targetTransform = closestDemon.transform;
			Vector2 closestDemonDirection = (pathfinding.targetTransform.transform.position - transform.position).normalized;
			pathfinding.targetPosition = pathfinding.grid.GetClosestWalkableNode((Vector2)transform.position - (closestDemonDirection * 10)).worldPosition;
		} else {
			pathfinding.targetPosition = pathfinding.grid.GetClosestWalkableNode((Vector2)gameManager.player.transform.position + (Vector2)(Quaternion.Euler(0, 0, ((Time.timeSinceLevelLoad * 100) % 360f) * (gameManager.humans.IndexOf(entity) + 1)) * Vector2.right * 4)).worldPosition;
		}
		
		Vector2 nextPathPoint = pathfinding.targetPosition;

		if (isPriest == true) {
			if (Vector2.Distance(transform.position, gameManager.player.transform.position) < 3) {
				speed = 3f;
			} else {
				speed = 6.5f;
			}
		}

		if (pathfinding.path != null && pathfinding.path.Count > 0) {
			if (pathfinding.path.Count > 1 && Vector2.Distance(pathfinding.path[0].worldPosition, transform.position) < 0.3f) {
				pathfinding.path.RemoveAt(0);
			}
			nextPathPoint = pathfinding.path[0].worldPosition;
		}

		velocityDesired = new Vector2(nextPathPoint.x - transform.position.x, nextPathPoint.y - transform.position.y).normalized;
		velocityCurrent = Vector2.Lerp(velocityCurrent, velocityDesired * speed, 100 * Time.deltaTime);

		rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, velocityCurrent, 5 * Time.deltaTime);
		transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocityCurrent.y, velocityCurrent.x));

		feet.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocityCurrent.y, velocityCurrent.x));
		feetAnimator.speed = rigidbody.velocity.magnitude / speed;
		bodyAnimator.speed = rigidbody.velocity.magnitude / speed;
	}

	void OnCollisionStay2D(Collision2D coll) {
		if (coll.transform.gameObject.layer == LayerMask.NameToLayer("Demon")) {
			if (coll.transform.gameObject.GetComponent<Demon>()) {
				entity.TakeDamage(1, transform.position - coll.transform.position, "Demon");
			}
		}
	}

	void OnEntityDamage(Vector2 direction, string damageTag) {
		if (entity.health > 0) {

			audioManager.PlayJukeboxAtPoint(clipHit, transform.position, 0.5f);

			rigidbody.velocity += direction * 8;

			int forwardBloodSplats = Random.Range(8, 13);

			for (int a = 0; a < forwardBloodSplats; a++) {
				Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
				float randomVelocityMagnitude = Random.Range(0.25f, 2f);
				GameObject newBloodSplatter = (GameObject)Instantiate(bloodSplatterPrefab, transform.position, randomRotation);
				BloodSplatter newBloodSplatterClass = newBloodSplatter.GetComponent<BloodSplatter>();
				newBloodSplatterClass.velocity = randomRotation * (direction * randomVelocityMagnitude * 75);
				newBloodSplatterClass.bgController = bgController;
				newBloodSplatterClass.waitDistance = Random.Range(0.5f, 2.5f);
				newBloodSplatterClass.fadeDistance = Random.Range(2f, 3f);
			}
		}
	}


	void OnEntityDeath(Vector2 direction, string damageTag) {
		audioManager.PlayJukeboxAtPoint(clipDeath, transform.position, 0.75f);
		GameObject newBody = (GameObject)Instantiate(bodyPrefab, new Vector3(Mathf.Round(transform.position.x * 8) / 8 + 0.0625f, Mathf.Round(transform.position.y * 8) / 8 + 0.0625f, 0.5f), Quaternion.Euler(0, 0, 90 * Random.Range(0, 4)));
		newBody.GetComponent<SpriteRenderer>().sprite = bodies[Random.Range(0, bodies.Length)];

		gameManager.UpdateScore();

		if (damageTag != "Demon") {
			for (int s = 0; s < soulCount; s++) {
				GameObject newSoul = (GameObject)Instantiate(soulPrefab, transform.position, Quaternion.identity);
				if (soulCount > 1) {
					newSoul.GetComponent<Rigidbody2D>().velocity = new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f));
				}
				gameManager.PRM.pathfinders.Add(newSoul.GetComponent<Pathfinding>());
			}
		}

		int forwardBloodSplats = Random.Range(15, 20);

		for (int a = 0; a < forwardBloodSplats; a++) {
			Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-20f, 20f));
			float randomVelocityMagnitude = Random.Range(0.25f, 2f);
			GameObject newBloodSplatter = (GameObject)Instantiate(bloodSplatterPrefab, transform.position, randomRotation);
			BloodSplatter newBloodSplatterClass = newBloodSplatter.GetComponent<BloodSplatter>();
			newBloodSplatterClass.velocity = randomRotation * (direction * randomVelocityMagnitude * 75);
			newBloodSplatterClass.bgController = bgController;
			newBloodSplatterClass.waitDistance = Random.Range(0.5f, 2.5f);
			newBloodSplatterClass.fadeDistance = Random.Range(2f, 3f);
		}

		gameManager.humans.Remove(entity);

		Destroy(gameObject);
	}
}

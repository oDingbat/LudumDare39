using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody2D), (typeof (Entity)))]
public class Demon : MonoBehaviour {

	public LayerMask visionMask;
	Rigidbody2D rigidbody;
	Entity entity;
	Pathfinding pathfinding;

	public GameManager gameManager;
	public BackgroundController bgController;
	public AudioManager audioManager;

	public GameObject feet;
	Animator feetAnimator;

	public Vector2 velocityDesired;
	public Vector2 velocityCurrent;

	public GameObject bloodSplatterPrefab;

	public GameObject bodyPrefab;
	public Sprite[] bodies;

	public AudioClip clipHit;
	public AudioClip clipDeath;

	float speed = 8;

	void Start () {
		rigidbody = GetComponent<Rigidbody2D>();
		entity = GetComponent<Entity>();
		pathfinding = GetComponent<Pathfinding>();
		feetAnimator = feet.GetComponent<Animator>();
		gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
		bgController = GameObject.Find("Background").GetComponent<BackgroundController>();

		rigidbody.mass = Random.Range(75, 125);

		audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();

		entity.deathEvent += OnEntityDeath;
		entity.damageEvent += OnEntityDamage;
	}

	void Update () {

		pathfinding.targetTransform = gameManager.GetClosestHumanOrPlayer(transform.position).transform;
		pathfinding.targetPosition = pathfinding.targetTransform.position;

		Vector2 nextPathPoint = pathfinding.targetPosition;

		RaycastHit2D hit = Physics2D.Raycast(transform.position, (pathfinding.targetPosition - (Vector2)transform.position).normalized, 100f, visionMask);

		if (hit && hit.transform == pathfinding.targetTransform) {
			if (pathfinding.path != null && pathfinding.path.Count > 1 && Vector2.Distance(transform.position, pathfinding.path[0].worldPosition) < 0.35f) {
				pathfinding.path.RemoveAt(0);
			}
			nextPathPoint = pathfinding.targetPosition;
		} else {
			if (pathfinding.path != null && pathfinding.path.Count > 0) {
				if (pathfinding.path.Count > 1 && Vector2.Distance(pathfinding.path[0].worldPosition, transform.position) < 0.35f) {
					pathfinding.path.RemoveAt(0);
				}
				nextPathPoint = pathfinding.path[0].worldPosition;
			}
		}

		velocityDesired = new Vector2(nextPathPoint.x - transform.position.x, nextPathPoint.y - transform.position.y).normalized;
		velocityCurrent = Vector2.Lerp(velocityCurrent, velocityDesired * speed, 100 * Time.deltaTime);

		rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, velocityCurrent, 5 * Time.deltaTime);
		transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocityCurrent.y, velocityCurrent.x));

		feet.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocityCurrent.y, velocityCurrent.x));
		feetAnimator.speed = rigidbody.velocity.magnitude / speed;
	}

	void OnEntityDamage (Vector2 direction, string damageTag) {
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


	void OnEntityDeath (Vector2 direction, string damageTag) {
		audioManager.PlayJukeboxAtPoint(clipDeath, transform.position, 0.75f);
		GameObject newBody = (GameObject)Instantiate(bodyPrefab, new Vector3(Mathf.Round(transform.position.x * 8) / 8 + 0.0625f, Mathf.Round(transform.position.y * 8) / 8 + 0.0625f, 0.5f), Quaternion.Euler(0, 0, 90 * Random.Range(0, 4)));
		newBody.GetComponent<SpriteRenderer>().sprite = bodies[Random.Range(0, bodies.Length)];

		gameManager.demons.Remove(entity);
		gameManager.UpdateScore();

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

		Destroy(gameObject);
	}
}

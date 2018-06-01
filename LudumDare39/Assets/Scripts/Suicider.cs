using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Rigidbody2D), (typeof (Entity)))]
public class Suicider : MonoBehaviour {

	Rigidbody2D rigidbody;
	Entity entity;
	Pathfinding pathfinding;

	public LayerMask visionMask;

	public GameManager gameManager;
	public BackgroundController bgController;
	public AudioManager audioManager;

	public LayerMask blockLayermask;
	public Sprite destroyedBlock;

	public GameObject feet;
	Animator feetAnimator;

	public Vector2 velocityDesired;
	public Vector2 velocityCurrent;

	public GameObject bloodSplatterPrefab;
	public GameObject flakPrefab;

	public GameObject bodyPrefab;
	public Sprite[] bodies;

	public AudioClip clipHit;
	public AudioClip clipDeath;
	public AudioClip clipYell;

	float speed = 10;
	bool suiciding = false;

	void Start () {
		rigidbody = GetComponent<Rigidbody2D>();
		entity = GetComponent<Entity>();
		pathfinding = GetComponent<Pathfinding>();
		feetAnimator = feet.GetComponent<Animator>();
		gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
		bgController = GameObject.Find("Background").GetComponent<BackgroundController>();

		GetComponent<AudioSource>().pitch = Random.Range(0.9f, 1.1f);

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

		if (Vector2.Distance(transform.position, pathfinding.targetTransform.position) < 4f) {
			if (pathfinding.targetTransform.gameObject.layer == LayerMask.NameToLayer("Human") || pathfinding.targetTransform.GetComponent<Entity>().health > 0) {
				StartCoroutine(Suicide());
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

	IEnumerator Suicide () {
		if (suiciding == false) {
			suiciding = true;
			GetComponent<AudioSource>().Stop();
			GetComponent<AudioSource>().loop = false;
			GetComponent<AudioSource>().clip = clipYell;
			GetComponent<AudioSource>().volume = 0.95f;
			GetComponent<AudioSource>().Play();
			yield return new WaitForSeconds(0.55f);
			OnEntityDeath(Vector2.zero, "Suicide");
		}
	}

	void OnEntityDeath (Vector2 direction, string damageTag) {
		audioManager.PlayJukeboxAtPoint(clipDeath, transform.position, 1f);
		GameObject newBody = (GameObject)Instantiate(bodyPrefab, new Vector3(Mathf.Round(transform.position.x * 8) / 8 + 0.0625f, Mathf.Round(transform.position.y * 8) / 8 + 0.0625f, 0.5f), Quaternion.Euler(0, 0, 90 * Random.Range(0, 4)));
		newBody.GetComponent<SpriteRenderer>().sprite = bodies[Random.Range(0, bodies.Length)];

		gameManager.UpdateScore();
		gameManager.demons.Remove(entity);

		Explode();

		Destroy(gameObject);
	}

	void Explode () {

		Collider2D[] blocksNearby = Physics2D.OverlapCircleAll(transform.position, 2.5f, blockLayermask);

		foreach (Collider2D block in blocksNearby) {
			block.GetComponent<SpriteRenderer>().sprite = destroyedBlock;
			block.enabled = false;
			pathfinding.grid.GetNodeFromWorldPoint(block.transform.position).walkable = true;
			block.GetComponent<SpriteRenderer>().sortingOrder = -50;
			block.transform.position += new Vector3(0, 0, 0.5f);
		}

		int forwardBloodSplats = Random.Range(55, 65);
		for (int a = 0; a < forwardBloodSplats; a++) {
			Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
			float randomVelocityMagnitude = Random.Range(0.25f, 2f);
			GameObject newBloodSplatter = (GameObject)Instantiate(bloodSplatterPrefab, transform.position, randomRotation);
			BloodSplatter newBloodSplatterClass = newBloodSplatter.GetComponent<BloodSplatter>();
			newBloodSplatterClass.velocity = randomRotation * (Vector2.right * randomVelocityMagnitude * 75);
			newBloodSplatterClass.bgController = bgController;
			newBloodSplatterClass.waitDistance = Random.Range(0.5f, 2.5f);
			newBloodSplatterClass.fadeDistance = Random.Range(2f, 3f);
		}

		int flakCount = 32;
		for (int b = 0; b < flakCount; b++) {
			Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
			float randomVelocityMagnitude = Random.Range(1.75f, 2f);
			GameObject newFlak = (GameObject)Instantiate(flakPrefab, transform.position, randomRotation);
			Projectile newFlakProjectile = newFlak.GetComponent<Projectile>();
			newFlakProjectile.velocity = randomRotation * (Vector2.right * randomVelocityMagnitude * 35);
			newFlakProjectile.ricochets = 100;

		}
	}
}

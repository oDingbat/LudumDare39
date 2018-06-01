using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Entity), typeof(Pathfinding), typeof(Rigidbody2D))]
public class Soul : MonoBehaviour {

	public LayerMask collisionMask;

	Pathfinding pathfinding;
	GameManager gameManager;
	public AudioManager audioManager;

	public Rigidbody2D rigidbody;

	float moveSpeed = 15f;

	public AudioClip clipSoulCollection;

	void Start () {
		pathfinding = GetComponent<Pathfinding>();
		audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
		gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
		rigidbody = GetComponent<Rigidbody2D>();
	}

	void Update () {
		pathfinding.targetTransform = gameManager.player.transform;
		pathfinding.targetPosition = pathfinding.targetTransform.position;

		RaycastHit2D hit = Physics2D.Raycast(transform.position, (pathfinding.targetPosition - (Vector2)transform.position).normalized, 100f, collisionMask);

		if (hit && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player")) {
			if (pathfinding.path != null && pathfinding.path.Count > 1 && Vector2.Distance(transform.position, pathfinding.path[0].worldPosition) < 0.25f) {
				pathfinding.path.RemoveAt(0);
			}
			rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, (pathfinding.targetPosition - (Vector2)transform.position).normalized * moveSpeed, 10 * Time.deltaTime);
		} else {
			if (pathfinding.path != null && pathfinding.path.Count > 0) {
				if (pathfinding.path.Count > 1 && Vector2.Distance(transform.position, pathfinding.path[0].worldPosition) < 0.25f) {
					pathfinding.path.RemoveAt(0);
				}
				rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, (pathfinding.path[0].worldPosition - transform.position).normalized * moveSpeed, 10 * Time.deltaTime);
			}
		}
	}

	void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.layer == LayerMask.NameToLayer("Player")) {
			gameManager.player.GetComponent<Player>().GetSoul();
			audioManager.PlayJukeboxAtPoint(clipSoulCollection, transform.position, 0.6f);
			Destroy(gameObject);
		}
    }

}

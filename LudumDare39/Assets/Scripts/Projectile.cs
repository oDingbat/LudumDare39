using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	public int damage;
	public Vector2 velocity;
	public int ricochets;
	public int piercings;

	bool broken = false;

	public bool damagePlayer = false;

	public float projectileRadius;

	public LayerMask collisionMask;

	public string damageTag;

	void Start () {
		transform.position += (Vector3)velocity.normalized * 0.01f;
	}

	void Update () {
		if (broken == false) {
			bool projectileHit = false;
			RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity.normalized, velocity.magnitude * Time.deltaTime, collisionMask);

			if (hit) {
				projectileHit = true;
				if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Block")) {
					if (ricochets > 0) {
						transform.position = hit.point + hit.normal * 0.01f;
						velocity = Vector2.Reflect(velocity, hit.normal);
					} else {
						transform.position = hit.point;
						StartCoroutine(BreakProjectile());
					}
				} else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Demon")) {
					transform.position = hit.point;
					hit.transform.gameObject.GetComponent<Entity>().TakeDamage(damage, velocity.normalized, damageTag);
					if (piercings > 0) {
						piercings--;
						transform.position += (Vector3)velocity.normalized * 0.5f;
					} else {
						StartCoroutine(BreakProjectile());
					}
				} else if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Human")) {
					transform.position = hit.point;
					hit.transform.gameObject.GetComponent<Entity>().TakeDamage(damage, velocity.normalized, damageTag);
					if (piercings > 0) {
						piercings--;
						transform.position += (Vector3)velocity.normalized * 0.5f;
					} else {
						StartCoroutine(BreakProjectile());
					}
				} else if (damagePlayer == true && hit.transform.gameObject.layer == LayerMask.NameToLayer("Player")) {
					Debug.Log("Hit Player");
					transform.position = hit.point;
					hit.transform.gameObject.GetComponent<Entity>().TakeDamage(damage, velocity.normalized, damageTag);
					if (piercings > 0) {
						piercings--;
						transform.position += (Vector3)velocity.normalized * 0.5f;
					} else {
						StartCoroutine(BreakProjectile());
					}
				}
			}

			if (projectileHit == false) {
				transform.position += (Vector3)velocity * Time.deltaTime;
			}

			velocity = Vector2.Lerp(velocity, Vector2.zero, 15 * Time.deltaTime);
		}
	}

	IEnumerator BreakProjectile () {
		broken = true;
		yield return new WaitForSeconds(0.05f);
		Destroy(this);
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodSplatter : MonoBehaviour {

	public LayerMask collisionMask;
	public BackgroundController bgController;

	public Vector2 velocity;
	public float waitDistance;
	public float fadeDistance;

	Vector2 posLastFrame = Vector2.zero;
	Vector2 startPos;

	bool broken = false;

	public Color bloodColor;

	void Start () {
		posLastFrame = transform.position;
		startPos = transform.position;
	}

	void Update() {
		if (broken == false) {
			RaycastHit2D hit = Physics2D.Raycast(transform.position, velocity.normalized, velocity.magnitude * Time.deltaTime, collisionMask);
			if (hit) {
				transform.position = hit.point;
				UpdateBloodSplatter();
				Destroy(gameObject);
			} else {
				transform.position += (Vector3)velocity * Time.deltaTime;
				UpdateBloodSplatter();
			}
		}

		velocity = Vector2.Lerp(velocity, Vector2.zero, 20 * Time.deltaTime);
		posLastFrame = transform.position;

		if (velocity.magnitude < 0.1) {
			UpdateBloodSplatter();
			if (Random.Range(0, 10) > 8) {
				bgController.SetPixelAtPoint(transform.position, bloodColor);
			}
			Destroy(gameObject);
		}
	}

	void UpdateBloodSplatter () {
		int x = (int)Mathf.Round(Vector2.Distance(transform.position, posLastFrame) * 16);
		Vector2 direction = ((Vector2)transform.position - posLastFrame).normalized;

		for (int i = 0; i < x; i++) {
			if (bgController) {
				Vector2 newPos = posLastFrame + ((direction / 16) * i);
				if (Vector2.Distance(newPos, startPos) >= waitDistance) {
					if (Vector2.Distance(newPos, startPos) >= fadeDistance) {
						if (Random.Range(0f, fadeDistance * 1.5f) > Vector2.Distance(newPos, startPos) - fadeDistance) {
							bgController.SetPixelAtPoint(newPos, bloodColor);
						}
					} else {
						if (Random.Range(0f, 10f) > 7) {
							bgController.SetPixelAtPoint(newPos, bloodColor);
						}
					}
				}
			}
		}
	}

}

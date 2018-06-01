using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Entity : MonoBehaviour {

	public int health;
	public float timeLastHit;

	public float damageCooldown = 0;

	public void TakeDamage (int damage, Vector2 direction, string damageTag) {
		if (timeLastHit + damageCooldown < Time.timeSinceLevelLoad) {
			health -= damage;
			timeLastHit = Time.timeSinceLevelLoad;

			if (damageEvent != null) {
				damageEvent(direction, damageTag);
			}

			if (health <= 0) {
				if (deathEvent != null) {
					deathEvent(direction, damageTag);
				}
			}
		}
	}

	public event Action<Vector2, string> damageEvent;

	public event Action<Vector2, string> deathEvent;

}

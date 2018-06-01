using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Weapon {

	public float firerate = 3;

	public int damage;
	public int ammo;

	public float rechargeRate;

	public float accuracy;

	public int projectileCount;
	public int projectileVelocity;
	public int projectileRicochets;
	public int projectilePiercings;
	public GameObject projectilePrefab;

}

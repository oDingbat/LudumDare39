using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent (typeof (Rigidbody2D))]
public class Player : MonoBehaviour {

	public Rigidbody2D rigidbody;
	public GameObject worldCamera;
	public BackgroundController bgController;

	public GameObject crosshair;

	Entity entity;

	public LayerMask blockLayermask;
	public Sprite destroyedBlock;

	public GameObject feet;
	public Animator feetAnimator;

	public Camera cursorCamera;

	Vector2 mouseWorldPosition;

	public Grid grid;

	AudioSource audioSource;
	GameManager gameManager;
	AudioManager audioManager;
	public AudioClip shoot;
	public AudioClip clipPlayerHit;
	public AudioClip clipAmmo;
	public AudioClip clipDeath;
	public AudioClip clipExplosiveShot;
	public AudioClip clipNotEnoughSouls;
	public AudioClip clipFail;

	public Transform ammoBar;

	public int souls;

	Vector2 velocityDesired;
	Vector2 velocityCurrent;

	Vector2 cameraPositionCurrent = new Vector3(0, 0, -10);

	public Weapon weapon;

	public GameObject bloodSplatterPrefab;

	public NumberLabel soulCounter;
	public NumberLabel ammoCounter;
	public NumberLabel healthCounter;

	public GameObject angelFlakPrefab;
	public GameObject bodyPrefab;

	bool isDead = false;

	public GameObject tooltipQ;
	public GameObject tooltipE;

	public GameObject pressRToRestart;

	// Player Attributes
	float playerSpeed = 7;
	
	void Start () {
		Cursor.visible = false;
		rigidbody = GetComponent<Rigidbody2D>();
		feetAnimator = feet.GetComponent<Animator>();
		audioSource = GetComponent<AudioSource>();
		entity = GetComponent<Entity>();

		soulCounter.UpdateLabel(souls);
		ammoCounter.UpdateLabel(weapon.ammo);
		healthCounter.UpdateLabel(entity.health);

		ammoBar.transform.localScale = new Vector3(Mathf.Clamp(weapon.ammo * 4, 0, 64), 1, 1);

		gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
		audioManager = GameObject.Find("Audio Manager").GetComponent<AudioManager>();
		bgController = GameObject.Find("Background").GetComponent<BackgroundController>();

		entity.deathEvent += OnDeath;
		entity.damageEvent += OnDamage;

		StartCoroutine(BleedUpdate());
	}

	// Update is called once per frame
	void Update () {
		if (isDead == false) {
			MovePlayer();
			MoveFeet();
			GetInput();
		} else if (Input.GetKeyDown(KeyCode.R)) {
			SceneManager.LoadScene("Playing");
		}
		GetMousePosition();
		MoveCamera();
	}

	IEnumerator BleedUpdate () {
		while (true) {
			yield return new WaitForSeconds(Random.Range(0.25f, 0.75f));
			if (entity.health < 3) {
				int forwardBloodSplats = 50;
				for (int a = 0; a < forwardBloodSplats; a++) {
					Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
					float randomVelocityMagnitude = Random.Range(1.5f, 2f);
					GameObject newBloodSplatter = (GameObject)Instantiate(bloodSplatterPrefab, transform.position, randomRotation);
					BloodSplatter newBloodSplatterClass = newBloodSplatter.GetComponent<BloodSplatter>();
					newBloodSplatterClass.velocity = randomRotation * (Vector2.right * randomVelocityMagnitude * 2);
					newBloodSplatterClass.bgController = bgController;
					newBloodSplatterClass.waitDistance = Random.Range(0.5f, 2.5f);
					newBloodSplatterClass.fadeDistance = Random.Range(2f, 3f);
				}
			}
		}
	}

	void GetMousePosition () {
		mouseWorldPosition = cursorCamera.ScreenToWorldPoint(Input.mousePosition);
		mouseWorldPosition = new Vector2(Mathf.Round(mouseWorldPosition.x * 8) / 8, Mathf.Round(mouseWorldPosition.y * 8) / 8);
		mouseWorldPosition += new Vector2(0.0625f, 0.0625f);
		crosshair.transform.position = new Vector3(Mathf.Round(mouseWorldPosition.x * 8) / 8, Mathf.Round(mouseWorldPosition.y * 8) / 8, 0);
	}

	void MovePlayer () {
		velocityDesired = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		velocityDesired.Normalize();
		velocityCurrent = Vector3.Lerp(velocityCurrent, velocityDesired, 25 * Time.deltaTime);

		transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(transform.position.y - mouseWorldPosition.y, transform.position.x - mouseWorldPosition.x));

		rigidbody.velocity = velocityCurrent * playerSpeed;
	}
	
	void MoveFeet () {
		feet.transform.position = transform.position;
		feet.transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * Mathf.Atan2(velocityCurrent.y, velocityCurrent.x));
		feetAnimator.speed = velocityCurrent.magnitude * (playerSpeed / 4);
	}
	
	void MoveCamera () {
		cameraPositionCurrent = Vector3.Lerp(cameraPositionCurrent, transform.position, 5 * Time.deltaTime);
		worldCamera.transform.position = new Vector3(cameraPositionCurrent.x, cameraPositionCurrent.y, -10);
	}

	void GetInput () {
		if (Input.GetMouseButtonDown(0)) {
			FireWeapon();
		}

		if (Input.GetMouseButtonDown(1)) {
			FireWeaponAlt();
		}

		if (Input.GetKeyDown(KeyCode.R)) {
			entity.TakeDamage(entity.health, Vector2.zero, "Suicide");
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

		Transform closestPriest = null;
		float closestDistance = 2;

		foreach (Entity human in gameManager.humans) {
			if (human.gameObject.GetComponent<Human>().isPriest == true) {
				if (Vector2.Distance(transform.position, human.transform.position) <= closestDistance) {
					closestDistance = Vector2.Distance(transform.position, human.transform.position);
					closestPriest = human.transform;
				}
			}
		}

		if (closestPriest != null) {

			tooltipQ.SetActive(true);
			tooltipE.SetActive(true);

			tooltipQ.transform.position = new Vector3((Mathf.Round(closestPriest.position.x * 8) / 8) - 0.5f, Mathf.Round(closestPriest.position.y * 8) / 8, 0);
			tooltipE.transform.position = new Vector3((Mathf.Round(closestPriest.position.x * 8) / 8) + 0.5f, Mathf.Round(closestPriest.position.y * 8) / 8, 0);

			if (Input.GetKeyDown(KeyCode.E)) {
				if (souls > 0) {
					audioSource.PlayOneShot(clipAmmo);

					souls--;
					weapon.ammo += 10;

					soulCounter.UpdateLabel(souls);
					ammoCounter.UpdateLabel(weapon.ammo);
					ammoBar.transform.localScale = new Vector3(Mathf.Clamp(weapon.ammo * 4, 0, 64), 1, 1);
				} else {
					closestPriest.GetComponent<AudioSource>().PlayOneShot(clipNotEnoughSouls, 0.5f);
				}
			}

			if (Input.GetKeyDown(KeyCode.Q)) {
				if (souls > 9) {
					audioSource.PlayOneShot(clipAmmo);

					souls-= 10;
					entity.health += 1;

					soulCounter.UpdateLabel(souls);
					healthCounter.UpdateLabel(entity.health);
				} else {
					closestPriest.GetComponent<AudioSource>().PlayOneShot(clipNotEnoughSouls, 0.5f);
				}
			}
		} else {
			tooltipQ.SetActive(false);
			tooltipE.SetActive(false);
		}
	}

	void FireWeapon () {
		if (weapon.ammo > 0) {

			audioSource.pitch = (weapon.ammo > 20 ? 1 : 1 + (Mathf.Abs((20 - weapon.ammo) * 0.1f)));
			audioSource.PlayOneShot(shoot);

			weapon.ammo--;
			ammoCounter.UpdateLabel(weapon.ammo);
			ammoBar.transform.localScale = new Vector3(Mathf.Clamp(weapon.ammo * 4, 0, 64), 1, 1);

			Quaternion randomRotation = Quaternion.Euler(0, 0, Mathf.Abs(weapon.accuracy - 1) * 45 * Random.Range(-1f, 1f));

			GameObject newProjectile = (GameObject)Instantiate(weapon.projectilePrefab, transform.position + transform.right * -0.5f, randomRotation * transform.rotation);
			Projectile newProjectileClass = newProjectile.GetComponent<Projectile>();
			newProjectileClass.damage = weapon.damage;
			newProjectileClass.velocity = randomRotation * (weapon.projectileVelocity * 0.8f * transform.right * -0.5f);
			newProjectileClass.ricochets = weapon.projectileRicochets;
			newProjectileClass.piercings = weapon.projectilePiercings;
		}
	}

	void FireWeaponAlt () {
		if (weapon.ammo > 4) {

			audioSource.pitch = (weapon.ammo > 20 ? 1 : 1 + (Mathf.Abs((20 - weapon.ammo) * 0.1f)));
			audioSource.PlayOneShot(clipExplosiveShot);

			weapon.ammo-=5;
			ammoCounter.UpdateLabel(weapon.ammo);
			ammoBar.transform.localScale = new Vector3(Mathf.Clamp(weapon.ammo * 4, 0, 64), 1, 1);
			int projectileCount = 10;

			for (int i = 0; i < projectileCount; i++) {
				Quaternion randomRotation = Quaternion.Euler(0, 0, Mathf.Abs((weapon.accuracy * 0.2f) - 1) * 45 * Random.Range(-1f, 1f));

				GameObject newProjectile = (GameObject)Instantiate(weapon.projectilePrefab, transform.position + transform.right * -0.25f, randomRotation * transform.rotation);
				Projectile newProjectileClass = newProjectile.GetComponent<Projectile>();
				newProjectileClass.damage = weapon.damage;
				newProjectileClass.velocity = randomRotation * (weapon.projectileVelocity * transform.right * -0.5f);
				newProjectileClass.ricochets = weapon.projectileRicochets;
				newProjectileClass.piercings = weapon.projectilePiercings;
			}
		}
	}

	void OnCollisionStay2D(Collision2D coll) {
		if (isDead == false) {
			if (coll.transform.gameObject.layer == LayerMask.NameToLayer("Demon")) {
				if (coll.transform.gameObject.GetComponent<Demon>()) {
					entity.TakeDamage(1, (transform.position - coll.transform.position).normalized, "Demon");
				}
			} else if (coll.transform.gameObject.layer == LayerMask.NameToLayer("Spikes")) {
				entity.TakeDamage(1, ((Vector2)(coll.transform.position - transform.position) + velocityDesired.normalized).normalized * 0.5f, "Spikes");
			}
		}
	}

	public void GetSoul () {
		souls++;
		soulCounter.UpdateLabel(souls);
	}

	public void OnDamage (Vector2 direction, string damageTag) {
		audioManager.PlayJukeboxAtPoint(clipPlayerHit, transform.position, 0.95f);

		healthCounter.UpdateLabel(entity.health);

		int forwardBloodSplats = Random.Range(25, 30);

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

	public void OnDeath (Vector2 direction, string damageTag) {
		audioManager.PlayJukeboxAtPoint(clipDeath, transform.position, 1f);
		GameObject newBody = (GameObject)Instantiate(bodyPrefab, new Vector3(Mathf.Round(transform.position.x * 8) / 8, Mathf.Round(transform.position.y * 8) / 8, 0.5f), Quaternion.identity);

		healthCounter.UpdateLabel(entity.health);

		isDead = true;
		GetComponent<CircleCollider2D>().enabled = false;

		Collider2D[] blocksNearby = Physics2D.OverlapCircleAll(transform.position, 5f, blockLayermask);
		rigidbody.isKinematic = true;
		rigidbody.velocity = Vector2.zero;
		feet.GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<SpriteRenderer>().enabled = false;

		foreach (Collider2D block in blocksNearby) {
			block.GetComponent<SpriteRenderer>().sprite = destroyedBlock;
			block.enabled = false;
			grid.GetNodeFromWorldPoint(block.transform.position).walkable = true;
			block.GetComponent<SpriteRenderer>().sortingOrder = -50;
			block.transform.position += new Vector3(0, 0, 0.5f);
		}

		int forwardBloodSplats = Random.Range(90, 100);
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

		int flakCount = 64;
		for (int b = 0; b < flakCount; b++) {
			Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
			float randomVelocityMagnitude = Random.Range(1.75f, 2f);
			GameObject newFlak = (GameObject)Instantiate(angelFlakPrefab, transform.position, randomRotation);
			Projectile newFlakProjectile = newFlak.GetComponent<Projectile>();
			newFlakProjectile.velocity = randomRotation * (Vector2.right * randomVelocityMagnitude * 75);
			newFlakProjectile.ricochets = 100;

		}

		StartCoroutine(AfterDeath());
	}

	IEnumerator AfterDeath () {
		yield return new WaitForSeconds(1);
		pressRToRestart.SetActive(true);
		audioManager.PlayJukeboxAtPoint(clipFail, transform.position, 1f);
	}
}

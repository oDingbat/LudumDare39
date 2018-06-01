using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour {

	int screenNumber = 0;

	public SpriteRenderer screen1;
	public SpriteRenderer screen2;
	public SpriteRenderer screen3;

	void Start () {
		StartCoroutine(StartIntro());
	}
	
	IEnumerator StartIntro () {
		yield return new WaitForSeconds(3);
		GetComponent<AudioSource>().Play();
		screenNumber = 1;
		yield return new WaitForSeconds(17.5f);
		screenNumber = 2;
	}

	void Update () {
		if (screenNumber == 1) {
			GetComponent<AudioSource>().volume = Mathf.Lerp(GetComponent<AudioSource>().volume, 1, 5 * Time.deltaTime);
			screen1.color = Color.Lerp(screen1.color, new Color(1, 1, 1, 0), 5 * Time.deltaTime);
			if (Input.GetKey(KeyCode.Space)) {
				screenNumber = 2;
			}

		} else if (screenNumber == 2) {
			GetComponent<AudioSource>().volume = Mathf.Lerp(GetComponent<AudioSource>().volume, 0, 5 * Time.deltaTime);
			screen1.color = Color.Lerp(screen1.color, new Color(1, 1, 1, 0), 5 * Time.deltaTime);
			screen2.color = Color.Lerp(screen2.color, new Color(1, 1, 1, 0), 5 * Time.deltaTime);

			if (Input.anyKeyDown) {
				SceneManager.LoadScene("Playing");
			}
		}

		if (Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}

	}
}

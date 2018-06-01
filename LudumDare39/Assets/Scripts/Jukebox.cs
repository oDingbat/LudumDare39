using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(AudioSource ))]
public class Jukebox : MonoBehaviour {

	AudioSource audioSource;

	void Awake () {
		audioSource = GetComponent<AudioSource>();
	}

	void Update () {
		audioSource.volume = Mathf.Clamp01(1 / Vector3.Distance(transform.position, (Vector2)Camera.main.transform.position));
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public GameObject[] jukeboxes;
	public GameObject jukeboxPrefab;

	public int jukeboxCurrent;
	public int jukeboxCount;

	void Start () {
		jukeboxes = new GameObject[jukeboxCount];
		for (int i = 0; i < jukeboxCount; i++) {
			GameObject newJukebox = (GameObject)Instantiate(jukeboxPrefab, Vector3.zero, Quaternion.identity);
			newJukebox.transform.parent = transform;
			jukeboxes[i] = newJukebox;
		}
	}
	
	public void PlayJukeboxAtPoint (AudioClip _clip, Vector2 point, float _volume) {
		jukeboxes[jukeboxCurrent].transform.position = point;
		jukeboxes[jukeboxCurrent].GetComponent<AudioSource>().Stop();
		jukeboxes[jukeboxCurrent].GetComponent<AudioSource>().clip = _clip;
		jukeboxes[jukeboxCurrent].GetComponent<AudioSource>().volume = _volume;
		jukeboxes[jukeboxCurrent].GetComponent<AudioSource>().Play();
		jukeboxCurrent = (jukeboxCurrent == jukeboxes.Length - 1 ? 0 : jukeboxCurrent + 1);
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundController : MonoBehaviour {

	public GameObject player;

	public Texture2D tex;

	void Start () {
		tex = Instantiate(GetComponent<Renderer>().material.mainTexture) as Texture2D;
		GetComponent<Renderer>().material.mainTexture = tex as Texture;
	}

	void FixedUpdate () {
		tex.Apply();
	}

	public void SetPixelAtPoint (Vector2 point, Color color) {
		point = new Vector2(Mathf.Round(point.x * 8) + 512, Mathf.Round(point.y * 8) + 512);
		tex.SetPixel((int)point.x, (int)point.y, color);
	}
}

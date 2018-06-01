using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberLabel : MonoBehaviour {

	public Sprite[] numberSprites;
	public SpriteRenderer[] labelNumbers;

	public void UpdateLabel (int newNumber) {
		for (int i = 0; i < labelNumbers.Length; i++) {
			if (i >= newNumber.ToString().Length) {
				labelNumbers[i].sprite = null;
			} else {
				labelNumbers[i].sprite = numberSprites[(int)Mathf.Clamp((int)char.GetNumericValue(newNumber.ToString()[i]), 0, 99)];
			}
		}
	}

}

using UnityEngine;
using System.Collections;

public class Cube : MonoBehaviour {
	
	public int X, Y;
	public float materialFactor = 1.0f;
	
	Color originalColor;
	public Vector3 originalPosition;
	public bool selected = false;
	
	// Use this for initialization
	void Start () {
	}
	
	public void SetOriginalPosition(Vector3 position) {
		originalPosition = position;
	}
	
	// Update is called once per frame
	void Update () {
		if (!selected) {
			transform.localPosition -= (transform.localPosition - originalPosition) * 4.0f * Time.deltaTime;
		}
	}
}

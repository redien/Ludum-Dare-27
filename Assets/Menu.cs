using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour {
	
	public GameObject QuitButton, PlayButton;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject target = hit.collider.transform.gameObject;
				
				if (target.tag == "button") {
					target.transform.parent.animation.Play("Selected");
					target.transform.parent.audio.Play();
				}
			}
		}
		
		if (Input.GetMouseButtonUp(0)) {
			var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				GameObject target = hit.collider.transform.gameObject;
				if (target == QuitButton) {
					Application.Quit();
				}
				
				if (target == PlayButton) {
					Application.LoadLevel(1);
				}
			}
		}
	}
}

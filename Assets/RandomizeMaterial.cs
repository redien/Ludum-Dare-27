using UnityEngine;
using System.Collections;

public class RandomizeMaterial : MonoBehaviour {
	public Material[] materials = null;

	public void Randomize() {
		int index = Random.Range(0, materials.Length);
		renderer.material = materials[index];
	}
}

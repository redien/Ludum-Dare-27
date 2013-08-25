using UnityEngine;
using System.Collections;

public class SoundEffectProxy : MonoBehaviour {
	public void Play(string name) {
		SoundEffect[] effects = GetComponentsInChildren<SoundEffect>();
		
		foreach (SoundEffect effect in effects) {
			effect.Play(name);
		}
	}
}

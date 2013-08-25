using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour {
	
	public AudioClip[] audioClips;
	public AudioSource audioSource;
	public string effectName;
	
	public void Play(string name) {
		if (name == effectName) {
			audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
			audioSource.Play();
		}
	}
}

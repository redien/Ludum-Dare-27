using UnityEngine;
using System.Collections;

public class SoundEffect : MonoBehaviour {
	
	public AudioClip[] audioClips;
	public AudioSource[] audioSources;
	int sourceIndex = 0;
	public string effectName;
	
	public void Play(string name) {
		if (name == effectName) {
			audioSources[sourceIndex].clip = audioClips[Random.Range(0, audioClips.Length)];
			audioSources[sourceIndex].Play();
			sourceIndex = (sourceIndex + 1) % audioSources.Length;
		}
	}
}

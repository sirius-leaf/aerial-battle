using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempAudioSource55 : MonoBehaviour
{
    private AudioSource audioSrc;
    private bool playAudio = false;

    public void Init(AudioClip audio) {
        audioSrc = GetComponent<AudioSource>();

        audioSrc.clip = audio;
        audioSrc.Play();
        playAudio = true;
    }

    void Update() { if (playAudio && !audioSrc.isPlaying) Destroy(gameObject); }
}

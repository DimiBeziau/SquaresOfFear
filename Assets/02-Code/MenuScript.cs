using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour {

    [Header("Audio")]
    public AudioClip sfxClick;
    private AudioSource _audio;

    void Start()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    public void PlayGame(){
        if (sfxClick != null) _audio.PlayOneShot(sfxClick);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}

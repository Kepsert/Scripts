using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicSettings : MonoBehaviour
{
    public static MusicSettings Instance { get; private set; }

    [SerializeField] Slider[] musicSliders;
    [SerializeField] Slider[] sfxSliders;

    //To Kep: Maybe save these values for wehn player restarts game
    float volMusic = 0.25f;
    float volSFX = 0.25f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(this);
        }
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            volMusic = PlayerPrefs.GetFloat("MusicVolume");
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            volSFX = PlayerPrefs.GetFloat("SFXVolume");
        }
        UpdateMusicSliders();
        UpdateSFXSliders();
    }

    private void UpdateMusicSliders()
    {
        for (int i = 0; i < musicSliders.Length; i++)
        {
            if (musicSliders[i] != null)
                musicSliders[i].value = volMusic;
        }
    }

    private void UpdateSFXSliders()
    {
        for (int i = 0; i < sfxSliders.Length; i++)
        {
            if (musicSliders[i] != null)
                sfxSliders[i].value = volSFX;
        }
    }

    public void SetLevelMusic(Slider sl)
    {
        volMusic = sl.value;
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            PlayerPrefs.DeleteKey("MusicVolume");
        }
        PlayerPrefs.SetFloat("MusicVolume", sl.value);
        UpdateMusicSliders();
    }

    public void SetLevelSFX(Slider sl)
    {
        volSFX = sl.value;
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            PlayerPrefs.DeleteKey("SFXVolume");
        }
        PlayerPrefs.SetFloat("SFXVolume", sl.value);
        UpdateSFXSliders();
    }

    public float GetLevelMusic()
    {
        return volMusic;
    }

    public float GetLevelSFX()
    {
        return volSFX;
    }
}

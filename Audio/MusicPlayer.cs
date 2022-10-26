using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [SerializeField] private AudioClip bgMusicDefault;
    [SerializeField] AudioClip[] bgMusics;

    [SerializeField] private AudioClip[] soundFXs;

    private float prevVolumeFX;
    private float prevVolumeMusic;

    private AudioSource[] audioSource_FX;
    private AudioSource[] audioSource_Music;

    private bool isInitiated;

    private string s_prevMusicName;
    private string s_currentMusicName;

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

    // Start is called before the first frame update
    void Start()
    {
        s_prevMusicName = "";

        if (!isInitiated)
        {
            //create audio sources (sfx)
            for (int i = 0; i < soundFXs.Length; i++)
                this.gameObject.AddComponent<AudioSource>();
            audioSource_FX = this.GetComponents<AudioSource>();

            for (int i = 0; i < soundFXs.Length; i++)
                audioSource_FX[i].clip = soundFXs[i];


            //create audio sources (music)
            for (int i = 0; i < bgMusics.Length; i++)
                this.gameObject.AddComponent<AudioSource>();

            List<AudioSource> audioSource_Music_list = new List<AudioSource>();
            audioSource_Music_list.AddRange(this.GetComponents<AudioSource>());
            audioSource_Music_list = audioSource_Music_list.GetRange(soundFXs.Length, audioSource_Music_list.Count - soundFXs.Length);
            audioSource_Music = audioSource_Music_list.ToArray();

            for (int i = 0; i < bgMusics.Length; i++)
                audioSource_Music[i].clip = bgMusics[i];

            //preset bg music setting
            for (int i = 0; i < audioSource_Music.Length; i++)
            {
                audioSource_Music[i].volume = 0;
                audioSource_Music[i].spatialBlend = 0;
                audioSource_Music[i].spread = 360;
                audioSource_Music[i].loop = true;
            }

            isInitiated = true;
        }

        int i_musicID = 0;
        if (bgMusicDefault != null)
            i_musicID = System.Array.FindIndex(bgMusics, x => x.name.ToLower() == bgMusicDefault.name.ToLower());
        if (i_musicID < 0) i_musicID = 0;

        //play music if it isn't playing already
        if (!audioSource_Music[i_musicID].isPlaying)
        {
            if (bgMusicDefault != null)
                audioSource_Music[i_musicID].Play();
        }
        if (bgMusicDefault != null)
            s_currentMusicName = bgMusicDefault.name;
        else
            s_currentMusicName = "";
    }

    // Update is called once per frame
    void Update()
    {
        //handle fx volume change
        if (prevVolumeFX != MusicSettings.Instance.GetLevelSFX())
        {
            //if the volume setting has changed adjust the 
            for (int i = 1; i < audioSource_FX.Length; i++)
                audioSource_FX[i].volume = MusicSettings.Instance.GetLevelSFX();
        }

        //cross fading bg music
        if (s_currentMusicName != null && s_currentMusicName != "")
        {
            float musicVol = MusicSettings.Instance.GetLevelMusic();
            int i_musicID = System.Array.FindIndex(bgMusics, x => x.name.ToLower() == s_currentMusicName.ToLower());
            if (audioSource_Music[i_musicID].volume < musicVol)
            {
                audioSource_Music[i_musicID].volume += 0.01f;
                if (audioSource_Music[i_musicID].volume > musicVol)
                    audioSource_Music[i_musicID].volume = musicVol;
            }
            else if (audioSource_Music[i_musicID].volume > musicVol)
            {
                audioSource_Music[i_musicID].volume -= 0.01f;
                if (audioSource_Music[i_musicID].volume < musicVol)
                    audioSource_Music[i_musicID].volume = musicVol;
            }
        }
        //fade out previous music
        if (s_prevMusicName != "")
        {
            int i_musicID = System.Array.FindIndex(bgMusics, x => x.name.ToLower() == s_prevMusicName.ToLower());
            if (audioSource_Music[i_musicID].volume > 0)
            {
                audioSource_Music[i_musicID].volume -= 0.01f;
                if (audioSource_Music[i_musicID].volume < 0)
                {
                    audioSource_Music[i_musicID].volume = 0;
                    audioSource_Music[i_musicID].Stop();
                }
            }
        }

        prevVolumeFX = MusicSettings.Instance.GetLevelSFX();
        prevVolumeMusic = MusicSettings.Instance.GetLevelMusic();
    }

    public void ChangeBgMusic(string s_name, bool isInstant)
    {
        Debug.Log("change music: " + s_name);
        int i_musicID = 0;

        if (s_name != "")
        {
            s_name = "Music_" + s_name;
            if (s_name == s_currentMusicName)
                return;

            i_musicID = System.Array.FindIndex(bgMusics, x => x.name.ToLower() == s_name.ToLower());
            //return if music does not exist
            if (i_musicID < 0)
                return;
            audioSource_Music[i_musicID].Play();

            //if the prior music was still fading out just stop it now unless we are transitioning back to it
            if (s_prevMusicName != "" && s_prevMusicName != s_name)
            {
                i_musicID = System.Array.FindIndex(bgMusics, x => x.name.ToLower() == s_prevMusicName.ToLower());
                audioSource_Music[i_musicID].volume = 0;
                audioSource_Music[i_musicID].Stop();
            }
        }

        s_prevMusicName = s_currentMusicName;
        s_currentMusicName = s_name;

        if (isInstant)
        {
            i_musicID = System.Array.FindIndex(bgMusics, x => x.name.ToLower() == s_prevMusicName.ToLower());
            Debug.Log("Music ID: " + i_musicID + " - Audiosource: " + audioSource_Music);
            if (i_musicID!=-1)
                audioSource_Music[i_musicID].volume = 0;
        }
    }
    public void PlaySoundEffectByName(string name)
    {
        string s_nameCheck = "SFX_" + name;//"SFX_echoDrip";
        int i_soundEffectID = System.Array.FindIndex(soundFXs, x => x.name.ToLower() == s_nameCheck.ToLower());
        if (i_soundEffectID < 0) i_soundEffectID = 0;
        audioSource_FX[i_soundEffectID].PlayOneShot(soundFXs[i_soundEffectID]);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource bg_adudio;
    [SerializeField] internal AudioSource audioPlayer_wl;
    [SerializeField] internal AudioSource audioPlayer_button;
    [SerializeField] internal AudioSource audioPlayer_Spin;
    
    [SerializeField] private AudioClip SpinButtonClip;
    [SerializeField] private AudioClip SpinClip;
    [SerializeField] private AudioClip ChestOpenClip;
    [SerializeField] private AudioClip Button;
    [SerializeField] private AudioClip Win_Audio;
    [SerializeField] private AudioClip BonusWin_Audio;
    [SerializeField] private AudioClip BonusLose_Audio;
    [SerializeField] private AudioClip NormalBg_Audio;
    [SerializeField] private AudioClip BonusBg_Audio;

// TODO: slot add button click on next prev
    private void Start()
    {
        playBgAudio();
    }

    internal void PlayWLAudio(string type)
    {


        switch (type)
        {

            case "win":
                //index = UnityEngine.Random.Range(1, 2);
                audioPlayer_wl.clip = Win_Audio;
                break;
            case "bonuswin":
                audioPlayer_wl.clip = BonusWin_Audio;
                break;
            case "bonuslose":
                audioPlayer_wl.clip = BonusLose_Audio;
                break;
                //index = 3;

        }
        StopWLAaudio();
        //audioPlayer_wl.clip = clips[index];
        //audioPlayer_wl.loop = true;
        audioPlayer_wl.Play();

    }



    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            bg_adudio.Pause();
            audioPlayer_wl.Pause();
            audioPlayer_button.Pause();
            audioPlayer_Spin.Pause();
        }
        else
        {
            if (!bg_adudio.mute) bg_adudio.UnPause();
            if (!audioPlayer_wl.mute) audioPlayer_wl.UnPause();
            if (!audioPlayer_button.mute) audioPlayer_button.UnPause();
            if (!audioPlayer_Spin.mute) audioPlayer_Spin.UnPause();

        }
    }

    internal void PlaySpinBonusAudio(string type = "spin")
    {

        if (audioPlayer_Spin)
        {
            if (type == "spin")
            {
                audioPlayer_Spin.clip = SpinClip;

            }
            else if (type == "bonus")
            {

                audioPlayer_Spin.clip = ChestOpenClip;

            }


            audioPlayer_Spin.Play();
        }

    }

    internal void StopSpinBonusAudio()
    {

        if (audioPlayer_Spin) audioPlayer_Spin.Stop();

    }
    internal void playBgAudio(string type = "normal")
    {
        //int randomIndex = UnityEngine.Random.Range(0, Bg_Audio.Length);
        if (bg_adudio)
        {
            if (type == "normal")
                bg_adudio.clip = NormalBg_Audio;
            else if (type == "bonus")
                bg_adudio.clip = BonusBg_Audio;

            bg_adudio.Play();
        }

    }

    internal void PlayButtonAudio(string type = "default")
    {

        if (type == "spin")
            audioPlayer_button.clip = SpinButtonClip;
        else
            audioPlayer_button.clip = Button;

        //StopButtonAudio();
        audioPlayer_button.Play();
        //Invoke("StopButtonAudio", audioPlayer_button.clip.length);

    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
    }

    internal void StopButtonAudio()
    {

        audioPlayer_button.Stop();

    }


    internal void StopBgAudio()
    {
        bg_adudio.Stop();

    }


    internal void ToggleMute(bool toggle, string type = "all")
    {

        switch (type)
        {
            case "bg":
                bg_adudio.mute = toggle;
                break;
            case "button":
                audioPlayer_button.mute = toggle;
                audioPlayer_Spin.mute = toggle;
                break;
            case "wl":
                audioPlayer_wl.mute = toggle;
                break;
            case "all":
                audioPlayer_wl.mute = toggle;
                bg_adudio.mute = toggle;
                audioPlayer_button.mute = toggle;
                break;
        }
    }

}

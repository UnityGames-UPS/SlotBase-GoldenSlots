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

    private bool isForceMuted = false;
    private readonly Dictionary<AudioSource, bool> preFocusMuteState = new Dictionary<AudioSource, bool>();

    private IEnumerable<AudioSource> AllSources()
    {
        yield return bg_adudio;
        yield return audioPlayer_wl;
        yield return audioPlayer_button;
        yield return audioPlayer_Spin;
    }

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



    // Native/editor focus path — calls the SAME method the WebGL OnFocusChanged path calls (UIManager.OnFocusChanged).
    private void OnApplicationFocus(bool focus)
    {
        SetMuteAll(!focus);
    }

    // Focus-driven — called from BOTH UIManager.OnFocusChanged (JS bridge) and OnApplicationFocus above.
    // Reentrancy-guarded: a duplicate call for the same direction (both focus sources firing for one
    // blur/focus event) is a no-op, so the second call can't clobber the first call's captured restore state.
    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted) return;
        isForceMuted = forceMute;

        foreach (var source in AllSources())
        {
            if (source == null) continue;
            if (forceMute)
            {
                preFocusMuteState[source] = source.mute;
                source.mute = true;
            }
            else
            {
                source.mute = preFocusMuteState.TryGetValue(source, out bool prevMuted) ? prevMuted : source.mute;
            }
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


    // User-toggle-driven — Sound/Music button callbacks (UIManager.ToggleSound/ToggleMusic).
    // Always writes .mute directly (an explicit user interaction always wins over a stuck/stale
    // forced-mute flag — see Check 3's "reverse invariant"). If a focus-mute is currently active,
    // also updates the captured "restore to" value so a later legitimate focus-regain doesn't
    // clobber the user's newer choice back to the stale pre-blur state.
    internal void ToggleMute(bool toggle, string type = "all")
    {
        switch (type)
        {
            case "bg":
                SetSourceMute(bg_adudio, toggle);
                break;
            case "button":
                SetSourceMute(audioPlayer_button, toggle);
                SetSourceMute(audioPlayer_Spin, toggle);
                break;
            case "wl":
                SetSourceMute(audioPlayer_wl, toggle);
                break;
            case "all":
                SetSourceMute(audioPlayer_wl, toggle);
                SetSourceMute(bg_adudio, toggle);
                SetSourceMute(audioPlayer_button, toggle);
                break;
        }
    }

    private void SetSourceMute(AudioSource source, bool toggle)
    {
        if (source == null) return;
        source.mute = toggle;
        if (isForceMuted) preFocusMuteState[source] = toggle;
    }

}

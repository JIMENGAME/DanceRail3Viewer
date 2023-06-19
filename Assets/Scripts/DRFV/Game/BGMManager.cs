using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [SerializeField] private AudioSource mainSource, noteFXSource;
    private float _pitch;

    public float Pitch
    {
        get => _pitch;
        set => mainSource.pitch = noteFXSource.pitch = _pitch = value;
    }
    
    public AudioClip MainClip
    {
        get => mainSource.clip;
        set => mainSource.clip = value;
    }
    
    public AudioClip NoteFxClip
    {
        get => noteFXSource.clip;
        set => noteFXSource.clip = value;
    }

    public float Time
    {
        get => mainSource.time;
        set => mainSource.time = noteFXSource.time = value;
    }

    public float Volume
    {
        get => mainSource.volume;
        set => mainSource.volume = noteFXSource.volume = value;
    }

    public float panStereo
    {
        get => mainSource.panStereo;
        set => mainSource.panStereo = noteFXSource.panStereo = value;
    }

    public void Play()
    {
        mainSource.Play();
        noteFXSource.Play();
    }
    
    public void PlayScheduled(double t)
    {
        mainSource.PlayScheduled(t);
        noteFXSource.PlayScheduled(t);
        
    }

    public void Pause()
    {
        mainSource.Pause();
        noteFXSource.Pause();
    }

    public void UnPause()
    {
        mainSource.UnPause();
        noteFXSource.UnPause();
    }

    public void DoFade(float endValue, float duration, Ease ease)
    {
        mainSource.DOFade(endValue, duration).SetEase(ease);
        mainSource.DOFade(endValue, duration).SetEase(ease);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MixerConnecter : MonoBehaviour
{
    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.outputAudioMixerGroup = AudioMixerManager.Instance.SFXGroup;
    }
}

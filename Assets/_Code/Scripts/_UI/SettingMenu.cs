using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SettingMenu : Singleton<SettingMenu>
{
    [SerializeField] private GameSettings _gameSettings;
    [SerializeField] private GameObject _gameObject;
    
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private TMP_Dropdown _resolutionDropdown;
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private Slider _volumeSlider;

    private Resolution[] _resolutions;

    public GameSettings GameSettings => _gameSettings;
    
    public void ToggleMenu(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _gameObject.SetActive(!_gameObject.activeSelf);
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = (Cursor.lockState == CursorLockMode.Locked) ? CursorLockMode.Confined : CursorLockMode.Locked;
        }
    }

    private void Start()
    {
        _resolutions = Screen.resolutions;

        _resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height + " - " + _resolutions[i].refreshRateRatio;
            options.Add(option);

            if (_resolutions[i].width == Screen.currentResolution.width && _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        _resolutionDropdown.AddOptions(options);
        _resolutionDropdown.value = currentResolutionIndex;
        _resolutionDropdown.RefreshShownValue();
        
        SetResolution(currentResolutionIndex);
        
        SetVolume(_gameSettings.Volume);
        _volumeSlider.value = _gameSettings.Volume;
        SetQuality(_gameSettings.QualitySettings);
        _qualityDropdown.value = _gameSettings.QualitySettings;
        _qualityDropdown.RefreshShownValue();
        _sensitivitySlider.value = _gameSettings.Sensitivity;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = _resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        _gameSettings.Resolution = resolutionIndex;
        _gameSettings.ResolutionBeenChanged = true;
    }

    public void SetVolume(float volume)
    {
        _audioMixer.SetFloat("volume", volume);
        _gameSettings.Volume = volume;
    }
    
    public void SetSensitiviy(float value)
    {
        _gameSettings.Sensitivity = value;
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        _gameSettings.QualitySettings = qualityIndex;
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }
}
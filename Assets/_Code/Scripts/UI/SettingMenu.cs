using System.Collections.Generic;
using TMPro;
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
       _gameObject.SetActive(!_gameObject.activeSelf);
       //Cursor.visible = !Cursor.visible;
       if (Cursor.lockState == CursorLockMode.Locked)
       {
           Cursor.lockState = CursorLockMode.None;
       }
       else
       {
           Cursor.lockState = CursorLockMode.Locked;
       }
    }
    
    private void Start()
    {
        _resolutions = Screen.resolutions;

        _resolutionDropdown.ClearOptions();

        //Cursor.visible = false;

        List<string> options = new List<string>();

        int currentResolutionIndex = 0;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            string option = _resolutions[i].width + " x " + _resolutions[i].height;
            options.Add(option);

            if (_resolutions[i].width == Screen.currentResolution.width && _resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        _resolutionDropdown.AddOptions(options);
        
        if (!_gameSettings.ResolutionBeenChanged)
        {
            _resolutionDropdown.value = currentResolutionIndex;
            _resolutionDropdown.RefreshShownValue();
        }
        else
        {
            SetResolution(_gameSettings.Resolution);
            _resolutionDropdown.value = GameSettings.Resolution;
            _resolutionDropdown.RefreshShownValue();
        }
        
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

[CreateAssetMenu(fileName = "Settings", menuName = "GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public float Sensitivity;
    public float Volume;
    public int Resolution;
    public int QualitySettings;

    public bool ResolutionBeenChanged;
}
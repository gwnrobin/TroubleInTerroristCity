using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class SettingMenu : Singleton<SettingMenu>
{
    [SerializeField] private Volume _volume;
    
    [SerializeField] private GameSettings _gameSettings;
    
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    [SerializeField] private Slider _sensitivitySlider;
    [SerializeField] private Slider _volumeSlider;
    

    public GameSettings GameSettings => _gameSettings;

    private void Start()
    {
        _volumeSlider.value = _gameSettings.Volume;
        SetQuality(_gameSettings.QualitySettings);
        _qualityDropdown.value = _gameSettings.QualitySettings;
        _qualityDropdown.RefreshShownValue();
        _sensitivitySlider.value = _gameSettings.Sensitivity;
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
    
    public void SetShadowDistance(float value)
    {
        if (_volume.profile.TryGet(out HDShadowSettings shadows))
        {
            shadows.maxShadowDistance.value = value;
        }
    }
}
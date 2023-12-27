using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

public class SettingMenu : Singleton<SettingMenu>
{
    [SerializeField] private GameSettings _gameSettings;
    
    [SerializeField] private TMP_Dropdown _qualityDropdown;
    

    private void Start()
    {
        SetQuality(_gameSettings.QualitySettings);
        _qualityDropdown.value = _gameSettings.QualitySettings;
        _qualityDropdown.RefreshShownValue();
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        _gameSettings.QualitySettings = qualityIndex;
    }
}
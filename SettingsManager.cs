using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    //AUDIO
    [Header("AUDIO & MOUSE SENSITIVITY SETTINGS")]
    public AudioMixer audioMixer;

    [SerializeField] private Slider masterVolumeSlider;

    //SENSITIVITY
    [SerializeField] private Slider mouseSensitivitySlider;

    private void Start()
    {
        //AUDIO VOLUME
        InitializeSettings();

        float savedVolume = PlayerPrefs.GetFloat("masterVolume", 0.75f) * 100.0f;
        SetVolume(savedVolume);

        //MOUSE SENSITIVITY
        float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50.0f);
        SetMouseSensitivity(savedSensitivity);
    }

    public void InitializeSettings()
    {
        UpdateSliders();
    }

    public void UpdateSliders()
    {
        if (masterVolumeSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("masterVolume", 0.75f) * 100.0f;
            masterVolumeSlider.value = savedVolume;
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (mouseSensitivitySlider != null)
        {
            float savedSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 50.0f);
            mouseSensitivitySlider.value = savedSensitivity;
            mouseSensitivitySlider.onValueChanged.RemoveAllListeners();
            mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
        }
    }

    public void SetVolume(float value)
    {
        Debug.Log($"Setting volume: {value}");
        string parameter = "MusicVolume";
        float normalizedValue = value / 100.0f;
        float dbValue = normalizedValue > 0.0001f ? 20f * Mathf.Log10(normalizedValue) : -80f;

        audioMixer.SetFloat(parameter, dbValue);
        PlayerPrefs.SetFloat("masterVolume", normalizedValue);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float value)
    {
        float mappedValue = Mathf.Lerp(50.0f, 500.0f, value / 100.0f);

        Debug.Log("Mouse Sensitivity " + mappedValue);

        PlayerPrefs.SetFloat("MouseSensitivity", value);
        PlayerPrefs.Save();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            FPMovement playerMovement = player.GetComponent<FPMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetMouseSensitivity(mappedValue);
            }
        }
    }
}

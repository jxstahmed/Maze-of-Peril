using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManagerr : MonoBehaviour
{

    [Header("Menus")]
    [SerializeField] public GameObject Menus;
    [SerializeField] public GameObject Menu;
    [SerializeField] public GameObject OptionsView;
    [SerializeField] public GameObject CreditsView;

    [Header("Sliders")]
    [SerializeField] public Slider MusicSlider;
    [SerializeField] public Slider FXSlider;

    [Header("States")]
    [SerializeField] public GameSettings GameOptions;

    public void Start()
    {
        MusicSlider.value = GameOptions.MusicAudioLevel;
        MusicSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged("music"); });


        FXSlider.value = GameOptions.FXAudioLevel;
        FXSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged("fx"); });
    }

    private void OnSliderValueChanged(string type)
    {
        if(type == "music")
        {
            GameOptions.MusicAudioLevel = MusicSlider.value;
        } else if (type == "fx")
        {
            GameOptions.FXAudioLevel = FXSlider.value;
        }
    }

    public void StartGame()
    {
        SceneManager.LoadScene(GameOptions.SCENE_LEVEL_1);
        AudioManager.Instance.ToggleGamePlayAudio(true);
    }
    public void StartLevel(int level)
    {
        if (level == 1)
        {
            AudioManager.Instance.ToggleGamePlayAudio(true);
            SceneManager.LoadScene(GameOptions.SCENE_LEVEL_1);
        }
        else if (level == 2)
        {
            AudioManager.Instance.ToggleGamePlayAudio(true);
            SceneManager.LoadScene(GameOptions.SCENE_LEVEL_2);
        }
    }
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(GameOptions.SCENE_MAIN);
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        if (Menus)
            Menus.SetActive(true);
        if (Menu)
            Menu.SetActive(true);
        if (OptionsView)
            OptionsView.SetActive(false);
        if (CreditsView)
            CreditsView.SetActive(false);
    }
    public void PauseGame()
    {
        AudioManager.Instance.ToggleGamePlayAudio(false);
        Time.timeScale = 0f;
        if (Menus)
            Menus.SetActive(true);
        if (Menu)
            Menu.SetActive(true);
        if (OptionsView)
            OptionsView.SetActive(false);
        if (CreditsView)
            CreditsView.SetActive(false);
    }

    public void OpenOptions()
    {
        if (Menu)
            Menu.SetActive(false);
        if (OptionsView)
            OptionsView.SetActive(true);
        if (CreditsView)
            CreditsView.SetActive(false);
    }

    public void OpenCredits()
    {
        if (Menu)
            Menu.SetActive(false);
        if (OptionsView)
            OptionsView.SetActive(false);
        if (CreditsView)
            CreditsView.SetActive(true);
    }

    public void ReturnPause(string from)
    {
        if (from == "credits" || from == "options")
        {
            if (Menu)
                Menu.SetActive(true);
            if (OptionsView)
                OptionsView.SetActive(false);
            if (CreditsView)
                CreditsView.SetActive(false);
        }
    }

    public void ResumeGame()
    {
        AudioManager.Instance.ToggleGamePlayAudio(true);
        Time.timeScale = 1f;
        if (Menus)
            Menus.SetActive(false);
    }

    public void QuitGame()
    {
     
        Application.Quit();
    }

}

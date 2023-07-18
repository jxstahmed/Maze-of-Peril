using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public static MenuManager Instance;

    [Header("Menus")]
    [SerializeField] public GameObject Menus;
    [SerializeField] public GameObject Menu;
    [SerializeField] public GameObject OptionsView;
    [SerializeField] public GameObject CreditsView;
    [SerializeField] public GameObject DeathView;
    [SerializeField] public GameObject LevelEndView;
    [SerializeField] public GameObject ControlsView;

    [Header("Sliders")]
    [SerializeField] public Slider MusicSlider;
    [SerializeField] public Slider FXSlider;

    [Header("States")]
    [SerializeField] public GameSettings GameOptions;
    [SerializeField] bool StartMenuMusic = false;

    void Awake()
    {
        Instance = this;
    }

    public void Start()
    {

        MusicSlider.value = GameOptions.MusicAudioLevel;
        MusicSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged("music"); });


        FXSlider.value = GameOptions.FXAudioLevel;
        FXSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged("fx"); });

        StartCoroutine(Validate());
    }

    private IEnumerator Validate()
    {
        yield return new WaitForSeconds(1.5f);
        if (StartMenuMusic)
        {
            AudioManager.Instance.ToggleMenuAudio(true);

        }
    }

    private void OnSliderValueChanged(string type)
    {
        if (type == "music")
        {
            GameOptions.MusicAudioLevel = MusicSlider.value;
            AudioManager.Instance.ToggleMenuAudio(true);
        }
        else if (type == "fx")
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
        Time.timeScale = 1f;
        if (level == 1)
        {
            // should reset weapons?
            AudioManager.Instance.ToggleGamePlayAudio(true);
            SceneManager.LoadScene(GameOptions.SCENE_LEVEL_1);
        }
        else if (level == 2)
        {
            AudioManager.Instance.ToggleGamePlayAudio(true);
            SceneManager.LoadScene(GameOptions.SCENE_LEVEL_2);
        }
        else
        {
            // nothing found, go to main menu
            OpenMainMenu();
        }
    }
    public void StartNextLevel()
    {
        Scene scene = SceneManager.GetActiveScene();
        Time.timeScale = 1f;

        int currentLevel = -1;
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == GameOptions.SCENE_LEVEL_1)
        {
            currentLevel = 1;
        }
        else if (currentScene == GameOptions.SCENE_LEVEL_2)
        {
            currentLevel = 2;
        }

        StartLevel(currentLevel + 1);
    }

    public void RestartScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(GameOptions.SCENE_MAIN);
        Time.timeScale = 1f;
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        AudioManager.Instance.ToggleMenuAudio(true);

        if (Menus)
            Menus.SetActive(true);

        if (Menu)
            Menu.SetActive(true);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(false);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(false);

    }
    public void PauseGame()
    {
        Time.timeScale = 0f;

        AudioManager.Instance.ToggleMenuAudio(true);

        if (Menus)
            Menus.SetActive(true);

        if (Menu)
            Menu.SetActive(true);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(false);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(false);
    }

    public void OpenOptions()
    {
        if (Menu)
            Menu.SetActive(false);

        if (OptionsView)
            OptionsView.SetActive(true);

        if (CreditsView)
            CreditsView.SetActive(false);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(false);
    }

    public void OpenCredits()
    {
        if (Menu)
            Menu.SetActive(false);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(true);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(false);
    }

    public void OpenDeathView()
    {
        Time.timeScale = 0f;
        AudioManager.Instance.ToggleMenuAudio(true);

        if (Menus)
            Menus.SetActive(true);

        if (Menu)
            Menu.SetActive(false);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(false);

        if (DeathView)
            DeathView.SetActive(true);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(false);
    }

    public void OpenLevelEndView()
    {
        Time.timeScale = 0f;
        AudioManager.Instance.ToggleMenuAudio(true);

        if (Menus)
            Menus.SetActive(true);

        if (Menu)
            Menu.SetActive(false);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(false);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(true);

        if (ControlsView)
            ControlsView.SetActive(false);
    }

    public void OpenControlsView()
    {
        Time.timeScale = 0f;
        AudioManager.Instance.ToggleMenuAudio(true);

        if (Menus)
            Menus.SetActive(true);

        if (Menu)
            Menu.SetActive(false);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(false);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(true);
    }

    public void ReturnPause(string from)
    {
        if (from == "credits" || from == "options" || from == "controls")
        {
            if (Menu)
                Menu.SetActive(true);

            if (OptionsView)
                OptionsView.SetActive(false);

            if (CreditsView)
                CreditsView.SetActive(false);

            if (DeathView)
                DeathView.SetActive(false);

            if (LevelEndView)
                LevelEndView.SetActive(false);

            if (ControlsView)
                ControlsView.SetActive(false);
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

    public int GetCurrentLevel()
    {
        int currentLevel = 0;
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene == GameOptions.SCENE_LEVEL_1)
        {
            currentLevel = 1;
        }
        else if (currentScene == GameOptions.SCENE_LEVEL_2)
        {
            currentLevel = 2;
        }

        return currentLevel;
    }

}

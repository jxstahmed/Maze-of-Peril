using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{

    public static MenuManager Instance;
    private AudioSource audioSource;

    [Header("Menus")]
    [SerializeField] public GameObject Menus;
    [SerializeField] public GameObject Menu;
    [SerializeField] public GameObject OptionsView;
    [SerializeField] public GameObject CreditsView;
    [SerializeField] public GameObject DeathView;
    [SerializeField] public GameObject LevelEndView;
    [SerializeField] public GameObject ControlsView;
    [SerializeField] public GameObject StartControlsView;

    [Header("Sliders")]
    [SerializeField] public Slider MusicSlider;
    [SerializeField] public Slider FXSlider;

    [Header("States")]
    [SerializeField] public GameSettings GameOptions;
    [SerializeField] bool StartMenuMusic = false;

    [SerializeField] Camera cam;

    private string nestedMenuOriginal = "";
    void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        MusicSlider.value = GameOptions.MusicAudioLevel * 10;
        MusicSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged("music"); });


        FXSlider.value = GameOptions.FXAudioLevel * 2;
        FXSlider.onValueChanged.AddListener(delegate { OnSliderValueChanged("fx"); });

        StartCoroutine(Validate());

        if(GetCurrentLevel() == 1 && !GameOptions.LEVEL_1_TUTORIAL_SHOWN)
        {
            GameOptions.LEVEL_1_TUTORIAL_SHOWN = true;
            OpenStartControlsTutorial();
        }
            
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
            GameOptions.MusicAudioLevel = MusicSlider.value/10;
            AudioManager.Instance.ToggleMenuAudio(true);
        }
        else if (type == "fx")
        {
            GameOptions.FXAudioLevel = FXSlider.value/2;
        }
    }

    public void StartGame()
    {
        StartLevel(1);
    }
    public void StartLevel(int level)
    {
        try
        {
            GameManager.Instance.ResetScriptableValues();
        } catch { }

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
        else if (level == 3)
        {
            AudioManager.Instance.ToggleGamePlayAudio(true);
            SceneManager.LoadScene(GameOptions.SCENE_LEVEL_3);
        }
        else
        {
            // nothing found, go to main menu
            OpenMainMenu();
        }
    }


    public void StartNextLevel()
    {
        AudioManager.Instance.Timers = new List<AudioManager.CustomTimer>();
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
        
        else if (currentScene == GameOptions.SCENE_LEVEL_3)
        {
            currentLevel = 3;
        }

        StartLevel(currentLevel + 1);
    }

    public void RestartScene()
    {
        AudioManager.Instance.Timers = new List<AudioManager.CustomTimer>();
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        AudioManager.Instance.ToggleGamePlayAudio(true);
 
        Time.timeScale = 1f;
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

        nestedMenuOriginal = "main";

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

        nestedMenuOriginal = "pause";

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
        {
            MusicSlider.value = GameOptions.MusicAudioLevel * 10;
            FXSlider.value = GameOptions.FXAudioLevel * 2;
            OptionsView.SetActive(true);
        }


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

        nestedMenuOriginal = "death";

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
        audioSource.clip = AudioManager.Instance.levelComplete.clip;
        audioSource.volume = AudioManager.Instance.Settings.FXAudioLevel;
        audioSource.Play();

        nestedMenuOriginal = "end";

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
        //Time.timeScale = 0f;
        //AudioManager.Instance.ToggleMenuAudio(true);

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

            if(DeathView)
            DeathView.SetActive(false);
            if(LevelEndView)
            LevelEndView.SetActive(false);
            if(Menu)
            Menu.SetActive(false);

            if (nestedMenuOriginal == "death")
            {
                if (DeathView)
                    DeathView.SetActive(true);


            }
            else if (nestedMenuOriginal == "end")
            {
                if (LevelEndView)
                    LevelEndView.SetActive(true);
            } else
            {
                if (Menu)
                    Menu.SetActive(true);
            }


            if (OptionsView)
                OptionsView.SetActive(false);

            if (CreditsView)
                CreditsView.SetActive(false);


            if (ControlsView)
                ControlsView.SetActive(false);
        }
    }

    public void OpenStartControlsTutorial()
    {
        Time.timeScale = 0f;

        AudioManager.Instance.ToggleMenuAudio(true);

        Debug.Log("Starting ss");


        if (Menus)
            Menus.SetActive(true);

        if (Menu)
            Menu.SetActive(false);

        if (OptionsView)
            OptionsView.SetActive(false);

        if (CreditsView)
            CreditsView.SetActive(false);
        

        if (StartControlsView)
            StartControlsView.SetActive(true);

        if (DeathView)
            DeathView.SetActive(false);

        if (LevelEndView)
            LevelEndView.SetActive(false);

        if (ControlsView)
            ControlsView.SetActive(false);
    }

    public void HideStartControlsTutorial()
    {
        if (StartControlsView)
            StartControlsView.SetActive(false);

        ResumeGame();
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
        else if (currentScene == GameOptions.SCENE_LEVEL_3)
        {
            currentLevel = 3;
        }

        return currentLevel;
    }
    public void playSound()
    {
        audioSource.clip = AudioManager.Instance.MenuClick.clip;
        audioSource.volume = AudioManager.Instance.Settings.FXAudioLevel;
        audioSource.Play();
    }
}

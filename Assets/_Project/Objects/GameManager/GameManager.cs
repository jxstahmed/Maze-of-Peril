using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using NaughtyAttributes;
using static Cinemachine.DocumentationSortingAttribute;
using System.Drawing;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public static event Action<Hashtable> GameEvent;

    
    [Header("Overall")]
    [SerializeField] public bool ResetScriptableAfterRun = true;


    [Header("Prefabs")]
    [SerializeField] public GameObject TextLabelPrefab;


    [Header("Tags")]
    [SerializeField] public string PlayerTag = "Player";
    [SerializeField] public string EnemyTag = "Enemy";
    [SerializeField] public string StaticTag = "Static";

    [Header("Attachments")]
    [SerializeField] public GameSettings Settings;
    [SerializeField] public WeaponsPack WeaponsPackData;
    [SerializeField] public PlayerStats PlayerData;
    [SerializeField] public List<EnemiesStats> Enemies = new List<EnemiesStats>();
    [SerializeField] public List<KeysStats> Keys = new List<KeysStats>();
    [SerializeField] public GameObject HealthPotion;
    [SerializeField] public GameObject HealthSizePotion;
    [SerializeField] public GameObject StaminaSizePotion;

    [Header("Slow Motion")]
    [SerializeField] public bool CanSlowMoAfterHit = false;
    [SerializeField] public float slowMotionTimeScale = 0.05f;
    [SerializeField] public float SlowMotionDuration = 0.2f;

    [Header("Camera Shake")]
    [SerializeField] public bool CanShakeCameraAfterHit = false;
    [SerializeField] public float ShakeDuration = 1f;
    [SerializeField] public float ShakeIntensity = 1f;

    private float startTimeScale;
    private float startFixedDeltaTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    private void Start()
    {
        startTimeScale = Time.timeScale;
        startFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void ResetScriptableValues()
    {
        // Weapons
        PlayerData.EquippedWeaponID = "";
        PlayerData.WeaponsIDsList.Clear();

        // Level options
        if (MenuManager.Instance != null && Settings != null && Settings.LevelsOptions != null && Settings.LevelsOptions.Count > 0)
        {
            int currentLevel = MenuManager.Instance.GetCurrentLevel();
            if (currentLevel > 0)
            {
                for (int i = 0; i < Settings.LevelsOptions.Count; i++)
                {
                    Settings.LevelsOptions[i].player_health = Settings.LevelsOptions[i].player_health_original;
                    Settings.LevelsOptions[i].player_stamina = Settings.LevelsOptions[i].player_stamina_original;
                }
            }
        }
    }


    public void ChangePlayerStamina(float staminaRate)
    {
        Debug.Log("Reducing stamina via GameManager");
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.AffectStamina);
        payload.Add("stamina", staminaRate);
        GameEvent?.Invoke(payload);
    }

    public void AttackPlayer(float damage)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.AttackPlayer);
        payload.Add("damage", damage);
        Debug.Log("Applying damage to player");
        GameEvent?.Invoke(payload);
    } 
    
    public void AffectPlayerHealth(float health)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.AffectHealth);
        payload.Add("health", health);
        GameEvent?.Invoke(payload);
    } 
    
    public void AffectPlayerStamina(float stamina)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.AffectStamina);
        payload.Add("stamina", stamina);
        GameEvent?.Invoke(payload);
    } 

    
    public void AffectPlayerStaminaSize(float size)
    {
        if (MenuManager.Instance != null && Settings != null && Settings.LevelsOptions != null && Settings.LevelsOptions.Count > 0)
        {
            int currentLevel = MenuManager.Instance.GetCurrentLevel();
            if (currentLevel > 0)
            {
                for(int i =0; i < Settings.LevelsOptions.Count; i++)
                {
                    if (Settings.LevelsOptions[i].level_number == currentLevel)
                    {
                        Settings.LevelsOptions[i].player_stamina += size;
                        break;
                    }
                }
            }
        }
    } 

    
    public void AffectPlayerHealthSize(float size)
    {
        if (MenuManager.Instance != null && Settings != null && Settings.LevelsOptions != null && Settings.LevelsOptions.Count > 0)
        {
            int currentLevel = MenuManager.Instance.GetCurrentLevel();
            if (currentLevel > 0)
            {
                for (int i = 0; i < Settings.LevelsOptions.Count; i++)
                {
                    if (Settings.LevelsOptions[i].level_number == currentLevel)
                    {
                        Settings.LevelsOptions[i].player_health += size;
                        break;
                    }
                }
            }
        }
    } 

    

    public void ShakeCamera()
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.ShakeCamera);
        GameEvent?.Invoke(payload);
    } 


    
    public void StopEnemies(bool isPlayerDead)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.StopEnemies);
        payload.Add("dead", isPlayerDead);
        GameEvent?.Invoke(payload);
    }

    public void CreateSlowMotionEffect(float duration, bool shouldShake = false)
    {
        if (!CanSlowMoAfterHit)
        {
            if (shouldShake && CanShakeCameraAfterHit)
            {
                ShakeCamera();
            }
            return;
        };
        StopCoroutine(SlowMotion(duration, shouldShake));
        StartCoroutine(SlowMotion(duration, shouldShake));
    }



    public Vector2 getTargetPosition(Transform origin, Transform target)
    {
        Vector2 pos = new Vector2(0f, 0f);

        pos.x = origin.position.x - target.position.x;
        pos.y = origin.position.y - target.position.y;

        return pos;
    }


    private IEnumerator SlowMotion(float duration, bool shouldShake)
    {
        StartSlowMotion();
        yield return new WaitForSeconds(duration);
        StopSlowMotion();

        if(shouldShake)
        {
            ShakeCamera();
        }
    }


    private void StartSlowMotion()
    {
        Time.timeScale = slowMotionTimeScale;
        Time.fixedDeltaTime = startFixedDeltaTime * slowMotionTimeScale;
    }

    private void StopSlowMotion()
    {
        Time.timeScale = startTimeScale;
        Time.fixedDeltaTime = startFixedDeltaTime;
    }

    public GameSettings.LevelOption GetLevelDifficultyOptions()
    {
        int currentLevel = -1;
        GameSettings.LevelOption currentFactor = new GameSettings.LevelOption();

        if(MenuManager.Instance != null && Settings != null && Settings.LevelsOptions != null && Settings.LevelsOptions .Count > 0)
        {
            currentLevel = MenuManager.Instance.GetCurrentLevel();
            if(currentLevel > 0)
            {
                Settings.LevelsOptions.ForEach(level => {
                    if(level.level_number == currentLevel)
                    {
                        currentFactor = level;
                        return;
                    }
                });
            }
        }

        return currentFactor;
    }

 
    public string[] GetEnemiesDropdown()
    {
        string[] p = new string[Enemies.Count];

        for(int i = 0; i < Enemies.Count; i++)
        {
            p[i] = Enemies[i].Name;
        }

        return p;
    }

    public void InitiateLabel(GameSettings.LabelOptions labelOptions, string text, Transform parent)
    {
        // GameManager.Instance.TextLabelPrefab
        Debug.Log("Calling the initiate of the label");
        GameObject label = (GameObject)Instantiate(GameManager.Instance.TextLabelPrefab, parent.position, Quaternion.identity);

        if (label.transform.GetChild(0))
        {
            GameObject obj = label.transform.GetChild(0).gameObject;
            LabelTextObjectController labelTextObjectController = obj.GetComponent<LabelTextObjectController>();

            TMPro.TextMeshProUGUI textMeshProUGUI = obj.GetComponent<TMPro.TextMeshProUGUI>();
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = text;
            }

            if (labelTextObjectController != null)
            {
                labelTextObjectController.Options = labelOptions;
            }
        }


        label.transform.SetParent(parent);
    }



    [System.Serializable]
    public class Weapon
    {
        private List<string> List { get { return new List<string>() { "SwordChunky", "SwordGolden", "SwordLava", "SwordMeaty", "SwordOnFire"}; } }

        [Dropdown("List")]
        public string ID;

        [Tooltip("Used for objectives, e.g. to make sure that the player did indeed take the weapon.")]
        public string WeaponID;
    }

    [System.Serializable]
    public class Enemy
    {
        private List<string> List { get { return new List<string>() { "AngryBigBoi", "BigBoi", "GreenBi", "MeanSlime", "RedBoi" }; } }

        [Dropdown("List")]
        public string Name;

        [Tooltip("Used for objectives, e.g. to make sure that the player did indeed kill the enemy.")]
        public string EnemyID;
    }

    [System.Serializable]
    public class Key
    {
        public string ID;

        private List<string> List { get { return new List<string>() { "White", "Green", "Blue", "Yellow", "Purple", "Red" }; } }
        
        [Dropdown("List")]
        public string Color;
    }

    [System.Serializable]
    public class Potion
    {
        public string ID;
        public int value;
    }
    
}




public enum GameState
{
    AffectStamina,
    AffectHealth,
    AttackPlayer,
    StopEnemies,
    ShakeCamera
}
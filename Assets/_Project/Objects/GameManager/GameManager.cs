using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;
    public static event Action<Hashtable> GameEvent;

    // list of collected keys
    [SerializeField] public List<ScriptableKey> CollectedKeys;
    
    // list of killed enemies
    [SerializeField] public List<ScriptableEnemy> KilledEnemies;

    // 
    [SerializeField] public WeaponsPack WeaponsPackData;
    [SerializeField] public PlayerStats PlayerData;
    [SerializeField] public Player PlayerScript;

    [SerializeField] public string PlayerTag = "Player";
    [SerializeField] public string EnemyTag = "Enemy";
    [SerializeField] public string StaticTag = "Static";

    [SerializeField] public GameObject PauseMenu;
    [SerializeField] public int SCENE_MAIN = 0;
    [SerializeField] public int SCENE_LEVEL_1 = 1;
    [SerializeField] public int SCENE_LEVEL_2 = 2;

    [SerializeField] public float slowMotionTimeScale = 0.1f;
    private float startTimeScale;
    private float startFixedDeltaTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CollectedKeys = new List<ScriptableKey>();
            KilledEnemies = new List<ScriptableEnemy>();
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

        PauseMenu = GameObject.Find("Menus").transform.GetChild(0).gameObject;
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



    public void StartGame()
    {
        SceneManager.LoadScene(SCENE_LEVEL_1);
    }
    public void StartLevel(int level)
    {
        if(level == 1)
        {
            SceneManager.LoadScene(SCENE_LEVEL_1);
        } else if (level == 2)
        {
            SceneManager.LoadScene(SCENE_LEVEL_2);
        }
    }
    public void OpenMainMenu()
    {
        SceneManager.LoadScene(SCENE_MAIN);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        PauseMenu.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        PauseMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
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
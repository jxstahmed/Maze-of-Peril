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


    
    public void StopEnemies(bool isPlayerDead)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.StopEnemies);
        payload.Add("dead", isPlayerDead);
        GameEvent?.Invoke(payload);
    }

  


    public Vector2 getTargetPosition(Transform origin, Transform target)
    {
        Vector2 pos = new Vector2(0f, 0f);

        pos.x = origin.position.x - target.position.x;
        pos.y = origin.position.y - target.position.y;

        return pos;
    }

   
 
}



public enum GameState
{
    AffectStamina,
    AffectHealth,
    AttackPlayer,
    StopEnemies
}
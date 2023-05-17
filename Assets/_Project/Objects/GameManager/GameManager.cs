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

    public void AttackPlayer(float damage)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.AttackPlayer);
        payload.Add("damage", damage);
        GameEvent?.Invoke(payload);
    } 
    
    public void StopEnemies(bool isPlayerDead)
    {
        Hashtable payload = new Hashtable();
        payload.Add("state", GameState.StopEnemies);
        payload.Add("dead", isPlayerDead);
        GameEvent?.Invoke(payload);
    }

    public void CollectKey(string ID, string Label)
    {
        ScriptableKey collectedKey = new ScriptableKey(ID, Label);
        CollectedKeys.Add(collectedKey);
        Debug.Log("CollectKey");
        Debug.Log(collectedKey.ToString());
    }

    public bool hasCollectedKey(string ID)
    {
        if (CollectedKeys == null || CollectedKeys.Count == 0) return false;
        bool hasFound = false;
        CollectedKeys.ForEach(collectedKey =>
        {
            if(collectedKey.getID() == ID)
            {
                hasFound = true;
                return;
            }
        });

        return hasFound;
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
    AttackPlayer,
    StopEnemies
}
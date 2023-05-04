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
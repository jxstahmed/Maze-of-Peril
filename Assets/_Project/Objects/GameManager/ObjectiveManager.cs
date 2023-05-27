using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class ObjectiveManager : MonoBehaviour
{

    public static ObjectiveManager Instance;
    [SerializeField] public List<LevelObjective> LevelObjectives = new List<LevelObjective>();
    [SerializeField] public List<string> GenericKeys = new List<string>();
   
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

    public int GetCurrentObjectiveNumber()
    {
        int firstMatch = -1;
        for(int i = 0; i < LevelObjectives.Count; i++)
        {
            if (LevelObjectives[i].IsActive())
            {
                firstMatch = i;
                break;
            }
        }

        return firstMatch;
    }
    public LevelObjective GetCurrentObjective()
    {
        LevelObjective levelObjective = null;

        LevelObjectives.ForEach(e => { 
            if(e.IsActive())
            {
                levelObjective = e;
                return;
            }
        });

        return levelObjective;
    }

    public void CollectKey(string ID, string Label)
    {
        Debug.Log("CollectKey, ID: " + ID);
        AddIDToObjective(ID);
    }
    public void CollectCheckpoint(string ID)
    {
        Debug.Log("CollectCheckpoint, ID: " + ID);
        AddIDToObjective(ID);
    }

    public void CollectEnemy(string ID)
    {
        Debug.Log("CollectEnemy, ID: " + ID);
        AddIDToObjective(ID);
    }

    public void AddIDToObjective(string ID)
    {
        if (ID == null) return;
        bool hasAdded = false;
        LevelObjectives.ForEach(levelObjective =>
        {
            if(levelObjective.needIDs.Contains(ID))
            {
                hasAdded = true;
                if (!levelObjective.collectedIDs.Contains(ID))
                {
                    levelObjective.collectedIDs.Add(ID);
                }
            }
            
        });

        if(!hasAdded)
        {
            GenericKeys.Add(ID);
        }
    }


    public bool hasCollectedID(string ID)
    {
        if (LevelObjectives == null || LevelObjectives.Count == 0) return false;
        bool hasFound = false;
        LevelObjectives.ForEach(collectedKey =>
        {
            if (collectedKey.collectedIDs.Contains(ID))
            {
                hasFound = true;
                return;
            }
        });

        if(!hasFound)
        {
            hasFound = GenericKeys.Contains(ID);
        }

        return hasFound;
    }
}

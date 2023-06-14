using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;

public class ObjectiveManager : MonoBehaviour
{

    public static ObjectiveManager Instance;
    [SerializeField] public List<LevelObjective> LevelObjectives = new List<LevelObjective>();
    [SerializeField] public List<string> GenericKeys = new List<string>();

    [SerializeField] TMPro.TextMeshProUGUI ObjectiveTitle;
    
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

    public void FixedUpdate()
    {
        if(ObjectiveTitle != null)
        {
            LevelObjective obj = GetCurrentActiveObjective();
            if(obj != null)
            {
                ObjectiveTitle.text = obj.title + "\n" + "<size=70%><voffset=2em><i>" + obj.description + "</i>";
            }
        }
    }

    public int GetCurrentObjectiveNumber()
    {
        int firstMatch = -1;
        for(int i = 0; i < LevelObjectives.Count; i++)
        {
            if (!LevelObjectives[i].isAccomplished())
            {
                firstMatch = i;
                break;
            }
        }

        return firstMatch;
    }
    public LevelObjective GetCurrentActiveObjective()
    {
        LevelObjective levelObjective = null;

        LevelObjectives.ForEach(e => { 
            if(!e.isAccomplished())
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

    public void CollectChest(string ID)
    {
        Debug.Log("CollectChest, ID: " + ID);
        AddIDToObjective(ID);
    }
    
    public void CollectGate(string ID)
    {
        Debug.Log("CollectGate, ID: " + ID);
        AddIDToObjective(ID);
    }
    
    public void CollectMoveableTriggerPoint(string ID)
    {
        Debug.Log("CollectGate, ID: " + ID);
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

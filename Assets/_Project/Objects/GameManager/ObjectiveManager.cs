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
    [SerializeField] public List<string> CollectedKeys = new List<string>();

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
        if (LevelObjectives == null || LevelObjectives.Count == 0) return -1;

        int firstMatch = -1;
        for(int i = 0; i < LevelObjectives.Count; i++)
        {
            if (!HasAccomplishedObjective(LevelObjectives[i]))
            {
                firstMatch = i;
                break;
            }
        }

        return firstMatch;
    }
    public LevelObjective GetCurrentActiveObjective()
    {
        if (LevelObjectives == null || LevelObjectives.Count == 0) return null;
        LevelObjective levelObjective = null;

        for(int i = 0; i < LevelObjectives.Count; i++)
        {
            if (LevelObjectives[i] != null && !HasAccomplishedObjective(LevelObjectives[i]))
            {
                levelObjective = LevelObjectives[i];
                break;
            }
        }
        return levelObjective;
    }
    
    public void CollectWeapon(string ID)
    {
        Debug.Log("CollectedWeapon, ID: " + ID);
        AddIDToObjective(ID);
    }
    public void CollectKey(string ID)
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

        ValidateEndScreen(ID);

        if (!HasCollectedID(ID))
            CollectedKeys.Add(ID);
    }

    private void ValidateEndScreen(string ID)
    {
        Debug.Log("ID: " + ID + " == " + GameManager.Instance.Settings.LEVEL_1_END_ID);

        if (!ID.Equals(GameManager.Instance.Settings.LEVEL_1_END_ID)) return;

        Debug.Log("END");
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.OpenLevelEndView();
        }
    }


    public bool HasCollectedID(string ID)
    {
        return CollectedKeys.Contains(ID);
    }

    public bool HasAccomplishedObjective(LevelObjective levelObjective)
    {
        if (levelObjective == null) return false;
        if (levelObjective.needIDs.Count == 0) return true;

        bool missing = true;
        levelObjective.needIDs.ForEach(x =>
        {
            if(!CollectedKeys.Contains(x))
            {
                missing = false;
                return;
            }
        });



        return missing;
    }
}

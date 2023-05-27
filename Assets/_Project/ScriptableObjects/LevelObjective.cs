using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Objective")]
public class LevelObjective : ScriptableObject
{
    [Header("Text")]
    public string title;
    public string description;

    [Header("Payload")]
    public List<string> needIDs = new List<string>();
    public List<string> collectedIDs = new List<string>();
    

    [Header("States")]
    public bool isAccomplished = false;

    public bool IsActive()
    {
        return collectedIDs != null && collectedIDs.Count > 0;
    }
}

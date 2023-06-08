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
    

    public bool isAccomplished()
    {
        return collectedIDs != null && needIDs != null && collectedIDs.Count == needIDs.Count;
    }
    public bool IsActive()
    {
        return collectedIDs != null && collectedIDs.Count > 0;
    }
}

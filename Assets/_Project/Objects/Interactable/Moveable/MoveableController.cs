using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public List<string> PushesIDs = new List<string>();
    [SerializeField] public string[] NeedsIDs;
    [SerializeField] public string Label;


    public void Start()
    {

    }

    private bool HasIDs()
    {
        if (NeedsIDs == null || NeedsIDs.Length == 0 || string.Join("", NeedsIDs) == "") return true;


        bool canOpen = true;
        foreach (string keyId in NeedsIDs)
        {
            if (ObjectiveManager.Instance.HasCollectedID(keyId) == false)
            {
                canOpen = false;
                break;
            }
        }

        return canOpen;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.StaticTag))
        {
            if (HasIDs())
            {
                PushesIDs.ForEach(id => ObjectiveManager.Instance.CollectMoveableTriggerPoint(id));
            } else
            {
                if (Label != null && Label != "")
                {
                    GameManager.Instance.InitiateLabel(GameManager.Instance.Settings.ObjectiveLabel, Label, transform);
                }
            }

        }
    }
}

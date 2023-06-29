using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public string ID;
    [SerializeField] public string[] NeedsIDs;
    [SerializeField] public bool HasReached;

    [Tooltip("Select a gate that we completely close after the checkpoint is passed")]
    [SerializeField] public List<GateController> ClosableGate;

    // Update is called once per frame
    void Update()
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
        if (!HasReached && other.CompareTag(GameManager.Instance.PlayerTag))
        {
            if(HasIDs())
            {
                HasReached = true;
                ObjectiveManager.Instance.CollectCheckpoint(ID);

                if(ClosableGate != null && ClosableGate.Count > 0)
                {
                    ClosableGate.ForEach(gate =>
                    {
                        gate.CanOpen = false;
                        gate.SetClosed();
                    });
                }
            }
        }
    }
}

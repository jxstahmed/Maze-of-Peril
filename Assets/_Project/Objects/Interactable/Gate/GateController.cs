using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public string ID;
    [SerializeField] public string Label;
    [SerializeField] public string[] NeedsIDs;
   
    [SerializeField] public bool CanOpen = true;

    [Tooltip("Player's weapon can look ugly. You may disable their weapon once they collide with the door when it's open.")]
    [SerializeField] public bool ShouldHidePlayerWeapon = false;
    [SerializeField] public bool IsOpen = false;

    private bool hasAddedID = false;
    private GameObject openObject;
    private GameObject closeObject;
    [SerializeField] public bool ANIMATOR_TRIGGER_OPEN
    {
        get
        {
            return IsOpen;
        }

        set
        {
            if(value)
            {
                SetOpened();
            } else
            {
                SetClosed();
            }
        }
    }
    void Start()
    {
        openObject = transform.Find("Opened").gameObject;
        closeObject = transform.Find("Closed").gameObject;

        prepareGate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void prepareGate()
    {
        if ((openObject.activeSelf && closeObject.activeSelf) || (!openObject.activeSelf && !closeObject.activeSelf))
        {
            SetClosed();
        }
        else
        {
            VerifyState();
        }
    }

    public void VerifyState()
    {
        if(IsOpen)
        {
            SetOpened();
        } else
        {
            SetClosed();
        }
    }

    public void SetClosed()
    {
        if (IsOpen) AudioManager.Instance.PlayFromPosition(AudioManager.Instance.DoorCloses, gameObject.transform);

        IsOpen = false;
        closeObject.SetActive(true);
        openObject.SetActive(false);
    }

    public void SetOpened()
    {
        if (!CanOpen) return;

        if (!IsOpen) AudioManager.Instance.PlayFromPosition(AudioManager.Instance.DoorOpens, gameObject.transform);

        IsOpen = true;
        closeObject.SetActive(false);
        openObject.SetActive(true);
        if (!hasAddedID)
        {
            hasAddedID = true;
            ObjectiveManager.Instance.CollectGate(ID);
        }
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
        if (CanOpen && other.CompareTag(GameManager.Instance.PlayerTag))
        {
           // Check if we have the collected ids/groups
           if(HasIDs())
            {
                Debug.Log("Gate:OnTriggerEnter2D, HasIDs");
                SetOpened();
                if (IsOpen && ShouldHidePlayerWeapon)
                {
                    Player player = other.GetComponent<Player>();
                    player.HideWeapon();
                }

            } else
            {
                AudioManager.Instance.PlayFromPosition(AudioManager.Instance.ObjectNotYetUnlocked, gameObject.transform);
                if (Label != null && Label != "")
                {
                    GameManager.Instance.InitiateLabel(GameManager.Instance.Settings.ObjectiveLabel, Label, transform);
                }
                Debug.Log("Gate:OnTriggerEnter2D, Not HasIDs");
            }
            
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (CanOpen && other.CompareTag(GameManager.Instance.PlayerTag) && HasIDs() && IsOpen && ShouldHidePlayerWeapon)
        {
            Player player = other.GetComponent<Player>();
            player.HideWeapon();
        } 
    }
}

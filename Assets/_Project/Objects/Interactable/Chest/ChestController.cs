using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public string ID;
    [SerializeField] public string[] NeedsIDs;
    [SerializeField] public List<string> PushesWeaponsIDs = new List<string>();
    [SerializeField] public GameObject GoodiesObject;
    [SerializeField] public bool IsOpen = false;
    [SerializeField] public bool Trigger = false;

    private SpriteRenderer openObject;
    private SpriteRenderer closeObject;
    private bool hasAddedID = false;
    [SerializeField]
    public bool ANIMATOR_TRIGGER_OPEN
    {
        get
        {
            return IsOpen;
        }

        set
        {
            if (value)
            {
                SetOpened();
            }
            else
            {
                SetClosed();
            }
        }
    }
    void Start()
    {
        openObject = transform.Find("Opened").gameObject.GetComponent<SpriteRenderer>();
        closeObject = transform.Find("Closed").gameObject.GetComponent<SpriteRenderer>();

        prepareGate();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(Trigger)
        {
            if (IsOpen)
            {
                SetOpened();
            }
            else
            {
                SetClosed();
            }
            Trigger = false;
        }
    }

    private void prepareGate()
    {
        if ((openObject.enabled && closeObject.enabled) || (!openObject.enabled && !closeObject.enabled))
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
        if (IsOpen)
        {
            SetOpened();
        }
        else
        {
            SetClosed();
        }
    }

    public void SetClosed()
    {
        IsOpen = false;
        closeObject.enabled = true;
        openObject.enabled = false;
    }

    public void SetOpened()
    {
        IsOpen = true;
        closeObject.enabled = false;
        openObject.enabled = true;
        pushWeapons();

        if(!hasAddedID)
        {
            hasAddedID = true;
            ObjectiveManager.Instance.CollectChest(ID);
        }
    }

    public void pushWeapons()
    {
        if(PushesWeaponsIDs != null && PushesWeaponsIDs.Count > 0)
        {
           
            for(int i = 0; i < PushesWeaponsIDs.Count; i++)
            {
                Debug.Log(PushesWeaponsIDs[i]);
                int weaponIndex = WeaponsManager.Instance.FindWeaponIndex(PushesWeaponsIDs[i]);
                WeaponStats weaponStatsProfile = GameManager.Instance.WeaponsPackData.Swords[weaponIndex];

                if (weaponStatsProfile != null)
                {
                    //weaponStatsProfile.prefab.
                    WeaponStatsProfile controller = weaponStatsProfile.prefab.GetComponent<WeaponStatsProfile>();
                    controller.isEquipEnabled = true;

                    GameObject obj = Instantiate(weaponStatsProfile.prefab, Vector2.zero, weaponStatsProfile.prefab.transform.rotation);


                    obj.transform.SetParent(GoodiesObject.transform, true);
                    obj.transform.localPosition = weaponStatsProfile.Position;
                   
                }

                PushesWeaponsIDs.RemoveAt(i);
                i--;
            }
        }
    }


    private bool HasIDs()
    {
        if (NeedsIDs == null || NeedsIDs.Length == 0 || string.Join("", NeedsIDs) == "") return true;


        bool canOpen = true;
        foreach (string keyId in NeedsIDs)
        {
            if (ObjectiveManager.Instance.hasCollectedID(keyId) == false)
            {
                canOpen = false;
                break;
            }
        }

        return canOpen;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(GameManager.Instance.PlayerTag))
        {
            // Check if we have the collected ids/groups
            if (HasIDs())
            {
                Debug.Log("Chest:OnTriggerEnter2D, HasIDs");
                SetOpened();
            }
            else
            {
                Debug.Log("Chest:OnTriggerEnter2D, Not HasIDs");
            }

        }
    }
}

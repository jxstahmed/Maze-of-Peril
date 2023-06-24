using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChestController : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public string Label;
    [SerializeField] public string ID;
    [SerializeField] public string[] NeedsIds;
    [SerializeField] public List<GameManager.Weapon> PushesWeaponsIds = new List<GameManager.Weapon>();


    [SerializeField] public List<GameManager.Enemy> PushesEnemiesIds = new List<GameManager.Enemy>();


    [SerializeField] public List<GameManager.Key> PushesKeysIds = new List<GameManager.Key>();
    [SerializeField] public bool IsOpen = false;
    [SerializeField] public bool Trigger = false;

    private SpriteRenderer openObject;
    private SpriteRenderer closeObject;
    private bool hasAddedID = false;
    private LabelTextController labelController;
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
        labelController = GetComponent<LabelTextController>();
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
        if (IsOpen) AudioManager.Instance.PlayFromPosition(AudioManager.Instance.ChestCloses, gameObject.transform);

        IsOpen = false;
        closeObject.enabled = true;
        openObject.enabled = false;
    }

    public void SetOpened()
    {
        // If it was closed and now opened, we activate the sound
        if(!IsOpen) AudioManager.Instance.PlayFromPosition(AudioManager.Instance.ChestOpens, gameObject.transform);


        IsOpen = true;
        closeObject.enabled = false;
        openObject.enabled = true;
        pushWeapons();
        pushEnemies();
        pushKeys();
        

        if(!hasAddedID)
        {
            hasAddedID = true;
            ObjectiveManager.Instance.CollectChest(ID);
        }
    }

    public void pushEnemies()
    {
        if (PushesEnemiesIds != null && PushesEnemiesIds.Count > 0)
        {
            for (int i = 0; i < PushesEnemiesIds.Count; i++)
            {
                // search for it in the game manager
                for (int j = 0; j < GameManager.Instance.Enemies.Count; j++)
                {
                    if (GameManager.Instance.Enemies[j].Name.Equals(PushesEnemiesIds[i].Name))
                    {
                        GameObject parent = new GameObject("EnemyParent");
                        parent.transform.SetParent(transform);
                        parent.transform.localPosition = new Vector2(0, -1f);

                        GameObject obj = Instantiate(GameManager.Instance.Enemies[j].Prefab, Vector2.zero, GameManager.Instance.Enemies[j].Prefab.transform.rotation);

                        NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
                        agent.enabled = false;
                        EnemyAgentController controller = obj.GetComponent<EnemyAgentController>();
                        controller.ID = PushesEnemiesIds[i].EnemyID;
                        
                        controller.CanPatrol = false;
                        obj.transform.SetParent(parent.transform, true);
                        obj.transform.localPosition = new Vector2(0, 0);
                        agent.enabled = true;

                        PushesEnemiesIds.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    public void pushKeys()
    {
        if(PushesKeysIds != null && PushesKeysIds.Count > 0) {
            for(int i = 0; i < PushesKeysIds.Count; i++)
            {
                // search for it in the game manager
                for(int j = 0; j < GameManager.Instance.Keys.Count; j++)
                {
                    if (GameManager.Instance.Keys[j].Color.Equals(PushesKeysIds[i].Color))
                    {
                        GameObject parent = new GameObject("KeyParent");
                        parent.transform.SetParent(transform);
                        parent.transform.localPosition = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-1.5f, -0.5f));

                        GameObject obj = Instantiate(GameManager.Instance.Keys[j].Prefab, Vector2.zero, GameManager.Instance.Keys[j].Prefab.transform.rotation);


                        KeyController controller = obj.GetComponent<KeyController>();
                        controller.ID = PushesKeysIds[i].ID;
                        obj.transform.SetParent(parent.transform, true);
                        obj.transform.localPosition = new Vector2(0, 0);

                        PushesKeysIds.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
    public void pushWeapons()
    {
        if(PushesWeaponsIds != null && PushesWeaponsIds.Count > 0)
        {
           
            for(int i = 0; i < PushesWeaponsIds.Count; i++)
            {
                int weaponIndex = WeaponsManager.Instance.FindWeaponIndex(PushesWeaponsIds[i].ID);
                WeaponStats weaponStatsProfile = GameManager.Instance.WeaponsPackData.Swords[weaponIndex];

                if (weaponStatsProfile != null)
                {
                    //weaponStatsProfile.prefab.
                    WeaponStatsProfile controller = weaponStatsProfile.prefab.GetComponent<WeaponStatsProfile>();
                    controller.isEquipEnabled = true;
                    controller.ID = PushesWeaponsIds[i].WeaponID;

                    GameObject parent = new GameObject("WeaponParent");
                    parent.transform.SetParent(transform);
                    parent.transform.localPosition = new Vector2(0f, -0.6f);


                    GameObject obj = Instantiate(weaponStatsProfile.prefab, Vector2.zero, weaponStatsProfile.prefab.transform.rotation);


                    obj.transform.SetParent(parent.transform, true);
                    obj.transform.localPosition = weaponStatsProfile.Position;
                   
                }

                PushesWeaponsIds.RemoveAt(i);
                i--;
            }
        }
    }


    private bool HasIDs()
    {
        if (NeedsIds == null || NeedsIds.Length == 0 || string.Join("", NeedsIds) == "") return true;


        bool canOpen = true;
        foreach (string keyId in NeedsIds)
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
                AudioManager.Instance.PlayFromPosition(AudioManager.Instance.ObjectNotYetUnlocked, gameObject.transform);

                if(labelController != null && Label != null && Label != "")
                {
                    labelController.Initiate(Label, transform);
                }

                Debug.Log("Chest:OnTriggerEnter2D, Not HasIDs");
            }

        }
    }
}

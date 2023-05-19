using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public string[] NeedsIDs;
    [SerializeField] public bool IsOpen = false;
    
    private Animator animator;

    private SpriteRenderer openObject;
    private SpriteRenderer closeObject;
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
        animator = GetComponent<Animator>();
        openObject = transform.Find("Opened").gameObject.GetComponent<SpriteRenderer>();
        closeObject = transform.Find("Closed").gameObject.GetComponent<SpriteRenderer>();

        prepareGate();
    }

    // Update is called once per frame
    void Update()
    {
        
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

    private bool HasIDs()
    {
        if (NeedsIDs == null || NeedsIDs.Length == 0 || string.Join("", NeedsIDs) == "") return true;


        bool canOpen = true;
        foreach(string keyId in NeedsIDs) {
            if(GameManager.Instance.hasCollectedKey(keyId) == false)
            {
                canOpen = false;
                break;
            }
        }

        return canOpen;
    }

 
    private void TriggerAnimation(string Type)
    {
        // Avoid using the animation for now. We have two simple prefabs and the animation takes time
        if(Type == "open")
        {
            Debug.Log("Trigger open");
            animator.SetTrigger("open");
        } else
        {
            animator.SetTrigger("close");
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
        IsOpen = false;
        closeObject.enabled = true;
        openObject.enabled = false;
    }

    public void SetOpened()
    {
        IsOpen = true;
        closeObject.enabled = false;
        openObject.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
           // Check if we have the collected ids/groups
           if(HasIDs())
            {
                Debug.Log("Door:OnTriggerEnter2D, HasIDs");
                SetOpened();
            } else
            {
                Debug.Log("Door:OnTriggerEnter2D, Not HasIDs");
            }
            
        }
    }
}

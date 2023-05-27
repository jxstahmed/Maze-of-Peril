using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyController : MonoBehaviour
{

   
    [SerializeField] public string Label;
    [SerializeField] public string ID;

    // Start is called before the first frame update
    void Start()
    {
       

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Contains(GameManager.Instance.PlayerTag))
        {
            ObjectiveManager.Instance.CollectKey(ID, Label);
            Destroy(gameObject);
        }
    }
}

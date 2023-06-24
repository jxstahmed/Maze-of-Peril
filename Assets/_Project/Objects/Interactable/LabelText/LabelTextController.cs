using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelTextController : MonoBehaviour
{
    public void Initiate(string text, Transform parent)
    {
        // GameManager.Instance.TextLabelPrefab
        Debug.Log("Calling the initiate of the label");
        GameObject label = (GameObject)Instantiate(GameManager.Instance.TextLabelPrefab, parent.position, Quaternion.identity);

        Canvas canvas = label.GetComponent<Canvas>();
        if(canvas)
        {
            TMPro.TextMeshProUGUI textMeshProUGUI = canvas.GetComponent<TMPro.TextMeshProUGUI>();
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = text;
            }
        }
        

        label.transform.SetParent(parent);
    }
}

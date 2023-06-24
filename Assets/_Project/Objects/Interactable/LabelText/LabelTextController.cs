using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelTextController : MonoBehaviour
{
    public void Initiate(GameSettings.LabelOptions labelOptions, string text, Transform parent)
    {
        // GameManager.Instance.TextLabelPrefab
        Debug.Log("Calling the initiate of the label");
        GameObject label = (GameObject)Instantiate(GameManager.Instance.TextLabelPrefab, parent.position, Quaternion.identity);

        if(label.transform.GetChild(0))
        {
            GameObject obj = label.transform.GetChild(0).gameObject;
            LabelTextObjectController labelTextObjectController = obj.GetComponent<LabelTextObjectController>();

            TMPro.TextMeshProUGUI textMeshProUGUI = obj.GetComponent<TMPro.TextMeshProUGUI>();
            if (textMeshProUGUI != null)
            {
                textMeshProUGUI.text = text;
            }

            if(labelTextObjectController != null)
            {
                labelTextObjectController.Options = labelOptions;
            }
        }
        

        label.transform.SetParent(parent);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelTextObjectController : MonoBehaviour
{
    
    public void Start()
    {
        StartCoroutine(FadeOut());
    }

    private void FixedUpdate()
    {
        // Moving the item
        transform.Translate(Vector2.up * GameManager.Instance.Settings.LabelTextSpeed * Time.deltaTime);
    }

    private IEnumerator FadeOut()
    {
        float startAlpha = GetComponent<TMPro.TextMeshProUGUI>().color.a;

        float rate = 1.0f / GameManager.Instance.Settings.LabelTextFadeOutTime;
        float progress = 0.0f;
        while (progress < 1.0f)
        {
            if (GetComponent<TMPro.TextMeshProUGUI>() == null) break;

            Color tmpColor = GetComponent<TMPro.TextMeshProUGUI>().color;
            GetComponent<TMPro.TextMeshProUGUI>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));
            progress += rate * Time.deltaTime;

            yield return null;
        }

        Destroy(gameObject);
    }
}

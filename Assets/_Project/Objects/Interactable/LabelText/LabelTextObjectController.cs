using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LabelTextObjectController : MonoBehaviour
{
    public GameSettings.LabelOptions Options;
    public void Start()
    {
        if (GetComponent<TMPro.TextMeshProUGUI>() != null)
        {
            Debug.Log(Options.text_color);
            GetComponent<TMPro.TextMeshProUGUI>().color = Options.text_color;
            GetComponent<TMPro.TextMeshProUGUI>().fontSize = Options.text_size;
        }

        transform.localPosition = new Vector2(transform.localPosition.x, transform.localPosition.y + 50f);
        StartCoroutine(FadeOut());
    }

    private void FixedUpdate()
    {
        // Moving the item
        transform.Translate(Vector2.up * Options.text_speed * Time.deltaTime);
    }

    private IEnumerator FadeOut()
    {
        float startAlpha = GetComponent<TMPro.TextMeshProUGUI>().color.a;

        float rate = 1.0f / Options.fade_out_time;
        float progress = 0.0f;
        while (progress < 1.0f)
        {
            if (GetComponent<TMPro.TextMeshProUGUI>() == null) break;

            Color tmpColor = GetComponent<TMPro.TextMeshProUGUI>().color;
            GetComponent<TMPro.TextMeshProUGUI>().color = new Color(tmpColor.r, tmpColor.g, tmpColor.b, Mathf.Lerp(startAlpha, 0, progress));
            progress += rate * Time.deltaTime;

            yield return null;
        }

        Destroy(transform.parent.gameObject);
    }
}

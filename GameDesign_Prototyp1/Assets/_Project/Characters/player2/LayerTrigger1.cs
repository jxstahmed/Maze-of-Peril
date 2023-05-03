using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerTrigger1 : MonoBehaviour
{
    public string layer;
    public string sortingLayer;

    private void OnTriggerExit2D(Collider2D other)
    {
        other.gameObject.layer = LayerMask.NameToLayer(layer);

        other.gameObject.GetComponent<SpriteRenderer>().sortingLayerName = sortingLayer;
        SpriteRenderer[] srs = other.gameObject.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in srs)
        {
            sr.sortingLayerName = sortingLayer;
        }
    }
}

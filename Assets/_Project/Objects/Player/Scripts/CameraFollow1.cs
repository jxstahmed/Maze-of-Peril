using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollow1 : MonoBehaviour
{


    public Transform target;
    public float lerpSpeed = 1.0f;

    private Vector3 offset;
    private Vector3 targetPos;



    private void Start()
    {
        if (target == null) return;
        Vector3 campos = target.position;
        campos.z = transform.position.z;
        transform.position = campos;
        offset = transform.position - target.position;

        
    }

    private void Update()
    {
        if (target == null) return;

        targetPos = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, lerpSpeed * Time.deltaTime);

    }

}

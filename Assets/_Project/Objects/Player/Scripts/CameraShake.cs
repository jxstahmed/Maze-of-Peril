using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] public CinemachineVirtualCamera Cinema;
    private CinemachineBasicMultiChannelPerlin CinemaPerlin;

    private void Awake()
    {
        GameManager.GameEvent += onGameEventListen;
    }

    private void OnDestroy()
    {
        GameManager.GameEvent -= onGameEventListen;

    }

    private void Start()
    {
        CinemaPerlin = Cinema.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        CinemaPerlin.m_AmplitudeGain = 0;
    }

  

    private void onGameEventListen(Hashtable payload)
    {
        if ((GameState)payload["state"] == GameState.ShakeCamera)
        {
            StopAllCoroutines();
            StartCoroutine(ShakeCamera());
        }
    }

    private IEnumerator ShakeCamera()
    {
        CinemaPerlin.m_AmplitudeGain = GameManager.Instance.ShakeIntensity;
        yield return new WaitForSeconds(GameManager.Instance.ShakeDuration);
        CinemaPerlin.m_AmplitudeGain = 0;
    }
}

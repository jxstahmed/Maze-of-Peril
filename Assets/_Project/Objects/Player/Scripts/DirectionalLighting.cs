using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DirectionalLighting : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Player PlayerScript;
    [SerializeField] public Light2D LightSpotDirectional;

    [Header("Light")]
    [SerializeField] public float playerDirectionalCameraPositionXDefault = 0f;
    [SerializeField] public float playerDirectionalCameraPositionYDefault = 0f;

    [SerializeField] public float playerDirectionalCameraPositionDistance = 0.09f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(PlayerScript != null && LightSpotDirectional != null)
        {
            PlayerLight();
        }
    }

    private void PlayerLight()
    {

        // -90 => right, -180 => bottom, -270 => left, 0 => up
        float player_direction_z = PlayerScript.PlayerMovingAngle(true);
        float player_position_x = playerDirectionalCameraPositionXDefault;
        float player_position_y = playerDirectionalCameraPositionYDefault;

        if (PlayerScript.IsPlayerMoving(Player.PLAYER_DIRECTIONS.RIGHT, true))
        {
            player_position_x = -playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionXDefault;
        }

        if (PlayerScript.IsPlayerMoving(Player.PLAYER_DIRECTIONS.LEFT, true))
        {
            player_position_x = playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionXDefault;
        }

        if (PlayerScript.IsPlayerMoving(Player.PLAYER_DIRECTIONS.UP, true))
        {
            player_position_y = -playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionYDefault;
        }

        if (PlayerScript.IsPlayerMoving(Player.PLAYER_DIRECTIONS.DOWN, true))
        {
            player_position_y = playerDirectionalCameraPositionDistance + playerDirectionalCameraPositionYDefault;
        }

        LightSpotDirectional.transform.localRotation = Quaternion.Euler(LightSpotDirectional.transform.rotation.x, LightSpotDirectional.transform.rotation.y, player_direction_z);

        LightSpotDirectional.transform.localPosition = new Vector3(player_position_x, player_position_y, transform.localPosition.z);
    }
}

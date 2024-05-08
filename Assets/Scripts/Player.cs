using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;

    private Vector3 playerScreenPos;
    void Start()
    {
        playerScreenPos = mainCam.WorldToScreenPoint(transform.position);
    }

    void Update()
    {
        var mouseScreenPose = Input.mousePosition;
        // rotate player to mouse position by player screen position and mouse screen position
        float angle = Mathf.Atan2(mouseScreenPose.y - playerScreenPos.y, mouseScreenPose.x - playerScreenPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(-angle, 90f, -90f);
    }
}

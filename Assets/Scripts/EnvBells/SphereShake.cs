using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereShake : MonoBehaviour
{
    [SerializeField]
    private float speed = 1.5f;
    private const float shift = 0.5f;
    private float baseZ;
    void Start()
    {
        baseZ = transform.position.z;
    }

    void Update()
    {
        float shiftedZ = Mathf.Lerp(baseZ, baseZ + shift, Mathf.PingPong(Time.time * speed, 1));
        transform.position = new Vector3(transform.position.x, transform.position.y, shiftedZ);
    }
}

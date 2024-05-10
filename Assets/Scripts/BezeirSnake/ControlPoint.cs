using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Orange color in dec rgb is 255, 165, 0
        Gizmos.color = new Color(255, 165, 0, 0.5f);
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}

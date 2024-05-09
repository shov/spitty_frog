using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPlaceholderCollider : MonoBehaviour
{
    public delegate void OnColliderTriggered(int index, Collider other);

    public event OnColliderTriggered ColliderTriggered;

    public int index = -1;

    private void OnTriggerEnter(Collider other)
    {
        ColliderTriggered?.Invoke(index, other);
    }
}

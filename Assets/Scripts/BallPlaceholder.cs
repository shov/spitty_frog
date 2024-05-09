using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPlaceholder : MonoBehaviour
{
    private BallPlaceholderCollider ballPlaceholderCollider;

    private int m_index = -1;
    public int Index
    {
        get
        {
            return m_index;
        }
        set
        {
            m_index = value;
            if (null != ballPlaceholderCollider)
            {
                ballPlaceholderCollider.index = value;
            }
        }
    }

    public BallPlaceholderCollider BallPlaceholderCollider
    {
        get
        {
            return ballPlaceholderCollider;
        }
    }

    private void Awake()
    {
        ballPlaceholderCollider = GetComponentInChildren<BallPlaceholderCollider>();
        Debug.Log($"BallPlaceholderCollider: {ballPlaceholderCollider}");
    }
    // draw gizmos in editor
    private void OnDrawGizmos()
    {
        // Gizmos.color = Color.white;
        // Gizmos.DrawSphere(transform.position, 0.5f);
    }
}

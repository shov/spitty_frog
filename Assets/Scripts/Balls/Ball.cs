using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [SerializeField]
    protected float moveSpeed = 2f;

    protected Vector3? target = null;
    public bool IsMoving { get; protected set; } = false;
    public void Move(Vector3 target)
    {
        this.target = target;
        IsMoving = true;
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.Value, moveSpeed * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, target.Value) < 0.01f)
            {
                target = null;
                IsMoving = false;
            }
        }
    }
}

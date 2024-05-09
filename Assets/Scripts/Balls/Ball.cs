using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    // STATE
    public enum EBallState
    {
        Neutral,
        Fired,
        Snaked,
    }

    public EBallState State { get; protected set; } = EBallState.Neutral;

    public void SetState(EBallState state)
    {
        State = state;
    }

    // MOVEMENT
    protected float defaultMoveSpeed = 2f;
    protected float currentMoveSpeed = 2f;

    protected Vector3? target = null;
    public bool IsMoving { get; protected set; } = false;
    public void Move(Vector3 target, float speed)
    {
        this.target = target;
        currentMoveSpeed = speed;

        IsMoving = true;
    }
    public void Move(Vector3 target)
    {
        this.target = target;
        IsMoving = true;
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.Value, currentMoveSpeed * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, target.Value) < 0.01f)
            {
                target = null;
                currentMoveSpeed = defaultMoveSpeed;
                IsMoving = false;
            }
        }

        // Selfdestroy if fired and out of screen
        if (State == EBallState.Fired)
        {
            var screenPos = Camera.main.WorldToScreenPoint(transform.position);
            if (screenPos.y > Screen.height || screenPos.y < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

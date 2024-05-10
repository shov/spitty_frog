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

    // COLOR
    public virtual Generator.EBallType GetColor() {         
        throw new System.NotImplementedException();
    }

    // SCORE
    public virtual int GetScore(int stackedScore)
    {
        throw new System.NotImplementedException();
    }

    // MOVEMENT
    protected float defaultMoveSpeed = 2f;
    protected float currentMoveSpeed = 2f;

    protected Vector3? target = null;
    protected List<Vector3> targetList = null;

    public bool IsMoving { get; protected set; } = false;

    // Helper
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    public void Move(Vector3 target, float speed)
    {
        this.targetList = null;
        this.target = target;
        currentMoveSpeed = speed;

        IsMoving = true;
    }

    public void MoveByTargetList()
    {
        if(targetList.Count == 0)
        {
            HoldOn();
            return;
        }

        this.target = targetList[0];
        targetList.RemoveAt(0);

        IsMoving = true;
    }


    public void Move(Vector3 target)
    {
        this.targetList = null;
        this.target = target;
        IsMoving = true;
    }

    public void HoldOn()
    {
        target = null;
        this.targetList = null;
        currentMoveSpeed = defaultMoveSpeed;
        IsMoving = false;
    }

    public void MoveByPath(List<Vector3> path, float speed) {
        targetList = path;
        currentMoveSpeed = speed;
        MoveByTargetList();
    }

    void FixedUpdate()
    {
        if (target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.Value, currentMoveSpeed * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, target.Value) < 0.01f)
            {
                if (targetList == null)
                {
                    HoldOn();
                } else
                {
                    MoveByTargetList();
                }
            }
        }

        // Selfdestroy if fired and out of screen
        if (State == EBallState.Fired)
        {
            var screenPos = camera.WorldToScreenPoint(transform.position);
            if (screenPos.y > Screen.height || screenPos.y < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}

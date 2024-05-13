using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveGenMeshCollider : MonoBehaviour
{
    public delegate void OnMeetFiredBallHandler(Collider ball);

    public event OnMeetFiredBallHandler OnMeetFiredBall;

    public delegate void OnRayHitsTheColliderHandler(Vector3 hitPoint, GameObject firedBall, Action<Vector3> performFire);

    public event OnRayHitsTheColliderHandler OnRayHitsTheCollider;

    public void RayHitsTheCollider(Vector3 hitPoint, GameObject firedBall, Action<Vector3> performFire) {
        OnRayHitsTheCollider?.Invoke(hitPoint, firedBall, performFire);
    }

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponent<Ball>();

        // Check if has no Ball Component
        if (ball == null)
        {
            return;
        }

        // Check if the ball is fired
        if (ball.State != Ball.EBallState.Fired)
        {
            return;
        }

        // Fire the event
        OnMeetFiredBall?.Invoke(other);
    }
}

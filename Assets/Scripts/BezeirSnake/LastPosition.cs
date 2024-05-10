using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LastPosition : MonoBehaviour
{
    public delegate void LastPositionTouchedHandler(Collider ball);

    public event LastPositionTouchedHandler OnLastPositionTouched;

    private void OnTriggerEnter(Collider other)
    {
        // Check has no Ball component
        if (other.GetComponent<Ball>() == null)
        {
            return;
        }

        // Check the ball is snaked
        var ball = other.GetComponent<Ball>();
        if(ball.State != Ball.EBallState.Snaked)
        {
            return;
        }

        OnLastPositionTouched?.Invoke(other);
    }
}

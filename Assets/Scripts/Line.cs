using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Line : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 70f;

    [SerializeField]
    private const int ballCount = 5;

    [SerializeField]
    private GameObject startPlaceHolder;

    [SerializeField]
    private GameObject[] ballPlaceholderList = new GameObject[ballCount];

    private List<GameObject> ballList = new List<GameObject>();

    private bool isBallAdding = false;

    void FixedUpdate()
    {
        if (ballList.Count < ballCount && !isBallAdding)
        {
            StartCoroutine(AddBallRoutine());
        }
    }

    public GameObject GetNextBall(Vector3 warpTo)
    {
        if (ballList.Count == 0)
        {
            return null;
        }

        var nextBall = ballList[0];
        nextBall.transform.position = warpTo;

        // Set default layer
        nextBall.layer = 0;

        // Remove this GO from parent, so ball now has no parent
        nextBall.transform.SetParent(null);

        // Scale them back to normal
        nextBall.transform.localScale = new Vector3(1, 1, 1);

        ballList.RemoveAt(0);
        return nextBall;
    }

    protected IEnumerator AddBallRoutine()
    {
        isBallAdding = true;

        FillNextBall();

        yield return new WaitForSeconds(0.5f);
        isBallAdding = false;
    }
    protected void FillNextBall()
    {
        // at first move all balls to the top
        int targetPositionForNextBall = 0;
        {
            int i = 0;
            foreach (var ball in ballList)
            {
                ball.GetComponent<Ball>().Move(ballPlaceholderList[i].transform.position, moveSpeed);
                i++;
            }
            targetPositionForNextBall = i;
        }

        if (targetPositionForNextBall >= ballCount)
        {
            // Unexpected situation
            Debug.LogError($"Unexpected situation target position for Line next ball is {targetPositionForNextBall}");
            return;
        }

        var startPostion = startPlaceHolder.transform.position;
        var targetPosition = ballPlaceholderList[targetPositionForNextBall].transform.position;
        var nextBall = Generator.GetNextAtPosition(startPostion);

        // Set UI layer
        nextBall.layer = 5;
        // Set this GO as a parent to the ball
        nextBall.transform.SetParent(transform);
        // Set ball scale to placeholder scale
        nextBall.transform.localScale = ballPlaceholderList[targetPositionForNextBall].transform.localScale;

        ballList.Add(nextBall);

        var ballComponent = nextBall.GetComponent<Ball>();
        ballComponent.SetState(Ball.EBallState.Neutral);
        ballComponent.Move(targetPosition, moveSpeed);
    }
}

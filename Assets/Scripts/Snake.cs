using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Snake : MonoBehaviour
{

    private const int stepCount = 13;

    [SerializeField]
    private GameObject[] stepPlaceholderList = new GameObject[stepCount];
    private BallPlaceholder[] ballPlaceholderList = new BallPlaceholder[stepCount];

    [SerializeField]
    private GameObject startPlaceholder;

    [SerializeField]
    private GameObject frog;

    [SerializeField]
    private float stepSpeed = 1.5f;

    [SerializeField]
    private float shiftSpeed = 30f;

    private bool isStepping = false;
    private bool isInjecting = false;

    private GameObject?[] ballList = new GameObject[stepCount];

    void Start()
    {
        for (int i = 0; i < stepCount; i++)
        {
            ballPlaceholderList[i] = stepPlaceholderList[i].GetComponentInChildren<BallPlaceholder>();
            ballPlaceholderList[i].Index = i;
            if (null == ballPlaceholderList[i].BallPlaceholderCollider)
            {
                // Extremely unexpected
                throw new System.Exception($"BallPlaceholderCollider is null, index {i}");
            }

            ballPlaceholderList[i].BallPlaceholderCollider.ColliderTriggered += OnTriggerPlaceholder;
        }
    }

    void FixedUpdate()
    {
        if (!isStepping && !isInjecting && !GameController.Instance.IsGameOver)
        {
            StartCoroutine(MakeAStep());
        }
    }

    protected IEnumerator MakeAStep()
    {
        isStepping = true;

        Step();

        yield return new WaitForSeconds(stepSpeed);
        isStepping = false;
    }

    protected void Step()
    {
        // all the balls set to the next step and moves to their locations
        // if that the last step, the ball attaks the frog
        // then new ball received from generator and set to the first step (placed to start and moves to the first step)
        for (int i = stepCount - 1; i >= 0; i--)
        {
            MoveToTheRight(i);
        }

        var nextBall = Generator.GetNextAtPosition(startPlaceholder.transform.position);
        ballList[0] = nextBall;

        var nextBallComponent = nextBall.GetComponent<Ball>();
        nextBallComponent.SetState(Ball.EBallState.Snaked);
        nextBallComponent.Move(stepPlaceholderList[0].transform.position, shiftSpeed);
    }

    protected void OnTriggerPlaceholder(int index, Collider other)
    {
        // if has no Ball component, return
        Ball ball = other.GetComponent<Ball>();
        if (null == ball)
        {
            return;
        }


        // if the ball is not Fired, return
        if (ball.State != Ball.EBallState.Fired)
        {
            return;
        }

        InjectBall(index, other.gameObject);
    }

    protected void MoveToTheRight(int i)
    {
        var ball = ballList[i];
        if (null == ball)
        {
            return;
        }

        Ball ballComponent = ball.GetComponent<Ball>();

        // last step
        if (i == stepCount - 1)
        {
            // attack the frog
            ballComponent.Move(frog.transform.position, shiftSpeed);
            ballList[i] = null;
            return;
        }

        // move to the next step
        ballComponent.Move(stepPlaceholderList[i + 1].transform.position, shiftSpeed);
        ballList[i] = null;
        ballList[i + 1] = ball;
    }

    protected void InjectBall(int index, GameObject ball)
    {
        isInjecting = true;

        // if index taken, move all the balls to the right
        if (null != ballList[index])
        {
            for (int i = stepCount - 1; i > index; i--)
            {
                MoveToTheRight(i);
            }
        }

        ballList[index] = ball;
        var ballComponent = ball.GetComponent<Ball>();
        ballComponent.SetState(Ball.EBallState.Snaked);
        ballComponent.Move(stepPlaceholderList[index].transform.position, shiftSpeed);

        ShiftLeftAll();
        Check();

        isInjecting = false;
    }

    protected void ShiftLeftAll()
    {
        bool madeShift = false;
        do
        {
            madeShift = false;
            for (int i = 1; i < stepCount - 1; i++)
            {
                if (null == ballList[i])
                {
                    continue;
                }

                if (null == ballList[i - 1])
                {
                    ballList[i - 1] = ballList[i];
                    ballList[i] = null;
                    var ballComponent = ballList[i - 1].GetComponent<Ball>();
                    ballComponent.Move(stepPlaceholderList[i - 1].transform.position, shiftSpeed);
                    madeShift = true;
                }
            }
        } while (madeShift);
    }

    protected void Check()
    {
        // TODO check if the same color balls are connected (score making logic)        
        //ShiftLeftAll();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierSnake : MonoBehaviour
{
    public enum ESnakeState
    {
        AddNext,
        Creep,
        Inject,
        Check,
        Shift,
        LastShoot,
    }

    public ESnakeState State { get; private set; } = ESnakeState.Creep;

    [SerializeField]
    private GameObject ballPrefab;

    [SerializeField]
    private Transform[] controlPointList;
    private Vector3[] cpPosList;

    [SerializeField]
    private GameObject curve;
    private LineRenderer lineRenderer;

    private const float tStep = 0.05f;

    [SerializeField]
    private Transform nextBallStartPlaceholder;

    [SerializeField]
    LastPosition lastPosition;

    [SerializeField]
    private GameObject frog;

    [SerializeField]
    private float addNextFreq = 1.5f;
    [SerializeField]
    private float nextAppearSpeed = 5f;
    [SerializeField]
    private float creepTSpeed = 0.005f;
    [SerializeField]
    private float injectSpeed = 5f;
    [SerializeField]
    private float shiftSpeed = 5f;


    private List<GameObject> ballList = new List<GameObject>();
    private Dictionary<GameObject, float> ballPosDict = new Dictionary<GameObject, float>();
    private float ballRadius;
    private float curveLen;
    private float ballSizeOnCurve;

    private Coroutine startedCoroutine;

    private void Awake()
    {
        lineRenderer = curve.GetComponent<LineRenderer>();
        ballRadius = ballPrefab.transform.localScale.x / 2;
        lastPosition.OnLastPositionTouched += LastShoot;
        Curve curveComponent = curve.GetComponent<Curve>();
        curveComponent.OnMeetFiredBall += Inject;
    }

    private void Start()
    {
        FillCpPosList();

        curve.SetActive(true);
        DrawBezierCurve();
        GameController.Instance.Resume(); // Start the game running
    }

    private void Update()
    {
        // only in editor mode
        if (!Application.isEditor)
        {
            //DrawBezierCurve();
            //SetEdgeCollider();
        }

        if (!GameController.Instance.IsRun)
        {
            return;
        }

        if (State == ESnakeState.Creep && startedCoroutine == null)
        {
            startedCoroutine = StartCoroutine(AddNextBallRoutine());
        }

        if (State == ESnakeState.Creep)
        {
            Creep();
        }

        switch(State)
        {
            case ESnakeState.AddNext:
            case ESnakeState.Creep:
                GameController.Instance.IsFireAllowed = true;
                break;
            
            default:
                GameController.Instance.IsFireAllowed = false;
                break;
        }
    }

    private IEnumerator AddNextBallRoutine()
    {
        yield return new WaitForSeconds(addNextFreq);

        State = ESnakeState.AddNext;
        var ballGO = AddNextBall();
        yield return new WaitForSeconds(1 / nextAppearSpeed);
        State = ESnakeState.Creep;
        startedCoroutine = null;
    }

    private GameObject AddNextBall()
    {
        var ballGO = Generator.GetNextAtPosition(nextBallStartPlaceholder.position);
        Ball ballComponent = ballGO.GetComponent<Ball>();
        ballComponent.SetState(Ball.EBallState.Snaked);

        // Move all balls to the right
        if (ballList.Count > 0)
        {
            float theFirstBalAndSizeBallDiff = ballSizeOnCurve - ballPosDict[ballList[0]];
            foreach (var existingBallGO in ballList)
            {
                ballPosDict[existingBallGO] += theFirstBalAndSizeBallDiff;
                if (ballPosDict[existingBallGO] >= 1)
                {
                    // Here it meets collider
                }
                // get world pos by the curve pos (t) of ballPosDict[ball]
                var rightShiftedPos = CalculateBezierPointRecur(ballPosDict[existingBallGO], cpPosList);
                var existingBallComponent = existingBallGO.GetComponent<Ball>();
                existingBallComponent.Move(rightShiftedPos, nextAppearSpeed);
            }
        }

        ballComponent.Move(controlPointList[0].position, nextAppearSpeed);
        ballList.Insert(0, ballGO);
        ballPosDict.Add(ballGO, 0);
        return ballGO;
    }

    private void Creep()
    {
        // get ball position on the curve
        foreach (var ballGO in ballList)
        {
            ballPosDict[ballGO] += creepTSpeed * Time.deltaTime;
            if (ballPosDict[ballGO] >= 1)
            {
                // Here it meets collider
            }
            // get world pos by the curve pos (t) of ballPosDict[ball]
            var rightShiftedPos = CalculateBezierPointRecur(ballPosDict[ballGO], cpPosList);
            ballGO.transform.position = rightShiftedPos;
        }
    }

    public void LastShoot(Collider ball)
    {
        State = ESnakeState.LastShoot;
        if (null != startedCoroutine)
        {
            StopCoroutine(startedCoroutine);
            startedCoroutine = null;
        }

        if (!ballPosDict.ContainsKey(ball.gameObject))
        {
            // Extremely not expected
            throw new System.Exception("Ball not found in ballPosDict");
        }
        // Remove the ball from the list
        ballList.Remove(ball.gameObject);
        ballPosDict.Remove(ball.gameObject);

        var ballComponent = ball.GetComponent<Ball>();
        ballComponent.Move(frog.transform.position, frog.GetComponent<Player>().FireSpeed);
    }

    private void FillCpPosList()
    {
        cpPosList = new Vector3[controlPointList.Length];
        for (int i = 0; i < controlPointList.Length; i++)
        {
            cpPosList[i] = controlPointList[i].position;
        }
    }

    public void Inject(Collider firedBallCl)
    {
        GameObject firedBall = firedBallCl.gameObject;

        State = ESnakeState.Inject;
        if (startedCoroutine != null)
        {
            StopCoroutine(startedCoroutine);
            startedCoroutine = null;
        }

        // Make it snaked
        var firedBallComponent = firedBall.GetComponent<Ball>();
        firedBallComponent.SetState(Ball.EBallState.Snaked);

        // find closest point on the curve
        float minDist = float.MaxValue;
        float closestT = 0;
        for (float t = 0; t <= 1; t += tStep)
        {
            Vector3 point = CalculateBezierPointRecur(t, cpPosList);
            float dist = Vector3.Distance(point, firedBall.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestT = t;
            }

            // Fix the last point ( floating-point inaccuracies )
            if (t + tStep > 1 && t < 1)
            {
                t = 1 - tStep;
            }
        }

        int injectPositionIndex = 0; // like if no ball on the curve
        // If there is a ball overlaping the closest point from left, move closest point to the right on the difference
        if (ballList.Count > 0)
        {
            float[] posList = GetCurvePostList();

            // find one the right left from the closestT
            float leftFromClosest = -1;
            for (int i = 0; i < posList.Length; i++)
            {
                if (posList[i] < closestT && posList[i] > leftFromClosest)
                {
                    leftFromClosest = posList[i];
                    injectPositionIndex = i;
                }
            }
            if (leftFromClosest != -1 && (leftFromClosest + ballSizeOnCurve / 2) > (closestT - ballSizeOnCurve / 2))
            {
                closestT = leftFromClosest + ballSizeOnCurve;
            }

            MoveNextBallRightIfOverlapRecur(closestT);
        }

        // Set the fired ball to the closest point
        ballList.Insert(injectPositionIndex, firedBall);
        ballPosDict[firedBall] = closestT;
        var closestPoint = CalculateBezierPointRecur(closestT, cpPosList);
        firedBallComponent.Move(closestPoint, injectSpeed);

        Shift(ESnakeState.Inject);
        Check();
    }

    private void MoveNextBallRightIfOverlapRecur(float curvePos)
    {
        if (ballList.Count == 0)
        {
            return;
        }

        // check if there is a overlapping ball righter than curvePos
        float[] posList = GetCurvePostList();

        int index = -1;
        for (int i = 0; i < posList.Length; i++)
        {
            if (posList[i] > curvePos && (posList[i] - ballSizeOnCurve / 2) <= (curvePos + ballSizeOnCurve / 2))
            {
                index = i;
                break;
            }
        }
        if (index == -1)
        {
            return;
        }

        // move the ball to the right
        var ballGO = ballList[index];
        ballPosDict[ballGO] = curvePos + ballSizeOnCurve;
        var rightShiftedPos = CalculateBezierPointRecur(ballPosDict[ballGO], cpPosList);
        var ballComponent = ballGO.GetComponent<Ball>();
        ballComponent.Move(rightShiftedPos, injectSpeed);

        MoveNextBallRightIfOverlapRecur(ballPosDict[ballGO]);
    }

    public void Check()
    {
        State = ESnakeState.Check;
        if (startedCoroutine != null)
        {
            StopCoroutine(startedCoroutine);
            startedCoroutine = null;
        }

        bool foundARow = false;
        Generator.EBallType colorType = Generator.EBallType.Red; // Doesn't matter
        int score = 0;
        int ballCounter = 0;
        List<int> toRemoveIndexList = new List<int>();
        for (int i = 0; i < ballList.Count; i++)
        {
            Ball ball = ballList[i].GetComponent<Ball>();
            if (i == 0)
            {
                toRemoveIndexList.Add(i);
                ballCounter = 1;
                colorType = ball.GetColor();
                score = ball.GetScore(0);
                continue;
            }

            if (ball.GetColor() == colorType)
            {
                ballCounter++;
                score = ball.GetScore(score);
            }
            else
            {
                toRemoveIndexList.Clear();
                toRemoveIndexList.Add(i);
                ballCounter = 1;
                colorType = ball.GetColor();
                score = ball.GetScore(0);

                if (foundARow)
                {
                    break; // need to shift and recurse
                }
            }

            if (ballCounter >= 3)
            {
                foundARow = true;
            }
        }

        if (foundARow)
        {
            GameController.Instance.AddScore(score);

            // remove the balls
            for (int i = 0; i < toRemoveIndexList.Count; i++)
            {
                var ballGO = ballList[toRemoveIndexList[i]];
                ballList.RemoveAt(toRemoveIndexList[i]);
                ballPosDict.Remove(ballGO);
                Destroy(ballGO);
            }

            Shift(ESnakeState.Check);
            Check();
        }


        State = ESnakeState.Creep;
    }

    private void Shift(ESnakeState bakState)
    {
        State = ESnakeState.Shift;
        if (startedCoroutine != null)
        {
            StopCoroutine(startedCoroutine);
            startedCoroutine = null;
        }

        // For each ball check if it has a ball on the left, it should move to the left to that ball
        if (ballList.Count < 1)
        {
            State = bakState;
            return;
        }

        for (int i = 0; i < ballList.Count; i++)
        {
            if (i == 0)
            {
                // Shift to 0 position
                ballPosDict[ballList[i]] = 0;
            }
            else
            {
                var prevBallPos = ballPosDict[ballList[i - 1]];
                ballPosDict[ballList[i]] = prevBallPos + ballSizeOnCurve;
            }

            var shiftedPos = CalculateBezierPointRecur(ballPosDict[ballList[i]], cpPosList);
            var ballComponent = ballList[i].GetComponent<Ball>();
            ballComponent.Move(shiftedPos, shiftSpeed);
        }

        State = bakState;
    }

    // Helper
    private float[] GetCurvePostList()
    {
        float[] result = new float[ballList.Count];
        foreach (var ballGo in ballList)
        {
            result[ballList.IndexOf(ballGo)] = ballPosDict[ballGo];
        }

        return result;
    }

    // Bezier curve
    private void DrawBezierCurve()
    {
        lineRenderer.positionCount = 0;
        for (float t = 0; t <= 1; t += tStep)
        {
            Vector3 point = CalculateBezierPointRecur(t, cpPosList);

            lineRenderer.positionCount++;
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, point);

            // Fix the last point ( floating-point inaccuracies )
            if (t + tStep > 1 && t < 1)
            {
                t = 1 - tStep;
            }
        }

        // calculate curve length
        curveLen = 0;
        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            curveLen += Vector3.Distance(lineRenderer.GetPosition(i), lineRenderer.GetPosition(i + 1));
        }
        ballSizeOnCurve = (ballRadius * 2) / curveLen;

    }

    private Vector3 CalculateBezierPointRecur(float t, Vector3[] cpPosList)
    {
        if (cpPosList.Length == 2)
        {
            return Vector3.Lerp(cpPosList[0], cpPosList[1], t);
        }

        Vector3[] tempControlPoints = new Vector3[cpPosList.Length - 1];
        for (int i = 0; i < tempControlPoints.Length; i++)
        {
            tempControlPoints[i] = Vector3.Lerp(cpPosList[i], cpPosList[i + 1], t);
        }

        return CalculateBezierPointRecur(t, tempControlPoints);
    }

    private Vector3 CalculateBezierPointBiCo(float t, Vector3[] cpPosList)
    {
        int n = cpPosList.Length - 1; // Degree of the curve
        Vector3 point = Vector3.zero;
        for (int i = 0; i <= n; i++)
        {
            point += BinomialCoefficient(n, i) * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i) * cpPosList[i];
        }
        return point;
    }

    private int BinomialCoefficient(int n, int k)
    {
        if (k > n) return 0;
        if (k == 0 || k == n) return 1;

        int result = 1;
        for (int i = 1; i <= k; i++)
        {
            result = result * (n - i + 1) / i;
        }
        return result;
    }

    /**
     * @deprecated
     * Now generate 3d Mesh collider for the curve
     */
    [System.Obsolete]
    private void SetEdgeCollider()
    {
        EdgeCollider2D edgeCollider = curve.GetComponent<EdgeCollider2D>();
        Vector2[] pointList = new Vector2[lineRenderer.positionCount];
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {
            pointList[i] = lineRenderer.GetPosition(i);
        }
        edgeCollider.points = pointList;
    }

}

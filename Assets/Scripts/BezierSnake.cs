using System;
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
    private Canvas canvas;

    [SerializeField]
    private EarnScoreSpawner earnScoreSpawner;

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
    private float shiftOnCurStep = 0.06f;


    private List<GameObject> ballList = new List<GameObject>();
    private Dictionary<GameObject, float> ballPosDict = new Dictionary<GameObject, float>();
    private float ballRadius;
    private float curveLen;
    private float ballSizeOnCurve;

    private Coroutine addNextCoroutine;
    private Coroutine injectCoroutine;

    private void Awake()
    {
        lineRenderer = curve.GetComponent<LineRenderer>();
        ballRadius = ballPrefab.transform.localScale.x / 2;
        lastPosition.OnLastPositionTouched += LastShoot;
        Curve curveComponent = curve.GetComponent<Curve>();

        curveComponent.OnMeetFiredBall += BallMeetsSnakeCollider;
        curveComponent.OnRayHitsTheCollider += Inject;
    }

    private void Start()
    {
        // ABSTRACTION
        FillCpPosList();

        curve.SetActive(true);
        // ABSTRACTION
        DrawBezierCurve();
        GameController.Instance.Resume(); // Start the game running
    }

    private void Update()
    {
        // only in editor mode
        if (Application.isEditor)
        {
            FillCpPosList();
            DrawBezierCurve();
        }

        if (!GameController.Instance.IsRun)
        {
            return;
        }

        if (State == ESnakeState.Creep && addNextCoroutine == null)
        {
            addNextCoroutine = StartCoroutine(AddNextBallRoutine());
        }

        if (State == ESnakeState.Creep)
        {
            // ABSTRACTION
            Creep();
        }

        switch (State)
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
        addNextCoroutine = null;
    }

    private GameObject AddNextBall()
    {
        GameObject ballGO;

        if (
            ballList.Count > 1
            && ballList[0].GetComponent<Ball>().GetColor() == ballList[1].GetComponent<Ball>().GetColor()
            )
        {
            ballGO = Generator.GetNextAtPosition(nextBallStartPlaceholder.position, ballList[0].GetComponent<Ball>().GetColor());
        }
        else
        {
            ballGO = Generator.GetNextAtPosition(nextBallStartPlaceholder.position);
        }

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
        if (null != addNextCoroutine)
        {
            StopCoroutine(addNextCoroutine);
            addNextCoroutine = null;
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

    public void BallMeetsSnakeCollider(Collider firedBallCl)
    {
        GameObject firedBall = firedBallCl.gameObject;
        // Make it snaked
        var firedBallComponent = firedBall.GetComponent<Ball>();
        firedBallComponent.SetState(Ball.EBallState.Snaked);
    }

    public void Inject(Vector3 hitPoint, GameObject firedBall, Action<Vector3> performFire)
    {
        injectCoroutine = StartCoroutine(InjectRoutine(hitPoint, firedBall, performFire));
    }

    private IEnumerator InjectRoutine(Vector3 hitPoint, GameObject firedBall, Action<Vector3> performFire)
    {
        State = ESnakeState.Inject;
        if (addNextCoroutine != null)
        {
            StopCoroutine(addNextCoroutine);
            addNextCoroutine = null;
        }

        // find closest point on the curve
        float minDist = float.MaxValue;
        float closestT = 0;
        for (float t = 0; t <= 1; t += tStep)
        {
            Vector3 point = CalculateBezierPointRecur(t, cpPosList);
            float dist = Vector3.Distance(point, hitPoint);
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
                    injectPositionIndex = i + 1;
                    // List<T>.Insert put element to position and move original (if there is) to the next
                    // here we nned to move the next one, not the prev
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

        // Send theball to the closest point
        performFire(closestPoint);

        // ABSTRACTION
        yield return StartCoroutine(WaitBallsAreMoving());

        yield return StartCoroutine(Shift(ESnakeState.Inject));
        yield return StartCoroutine(Check());

        injectCoroutine = null;
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

        // ABSTRACTION
        MoveNextBallRightIfOverlapRecur(ballPosDict[ballGO]);
    }

    public IEnumerator Check()
    {
        State = ESnakeState.Check;
        if (addNextCoroutine != null)
        {
            StopCoroutine(addNextCoroutine);
            addNextCoroutine = null;
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

            // POLYMORPHISM
            if (ball.GetColor() == colorType)
            {
                ballCounter++;
                toRemoveIndexList.Add(i);
                // POLYMORPHISM
                score = ball.GetScore(score);
            }
            else
            {
                if (foundARow)
                {
                    break; // need to shift and recurse
                }

                toRemoveIndexList.Clear();
                toRemoveIndexList.Add(i);
                ballCounter = 1;
                colorType = ball.GetColor();
                score = ball.GetScore(0);
            }

            if (ballCounter >= 3)
            {
                foundARow = true;
            }
        }

        if (foundARow)
        {
            GameController.Instance.AddScore(score);

            // remove the balls, RemoveAt moves the next elements to the left so we need to remove from the end
            for (int i = toRemoveIndexList.Count - 1; i >= 0; i--)
            {
                var ballGO = ballList[toRemoveIndexList[i]];
                ballList.RemoveAt(toRemoveIndexList[i]);
                ballPosDict.Remove(ballGO);

                // Spawn earn score hint
                if (i == toRemoveIndexList.Count / 2)
                {
                    earnScoreSpawner.SpawnScore(ballGO.transform.position, score);
                }

                Destroy(ballGO);
            }

            // ABSTRACTION
            yield return StartCoroutine(Shift(ESnakeState.Check));
            yield return StartCoroutine(Check());
        }


        State = ESnakeState.Creep;
    }

    private IEnumerator Shift(ESnakeState bakState)
    {
        State = ESnakeState.Shift;
        if (addNextCoroutine != null)
        {
            StopCoroutine(addNextCoroutine);
            addNextCoroutine = null;
        }

        // For each ball check if it has a ball on the left, it should move to the left to that ball
        if (ballList.Count < 1)
        {
            State = bakState;
            // immediately break the coroutine
            yield break;
        }

        for (int i = 0; i < ballList.Count; i++)
        {
            float currCurvePos = ballPosDict[ballList[i]];
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

            List<Vector3> moveThruCurvePointList = new List<Vector3>();
            if (System.Math.Abs(ballPosDict[ballList[i]] - currCurvePos) <= shiftOnCurStep)
            {
                ballComponent.Move(shiftedPos, shiftSpeed);
            }
            else
            {
                // Make a list of points to move the ball thru by the cyrve from currCurvePos to shiftedPos
                float diff = System.Math.Abs(ballPosDict[ballList[i]] - currCurvePos);
                int stepCount = (int)System.Math.Floor(diff / shiftOnCurStep);
                float incrementor = currCurvePos <= ballPosDict[ballList[i]] ? shiftOnCurStep : -shiftOnCurStep;
                float pointOnCurr = currCurvePos;
                for (int j = 0; j < stepCount; j++)
                {
                    pointOnCurr = currCurvePos + i * incrementor;
                    moveThruCurvePointList.Add(CalculateBezierPointRecur(pointOnCurr, cpPosList));
                }
                if (pointOnCurr != ballPosDict[ballList[i]])
                {
                    moveThruCurvePointList.Add(CalculateBezierPointRecur(ballPosDict[ballList[i]], cpPosList));
                }
                ballComponent.MoveByPath(moveThruCurvePointList, shiftSpeed);
            }


        }

        // ABSTRACTION
        yield return StartCoroutine(WaitBallsAreMoving());

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

    private IEnumerator WaitBallsAreMoving()
    {
        while (null != ballList.Find(ball => ball.GetComponent<Ball>().IsMoving))
        {
            yield return new WaitForSeconds(0.2f);
        }
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

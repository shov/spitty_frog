using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Line : MonoBehaviour
{
    [SerializeField]
    private GameObject ballPrefab;

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

    IEnumerator AddBallRoutine()
    {
        isBallAdding = true;

        FillNextBall();

        yield return new WaitForSeconds(0.5f);
        isBallAdding = false;
    }
    protected void FillNextBall()
    {
        // at first move all balls to the next position
        for (int i = ballList.Count - 1; i > 0; i--)
        {
            var destination = ballPlaceholderList[i - 1].transform.position;
            ballList[i].GetComponent<Ball>().Move(destination);
        }

        var type = Generator.GetNext();
        // TODO : add ball with type
        var ball = Instantiate(ballPrefab, startPlaceHolder.transform.position, Quaternion.identity);
        ballList.Add(ball);
    }
}

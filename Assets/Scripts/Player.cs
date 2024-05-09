using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private GameObject fireStartPlaceholder;

    [SerializeField]
    private Line line;

    [SerializeField]
    private float fireSpeed = 35f;

    private bool isFiring = false;
    private bool haveBall = false;

    private GameObject ball;

    private Vector3 playerScreenPos;
    void Start()
    {
        playerScreenPos = mainCam.WorldToScreenPoint(transform.position);
    }

    void Update()
    {
        var mouseScreenPose = Input.mousePosition;
        // rotate player to mouse position by player screen position and mouse screen position
        float angle = Mathf.Atan2(mouseScreenPose.y - playerScreenPos.y, mouseScreenPose.x - playerScreenPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(-angle, 90f, -90f);

        // get ball
        if(!haveBall)
        {
            GetBall();
        }

        if(haveBall && !isFiring)
        {
            ball.transform.position = fireStartPlaceholder.transform.position;
            ball.SetActive(true);
        }

        // fire
        var fireTriggered = Input.GetMouseButtonDown(0) || Input.GetKey("Space");
        if (fireTriggered && !isFiring && haveBall)
        {
            StartCoroutine(FireRoutine());
        }
    }

    protected IEnumerator FireRoutine()
    {
        isFiring = true;

        Fire();
        yield return new WaitForSeconds(0.5f);

        isFiring = false;
    }

    protected void Fire()
    {
        var directionedPos = transform.position + transform.forward * 100f;
        // Remove parent
        ball.transform.SetParent(null);
        // Fire
        ball.GetComponent<Ball>().Move(directionedPos, fireSpeed);
        // Have no ball anymore
        haveBall = false;
    }

    protected void GetBall()
    {
        var ballToFireGO = line.GetNextBall(fireStartPlaceholder.transform.position);
        if (ballToFireGO == null)
        {
            return;
        }

        var ballComponent = ballToFireGO.GetComponent<Ball>();
        ballComponent.SetState(Ball.EBallState.Fired);
        
        // Just to reset any movement
        ballComponent.Move(fireStartPlaceholder.transform.position, fireSpeed);

        // Set parent to start placeholder
        ballToFireGO.transform.SetParent(fireStartPlaceholder.transform);

        // Set rotation to identity
        ballToFireGO.transform.rotation = Quaternion.identity;

        ball = ballToFireGO;
        ball.SetActive(false);
        haveBall = true;
    }

    // If ball trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if other has component Ball
        var ballComponent = other.GetComponent<Ball>();
        if (ballComponent == null)
        {
            return;
        }

        // Check if ball is snaked
        if (ballComponent.State == Ball.EBallState.Snaked)
        {
            // Gameover
            GameController.Instance.GameOver();
            Destroy(gameObject);
            Destroy(other);
        }
    }
}

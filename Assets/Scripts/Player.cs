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

    public float FireSpeed
    {
        get => fireSpeed;
        private set => fireSpeed = value;
    }

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
        if(!GameController.Instance.IsRun)
        {
            return;
        }

        var mouseScreenPose = Input.mousePosition;
        // rotate player to mouse position by player screen position and mouse screen position
        float angle = Mathf.Atan2(mouseScreenPose.y - playerScreenPos.y, mouseScreenPose.x - playerScreenPos.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(-angle, 90f, -90f);

        // get ball
        if(!haveBall)
        {
            GetBall();
        }

        if(haveBall && !isFiring && GameController.Instance.IsFireAllowed)
        {
            ball.transform.position = fireStartPlaceholder.transform.position;
            ball.SetActive(true);
        }

        // fire LMB or Space
        var fireTriggered = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space);
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
            //Destroy(gameObject); looks awful
            Destroy(other.gameObject);
            Destroy(ball); // in mouth
            StartCoroutine(LerpFrogColorToRed());
            GameController.Instance.GameOver();
        }
    }

    private IEnumerator LerpFrogColorToRed()
    {
        // Gt all objects by tag to the list, then get all their materials
        GameObject[] greenMat = GameObject.FindGameObjectsWithTag("FrogGreenBodyPart");
        Material[] materials = new Material[greenMat.Length];
        for (int i = 0; i < greenMat.Length; i++)
        {
            materials[i] = greenMat[i].GetComponent<Renderer>().material;
        }
        Color[] colors = new Color[materials.Length];

        var startColor = colors[0];
        var endColor = Color.red;
        var duration = 1f;
        var t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            for (int i = 0; i < materials.Length; i++)
            {
                colors[i] = Color.Lerp(startColor, endColor, t);
                materials[i].color = colors[i];
            }
            yield return null;
        }
    }   
}

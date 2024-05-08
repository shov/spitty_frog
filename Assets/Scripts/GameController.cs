using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public bool IsRun { get; private set; }
    public bool IsGameOver { get; private set; } = false;

    void Start()
    {
        
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            SceneManager.LoadScene(0);
        }

        // any key to load menu scene if is game over
        if (IsGameOver && Input.anyKeyDown)
        {
            SceneManager.LoadScene(0);
        }
    }
}

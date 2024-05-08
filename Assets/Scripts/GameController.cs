using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }

    [SerializeField]
    private GameObject gameOverSplash;
    public bool IsRun { get; private set; } = false;
    public bool IsGameOver { get; private set; } = false;

    void Start()
    {
        Instance = this;
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

    public void GameOver()
    {
        IsRun = false;
        IsGameOver = true;
        gameOverSplash.SetActive(true);
        gameOverSplash.GetComponent<Animator>().SetTrigger("Appear");
    }

    public void Pause()
    {
        IsRun = false;
    }

    public void Resume()
    {
        IsRun = true;
    }
}

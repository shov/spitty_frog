using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class mainMenuUIController : MonoBehaviour
{
    [SerializeField]
    private GameObject scoreGroup;
    [SerializeField]
    private TextMeshProUGUI topScoreText;

    void Start()
    {
        var data = Storage.Load();
        if (data.topScore > 0)
        {
            scoreGroup.SetActive(true);
            topScoreText.text = $"{data.topScore}";
        }
    }

    public void StartBtClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
    public void ExitBtClick()
    {
        if (Application.isEditor)
        {
            UnityEditor.EditorApplication.isPlaying = false;
            return;
        }
        Application.Quit();
    }
}

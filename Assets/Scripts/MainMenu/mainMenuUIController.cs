using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

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
#if UNITY_EDITOR
        if (Application.isEditor)
        {
            EditorApplication.isPlaying = false;
            return;
        }
#endif
        Application.Quit();
    }
}

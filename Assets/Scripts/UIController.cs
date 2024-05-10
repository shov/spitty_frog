using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[DefaultExecutionOrder(1000)]
public class UIController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI scoreText;

    void Start()
    {
        scoreText.text = "0";
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

}

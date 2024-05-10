using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarnScoreSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject earnScorePrefab;

    [SerializeField]
    private Camera mainCam;

    [SerializeField]
    private Camera uiCam;

    [SerializeField]
    private Canvas canvas;

    public void SpawnScore(Vector3 worldPos, int score)
    {
        GameObject earnScoreObj = Instantiate(earnScorePrefab, canvas.transform);
        EarnScore earnScore = earnScoreObj.GetComponent<EarnScore>();
        earnScore.SetScore(score);
        earnScore.SpawnAtPoint(worldPos, mainCam, uiCam, canvas);
    }
}

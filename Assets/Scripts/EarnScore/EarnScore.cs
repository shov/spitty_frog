using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarnScore : MonoBehaviour
{
    [SerializeField]
    private float speed = 100f;

    [SerializeField]
    private float destroyHeight = 1000f;

    private RectTransform rectTransform;


    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }


    void Update()
    {
        rectTransform.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        if (rectTransform.anchoredPosition.y > destroyHeight)
        {
            Destroy(gameObject);
        }

    }

    public void SetScore(int score)
    {
        TMPro.TextMeshProUGUI textObj = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        textObj.text = score.ToString();
    }

    public void SpawnAtPoint(Vector3 worldPos, Camera mainCam, Camera uiCam, Canvas canvas)
    {
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);
        Vector2 canvasPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPos, uiCam, out canvasPos);
        rectTransform.anchoredPosition = canvasPos;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public enum EBallType
    {
        Red,
        Green,
        Blue
    }

    public static Generator Instance { get; private set; }

    [SerializeField]
    private GameObject[] ballPrefabList = new GameObject[3];

    private void Start()
    {
        Instance = this;
    }

    

    public static EBallType GetNextType()
    {
        return (EBallType)Random.Range(0, 3);
    }

    public static GameObject GetNext()
    {
        var type = GetNextType();
        return Instance.ballPrefabList[(int)type];
    }

    public static GameObject GetNextAtPosition(Vector3 pos)
    {
        var ball = GetNext();
        return Instantiate(ball, pos, Quaternion.identity);
    }
}

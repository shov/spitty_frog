using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour
{
    [SerializeField]
    private GameObject ballPrefab;

    public enum EBallType
    {
        Red,
        Green,
        Blue
    }

    public static EBallType GetNext()
    {
        return (EBallType)Random.Range(0, 3);
    }
}

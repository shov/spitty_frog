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

    private void Awake()
    {
        Instance = this;
    }



    public static EBallType GetNextType()
    {
        return (EBallType)Random.Range(0, 3);
    }
    public static EBallType GetNextType(EBallType exclude)
    {
        EBallType generated;
        do
        {
            generated = GetNextType();
        } while (generated == exclude);
        return generated;
    }

    public static GameObject GetNext()
    {
        var type = GetNextType();
        return Instance.ballPrefabList[(int)type];
    }
    public static GameObject GetNext(EBallType exclude)
    {
        var type = GetNextType(exclude);
        return Instance.ballPrefabList[(int)type];
    }

    public static GameObject GetNextAtPosition(Vector3 pos)
    {
        var ball = GetNext();
        return Instantiate(ball, pos, Quaternion.identity);
    }
    public static GameObject GetNextAtPosition(Vector3 pos, EBallType exclude)
    {
        var ball = GetNext(exclude);
        return Instantiate(ball, pos, Quaternion.identity);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueBall : Ball
{
    private const int baseScore = 10;
    public override Generator.EBallType GetColor()
    {
        return Generator.EBallType.Blue;
    }

    public override int GetScore(int stackedScore)
    {
        if(stackedScore == 0)
        {
            return baseScore;
        }
        return stackedScore + baseScore;
    }
}

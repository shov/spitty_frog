using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBall : Ball
{
    private const int baseScore = 20;
    public override Generator.EBallType GetColor()
    {
        return Generator.EBallType.Red;
    }

    public override int GetScore(int stackedScore)
    {
        if (stackedScore == 0)
        {
            return baseScore;
        }
        return stackedScore + baseScore / (stackedScore / baseScore);
    }
}

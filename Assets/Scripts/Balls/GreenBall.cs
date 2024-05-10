using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenBall : Ball
{
    
    private const int baseScore = 2;
    public override Generator.EBallType GetColor()
    {
        return Generator.EBallType.Green;
    }

    public override int GetScore(int stackedScore)
    {
        if (stackedScore == 0)
        {
            return baseScore;
        }
        return stackedScore * baseScore;
    }
}

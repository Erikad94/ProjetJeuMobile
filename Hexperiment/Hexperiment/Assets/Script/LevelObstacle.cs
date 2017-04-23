using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObstacle : Level {
    
    public GridScript.PieceType[] obstacleTypes;

    private int numObstacleLeft;

    // Use this for initialization
    void Start () {
        type = LevelType.OBSTACLE;
        for(int i = 0; i < obstacleTypes.Length; i++)
        {
            numObstacleLeft += grid.GetPiecesOfType(obstacleTypes[i]).Count;
        }

        hud.SetLevelType(type);
        hud.SetScore(currentScore);
        hud.SetTarget(numObstacleLeft);
        hud.SetRemaining(numMoves);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnMove()
    {
        base.OnMove();
        if (numMoves - movesUsed == 0 && numObstacleLeft > 0)
        {
            GameLose();
        }
    }

    public override void OnPieceCleared(HexPiece piece)
    {
        base.OnPieceCleared(piece);

        for(int i = 0; i < obstacleTypes.Length;i++)
        {
            if(obstacleTypes[i] == piece.Type)
            {
                numObstacleLeft--;
                hud.SetTarget(numObstacleLeft);

                if (numObstacleLeft == 0)
                {
                    //currentScore += (numMoves - movesUsed) * 1000;
                   // hud.SetScore(currentScore);
                    GameWin();
                }
            }
        }
    }
}

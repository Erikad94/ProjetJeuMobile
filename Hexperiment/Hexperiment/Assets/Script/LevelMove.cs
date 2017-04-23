using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelMove : Level {

    public int targetScore;

	// Use this for initialization
	void Start () {
        type = LevelType.MOVES;

        hud.SetLevelType(type);
        hud.SetScore(currentScore);
        hud.SetTarget(targetScore);
        hud.SetRemaining(numMoves);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void OnMove()
    {
        base.OnMove();

        if (numMoves - movesUsed == 0)
        {
            if(currentScore >= targetScore)
            {
                GameWin();
            }
            else
            {
                GameLose();
            }
        }
    }
}

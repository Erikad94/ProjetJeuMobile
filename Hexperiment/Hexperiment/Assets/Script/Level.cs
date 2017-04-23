using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {


    public enum LevelType
    {
        TIMER,
        OBSTACLE,
        MOVES,
        COUNT
    }

    public GridScript grid;
    public HUD hud;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    public int numMoves;
    public int movesUsed = 0;

    protected LevelType type;
    protected int currentScore;
    protected bool didWin;

    public LevelType Type
    {
        get { return type; }
    }

    // Use this for initialization
    void Start () {
        hud.SetScore(0);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public virtual void GameWin()
    {
        grid.GameOver(true);
        didWin = true;
        StartCoroutine(WaitForGridFill());
    }

    public virtual void GameLose()
    {
        grid.GameOver(false);
        didWin = false;
        StartCoroutine(WaitForGridFill());
    }

    public virtual void OnMove()
    {
        movesUsed++;

        hud.SetRemaining(numMoves - movesUsed);
    }

    public virtual void OnPieceCleared(HexPiece piece)
    {
        currentScore += piece.score;
        hud.SetScore(currentScore);
    }

    protected virtual IEnumerator WaitForGridFill()
    {
        while (grid.gameState != GridScript.GameState.DONE_CLEARING)
        {
            yield return 0;
        }

        if( didWin)
        {
            hud.OnGameWin(currentScore);
        }
        else
        {
            hud.OnGameLose(currentScore);
        }
    }
    public bool HasRemainingMove()
    {
        return numMoves - movesUsed > 0;
    }

    public bool ConsumeMove()
    {
        if (HasRemainingMove())
        {
            movesUsed++;
            UpdateRemainingMove();
            return true;
        }
        return false;
    }

    public void UpdateRemainingMove()
    {
        hud.SetRemaining(numMoves - movesUsed);
    }
}

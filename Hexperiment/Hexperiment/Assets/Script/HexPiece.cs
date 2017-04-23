using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexPiece : MonoBehaviour {

    private int x;
    private int y;
    private GridScript.PieceType type;
    private GridScript grid;
    private MovablePiece movableComponent;
    private ColorPiece colorComponent;
    private ClearablePiece clearComponent;

    public int score;
    public GridScript.PieceType nextType;

    public int X
    {
        get { return x; }
        set
        {
            if(IsMovable())
            {
                x = value;
            }
        }
    }

    public int Y
    {
        get { return y; }
        set
        {
            if (IsMovable())
            {
                y = value;
            }
        }
    }
    public GridScript.PieceType NextType
    {
        get { return nextType; }
        set
        {
            nextType = value;
        }
    }

    public GridScript.PieceType Type
    {
        get { return type; }
    }
    public GridScript Grid
    {
        get { return grid; }
    }
    public MovablePiece MovableComponent
    {
        get { return movableComponent; }
    }
    public ColorPiece ColorComponent
    {
        get { return colorComponent; }
    }
    public ClearablePiece ClearComponent
    {
        get { return clearComponent; }
    }

    void Awake()
    {
        colorComponent = GetComponent<ColorPiece>();
        movableComponent = GetComponent<MovablePiece>();
        clearComponent = GetComponent<ClearablePiece>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(int _x, int _y,GridScript _grid, GridScript.PieceType _type)
    {
        x = _x;
        y = _y;
        grid = _grid;
        type = _type;
    }

    public bool IsMovable()
    {
        return movableComponent != null;
    }
    public bool IsColored()
    {
        return colorComponent != null;
    }
    public bool IsClearable()
    {
        return clearComponent != null;
    }

    public bool CanClear()
    {
        return IsClearable() && !clearComponent.IsBeingCleared && !clearComponent.IsCleared;
    }
}

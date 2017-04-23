﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearDiagonalLeftPiece : ClearablePiece
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void Clear()
    {
        base.Clear();

        piece.Grid.ClearDiagonalLeft(piece.X, piece.Y);
    }
}

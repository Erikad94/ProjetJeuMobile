using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearablePiece : MonoBehaviour {

    public AnimationClip clearAnimation;

    private bool isBeingCleared = false;
    private bool isCleared = false;

    protected HexPiece piece;

    public bool IsBeingCleared
    {
        get { return isBeingCleared; }
    }
    public bool IsCleared
    {
        get { return isCleared; }
    }

    void Awake()
    {
        piece = GetComponent<HexPiece>();
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    virtual public void Clear()
    {
        piece.Grid.level.OnPieceCleared(piece);
        isBeingCleared = true;
        StartCoroutine(ClearCoroutine());
    }

    private IEnumerator ClearCoroutine()
    {
        Animator animator = GetComponent<Animator>();

        if(animator)
        {
            animator.Play(clearAnimation.name);
            //TODO: add sound
            yield return new WaitForSeconds(clearAnimation.length);
        }

        isBeingCleared = false;
        isCleared = true;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovablePiece : MonoBehaviour
{

    private HexPiece piece;
    private IEnumerator moveCoroutine;
    private IEnumerator rotateCoroutine;
    private bool isMoving = false;

    public bool IsMoving
    {
        get { return isMoving; }
    }

    void Awake()
    {
        piece = GetComponent<HexPiece>();
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Move(int newX, int newY, float time)
    {
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
        }

        moveCoroutine = MoveCoroutine(newX, newY, time);
        StartCoroutine(moveCoroutine);

    }
    public void Rotate(float degree, int newX, int newY, Vector3 center, float time)
    {
        isMoving = true;
        if (rotateCoroutine != null)
        {
            StopCoroutine(rotateCoroutine);
        }

        rotateCoroutine = RotateCoroutine(degree, newX, newY, center, time);
        StartCoroutine(rotateCoroutine);

    }

    private IEnumerator MoveCoroutine(int newX, int newY, float time)
    {
        piece.X = newX;
        piece.Y = newY;

        Vector3 startPos = transform.position;
        Vector3 endPos = piece.Grid.GetWorldPosition(newX, newY);

        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Lerp(startPos, endPos, t / time);
            yield return 0;
        }

        piece.transform.position = piece.Grid.GetWorldPosition(newX, newY);
    }
    private IEnumerator RotateCoroutine(float degree, int newX, int newY, Vector3 center, float time)
    {
        piece.X = newX;
        piece.Y = newY;

        Vector3 endPos = piece.Grid.GetWorldPosition(newX, newY);
        Vector3 startPos = transform.position - center;
        Vector3 endLerp = endPos - center;

        for (float t = 0; t <= 1 * time; t += Time.deltaTime)
        {
            piece.transform.position = Vector3.Slerp(startPos, endLerp, t / time) + center;
            //piece.transform.rotation = Quaternion.Slerp(piece.transform.rotation, Quaternion.AngleAxis(degree, Vector3.forward), (t / time)* 0.1f);
            yield return 0;
        }

        piece.transform.position = piece.Grid.GetWorldPosition(newX, newY);
        //piece.transform.rotation = Quaternion.identity;

        isMoving = false;
    }
}

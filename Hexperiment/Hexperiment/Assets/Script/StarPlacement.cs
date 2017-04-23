using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarPlacement : MonoBehaviour {

    private Level level;

    public float zeroPercentPosition;
    public float hundredPercentPosition;
    public float hundredPercentPositionOrigin;

    // Use this for initialization
    void Start () {
        zeroPercentPosition = GameObject.Find("starEmpty1").transform.position.x;
        hundredPercentPosition = GameObject.Find("starEmpty3").transform.position.x;
        hundredPercentPositionOrigin = hundredPercentPosition - zeroPercentPosition;

        level = GameObject.Find("Level").GetComponent<Level>();

        changeStar(level.score1Star, hundredPercentPositionOrigin, zeroPercentPosition, 1);
        changeStar(level.score2Star, hundredPercentPositionOrigin, zeroPercentPosition, 2);

        for ( int i = 1; i <=3; i++)
        {
            GameObject.Find("starFilled" + i).SetActive(false);
        }

    }

    void changeStar(int scoreStar, float hundredPercentPositionOrigin, float zeroPercentPosition, int i)
    {
        float starPos = (scoreStar * hundredPercentPositionOrigin / level.score3Star) + zeroPercentPosition;
        var vecStarPos = new Vector3(starPos, GameObject.Find("starEmpty" + i).transform.position.y, GameObject.Find("starEmpty" + i).transform.position.z);
        GameObject.Find("starEmpty" + i).transform.position = vecStarPos;
        GameObject.Find("starFilled" + i).transform.position = vecStarPos;
        vecStarPos = new Vector3(starPos, GameObject.Find("starbarLine" + i).transform.position.y, GameObject.Find("starbarLine" + i).transform.position.z);
        GameObject.Find("starbarLine" + i).transform.position = vecStarPos;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}

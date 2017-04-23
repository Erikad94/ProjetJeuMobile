using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUD : MonoBehaviour {

    public Level level;
    public StarPlacement starPlacement;
    public GameOver gameOver;

    public UnityEngine.UI.Text remainingText;
    public UnityEngine.UI.Text remainingSubtext;
    public UnityEngine.UI.Text targetText;
    public UnityEngine.UI.Text targetSubtext;
    public UnityEngine.UI.Text scoreText;
    public GameObject[] stars;

    public float startPosition;

    private int starIdx;

    // Use this for initialization
    void Start () {
        startPosition = GameObject.Find("StarBarColor").transform.position.x;
        Debug.Log(startPosition);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();

        int visibleStar = 5;

        if (score >= level.score1Star && score < level.score2Star)
        {
            visibleStar = 0;
        }
        else if (score >= level.score2Star && score < level.score3Star)
        {
            visibleStar = 1;
        }
        else if (score >= level.score3Star)
        {
            visibleStar = 2;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            if (i == visibleStar)
            {
                stars[i].SetActive(true);
            }
        }

        starIdx = visibleStar;

        if (score < level.score3Star)
        {
            float previousBarWidth = GameObject.Find("StarBarColor").GetComponent<RectTransform>().sizeDelta.x;
            float barWidth = (score * (GameObject.Find("StarBarInside").GetComponent<RectTransform>().sizeDelta.x-20) / level.score3Star);

            var vecBarSize = new Vector2(barWidth, GameObject.Find("StarBarColor").GetComponent<RectTransform>().sizeDelta.y);
            GameObject.Find("StarBarColor").GetComponent<RectTransform>().sizeDelta = vecBarSize;
        }
    }

    public void SetTarget(int target)
    {
        targetText.text = target.ToString();
    }

    public void SetRemaining(int remaining)
    {
        remainingText.text = remaining.ToString();
    }

    public void SetRemaining(string remaining)
    {
        remainingText.text = remaining;
    }

    public void SetLevelType(Level.LevelType type)
    {
        if(type == Level.LevelType.MOVES)
        {
            remainingSubtext.text = "moves remaining";
            targetSubtext.text = "target score";
        }
        else if (type == Level.LevelType.OBSTACLE)
        {
            remainingSubtext.text = "moves remaining";
            targetSubtext.text = "obstacles remaining";
        }
        else if (type == Level.LevelType.TIMER)
        {
            remainingSubtext.text = "time remaining";
            targetSubtext.text = "target score";
        }
        else
        {
            Debug.LogError("A new level type must be consider");
        }
    }

    public void OnGameWin(int score)
    {
        gameOver.ShowWin(score, starIdx);
        if (score > PlayerPrefs.GetInt(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, 0))
        {
            PlayerPrefs.SetInt(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, score);
        }
        if (starIdx > PlayerPrefs.GetInt(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "stars", 0))
        {
            PlayerPrefs.SetInt(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name + "stars", starIdx);
        }
    }

    public void OnGameLose(int score)
    {
        PlayerPrefs.SetInt("life", PlayerPrefs.GetInt("life", 0) - 1);
        gameOver.ShowLose();
    }
}

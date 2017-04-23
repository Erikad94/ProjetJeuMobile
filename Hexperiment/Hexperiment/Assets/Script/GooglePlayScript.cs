using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using UnityEngine.SocialPlatforms;

public class GooglePlayScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GooglePlayGames.PlayGamesPlatform.Activate();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SignIn()
    {
    // authenticate user:
    Social.localUser.Authenticate((bool success) => {
        GameObject.Find("Google").SetActive(false);
        PlayGamesPlatform.Instance.ReportProgress("CgkI0uuioIkOEAIQBw", 100.0f, (bool successAchiev) => {
            PlayGamesPlatform.Instance.ShowAchievementsUI();
        });
    });
    }
}

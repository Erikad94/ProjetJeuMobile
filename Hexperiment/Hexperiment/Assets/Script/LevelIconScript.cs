using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelIconScript : MonoBehaviour
{
    public string PreviousSceneName;
    public string SceneName;
    [SerializeField] GameObject CapsuleLocked;
    [SerializeField] GameObject CapsuleAvailable;
    [SerializeField] GameObject CapsuleFinished;


    // Use this for initialization
    void Start ()
    {
        //Load state
        if (PlayerPrefs.GetInt(PreviousSceneName, 0) == 0)
        {
            CapsuleLocked.SetActive(true);
            CapsuleAvailable.SetActive(false);
            CapsuleFinished.SetActive(false);
        }
        else if (PlayerPrefs.GetInt(SceneName, 0) == 0)
        {
            CapsuleLocked.SetActive(false);
            CapsuleAvailable.SetActive(true);
            CapsuleFinished.SetActive(false);
        }
        else // level is done
        {
            CapsuleLocked.SetActive(false);
            CapsuleAvailable.SetActive(false);
            CapsuleFinished.SetActive(true);

            int nbStars = PlayerPrefs.GetInt(SceneName+"stars", 0);

            for (int starIdx = 0; starIdx <= 3; starIdx++)
            {
                Transform star = CapsuleFinished.gameObject.transform.Find("stars" + starIdx);

                if (starIdx == nbStars)
                {
                    star.gameObject.SetActive(true);
                }
                else
                {
                    star.gameObject.SetActive(false);
                }
            }
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    public void Clicked()
    {
        if (CapsuleAvailable.activeSelf || CapsuleFinished.activeSelf)
        {
            SceneManager.LoadScene(SceneName);
        }
    }
}

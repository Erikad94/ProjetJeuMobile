using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour {

    //public GameObject audioSource;
    //
    //[System.Serializable]
    //public struct ButtonPlayerPrefs
    //{
    //    public GameObject gameObject;
    //    public string playerPrefKey;
    //}

    //public ButtonPlayerPrefs[] buttons;

	// Use this for initialization
	void Awake () {
        //if (PlayerPrefs.GetInt("Music") == 1)
        //{
        //    audioSource.SetActive(false);
        //}
        //
        ////PlayerPrefs.DeleteAll();//this is to reset the game :)
        //PlayerPrefs.SetInt("Level_000", 1);//this is a simple hack to allowed first level to be available instead of locked
        //PlayerPrefs.SetInt("life", 5);//this is a simple hack to allowed first level to be available instead of locked
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector2 pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        //    RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(pos), Vector2.zero);
        //    // RaycastHit2D can be either true or null, but has an implicit conversion to bool, so we can use it like this
        //    if (hitInfo)
        //    {
        //        //Debug.Log(hitInfo.transform.gameObject.name);
        //        //Debug.Log(hitInfo.transform.parent.name);
        //        hitInfo.transform.GetComponent<LevelIconScript>().Clicked();
        //        // Here you can check hitInfo to see which collider has been hit, and act appropriately.
        //    }
        //}
    }        
}

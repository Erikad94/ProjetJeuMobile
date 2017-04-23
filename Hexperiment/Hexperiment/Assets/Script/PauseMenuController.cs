using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour {

    private bool menuOpen = false;
    public GameObject musicOff;
    public GameObject musicOn;
    public GameObject soundOff;
    public GameObject soundOn;
    public GameObject audioSource;
    public GameObject resetButton;
    public GameObject infoBox;
    public GameObject blackOut;
    public GameObject animatorClass;
    public GridScript gridScript;
    public bool pageType;

    // Use this for initialization
    void Start () {
        updateAll();
        if (Debug.isDebugBuild)
        {
            resetButton.SetActive(true);
        }
        else
        {
            resetButton.SetActive(false);
        }
    }

    public void updateAll()
    {
        if (PlayerPrefs.GetInt("Music") == 1)
        {
            audioSource.SetActive(false);
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }
        else
        {
            audioSource.SetActive(true);
            musicOn.SetActive(true);
            musicOff.SetActive(false);
        }
        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            soundOn.SetActive(false);
            soundOff.SetActive(true);
        }
        else
        {
            soundOn.SetActive(true);
            soundOff.SetActive(false);
        }
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Clicked() {
        if (menuOpen){
            menuOpen = false;
            CloseMenu();
        }
        else{
            menuOpen = true;
            OpenMenu();
        }
    }

    public void clickMusic() {
        if (PlayerPrefs.GetInt("Music") == 1) {
            audioSource.SetActive(true);
            musicOn.SetActive(true);
            musicOff.SetActive(false);
            PlayerPrefs.SetInt("Music", 0);
        }
        else {
            audioSource.SetActive(false);
            musicOn.SetActive(false);
            musicOff.SetActive(true);
            PlayerPrefs.SetInt("Music", 1);
        }
        
    }

    public void clickSound() {
        if (PlayerPrefs.GetInt("Sound") == 1)
        {
            soundOn.SetActive(true);
            soundOff.SetActive(false);
            PlayerPrefs.SetInt("Sound", 0);
        }
        else {
            soundOn.SetActive(false);
            soundOff.SetActive(true);
            PlayerPrefs.SetInt("Sound", 1);
        }
    }

    public void clickHelp() {
        infoBox.SetActive(true);
    }

    public void closeHelp()
    {
        infoBox.SetActive(false);
    }

    public void clickHome() {
        if(gridScript != null) {
            gridScript.setTouchActionState(GridScript.ActionState.NONE);
        }
        SceneManager.LoadScene("Main");
    }

    public void clickReset()
    {
        PlayerPrefs.DeleteAll();
        updateAll();
    }


    public void OpenMenu() {
        if (gridScript != null) {
            gridScript.setTouchActionState(GridScript.ActionState.DISABLED);
        }
        Debug.Log(animatorClass);
        Animator animator = animatorClass.GetComponent<Animator>();
        if (animator && !pageType) {
            animator.Play("MenuShow");
        }
        Debug.Log(animator);
        if (animator && pageType){
            animator.Play("MenuShowMain");
        }
    }

    public void CloseMenu() {
        if (gridScript != null) {
            gridScript.setTouchActionState(GridScript.ActionState.NONE);
        }
        Debug.Log(animatorClass);
        Animator animator = animatorClass.GetComponent<Animator>();
        if (animator && !pageType){
            animator.Play("MenuHide");
        }
        if (animator && pageType) {
            animator.Play("MenuHideMain");
        }
    }
}

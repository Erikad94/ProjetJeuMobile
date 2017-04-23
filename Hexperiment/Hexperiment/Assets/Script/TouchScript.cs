using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchScript : MonoBehaviour
{
    public float orthoZoomSpeed = 0.1f;
    public float orthoZoomMin = 2.0f;
    public float orthoZoomMax = 6.0f;
    public float moveThreshold = 5.0f;
    public float scrollFriction = 0.9f;
    public float scrollMinVelocity = 0.01f;
    public float scrollBoost = 2.5f;//for touch screen
    public Vector3 scrollVelocity;
    private bool menuOpen = false;
    private bool helpOpen = false;


    private Bounds lowerFiledBounds;
    private Bounds upperFiledBounds;
    private struct MenuAction
    {
        public enum TouchState
        {
            NONE,
            DOWN,
            MOVED,
            UP,
            CLICKED
        }
        public TouchState state;
        public Vector2 touchStart;
        public Vector2 touchLastPosition;
        public Vector2 touchCurrentPosition;
        public Vector2 touchEnded;
    }
    private MenuAction menuAction;

    [SerializeField] GameObject backgroundDebut;
    [SerializeField] GameObject backgroundFin;

    public GameObject audioSource;

    [System.Serializable]
    public struct ButtonPlayerPrefs
    {
        public GameObject gameObject;
        public string playerPrefKey;
    }

    void Awake()
    {
        if (PlayerPrefs.GetInt("Music") == 1)
        {
            audioSource.SetActive(false);
        }

        //PlayerPrefs.DeleteAll();//this is to reset the game :)
        PlayerPrefs.SetInt("Level_000", 1);//this is a simple hack to allowed first level to be available instead of locked
        PlayerPrefs.SetInt("life", 5);//this is a simple hack to allowed first level to be available instead of locked
    }

    void Start()
    {
        lowerFiledBounds = backgroundDebut.GetComponent<SpriteRenderer>().bounds;
        upperFiledBounds = backgroundFin.GetComponent<SpriteRenderer>().bounds;
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                menuAction.touchStart = touch.position;
                menuAction.touchLastPosition = touch.position;
                menuAction.touchCurrentPosition = touch.position;
                menuAction.state = MenuAction.TouchState.DOWN;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                menuAction.touchLastPosition = menuAction.touchCurrentPosition;
                menuAction.touchCurrentPosition = touch.position;
                menuAction.touchEnded = touch.position;
                if (menuAction.state == MenuAction.TouchState.MOVED)
                {
                    menuAction.state = MenuAction.TouchState.UP;
                }
                else if (menuAction.state == MenuAction.TouchState.DOWN)
                {
                    menuAction.state = MenuAction.TouchState.CLICKED;
                }
                else
                {
                    Debug.LogError("State impossible verifier le code");
                }
            }
            else if((menuAction.state == MenuAction.TouchState.DOWN || menuAction.state == MenuAction.TouchState.MOVED))
            {
                menuAction.touchLastPosition = menuAction.touchCurrentPosition;
                menuAction.touchCurrentPosition = touch.position;
                if ((menuAction.touchCurrentPosition - menuAction.touchStart).magnitude >= moveThreshold)
                {
                    menuAction.state = MenuAction.TouchState.MOVED;
                }
            }
        }
        else if (Input.GetMouseButtonDown(0))//mouseDown
        {
            menuAction.touchStart = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            menuAction.touchLastPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            menuAction.touchCurrentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            menuAction.state = MenuAction.TouchState.DOWN;
        }
        else if (Input.GetMouseButtonUp(0))//mouseUp
        {
            menuAction.touchLastPosition = menuAction.touchCurrentPosition;
            menuAction.touchCurrentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            menuAction.touchEnded = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if (menuAction.state == MenuAction.TouchState.MOVED)
            {
                menuAction.state = MenuAction.TouchState.UP;
            }
            else if (menuAction.state == MenuAction.TouchState.DOWN)
            {
                menuAction.state = MenuAction.TouchState.CLICKED;
            }
            else
            {
                Debug.LogError("State impossible verifier le code");
            }
        }
        else if ((menuAction.state == MenuAction.TouchState.DOWN || menuAction.state == MenuAction.TouchState.MOVED))
        {
            menuAction.touchLastPosition = menuAction.touchCurrentPosition;
            menuAction.touchCurrentPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            if ((menuAction.touchCurrentPosition - menuAction.touchStart).magnitude >= moveThreshold)
            {
                menuAction.state = MenuAction.TouchState.MOVED;
            }
        }
        
        //Debug.Log(menuAction.state.ToString());

        if (Input.touchCount == 2)
        {
            ZoomAction();
            menuAction.state = MenuAction.TouchState.NONE;
        }
        else if (menuAction.state != MenuAction.TouchState.DOWN)
        {
            TakeAction();
        }

        ReplaceCamera();

    }

    private void TakeAction()
    {
        if(menuAction.state == MenuAction.TouchState.CLICKED)
        {
            RaycastHit2D hitInfo = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(menuAction.touchEnded), Vector2.zero);
            // RaycastHit2D can be either true or null, but has an implicit conversion to bool, so we can use it like this
            if (hitInfo)
            {
                //Debug.Log(hitInfo.transform.gameObject.name);
                //Debug.Log(hitInfo.transform.parent.name);
                if (hitInfo.transform.GetComponent<LevelIconScript>() != null && !menuOpen)
                {
                    hitInfo.transform.GetComponent<LevelIconScript>().Clicked();
                }
                if (hitInfo.transform.name == "Gear")
                {
                    menuOpen = !menuOpen;
                    hitInfo.transform.GetComponent<PauseMenuController>().Clicked();
                }
                else if (hitInfo.transform.name == "Sound")
                {
                    hitInfo.transform.GetComponent<PauseMenuController>().clickSound();
                }
                else if (hitInfo.transform.name == "Musique")
                {
                    hitInfo.transform.GetComponent<PauseMenuController>().clickMusic();
                }
                else if (hitInfo.transform.name == "Help" && !helpOpen)
                {
                    helpOpen = !helpOpen;
                    hitInfo.transform.GetComponent<PauseMenuController>().clickHelp();
                }
                else if (hitInfo.transform.name == "Help" && helpOpen)
                {
                    helpOpen = !helpOpen;
                    hitInfo.transform.GetComponent<PauseMenuController>().closeHelp();
                }
                else if (hitInfo.transform.name == "Home")
                {
                    hitInfo.transform.GetComponent<PauseMenuController>().clickHome();
                }
                else if (hitInfo.transform.name == "Reset")
                {
                    hitInfo.transform.GetComponent<PauseMenuController>().clickReset();
                }
                else if (hitInfo.transform.name == "infoBox")
                {
                    helpOpen = !helpOpen;
                    hitInfo.transform.GetComponent<PauseMenuController>().closeHelp();
                }

                // Here you can check hitInfo to see which collider has been hit, and act appropriately.
            }
            menuAction.state = MenuAction.TouchState.NONE;
        }
        else if(menuAction.state == MenuAction.TouchState.UP)
        {
            Vector3 debut = Camera.main.ScreenToWorldPoint(new Vector3(menuAction.touchLastPosition.x, menuAction.touchLastPosition.y, 0));
            Vector3 fin = Camera.main.ScreenToWorldPoint(new Vector3(menuAction.touchCurrentPosition.x, menuAction.touchCurrentPosition.y, 0));
            scrollVelocity = (debut - fin) * scrollBoost;
            //scroll the Cam
            menuAction.state = MenuAction.TouchState.NONE;
        }
        else if (menuAction.state == MenuAction.TouchState.MOVED)
        {
            Vector3 debut = Camera.main.ScreenToWorldPoint(new Vector3(menuAction.touchLastPosition.x, menuAction.touchLastPosition.y, 0));
            Vector3 fin = Camera.main.ScreenToWorldPoint(new Vector3(menuAction.touchCurrentPosition.x, menuAction.touchCurrentPosition.y, 0));
            Vector3 deplacement = (debut - fin);
            GetComponent<Camera>().transform.Translate(deplacement);
        }
        else if(menuAction.state == MenuAction.TouchState.NONE)
        {
            if(scrollVelocity.magnitude <= scrollMinVelocity)
            {
                scrollVelocity = new Vector2();
            }
            else
            {
                scrollVelocity = scrollVelocity * scrollFriction;
            }
            GetComponent<Camera>().transform.Translate(scrollVelocity);
        }
    }
    private void ZoomAction()
    {
        Touch touch = Input.GetTouch(0);
        Touch touchOne = Input.GetTouch(1);

        Vector2 touchPrevPos = touch.position - touch.deltaPosition;
        Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

        float prevTouchDeltaMag = (touchPrevPos - touchOnePrevPos).magnitude;
        float touchDeltaMag = (touch.position - touchOne.position).magnitude;
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        Camera cam = GetComponent<Camera>();
        cam.orthographicSize += deltaMagnitudeDiff * orthoZoomSpeed;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, orthoZoomMin, orthoZoomMax);
    }

    private void ReplaceCamera()
    {
        Vector3 lowerBoundCam = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 upperBoundCam = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));

        //BOTTOM LEFT
        if (lowerBoundCam.x - lowerFiledBounds.min.x < 0)
        {
            Camera.main.transform.Translate(new Vector3(lowerFiledBounds.min.x - lowerBoundCam.x, 0, 0));
            scrollVelocity = new Vector2(0, scrollVelocity.y);
        }
        if (lowerBoundCam.y - lowerFiledBounds.min.y < 0)
        {
            Camera.main.transform.Translate(new Vector3(0, lowerFiledBounds.min.y - lowerBoundCam.y, 0));
            scrollVelocity = new Vector2(scrollVelocity.x, 0);
        }

        //TOP RIGHT
        if (upperBoundCam.x - upperFiledBounds.max.x > 0)
        {
            Camera.main.transform.Translate(new Vector3(upperFiledBounds.max.x - upperBoundCam.x, 0, 0));
            scrollVelocity = new Vector2(0, scrollVelocity.y);
        }
        if (upperBoundCam.y - upperFiledBounds.max.y > 0)
        {
            Camera.main.transform.Translate(new Vector3(0, upperFiledBounds.max.y - upperBoundCam.y, 0));
            scrollVelocity = new Vector2(scrollVelocity.x, 0);
        }

        if (Camera.main.orthographicSize > orthoZoomMax)
        {
            Camera.main.orthographicSize = orthoZoomMax;
        }
        if (Camera.main.orthographicSize < orthoZoomMin)
        {
            Camera.main.orthographicSize = orthoZoomMin;
        }
    }
}


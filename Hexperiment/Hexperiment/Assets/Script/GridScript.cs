using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridScript : MonoBehaviour {


    private string rotateSound = "NFF-rolling-a";
    private string wrongSound = "NFF-footsie";
    private string matchedSound = "NFF-good-tip-low";

    public enum PieceType
    {
        EMPTY,
        NORMAL,
        OBSTACLE,
        COLUMN_CLEAR,//special must be together
        DIAGONAL_LEFT,//special must be together
        DIAGONAL_RIGHT,//special must be together
        SPIRAL_MOVE,
        LOCKED,
        COUNT
    };

    public enum ActionState
    {
        NONE,
        DOWN,
        UP,
        SELECTED,
        ROTATING,
        DISABLED
    }

    public enum GameState
    {
        WAITING,
        FILLING,
        LEVEL_DONE,
        CLEARING_SPECIAL,
        DONE_CLEARING
    }

    private AudioSource rotateSource;
    private AudioSource matchedSource;
    private AudioSource wrongSource;

    [System.Serializable]
    public struct PiecePrefab
    {
        public PieceType type;
        public GameObject prefab;
    }

    [System.Serializable]
    public struct piecePosition
    {
        public PieceType type;
        public int x;
        public int y;
    }

    private struct TouchAction
    {
        public ActionState state;
        public Vector2 touchStart;
        public Vector2 touchEnded;
        public int nbRotation;
    }
    private struct SelectedGroup
    {
        public bool highLigthFlipped;
        public HexPiece[] HexGroup;
        public Vector3 center;
    }

    public Level level;
    public int xDim;
    public int yDim;
    public float fillTime;
    public GameObject highlight;
    public PiecePrefab[] piecePrefabs;
    public piecePosition[] initialPieceArray;
    public GameState gameState = GameState.WAITING;

    private Dictionary<PieceType, GameObject> piecePrefabDict;
    private HexPiece[,] pieces;
    private bool inverse = false;
    private BoxCollider2D boxCollider;
    private SelectedGroup selectedHexGroup;
    private bool gameOver = false;
    private TouchAction touchAction;

    // Use this for initialization
    void Awake() {
        rotateSource = gameObject.AddComponent<AudioSource>();
        rotateSource.clip = Resources.Load(rotateSound) as AudioClip;
        rotateSource.volume = 0.5f;

        matchedSource = gameObject.AddComponent<AudioSource>();
        matchedSource.clip = Resources.Load(matchedSound) as AudioClip;
        matchedSource.volume = 0.3f;

        wrongSource = gameObject.AddComponent<AudioSource>();
        wrongSource.clip = Resources.Load(wrongSound) as AudioClip;
        wrongSource.volume = 0.5f;

        piecePrefabDict = new Dictionary<PieceType, GameObject>();
        selectedHexGroup.HexGroup = new HexPiece[3];
        for (int i = 0; i < piecePrefabs.Length; i++)
        {
            if (!piecePrefabDict.ContainsKey(piecePrefabs[i].type))
            {
                piecePrefabDict.Add(piecePrefabs[i].type, piecePrefabs[i].prefab);
            }
        }

        pieces = new HexPiece[xDim, yDim];

        for (int i = 0; i < initialPieceArray.Length; i++)
        {
            if (initialPieceArray[i].x < 0 || initialPieceArray[i].x >= xDim ||
               initialPieceArray[i].y < 0 || initialPieceArray[i].y >= yDim)
            {
                Debug.LogError("Error lors de spawning des pieces predefinies");
            }
            SpawnNewPiece(initialPieceArray[i].x, initialPieceArray[i].y, initialPieceArray[i].type);
        }

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y] == null)
                {
                    SpawnNewPiece(x, y, PieceType.EMPTY);
                }
            }
        }

        highlight.SetActive(false);
        StartCoroutine(Fill());

        boxCollider = transform.parent.GetComponent<BoxCollider2D>();
    }

    public bool ChooseGroup(Vector3 pos)
    {
        if (boxCollider.bounds.Contains(pos))
        {
            selectedHexGroup.HexGroup = new HexPiece[3];
            //Brute force the grid to get the 3 closest pieces
            HexPiece best = pieces[0, 0];
            HexPiece best2 = pieces[0, 0];
            HexPiece best3 = pieces[0, 0];
            float distance = float.MaxValue;
            float distance2 = float.MaxValue;
            float distance3 = float.MaxValue;

            for (int x = 0; x < xDim; x++)
            {
                for (int y = 0; y < yDim; y++)
                {
                    //float curDistance = Vector3.Distance(pos, (Vector3)pieces[0, 0].Grid.GetWorldPosition(x, y));
                    float curDistance = Vector3.Distance(pos, pieces[x, y].transform.position);
                    if (curDistance < distance)
                    {
                        distance3 = distance2;
                        best3 = best2;
                        distance2 = distance;
                        best2 = best;
                        distance = curDistance;
                        best = pieces[x, y];
                    }
                    else if (curDistance < distance2 && !Mathf.Approximately(curDistance, distance))
                    {
                        distance3 = distance2;
                        best3 = best2;
                        distance2 = curDistance;
                        best2 = pieces[x, y];
                    }
                    else if (curDistance < distance3 && curDistance > distance2 && !Mathf.Approximately(curDistance, distance2))
                    {
                        distance3 = curDistance;
                        best3 = pieces[x, y];
                    }
                }
            }
            if (IsAdjacent(best, best2, best3) && IsRotatable(best, best2, best3))
            {
                SelectHexGroup(best, best2, best3);

                highlight.SetActive(true);
                if (selectedHexGroup.HexGroup[0].X == selectedHexGroup.HexGroup[2].X && selectedHexGroup.highLigthFlipped ||
                    selectedHexGroup.HexGroup[0].X != selectedHexGroup.HexGroup[2].X && !selectedHexGroup.highLigthFlipped)
                {
                    highlight.transform.localScale = new Vector3(highlight.transform.localScale.x * -1, highlight.transform.localScale.y, highlight.transform.localScale.z);
                    selectedHexGroup.highLigthFlipped = !selectedHexGroup.highLigthFlipped;
                }
                return true;
            }
        }
        selectedHexGroup.HexGroup = null;
        return false;
    }

    // Update is called once per frame
    void Update() {
        if (touchAction.state == ActionState.DISABLED)
        {
            Destroy(GameObject.Find("DESTROY"));//HACKY AS F*CK look at the note HACK_DESTROY
            return;
        }
        if (touchAction.state == ActionState.UP)
        {
            if (Vector2.Distance(touchAction.touchStart, touchAction.touchEnded) > 25 && (selectedHexGroup.HexGroup != null))
            {
                if (PlayerPrefs.GetInt("Sound") == 0) {
                    rotateSource.Play();
                }
                RotateSelectedPiece();
                touchAction.state = ActionState.ROTATING;
            }
            else
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(touchAction.touchEnded);
                pos.z = boxCollider.bounds.center.z;
                ChooseGroup(pos);
                touchAction.state = ActionState.SELECTED;
            }
        }

        if (touchAction.state != ActionState.ROTATING && touchAction.state != ActionState.DISABLED)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchAction.touchStart = touch.position;
                    touchAction.state = ActionState.DOWN;
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    touchAction.touchEnded = Input.mousePosition;
                    touchAction.state = ActionState.UP;
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                touchAction.touchStart = Input.mousePosition;
                touchAction.state = ActionState.DOWN;
            }

            if (Input.GetMouseButtonUp(0))
            {
                touchAction.touchEnded = Input.mousePosition;
                touchAction.state = ActionState.UP;
            }
        }
        else if (touchAction.state != ActionState.DISABLED)
        {
            if (selectedHexGroup.HexGroup != null && !selectedHexGroup.HexGroup[0].MovableComponent.IsMoving && touchAction.nbRotation > 0 && touchAction.nbRotation < 3)
            {
                RotateSelectedPiece();
                if (touchAction.nbRotation >= 3)
                {
                    //touchAction.state = ActionState.SELECTED;
                    if (PlayerPrefs.GetInt("Sound") == 0)
                    {
                        wrongSource.Play();
                    }
                    touchAction.nbRotation = 0;
                }
            }
            else if (selectedHexGroup.HexGroup != null && selectedHexGroup.HexGroup[0].MovableComponent.IsMoving)
            {
                touchAction.state = ActionState.ROTATING;
            }
            else if (selectedHexGroup.HexGroup != null && !selectedHexGroup.HexGroup[0].MovableComponent.IsMoving)
            {
                touchAction.state = ActionState.SELECTED;
                highlight.SetActive(true);
            }
            else
            {
                touchAction.state = ActionState.NONE;
            }
        }

        //DEBUG ONLY
        /*for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].X != x || pieces[x, y].Y != y)
                {
                    Debug.LogError("error insertion expected " + x + ", " + y + " got " + pieces[x, y].X + ", " + pieces[x, y].Y);
                }
            }
        }*/
    }

    private HexPiece GetHexPiece(int x, int y)
    {
        if (x < 0 || x >= xDim || y < 0 || y >= yDim)
            return null;
        return pieces[x, y];
    }

    public void SelectHexGroup(HexPiece p1, HexPiece p2, HexPiece p3)
    {
        //make the p1 piece the leftest piece and then on top of others if possible
        HexPiece temp = p1;
        if (p1.X > p2.X)
        {
            p1 = p2;
            p2 = temp;
        }
        else if (p1.X > p3.X)
        {
            p1 = p3;
            p3 = temp;
        }

        temp = p1;
        if (p1.X == p2.X && p1.Y > p2.Y)
        {
            p1 = p2;
            p2 = temp;
        }
        else if (p1.X == p3.X && p1.Y > p3.Y)
        {
            p1 = p3;
            p3 = temp;
        }

        temp = p2;
        if (p2.X < p3.X)
        {
            p2 = p3;
            p3 = temp;
        }
        else if (p2.Y > p3.Y)
        {
            p2 = p3;
            p3 = temp;
        }

        selectedHexGroup.HexGroup[0] = p1;
        selectedHexGroup.HexGroup[1] = p2;
        selectedHexGroup.HexGroup[2] = p3;


        selectedHexGroup.center = (p1.transform.position + p2.transform.position + p3.transform.position) / 3 - Vector3.forward;

        highlight.transform.position = selectedHexGroup.center;
    }

    public IEnumerator Fill()
    {
        bool needsRefill = true;
        gameState = GameState.FILLING;

        while (needsRefill)
        {
            yield return new WaitForSeconds(fillTime);
            while (PiecesBeingClear())
            {
                yield return new WaitForSeconds(fillTime);
            }

            while (FillStep())
            {
                inverse = !inverse;
                yield return new WaitForSeconds(fillTime);
            }

            needsRefill = ClearAllValidMatches();
        }

        gameState = GameState.WAITING;
    }

    public bool FillStep()
    {
        bool movedPiece = false;

        for (int y = yDim - 2; y >= 0; y--)
        {
            for (int loopX = 0; loopX < xDim; loopX++)
            {
                int x = loopX;

                if (inverse)
                {
                    x = xDim - 1 - loopX;
                }

                HexPiece piece = pieces[x, y];

                if (piece.IsMovable())
                {
                    HexPiece pieceBelow = pieces[x, y + 1];

                    if (pieceBelow.Type == PieceType.EMPTY)
                    {
                        GameObject.Destroy(pieceBelow.gameObject);
                        piece.MovableComponent.Move(x, y + 1, fillTime);
                        pieces[x, y + 1] = piece;
                        SpawnNewPiece(x, y, PieceType.EMPTY);
                        movedPiece = true;
                    }
                    else
                    {
                        for (int diag = -1; diag <= 1; diag++)
                        {
                            if (diag != 0)
                            {
                                int diagX = x + diag;

                                if (inverse)
                                {
                                    diagX = x - diag;
                                }

                                if (diagX >= 0 && diagX < xDim)
                                {
                                    HexPiece diagonialPiece = pieces[diagX, y + 1];

                                    if (diagonialPiece.Type == PieceType.EMPTY)
                                    {
                                        bool hasPieceAbove = true;

                                        for (int aboveY = y; aboveY >= 0; aboveY--)
                                        {
                                            HexPiece pieceAbove = pieces[diagX, aboveY];

                                            if (pieceAbove.IsMovable())
                                            {
                                                break;
                                            }
                                            else if (!pieceAbove.IsMovable() && pieceAbove.Type != PieceType.EMPTY)
                                            {
                                                hasPieceAbove = false;
                                                break;
                                            }
                                        }

                                        if (!hasPieceAbove)
                                        {
                                            GameObject.Destroy(diagonialPiece.gameObject);
                                            piece.MovableComponent.Move(diagX, y + 1, fillTime);
                                            pieces[diagX, y + 1] = piece;
                                            SpawnNewPiece(x, y, PieceType.EMPTY);
                                            movedPiece = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Fill the top row
        for (int x = 0; x < xDim; x++)
        {
            HexPiece pieceBelow = pieces[x, 0];

            if (pieceBelow.Type == PieceType.EMPTY)
            {
                GameObject.Destroy(pieceBelow.gameObject);
                GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[PieceType.NORMAL], GetWorldPosition(x, -1), Quaternion.identity);
                newPiece.transform.parent = transform;

                if (pieceBelow.Type == PieceType.EMPTY)
                {
                    pieces[x, 0] = newPiece.GetComponent<HexPiece>();
                    pieces[x, 0].Init(x, -1, this, PieceType.NORMAL);
                    pieces[x, 0].MovableComponent.Move(x, 0, fillTime);
                    pieces[x, 0].ColorComponent.SetColor((ColorPiece.ColorType)Random.Range(0, pieces[x, 0].ColorComponent.NumColors));
                    movedPiece = true;
                }

            }
        }

        return movedPiece;
    }

    public Vector2 GetWorldPosition(int x, int y)
    {
        if (x % 2 == 0)
        {
            return new Vector2(transform.position.x + x - x * 0.2175f,
                                transform.position.y - y * 0.896f);
        }
        else
        {
            return new Vector2(transform.position.x + x - x * 0.2175f,
                                transform.position.y - y * 0.896f - 0.45f);
        }
        //return new Vector2(transform.position.x - xDim / 2.0f + x, transform.position.y + yDim / 2.0f - y);
    }

    public void ChangePieceType(int x, int y)
    {
        pieces[x, y].name = "DESTROY";//HACKY AS F*CK look at the note HACK_DESTROY
        PieceType nextType = pieces[x, y].NextType;
        if (pieces[x, y].IsColored())
        {
            ColorPiece.ColorType color = pieces[x, y].ColorComponent.Color;            
            GameObject.Destroy(pieces[x, y]);
            SpawnNewPiece(x, y, nextType);
            if (pieces[x, y].IsColored())
                pieces[x, y].ColorComponent.SetColor(color);
        }
        else
        {
            GameObject.Destroy(pieces[x, y]);
            SpawnNewPiece(x, y, nextType);
        }
    }

    public bool PiecesBeingClear()
    {
        bool pieceBeingClear = false;
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].IsClearable())
                {
                    if (pieces[x, y].ClearComponent.IsBeingCleared)
                    {
                        pieceBeingClear = true;
                    }
                    else if (pieces[x, y].ClearComponent.IsCleared)
                    {
                        ChangePieceType(x, y);
                    }
                }
            }
        }

        return pieceBeingClear;
    }

    public HexPiece SpawnNewPiece(int x, int y, PieceType type)
    {
        GameObject newPiece = (GameObject)Instantiate(piecePrefabDict[type], GetWorldPosition(x, y), Quaternion.identity);
        newPiece.transform.parent = transform;

        pieces[x, y] = newPiece.GetComponent<HexPiece>();
        pieces[x, y].Init(x, y, this, type);

        return pieces[x, y];
    }

    public bool IsAdjacent(HexPiece p1, HexPiece p2, HexPiece p3)
    {
        if (p1.X == p2.X && p1.X == p3.X ||
           p1.Y == p2.Y && p1.Y == p3.Y)
        {
            return false;
        }
        return true;
    }
    public bool IsRotatable(HexPiece p1, HexPiece p2, HexPiece p3)
    {
        if (p1.IsMovable() && p2.IsMovable() && p3.IsMovable())
        {
            return true;
        }

        return false;
    }

    public void RotateSelectedPiece()
    {

        Vector3 pos = Camera.main.ScreenToWorldPoint(touchAction.touchEnded);
        bool clockWise = false;
        if (pos.x > selectedHexGroup.center.x)
        {
            clockWise = touchAction.touchEnded.y < touchAction.touchStart.y;
        }
        else
        {
            clockWise = touchAction.touchEnded.y > touchAction.touchStart.y;
        }

        if (gameState >= GameState.LEVEL_DONE) return;
        highlight.SetActive(false);

        if (clockWise)
        {
            pieces[selectedHexGroup.HexGroup[0].X, selectedHexGroup.HexGroup[0].Y] = selectedHexGroup.HexGroup[2];
            pieces[selectedHexGroup.HexGroup[1].X, selectedHexGroup.HexGroup[1].Y] = selectedHexGroup.HexGroup[0];
            pieces[selectedHexGroup.HexGroup[2].X, selectedHexGroup.HexGroup[2].Y] = selectedHexGroup.HexGroup[1];

            int p3x = selectedHexGroup.HexGroup[2].X;
            int p3y = selectedHexGroup.HexGroup[2].Y;

            Vector3 center = (selectedHexGroup.HexGroup[0].transform.position + selectedHexGroup.HexGroup[1].transform.position + selectedHexGroup.HexGroup[2].transform.position) / 3;

            selectedHexGroup.HexGroup[2].MovableComponent.Rotate(-120.0f, selectedHexGroup.HexGroup[0].X, selectedHexGroup.HexGroup[0].Y, center, fillTime);
            selectedHexGroup.HexGroup[0].MovableComponent.Rotate(-120.0f, selectedHexGroup.HexGroup[1].X, selectedHexGroup.HexGroup[1].Y, center, fillTime);
            selectedHexGroup.HexGroup[1].MovableComponent.Rotate(-120.0f, p3x, p3y, center, fillTime);
        }
        else
        {
            pieces[selectedHexGroup.HexGroup[0].X, selectedHexGroup.HexGroup[0].Y] = selectedHexGroup.HexGroup[1];
            pieces[selectedHexGroup.HexGroup[1].X, selectedHexGroup.HexGroup[1].Y] = selectedHexGroup.HexGroup[2];
            pieces[selectedHexGroup.HexGroup[2].X, selectedHexGroup.HexGroup[2].Y] = selectedHexGroup.HexGroup[0];

            int p1x = selectedHexGroup.HexGroup[0].X;
            int p1y = selectedHexGroup.HexGroup[0].Y;

            Vector3 center = (selectedHexGroup.HexGroup[0].transform.position + selectedHexGroup.HexGroup[1].transform.position + selectedHexGroup.HexGroup[2].transform.position) / 3;

            selectedHexGroup.HexGroup[0].MovableComponent.Rotate(-120.0f, selectedHexGroup.HexGroup[2].X, selectedHexGroup.HexGroup[2].Y, center, fillTime);
            selectedHexGroup.HexGroup[2].MovableComponent.Rotate(-120.0f, selectedHexGroup.HexGroup[1].X, selectedHexGroup.HexGroup[1].Y, center, fillTime);
            selectedHexGroup.HexGroup[1].MovableComponent.Rotate(-120.0f, p1x, p1y, center, fillTime);
        }

        if (ClearAllValidMatches())
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                matchedSource.Play();
            }
            StartCoroutine(Fill());
            level.OnMove();
            touchAction.nbRotation = 0;
        }
        else
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                rotateSource.Play();
            }
            touchAction.nbRotation++;
        }
    }

    public List<HexPiece> GetAdjacentPieceOrdered(HexPiece piece, int newX, int newY)
    {
        List<HexPiece> adjacentPieces = new List<HexPiece>();
        if (newX % 2 == 0)
        {
            if (newX > 0 && newY > 0 && pieces[newX - 1, newY - 1].IsColored())//left-up
            {
                adjacentPieces.Add(pieces[newX - 1, newY - 1]);
            }
            if (newY > 0 && pieces[newX, newY - 1].IsColored())//up
            {
                adjacentPieces.Add(pieces[newX, newY - 1]);
            }
            if (newX < xDim - 1 && newY > 0 && pieces[newX + 1, newY - 1].IsColored())//right-up
            {
                adjacentPieces.Add(pieces[newX + 1, newY - 1]);
            }
            if (newX < xDim - 1 && pieces[newX + 1, newY].IsColored())//rigth-bottom
            {
                adjacentPieces.Add(pieces[newX + 1, newY]);
            }
            if (newY < yDim - 1 && pieces[newX, newY + 1].IsColored())//bottom 
            {
                adjacentPieces.Add(pieces[newX, newY + 1]);
            }
            if (newX > 0 && pieces[newX - 1, newY].IsColored())//left-bottom 
            {
                adjacentPieces.Add(pieces[newX - 1, newY]);
            }
        }
        else
        {
            if (newX > 0 && pieces[newX - 1, newY].IsColored())//left-up
            {
                adjacentPieces.Add(pieces[newX - 1, newY]);
            }
            if (newY > 0 && pieces[newX, newY - 1].IsColored())//up
            {
                adjacentPieces.Add(pieces[newX, newY - 1]);
            }
            if (newX < xDim - 1 && pieces[newX + 1, newY].IsColored())//right-up
            {
                adjacentPieces.Add(pieces[newX + 1, newY]);
            }
            if (newX < xDim - 1 && newY < yDim - 2 && pieces[newX + 1, newY + 1].IsColored())//rigth-bottom
            {
                adjacentPieces.Add(pieces[newX + 1, newY + 1]);
            }
            if (newY < yDim - 1 && pieces[newX, newY + 1].IsColored())//bottom 
            {
                adjacentPieces.Add(pieces[newX, newY + 1]);
            }
            if (newX > xDim - 1 && newY > 0 && pieces[newX - 1, newY].IsColored())//left-bottom 
            {
                adjacentPieces.Add(pieces[newX - 1, newY]);
            }
        }

        return adjacentPieces;
    }

    public List<HexPiece> GetFlower(HexPiece piece, int newX, int newY)
    {
        List<HexPiece> flowerPieces = new List<HexPiece>();
        if (pieces[newX, newY - 1].IsColored() && pieces[newX, newY + 1].IsColored() &&
            pieces[newX - 1, newY].IsColored() && pieces[newX + 1, newY].IsColored() &&
            pieces[newX, newY - 1].ColorComponent.Color == pieces[newX, newY + 1].ColorComponent.Color &&
            pieces[newX, newY - 1].ColorComponent.Color == pieces[newX - 1, newY].ColorComponent.Color &&
            pieces[newX, newY - 1].ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color)
        {
            if (newX % 2 == 0)
            {
                if (pieces[newX - 1, newY - 1].IsColored() && pieces[newX + 1, newY - 1].IsColored())
                {
                    if (pieces[newX, newY - 1].ColorComponent.Color == pieces[newX - 1, newY - 1].ColorComponent.Color &&
                       pieces[newX, newY - 1].ColorComponent.Color == pieces[newX + 1, newY - 1].ColorComponent.Color)
                    {
                        flowerPieces.Add(pieces[newX, newY - 1]);
                        flowerPieces.Add(pieces[newX, newY + 1]);
                        flowerPieces.Add(pieces[newX - 1, newY]);
                        flowerPieces.Add(pieces[newX + 1, newY]);
                        flowerPieces.Add(pieces[newX - 1, newY - 1]);
                        flowerPieces.Add(pieces[newX + 1, newY - 1]);
                        return flowerPieces;
                    }
                }
            }
            else
            {
                if (pieces[newX - 1, newY + 1].IsColored() && pieces[newX + 1, newY + 1].IsColored())
                {
                    if (pieces[newX, newY - 1].ColorComponent.Color == pieces[newX - 1, newY + 1].ColorComponent.Color &&
                        pieces[newX, newY - 1].ColorComponent.Color == pieces[newX + 1, newY + 1].ColorComponent.Color)
                    {
                        flowerPieces.Add(pieces[newX, newY - 1]);
                        flowerPieces.Add(pieces[newX, newY + 1]);
                        flowerPieces.Add(pieces[newX - 1, newY]);
                        flowerPieces.Add(pieces[newX + 1, newY]);
                        flowerPieces.Add(pieces[newX - 1, newY + 1]);
                        flowerPieces.Add(pieces[newX + 1, newY + 1]);
                        return flowerPieces;
                    }
                }
            }
        }

        return null;
    }

    public List<HexPiece> GetGroup4(HexPiece piece, int newX, int newY)
    {
        List<HexPiece> group = new List<HexPiece>();
        if (pieces[newX, newY - 1].IsColored() && piece.IsColored() &&
            pieces[newX, newY - 1].ColorComponent.Color == piece.ColorComponent.Color)
        {
            if (newX % 2 == 0 && newY > 1)
            {
                /*             __
                              /  \__
                              \__/  \
                              /  \__/
                              \__/||\
                                 \||/       */
                if (newX > 0 && pieces[newX - 1, newY - 1].IsColored() && pieces[newX - 1, newY - 2].IsColored() &&
                    piece.ColorComponent.Color == pieces[newX - 1, newY - 1].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX - 1, newY - 2].ColorComponent.Color)
                {
                    group.Add(pieces[newX, newY - 1]);
                    group.Add(pieces[newX - 1, newY - 1]);
                    group.Add(pieces[newX - 1, newY - 2]);
                    pieces[newX, newY].NextType = PieceType.DIAGONAL_LEFT;
                }
                /*              __
                             __/  \
                            /  \__/
                            \__/  \
                            /||\__/
                            \||/             */
                if (newX < xDim - 1 && pieces[newX + 1, newY - 1].IsColored() && pieces[newX + 1, newY - 2].IsColored() &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY - 1].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY - 2].ColorComponent.Color)
                {
                    group.Add(pieces[newX, newY - 1]);
                    group.Add(pieces[newX + 1, newY - 1]);
                    group.Add(pieces[newX + 1, newY - 2]);
                    pieces[newX, newY].NextType = PieceType.DIAGONAL_RIGHT;
                }
                /*            __  
                           __/  \__
                          /  \__/  \
                          \__/||\__/
                             \||/            */
                if (newX > 0 && newX < xDim - 1 && pieces[newX - 1, newY - 1].IsColored() && pieces[newX + 1, newY - 1].IsColored() &&
                    piece.ColorComponent.Color == pieces[newX - 1, newY - 1].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY - 1].ColorComponent.Color)
                {
                    group.Add(pieces[newX, newY - 1]);
                    group.Add(pieces[newX - 1, newY - 1]);
                    group.Add(pieces[newX + 1, newY - 1]);
                    pieces[newX, newY].NextType = PieceType.COLUMN_CLEAR;
                }
            }
            else
            {
                /*             __
                              /  \__
                              \__/  \
                              /  \__/
                              \__/||\
                                 \||/       */
                if (newX > 0 && pieces[newX - 1, newY].IsColored() && pieces[newX - 1, newY - 1].IsColored() &&
                    piece.ColorComponent.Color == pieces[newX - 1, newY].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX - 1, newY - 1].ColorComponent.Color)
                {
                    group.Add(pieces[newX, newY - 1]);
                    group.Add(pieces[newX - 1, newY]);
                    group.Add(pieces[newX - 1, newY - 1]);
                    pieces[newX, newY].NextType = PieceType.DIAGONAL_LEFT;
                }
                /*              __
                             __/  \
                            /  \__/
                            \__/  \
                            /||\__/
                            \||/             */
                if (newX < xDim - 1 && pieces[newX + 1, newY].IsColored() && pieces[newX + 1, newY - 1].IsColored() &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY - 1].ColorComponent.Color)
                {
                    group.Add(pieces[newX, newY - 1]);
                    group.Add(pieces[newX + 1, newY]);
                    group.Add(pieces[newX + 1, newY - 1]);
                    pieces[newX, newY].NextType = PieceType.DIAGONAL_RIGHT;
                }
                /*            __  
                           __/  \__
                          /  \__/  \
                          \__/||\__/
                             \||/            */
                if (newX > 0 && newX < xDim - 1 && pieces[newX - 1, newY].IsColored() && pieces[newX + 1, newY].IsColored() &&
                    piece.ColorComponent.Color == pieces[newX - 1, newY].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color)
                {
                    group.Add(pieces[newX, newY - 1]);
                    group.Add(pieces[newX - 1, newY]);
                    group.Add(pieces[newX + 1, newY]);
                    pieces[newX, newY].NextType = PieceType.COLUMN_CLEAR;
                }
            }
        }

        return group.Count > 0 ? group : null;
    }

    public List<HexPiece> GetGroup3(HexPiece piece, int newX, int newY)
    {
        List<HexPiece> group = new List<HexPiece>();

        if (newX % 2 == 0)
        {
            /*             
                            __
                           /xy\__
                           \__/  \
                           /  \__/
                           \__/          */
            if (newX < xDim - 1)
            {
                if (newY < yDim - 1)
                {
                    if (pieces[newX, newY + 1].IsColored())
                    {
                        if (pieces[newX + 1, newY].IsColored())
                        {
                            if (piece.ColorComponent.Color == pieces[newX, newY + 1].ColorComponent.Color &&
                    piece.ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color)
                            {
                                group.Add(pieces[newX, newY + 1]);
                                group.Add(pieces[newX + 1, newY]);
                            }
                        }
                    }
                }
            }
            if (newX < xDim - 1 && newY < yDim - 1 && pieces[newX, newY + 1].IsColored() && pieces[newX + 1, newY].IsColored() &&
                piece.ColorComponent.Color == pieces[newX, newY + 1].ColorComponent.Color &&
                piece.ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color)
            {
                group.Add(pieces[newX, newY + 1]);
                group.Add(pieces[newX + 1, newY]);
            }
            /*             
                                __
                             __/  \
                            /xy\__/
                            \__/  \
                               \__/       */
            if (newX < xDim - 1 && newY > 0 && pieces[newX + 1, newY - 1].IsColored() && pieces[newX + 1, newY].IsColored() &&
                piece.ColorComponent.Color == pieces[newX + 1, newY - 1].ColorComponent.Color &&
                piece.ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color)
            {
                group.Add(pieces[newX + 1, newY - 1]);
                group.Add(pieces[newX + 1, newY]);
            }
        }
        else
        {
            /*             
                            __
                           /xy\__
                           \__/  \
                           /  \__/
                           \__/          */
            if (newX < xDim - 1 && newY < yDim - 1 && pieces[newX + 1, newY + 1].IsColored() && pieces[newX, newY + 1].IsColored() &&
                piece.ColorComponent.Color == pieces[newX + 1, newY + 1].ColorComponent.Color &&
                piece.ColorComponent.Color == pieces[newX, newY + 1].ColorComponent.Color)
            {
                group.Add(pieces[newX + 1, newY + 1]);
                group.Add(pieces[newX, newY + 1]);
            }
            /*             
                                __
                             __/  \
                            /xy\__/
                            \__/  \
                               \__/       */
            if (newX < xDim - 1 && newY < yDim - 1 && pieces[newX + 1, newY].IsColored() && pieces[newX + 1, newY + 1].IsColored() &&
                piece.ColorComponent.Color == pieces[newX + 1, newY].ColorComponent.Color &&
                piece.ColorComponent.Color == pieces[newX + 1, newY + 1].ColorComponent.Color)
            {
                group.Add(pieces[newX + 1, newY]);
                group.Add(pieces[newX + 1, newY + 1]);
            }
        }

        return group.Count > 0 ? group : null;
    }

    public void ClearColumn(int column)
    {
        for (int y = 0; y < yDim; y++)
        {
            ClearPiece(column, y);
        }
    }

    public void ClearDiagonalRight(int x, int y)
    {
        int cpy = y;
        for (int cpx = x - 1; cpx >= 0; --cpx)
        {
            if (cpx % 2 == 0) ++cpy;
            if (cpy >= yDim) continue;
            ClearPiece(cpx, cpy);
        }

        cpy = y;
        for (int cpx = x + 1; cpx < xDim; ++cpx)
        {
            if (cpx % 2 != 0) --cpy;
            if (cpy < 0) continue;
            ClearPiece(cpx, cpy);
        }
    }

    public void ClearDiagonalLeft(int x, int y)
    {
        int cpy = y;
        for (int cpx = x - 1; cpx >= 0; --cpx)
        {
            if (cpx % 2 != 0) --cpy;
            if (cpy < 0) continue;
            ClearPiece(cpx, cpy);
        }

        cpy = y;
        for (int cpx = x + 1; cpx < xDim; ++cpx)
        {
            if (cpx % 2 == 0) ++cpy;
            if (cpy >= yDim) continue;
            ClearPiece(cpx, cpy);
        }
    }

    public void ClearColor(ColorPiece.ColorType color)
    {
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].IsColored() && (pieces[x, y].ColorComponent.Color == color
                    || color == ColorPiece.ColorType.ANY))
                {
                    ClearPiece(x, y);
                }
            }
        }
    }

    public bool ClearAllValidMatches()
    {
        bool needsRefill = false;

        List<HexPiece> toClear = new List<HexPiece>();

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (!pieces[x, y].IsColored()) continue;
                if (x >= 1 && x < xDim - 1 && y >= 1 && y < yDim - 1)//check flower only if possible
                {
                    List<HexPiece> flower = GetFlower(pieces[x, y], x, y);
                    if (flower != null && CanClearGroup(flower))
                    {
                        pieces[x, y].NextType = PieceType.SPIRAL_MOVE;
                        foreach (HexPiece hex in flower)
                        {
                            toClear.Add(hex);
                        }
                        toClear.Add(pieces[x, y]);
                    }
                }
                if (y > 0 && pieces[x, y].CanClear())  // no need to check first row
                {
                    List<HexPiece> group4 = GetGroup4(pieces[x, y], x, y);
                    if (group4 != null && CanClearGroup(group4))
                    {

                        //pieces[x, y].NextType = PieceType.COLUMN_CLEAR;
                        foreach (HexPiece hex in group4)
                        {
                            toClear.Add(hex);
                        }
                        toClear.Add(pieces[x, y]);
                    }
                }
                if (pieces[x, y].CanClear())
                {
                    List<HexPiece> group3 = GetGroup3(pieces[x, y], x, y);
                    if (group3 != null && CanClearGroup(group3))
                    {

                        foreach (HexPiece hex in group3)
                        {
                            toClear.Add(hex);
                        }
                        toClear.Add(pieces[x, y]);
                    }
                }
            }
        }
        foreach (HexPiece hex in toClear)
        {
            if (ClearPiece(hex.X, hex.Y))
            {
                if (PlayerPrefs.GetInt("Sound") == 0)
                {
                    matchedSource.Play();
                }
                needsRefill = true;
            }
        }

        return needsRefill;
    }

    public bool CanClearGroup(List<HexPiece> group)
    {
        foreach (HexPiece hex in group)
        {
            if (!hex.CanClear())
            {
                return false;
            }
        }

        return true;
    }

    public bool ClearPiece(int x, int y)
    {
        if (pieces[x, y].CanClear())
        {
            pieces[x, y].ClearComponent.Clear();
            //var vecPos = GetWorldPosition(x, y);
            //int score = pieces[x, y].score;
            //Text textScore = GameObject.Find("ScoreText").GetComponent<Text>();
            //textScore.text = score.ToString();
            //textScore.transform.position = vecPos;

            ClearObstacle(x, y);
            selectedHexGroup.HexGroup = null;
            return true;
        }
        return false;
    }

    public void ClearObstacle(int x, int y)
    {
        HexPiece toClear;

        if (x % 2 == 0)
        {
            toClear = GetHexPiece(x - 1, y - 1);
            if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
            {
                toClear.ClearComponent.Clear();
            }
            toClear = GetHexPiece(x + 1, y - 1);
            if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
            {
                toClear.ClearComponent.Clear();
            }
        }
        else
        {
            toClear = GetHexPiece(x - 1, y + 1);
            if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
            {
                toClear.ClearComponent.Clear();
            }
            toClear = GetHexPiece(x + 1, y + 1);
            if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
            {
                toClear.ClearComponent.Clear();
            }
        }

        toClear = GetHexPiece(x, y - 1);
        if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
        {
            toClear.ClearComponent.Clear();
        }
        toClear = GetHexPiece(x, y + 1);
        if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
        {
            toClear.ClearComponent.Clear();
        }

        toClear = GetHexPiece(x - 1, y);
        if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
        {
            toClear.ClearComponent.Clear();
        }
        toClear = GetHexPiece(x + 1, y);
        if (toClear && toClear.Type == PieceType.OBSTACLE && toClear.CanClear())
        {
            toClear.ClearComponent.Clear();
        }
    }

    public void GameOver(bool win)
    {
        touchAction.state = ActionState.DISABLED;
        if (win)
        {
            gameState = GameState.LEVEL_DONE;
            StartCoroutine(ClearingSpecial());
        }
        else
        {
            gameState = GameState.DONE_CLEARING;
        }
    }

    public List<HexPiece> GetPiecesOfType(PieceType type)
    {
        List<HexPiece> piecesOfType = new List<HexPiece>();

        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].Type == type)
                {
                    piecesOfType.Add(pieces[x, y]);
                }
            }
        }

        return piecesOfType;
    }

    public void setTouchActionState(ActionState state) {
        touchAction.state = state;
    }

    public bool IsSpecialPiece(HexPiece piece)
    {
        return piece.Type == PieceType.COLUMN_CLEAR || piece.Type == PieceType.DIAGONAL_RIGHT || piece.Type ==  PieceType.DIAGONAL_LEFT;
    }

    public void TransformRandomPiece(HexPiece piece)
    {
        piece.nextType = (PieceType)Random.Range((int)PieceType.COLUMN_CLEAR, (int)PieceType.DIAGONAL_RIGHT + 1);
        ChangePieceType(piece.X, piece.Y);
    }

    public void TransformRandomPiece()
    {
        bool found = false;

        while(!found)
        {
            int index = Random.Range(0, xDim * yDim);
            //Debug.Log(index + " (" + index % xDim + ", " + index / yDim + ")");
            if(!IsSpecialPiece(pieces[index % xDim, index / yDim]))
            {
                found = true;
                TransformRandomPiece(pieces[index % xDim, index / yDim]);
            }
        }
    }

    public bool ClearRandomSpecial()
    {
        List<HexPiece> specialPieces = new List<HexPiece>();
        for (int x = 0; x < xDim; x++)
        {
            for (int y = 0; y < yDim; y++)
            {
                if (pieces[x, y].Type == PieceType.COLUMN_CLEAR ||
                    pieces[x, y].Type == PieceType.DIAGONAL_LEFT ||
                    pieces[x, y].Type == PieceType.DIAGONAL_RIGHT )
                {
                    specialPieces.Add(pieces[x, y]);
                }
            }
        }

        if(specialPieces.Count > 0 )
        {
            HexPiece SelectedPiece = specialPieces[Random.Range(0, specialPieces.Count)];
            if(ClearPiece(SelectedPiece.X, SelectedPiece.Y))
            {
                gameState = GameState.FILLING;
                StartCoroutine(Fill());
                return true;
            }
        }
        return false;
    } 

    private IEnumerator ClearingSpecial()
    {
        while(gameState == GameState.FILLING || gameState == GameState.LEVEL_DONE)
        {
            yield return 0;
        }

        touchAction.state = ActionState.DISABLED;
        gameState = GameState.LEVEL_DONE;

        while (ClearRandomSpecial())
        {
            while (gameState == GameState.FILLING)
            {
                yield return 0;
            }
        }

        while (level.ConsumeMove())
        {
            TransformRandomPiece();
            yield return new WaitForSeconds(0.5f);
        }

        while (ClearRandomSpecial())
        {
            while (gameState == GameState.FILLING)
            {
                yield return 0;
            }
        }

        gameState = GameState.DONE_CLEARING;
    }
}
//note HACK_DESTROY : Je suspect qu'on ne peu pas detruire un object a l'interieur de la coroutine ClearingSpecial() qui elle creer des specials et qui demande a la grid de faire une autrre coroutine Fill() 
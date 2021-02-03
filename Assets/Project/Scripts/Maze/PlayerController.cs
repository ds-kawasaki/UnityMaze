using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private enum Direction
    {
        South,
        West,
        North,
        East,
    }
    private readonly Vector2Int[] directionForward = new Vector2Int[] {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
    };

    [SerializeField] private UnityEngine.UI.Text goalMessage;

    private MazeData mazeData { set; get; }
    private Vector2Int cellPos = Vector2Int.zero;
    private Direction direction = Direction.South;
    private Coroutine moving = null;
    private Coroutine turning = null;
    private Vector2Int nextCellPos = Vector2Int.zero;
    private Direction nextDirection = Direction.South;

    public void Setup(MazeData mData, Vector2Int pos)
    {
        this.mazeData = mData;
        this.cellPos = pos;
        this.direction = Direction.South;
        this.goalMessage.enabled = false;
    }

    public void TouchCallback(TouchInfo info)
    {
        if (this.goalMessage.enabled)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("scnTitle");
        }
        else
        {
            float katamuki = info.screenPoint.x * Screen.height / Screen.width;
            //Debug.Log(string.Format("Screen: {0} {1}  Touch: {2} {3}  katamuki {4}", Screen.width, Screen.height, info.screenPoint.x, info.screenPoint.y, katamuki));
            if (info.screenPoint.y > katamuki)
            {
                if (info.screenPoint.y > Screen.height - katamuki)
                {
                    //  上
                    //Debug.Log("Up");
                    MoveForward();
                }
                else
                {
                    //  左
                    //Debug.Log("Left");
                    TurnLeft();
                }
            }
            else
            {
                if (info.screenPoint.y > Screen.height - katamuki)
                {
                    //  右
                    //Debug.Log("Right");
                    TurnRight();
                }
                else
                {
                    //  下
                    //Debug.Log("Down");
                    MoveBack();
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        TouchInput.Started += TouchCallback;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            MoveForward();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            MoveBack();
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TurnLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            TurnRight();
        }
    }

    private void MoveForward()
    {
        CancelMove();
        CancelTurn();
        this.nextCellPos = this.cellPos + this.directionForward[(int)this.direction];
        if (this.mazeData.GetCell(this.nextCellPos) == MazeData.CellType.Wall) { return; }
        this.moving = StartCoroutine(Move(1.0f));
    }
    private void MoveBack()
    {
        CancelMove();
        CancelTurn();
        this.nextCellPos = this.cellPos + this.directionForward[(int)this.direction] * -1;
        if (this.mazeData.GetCell(this.nextCellPos) == MazeData.CellType.Wall) { return; }
        this.moving = StartCoroutine(Move(-1.0f));
    }
    private void TurnLeft()
    {
        CancelMove();
        CancelTurn();
        this.nextDirection = (Direction)(((int)this.direction + 4 - 1) % 4);
        this.turning = StartCoroutine(Turn(-90.0f));
    }
    private void TurnRight()
    {
        CancelMove();
        CancelTurn();
        this.nextDirection = (Direction)(((int)this.direction + 1) % 4);
        this.turning = StartCoroutine(Turn(90.0f));
    }

    private IEnumerator Move(float direction)
    {
        const float DURATION = 0.5f;

        float startTime = Time.time;
        while (true)
        {
            float now = Time.time - startTime;
            if (now > DURATION) { break; }

            transform.position += transform.forward * Time.deltaTime * direction / DURATION;

            yield return null;
        }

        transform.position = new Vector3(
                            (this.mazeData.Size.x / 2) * -1.0f + this.nextCellPos.x,
                            0.0f,
                            (this.mazeData.Size.y / 2) * -1.0f + this.nextCellPos.y
                            );
        this.cellPos = this.nextCellPos;
        if (this.mazeData.GetCell(this.cellPos) == MazeData.CellType.Goal)
        {
            this.goalMessage.enabled = true;
        }
        this.moving = null;
    }
    private IEnumerator Turn(float direction)
    {
        const float DURATION = 0.5f;

        float startTime = Time.time;
        while (true)
        {
            float now = Time.time - startTime;
            if (now > DURATION) { break; }

            transform.Rotate(0.0f, direction*Time.deltaTime / DURATION, 0.0f);

            yield return null;
        }

        transform.rotation = Quaternion.Euler(new Vector3(0.0f, (float)this.nextDirection * 90.0f, 0.0f));
        this.direction = this.nextDirection;
        this.turning = null;
    }

    private void CancelMove()
    {
        if (this.moving == null) { return; }

        StopCoroutine(this.moving);

        transform.position = new Vector3(
                            (this.mazeData.Size.x / 2) * -1.0f + this.nextCellPos.x,
                            0.0f,
                            (this.mazeData.Size.y / 2) * -1.0f + this.nextCellPos.y
                            );
        this.cellPos = this.nextCellPos;

        this.moving = null;
    }
    private void CancelTurn()
    {
        if (this.turning == null) { return; }

        StopCoroutine(this.turning);

        transform.rotation = Quaternion.Euler(new Vector3(0.0f, (float)this.nextDirection * 90.0f, 0.0f));
        this.direction = this.nextDirection;

        this.turning = null;
    }
}

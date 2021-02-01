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
    private Vector2Int nextCellPos = Vector2Int.zero;
    private Direction nextDirection = Direction.South;

    public void Setup(MazeData mData, Vector2Int pos)
    {
        this.mazeData = mData;
        this.cellPos = pos;
        this.direction = Direction.South;
        this.goalMessage.enabled = false;
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CancelMove();
            this.nextCellPos = this.cellPos + this.directionForward[(int)this.direction];
            if (this.mazeData.GetCell(this.nextCellPos) == MazeData.CellType.Wall) { return; }
            this.moving = StartCoroutine(Move(1.0f));
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CancelMove();
            this.nextCellPos = this.cellPos + this.directionForward[(int)this.direction] * -1;
            if (this.mazeData.GetCell(this.nextCellPos) == MazeData.CellType.Wall) { return; }
            this.moving = StartCoroutine(Move(-1.0f));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CancelMove();
            this.nextDirection = (Direction)(((int)this.direction + 4 - 1) % 4);
            this.moving = StartCoroutine(Rot(-90.0f));
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CancelMove();
            this.nextDirection = (Direction)(((int)this.direction + 1) % 4);
            this.moving = StartCoroutine(Rot(90.0f));
        }
    }

    IEnumerator Move(float direction)
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
    IEnumerator Rot(float direction)
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
        this.moving = null;
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
        transform.rotation = Quaternion.Euler(new Vector3(0.0f, (float)this.nextDirection * 90.0f, 0.0f));
        this.cellPos = this.nextCellPos;
        this.direction = this.nextDirection;

        this.moving = null;
    }
}

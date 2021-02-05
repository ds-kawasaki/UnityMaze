using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WallExtendMapGenerator : MonoBehaviour
{
    private enum Direction
    {
        Up,
        Right,
        Down,
        Left,
    }

    /// <summary>
    /// 壁のプレハブ 
    /// </summary>
    [SerializeField] private GameObject boxObjPrefab;
    /// <summary>
    /// 壁配置の基準オブジェクト
    /// </summary>
    [SerializeField] private GameObject boxesParentObj;
    /// <summary>
    /// 生成間隔
    /// </summary>
    [SerializeField] private float span = 1.0f;
    /// <summary>
    /// 壁升目のサイズ（最外周の壁を含む）
    /// </summary>
    [SerializeField] private Vector2Int mapCellSize;
    /// <summary>
    /// 壁升目の状態（最外周の壁を含む）
    /// </summary>
    private int[,] cells;
    /// <summary>
    /// 現在拡張中の壁情報
    /// </summary>
    private Stack<Vector2Int> currentWallCells = new Stack<Vector2Int>();
    /// <summary>
    /// 壁拡張を行う開始升の情報
    /// </summary>
    private List<Vector2Int> startCells = new List<Vector2Int>();

    private const int CELL_TYPE_YUKA = 0;
    private const int CELL_TYPE_WALL = 1;
    private const int CELL_TYPE_START = 2;
    private const int CELL_TYPE_GOAL = 3;


    private void Awake()
    {
        Init(this.mapCellSize);

        // 壁升目の生成
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                switch (this.cells[x, y])
                {
                    case CELL_TYPE_WALL:
                        GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
                        g.transform.position = new Vector3(
                            (this.mapCellSize.x / 2) * -1.0f + x,
                            0.5f,
                            (this.mapCellSize.y / 2) * -1.0f + y
                            );
                        break;
                }
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Generate());
    }

    private void Init(Vector2Int size)
    {
        this.mapCellSize = size;

        // 升目は奇数
        if ((this.mapCellSize.x & 1) == 0) { this.mapCellSize.x--; }
        if ((this.mapCellSize.y & 1) == 0) { this.mapCellSize.y--; }
        // 最小5x5
        if (this.mapCellSize.x < 5) { this.mapCellSize.x = 5; }
        if (this.mapCellSize.y < 5) { this.mapCellSize.y = 5; }

        this.cells = new int[this.mapCellSize.x, this.mapCellSize.y];
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                if (x == 0 || y == 0 || x == mapCellSize.x - 1 || y == mapCellSize.y - 1)
                {
                    this.cells[x, y] = CELL_TYPE_WALL;
                }
                else
                {
                    this.cells[x, y] = CELL_TYPE_YUKA;

                    // 外周ではない偶数升を壁伸ばし開始点に登録
                    if (x % 2 == 0 && y % 2 == 0)
                    {
                        this.startCells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        //  開始点をシャッフル
        this.startCells = this.startCells.OrderBy(i => System.Guid.NewGuid()).ToList();
    }

    private IEnumerator Generate()
    {
        yield return new WaitForSeconds(2.0f);

        var rnd = new System.Random();
        Queue<Vector2Int> recurrence = new Queue<Vector2Int>();
        foreach(var startCell in this.startCells)
        {
            // 既に壁の場合はスキップ 
            if (this.cells[startCell.x, startCell.y] == CELL_TYPE_WALL)
            {
                continue;
            }

            this.currentWallCells.Clear();

            recurrence.Clear();
            recurrence.Enqueue(startCell);
            while (recurrence.Count > 0)
            {
                Vector2Int now = recurrence.Dequeue();

                // 伸ばせる方向の判定 
                var directions = CheckDirection(now);
                if (directions.Count > 0)
                {
                    SetWall(now);
                    yield return new WaitForSeconds(this.span / 2);

                    // 伸ばせる方向からランダムな方向に伸ばす
                    var next = now;
                    var isPath = false;
                    switch (directions[rnd.Next(directions.Count)])
                    {
                        case Direction.Up:
                            isPath = (this.cells[now.x, now.y - 2] == CELL_TYPE_YUKA);
                            next.y--; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            next.y--; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            break;
                        case Direction.Down:
                            isPath = (this.cells[now.x, now.y + 2] == CELL_TYPE_YUKA);
                            next.y++; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            next.y++; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            break;
                        case Direction.Left:
                            isPath = (this.cells[now.x - 2, now.y] == CELL_TYPE_YUKA);
                            next.x--; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            next.x--; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            break;
                        case Direction.Right:
                            isPath = (this.cells[now.x + 2, now.y] == CELL_TYPE_YUKA);
                            next.x++; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            next.x++; SetWall(next);
                            yield return new WaitForSeconds(this.span / 2);
                            break;
                    }
                    if (isPath)
                    {
                        // 伸ばした先が床の場合は伸ばし続ける
                        recurrence.Enqueue(next);
                    }
                }
                else
                {
                    // 伸ばせる方向がなくなったら途中から再開
                    var before = this.currentWallCells.Pop();
                    recurrence.Enqueue(before);
                }
            }
        }

        // スタートとゴール位置（暫定で決め打ち）
        this.cells[1, 1] = CELL_TYPE_START;
        this.cells[this.mapCellSize.x - 2, this.mapCellSize.y - 2] = CELL_TYPE_GOAL;

        yield return new WaitForSeconds(this.span * 4);

        Debug.Log("End");
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += MazeSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnMaze");
    }

    private IEnumerator MakeBox(int x, int y)
    {
        GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
        g.transform.position = new Vector3(
            (this.mapCellSize.x / 2) * -1.0f + x,
            -0.5f,
            (this.mapCellSize.y / 2) * -1.0f + y
            );

        float startTime = Time.time;
        while (true)
        {
            float now = Time.time - startTime;
            if (now > this.span) { break; }

            g.transform.Translate(0.0f, Time.deltaTime / this.span, 0.0f);

            float nowScale = 0.3f + ((1.0f - 0.3f) * now / this.span);
            g.transform.localScale = new Vector3(nowScale, nowScale, nowScale);

            yield return null;
        }

        g.transform.position = new Vector3(
            (this.mapCellSize.x / 2) * -1.0f + x,
            0.5f,
            (this.mapCellSize.y / 2) * -1.0f + y
            );
        g.transform.localScale = Vector3.one;
    }


    private void SetWall(Vector2Int cell)
    {
        if (this.cells[cell.x, cell.y] == CELL_TYPE_YUKA)
        {
            StartCoroutine(MakeBox(cell.x, cell.y));
        }
        this.cells[cell.x, cell.y] = CELL_TYPE_WALL;
        if (cell.x % 2 == 0 && cell.y % 2 == 0)
        {
            this.currentWallCells.Push(cell);
        }
    }
    private List<Direction> CheckDirection(Vector2Int cell)
    {
        var result = new List<Direction>();
        if (this.cells[cell.x, cell.y - 1] == CELL_TYPE_YUKA && !IsCurrentWall(cell.x, cell.y - 2))
        {
            result.Add(Direction.Up);
        }
        if (this.cells[cell.x, cell.y + 1] == CELL_TYPE_YUKA && !IsCurrentWall(cell.x, cell.y + 2))
        {
            result.Add(Direction.Down);
        }
        if (this.cells[cell.x - 1, cell.y] == CELL_TYPE_YUKA && !IsCurrentWall(cell.x - 2, cell.y))
        {
            result.Add(Direction.Left);
        }
        if (this.cells[cell.x + 1, cell.y] == CELL_TYPE_YUKA && !IsCurrentWall(cell.x + 2, cell.y))
        {
            result.Add(Direction.Right);
        }
        return result;
    }
    private bool IsCurrentWall(int x, int y)
    {
        return this.currentWallCells.Contains(new Vector2Int(x, y));
    }



    private void MazeSceneLoaded(UnityEngine.SceneManagement.Scene next, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        var obj = GameObject.FindWithTag("MazeManager");
        if (obj != null)
        {
            var mazeMgr = obj.GetComponent<MazeManager>();
            if (mazeMgr != null)
            {
                //   データ受け渡し 
                var mazeData = new MazeData(new Vector2Int(this.mapCellSize.x - 2, this.mapCellSize.y - 2));
                for (int y = 1; y < this.mapCellSize.y - 1; ++y)
                {
                    for (int x = 1; x < this.mapCellSize.x - 1; ++x)
                    {
                        MazeData.CellType mCell = MazeData.CellType.Yuka;
                        switch (this.cells[x, y])
                        {
                            case CELL_TYPE_WALL:
                                mCell = MazeData.CellType.Wall;
                                break;

                            case CELL_TYPE_START:
                                mCell = MazeData.CellType.Start;
                                break;

                            case CELL_TYPE_GOAL:
                                mCell = MazeData.CellType.Goal;
                                break;

                            default:
                                mCell = MazeData.CellType.Yuka;
                                break;
                        }

                        mazeData.SetCell(new Vector2Int(x - 1, y - 1), mCell);
                    }
                }

                mazeMgr.mazeData = mazeData;
            }

        }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= MazeSceneLoaded;
    }
}

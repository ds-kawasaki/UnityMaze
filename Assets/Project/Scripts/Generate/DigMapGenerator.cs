using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DigMapGenerator : MonoBehaviour
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
    /// 壁インスタンス（最外周の壁は含まない）
    /// </summary>
    private List<GameObject> boxes;
    /// <summary>
    /// 穴掘り開始候補のリスト
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
        this.boxes = new List<GameObject>(this.mapCellSize.x * this.mapCellSize.y);
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
                        this.boxes.Add(g);
                        break;
                    default:
                        this.boxes.Add(null);
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
                this.cells[x, y] = CELL_TYPE_WALL;
            }
        }
    }

    private IEnumerator Generate()
    {
        yield return new WaitForSeconds(2.0f);

        // 判定用に外周は通路にしておく
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                if (x==0 || y==0 || x==this.mapCellSize.x-1 || y== this.mapCellSize.y-1)
                {
                    this.cells[x, y] = CELL_TYPE_YUKA;
                }
            }
        }

        this.startCells.Clear();

        var rnd = new System.Random();
        Queue<Vector2Int> recurrence = new Queue<Vector2Int>();
        recurrence.Enqueue(new Vector2Int(1, 1));
        while (recurrence.Count > 0)
        {
            Vector2Int now = recurrence.Dequeue();
            while(true)
            {
                var direcsions = CheckDirection(now);
                if (direcsions.Count == 0)
                {
                    // 四方掘り進められない場合はループ抜ける
                    break;
                }

                bool canDig = (this.cells[now.x, now.y] == CELL_TYPE_WALL);
                SetYuka(now);
                if (canDig)
                {
                    yield return new WaitForSeconds(this.span / 2);
                }

                // 掘れる方向からランダムに２升掘り進む
                switch (direcsions[rnd.Next(direcsions.Count)])
                {
                    case Direction.Up:
                        now.y--; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        now.y--; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        break;
                    case Direction.Down:
                        now.y++; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        now.y++; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        break;
                    case Direction.Left:
                        now.x--; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        now.x--; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        break;
                    case Direction.Right:
                        now.x++; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        now.x++; SetYuka(now);
                        yield return new WaitForSeconds(this.span / 2);
                        break;
                }
            }

            if (this.startCells.Count > 0)
            {
                // 穴掘り開始候補からランダムに次の升を取り出す
                var idx = rnd.Next(this.startCells.Count);
                var next = this.startCells[idx];
                this.startCells.RemoveAt(idx);
                recurrence.Enqueue(next);
            }
        }


        // 判定用に外周を壁に戻す
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                if (x == 0 || y == 0 || x == this.mapCellSize.x - 1 || y == this.mapCellSize.y - 1)
                {
                    this.cells[x, y] = CELL_TYPE_WALL;
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

    private IEnumerator DeleteBox(GameObject g)
    {
        const float DURATION = 1.0f;

        float startTime = Time.time;
        while (true)
        {
            float now = Time.time - startTime;
            if (now > DURATION) { break; }

            g.transform.Translate(0.0f, -Time.deltaTime, 0.0f);

            float nowScale = 1.0f + ((0.3f - 1.0f) * now / DURATION);
            g.transform.localScale = new Vector3(nowScale, nowScale, nowScale);

            yield return null;
        }

        Destroy(g);
    }




    private List<Direction> CheckDirection(Vector2Int cell)
    {
        var result = new List<Direction>();
        if (this.cells[cell.x, cell.y - 1] == CELL_TYPE_WALL && this.cells[cell.x, cell.y - 2] == CELL_TYPE_WALL)
        {
            result.Add(Direction.Up);
        }
        if (this.cells[cell.x, cell.y + 1] == CELL_TYPE_WALL && this.cells[cell.x, cell.y + 2] == CELL_TYPE_WALL)
        {
            result.Add(Direction.Down);
        }
        if (this.cells[cell.x - 1, cell.y] == CELL_TYPE_WALL && this.cells[cell.x - 2, cell.y] == CELL_TYPE_WALL)
        {
            result.Add(Direction.Left);
        }
        if (this.cells[cell.x + 1, cell.y] == CELL_TYPE_WALL && this.cells[cell.x + 2, cell.y] == CELL_TYPE_WALL)
        {
            result.Add(Direction.Right);
        }
        return result;
    }

    private void SetYuka(Vector2Int cell)
    {
        if (this.cells[cell.x, cell.y] == CELL_TYPE_WALL)
        {
            var g = this.boxes[cell.x + cell.y * this.mapCellSize.x];
            if (g != null)
            {
                StartCoroutine(DeleteBox(g));
                this.boxes[cell.x + cell.y * this.mapCellSize.x] = null;
            }
        }
        this.cells[cell.x, cell.y] = CELL_TYPE_YUKA;
        if (cell.x % 2 == 1 && cell.y % 2 == 1)
        {
            this.startCells.Add(cell);
        }
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

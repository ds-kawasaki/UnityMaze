using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefeatStickMapGenerator : MonoBehaviour
{
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
                if (x == 0 || y == 0 || x == mapCellSize.x-1 || y == mapCellSize.y-1)
                {
                    this.cells[x, y] = CELL_TYPE_WALL;
                }
                else
                {
                    this.cells[x, y] = CELL_TYPE_YUKA;
                }
            }
        }
    }

    private IEnumerator Generate()
    {
        yield return new WaitForSeconds(2.0f);

        var rnd = new System.Random();
        for (int y = 2; y < this.mapCellSize.y-1; y += 2)
        {
            for (int x = 2; x < this.mapCellSize.x-1; x += 2)
            {
                // 棒を立てる 
                this.cells[x, y] = CELL_TYPE_WALL;
                StartCoroutine(MakeBox(x, y));

                yield return new WaitForSeconds(this.span);

                while (true)
                {
                    int direction = (y == 2) ? rnd.Next(4) : rnd.Next(3);

                    int wallX = x;
                    int wallY = y;
                    switch (direction)
                    {
                        default:
                        case 0:
                            ++wallX;
                            break;
                        case 1:
                            ++wallY;
                            break;
                        case 2:
                            --wallX;
                            break;
                        case 3:
                            --wallY;
                            break;
                    }

                    if (this.cells[wallX, wallY] != CELL_TYPE_WALL)
                    {
                        this.cells[wallX, wallY] = CELL_TYPE_WALL;
                        StartCoroutine(MakeBox(wallX, wallY));
                        break;
                    }
                }

                yield return new WaitForSeconds(this.span);
            }
        }

        // スタートとゴール位置（暫定で決め打ち）
        this.cells[1, 1] = CELL_TYPE_START;
        this.cells[this.mapCellSize.x-2, this.mapCellSize.y-2] = CELL_TYPE_GOAL;

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


    private void MazeSceneLoaded(UnityEngine.SceneManagement.Scene next, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        var obj = GameObject.FindWithTag("MazeManager");
        if (obj != null)
        {
            var mazeMgr = obj.GetComponent<MazeManager>();
            if (mazeMgr != null)
            {
                //   データ受け渡し 
                var mazeData = new MazeData(new Vector2Int(this.mapCellSize.x-2, this.mapCellSize.y-2));
                for (int y = 1; y < this.mapCellSize.y-1; ++y)
                {
                    for (int x = 1; x < this.mapCellSize.x-1; ++x)
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

                        mazeData.SetCell(new Vector2Int(x-1, y-1), mCell);
                    }
                }

                mazeMgr.mazeData = mazeData;
            }

        }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= MazeSceneLoaded;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// クラスタリング迷路生成
/// </summary>
public class ClustringMapGenerator : MonoBehaviour
{
    /// <summary>
    /// 壁のプレハブ 
    /// </summary>
    [SerializeField] private GameObject boxObjPrefab;
    /// <summary>
    /// クラスタ文字のプレハブ
    /// </summary>
    [SerializeField] private GameObject textObjPrefab;
    /// <summary>
    /// 壁配置の基準オブジェクト
    /// </summary>
    [SerializeField] private GameObject boxesParentObj;
    /// <summary>
    /// 生成間隔
    /// </summary>
    [SerializeField] private float span = 1.0f;
    /// <summary>
    /// クラスタ番号表示フラグ
    /// </summary>
    [SerializeField] private bool isDispCluster = true;
    /// <summary>
    /// 壁升目のサイズ（最外周の壁は含まない）
    /// </summary>
    [SerializeField] private Vector2Int mapCellSize;
    /// <summary>
    /// クラスタ升目のサイズ
    /// </summary>
    private Vector2Int clusterSize;
    /// <summary>
    /// 壁升目の状態（最外周の壁は含まない）
    /// </summary>
    private List<int> cells;
    /// <summary>
    /// 壁インスタンス（最外周の壁は含まない）
    /// </summary>
    private List<GameObject> boxes;
    /// <summary>
    /// 壊す（可能性のある）壁のランダム並び順
    /// </summary>
    private List<int> randWalls;


    private const int CELL_TYPE_YUKA = 0;
    private const int CELL_TYPE_WALL = 1;
    private const int CELL_TYPE_START = 2;
    private const int CELL_TYPE_GOAL = 3;
    private const int CELL_TYPE_TMP_WALL = 4;
    private const int CELL_TYPE_CLUSTER_OFFSET = 10;


    private void Awake()
    {
        Init(this.mapCellSize);

        // 壁升目の生成
        this.boxes = new List<GameObject>(this.mapCellSize.x * this.mapCellSize.y);
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                switch(this.cells[x + y * this.mapCellSize.x])
                {
                    case CELL_TYPE_WALL:
                    case CELL_TYPE_TMP_WALL:
                        GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
                        g.transform.position = new Vector3(
                            (this.mapCellSize.x / 2) * -1.0f + x,
                            0.5f,
                            (this.mapCellSize.y / 2) * -1.0f + y
                            );
                        this.boxes.Add(g);
                        break;

                    case CELL_TYPE_YUKA:
                        this.boxes.Add(null);
                        break;

                    default:
                        if (this.isDispCluster)
                        {
                            GameObject g2 = Instantiate(this.textObjPrefab, this.boxesParentObj.transform);
                            g2.transform.position = new Vector3(
                                (this.mapCellSize.x / 2) * -1.0f + x,
                                1.0f,
                                (this.mapCellSize.y / 2) * -1.0f + y
                                );
                            TextMesh t = g2.GetComponent<TextMesh>();
                            if (t != null)
                            {
                                int n = this.cells[x + y * this.mapCellSize.x] - CELL_TYPE_CLUSTER_OFFSET;
                                t.text = n.ToString();
                            }
                            this.boxes.Add(g2);
                        }
                        else
                        {
                            this.boxes.Add(null);
                        }
                        break;

                }
            }
        }

        // 最外周の壁を追加 
        for (int i = 0; i < this.mapCellSize.y + 2; ++i)
        {
            GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g.transform.position = new Vector3(
                ((this.mapCellSize.x+1) / 2) * -1.0f,
                0.5f,
                ((this.mapCellSize.y+1) / 2) * -1.0f + i
                );

            GameObject g2 = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g2.transform.position = new Vector3(
                ((this.mapCellSize.x + 1) / 2) * 1.0f,
                0.5f,
                ((this.mapCellSize.y + 1) / 2) * -1.0f + i
                );
        }
        for (int i = 0; i < this.mapCellSize.x; ++i)
        {
            GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g.transform.position = new Vector3(
                (this.mapCellSize.x / 2) * -1.0f + i,
                0.5f,
                ((this.mapCellSize.y + 1) / 2) * -1.0f
                );

            GameObject g2 = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g2.transform.position = new Vector3(
                (this.mapCellSize.x / 2) * -1.0f + i,
                0.5f,
                ((this.mapCellSize.y + 1) / 2) * 1.0f
                );
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

        this.clusterSize.x = (this.mapCellSize.x + 1) / 2;
        this.clusterSize.y = (this.mapCellSize.y + 1) / 2;


        int cellNum = this.mapCellSize.x * this.mapCellSize.y;
        this.cells = new List<int>(cellNum);
        for (int i=0; i<cellNum; ++i)
        {
            this.cells.Add(CELL_TYPE_WALL);
        }

        // 各クラスタ番号代入
        int clusterNum = this.clusterSize.x * this.clusterSize.y;
        for (int i = 0; i < clusterNum; ++i)
        {
            this.cells[GetCellFromCluster(i)] = i + CELL_TYPE_CLUSTER_OFFSET;
        }

        // クラスタ間の壁
        int wallNum = (this.mapCellSize.x * this.mapCellSize.y - 1) / 2;
        for (int i = 0; i < wallNum; ++i)
        {
            this.cells[GetCellFromWall(i)] = CELL_TYPE_TMP_WALL;
        }
        // クラスタ間の壁をランダムに壊すけど、すべてをなめるので配列にしておく
        this.randWalls = Enumerable.Range(0, wallNum)
            .Select(i => i)
            .OrderBy(i => System.Guid.NewGuid())
            .ToList<int>();
        
        //DebugLogCells();
        //Debug.Log(string.Join(", ", this.randWalls.Select(i => i.ToString())));
    }


    private IEnumerator Generate()
    {
        yield return new WaitForSeconds(2.0f);

        foreach (var w in this.randWalls)
        {
            var (p1, p2) = GetClusterJoinWall(w);
            var c1 = this.cells[GetCellFromCluster(p1)];
            var c2 = this.cells[GetCellFromCluster(p2)];

            //Debug.Log(string.Format("w:{0} ({1}:{2})({3}:{4}) {5}:{6}", w, p1.x, p1.y, p2.x, p2.y, c1, c2));

            if (c1 == c2)
            {
                //  同じクラスタの間の壁は残す
                continue;
            }

            int wc = GetCellFromWall(w);
            //Debug.Log(string.Format("wc:{0} cell:{1}", wc, this.cells[wc]));
            if (this.cells[wc] == CELL_TYPE_TMP_WALL)
            {
                this.cells[wc] = CELL_TYPE_YUKA;
                // クラスタをつなぐ
                TuneCluster(c1, c2);

                if (this.boxes[wc] != null)
                {
                    StartCoroutine(DeleteBox(this.boxes[wc]));
                    this.boxes[wc] = null;
                }
            }

            yield return new WaitForSeconds(this.span);
        }

        // スタートとゴール位置（暫定で決め打ち）
        this.cells[0] = CELL_TYPE_START;
        this.cells[this.cells.Count-1] = CELL_TYPE_GOAL;

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


    private void MazeSceneLoaded(UnityEngine.SceneManagement.Scene next, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        var obj = GameObject.FindWithTag("MazeManager");
        if (obj != null)
        {
            var mazeMgr = obj.GetComponent<MazeManager>();
            if (mazeMgr != null)
            {
                //   データ受け渡し 
                var mazeData = new MazeData(this.mapCellSize);
                for (int y = 0; y < this.mapCellSize.y; ++y)
                {
                    for (int x = 0; x < this.mapCellSize.x; ++x)
                    {
                        MazeData.CellType mCell = MazeData.CellType.Yuka;
                        switch (this.cells[x + y * this.mapCellSize.x])
                        {
                            case CELL_TYPE_WALL:
                            case CELL_TYPE_TMP_WALL:
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

                        mazeData.SetCell(new Vector2Int(x, y), mCell);
                    }
                }

                mazeMgr.mazeData = mazeData;
            }

        }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= MazeSceneLoaded;
    }



    private int GetCellFromCluster(int c)
    {
        return ((c % this.clusterSize.x) * 2)
            + (((c / this.clusterSize.x) * this.mapCellSize.x) * 2);
    }
    private int GetCellFromCluster(Vector2Int p)
    {
        return (p.x * 2)
            + ((p.y * this.mapCellSize.x) * 2);
    }
    private int GetCellFromWall(int w)
    {
        return (w * 2) + 1;
    }
    private (Vector2Int, Vector2Int) GetClusterJoinWall(int w)
    {
        int width = this.mapCellSize.x;
        int y = w / width;
        int x = w % width;

        bool isYoko = x < ((width - 1) / 2);
        if (isYoko)
        {
            return (new Vector2Int(x, y), new Vector2Int(x+1, y));
        }
        else
        {
            x -= (width - 1) / 2;
            return (new Vector2Int(x, y), new Vector2Int(x, y+1));
        }
    }
    private void TuneCluster(int dst, int src)
    {
        if (dst < src)
        {
            //(dst, src) = (src, dst);
            int tmp = dst;
            dst = src;
            src = tmp;
        }

        int n = src - CELL_TYPE_CLUSTER_OFFSET;

        int clusterNum = this.clusterSize.x * this.clusterSize.y;
        for (int i = 0; i < clusterNum; ++i)
        {
            int cc = GetCellFromCluster(i);
            if (this.cells[cc] == dst)
            {
                this.cells[cc] = src;

                if (this.isDispCluster && this.boxes[cc] != null)
                {
                    TextMesh t = this.boxes[cc].GetComponent<TextMesh>();
                    t.text = n.ToString();
                }
            }
        }
    }


    private void DebugLogCells()
    {
#if true
        string tmp = "==========================================\n";
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                tmp += string.Format("{0,2} ", this.cells[x + y * this.mapCellSize.x]);
            }
            tmp += "\n";
        }
        Debug.Log(tmp);
#else
        Debug.Log("==========================================");
        for (int y = 0; y < this.mapCellSize.y; ++y)
        {
            string tmp = "";
            for (int x = 0; x < this.mapCellSize.x; ++x)
            {
                tmp += string.Format("{0,2} ", this.cells[x + y * this.mapCellSize.x]);
            }
            Debug.Log(tmp);
        }
#endif
    }
}

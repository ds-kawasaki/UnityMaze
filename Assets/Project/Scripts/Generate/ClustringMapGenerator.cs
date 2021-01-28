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
    public GameObject boxObjPrefab;
    /// <summary>
    /// クラスタ文字のプレハブ
    /// </summary>
    public GameObject textObjPrefab;
    /// <summary>
    /// 壁配置の基準オブジェクト
    /// </summary>
    public GameObject boxesParentObj;
    /// <summary>
    /// 生成間隔
    /// </summary>
    public float span = 1.0f;
    /// <summary>
    /// クラスタ番号表示フラグ
    /// </summary>
    public bool isDispCluster = true;
    /// <summary>
    /// 壁升目のサイズ（最外周の壁は含まない）
    /// </summary>
    public Vector2Int mapCellSize;
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
        Init(mapCellSize);

        // 壁升目の生成
        boxes = new List<GameObject>(mapCellSize.x * mapCellSize.y);
        for (int y = 0; y < mapCellSize.y; ++y)
        {
            for (int x = 0; x < mapCellSize.x; ++x)
            {
                switch(cells[x + y * mapCellSize.x])
                {
                    case CELL_TYPE_WALL:
                    case CELL_TYPE_TMP_WALL:
                        GameObject g = Instantiate(boxObjPrefab, boxesParentObj.transform);
                        g.transform.position = new Vector3(
                            (mapCellSize.x / 2) * -1.0f + x,
                            0.5f,
                            (mapCellSize.y / 2) * -1.0f + y
                            );
                        boxes.Add(g);
                        break;

                    case CELL_TYPE_YUKA:
                        boxes.Add(null);
                        break;

                    default:
                        if (isDispCluster)
                        {
                            GameObject g2 = Instantiate(textObjPrefab, boxesParentObj.transform);
                            g2.transform.position = new Vector3(
                                (mapCellSize.x / 2) * -1.0f + x,
                                1.0f,
                                (mapCellSize.y / 2) * -1.0f + y
                                );
                            TextMesh t = g2.GetComponent<TextMesh>();
                            if (t != null)
                            {
                                int n = cells[x + y * mapCellSize.x] - CELL_TYPE_CLUSTER_OFFSET;
                                t.text = n.ToString();
                            }
                            boxes.Add(g2);
                        }
                        else
                        {
                            boxes.Add(null);
                        }
                        break;

                }
            }
        }

        // 最外周の壁を追加 
        for (int i = 0; i < mapCellSize.y + 2; ++i)
        {
            GameObject g = Instantiate(boxObjPrefab, boxesParentObj.transform);
            g.transform.position = new Vector3(
                ((mapCellSize.x+1) / 2) * -1.0f,
                0.5f,
                ((mapCellSize.y+1) / 2) * -1.0f + i
                );

            GameObject g2 = Instantiate(boxObjPrefab, boxesParentObj.transform);
            g2.transform.position = new Vector3(
                ((mapCellSize.x + 1) / 2) * 1.0f,
                0.5f,
                ((mapCellSize.y + 1) / 2) * -1.0f + i
                );
        }
        for (int i = 0; i < mapCellSize.x; ++i)
        {
            GameObject g = Instantiate(boxObjPrefab, boxesParentObj.transform);
            g.transform.position = new Vector3(
                (mapCellSize.x / 2) * -1.0f + i,
                0.5f,
                ((mapCellSize.y + 1) / 2) * -1.0f
                );

            GameObject g2 = Instantiate(boxObjPrefab, boxesParentObj.transform);
            g2.transform.position = new Vector3(
                (mapCellSize.x / 2) * -1.0f + i,
                0.5f,
                ((mapCellSize.y + 1) / 2) * 1.0f
                );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Generate());
    }

    // Update is called once per frame
    void Update()
    {
    }


    private void Init(Vector2Int size)
    {
        mapCellSize = size;

        // 升目は奇数
        if ((mapCellSize.x & 1) == 0) { mapCellSize.x--; }
        if ((mapCellSize.y & 1) == 0) { mapCellSize.y--; }

        clusterSize.x = (mapCellSize.x + 1) / 2;
        clusterSize.y = (mapCellSize.y + 1) / 2;


        int cellNum = mapCellSize.x * mapCellSize.y;
        cells = new List<int>(cellNum);
        for (int i=0; i<cellNum; ++i)
        {
            cells.Add(CELL_TYPE_WALL);
        }

        // 各クラスタ番号代入
        int clusterNum = clusterSize.x * clusterSize.y;
        for (int i = 0; i < clusterNum; ++i)
        {
            cells[GetCellFromCluster(i)] = i + CELL_TYPE_CLUSTER_OFFSET;
        }

        // クラスタ間の壁
        int wallNum = (mapCellSize.x * mapCellSize.y - 1) / 2;
        for (int i = 0; i < wallNum; ++i)
        {
            cells[GetCellFromWall(i)] = CELL_TYPE_TMP_WALL;
        }
        // クラスタ間の壁をランダムに壊すけど、すべてをなめるので配列にしておく
        randWalls = Enumerable.Range(0, wallNum)
            .Select(i => i)
            .OrderBy(i => System.Guid.NewGuid())
            .ToList<int>();
        
        //DebugLogCells();
        //Debug.Log(string.Join(", ", randWalls.Select(i => i.ToString())));
    }


    IEnumerator Generate()
    {
        yield return new WaitForSeconds(2.0f);

        foreach (var w in randWalls)
        {
            var (p1, p2) = GetClusterJoinWall(w);
            var c1 = cells[GetCellFromCluster(p1)];
            var c2 = cells[GetCellFromCluster(p2)];

            //Debug.Log(string.Format("w:{0} ({1}:{2})({3}:{4}) {5}:{6}", w, p1.x, p1.y, p2.x, p2.y, c1, c2));

            if (c1 == c2)
            {
                //  同じクラスタの間の壁は残す
                continue;
            }

            int wc = GetCellFromWall(w);
            //Debug.Log(string.Format("wc:{0} cell:{1}", wc, cells[wc]));
            if (cells[wc] == CELL_TYPE_TMP_WALL)
            {
                cells[wc] = CELL_TYPE_YUKA;
                // クラスタをつなぐ
                TuneCluster(c1, c2);

                if (boxes[wc] != null)
                {
                    StartCoroutine(DeleteBox(boxes[wc]));
                    boxes[wc] = null;
                }
            }

            yield return new WaitForSeconds(span);
        }

        Debug.Log("End");
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += MazeSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnMaze");
    }

    IEnumerator DeleteBox(GameObject g)
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
                mazeMgr.mazeData = new MazeData(); //   データ受け渡し 
            }

        }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= MazeSceneLoaded;
    }



    private int GetCellFromCluster(int c)
    {
        return ((c % clusterSize.x) * 2)
            + (((c / clusterSize.x) * mapCellSize.x) * 2);
    }
    private int GetCellFromCluster(Vector2Int p)
    {
        return (p.x * 2)
            + ((p.y * mapCellSize.x) * 2);
    }
    private int GetCellFromWall(int w)
    {
        return (w * 2) + 1;
    }
    private (Vector2Int, Vector2Int) GetClusterJoinWall(int w)
    {
        int width = mapCellSize.x;
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

        int clusterNum = clusterSize.x * clusterSize.y;
        for (int i = 0; i < clusterNum; ++i)
        {
            int cc = GetCellFromCluster(i);
            if (cells[cc] == dst)
            {
                cells[cc] = src;

                if (isDispCluster && boxes[cc] != null)
                {
                    TextMesh t = boxes[cc].GetComponent<TextMesh>();
                    t.text = n.ToString();
                }
            }
        }
    }


    private void DebugLogCells()
    {
#if true
        string tmp = "==========================================\n";
        for (int y = 0; y < mapCellSize.y; ++y)
        {
            for (int x = 0; x < mapCellSize.x; ++x)
            {
                tmp += string.Format("{0,2} ", cells[x + y * mapCellSize.x]);
            }
            tmp += "\n";
        }
        Debug.Log(tmp);
#else
        Debug.Log("==========================================");
        for (int y = 0; y < mapCellSize.y; ++y)
        {
            string tmp = "";
            for (int x = 0; x < mapCellSize.x; ++x)
            {
                tmp += string.Format("{0,2} ", cells[x + y * mapCellSize.x]);
            }
            Debug.Log(tmp);
        }
#endif
    }
}

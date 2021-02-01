using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public MazeData mazeData { set; get; }

    [SerializeField] private GameObject boxObjPrefab;
    [SerializeField] private GameObject goalObjPrefab;
    [SerializeField] private GameObject boxesParentObj;
    [SerializeField] private GameObject playerObj;

    private void Awake()
    {
        this.mazeData = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        //MazeSceneLoadedはAwakeより後に呼ばれるので、こっちで初期化する

        if (this.mazeData == null)  // Mazeシーン単体テスト用 
        {
            var mData = new MazeData(new Vector2Int(3, 3));
            mData.SetCell(new Vector2Int(0, 0), MazeData.CellType.Start);
            mData.SetCell(new Vector2Int(0, 2), MazeData.CellType.Goal);
            mData.SetCell(new Vector2Int(0, 1), MazeData.CellType.Wall);
            mData.SetCell(new Vector2Int(1, 1), MazeData.CellType.Wall);
            this.mazeData = mData;
        }

        for (int y = 0; y < this.mazeData.Size.y; ++y)
        {
            for (int x = 0; x < this.mazeData.Size.x; ++x)
            {
                GameObject g = null;
                switch (this.mazeData.GetCell(new Vector2Int(x, y)))
                {
                    case MazeData.CellType.Wall:
                        g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
                        g.transform.position = new Vector3(
                            (this.mazeData.Size.x / 2) * -1.0f + x,
                            0.5f,
                            (this.mazeData.Size.y / 2) * -1.0f + y
                            );
                        break;
                    case MazeData.CellType.Start:
                        g = this.playerObj;
                        g.transform.position = new Vector3(
                            (this.mazeData.Size.x / 2) * -1.0f + x,
                            0.0f,
                            (this.mazeData.Size.y / 2) * -1.0f + y
                            );
                        PlayerController plCtl = this.playerObj.GetComponent<PlayerController>();
                        if (plCtl != null)
                        {
                            plCtl.Setup(this.mazeData, new Vector2Int(x, y));
                        }
                        break;
                    case MazeData.CellType.Goal:
                        g = Instantiate(this.goalObjPrefab, this.boxesParentObj.transform);
                        g.transform.position = new Vector3(
                            (this.mazeData.Size.x / 2) * -1.0f + x,
                            0.0f,
                            (this.mazeData.Size.y / 2) * -1.0f + y
                            );
                        break;
                    default:
                        break;
                }
            }
        }

        // 最外周の壁を追加 
        for (int i = 0; i < this.mazeData.Size.y + 2; ++i)
        {
            GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g.transform.position = new Vector3(
                ((this.mazeData.Size.x + 1) / 2) * -1.0f,
                0.5f,
                ((this.mazeData.Size.y + 1) / 2) * -1.0f + i
                );

            GameObject g2 = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g2.transform.position = new Vector3(
                ((this.mazeData.Size.x + 1) / 2) * 1.0f,
                0.5f,
                ((this.mazeData.Size.y + 1) / 2) * -1.0f + i
                );
        }
        for (int i = 0; i < this.mazeData.Size.x; ++i)
        {
            GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g.transform.position = new Vector3(
                (this.mazeData.Size.x / 2) * -1.0f + i,
                0.5f,
                ((this.mazeData.Size.y + 1) / 2) * -1.0f
                );

            GameObject g2 = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
            g2.transform.position = new Vector3(
                (this.mazeData.Size.x / 2) * -1.0f + i,
                0.5f,
                ((this.mazeData.Size.y + 1) / 2) * 1.0f
                );
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

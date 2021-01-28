using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeManager : MonoBehaviour
{
    public MazeData mazeData { set; get; }

    [SerializeField] private GameObject boxObjPrefab;
    [SerializeField] private GameObject boxesParentObj;

    private void Awake()
    {
        this.mazeData = null;
    }

    // Start is called before the first frame update
    void Start()
    {
        //MazeSceneLoadedはAwakeより後に呼ばれるので、こっちで初期化する

        if (this.mazeData == null)
        {
            var mData = new MazeData(new Vector2Int(3, 3));
            this.mazeData = mData;
        }

        for (int y = 0; y < this.mazeData.Size.y; ++y)
        {
            for (int x = 0; x < this.mazeData.Size.x; ++x)
            {
                switch (this.mazeData.GetCell(new Vector2Int(x, y)))
                {
                    case MazeData.CellType.Wall:
                        GameObject g = Instantiate(this.boxObjPrefab, this.boxesParentObj.transform);
                        g.transform.position = new Vector3(
                            (this.mazeData.Size.x / 2) * -1.0f + x,
                            0.5f,
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

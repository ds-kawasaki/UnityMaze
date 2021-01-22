using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClustringMapGenerator : MonoBehaviour
{
    public GameObject boxObjPrefab;
    public GameObject boxesObj;
    public float span = 1.0f;
    public Vector2Int mapCellSize;

    private void Awake()
    {
        for (int x=0; x<mapCellSize.x; ++x)
        {
            for (int y=0; y<mapCellSize.y; ++y)
            {
                GameObject g = Instantiate(boxObjPrefab, boxesObj.transform);
                g.transform.position = new Vector3(
                    (mapCellSize.x / 2) * -1.0f + x,
                    0.5f,
                    (mapCellSize.y / 2) * -1.0f + y
                    );
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Init();
        StartCoroutine("Generate");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init()
    {

    }

    IEnumerable Generate()
    {
        while (true)
        {
            yield return new WaitForSeconds(span);
        }
    }
}

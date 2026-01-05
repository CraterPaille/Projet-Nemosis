using UnityEngine;

public class Maps : MonoBehaviour
{
    private int w = 1;
    private int o = 0;
    public int[,] Map;
    public GameObject wallPrefab;
    int posX, posY;
    void Awake()
    {
        Map = new int[,]
        {
            { w,w,w,w,w,w,w,w,w,w,w,w,w,w,w },
            { w,o,o,o,o,o,o,o,o,o,o,o,o,o,w },
            { w,o,o,w,o,o,o,o,o,o,o,w,o,o,w },
            { w,o,o,o,o,o,o,w,o,o,o,o,o,o,w },
            { w,o,o,o,w,o,o,o,o,o,w,o,o,o,w },
            { w,o,o,o,o,o,o,o,o,o,o,o,o,o,w },
            { w,o,o,w,o,o,o,o,o,o,o,w,o,o,w },
            { w,o,o,o,o,w,o,o,o,w,o,o,o,o,w },
            { w,o,o,o,o,o,o,w,o,o,o,o,o,o,w },
            { w,o,o,o,w,o,o,o,o,o,w,o,o,o,w },
            { w,o,o,o,o,o,o,o,o,o,o,o,o,o,w },
            { w,o,o,w,o,o,o,o,o,o,o,w,o,o,w },
            { w,o,o,o,o,o,o,w,o,o,o,o,o,o,w },
            { w,o,o,o,o,o,o,o,o,o,o,o,o,o,w },
            { w,w,w,w,w,w,w,w,w,w,w,w,w,w,w }
        };
    }


    void Start()
    {
        for (int i = 0; i < Map.GetLength(0); i++)
        {
            for (int j = 0; j < Map.GetLength(1); j++)
            {
                if(Map[i, j] == w)
                {
                    // Instantiate a wall prefab at the position (i, j)
                    Instantiate(wallPrefab, new Vector3(-j, -i, 0), Quaternion.identity);
                }
            }
        }
    }

    void Update()
    {

    }
}

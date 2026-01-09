using UnityEngine;

public class UICommand : MonoBehaviour
{
    public GameObject m1, m2, m3;
    public GameObject g1, g2, g3;
    public GameObject l1, l2, l3;

    private GameObject[] Manche;
    private GameObject[] Garde;
    private GameObject[] Lame;

    private GameObject[][] Items;
    private GameObject[] SelectedItems = new GameObject[3];
    public bool init = true;

    public SpriteRenderer imageLame;

    public SpriteRenderer imageGarde;
    public SpriteRenderer imageManche;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Manche = new GameObject[] { m1, m2, m3 };
        Garde = new GameObject[] { g1, g2, g3 };
        Lame = new GameObject[] { l1, l2, l3 };
        
        Items = new GameObject[][] { Manche, Garde, Lame };
    }

    // Update is called once per frame
    void Update()
    {
        if (init)
        {
            SelectedItems[0] = Manche[Random.Range(0, 3)];
            SelectedItems[1] = Garde[Random.Range(0, 3)];
            SelectedItems[2] = Lame[Random.Range(0, 3)];

            init = false;
            RenderItems();
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            init = true;
        }


    }


    void RenderItems()
    {
        imageManche.sprite = SelectedItems[0].GetComponent<SpriteRenderer>().sprite;
        imageGarde.sprite = SelectedItems[1].GetComponent<SpriteRenderer>().sprite;
        imageLame.sprite = SelectedItems[2].GetComponent<SpriteRenderer>().sprite;    
        

    }
}

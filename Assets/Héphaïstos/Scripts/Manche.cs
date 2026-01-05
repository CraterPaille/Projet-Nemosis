using UnityEngine;

public class Manche : MonoBehaviour
{
    public GameObject craftingTable;
    public float type = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void OnMouseDown()
    {
        craftingTable.GetComponent<CraftingTable>().manche = this;
    }
    void Update()
    {
        
    }
}

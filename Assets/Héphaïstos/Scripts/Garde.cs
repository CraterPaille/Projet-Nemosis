using UnityEngine;

public class Garde : MonoBehaviour
{
    public GameObject craftingTable;
    public float type = 1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void OnMouseDown()
    {
        craftingTable.GetComponent<CraftingTable>().garde = this;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

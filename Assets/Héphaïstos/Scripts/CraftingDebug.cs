using UnityEngine;

using TMPro;
public class SpriteManager : MonoBehaviour
{
    public TextMeshProUGUI CraftingDebugText;
    public GameObject craftingTable;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        string mancheType = craftingTable.GetComponent<CraftingTable>().manche?.type.ToString();
        string gardeType = craftingTable.GetComponent<CraftingTable>().garde?.type.ToString();
        string lameType = craftingTable.GetComponent<CraftingTable>().lame?.type.ToString();
        CraftingDebugText.text = "M : " + mancheType + " G : " + gardeType + " L : " + lameType;
    }
}

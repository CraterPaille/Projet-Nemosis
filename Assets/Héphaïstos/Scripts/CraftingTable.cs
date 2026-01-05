using UnityEngine;

public class CraftingTable : MonoBehaviour
{
    public GameObject Ventes;
    public Commands Command;
    public Garde garde;
    public Manche manche;
    public Lame lame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void OnMouseDown()
    {
        if (garde != null && manche != null && lame != null)
        {
           
        
            if (garde.type == Command.Items[1] && manche.type == Command.Items[0] && lame.type == Command.Items[2])
            {
                Command.init = true;
                garde = null;
                manche = null;
                lame = null;
                Ventes.GetComponent<Ventes>().NbrVentes += 1;
            }
            else
            {
                Debug.Log("Crafting failed.");
            }
        
        }
    }
    // Update is called once per frame
    void Update()
    {

    }
}

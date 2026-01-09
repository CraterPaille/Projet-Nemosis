using UnityEngine;

public class UILame : MonoBehaviour
{

    public GameObject l1, l2, l3;

    public SpriteRenderer Lame1, Lame2;

    public GameObject Forge;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Forge != null && Forge.GetComponent<TimerForge>() != null)
        {
            var timerForge = Forge.GetComponent<TimerForge>();
            
            // Afficher la première lame (Slot 0)
            if (Lame1 != null)
            {
                if (timerForge.Lames.Length > 0 && timerForge.Lames[0] != null)
                {
                    int lameType = (int)timerForge.Lames[0].type;
                    
                    if (lameType == 1 && l1 != null)
                        Lame1.sprite = l1.GetComponent<SpriteRenderer>().sprite;
                    else if (lameType == 2 && l2 != null)
                        Lame1.sprite = l2.GetComponent<SpriteRenderer>().sprite;
                    else if (lameType == 3 && l3 != null)
                        Lame1.sprite = l3.GetComponent<SpriteRenderer>().sprite;
                    
                    Lame1.enabled = true;
                }
                else
                {
                    Lame1.enabled = false;
                }
            }
            
            // Afficher la deuxième lame (Slot 1)
            if (Lame2 != null)
            {
                if (timerForge.Lames.Length > 1 && timerForge.Lames[1] != null)
                {
                    int lameType = (int)timerForge.Lames[1].type;
                    
                    if (lameType == 1 && l1 != null)
                        Lame2.sprite = l1.GetComponent<SpriteRenderer>().sprite;
                    else if (lameType == 2 && l2 != null)
                        Lame2.sprite = l2.GetComponent<SpriteRenderer>().sprite;
                    else if (lameType == 3 && l3 != null)
                        Lame2.sprite = l3.GetComponent<SpriteRenderer>().sprite;
                    
                    Lame2.enabled = true;
                }
                else
                {
                    Lame2.enabled = false;
                }
            }
            

        }
    }
}

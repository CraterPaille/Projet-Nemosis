using UnityEngine;

public class Score : MonoBehaviour
{
    public int Cire = 0;
    public int Plume = 0;
    public TMPro.TextMeshProUGUI ScoreText;

    private string CireString;
    private string PlumeString;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Cire <= 1)
        {
            CireString = "Cire";
        }else
        {
            CireString = "Cires";
        }
        if(Plume <= 1)
        {
            PlumeString = "Plume";
        }else
        {
            PlumeString = "Plumes";
        }

        ScoreText.text = CireString +" : " + Cire + "\n" + PlumeString + " : " + Plume;
    }
}

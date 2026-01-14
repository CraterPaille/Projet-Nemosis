using UnityEngine;

public class Score : MonoBehaviour
{
    public int Cire = 0;
    public int Plume = 0;
    public TMPro.TextMeshProUGUI ScoreText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        ScoreText.text = "Cire : " + Cire + "\nPlumes : " + Plume;
    }
}

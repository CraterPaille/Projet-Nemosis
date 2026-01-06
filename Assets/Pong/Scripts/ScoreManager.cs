using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{

    public TextMeshProUGUI healthVillageText;
    public TextMeshProUGUI healthGodText;
    public GameObject Village;
    public GameObject God;
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    // Update is called once per frame
    void Update()
    {

     healthVillageText.text = "Health: " + Village.GetComponent<Village>().Health.ToString();
    healthGodText.text = "Health: " + God.GetComponent<BallGenerator>().Health.ToString();
    }
    

}

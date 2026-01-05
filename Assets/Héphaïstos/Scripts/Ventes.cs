using UnityEngine;
using TMPro;
public class Ventes : MonoBehaviour
{
    public float NbrVentes;
    public TextMeshProUGUI VentesText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        VentesText.text = "Ventes: " + NbrVentes;
    }
}

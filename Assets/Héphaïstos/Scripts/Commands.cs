using UnityEngine;
using TMPro;

public class Commands : MonoBehaviour
{
    public TextMeshProUGUI commandText;

    public float[] Items = { 0f, 0f, 0f }; // 1: manche, 2: garde, 3: lame

    public bool init = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (init)
        {
            for (int i = 0; i < Items.Length; i++)
            {
                Items[i] = Mathf.Floor(Random.Range(1f, 3f));
            }
            commandText.text = "Items to craft: \nManche type " + Items[0] + ",\n Garde type " + Items[1] + ",\n Lame type " + Items[2];
            init = false;
        }
    }
           
}

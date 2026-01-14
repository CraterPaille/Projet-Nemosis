using UnityEngine;
using System.Collections;

public class FinalDoor : MonoBehaviour
{

    public float openHeight = 0f;
    public GameObject Score;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Score.GetComponent<Score>().Cire + Score.GetComponent<Score>().Plume >= 5)
        {
            transform.position += new Vector3(0, openHeight * Time.deltaTime, 0);
            StartCoroutine(SpawnDelay());

        }
    }


    
    IEnumerator SpawnDelay() {
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}

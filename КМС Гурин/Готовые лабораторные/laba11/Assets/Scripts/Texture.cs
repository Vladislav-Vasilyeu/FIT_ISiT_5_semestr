using UnityEngine;

public class Moving : MonoBehaviour
{
   
    public Texture2D texture1;
    public Texture2D texture2;
    public Renderer Cube1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Cube1.GetComponent<Renderer>().material.mainTexture = texture2;
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        float red = Random.Range(0f, 1f);
        float green = Random.Range(0f, 1f);
        float blue = Random.Range(0f, 1f);

        Color randomcolor = new Color(red, green, blue);
        //collision.gameObject.GetComponent<Renderer>().material.color = randomcolor;
        collision.gameObject.GetComponent<Renderer>().material.mainTexture = texture1;
    }
}

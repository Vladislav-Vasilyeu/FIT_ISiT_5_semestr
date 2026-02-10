using UnityEngine;

public class BombDestroyer : MonoBehaviour
{
    public GameObject explosionEffect;  
    public AudioClip explosionSound;

    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject, 1);
            GetComponent<Renderer>().enabled = false;
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
            gameObject.GetComponent<AudioSource>().PlayOneShot(gameObject.GetComponent<AudioSource>().clip);
        }
    }


    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

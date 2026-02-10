using Unity.VisualScripting;
using UnityEngine;

public class ShellController : MonoBehaviour
{
    public float livetime = 10f;
    public GameObject explosionEffect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, livetime);
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("goat") || collision.gameObject.CompareTag("Player"))
        {
            GetComponent<Renderer>().enabled = false;
            if(explosionEffect != null)
            {
                Instantiate(explosionEffect, transform.position, Quaternion.identity);
                gameObject.GetComponent<AudioSource>().PlayOneShot(gameObject.GetComponent<AudioSource>().clip);
            }
            Destroy(gameObject, 0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

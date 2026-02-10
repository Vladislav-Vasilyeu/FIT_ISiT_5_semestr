using UnityEngine;

public class Trigger1 : MonoBehaviour
{
    public GameObject Stenka;
    private void OnTriggerStay(Collider other)
    {
        Stenka.transform.Rotate(0f, 10f, 0);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

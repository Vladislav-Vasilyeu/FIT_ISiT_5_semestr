using UnityEngine;

public class TrigerScript : MonoBehaviour
{
    public Light light1;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player")
            light1.enabled = true;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.name == "player")
            light1.enabled = false;
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

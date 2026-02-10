using UnityEngine;

public class RedPointScript : MonoBehaviour
{
    public Light RedPoint;

    public void OnTriggerEnter(Collider other)
    {
        RedPoint.intensity = 10000;
    }
    public void OnTriggerExit(Collider other)
    {
        RedPoint.intensity = 0;
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

using UnityEngine;

public class BluePointScript : MonoBehaviour
{
    public Light BluePoint;
    private void OnTriggerEnter(Collider other)
    {
        BluePoint.intensity = 10000;

    }
    private void OnTriggerExit(Collider other)
    {
        BluePoint.intensity = 0;
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

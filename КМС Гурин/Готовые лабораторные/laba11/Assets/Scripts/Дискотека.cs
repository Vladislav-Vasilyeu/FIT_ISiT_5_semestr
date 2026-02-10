using UnityEngine;

public class Дискотека : MonoBehaviour
{
    public Light Point1;
    public Light Point2;
    public Light Point3;
    public GameObject Cylinder;
    public float maxIntensity = 20f;
    public float pulseSpeed = 2f;
    public float rotationSpeed = 30f;


    private void OnTriggerStay(Collider other)
    {
        if(other.name == "player" ||  other.name == "robot")
        {
            float intensity = Mathf.Lerp(0, maxIntensity, Mathf.PingPong(Time.time * pulseSpeed, 1));
            Point1.intensity = intensity;
            Point2.intensity = intensity * Mathf.Sin(Time.time * pulseSpeed * 1.2f); 
            Point3.intensity = intensity * Mathf.Cos(Time.time * pulseSpeed * 0.8f);

            
            Cylinder.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
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

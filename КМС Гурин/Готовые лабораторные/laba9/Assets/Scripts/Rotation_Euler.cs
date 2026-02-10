using UnityEngine;

public class Rotation_Euler : MonoBehaviour
{
    public float speedX = 60f;
    public float speedZ = 90f;

    private Vector3 eulerRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eulerRotation = Vector3.zero;
    }

    
    void Update()
    {
        eulerRotation.x += speedX * Time.deltaTime;
        eulerRotation.z += speedZ * Time.deltaTime;

        transform.eulerAngles = eulerRotation;
    }
}

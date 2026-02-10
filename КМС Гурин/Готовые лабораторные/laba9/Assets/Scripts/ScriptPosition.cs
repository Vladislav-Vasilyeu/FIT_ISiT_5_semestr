using UnityEngine;

public class ScriptPosition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private float speedX = 5f;
    private float speedY = 5f;
    private float speedZ = -3f;

    void Update()
    {
        transform.position += new Vector3(speedX, speedY, speedZ) * Time.deltaTime;
    }
}

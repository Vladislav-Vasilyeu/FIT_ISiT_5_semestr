using UnityEngine;

public class robotMoving : MonoBehaviour
{
    public float speed = 6f;
    void Start()
    {
        
    }

    
    void Update()
    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.U))
            movement.z += 1f;
        if (Input.GetKey(KeyCode.J))
            movement.z -= 1f;
        if (Input.GetKey(KeyCode.H))
            movement.x -= 1f;
        if (Input.GetKey(KeyCode.K))
            movement.x += 1f;
        if(movement.magnitude > 0f)
        {
            movement= movement.normalized * speed * Time.deltaTime;
            transform.position += movement;
        }
    }
}

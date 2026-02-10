using UnityEngine;

public class Move_Key : MonoBehaviour
{
    public float speed = 6f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            movement.y += 1f;
        if (Input.GetKey(KeyCode.S))
            movement.y -= 1f;
        if (Input.GetKey(KeyCode.A))
            movement.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            movement.x += 1f;
        if(Input.GetKey(KeyCode.Q))
            movement.z += 1f;
        if(Input.GetKey(KeyCode.E))
            movement.z -= 1f;
        if(movement.magnitude > 0)
        {
            movement = movement.normalized* speed * Time.deltaTime;
            transform.position += movement;
        }
    }
}

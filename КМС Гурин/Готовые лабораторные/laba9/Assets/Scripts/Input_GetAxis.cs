using UnityEngine;

public class Input_GetAxis : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float rotateSpeed = 120f;
    private float pitch = 0f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        float mouseX = Input.GetAxis("Mouse X") * rotateSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed * Time.deltaTime;

        transform.Rotate(0f, mouseX, 0, Space.World);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, 0f, 90f);

        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, 0f);
    }
}

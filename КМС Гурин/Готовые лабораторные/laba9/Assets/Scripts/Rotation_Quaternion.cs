using UnityEngine;

public class Rotation_Quaternion : MonoBehaviour
{
    private Quaternion initialRotation;
    public float angle = 0f;
    public float speed = 80f;
    void Start()
    {
        initialRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        float deltaAngle = speed * Time.deltaTime;
        angle += deltaAngle;


        //Vector3 customAxis = new Vector3(1f, 0.5f, 1f).normalized;
        //Quaternion customRotation = Quaternion.AngleAxis(angle, customAxis);

        Quaternion rotX = Quaternion.AngleAxis(angle, Vector3.right);
        Quaternion rotZ = Quaternion.AngleAxis(angle, Vector3.forward);

        Quaternion totalRotation = initialRotation * rotX * rotZ;

        //transform.rotation = initialRotation * customRotation;
        transform.rotation = totalRotation;
    }
}

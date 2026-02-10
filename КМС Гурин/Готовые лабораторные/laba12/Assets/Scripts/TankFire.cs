using UnityEngine;

public class TankFire : MonoBehaviour
{
    public GameObject shellPrefab;
    public float shellSpeed = 80f;
    public float fireRate = 0.5f;
    public Transform firePoint;

    private float nextFireTime = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(firePoint == null)
        {
            firePoint = transform;
            firePoint.position += transform.forward * 2f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey(KeyCode.Space) && Time.time > nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }
    void Fire()
    {
        Vector3 spawnPos = transform.position + transform.forward * 2f;

        GameObject shell = Instantiate(shellPrefab, spawnPos, transform.rotation);

        Rigidbody rb = shell.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.linearVelocity = transform.forward * shellSpeed;
    }
}

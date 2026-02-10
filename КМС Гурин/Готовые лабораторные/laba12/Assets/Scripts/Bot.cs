using UnityEngine;
using System.Collections;

public class Bot : MonoBehaviour
{
    public float moveSpeed = 1f;         
    public float rotateSpeedTank = 1f;   
    public float rotateSpeedTurret = 1f;
    public float speedcore = 80f;
    public int countLife = 3;

    public Transform turret;             
    public Transform gun;
    public GameObject shellPrefab;


           

    private bool canShoot = true;
    
    private RaycastHit hit;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))  
        {
            
            Vector3 relativePos = other.transform.position - transform.position;
            float distance = Vector3.Distance(other.transform.position, transform.position);

            Quaternion newrot = Quaternion.LookRotation(relativePos);

            
            turret.rotation = Quaternion.Slerp(turret.rotation, newrot, Time.deltaTime * rotateSpeedTurret);


            if (Physics.Raycast(turret.position, turret.forward, out hit, 100f))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    // Вычисляем угол между направлением башни и направлением к игроку
                    Vector3 directionToPlayer = (hit.transform.position - turret.position).normalized;
                    float angle = Vector3.Angle(turret.forward, directionToPlayer);

                    // Стреляем только если угол маленький (башня почти точно наведена)
                    if (angle < 5f && canShoot)  // 5 градусов — точность прицела, подбери под себя (3–10)
                    {
                        StartCoroutine(botshoot());
                    }
                }
            }




            if (distance < 30f)
            {
                Quaternion tankTarget = Quaternion.LookRotation(relativePos);
                transform.rotation = Quaternion.Slerp(transform.rotation, tankTarget, Time.deltaTime * rotateSpeedTank);

                
                transform.position += transform.forward * moveSpeed * Time.deltaTime;
            }
        }
    }

    IEnumerator botshoot()
    {
        canShoot = false;

        
        Vector3 forwardofstvol = gun.transform.position + gun.forward * 5f;

        GameObject newcore = Instantiate(shellPrefab, forwardofstvol, gun.rotation);

        
        Rigidbody rb = newcore.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = gun.forward * speedcore;
        }

        yield return new WaitForSeconds(3f);

        canShoot = true;
    }




    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "core")
        {
            countLife--;
            if (countLife < 1)
            {
                Destroy(gameObject);
            }
        }
    }

    void Start()
    {
        if (turret == null)
            turret = transform.Find("Turret");
        if (gun == null)
            gun = turret.Find("Gun");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

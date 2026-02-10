using UnityEngine;

public class TankMoving : MonoBehaviour
{
    public Transform turret;
    public Transform gun;
    public AudioSource source_tank;
    bool isPlaying = false;

    public float tankMoveSpeed = 10f;          
    public float tankRotateSpeed = 80f;        
    public float turretRotateSpeed = 60f;      
    public float gunPitchSpeed = 40f;          

    
    public float gunMinAngle = -10f;   
    public float gunMaxAngle = 45f;    

    
    public GameObject bombPrefab;
    public int bombsCount = 10;             
    public float bombZoneRadius = 12f;      
    public float bombHeight = 30f;          
    public float forwardDistance = 30f;



    private void DropBombs()
    {
        Vector3 center = transform.position + transform.forward * forwardDistance;

        for (int i = 0; i < bombsCount; i++)
        {
            
            Vector2 randomCircle = Random.insideUnitCircle * bombZoneRadius;
            Vector3 spawnPos = center + new Vector3(randomCircle.x, bombHeight, randomCircle.y);

            GameObject bomb = Instantiate(bombPrefab, spawnPos, Quaternion.identity);
            Rigidbody rb = bomb.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.down * 20f;  
        }
    }
    void Start()
    {
        
        if (turret == null)
            turret = transform.Find("Turret");
        if (gun == null)
            gun = turret.Find("Gun");
        source_tank = GetComponent<AudioSource>();
    }

    void Update()
    {
        
        float forward = Input.GetAxis("Vertical");   
        transform.Translate(0, 0, forward * tankMoveSpeed * Time.deltaTime, Space.Self);

        
        float turn = Input.GetAxis("Horizontal");    
        transform.Rotate(0, turn * tankRotateSpeed * Time.deltaTime, 0);

        if((forward!=0 ||  turn!=0) && !isPlaying)
        {
            isPlaying = true;
            source_tank.Play();
        }
        if(forward==0 &&  turn==0 && isPlaying)
        {
            isPlaying = false;
            source_tank.Stop();
        }

        
        float mouseX = Input.GetAxis("Mouse X");
        turret.Rotate(0, mouseX * turretRotateSpeed * Time.deltaTime, 0);

        
        float mouseY = Input.GetAxis("Mouse Y");
        float pitchInput = -mouseY * gunPitchSpeed * Time.deltaTime; 

        
        float currentX = gun.localEulerAngles.x;

        
        if (currentX > 180f)
            currentX -= 360f;

        
        float newX = currentX + pitchInput;

        
        newX = Mathf.Clamp(newX, gunMinAngle, gunMaxAngle);

        
        gun.localRotation = Quaternion.Euler(newX, 0, 0);



        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropBombs();
        }

        
    }

    
}
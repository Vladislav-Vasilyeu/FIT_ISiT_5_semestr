using System;
using UnityEngine;

public class DoorTriger : MonoBehaviour
{
    public GameObject door1;
    public GameObject door2;
    public GameObject FlyingBar;
    public float flyRotationSpeed = 60f;
    public float flyMoveSpeed = 3f;
    private Vector3 flyStartPosition;

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "player")
        {
            door1.transform.position -= new Vector3(1f, 0, 0);
            door2.transform.position += new Vector3(1f, 0, 0);
            
        }
        
    }


    private void OnTriggerExit (Collider other)
    {
        if (other.name == "player")
        {
            door1.transform.position += new Vector3(1f, 0, 0);
            door2.transform.position -= new Vector3(1f, 0, 0);
        }
        if (other.name == "robot")
        {
            UpdateFlyingBar(false);
            
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if(other.name =="robot")
        {
            UpdateFlyingBar(true);
        }
    }


    private void UpdateFlyingBar(bool active)
    {
        if (active)
        {
            FlyingBar.transform.Rotate(Vector3.up * flyRotationSpeed * Time.deltaTime);
            FlyingBar.transform.position += Vector3.forward * flyMoveSpeed * Time.deltaTime; 
        }
        else
        {
            FlyingBar.transform.rotation = Quaternion.identity;
            FlyingBar.transform.position = flyStartPosition;
       
        }
    }


    void Start()
    {
        flyStartPosition = FlyingBar.transform.position;
    }



 
    void Update()
    {
        
    }
}

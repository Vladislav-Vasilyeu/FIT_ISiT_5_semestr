using UnityEngine;

public class SpotTrigger : MonoBehaviour
{
    public Light spot;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "player")
            spot.transform.Rotate(0, 10, 0);
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

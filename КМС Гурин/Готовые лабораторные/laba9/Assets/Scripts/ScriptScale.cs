using UnityEngine;

public class ScriptScale : MonoBehaviour
{
    public float scaleSpeedX = 2f;
    public float scaleSpeedY = 1.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newScale = transform.localScale;
        newScale.x += scaleSpeedX * Time.deltaTime;
        newScale.y += scaleSpeedY * Time.deltaTime;
        transform.localScale = newScale;
    }
}

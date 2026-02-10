using UnityEngine;
using UnityEngine.EventSystems;

public class щелчки : MonoBehaviour, IPointerClickHandler
{
    public int forse = 100;
    public void OnPointerClick(PointerEventData eventData)
    {
        
        float red = Random.Range(0f, 1f);
        float green = Random.Range(0f, 1f);
        float blue = Random.Range(0f, 1f);
        Color randomCol = new Color(red, green, blue);
        gameObject.GetComponent<Renderer>().material.color = randomCol;

        Vector3 target = eventData.pointerPressRaycast.worldPosition;
        Vector3 collid = Camera.main.transform.position;
        Vector3 distanse = (target - collid).normalized;
        
        Vector3 newVector = distanse * forse;

        gameObject.GetComponent<Rigidbody>().AddForceAtPosition(newVector, target);
    }


    

 

    
    
}

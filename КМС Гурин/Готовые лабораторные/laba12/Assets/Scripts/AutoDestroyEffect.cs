using UnityEngine;

public class AutoDestroyEffect : MonoBehaviour
{
    public float destroyDelay = 0f;

    private void OnEnable()
    {
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null )
        {
            Destroy(gameObject, ps.main.duration + destroyDelay);
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;


public class ParentSet : MonoBehaviour
{
 
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public GameObject parent;
    // Update is called once per frame
    void Update()
    {
        transform.position = parent.transform.position; 
    }
}

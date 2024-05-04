using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Bird;
 
    public bool isWater;
    public bool isTerrain;
    public GameObject Terrain1;
    public GameObject Terrain2;
    // Update is called once per frame
    public LayerMask layerMask;

    private GameObject lastHitObject;
    GameObject hitObject;

    void Start()
    {
        if (isTerrain)
        {
            lastHitObject = Terrain1;
            Terrain1.transform.position = new Vector3(Terrain1.transform.position.x, Terrain1.transform.position.y, -100);
        }
    }
    void Update()
    {
        // オブジェクトから下にRayを飛ばす
        if (isTerrain)
        {
            RaycastHit hit;
            if (Physics.Raycast(Bird.transform.position, Vector3.down, out hit, Mathf.Infinity, layerMask))
            {
                hitObject = hit.collider.gameObject;

                // 前回の当たったオブジェクトと違う場合
                if (hitObject != lastHitObject)
                {


                    ChangeTerrain();

                }
            }

        }
if (isWater)
        transform.position = new Vector3(transform.position.x, transform.position.y, Bird.transform.position.z);
    }

    void ChangeTerrain()
    {
        lastHitObject.transform.position = new Vector3(lastHitObject.transform.position.x, lastHitObject.transform.position.y, lastHitObject.transform.position.z + (980 * 2));
        lastHitObject = hitObject;
    }
}

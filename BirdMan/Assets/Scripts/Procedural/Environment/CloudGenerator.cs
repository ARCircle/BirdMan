using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    public GameObject spherePrefab;  // SphereのPrefab
    public int numberOfSpheres = 100;  // 生成するSphereの数

    // 雲全体のサイズを個別のVector3の範囲として指定できるように
    public Vector3 positionRangeMin = new Vector3(-10f, -10f, -10f); // 最小範囲
    public Vector3 positionRangeMax = new Vector3(10f, 10f, 10f); // 最大範囲

    // スケールの最小・最大範囲をVector3で指定
    public Vector3 scaleRangeMin = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 scaleRangeMax = new Vector3(1.5f, 1.5f, 1.5f);

    // 回転の最小・最大範囲をVector3で指定
    public Vector3 rotationRangeMin = new Vector3(0f, 0f, 0f);
    public Vector3 rotationRangeMax = new Vector3(360f, 360f, 360f);

    // 雲の生成処理
    void Start()
    {
        GenerateCloud();
    }

    void GenerateCloud()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            // X, Y, Z軸それぞれでランダムな位置を決定
            float randomX = Random.Range(positionRangeMin.x, positionRangeMax.x);
            float randomY = Random.Range(positionRangeMin.y, positionRangeMax.y);
            float randomZ = Random.Range(positionRangeMin.z, positionRangeMax.z);
            Vector3 randomPosition = new Vector3(randomX, randomY, randomZ);

            GameObject newSphere = Instantiate(spherePrefab, Vector3.zero,  Quaternion.identity, transform);
            newSphere.transform.localPosition = randomPosition;
            // X, Y, Z軸それぞれでランダムなスケールを設定
            float randomScaleX = Random.Range(scaleRangeMin.x, scaleRangeMax.x);
            float randomScaleY = Random.Range(scaleRangeMin.y, scaleRangeMax.y);
            float randomScaleZ = Random.Range(scaleRangeMin.z, scaleRangeMax.z);
            newSphere.transform.localScale = new Vector3(randomScaleX, randomScaleY, randomScaleZ);

            // X, Y, Z軸それぞれでランダムな回転を設定
            float randomRotationX = Random.Range(rotationRangeMin.x, rotationRangeMax.x);
            float randomRotationY = Random.Range(rotationRangeMin.y, rotationRangeMax.y);
            float randomRotationZ = Random.Range(rotationRangeMin.z, rotationRangeMax.z);
            newSphere.transform.rotation = Quaternion.Euler(randomRotationX, randomRotationY, randomRotationZ);

            // 生成したSphereをこのオブジェクトの子供に設定
            //newSphere.transform.parent = this.;
        }
    }
}

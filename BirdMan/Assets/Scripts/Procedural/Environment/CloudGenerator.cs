using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudGenerator : MonoBehaviour
{
    public GameObject spherePrefab;  // Sphere��Prefab
    public int numberOfSpheres = 100;  // ��������Sphere�̐�

    // �_�S�̂̃T�C�Y���ʂ�Vector3�͈̔͂Ƃ��Ďw��ł���悤��
    public Vector3 positionRangeMin = new Vector3(-10f, -10f, -10f); // �ŏ��͈�
    public Vector3 positionRangeMax = new Vector3(10f, 10f, 10f); // �ő�͈�

    // �X�P�[���̍ŏ��E�ő�͈͂�Vector3�Ŏw��
    public Vector3 scaleRangeMin = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3 scaleRangeMax = new Vector3(1.5f, 1.5f, 1.5f);

    // ��]�̍ŏ��E�ő�͈͂�Vector3�Ŏw��
    public Vector3 rotationRangeMin = new Vector3(0f, 0f, 0f);
    public Vector3 rotationRangeMax = new Vector3(360f, 360f, 360f);

    // �_�̐�������
    void Start()
    {
        GenerateCloud();
    }

    void GenerateCloud()
    {
        for (int i = 0; i < numberOfSpheres; i++)
        {
            // X, Y, Z�����ꂼ��Ń����_���Ȉʒu������
            float randomX = Random.Range(positionRangeMin.x, positionRangeMax.x);
            float randomY = Random.Range(positionRangeMin.y, positionRangeMax.y);
            float randomZ = Random.Range(positionRangeMin.z, positionRangeMax.z);
            Vector3 randomPosition = new Vector3(randomX, randomY, randomZ);

            GameObject newSphere = Instantiate(spherePrefab, Vector3.zero,  Quaternion.identity, transform);
            newSphere.transform.localPosition = randomPosition;
            // X, Y, Z�����ꂼ��Ń����_���ȃX�P�[����ݒ�
            float randomScaleX = Random.Range(scaleRangeMin.x, scaleRangeMax.x);
            float randomScaleY = Random.Range(scaleRangeMin.y, scaleRangeMax.y);
            float randomScaleZ = Random.Range(scaleRangeMin.z, scaleRangeMax.z);
            newSphere.transform.localScale = new Vector3(randomScaleX, randomScaleY, randomScaleZ);

            // X, Y, Z�����ꂼ��Ń����_���ȉ�]��ݒ�
            float randomRotationX = Random.Range(rotationRangeMin.x, rotationRangeMax.x);
            float randomRotationY = Random.Range(rotationRangeMin.y, rotationRangeMax.y);
            float randomRotationZ = Random.Range(rotationRangeMin.z, rotationRangeMax.z);
            newSphere.transform.rotation = Quaternion.Euler(randomRotationX, randomRotationY, randomRotationZ);

            // ��������Sphere�����̃I�u�W�F�N�g�̎q���ɐݒ�
            //newSphere.transform.parent = this.;
        }
    }
}

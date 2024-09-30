using System.Collections.Generic;
using UnityEngine;

public class MoveForward : MonoBehaviour
{
    public List<Material> targetMaterials; // �ݒ肵�����}�e���A���̃��X�g��Inspector�Ŏw��

    public float speed = 5f; // �ړ����x
    public float displayRange = 10.0f; // �\���͈�

    void Update()
    {
        // z�������Ɉ�葬�x�ňړ�
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Player�̃��[���h���W���V�F�[�_�[�ɓn��
        Vector3 playerPosition = transform.position;

        // �e�}�e���A���Ƀv���C���[�̈ʒu�Ɣ͈͂�ݒ�
        foreach (Material material in targetMaterials)
        {
            if (material != null)
            {
                material.SetVector("_PlayerPosition", playerPosition);
                material.SetFloat("_Range", displayRange);
            }
        }
    }
}

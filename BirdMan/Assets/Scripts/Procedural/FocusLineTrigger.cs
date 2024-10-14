using UnityEngine;

public class FocusLineTrigger : MonoBehaviour
{
    public Material focusLineMaterial;  // �W�����V�F�[�_�[���g���Ă���}�e���A�����w��

    void Start()
    {
        // ������Ԃł͏W�������\���ɐݒ�
        SetFocusLineVisible(false);
    }

    // �g���K�[�ɐN�������ۂɏW������\��
    void OnTriggerEnter(Collider other)
    {
        SetFocusLineVisible(true); // �W������\��
    }

    // �g���K�[����o���ۂɏW�������\��
    void OnTriggerExit(Collider other)
    {
        SetFocusLineVisible(false); // �W�������\��
    }

    // �}�e���A���� _ShowFocusLines ��ύX���郁�\�b�h
    private void SetFocusLineVisible(bool isVisible)
    {
        if (focusLineMaterial != null)
        {
            focusLineMaterial.SetFloat("_ShowFocusLines", isVisible ? 1.0f : 0.0f); // �V�F�[�_�[�̃t���O��ύX
        }
    }
}

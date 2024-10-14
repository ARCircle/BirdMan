using UnityEngine;

[ExecuteInEditMode]
public class FocusLinesEffect : MonoBehaviour
{
    public Material focusLinesMaterial; // �W�����V�F�[�_�[�����}�e���A��
    public float lineCount = 20f;
    public float speed = 1f;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (focusLinesMaterial != null)
        {
            // �V�F�[�_�[�̃p�����[�^��ݒ�
            focusLinesMaterial.SetFloat("_LineCount", lineCount);
            focusLinesMaterial.SetFloat("_Speed", speed);
            print("s");
            // ��ʑS�̂ɃG�t�F�N�g��K�p
            Graphics.Blit(src, dest, focusLinesMaterial);
        }
        else
        {
            // �}�e���A�����ݒ肳��Ă��Ȃ��ꍇ�A���̃e�N�X�`�������̂܂ܕ\��
            Graphics.Blit(src, dest);
        }
    }
}

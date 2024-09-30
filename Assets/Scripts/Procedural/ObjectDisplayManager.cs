using UnityEngine;

public class ObjectDisplayManager : MonoBehaviour
{
    public float rotationSpeed = 30f;  // ��]���x
    public Camera displayCamera;       // �I�u�W�F�N�g��\������J����
    public float scalePadding = 1.2f;  // �J�����Ɏ��܂�]�T�i�{���j

    public GameObject oldDisplayObject = null;

    void Update()
    {
        // �J�����̐e�I�u�W�F�N�g����]������
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }

    public void Display(GameObject newDisplayObject)
    {
        if (oldDisplayObject != null)
            Destroy(oldDisplayObject);

        // newDisplayObject�Ƃ��̎q�I�u�W�F�N�g�Ƀ��C���[��K�p
        SetLayerRecursively(newDisplayObject, LayerMask.NameToLayer("ObjectDisplay"));

        // �q�I�u�W�F�N�g���܂ނ��ׂẴ����_���[���擾
        Renderer[] renderers = newDisplayObject.GetComponentsInChildren<Renderer>();

        // Rigidbody�������Ă���ꍇ��isKinematic�ɐݒ�
        Rigidbody rb = newDisplayObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        newDisplayObject.transform.position = Vector3.zero;
        newDisplayObject.transform.rotation = Quaternion.identity;

        // �o�E���h�S�̂��v�Z���邽�߂̏����l
        Bounds combinedBounds = new Bounds();
        int i = 0;

        foreach (Renderer renderer in renderers)
        {
            print(i + renderer.name + renderer.bounds);
            i++;
            combinedBounds.Encapsulate(renderer.bounds); // �e�����_���[�̃o�E���h������

            // MaterialPropertyBlock���g�p���� _UseRange ��ݒ�
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(mpb);  // ���݂̃v���p�e�B�u���b�N���擾
            mpb.SetFloat("_UseRange", 0.0f); // �e�I�u�W�F�N�g���Ƃɐݒ�i��: �������j
            renderer.SetPropertyBlock(mpb);  // �v���p�e�B�u���b�N��ݒ�
        }

        Vector3 boundsSize = combinedBounds.size;
        print(boundsSize);

        // �I�u�W�F�N�g�̃X�P�[���𒲐����ăJ�������Ɏ��߂�
        float maxBoundSize = Mathf.Max(boundsSize.x, boundsSize.y, boundsSize.z);
        float distanceToCamera = Vector3.Distance(newDisplayObject.transform.position, displayCamera.transform.position);
        float fieldOfViewInRadians = displayCamera.fieldOfView * Mathf.Deg2Rad;
        float desiredScale = (distanceToCamera * Mathf.Tan(fieldOfViewInRadians / 2)) / maxBoundSize;

        newDisplayObject.transform.localScale = Vector3.one * desiredScale * scalePadding; // �X�P�[������

        // �o�E���h�̒��S�����[���h�̌��_�ɍ��킹��
        newDisplayObject.transform.position = Vector3.zero;
        Bounds combinedBounds2 = new Bounds();

        foreach (Renderer renderer in renderers)
        {
            combinedBounds2.Encapsulate(renderer.bounds); // �e�����_���[�̃o�E���h���ēx����
        }

        Vector3 boundsCenter = combinedBounds2.center;
        print(boundsCenter);
        newDisplayObject.transform.position -= boundsCenter;

        oldDisplayObject = newDisplayObject;
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer); // �q�I�u�W�F�N�g�ɂ��K�p
        }
    }
}

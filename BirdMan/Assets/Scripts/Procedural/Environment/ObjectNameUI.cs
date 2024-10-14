using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class ObjectNameUI2 : MonoBehaviour
{
    // �V���O���g���̃C���X�^���X
    public static ObjectNameUI2 Instance { get; private set; }

    public class TrackedObject
    {
        public GameObject ObjectToTrack { get; private set; } // �ǐՂ���I�u�W�F�N�g
        public TextMeshPro NameText { get; private set; } // �\������TextMeshPro (World Space�p)
        public MeshRenderer ObjectMeshRenderer { get; private set; } // �I�u�W�F�N�g��MeshRenderer�R���|�[�l���g
        public string rawNameString { get; private set; } // ���̖��O�e�L�X�g
        public int currentIndex { get; set; } // ���݂̓��͐i�����Ǘ�
        public Vector3 initialTopCenter; // ����̃o�E���f�B���O�{�b�N�X�㕔���S��ۑ�
        public Vector3 lastPosition; // �Ō�̈ʒu
        public bool isWithinRange; // �v���C���[�͈͓̔��ɂ��邩�ǂ������Ǘ�����t���O

        public TrackedObject(GameObject targetObject, TextMeshPro text, string name)
        {
            ObjectToTrack = targetObject;
            NameText = text;
            rawNameString = name;
            currentIndex = 0; // �����l��0
            ObjectMeshRenderer = targetObject.GetComponent<MeshRenderer>();
            isWithinRange = false; // ������Ԃ͔͈͊O

            // ���ׂĂ�Renderer�R���|�[�l���g��ΏۂɃo�E���h������
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                // ����Ƀo�E���f�B���O�{�b�N�X�̏㕔���S���v�Z���ĕۑ�
                Bounds combinedBounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size); // �ŏ��̃o�E���h����ɏ�����
                foreach (Renderer renderer in renderers)
                {
                    combinedBounds.Encapsulate(renderer.bounds); // �e�����_���[�̃o�E���h������
                }

                // �o�E���h�̏㕔���S���v�Z
                initialTopCenter = new Vector3(combinedBounds.center.x, combinedBounds.max.y, combinedBounds.center.z) + new Vector3(0, 0.5f, 0);
                lastPosition = ObjectToTrack.transform.position; // �����ʒu���ۑ�
            }
        }
    }

    public List<TrackedObject> trackedObjects = new List<TrackedObject>();

    void Awake()
    {
        // �V���O���g���̃C���X�^���X��ݒ�
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // �C���X�^���X�����ɑ��݂���ꍇ�͔j��
        }
    }

    void Update()
    {
        foreach (var trackedObject in trackedObjects)
        {
            if (trackedObject.ObjectToTrack != null && trackedObject.NameText != null)
            {
                UpdateUIPosition(trackedObject);
            }
        }
    }

    public void SetNameObject(GameObject targetObject)
    {
        if (trackedObjects.Exists(t => t.ObjectToTrack == targetObject))
            return;

        TextMeshPro newText = CreateTextUI();
        newText.name = targetObject.name + "Text";
        TrackedObject newTrackedObject = new TrackedObject(targetObject, newText, targetObject.name);
        trackedObjects.Add(newTrackedObject);

        UpdateUIPosition(newTrackedObject);

        newTrackedObject.NameText.text = newTrackedObject.rawNameString;
        newTrackedObject.NameText.gameObject.SetActive(false);
    }

    private TextMeshPro CreateTextUI()
    {
        // TextMeshPro (3D Text) �I�u�W�F�N�g�𐶐�
        GameObject textGameObject = new GameObject("ObjectNameText", typeof(TextMeshPro));

        // TextMeshPro�R���|�[�l���g�̎Q�Ƃ��擾
        TextMeshPro text = textGameObject.GetComponent<TextMeshPro>();

        // TextMeshPro�̊�{�ݒ�
        text.fontSize = 50;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = false;

        // �V�F�[�_�[���uDistance Field Overlay�v�ɐݒ肵�āA��ɑO�ʂɕ\������悤�ɂ���
        Material textMaterial = text.fontMaterial;
        textMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");

        // World Space�ɐݒ�
        text.transform.localScale = Vector3.one * 0.1f;
        text.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        return text;
    }

    private void UpdateUIPosition(TrackedObject trackedObject)
    {
       
        // �I�u�W�F�N�g�̈ړ������v�Z���A���̕�����TextMeshPro�I�u�W�F�N�g���ړ�������
        //Vector3 positionDifference = trackedObject.ObjectToTrack.transform.position - trackedObject.lastPosition;
        trackedObject.NameText.transform.position =new Vector3(0, trackedObject.initialTopCenter.y,0)+ trackedObject.ObjectToTrack.transform.position;

        // �Ō�̈ʒu���X�V
        trackedObject.lastPosition = trackedObject.ObjectToTrack.transform.position;
    }
}

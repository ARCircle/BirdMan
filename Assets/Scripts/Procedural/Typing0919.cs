using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

public class Typing0919 : MonoBehaviour
{
    public GameObject player; // �v���C���[�I�u�W�F�N�g���Q��
    private List<ObjectNameUI.TrackedObject> trackedObjects; // ObjectNameUI��TrackedObject���Q��

    public float detectionRadius = 10f; // �v���C���[�̌��m�͈�

    // �폜�Ώۂ̃C���f�b�N�X��ۑ����郊�X�g
    private List<int> indicesToRemove = new List<int>();
    public ObjectDisplayManager displayManager; // ObjectDisplayManager�̎Q��

    void Start()
    {
        // ObjectNameUI�̃C���X�^���X����TrackedObject���X�g���擾
        trackedObjects = ObjectNameUI.Instance.trackedObjects;

        UpdateAllTextColors();
    }

    void Update()
    {
        foreach (KeyControl keyControl in Keyboard.current.allKeys)
        {
            if (keyControl.wasPressedThisFrame)
            {
                HandleKeyPress(keyControl.displayName);
            }
           
        }

        // �I�u�W�F�N�g�̍폜�����̓��[�v�O�Ŏ��s
        RemoveObjects();
    }

    void HandleKeyPress(string key)
    {
        for (int i = 0; i < trackedObjects.Count; i++)
        {
            TextMeshPro nameText = trackedObjects[i].NameText;
            string rawNameString = trackedObjects[i].rawNameString; // ObjectNameUI��rawNameString���Q��

            // ���������L�[���͂ƈ�v���邩�m�F
            if (trackedObjects[i].currentIndex < rawNameString.Length &&
                rawNameString[trackedObjects[i].currentIndex].ToString().Equals(key, System.StringComparison.OrdinalIgnoreCase))
            {

                // �v���C���[�̔��a���ɂ��邩�m�F
                if (trackedObjects[i].ObjectToTrack == null)
                    break;
                if (IsWithinPlayerRadius(trackedObjects[i].ObjectToTrack))
                {
                    trackedObjects[i].currentIndex++;
                    UpdateTextColor(i, nameText, rawNameString);

                    // �S�Ă̕��������͂��ꂽ�ꍇ�͍폜���X�g�ɒǉ�
                    if (trackedObjects[i].currentIndex >= rawNameString.Length)
                    {
                        indicesToRemove.Add(i);
                    }
                }
            }
        }
    }

    bool IsWithinPlayerRadius(GameObject targetObject)
    {
        // �v���C���[�ƃ^�[�Q�b�g��XZ���ʂł̋������v�Z
        Vector3 playerPos = player.transform.position;
        Vector3 targetPos = targetObject.transform.position;

        float distanceXZ = Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(targetPos.x, targetPos.z));

        // ���������m�͈͓����m�F
        return distanceXZ <= detectionRadius;
    }

    void UpdateTextColor(int index, TextMeshPro nameText, string rawNameString)
    {
        string coloredText = "";
        for (int i = 0; i < rawNameString.Length; i++)
        {
            if (i < trackedObjects[index].currentIndex)
            {
                coloredText += $"<color=green>{rawNameString[i]}</color>";
            }
            else
            {
                coloredText += rawNameString[i];
            }
        }
        nameText.text = coloredText;
    }

    // �܂Ƃ߂ăI�u�W�F�N�g���폜
    void RemoveObjects()
    {
        // �폜�̓��X�g�̌�납��s�����ƂŃC���f�b�N�X�̃Y����h��
        indicesToRemove.Sort();
        indicesToRemove.Reverse();

        foreach (int index in indicesToRemove)
        {
            DestroyObject(index);
        }

        indicesToRemove.Clear(); // �폜���X�g���N���A
    }

    void DestroyObject(int index)
    {
        // �I�u�W�F�N�g��j��
        //  Destroy(trackedObjects[index].ObjectToTrack);
        displayManager.Display(trackedObjects[index].ObjectToTrack);
        Destroy(trackedObjects[index].NameText.gameObject); // NameText��GameObject���폜
        trackedObjects.RemoveAt(index);
    }

    void UpdateAllTextColors()
    {
        for (int i = 0; i < trackedObjects.Count; i++)
        {
            TextMeshPro nameText = trackedObjects[i].NameText;
            string rawNameString = trackedObjects[i].rawNameString; // rawNameString���Q��
            UpdateTextColor(i, nameText, rawNameString);
        }
    }
}

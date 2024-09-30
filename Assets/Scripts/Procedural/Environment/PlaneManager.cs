using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    [System.Serializable]
    public class PrefabWithHeight
    {
        public GameObject prefab;    // Prefab��GameObject
        public Vector2 heightRange;  // �����͈̔́imin, max�j
    }

    public GameObject player; // �v���C���[�I�u�W�F�N�g
    public GameObject planePrefab; // �v���t�@�u������Plane
    public float planeLength = 1000f; // Plane�̒����iz�����j
    public float planeWidth = 1000f;  // Plane�̕��ix�����j
    public int maxObjectsPerPlane = 5; // 1��Plane�ɔz�u����I�u�W�F�N�g�̍ő吔
    public float objectMinDistance = 1.5f; // �I�u�W�F�N�g�Ԃ̍ŏ�����
    public float xOffsetLimit = 4f; // Plane�̒�������x�����ŃI�u�W�F�N�g���u����͈́i�}xOffsetLimit�j
    public float preGenerateThreshold = 250f; // �v���C���[������臒l�𒴂���Ɨא�Plane�����O����
    public int maxPlaneDistance = 3; // �v���C���[���牽�O���b�h�ȏ㗣�ꂽPlane���폜���邩
    public List<PrefabWithHeight> objectPrefabs; // Tree��River��Prefab���X�g�ƍ����͈�

    private Dictionary<Vector2Int, GameObject> planeGrid = new Dictionary<Vector2Int, GameObject>(); // Plane���Ǘ����鎫��
    private Vector2Int currentGridPosition = Vector2Int.zero; // ���݂̃O���b�h�ʒu

    void Start()
    {
        // �����ʒu (0,0) ��Plane�͂��łɂ�����̂Ƃ��āA�v���C���[�̎��͂�5��Plane�𐶐�
        GenerateInitialPlanes();
    }

    // �v���C���[�̏����ʒu�𒆐S�ɁA���A���O�A�O�A�E�O�A�E��Plane�𐶐�
    void GenerateInitialPlanes()
    {
        // ���݂̃O���b�h�ʒu�� (0,0) �Ƃ��Ĉ���
        Vector2Int[] initialOffsets = {
            new Vector2Int(-1, 0),   // ��
            new Vector2Int(-1, 1),   // ���O
            new Vector2Int(0, 1),    // �O
            new Vector2Int(1, 1),    // �E�O
            new Vector2Int(1, 0)     // �E
        };

        // 5��Plane�𐶐�
        foreach (var offset in initialOffsets)
        {
            Vector2Int gridPosition = currentGridPosition + offset;
            if (!planeGrid.ContainsKey(gridPosition))
            {
                Vector3 planePosition = new Vector3(gridPosition.x * planeWidth, 0, gridPosition.y * planeLength);
                GameObject newPlane = Instantiate(planePrefab, planePosition, Quaternion.identity);
                planeGrid[gridPosition] = newPlane;

                // Plane��ɃI�u�W�F�N�g��z�u
                PlaceObjectsOnPlane(planePosition.z);
            }
        }
    }

    void Update()
    {
        // �v���C���[�̃O���b�h�ʒu���v�Z
        Vector2Int playerGridPosition = new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x / planeWidth),
            Mathf.FloorToInt(player.transform.position.z / planeLength)
        );

        // �v���C���[���אڃO���b�h�ɋ߂Â����ꍇ
        float playerOffsetX = Mathf.Abs(player.transform.position.x - currentGridPosition.x * planeWidth);
        float playerOffsetZ = Mathf.Abs(player.transform.position.z - currentGridPosition.y * planeLength);

        if (playerOffsetX >= preGenerateThreshold || playerOffsetZ >= preGenerateThreshold)
        {
            // ���݂̃O���b�h�ƗאڃO���b�h�����O�ɐ���
            PreGenerateAdjacentPlanes(playerGridPosition);
            currentGridPosition = playerGridPosition;

            // �Â�Plane���폜
            RemoveOldPlanes(playerGridPosition);
        }
    }

    // �אڂ���Plane�Ƃ��̂���ɗׂ�Plane�����O�ɐ���
    void PreGenerateAdjacentPlanes(Vector2Int playerGridPosition)
    {
        // ���݂̃O���b�h�ɉ����āA���͂�8�̗אڃO���b�h�Ƃ��ׂ̗�����
        for (int xOffset = -2; xOffset <= 2; xOffset++)
        {
            for (int yOffset = 0; yOffset <= 2; yOffset++)
            {
                Vector2Int adjacentGridPosition = new Vector2Int(playerGridPosition.x + xOffset, playerGridPosition.y + yOffset);

                // �אڂ���O���b�h�Ƃ��̗אڃO���b�h���܂���������Ă��Ȃ��ꍇ�ɐ���
                if (!planeGrid.ContainsKey(adjacentGridPosition))
                {
                    Vector3 planePosition = new Vector3(adjacentGridPosition.x * planeWidth, 0, adjacentGridPosition.y * planeLength);
                    GameObject newPlane = Instantiate(planePrefab, planePosition, Quaternion.identity);
                    planeGrid[adjacentGridPosition] = newPlane;

                    // �V����Plane��ɃI�u�W�F�N�g��z�u
                    PlaceObjectsOnPlane(planePosition.z);
                }
            }
        }
    }

    // �Â�Plane���폜���郍�W�b�N
    void RemoveOldPlanes(Vector2Int playerGridPosition)
    {
        List<Vector2Int> planesToRemove = new List<Vector2Int>();

        foreach (var plane in planeGrid)
        {
            // 2D�������v�Z
            float distance = Vector2.Distance(new Vector2(plane.Key.x, plane.Key.y), new Vector2(playerGridPosition.x, playerGridPosition.y));

            // �v���C���[�̌��݂̃O���b�h�ʒu����̋����� maxPlaneDistance �𒴂�����폜
            if (distance > maxPlaneDistance)
            {
                planesToRemove.Add(plane.Key);
            }
        }

        // �폜����Plane�����X�g����폜
        foreach (var planeKey in planesToRemove)
        {
            Destroy(planeGrid[planeKey]);
            planeGrid.Remove(planeKey);
        }
    }

    // Plane��ɃI�u�W�F�N�g��z�u����
    void PlaceObjectsOnPlane(float planeZPosition)
    {
        int objectCount = Random.Range(1, maxObjectsPerPlane + 1); // �����_���ɃI�u�W�F�N�g��������
        for (int i = 0; i < objectCount; i++)
        {
            // �����_���Ȉʒu�ƃI�u�W�F�N�g��I��
            Vector3 randomPosition = GetRandomPositionOnPlane(planeZPosition);
            PrefabWithHeight randomPrefabWithHeight = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
            GameObject randomPrefab = randomPrefabWithHeight.prefab;

            // �����������_���ɐݒ�
            float randomHeight = Random.Range(randomPrefabWithHeight.heightRange.x, randomPrefabWithHeight.heightRange.y);
            randomPosition.y = randomHeight;

            GameObject newObject = Instantiate(randomPrefab, randomPosition, Quaternion.identity);
            newObject.name = randomPrefab.name;

            newObject.SetActive(true);
        }
    }

    // Plane��Ń����_���ɃI�u�W�F�N�g�̔z�u�ʒu���擾
    Vector3 GetRandomPositionOnPlane(float planeZPosition)
    {
        Vector3 position;
        bool validPosition;

        do
        {
            float randomX = Random.Range(-xOffsetLimit, xOffsetLimit);
            float randomZ = Random.Range(planeZPosition - planeLength / 2, planeZPosition + planeLength / 2);
            position = new Vector3(randomX, 0, randomZ);

            validPosition = true;

            // ���̃I�u�W�F�N�g�Əd�Ȃ�Ȃ��悤�ɔz�u����
            foreach (var trackedObject in ObjectNameUI.Instance.trackedObjects)
            {
                if (Vector3.Distance(position, trackedObject.ObjectToTrack.transform.position) < objectMinDistance)
                {
                    validPosition = false;
                    break;
                }
            }

        } while (!validPosition);

        return position;
    }
}

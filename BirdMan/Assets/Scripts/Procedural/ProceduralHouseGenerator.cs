using UnityEngine;
using System.Collections.Generic;



public class ProceduralBuildingGenerator : MonoBehaviour
{
    [System.Serializable]

    public class WeightedPrefab
    {
        public GameObject prefab;
        public float weight;
    }
    [Header("�v���n�u�ݒ�")]
    [Tooltip("�ǂ̃v���n�u���X�g")]
    public List<WeightedPrefab> wallPrefabs;
    [Tooltip("�����̃v���n�u���X�g")]
    public List<WeightedPrefab> roofPrefabs;
    [Tooltip("���̃v���n�u���X�g")]
    public List<WeightedPrefab> floorPrefabs;
    [Tooltip("���E�̃v���n�u���X�g")]
    public List<WeightedPrefab> borderPrefabs;
    [Tooltip("���̃v���n�u���X�g")]
    public List<WeightedPrefab> windowPrefabs;
    [Tooltip("�h�A�̃v���n�u���X�g")]
    public List<WeightedPrefab> doorPrefabs;
    [Tooltip("�K�i�̃v���n�u���X�g")]
    public List<WeightedPrefab> stairPrefabs;
    [Tooltip("�p�̒��̃v���n�u���X�g")]
    public List<WeightedPrefab> cornerPrefabs;

    [Header("�����̐��@�ݒ�")]
    [Tooltip("�����̕��͈̔�")]
    public Vector2 widthRange = new Vector2(4.0f, 8.0f);
    [Tooltip("�e�K�̍����͈̔�")]
    public Vector2 heightRange = new Vector2(3.0f, 5.0f);
    [Tooltip("�����̉��s���͈̔�")]
    public Vector2 depthRange = new Vector2(4.0f, 8.0f);
    [Tooltip("�ő�K��")]
    [Range(1, 10)]
    public int maxFloors = 3;

    [Header("���z�X�^�C���ݒ�")]
    [Tooltip("���z�X�^�C���̑I��")]
    public ArchitecturalStyle style = ArchitecturalStyle.Modern;


    [Header("�f�o�b�O�ݒ�")]
   
    public bool isUnravel = false; // ���̃t���O��Inspector�ŃI���ɂ���ƁA�w�肳�ꂽ�I�u�W�F�N�g����A�N�e�B�u�ɂȂ�܂��B

    // ���z�X�^�C���̗񋓌^
    public enum ArchitecturalStyle
    {
        Modern,
        Victorian,
        Futuristic,
        // �K�v�ɉ����đ��̃X�^�C����ǉ�
    }

    // �e�I�u�W�F�N�g��ێ�����ϐ�
    private GameObject wallsParent;
    private GameObject cornersParent;
    private GameObject bordersParent;
    private GameObject floorsParent;
    private GameObject roofParent;
    private GameObject interiorParent;

    // ����������
    void Start()
    {
        // �e�I�u�W�F�N�g�̍쐬
        CreateParentObjects();

        // �����̐���
        GenerateBuilding();

        // �e�I�u�W�F�N�g�̈ʒu�����Z�b�g
        ResetParentObjectPositions();
    }
    void Update()
    {
        // isUnravel��true�̏ꍇ�A�w�肳�ꂽ�����ɍ����I�u�W�F�N�g���A�N�e�B�u�ɂ���
        if (isUnravel)
        {
            UnravelBuilding();
        }
    }

    /// <summary>
    /// �e�I�u�W�F�N�g���쐬���郁�\�b�h�B
    /// </summary>
    void CreateParentObjects()
    {
        wallsParent = new GameObject("Walls");
        wallsParent.transform.parent = this.transform;

        cornersParent = new GameObject("Corners");
        cornersParent.transform.parent = this.transform;

        bordersParent = new GameObject("Borders");
        bordersParent.transform.parent = this.transform;

        floorsParent = new GameObject("Floors");
        floorsParent.transform.parent = this.transform;

        roofParent = new GameObject("Roofs");
        roofParent.transform.parent = this.transform;

        interiorParent = new GameObject("Interiors");
        interiorParent.transform.parent = this.transform;
    }

    /// <summary>
    /// �e�I�u�W�F�N�g�̈ʒu�����Z�b�g���郁�\�b�h�B
    /// </summary>
    void ResetParentObjectPositions()
    {
        wallsParent.transform.localPosition = Vector3.zero;
        cornersParent.transform.localPosition = Vector3.zero;
        bordersParent.transform.localPosition = Vector3.zero;
        floorsParent.transform.localPosition = Vector3.zero;
        roofParent.transform.localPosition = Vector3.zero;
        interiorParent.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// �����S�̂𐶐����郁�\�b�h�B
    /// </summary>
    public float width;
    public float depth;
    public int floors;

    void GenerateBuilding()
    {
        // �����̕��A���s���A�����������_���Ɍ���
     width = Mathf.CeilToInt( Random.Range(widthRange.x, widthRange.y));
       depth = Mathf.CeilToInt( Random.Range(depthRange.x, depthRange.y));
        floors = Mathf.CeilToInt(Random.Range(1, maxFloors + 1));

        // �e�K�𐶐�
        for (int floor = 0; floor < floors; floor++)
        {
            GenerateFloor(floor, width, depth);
        }

        // �����𐶐�
        CreateRoof(new Vector3(0, floors * heightRange.y, 0), width, depth);
    }

    /// <summary>
    /// �e�K�𐶐����郁�\�b�h�B
    /// </summary>
    void GenerateFloor(int floor, float width, float depth)
    {
        float floorHeight = floor * heightRange.y;
        float nextFloorHeight = (floor + 1) * heightRange.y;

        CreateFloor(new Vector3(0, floorHeight, 0), width, depth);
        CreateCorners(floorHeight, width, depth);
        CreateWalls(floorHeight, width, depth);
        CreateBorders(nextFloorHeight, width, depth);
        GenerateInterior(floor, width, depth, floorHeight);
    }

    /// <summary>
    /// �ǂ𐶐����郁�\�b�h�B
    /// </summary>
    void CreateWalls(float floorHeight, float width, float depth)
    {
        GameObject wallPrefab = GetRandomPrefab(wallPrefabs);
        if (wallPrefab == null)
        {
            Debug.LogWarning("�ǂ̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        MeshRenderer wallMesh = wallPrefab.GetComponentInChildren<MeshRenderer>();
        if (wallMesh == null)
        {
            Debug.LogWarning("�ǂ̃v���n�u��MeshRenderer������܂���B");
            return;
        }

        float wallThickness = wallMesh.bounds.size.z;
        int numWallsX = Mathf.CeilToInt((2 * width) / wallThickness);
        int numWallsZ = Mathf.CeilToInt((2 * depth) / wallThickness);

        // X�������̕ǂ𐶐�
        CreateWallsAlongAxis(floorHeight, width, depth, wallThickness, numWallsX, Axis.X);

        // Z�������̕ǂ𐶐�
        CreateWallsAlongAxis(floorHeight, width, depth, wallThickness, numWallsZ, Axis.Z);

        // ����h�A�������_���ɔz�u
        RandomlyPlaceWindowsAndDoors(floorHeight, width, depth);
    }

    // ���̗񋓌^
    enum Axis { X, Z }

    /// <summary>
    /// �w�肵�����ɉ����ĕǂ𐶐����郁�\�b�h�B
    /// </summary>
    void CreateWallsAlongAxis(float floorHeight, float width, float depth, float wallThickness, int numWalls, Axis axis)
    {
        for (int i = 0; i < numWalls; i++)
        {
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;

            if (axis == Axis.X)
            {
                float xPos = -width + i * wallThickness + wallThickness / 2;
                position = new Vector3(xPos, floorHeight, depth);
                rotation = Quaternion.Euler(0, -90, 0);
                CreateWall(position, rotation);
                position.z = -depth;
                rotation = Quaternion.Euler(0, 90, 0);
                CreateWall(position, rotation);
            }
            else if (axis == Axis.Z)
            {
                float zPos = -depth + i * wallThickness + wallThickness / 2;
                position = new Vector3(width, floorHeight, zPos);
                CreateWall(position, Quaternion.identity);
                position.x = -width;
                rotation = Quaternion.Euler(0, 180, 0);
                CreateWall(position, rotation);
            }
        }
    }

    /// <summary>
    /// �ǂ𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateWall(Vector3 position, Quaternion rotation)
    {
        GameObject wallPrefab = GetRandomPrefab(wallPrefabs);
        if (wallPrefab == null) return;

        GameObject wall = Instantiate(wallPrefab, position, rotation, wallsParent.transform);

        // �ǂ̃}�e���A����F�������_���ɕύX�i�r�W���A���v�f�̑��l���j
       // RandomizeWallAppearance(wall);
    }

    /// <summary>
    /// �ǂ̊O�ς������_���ɕύX���郁�\�b�h�B
    /// </summary>
    void RandomizeWallAppearance(GameObject wall)
    {
        Renderer renderer = wall.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // �����_���ȐF��K�p
            renderer.material.color = Random.ColorHSV();
        }
    }

    /// <summary>
    /// �p�̒��𐶐����郁�\�b�h�B
    /// </summary>
    void CreateCorners(float floorHeight, float width, float depth)
    {
        GameObject cornerPrefab = GetRandomPrefab(cornerPrefabs);
        if (cornerPrefab == null)
        {
            Debug.LogWarning("�p�̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        CreateCorner(new Vector3(width, floorHeight, depth), Quaternion.Euler(0, 90, 0), cornerPrefab);
        CreateCorner(new Vector3(-width, floorHeight, depth), Quaternion.Euler(0, 0, 0), cornerPrefab);
        CreateCorner(new Vector3(width, floorHeight, -depth), Quaternion.Euler(0, 180, 0), cornerPrefab);
        CreateCorner(new Vector3(-width, floorHeight, -depth), Quaternion.Euler(0, -90, 0), cornerPrefab);
    }

    /// <summary>
    /// �p�̒��𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateCorner(Vector3 position, Quaternion rotation, GameObject cornerPrefab)
    {
        Instantiate(cornerPrefab, position, rotation, cornersParent.transform);
    }

    /// <summary>
    /// ���E�𐶐����郁�\�b�h�B
    /// </summary>
    void CreateBorders(float nextFloorHeight, float width, float depth)
    {
        GameObject borderPrefab = GetRandomPrefab(borderPrefabs);
        if (borderPrefab == null)
        {
            Debug.LogWarning("���E�̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        MeshRenderer borderMesh = borderPrefab.GetComponentInChildren<MeshRenderer>();
        if (borderMesh == null)
        {
            Debug.LogWarning("���E�̃v���n�u��MeshRenderer������܂���B");
            return;
        }

        float borderWidth = borderMesh.bounds.size.x;
        float borderDepth = borderMesh.bounds.size.z;

        int numBordersX = Mathf.CeilToInt((2 * width) / borderDepth);
        int numBordersZ = Mathf.CeilToInt((2 * depth) / borderDepth);

        // X�������̋��E�𐶐�
        for (int i = 0; i < numBordersX; i++)
        {
            float xPos = -width + i * borderDepth + borderDepth / 2;
            CreateBorder(new Vector3(xPos, nextFloorHeight, depth - borderWidth / 2), Quaternion.Euler(0, 90, 0));
            CreateBorder(new Vector3(xPos, nextFloorHeight, -depth + borderWidth / 2), Quaternion.Euler(0, -90, 0));
        }

        // Z�������̋��E�𐶐�
        for (int i = 0; i < numBordersZ; i++)
        {
            float zPos = -depth + i * borderDepth + borderDepth / 2;
            CreateBorder(new Vector3(width - borderWidth / 2, nextFloorHeight, zPos), Quaternion.Euler(0, 180, 0));
            CreateBorder(new Vector3(-width + borderWidth / 2, nextFloorHeight, zPos), Quaternion.identity);
        }
    }

    /// <summary>
    /// ���E�𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateBorder(Vector3 position, Quaternion rotation)
    {
        GameObject borderPrefab = GetRandomPrefab(borderPrefabs);
        if (borderPrefab == null) return;

        GameObject border = Instantiate(borderPrefab, position, rotation, bordersParent.transform);

        // ���E�̃}�e���A����F�������_���ɕύX
       // RandomizeBorderAppearance(border);
    }

    /// <summary>
    /// ���E�̊O�ς������_���ɕύX���郁�\�b�h�B
    /// </summary>
    void RandomizeBorderAppearance(GameObject border)
    {
        Renderer renderer = border.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV();
        }
    }

    /// <summary>
    /// �C���e���A�𐶐����郁�\�b�h�B
    /// </summary>
    void GenerateInterior(int floor, float width, float depth, float floorHeight)
    {
        if (floor == 0)
        {
            CreateDoor(new Vector3(width, floorHeight, 0));
        }

        if (floor >= 0)
        {
            CreateStair(new Vector3(width / 2 - 1, floorHeight, -depth / 2 + 1));
        }

        // �����_���ɑ���z�u
        PlaceRandomWindows(floorHeight, width, depth);
    }

    /// <summary>
    /// �����_���ɑ���z�u���郁�\�b�h�B
    void PlaceRandomWindows(float floorHeight, float width, float depth)
    {
        int numWindows = Random.Range(1, 5);

        for (int i = 0; i < numWindows; i++)
        {
            float xPos, yPos, zPos;
            Vector3 position;
            Quaternion rotation;

            // �����_���ɕǂ̕�����I��
            if (Random.value > 0.5f) // X�����̕ǂɔz�u
            {
                xPos = (Random.value > 0.5f) ? width : -width; // ���E�̂ǂ��炩�̕�
                zPos = Random.Range(-depth + 1, depth - 1); // �ǂɉ������ʒu
                yPos = floorHeight + Random.Range(1.0f, heightRange.y - 1.0f);
                position = new Vector3(xPos, yPos, zPos);
                rotation = (xPos > 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            }
            else // Z�����̕ǂɔz�u
            {
                zPos = (Random.value > 0.5f) ? depth : -depth; // �O��̂ǂ��炩�̕�
                xPos = Random.Range(-width + 1, width - 1); // �ǂɉ������ʒu
                yPos = floorHeight + Random.Range(1.0f, heightRange.y - 1.0f);
                position = new Vector3(xPos, yPos, zPos);
                rotation = (zPos > 0) ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
            }

            CreateWindow(position, rotation);
        }
    }

    /// <summary>
    /// ���𐶐�����w���p�[���\�b�h�B
    /// </summary>
    /// ���𐶐�����w���p�[���\�b�h�i���݂��l���j�B
    /// </summary>
    void CreateWindow(Vector3 position, Quaternion rotation)
    {
        GameObject windowPrefab = GetRandomPrefab(windowPrefabs);
        if (windowPrefab == null) return;

        MeshRenderer windowMesh = windowPrefab.GetComponentInChildren<MeshRenderer>();
        if (windowMesh == null)
        {
            Debug.LogWarning("���̃v���n�u��MeshRenderer������܂���B");
            return;
        }

        // ���̌��݂��擾
        float windowThickness = windowMesh.bounds.size.x;

        // ��]�Ɋ�Â��đ��̌��݂��l�������I�t�Z�b�g���v�Z
        Vector3 offset = Vector3.zero;
        if (rotation == Quaternion.identity) // X�����̕ǁi�����j
        {
            offset = new Vector3(-windowThickness / 2, 0, 0);
        }
        else if (rotation == Quaternion.Euler(0, 180, 0)) // X�����̕ǁi�E���j
        {
            offset = new Vector3(windowThickness / 2, 0, 0);
        }
        else if (rotation == Quaternion.Euler(0, -90, 0)) // Z�����̕ǁi�O�ʁj
        {
            offset = new Vector3(0, 0, windowThickness / 2);
        }
        else if (rotation == Quaternion.Euler(0, 90, 0)) // Z�����̕ǁi�w�ʁj
        {
            offset = new Vector3(0, 0, -windowThickness / 2);
        }

        // �����I�t�Z�b�g��K�p���Ĕz�u
        Instantiate(windowPrefab, position + offset, rotation, interiorParent.transform);
    }

    /// <summary>
    /// �h�A�𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateDoor(Vector3 position)
    {
        GameObject doorPrefab = GetRandomPrefab(doorPrefabs);
        if (doorPrefab == null)
        {
            Debug.LogWarning("�h�A�̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        MeshRenderer doorMesh = doorPrefab.GetComponentInChildren<MeshRenderer>();
        if (doorMesh == null)
        {
            Debug.LogWarning("�h�A�̃v���n�u��MeshRenderer������܂���B");
            return;
        }

        Vector3 doorSize = doorMesh.bounds.size;
        Vector3 adjustedPosition = position + new Vector3(doorSize.x / 2, 0, 0);
        Instantiate(doorPrefab, adjustedPosition, Quaternion.identity, interiorParent.transform);
    }

    /// <summary>
    /// �K�i�𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateStair(Vector3 position)
    {
        GameObject stairPrefab = GetRandomPrefab(stairPrefabs);
        if (stairPrefab == null)
        {
            Debug.LogWarning("�K�i�̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        Instantiate(stairPrefab, position, Quaternion.identity, interiorParent.transform);
    }

    /// <summary>
    /// ���𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateFloor(Vector3 position, float width, float depth)
    {
        GameObject floorPrefab = GetRandomPrefab(floorPrefabs);
        if (floorPrefab == null)
        {
            Debug.LogWarning("���̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, floorsParent.transform);
        floor.transform.localScale = new Vector3(width, 1, depth);

        // ���̃}�e���A����F�������_���ɕύX
       // RandomizeFloorAppearance(floor);
    }

    /// <summary>
    /// ���̊O�ς������_���ɕύX���郁�\�b�h�B
    /// </summary>
    void RandomizeFloorAppearance(GameObject floor)
    {
        Renderer renderer = floor.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV();
        }
    }

    /// <summary>
    /// �����𐶐�����w���p�[���\�b�h�B
    /// </summary>
    void CreateRoof(Vector3 position, float width, float depth)
    {
        GameObject roofPrefab = GetRandomPrefab(roofPrefabs);
        if (roofPrefab == null)
        {
            Debug.LogWarning("�����̃v���n�u���ݒ肳��Ă��܂���B");
            return;
        }

        GameObject roof = Instantiate(roofPrefab, position, Quaternion.identity, roofParent.transform);
        roof.transform.localScale = new Vector3(width, 1, depth);

        // �����̃}�e���A����F�������_���ɕύX
       // RandomizeRoofAppearance(roof);
    }

    /// <summary>
    /// �����̊O�ς������_���ɕύX���郁�\�b�h�B
    /// </summary>
    void RandomizeRoofAppearance(GameObject roof)
    {
        Renderer renderer = roof.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV();
        }
    }

    /// <summary>
    /// �v���n�u���X�g����d�݂Ɋ�Â��ă����_���Ƀv���n�u���擾���郁�\�b�h�B
    /// </summary>
    GameObject GetRandomPrefab(List<WeightedPrefab> weightedPrefabs)
    {
        if (weightedPrefabs == null || weightedPrefabs.Count == 0) return null;

        // ���v�d�ʂ��v�Z
        float totalWeight = 0f;
        foreach (var wp in weightedPrefabs)
        {
            totalWeight += wp.weight;
        }

        // �����_���Ȓl�𐶐�
        float randomValue = Random.Range(0, totalWeight);

        // �d�݂Ɋ�Â��ăv���n�u��I��
        float cumulativeWeight = 0f;
        foreach (var wp in weightedPrefabs)
        {
            cumulativeWeight += wp.weight;
            if (randomValue <= cumulativeWeight)
            {
                return wp.prefab;
            }
        }

        // �f�t�H���g�Ƃ��čŏ��̃v���n�u��Ԃ�
        return weightedPrefabs[0].prefab;
    }

    /// <summary>
    /// ����h�A�������_���ɔz�u���郁�\�b�h�B
    /// </summary>
    void RandomlyPlaceWindowsAndDoors(float floorHeight, float width, float depth)
    {
        // ����h�A�̔z�u���W�b�N�������ɒǉ�
        // ���̗�ł͏ȗ����Ă��܂�
    }











    void UnravelBuilding()
    {
        // �e�q�I�u�W�F�N�g���`�F�b�N���ď����ɍ������̂��A�N�e�B�u�ɂ���
        DeactivateOutsideObjects(wallsParent);
        DeactivateOutsideObjects(cornersParent);
        DeactivateOutsideObjects(bordersParent);
        DeactivateOutsideObjects(floorsParent);
        DeactivateOutsideObjects(roofParent);
        DeactivateOutsideObjects(interiorParent);

        roofParent.gameObject.SetActive(false);
    }

    void DeactivateOutsideObjects(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Vector3 localPos = child.localPosition;

            // X��width�ȏ�AZ��-depth�ȏ�̃I�u�W�F�N�g���A�N�e�B�u�ɂ���
            if (localPos.x >= width- 0.5f || localPos.z <= -depth+0.5f)
            {
                child.gameObject.SetActive(false);
            }
        }
        
    }
}

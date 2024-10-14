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
    [Header("プレハブ設定")]
    [Tooltip("壁のプレハブリスト")]
    public List<WeightedPrefab> wallPrefabs;
    [Tooltip("屋根のプレハブリスト")]
    public List<WeightedPrefab> roofPrefabs;
    [Tooltip("床のプレハブリスト")]
    public List<WeightedPrefab> floorPrefabs;
    [Tooltip("境界のプレハブリスト")]
    public List<WeightedPrefab> borderPrefabs;
    [Tooltip("窓のプレハブリスト")]
    public List<WeightedPrefab> windowPrefabs;
    [Tooltip("ドアのプレハブリスト")]
    public List<WeightedPrefab> doorPrefabs;
    [Tooltip("階段のプレハブリスト")]
    public List<WeightedPrefab> stairPrefabs;
    [Tooltip("角の柱のプレハブリスト")]
    public List<WeightedPrefab> cornerPrefabs;

    [Header("建物の寸法設定")]
    [Tooltip("建物の幅の範囲")]
    public Vector2 widthRange = new Vector2(4.0f, 8.0f);
    [Tooltip("各階の高さの範囲")]
    public Vector2 heightRange = new Vector2(3.0f, 5.0f);
    [Tooltip("建物の奥行きの範囲")]
    public Vector2 depthRange = new Vector2(4.0f, 8.0f);
    [Tooltip("最大階数")]
    [Range(1, 10)]
    public int maxFloors = 3;

    [Header("建築スタイル設定")]
    [Tooltip("建築スタイルの選択")]
    public ArchitecturalStyle style = ArchitecturalStyle.Modern;


    [Header("デバッグ設定")]
   
    public bool isUnravel = false; // このフラグをInspectorでオンにすると、指定されたオブジェクトが非アクティブになります。

    // 建築スタイルの列挙型
    public enum ArchitecturalStyle
    {
        Modern,
        Victorian,
        Futuristic,
        // 必要に応じて他のスタイルを追加
    }

    // 親オブジェクトを保持する変数
    private GameObject wallsParent;
    private GameObject cornersParent;
    private GameObject bordersParent;
    private GameObject floorsParent;
    private GameObject roofParent;
    private GameObject interiorParent;

    // 初期化処理
    void Start()
    {
        // 親オブジェクトの作成
        CreateParentObjects();

        // 建物の生成
        GenerateBuilding();

        // 親オブジェクトの位置をリセット
        ResetParentObjectPositions();
    }
    void Update()
    {
        // isUnravelがtrueの場合、指定された条件に合うオブジェクトを非アクティブにする
        if (isUnravel)
        {
            UnravelBuilding();
        }
    }

    /// <summary>
    /// 親オブジェクトを作成するメソッド。
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
    /// 親オブジェクトの位置をリセットするメソッド。
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
    /// 建物全体を生成するメソッド。
    /// </summary>
    public float width;
    public float depth;
    public int floors;

    void GenerateBuilding()
    {
        // 建物の幅、奥行き、高さをランダムに決定
     width = Mathf.CeilToInt( Random.Range(widthRange.x, widthRange.y));
       depth = Mathf.CeilToInt( Random.Range(depthRange.x, depthRange.y));
        floors = Mathf.CeilToInt(Random.Range(1, maxFloors + 1));

        // 各階を生成
        for (int floor = 0; floor < floors; floor++)
        {
            GenerateFloor(floor, width, depth);
        }

        // 屋根を生成
        CreateRoof(new Vector3(0, floors * heightRange.y, 0), width, depth);
    }

    /// <summary>
    /// 各階を生成するメソッド。
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
    /// 壁を生成するメソッド。
    /// </summary>
    void CreateWalls(float floorHeight, float width, float depth)
    {
        GameObject wallPrefab = GetRandomPrefab(wallPrefabs);
        if (wallPrefab == null)
        {
            Debug.LogWarning("壁のプレハブが設定されていません。");
            return;
        }

        MeshRenderer wallMesh = wallPrefab.GetComponentInChildren<MeshRenderer>();
        if (wallMesh == null)
        {
            Debug.LogWarning("壁のプレハブにMeshRendererがありません。");
            return;
        }

        float wallThickness = wallMesh.bounds.size.z;
        int numWallsX = Mathf.CeilToInt((2 * width) / wallThickness);
        int numWallsZ = Mathf.CeilToInt((2 * depth) / wallThickness);

        // X軸方向の壁を生成
        CreateWallsAlongAxis(floorHeight, width, depth, wallThickness, numWallsX, Axis.X);

        // Z軸方向の壁を生成
        CreateWallsAlongAxis(floorHeight, width, depth, wallThickness, numWallsZ, Axis.Z);

        // 窓やドアをランダムに配置
        RandomlyPlaceWindowsAndDoors(floorHeight, width, depth);
    }

    // 軸の列挙型
    enum Axis { X, Z }

    /// <summary>
    /// 指定した軸に沿って壁を生成するメソッド。
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
    /// 壁を生成するヘルパーメソッド。
    /// </summary>
    void CreateWall(Vector3 position, Quaternion rotation)
    {
        GameObject wallPrefab = GetRandomPrefab(wallPrefabs);
        if (wallPrefab == null) return;

        GameObject wall = Instantiate(wallPrefab, position, rotation, wallsParent.transform);

        // 壁のマテリアルや色をランダムに変更（ビジュアル要素の多様化）
       // RandomizeWallAppearance(wall);
    }

    /// <summary>
    /// 壁の外観をランダムに変更するメソッド。
    /// </summary>
    void RandomizeWallAppearance(GameObject wall)
    {
        Renderer renderer = wall.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            // ランダムな色を適用
            renderer.material.color = Random.ColorHSV();
        }
    }

    /// <summary>
    /// 角の柱を生成するメソッド。
    /// </summary>
    void CreateCorners(float floorHeight, float width, float depth)
    {
        GameObject cornerPrefab = GetRandomPrefab(cornerPrefabs);
        if (cornerPrefab == null)
        {
            Debug.LogWarning("角のプレハブが設定されていません。");
            return;
        }

        CreateCorner(new Vector3(width, floorHeight, depth), Quaternion.Euler(0, 90, 0), cornerPrefab);
        CreateCorner(new Vector3(-width, floorHeight, depth), Quaternion.Euler(0, 0, 0), cornerPrefab);
        CreateCorner(new Vector3(width, floorHeight, -depth), Quaternion.Euler(0, 180, 0), cornerPrefab);
        CreateCorner(new Vector3(-width, floorHeight, -depth), Quaternion.Euler(0, -90, 0), cornerPrefab);
    }

    /// <summary>
    /// 角の柱を生成するヘルパーメソッド。
    /// </summary>
    void CreateCorner(Vector3 position, Quaternion rotation, GameObject cornerPrefab)
    {
        Instantiate(cornerPrefab, position, rotation, cornersParent.transform);
    }

    /// <summary>
    /// 境界を生成するメソッド。
    /// </summary>
    void CreateBorders(float nextFloorHeight, float width, float depth)
    {
        GameObject borderPrefab = GetRandomPrefab(borderPrefabs);
        if (borderPrefab == null)
        {
            Debug.LogWarning("境界のプレハブが設定されていません。");
            return;
        }

        MeshRenderer borderMesh = borderPrefab.GetComponentInChildren<MeshRenderer>();
        if (borderMesh == null)
        {
            Debug.LogWarning("境界のプレハブにMeshRendererがありません。");
            return;
        }

        float borderWidth = borderMesh.bounds.size.x;
        float borderDepth = borderMesh.bounds.size.z;

        int numBordersX = Mathf.CeilToInt((2 * width) / borderDepth);
        int numBordersZ = Mathf.CeilToInt((2 * depth) / borderDepth);

        // X軸方向の境界を生成
        for (int i = 0; i < numBordersX; i++)
        {
            float xPos = -width + i * borderDepth + borderDepth / 2;
            CreateBorder(new Vector3(xPos, nextFloorHeight, depth - borderWidth / 2), Quaternion.Euler(0, 90, 0));
            CreateBorder(new Vector3(xPos, nextFloorHeight, -depth + borderWidth / 2), Quaternion.Euler(0, -90, 0));
        }

        // Z軸方向の境界を生成
        for (int i = 0; i < numBordersZ; i++)
        {
            float zPos = -depth + i * borderDepth + borderDepth / 2;
            CreateBorder(new Vector3(width - borderWidth / 2, nextFloorHeight, zPos), Quaternion.Euler(0, 180, 0));
            CreateBorder(new Vector3(-width + borderWidth / 2, nextFloorHeight, zPos), Quaternion.identity);
        }
    }

    /// <summary>
    /// 境界を生成するヘルパーメソッド。
    /// </summary>
    void CreateBorder(Vector3 position, Quaternion rotation)
    {
        GameObject borderPrefab = GetRandomPrefab(borderPrefabs);
        if (borderPrefab == null) return;

        GameObject border = Instantiate(borderPrefab, position, rotation, bordersParent.transform);

        // 境界のマテリアルや色をランダムに変更
       // RandomizeBorderAppearance(border);
    }

    /// <summary>
    /// 境界の外観をランダムに変更するメソッド。
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
    /// インテリアを生成するメソッド。
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

        // ランダムに窓を配置
        PlaceRandomWindows(floorHeight, width, depth);
    }

    /// <summary>
    /// ランダムに窓を配置するメソッド。
    void PlaceRandomWindows(float floorHeight, float width, float depth)
    {
        int numWindows = Random.Range(1, 5);

        for (int i = 0; i < numWindows; i++)
        {
            float xPos, yPos, zPos;
            Vector3 position;
            Quaternion rotation;

            // ランダムに壁の方向を選択
            if (Random.value > 0.5f) // X方向の壁に配置
            {
                xPos = (Random.value > 0.5f) ? width : -width; // 左右のどちらかの壁
                zPos = Random.Range(-depth + 1, depth - 1); // 壁に沿った位置
                yPos = floorHeight + Random.Range(1.0f, heightRange.y - 1.0f);
                position = new Vector3(xPos, yPos, zPos);
                rotation = (xPos > 0) ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            }
            else // Z方向の壁に配置
            {
                zPos = (Random.value > 0.5f) ? depth : -depth; // 前後のどちらかの壁
                xPos = Random.Range(-width + 1, width - 1); // 壁に沿った位置
                yPos = floorHeight + Random.Range(1.0f, heightRange.y - 1.0f);
                position = new Vector3(xPos, yPos, zPos);
                rotation = (zPos > 0) ? Quaternion.Euler(0, -90, 0) : Quaternion.Euler(0, 90, 0);
            }

            CreateWindow(position, rotation);
        }
    }

    /// <summary>
    /// 窓を生成するヘルパーメソッド。
    /// </summary>
    /// 窓を生成するヘルパーメソッド（厚みを考慮）。
    /// </summary>
    void CreateWindow(Vector3 position, Quaternion rotation)
    {
        GameObject windowPrefab = GetRandomPrefab(windowPrefabs);
        if (windowPrefab == null) return;

        MeshRenderer windowMesh = windowPrefab.GetComponentInChildren<MeshRenderer>();
        if (windowMesh == null)
        {
            Debug.LogWarning("窓のプレハブにMeshRendererがありません。");
            return;
        }

        // 窓の厚みを取得
        float windowThickness = windowMesh.bounds.size.x;

        // 回転に基づいて窓の厚みを考慮したオフセットを計算
        Vector3 offset = Vector3.zero;
        if (rotation == Quaternion.identity) // X方向の壁（左側）
        {
            offset = new Vector3(-windowThickness / 2, 0, 0);
        }
        else if (rotation == Quaternion.Euler(0, 180, 0)) // X方向の壁（右側）
        {
            offset = new Vector3(windowThickness / 2, 0, 0);
        }
        else if (rotation == Quaternion.Euler(0, -90, 0)) // Z方向の壁（前面）
        {
            offset = new Vector3(0, 0, windowThickness / 2);
        }
        else if (rotation == Quaternion.Euler(0, 90, 0)) // Z方向の壁（背面）
        {
            offset = new Vector3(0, 0, -windowThickness / 2);
        }

        // 窓をオフセットを適用して配置
        Instantiate(windowPrefab, position + offset, rotation, interiorParent.transform);
    }

    /// <summary>
    /// ドアを生成するヘルパーメソッド。
    /// </summary>
    void CreateDoor(Vector3 position)
    {
        GameObject doorPrefab = GetRandomPrefab(doorPrefabs);
        if (doorPrefab == null)
        {
            Debug.LogWarning("ドアのプレハブが設定されていません。");
            return;
        }

        MeshRenderer doorMesh = doorPrefab.GetComponentInChildren<MeshRenderer>();
        if (doorMesh == null)
        {
            Debug.LogWarning("ドアのプレハブにMeshRendererがありません。");
            return;
        }

        Vector3 doorSize = doorMesh.bounds.size;
        Vector3 adjustedPosition = position + new Vector3(doorSize.x / 2, 0, 0);
        Instantiate(doorPrefab, adjustedPosition, Quaternion.identity, interiorParent.transform);
    }

    /// <summary>
    /// 階段を生成するヘルパーメソッド。
    /// </summary>
    void CreateStair(Vector3 position)
    {
        GameObject stairPrefab = GetRandomPrefab(stairPrefabs);
        if (stairPrefab == null)
        {
            Debug.LogWarning("階段のプレハブが設定されていません。");
            return;
        }

        Instantiate(stairPrefab, position, Quaternion.identity, interiorParent.transform);
    }

    /// <summary>
    /// 床を生成するヘルパーメソッド。
    /// </summary>
    void CreateFloor(Vector3 position, float width, float depth)
    {
        GameObject floorPrefab = GetRandomPrefab(floorPrefabs);
        if (floorPrefab == null)
        {
            Debug.LogWarning("床のプレハブが設定されていません。");
            return;
        }

        GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity, floorsParent.transform);
        floor.transform.localScale = new Vector3(width, 1, depth);

        // 床のマテリアルや色をランダムに変更
       // RandomizeFloorAppearance(floor);
    }

    /// <summary>
    /// 床の外観をランダムに変更するメソッド。
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
    /// 屋根を生成するヘルパーメソッド。
    /// </summary>
    void CreateRoof(Vector3 position, float width, float depth)
    {
        GameObject roofPrefab = GetRandomPrefab(roofPrefabs);
        if (roofPrefab == null)
        {
            Debug.LogWarning("屋根のプレハブが設定されていません。");
            return;
        }

        GameObject roof = Instantiate(roofPrefab, position, Quaternion.identity, roofParent.transform);
        roof.transform.localScale = new Vector3(width, 1, depth);

        // 屋根のマテリアルや色をランダムに変更
       // RandomizeRoofAppearance(roof);
    }

    /// <summary>
    /// 屋根の外観をランダムに変更するメソッド。
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
    /// プレハブリストから重みに基づいてランダムにプレハブを取得するメソッド。
    /// </summary>
    GameObject GetRandomPrefab(List<WeightedPrefab> weightedPrefabs)
    {
        if (weightedPrefabs == null || weightedPrefabs.Count == 0) return null;

        // 合計重量を計算
        float totalWeight = 0f;
        foreach (var wp in weightedPrefabs)
        {
            totalWeight += wp.weight;
        }

        // ランダムな値を生成
        float randomValue = Random.Range(0, totalWeight);

        // 重みに基づいてプレハブを選択
        float cumulativeWeight = 0f;
        foreach (var wp in weightedPrefabs)
        {
            cumulativeWeight += wp.weight;
            if (randomValue <= cumulativeWeight)
            {
                return wp.prefab;
            }
        }

        // デフォルトとして最初のプレハブを返す
        return weightedPrefabs[0].prefab;
    }

    /// <summary>
    /// 窓やドアをランダムに配置するメソッド。
    /// </summary>
    void RandomlyPlaceWindowsAndDoors(float floorHeight, float width, float depth)
    {
        // 窓やドアの配置ロジックをここに追加
        // この例では省略しています
    }











    void UnravelBuilding()
    {
        // 各子オブジェクトをチェックして条件に合うものを非アクティブにする
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

            // Xがwidth以上、Zが-depth以上のオブジェクトを非アクティブにする
            if (localPos.x >= width- 0.5f || localPos.z <= -depth+0.5f)
            {
                child.gameObject.SetActive(false);
            }
        }
        
    }
}

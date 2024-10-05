using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneManager : MonoBehaviour
{
    [System.Serializable]
    public class PrefabWithHeight
    {
        public GameObject prefab;    // PrefabのGameObject
        public Vector2 heightRange;  // 高さの範囲（min, max）
    }

    public GameObject player; // プレイヤーオブジェクト
    public GameObject planePrefab; // プレファブ化したPlane
    public float planeLength = 1000f; // Planeの長さ（z方向）
    public float planeWidth = 1000f;  // Planeの幅（x方向）
    public int maxObjectsPerPlane = 5; // 1つのPlaneに配置するオブジェクトの最大数
    public float objectMinDistance = 1.5f; // オブジェクト間の最小距離
    public float xOffsetLimit = 4f; // Planeの中央からx方向でオブジェクトが置ける範囲（±xOffsetLimit）
    public float preGenerateThreshold = 250f; // プレイヤーがこの閾値を超えると隣接Planeを事前生成
    public int maxPlaneDistance = 3; // プレイヤーから何グリッド以上離れたPlaneを削除するか
    public List<PrefabWithHeight> objectPrefabs; // TreeやRiverのPrefabリストと高さ範囲

    private Dictionary<Vector2Int, GameObject> planeGrid = new Dictionary<Vector2Int, GameObject>(); // Planeを管理する辞書
    private Vector2Int currentGridPosition = Vector2Int.zero; // 現在のグリッド位置

    void Start()
    {
        // 初期位置 (0,0) のPlaneはすでにあるものとして、プレイヤーの周囲に5つのPlaneを生成
        GenerateInitialPlanes();
    }

    // プレイヤーの初期位置を中心に、左、左前、前、右前、右のPlaneを生成
    void GenerateInitialPlanes()
    {
        // 現在のグリッド位置を (0,0) として扱う
        Vector2Int[] initialOffsets = {
            new Vector2Int(-1, 0),   // 左
            new Vector2Int(-1, 1),   // 左前
            new Vector2Int(0, 1),    // 前
            new Vector2Int(1, 1),    // 右前
            new Vector2Int(1, 0)     // 右
        };

        // 5つのPlaneを生成
        foreach (var offset in initialOffsets)
        {
            Vector2Int gridPosition = currentGridPosition + offset;
            if (!planeGrid.ContainsKey(gridPosition))
            {
                Vector3 planePosition = new Vector3(gridPosition.x * planeWidth, 0, gridPosition.y * planeLength);
                GameObject newPlane = Instantiate(planePrefab, planePosition, Quaternion.identity);
                planeGrid[gridPosition] = newPlane;
                // newPlane.GetComponent<MapGenerator>().lSystemGenerators.Clear();
                // newPlane.SetActive(true);
                // Plane上にオブジェクトを配置
                PlaceObjectsOnPlane(newPlane  ,planePosition.x, planePosition.z);
                //newPlane.GetComponent<MapGenerator>().GenerateBooleanTerrain();

            }
        }
    }

    void Update()
    {
        // プレイヤーのグリッド位置を計算
        Vector2Int playerGridPosition = new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x / planeWidth),
            Mathf.FloorToInt(player.transform.position.z / planeLength)
        );

        // プレイヤーが隣接グリッドに近づいた場合
        float playerOffsetX = Mathf.Abs(player.transform.position.x - currentGridPosition.x * planeWidth);
        float playerOffsetZ = Mathf.Abs(player.transform.position.z - currentGridPosition.y * planeLength);

        if (playerOffsetX >= preGenerateThreshold || playerOffsetZ >= preGenerateThreshold)
        {
            // 現在のグリッドと隣接グリッドを事前に生成
            PreGenerateAdjacentPlanes(playerGridPosition);
            currentGridPosition = playerGridPosition;

            // プレイヤーより後ろにあるPlaneを削除
            RemovePlanesBehindPlayer();
            // 古いPlaneを削除
            //  RemoveOldPlanes(playerGridPosition);
        }
      
    }
    void RemovePlanesBehindPlayer()
    {
        List<Vector2Int> planesToRemove = new List<Vector2Int>();

        foreach (var plane in planeGrid)
        {
            // プレイヤーのz座標よりもPlaneが後ろにある場合、そのPlaneを削除
            if (plane.Value.transform.position.z + 1024 < player.transform.position.z)
            {
                planesToRemove.Add(plane.Key);
            }
        }

        // 削除するPlaneをリストから削除
        foreach (var planeKey in planesToRemove)
        {
            // Planeとその上のオブジェクトを削除
            Destroy(planeGrid[planeKey]);
            planeGrid.Remove(planeKey);
        }
    }
    // 隣接するPlaneとそのさらに隣のPlaneを事前に生成
    void PreGenerateAdjacentPlanes(Vector2Int playerGridPosition)
    {
        // 現在のグリッドに加えて、周囲の8つの隣接グリッドとその隣も生成
        for (int xOffset = -3; xOffset <= 3; xOffset++)
        {
            for (int yOffset = 0; yOffset <= 2; yOffset++)
            {
                Vector2Int adjacentGridPosition = new Vector2Int(playerGridPosition.x + xOffset, playerGridPosition.y + yOffset);

                // 隣接するグリッドとその隣接グリッドがまだ生成されていない場合に生成
                if (!planeGrid.ContainsKey(adjacentGridPosition))
                {
                    Vector3 planePosition = new Vector3(adjacentGridPosition.x * planeWidth, 0, adjacentGridPosition.y * planeLength);
                    GameObject newPlane = Instantiate(planePrefab, planePosition, Quaternion.identity);
                    planeGrid[adjacentGridPosition] = newPlane;
                    // newPlane.SetActive(true);
                    //newPlane.GetComponent<MapGenerator>().lSystemGenerators.Clear();

                    // 新しいPlane上にオブジェクトを配置
                    PlaceObjectsOnPlane(newPlane ,planePosition.x, planePosition.z);
                    //newPlane.GetComponent<MapGenerator>().GenerateBooleanTerrain();

                }
            }
        }
    }

    // 古いPlaneを削除するロジック
    void RemoveOldPlanes(Vector2Int playerGridPosition)
    {
        List<Vector2Int> planesToRemove = new List<Vector2Int>();

        foreach (var plane in planeGrid)
        {
            // 2D距離を計算
            float distance = Vector2.Distance(new Vector2(plane.Key.x, plane.Key.y), new Vector2(playerGridPosition.x, playerGridPosition.y));

            // プレイヤーの現在のグリッド位置からの距離が maxPlaneDistance を超えたら削除
            if (distance > maxPlaneDistance)
            {
                planesToRemove.Add(plane.Key);
            }
        }

        // 削除するPlaneをリストから削除
        foreach (var planeKey in planesToRemove)
        {
            Destroy(planeGrid[planeKey]);
            planeGrid.Remove(planeKey);
        }
    }

    // Plane上にオブジェクトを配置する
    void PlaceObjectsOnPlane(GameObject plane, float planeXPosition, float planeZPosition)
    {
        int objectCount = Random.Range(1, maxObjectsPerPlane + 1); // ランダムにオブジェクト数を決定
        for (int i = 0; i < objectCount; i++)
        {
            // ランダムな位置とオブジェクトを選択
            Vector3 randomPosition = GetRandomPositionOnPlane(planeXPosition, planeZPosition);
            PrefabWithHeight randomPrefabWithHeight = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
            GameObject randomPrefab = randomPrefabWithHeight.prefab;

            // 高さをランダムに設定
            float randomHeight = Random.Range(randomPrefabWithHeight.heightRange.x, randomPrefabWithHeight.heightRange.y);
            randomPosition.y = randomHeight;

            // オブジェクトを生成し、そのPlaneの子として設定
            GameObject newObject = Instantiate(randomPrefab, randomPosition, Quaternion.identity);
            newObject.name = randomPrefab.name;

            // 生成したオブジェクトをPlaneの子供にする
            newObject.transform.SetParent(plane.transform);

            newObject.SetActive(true);
        }
    }

    // Plane上でランダムにオブジェクトの配置位置を取得
    // Plane上でランダムにオブジェクトの配置位置を取得
    Vector3 GetRandomPositionOnPlane(float planeXPosition, float planeZPosition)
    {
        Vector3 position;
        bool validPosition;

        do
        {
            // x方向をplaneXPositionに基づいてランダムに設定
            float randomX = Random.Range(planeXPosition - planeWidth / 2, planeXPosition + planeWidth / 2);
            // z方向もplaneZPositionに基づいてランダムに設定
            float randomZ = Random.Range(planeZPosition - planeLength / 2, planeZPosition + planeLength / 2);
            position = new Vector3(randomX, 0, randomZ);

            validPosition = true;

            // 他のオブジェクトと重ならないように配置する
            foreach (var trackedObject in ObjectNameUI2.Instance.trackedObjects)
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

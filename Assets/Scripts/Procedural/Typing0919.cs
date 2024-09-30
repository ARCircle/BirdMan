using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

public class Typing0919 : MonoBehaviour
{
    public GameObject player; // プレイヤーオブジェクトを参照
    private List<ObjectNameUI.TrackedObject> trackedObjects; // ObjectNameUIのTrackedObjectを参照

    public float detectionRadius = 10f; // プレイヤーの検知範囲

    // 削除対象のインデックスを保存するリスト
    private List<int> indicesToRemove = new List<int>();
    public ObjectDisplayManager displayManager; // ObjectDisplayManagerの参照

    void Start()
    {
        // ObjectNameUIのインスタンスからTrackedObjectリストを取得
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

        // オブジェクトの削除処理はループ外で実行
        RemoveObjects();
    }

    void HandleKeyPress(string key)
    {
        for (int i = 0; i < trackedObjects.Count; i++)
        {
            TextMeshPro nameText = trackedObjects[i].NameText;
            string rawNameString = trackedObjects[i].rawNameString; // ObjectNameUIのrawNameStringを参照

            // 頭文字がキー入力と一致するか確認
            if (trackedObjects[i].currentIndex < rawNameString.Length &&
                rawNameString[trackedObjects[i].currentIndex].ToString().Equals(key, System.StringComparison.OrdinalIgnoreCase))
            {

                // プレイヤーの半径内にあるか確認
                if (trackedObjects[i].ObjectToTrack == null)
                    break;
                if (IsWithinPlayerRadius(trackedObjects[i].ObjectToTrack))
                {
                    trackedObjects[i].currentIndex++;
                    UpdateTextColor(i, nameText, rawNameString);

                    // 全ての文字が入力された場合は削除リストに追加
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
        // プレイヤーとターゲットのXZ平面での距離を計算
        Vector3 playerPos = player.transform.position;
        Vector3 targetPos = targetObject.transform.position;

        float distanceXZ = Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(targetPos.x, targetPos.z));

        // 距離が検知範囲内か確認
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

    // まとめてオブジェクトを削除
    void RemoveObjects()
    {
        // 削除はリストの後ろから行うことでインデックスのズレを防ぐ
        indicesToRemove.Sort();
        indicesToRemove.Reverse();

        foreach (int index in indicesToRemove)
        {
            DestroyObject(index);
        }

        indicesToRemove.Clear(); // 削除リストをクリア
    }

    void DestroyObject(int index)
    {
        // オブジェクトを破壊
        //  Destroy(trackedObjects[index].ObjectToTrack);
        displayManager.Display(trackedObjects[index].ObjectToTrack);
        Destroy(trackedObjects[index].NameText.gameObject); // NameTextのGameObjectも削除
        trackedObjects.RemoveAt(index);
    }

    void UpdateAllTextColors()
    {
        for (int i = 0; i < trackedObjects.Count; i++)
        {
            TextMeshPro nameText = trackedObjects[i].NameText;
            string rawNameString = trackedObjects[i].rawNameString; // rawNameStringを参照
            UpdateTextColor(i, nameText, rawNameString);
        }
    }
}

using UnityEngine;
using TMPro;
using System.Collections.Generic;
public class ObjectNameUI2 : MonoBehaviour
{
    // シングルトンのインスタンス
    public static ObjectNameUI2 Instance { get; private set; }

    public class TrackedObject
    {
        public GameObject ObjectToTrack { get; private set; } // 追跡するオブジェクト
        public TextMeshPro NameText { get; private set; } // 表示するTextMeshPro (World Space用)
        public MeshRenderer ObjectMeshRenderer { get; private set; } // オブジェクトのMeshRendererコンポーネント
        public string rawNameString { get; private set; } // 元の名前テキスト
        public int currentIndex { get; set; } // 現在の入力進捗を管理
        public Vector3 initialTopCenter; // 初回のバウンディングボックス上部中心を保存
        public Vector3 lastPosition; // 最後の位置
        public bool isWithinRange; // プレイヤーの範囲内にいるかどうかを管理するフラグ

        public TrackedObject(GameObject targetObject, TextMeshPro text, string name)
        {
            ObjectToTrack = targetObject;
            NameText = text;
            rawNameString = name;
            currentIndex = 0; // 初期値は0
            ObjectMeshRenderer = targetObject.GetComponent<MeshRenderer>();
            isWithinRange = false; // 初期状態は範囲外

            // すべてのRendererコンポーネントを対象にバウンドを結合
            Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                // 初回にバウンディングボックスの上部中心を計算して保存
                Bounds combinedBounds = new Bounds(renderers[0].bounds.center, renderers[0].bounds.size); // 最初のバウンドを基準に初期化
                foreach (Renderer renderer in renderers)
                {
                    combinedBounds.Encapsulate(renderer.bounds); // 各レンダラーのバウンドを結合
                }

                // バウンドの上部中心を計算
                initialTopCenter = new Vector3(combinedBounds.center.x, combinedBounds.max.y, combinedBounds.center.z) + new Vector3(0, 0.5f, 0);
                lastPosition = ObjectToTrack.transform.position; // 初期位置も保存
            }
        }
    }

    public List<TrackedObject> trackedObjects = new List<TrackedObject>();

    void Awake()
    {
        // シングルトンのインスタンスを設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // インスタンスが既に存在する場合は破棄
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
        // TextMeshPro (3D Text) オブジェクトを生成
        GameObject textGameObject = new GameObject("ObjectNameText", typeof(TextMeshPro));

        // TextMeshProコンポーネントの参照を取得
        TextMeshPro text = textGameObject.GetComponent<TextMeshPro>();

        // TextMeshProの基本設定
        text.fontSize = 50;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.fontStyle = FontStyles.Bold;
        text.enableWordWrapping = false;

        // シェーダーを「Distance Field Overlay」に設定して、常に前面に表示するようにする
        Material textMaterial = text.fontMaterial;
        textMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");

        // World Spaceに設定
        text.transform.localScale = Vector3.one * 0.1f;
        text.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);

        return text;
    }

    private void UpdateUIPosition(TrackedObject trackedObject)
    {
       
        // オブジェクトの移動分を計算し、その分だけTextMeshProオブジェクトを移動させる
        //Vector3 positionDifference = trackedObject.ObjectToTrack.transform.position - trackedObject.lastPosition;
        trackedObject.NameText.transform.position =new Vector3(0, trackedObject.initialTopCenter.y,0)+ trackedObject.ObjectToTrack.transform.position;

        // 最後の位置を更新
        trackedObject.lastPosition = trackedObject.ObjectToTrack.transform.position;
    }
}

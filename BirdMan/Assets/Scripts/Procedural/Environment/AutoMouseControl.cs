using UnityEngine;
using UnityEngine.InputSystem;

public class AutoMouseControl : MonoBehaviour
{
    public BirdControl birdControl; // BirdControlスクリプトへの参照
    public float minFrequencyX = 0.5f; // 振動の最小周波数（X軸）
    public float maxFrequencyX = 2.0f; // 振動の最大周波数（X軸）
    public float minFrequencyY = 0.5f; // 振動の最小周波数（Y軸）
    public float maxFrequencyY = 2.0f; // 振動の最大周波数（Y軸）
    public float amplitudeX = 100.0f; // 振動の振幅（X軸）
    public float amplitudeY = 100.0f; // 振動の振幅（Y軸）

    private float currentFrequencyX; // 現在の振動の周波数（X軸）
    private float currentFrequencyY; // 現在の振動の周波数（Y軸）
    private float timeCounterX = 0f; // 時間のカウンター（X軸）
    private float timeCounterY = 0f; // 時間のカウンター（Y軸）
    public Vector2 simulatedMousePosition; // 仮想マウスの位置
    private Vector2 initialClickPosition; // 画面の中心位置

    // 位相チェック用フラグ
    private bool hasReachedPiOverTwoX = false;
    private bool hasReachedPiOverTwoY = false;
    private float phaseTolerance = 0.1f; // 位相の許容範囲

    // Sinusoidal制御を有効にするフラグ
    public bool isSinusoidalControlEnabled = false;

    // 周波数が負の値になりすぎるのを防ぐためのフラグとカウンター
    private int negativeFrequencyCounter = 0;
    public int maxNegativeFrequencyCount = 3; // 負の周波数が続く最大回数

    void Start()
    {
        // 初期の周波数をランダムに設定
        currentFrequencyX = Random.Range(minFrequencyX, maxFrequencyX);
        currentFrequencyY = Random.Range(minFrequencyY, maxFrequencyY);

        // ディスプレイの中心点を初期クリック位置として設定
        initialClickPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    void Update()
    {
        if (isSinusoidalControlEnabled)
        {
            // Y軸の振動計算
            timeCounterY += Time.deltaTime * currentFrequencyY;
            float oscillationY = Mathf.Sin(timeCounterY) * amplitudeY;

            // X軸の振動計算
            timeCounterX += Time.deltaTime * currentFrequencyX;
            float oscillationX = Mathf.Sin(timeCounterX) * amplitudeX;

            // 仮想的にマウスの位置を中心点を基準に制御する
            simulatedMousePosition = new Vector2(initialClickPosition.x + oscillationX, initialClickPosition.y + oscillationY);

            // Y軸の位相がπ/2に到達したかどうかを確認
            float phaseY = timeCounterY % (2 * Mathf.PI);
            if (Mathf.Abs(phaseY - Mathf.PI / 2) < phaseTolerance && !hasReachedPiOverTwoY)
            {
                // 位相がπ/2に近づいたので、Y軸の周波数をランダムに変更
                currentFrequencyY = Random.Range(minFrequencyY, maxFrequencyY);
                hasReachedPiOverTwoY = true; // フラグを立てる
            }
            else if (phaseY < Mathf.PI / 2)
            {
                // 位相が再び0に戻るまでフラグをリセット
                hasReachedPiOverTwoY = false;
            }

            // X軸の位相が±π/2に到達したかどうかを確認
            float phaseX = timeCounterX % (2 * Mathf.PI);
            if ((Mathf.Abs(phaseX - Mathf.PI / 2) < phaseTolerance || Mathf.Abs(phaseX + Mathf.PI / 2) < phaseTolerance) && !hasReachedPiOverTwoX)
            {
                // もし負の周波数が続きすぎた場合は正の値に変更
                if (negativeFrequencyCounter >= maxNegativeFrequencyCount)
                {
                    currentFrequencyX = Random.Range(minFrequencyX, maxFrequencyX);
                    negativeFrequencyCounter = 0; // カウンターをリセット
                }
                else
                {
                    // 位相が±π/2に近づいたので、X軸の周波数をランダムに変更
                    currentFrequencyX = Random.Range(minFrequencyX, maxFrequencyX);
                }

                // 周波数が負であるかどうかを確認し、カウンターを増やす
                if (currentFrequencyX < 0)
                {
                    negativeFrequencyCounter++;
                }
                else
                {
                    negativeFrequencyCounter = 0; // 正の周波数が出たらカウンターをリセット
                }

                hasReachedPiOverTwoX = true; // フラグを立てる
            }
            else if (phaseX < Mathf.PI / 2 && phaseX > -Mathf.PI / 2)
            {
                // 位相が再び0に戻るまでフラグをリセット
                hasReachedPiOverTwoX = false;
            }

            // BirdControlに仮想マウスの位置を設定
            // birdControl.SetSimulatedMousePosition(simulatedMousePosition);
        }
    }
}

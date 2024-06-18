using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class Timer : MonoBehaviour
{
    public Text timerText;
    private float timer = 0f;
    private float clearTime = 60f;
    public BirdControl BirdControl;
    public GameObject LongHand;
    public GameObject ShortHand;
    public GameObject TimerBack;
    public GameObject ClearUI;
    void Update()
    {
        timer += Time.deltaTime;

        // 分と秒に変換
        float minutes = Mathf.FloorToInt(timer / 60f);
        float seconds = Mathf.FloorToInt(timer % 60f);
        TimerBack.GetComponent<Image>().fillAmount = Mathf.FloorToInt(timer)/ clearTime;
        // タイマーをテキストに表示
        //timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (timer > clearTime)
        {
            ClearUI.SetActive(true);

            StartCoroutine(ToTitle(3f));
        }
        Hand();
       
    }

    private void Hand()
    {
        LongHand.transform.rotation = Quaternion.Euler(LongHand.transform.eulerAngles.x, LongHand.transform.eulerAngles.y, BirdControl.newRotationZR);
        ShortHand.transform.rotation = Quaternion.Euler(ShortHand.transform.eulerAngles.x, ShortHand.transform.eulerAngles.y, -BirdControl.newRotationZL + 180);

    }

   
    private IEnumerator ToTitle(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene("Title");
    }

}
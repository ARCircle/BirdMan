using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class Title : MonoBehaviour
{
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            // FキーとDキーで左翼のZ軸回転を制御
            if (keyboard.spaceKey.isPressed)
            {
               
                    // "Game"という名前のシーンに移動
                    SceneManager.LoadScene("Game");
                
            }
        }
    }
}

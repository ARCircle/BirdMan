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
            // F�L�[��D�L�[�ō�����Z����]�𐧌�
            if (keyboard.spaceKey.isPressed)
            {
               
                    // "Game"�Ƃ������O�̃V�[���Ɉړ�
                    SceneManager.LoadScene("Game");
                
            }
        }
    }
}

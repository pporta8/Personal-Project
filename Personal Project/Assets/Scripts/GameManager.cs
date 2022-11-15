using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public CharacterMovement characterMovement;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(characterMovement.health >= 0)
        {
            Scene thisScene = SceneManager.GetActiveScene();

            SceneManager.LoadScene(thisScene.name);
        }
    }
}

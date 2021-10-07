using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveUp : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ClickGiveUp()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnTitle");
    }
}

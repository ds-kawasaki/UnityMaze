using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGenerate : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TouchInput.Started += info =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("scnGenerateClusting");
        };
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGenerate : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }


    public void ClickDefeatStick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnGenerateDefeatStick");
    }

    public void ClickClustring()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnGenerateClusting");
    }
}

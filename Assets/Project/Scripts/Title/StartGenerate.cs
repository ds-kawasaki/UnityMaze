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

    public void ClickWallExtend()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnGenerateWallExtend");
    }

    public void ClickDig()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnGenerateDig");
    }

    public void ClickClustring()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("scnGenerateClusting");
    }
}

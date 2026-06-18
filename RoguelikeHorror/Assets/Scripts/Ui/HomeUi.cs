using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class HomeUi : MonoBehaviour
{
    public GameObject Panel;

    public void ExitButton()
    {
        SceneManager.LoadScene("Main");
    }
}

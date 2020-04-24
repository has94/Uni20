using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void OnStudentClick()
    {
        SceneManager.LoadScene(Constant.SCENE_STUDENT);
    }

    public void OnTeacherClick()
    {
        SceneManager.LoadScene(Constant.SCENE_TEACHER);
    }

    public void OnQuitClick()
    {
        Application.Quit();
    }
}

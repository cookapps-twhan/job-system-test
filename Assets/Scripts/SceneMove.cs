using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using UnityEngine.SceneManagement;

public class SceneMove : MonoBehaviour {
    public void OnClickAllObjectJob() {
        SceneManager.LoadScene("AllObjectJob");
    }
    
    public void OnClickAllObjectUpdate() {
        SceneManager.LoadScene("AllObjectUpdate");
    }
    
    public void OnClickJobParallelFor() {
        SceneManager.LoadScene("JobParallelFor");
    }
    
    public void OnClickJobParallelForTransform() {
        SceneManager.LoadScene("JobParallelForTransform");
    }
    
    public void OnClickBack() {
        SceneManager.LoadScene("Start");
    }
}

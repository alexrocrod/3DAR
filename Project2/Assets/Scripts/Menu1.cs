using System.Collections;  
using System.Collections.Generic;  
using UnityEngine;  
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using static GlobalReferences;

// Main menu script
public class Menu1 : MonoBehaviour
{
    public GameObject iField;
    private string dataPath;

    public GameObject toggleReload;
    private bool reload;

    public GameObject toggleInvert;
    private bool invert;

    public GameObject toggleNeon;
    private bool neon;

    // On button start click start the PCAdv1 scene
    public void StartPCAdv() {

        dataPath = iField.GetComponent<TMP_InputField>().text;
        GlobalReferences.CrossSceneText = dataPath;

        reload = toggleReload.GetComponent<Toggle>().isOn;
        GlobalReferences.CSReload = reload;

        invert = toggleInvert.GetComponent<Toggle>().isOn;
        GlobalReferences.CSInvert = invert;

        neon = toggleNeon.GetComponent<Toggle>().isOn;
        GlobalReferences.CSNeon = neon;

        SceneManager.LoadScene("Scenes/PCAdv1"); 
        
    }
}

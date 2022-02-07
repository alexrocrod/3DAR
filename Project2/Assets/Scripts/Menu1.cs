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
    // DataPath
    public GameObject iField;
    private string dataPath;

    // Reload
    public GameObject toggleReload;
    private bool reload;

    // Invert YZ
    public GameObject toggleInvert;
    private bool invert;

    // Neon (Changeable Color)
    public GameObject toggleNeon;
    private bool neon;

    // Load the PCAdv1 scene when |Start| is pressed
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

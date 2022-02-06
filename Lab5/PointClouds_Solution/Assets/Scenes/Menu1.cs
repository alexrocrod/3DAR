using System.Collections;  
using System.Collections.Generic;  
using UnityEngine;  
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using static GlobalReferences;


public class Menu1 : MonoBehaviour
{
    [SerializeField] private GameObject iField;
    private string dataPath;

    [SerializeField] private GameObject toggleReload;
    private bool reload;

    [SerializeField] private GameObject toggleInvert;
    private bool invert;

    [SerializeField] private GameObject toggleNeon;
    private bool neon;

    [SerializeField] private GameObject sliderScale;
    private float scale;

    public void PlayGame() {

        Debug.Log("Sending text: " + iField.GetComponent<TMP_InputField>().text);
        dataPath = iField.GetComponent<TMP_InputField>().text;
        GlobalReferences.CrossSceneText = dataPath;

        Debug.Log("Reload: " + toggleReload.GetComponent<Toggle>().isOn);
        reload = toggleReload.GetComponent<Toggle>().isOn;
        GlobalReferences.CSReload = reload;

        Debug.Log("Invert: " + toggleInvert.GetComponent<Toggle>().isOn);
        invert = toggleInvert.GetComponent<Toggle>().isOn;
        GlobalReferences.CSInvert = invert;

        Debug.Log("Neon: " + toggleNeon.GetComponent<Toggle>().isOn);
        neon = toggleNeon.GetComponent<Toggle>().isOn;
        GlobalReferences.CSNeon = neon;

        Debug.Log("Scale: " + sliderScale.GetComponent<Slider>().value);
        scale = sliderScale.GetComponent<Slider>().value;
        GlobalReferences.CSScale = scale;

        SceneManager.LoadScene("PCAdv1"); 
        
    }
}

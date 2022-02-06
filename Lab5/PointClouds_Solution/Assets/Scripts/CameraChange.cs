using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GlobalReferences;


public class CameraChange : MonoBehaviour
{
    public GameObject tg;
    public GameObject car;
    Toggle m_toggle;

    Slider m_sliderPS;
    Slider m_sliderC;
    public GameObject sliderPSize;
    public GameObject sliderColor;
    public Material MatVC;
    public Material MatNeon;

    private bool neon = false;

    void Start()
    {
        m_toggle = tg.GetComponent<Toggle>();
        m_toggle.onValueChanged.AddListener(delegate {ToggleValueChanged(m_toggle);});

        m_sliderPS = sliderPSize.GetComponent<Slider>();
        m_sliderPS.onValueChanged.AddListener(delegate {SliderValueChanged(m_sliderPS);});

        m_sliderC = sliderColor.GetComponent<Slider>();
        m_sliderC.onValueChanged.AddListener(delegate {SliderColorChanged(m_sliderC);});

        neon = GlobalReferences.CSNeon;


    }

    void ToggleValueChanged(Toggle change)
    {
        if (m_toggle.isOn){
            GetComponent<CarCameraScript>().enabled = false;
            car.GetComponent<NewMove>().enabled = false;
            GetComponent<UnityEngine.Rendering.FreeCamera>().enabled = true;
        }
        else {
            GetComponent<CarCameraScript>().enabled = true;
            car.GetComponent<NewMove>().enabled = true;
            GetComponent<UnityEngine.Rendering.FreeCamera>().enabled = false;
        }
        

    }

    void SliderValueChanged(Slider change)
    {
        if (!neon)
            MatVC.SetFloat("_PointSize", m_sliderPS.value);
        else 
            MatNeon.SetFloat("_PointSize", m_sliderPS.value);


    }

    void SliderColorChanged(Slider change)
    {
        if (neon)
            MatNeon.SetColor("_Color", Color.HSVToRGB(m_sliderC.value, 1, 1));
    }

}

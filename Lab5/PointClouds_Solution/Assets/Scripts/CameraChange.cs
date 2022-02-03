using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraChange : MonoBehaviour
{
    public GameObject tg;
    public GameObject car;
    Toggle m_toggle;

    void Start()
    {
        m_toggle = tg.GetComponent<Toggle>();
        m_toggle.onValueChanged.AddListener(delegate {ToggleValueChanged(m_toggle);});

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

}

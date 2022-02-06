using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static GlobalReferences;


public class Interface : MonoBehaviour
{
    // Free Camera Toggle
    public GameObject tg;
    public GameObject car;
    private Toggle m_toggle;

    // Point Size Slider
    Slider m_sliderPS;
    public GameObject sliderPSize;

    // Color Slider
    public GameObject sliderColor;
    private Slider m_sliderC;
    public Image sliderHandle;
    public Image sliderBg;
    private Texture2D colorTex;

    // Materials
    private bool neon = false;
    public Material MatVC;
    public Material MatNeon;

    void Start()
    {
        m_toggle = tg.GetComponent<Toggle>();
        m_toggle.onValueChanged.AddListener(delegate {ToggleValueChanged(m_toggle);});

        m_sliderPS = sliderPSize.GetComponent<Slider>();
        m_sliderPS.onValueChanged.AddListener(delegate {SliderValueChanged(m_sliderPS);});

        m_sliderC = sliderColor.GetComponent<Slider>();
        m_sliderC.onValueChanged.AddListener(delegate {SliderColorChanged(m_sliderC);});

        neon = GlobalReferences.CSNeon;

        // Create Hue Slider
        int density = 360;
        colorTex = ColorStrip(density);
        Rect rect = new Rect(0, 0, density, 1);
        sliderBg.sprite = Sprite.Create(colorTex, rect, rect.center);

        // defaults:
        SliderColorChanged(m_sliderC);
        SliderValueChanged(m_sliderPS);

    }

    // change camera behaviour
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

    // change point size
    void SliderValueChanged(Slider change)
    {
        if (!neon)
            MatVC.SetFloat("_PointSize", m_sliderPS.value);
        else 
            MatNeon.SetFloat("_PointSize", m_sliderPS.value);
    }

    // change slider and point color
    void SliderColorChanged(Slider change)
    {
        sliderHandle.color = Color.HSVToRGB(m_sliderC.value, 1, 1);
        if (neon)
            MatNeon.SetColor("_Color", Color.HSVToRGB(m_sliderC.value, 1, 1));
    }

    // Color strip texture
    private Texture2D ColorStrip (int density)
    {
        Texture2D hueTex = new Texture2D (density, 1);
        Color[] colors = new Color[density];

        for (int k = 0; k < density; k++)
            colors[k] = Color.HSVToRGB((1.0f * k) / density, 1, 1);
    
        hueTex.SetPixels(colors);
        hueTex.Apply();
        return hueTex;
    }

}

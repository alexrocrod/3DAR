using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSlider : MonoBehaviour
{
    // Slider components
    public Slider slider;
    public Image sliderHandle;
    public Image sliderBg;
    // public Texture2D sliderBg;

    // Assign our texture
    private Texture2D colorTex;

    // Use this for initialization
    void Awake ()
    {   
        // Build our texture
        this.colorTex = this.ColorStrip(360);

        // Build our sprite
        var rect = new Rect(0, 0, this.colorTex.width, this.colorTex.height);

        // Connect our slider
        this.slider.onValueChanged.AddListener(this.OnValueChanged);
        this.sliderBg.sprite = Sprite.Create(this.colorTex, rect, rect.center);

        // Assign our initial color
        this.OnValueChanged(this.slider.value);
    }

    // Slider value-changed callback
    private void OnValueChanged(float value)
    {
        this.sliderHandle.color = Color.HSVToRGB(value, 1, 1);
    }

    // Function to return a color strip texture from a given color density
    private Texture2D ColorStrip (int density)
    {
        // Use that density to control how large of a texture to make
        var hueTex = new Texture2D (density, 1);

        // It will also control our slider colors themselves
        var colors = new Color[density];
        for (int k = 0; k < density; k++)
        {
            colors[k] = Color.HSVToRGB ((1.0f * k) / density, 1, 1);
        }

        // Apply them
        hueTex.SetPixels (colors);
        hueTex.Apply ();

        // Return it
        return hueTex;
    }
}
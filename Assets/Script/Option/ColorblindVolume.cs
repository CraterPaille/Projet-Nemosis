using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class IntParameter : VolumeParameter<int>
{
    public IntParameter(int value, bool overrideState = false) : base(value, overrideState) { }
}

[System.Serializable]
[VolumeComponentMenu("Custom/Colorblind")]
public class ColorblindVolume : VolumeComponent, IPostProcessComponent
{
    // 0 = Normal, 1 = Protanopia, 2 = Deuteranopia, 3 = Tritanopia
    public IntParameter mode = new IntParameter(0);

    public bool IsActive() => mode.value != 0;   // actif seulement si pas égale 0
    public bool IsTileCompatible() => true;
}
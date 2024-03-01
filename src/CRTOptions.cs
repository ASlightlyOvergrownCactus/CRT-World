using BepInEx.Logging;
using JetBrains.Annotations;
using Menu.Remix.MixedUI;
using Menu.Remix.MixedUI.ValueTypes;
using UnityEngine;

namespace MerShaderLoader;

public class CRTOptions : OptionInterface
{
    public static readonly CRTOptions Instance = new();
    public static Configurable<float> intensity;
    public static float intensityF;
    public static Configurable<float> scanLineDarkness;
    public static float scanLineDarknessF;
    public static Configurable<float> distortion;
    public static float distortionF;
    public static Configurable<bool> screenDist;
    public static bool screenDistF;

    [CanBeNull]
    public static UIelement[] uIelements;

    public CRTOptions()
    {
        intensity = config.Bind<float>("intensity", 25, new ConfigAcceptableRange<float>(0, 100));
        scanLineDarkness = config.Bind<float>("scanLines", 50, new ConfigAcceptableRange<float>(0, 100));
        distortion = config.Bind<float>("distortion", 10, new ConfigAcceptableRange<float>(0, 100));
        screenDist = config.Bind<bool>("screenDist", true);
    }

    public override void Initialize()
    {
        OpTab opTab = new OpTab(this, "Options");
        Tabs = new[]
        {
            opTab
        };
        
        const int sliderBarLength = 135;
        const int rightSidePos = 360;
        const int leftSidePos = 60;
        #nullable enable

        uIelements = new UIelement[]
        {
            new OpLabel(200, 575, Translate("CRT Shader Options"), true) {alignment=FLabelAlignment.Center},
            
            // Make the options on the right side
            new OpFloatSlider(intensity, new Vector2(rightSidePos, 520), sliderBarLength) {description=Translate("RGB Intensity of CRT")},
            new OpLabel(rightSidePos, 500, Translate("Intensity of CRT")),
            
            new OpFloatSlider(scanLineDarkness, new Vector2(rightSidePos, 440), sliderBarLength) {description=Translate("Darkness Intensity of Scanlines")},
            new OpLabel(rightSidePos, 420, Translate("Intensity of Scanlines")),
            
            new OpFloatSlider(distortion, new Vector2(rightSidePos, 360), sliderBarLength) {description=Translate("Distortion Intensity of Screen")},
            new OpLabel(rightSidePos, 340, Translate("Intensity of distortion")),
            
            // Make the options on the left side
            new OpCheckBox(screenDist, new Vector2(leftSidePos, 520)) {description=Translate("Controls the full screen edge distortion")},
            new OpLabel(leftSidePos+30, 523, Translate("CRT Screen Edge"))
        };
        opTab.AddItems(uIelements);
        
    }

    public override void Update()
    {
        if (uIelements != null)
        {
            intensityF = ((OpFloatSlider)uIelements[1]).GetValueFloat();
            scanLineDarknessF = ((OpFloatSlider)uIelements[3]).GetValueFloat();
            distortionF = ((OpFloatSlider)uIelements[5]).GetValueFloat();
            screenDistF = ((OpCheckBox)uIelements[7]).GetValueBool();
        }
    }
}
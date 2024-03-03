using System.Collections.Generic;
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
    
    public static Configurable<float> brightness;
    public static float brightnessF;
    public static Configurable<float> offset;
    public static float offsetF;
    public static Configurable<float> contrast;
    public static float contrastF;
    public static Configurable<string> dither;
    public static string ditherF;
    public static Configurable<string> palette;
    public static string paletteF;
    public static Configurable<bool> usePal;
    public static bool usePalF;

    // Most palettes here were gotten from https://lospec.com/palette-list / They have a lot of great palettes! Go check em out!
    public static List<ListItem> palettes = new List<ListItem>()
    {
        // 4-color palettes
        new ("Chrome4"),
        new ("Hollow4"),
        new ("RetroGB4"),
        new ("Jojo4"),
        new ("Amber4"),
        new ("Blood4"),
        new ("Horror4"),
        new ("Lava4"),
        new ("Miku4"),
        new ("Aqua4"),
        new ("Wish4"),
        new ("Moonlight4"),
        new ("Royal4"),
        // 8-color palettes
        new ("Ammo8"),
        new ("Bi8"),
        new ("Borkfest8"),
        new ("Citrink8"),
        new ("Dream8"),
        new ("Fox8"),
        new ("Gothic8"),
        new ("Lava8"),
        new ("Morning8"),
        new ("Nebula8"),
        new ("Ocean8"),
        new ("Paper8"),
        new ("Parchment8"),
        new ("Purple8"),
        new ("RetroGB8"),
        new ("RustGold8"),
        new ("Chimera8"),
        new ("Winter8")
    };

    [CanBeNull]
    public static UIelement[] uIelements;

    public CRTOptions()
    {
        intensity = config.Bind<float>("CRTWorld_intensity", 25, new ConfigAcceptableRange<float>(0, 100));
        scanLineDarkness = config.Bind<float>("CRTWorld_scanLines", 50, new ConfigAcceptableRange<float>(0, 100));
        distortion = config.Bind<float>("CRTWorld_distortion", 10, new ConfigAcceptableRange<float>(0, 100));
        screenDist = config.Bind<bool>("CRTWorld_screenDist", true);
        brightness = config.Bind<float>("CRTWorld_brightness", 50, new ConfigAcceptableRange<float>(0, 100));
        offset = config.Bind<float>("CRTWorld_offset", 25, new ConfigAcceptableRange<float>(0, 100));
        contrast = config.Bind<float>("CRTWorld_contrast", 80, new ConfigAcceptableRange<float>(0, 100));
        dither = config.Bind<string>("CRTWorld_dither", "Bayer16");
        palette = config.Bind<string>("CRTWorld_palette", "Chrome4");
        usePal = config.Bind<bool>("CRTWorld_usepal", true);
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
            new OpLabel(leftSidePos+30, 523, Translate("CRT Screen Edge")),
            
            new OpFloatSlider(brightness, new Vector2(leftSidePos, 440), sliderBarLength) {description=Translate("Brightness of gameboy lines.")},
            new OpLabel(leftSidePos, 415, Translate("\nBrightness of gameboy grid. \nValues lower than 50 are darker, \nhigher than 50 are brighter.")),
            
            // put palette list here at y 260 label y 290 right
            new OpComboBox(palette, new Vector2(rightSidePos, 180), 100, palettes),
            new OpLabel(rightSidePos, 210, Translate("Preset Palettes to use.")),
            
            new OpCheckBox(usePal, new Vector2(rightSidePos, 280)) {description=Translate("Toggles the Posterization shader.")},
            new OpLabel(rightSidePos+30, 260, Translate("Toggle Posterization.")),
            
            // put dither list here at y 340 label y 360 left
            new OpListBox(dither, new Vector2(leftSidePos, 250), 100, new List<ListItem>{new ListItem("Bayer16"),new ListItem("Bayer8"),new ListItem("Bayer4"),new ListItem("Bayer2")}),
            new OpLabel(leftSidePos, 370, Translate("Type of Bayer dither.")),
            
            new OpFloatSlider(offset, new Vector2(leftSidePos, 200), sliderBarLength) {description=Translate("Offset of palette.")},
            new OpLabel(leftSidePos, 175, Translate("Offsets the palette.\nValues lower than 50 offset darker,\nhigher than 50 offset lighter")),
            
            new OpFloatSlider(contrast, new Vector2(leftSidePos, 120), sliderBarLength) {description=Translate("Contrast of palette shader.")},
            new OpLabel(leftSidePos, 100, Translate("Contrast of the palette."))
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
            brightnessF = ((OpFloatSlider)uIelements[9]).GetValueFloat();
            paletteF = ((OpComboBox)uIelements[11])._GetDisplayValue();
            usePalF = ((OpCheckBox)uIelements[13]).GetValueBool();
            ditherF = ((OpListBox)uIelements[15])._GetDisplayValue();
            offsetF = ((OpFloatSlider)uIelements[17]).GetValueFloat();
            contrastF = ((OpFloatSlider)uIelements[19]).GetValueFloat();
        }
    }
}
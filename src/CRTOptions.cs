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
    //This option is useless all the palettes are loading
    public static Configurable<bool> loadPalettes;
    public static bool loadPalettesF;

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

    public static Configurable<bool> vcrBlur;
    public static bool vcrBlurF;
    public static Configurable<float> smear;
    public static float smearF;
    public static Configurable<float> wiggle;
    public static float wiggleF;
    public static Configurable<int> blurSamples;
    public static int blurSamplesF;
    
    // Most palettes here were gotten from https://lospec.com/palette-list / They have a lot of great palettes! Go check em out!
    //Takes all the palettes with data and name from a loop
    public static List<ListItem> palettes = new();



    [CanBeNull]
    public static UIelement[] uIelements;

    public CRTOptions()
    {
        intensity = config.Bind<float>("CRTWorld_intensity", 25, new ConfigAcceptableRange<float>(0, 100));
        scanLineDarkness = config.Bind<float>("CRTWorld_scanLines", 50, new ConfigAcceptableRange<float>(0, 100));
        distortion = config.Bind<float>("CRTWorld_distortion", 10, new ConfigAcceptableRange<float>(0, 100));
        screenDist = config.Bind<bool>("CRTWorld_screenDist", true);
        brightness = config.Bind<float>("CRTWorld_brightness", 50, new ConfigAcceptableRange<float>(0, 100));
        offset = config.Bind<float>("CRTWorld_offset", 20, new ConfigAcceptableRange<float>(0, 100));
        contrast = config.Bind<float>("CRTWorld_contrast", 100, new ConfigAcceptableRange<float>(0, 100));
        dither = config.Bind<string>("CRTWorld_dither", "Bayer2");
        palette = config.Bind<string>("CRTWorld_palette", "Ammo8");
        usePal = config.Bind<bool>("CRTWorld_usepal", true);

        vcrBlur = config.Bind<bool>("CRTWorld_vcrBlur", true);
        smear = config.Bind<float>("CRTWorld_smear", 50, new ConfigAcceptableRange<float>(0, 100));
        wiggle = config.Bind<float>("CRTWorld_wiggle", 5, new ConfigAcceptableRange<float>(0, 100));
        blurSamples = config.Bind<int>("CRTWorld_blurSamples", 15, new ConfigAcceptableRange<int>(0, 30));
    }

    public override void Initialize()
    {
        OpTab opTab = new(this, "Options");
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
            new OpComboBox(palette, new Vector2(rightSidePos, 20), 100, palettes),
            new OpLabel(rightSidePos, 0, Translate("Preset and Downloaded Palettes to use.")),
            
            new OpCheckBox(usePal, new Vector2(rightSidePos, 280)) {description=Translate("Toggles the Posterization shader.")},
            new OpLabel(rightSidePos+30, 283, Translate("Toggle Posterization.")),
            
            // put dither list here at y 340 label y 360 left
            new OpListBox(dither, new Vector2(leftSidePos, 250), 100, new List<ListItem>{new("Bayer16"),new("Bayer8"),new("Bayer4"),new("Bayer2")}),
            new OpLabel(leftSidePos, 370, Translate("Type of Bayer dither.")),
            
            new OpFloatSlider(offset, new Vector2(rightSidePos, 200), sliderBarLength) {description=Translate("Offset of palette.")},
            new OpLabel(rightSidePos, 245, Translate("Offsets the palette.\nValues lower than 50 offset darker,\nhigher than 50 offset lighter")),
            
            new OpFloatSlider(contrast, new Vector2(rightSidePos, 150), sliderBarLength) {description=Translate("Contrast of palette shader.")},
            new OpLabel(rightSidePos, 170, Translate("Contrast of the palette.")),
            
            new OpCheckBox(vcrBlur, new Vector2(leftSidePos, 200)) {description=Translate("Applies the VCR Shader effect")},
            new OpLabel(leftSidePos+30, 203, Translate("VCR Effect")),
            
            new OpFloatSlider(smear, new Vector2(leftSidePos, 140), sliderBarLength) {description=Translate("Color Smear of VCR.")},
            new OpLabel(leftSidePos, 120, Translate("Smears color for VCR Shader")),
            
            new OpFloatSlider(wiggle, new Vector2(leftSidePos, 80), sliderBarLength) {description=Translate("Wiggle intensity of VCR.")},
            new OpLabel(leftSidePos, 60, Translate("Makes VCR screen wiggle with intensity.")),
            
            new OpSliderTick(blurSamples, new Vector2(leftSidePos, 20), sliderBarLength) {description=Translate("Amount of blur samplings.")},
            new OpLabel(leftSidePos, 0, Translate("Amount of samples for blur.")),
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
            vcrBlurF = ((OpCheckBox)uIelements[21]).GetValueBool();
            smearF = ((OpFloatSlider)uIelements[23]).GetValueFloat();
            wiggleF = ((OpFloatSlider)uIelements[25]).GetValueFloat();
            blurSamplesF = (int)((OpSliderTick)uIelements[27]).GetValueFloat();
        }
    }
}
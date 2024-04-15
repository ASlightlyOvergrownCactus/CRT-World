using System.Linq;
using System.Runtime.CompilerServices;

namespace MerShaderLoader;

[BepInPlugin(GUID: MOD_ID, Name: MOD_NAME, Version: VERSION)]
sealed class Plugin : BaseUnityPlugin
{
    public const string MOD_ID = "cactus.crt";
    public const string MOD_NAME = "CRT";
    public const string VERSION = "1.2";
    public const string AUTHORS = "SlightlyOverGrownCactus";

    static bool loaded = false;
    public static Shader CRTShader;
    public static Shader GameShader;
    public static Texture2D[] BayerTextures = new Texture2D[4];
    public static List<Texture2D> palettes = new();
    public static Dictionary<string, Texture2D> CustomPalettes = new();

    public static MaterialPropertyBlock bayerBlock;

    public void OnEnable()
    {
        try
        {
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            new Hook(typeof(Futile).GetMethod("get_mousePosition"), NewMousePos);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            Debug.LogError(e);
            throw new Exception("Exception from CRTWorld: " + e);
        }
    }

    private Vector3 NewMousePos(Func<Vector3> orig)
    {
        Vector3 mousePos = orig();
        // Implement fixed mouse position later, this is actually a really complicated issue apparently and for now doesnt affect gameplay much.
        /*
        if (Camera.main.gameObject.GetComponent<CrtScreen>() != null)
        {
            mousePos.x /= Camera.main.pixelWidth;
            mousePos.y /= Camera.main.pixelHeight;

            Vector2 p = new Vector2(mousePos.x, mousePos.y);
            float z = 1.0f / (CRTOptions.distortion.Value / 100f);
            float m = 0.95f;
            Vector2 numerator = (m) * (p - new Vector2(0.5f, 0.5f)) * Mathf.Sqrt(Mathf.Pow(z, 2f) + Mathf.Pow(0.5f, 2f));
            float denominator =
                Mathf.Sqrt(Mathf.Pow(m * (p.x - 0.5f), 2f) + Mathf.Pow(m * (p.y - 0.5f), 2f) + Mathf.Pow(z, 2f));
            mousePos = numerator / denominator;
            mousePos.x += 0.5f;
            mousePos.y += 0.5f;
            
            mousePos.x *= Camera.main.pixelWidth;
            mousePos.y *= Camera.main.pixelHeight;
        }*/
        return mousePos;
    }

    private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        try
        {
            if (loaded) return;
            loaded = true;

            MachineConnector.SetRegisteredOI("cactus.crt", CRTOptions.Instance);
            //Set on flase because this is from the previous build of the game, still waiting for Update
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/crtscreen"/*, false*/)); // Loads the asset bundle from your mod's file path

            bayerBlock = new();

            // Dithering Textures
            BayerTextures[0] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer2tile16.png");
            BayerTextures[1] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer4tile8.png");
            BayerTextures[2] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer8tile4.png");
            BayerTextures[3] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer16tile2.png");

            //This loads all the palettes from the bundle and the folder
            LoadPalettes(bundle);

            // Shaders
            GameShader = bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/GameGirlScreen.shader"); // Loads the shader itself from the asset bundle
            CRTShader = bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/CRTScreen.shader"); // Loads the shader itself from the asset bundle
            Camera.main.gameObject.AddComponent<CrtScreen>();

            //Add all the palettes to the RemixMenu
            foreach (var paletteName in CrtScreen.GetPaletteNames())
            {
                if (paletteName != "bayer16tile2" && paletteName != "bayer8tile4" && paletteName != "bayer4tile8" && paletteName != "bayer2tile16")
                    CRTOptions.palettes.Add(new ListItem(paletteName));
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
            Debug.LogException(ex);
            throw new Exception("Error trying to load OnModsInit CRT 1.1");
        }
        
    }

    public static void LoadPalettes(AssetBundle bundle)
    {
        // Take all the image filenames inside the Bundle file
        string[] assetNames = bundle.GetAllAssetNames();

        // Loop over the first part of the process
        foreach (var assetName in assetNames)
        {
            //Check for the bayer pngs
            if(assetName == "Assets/shaders 1.9.03/bayer16tile2.png" || assetName == "Assets/shaders 1.9.03/bayer2tile16.png" || assetName == "Assets/shaders 1.9.03/bayer4tile8.png" || assetName == "Assets/shaders 1.9.03/bayer8tile4.png")
            {
                continue;
            }
            
            if (IsImageFile(assetName))
            {
                //Debug.LogError(assetName);
                // Load the Texture2D from the bundle
                var paletteTexture = bundle.LoadAsset<Texture2D>(assetName);
                // Check if the texture is loaded successfully
                if (paletteTexture != null)
                {
                    // Extract the texture name
                    var textureName = Path.GetFileNameWithoutExtension(assetName);
                    // Set the name of the texture
                    paletteTexture.name = textureName;
                    // Add the texture to the palettes list
                    palettes.Add(paletteTexture);
                }
            }
        }

        // Get a list of files from the directory "crt_palettes" and its subdirectories
        var files = ListDirectory("crt_palettes", includeAll: true).Distinct().ToList();

        // Loop over the second part of the process
        foreach (var file in files)
        {
            // Check if the file has a .png extension
            if (".png".Equals(Path.GetExtension(file)))
            {
                // Read the image bytes from the file
                var imageBytes = File.ReadAllBytes(file);
                // Create a new Texture2D and load the image bytes into it
                var texture = new Texture2D(2, 2);
                if (texture.LoadImage(imageBytes))
                {
                    // Extract the texture name
                    var textureName = Path.GetFileNameWithoutExtension(file);
                    Debug.LogWarning(textureName + " loaded from outside source!");
                    // Set the name of the texture
                    texture.name = textureName;
                    // Add the texture to the palettes list
                    palettes.Add(texture);
                }
            }
        }
    }

    public static string[] ListDirectory(string path, bool directories = false, bool includeAll = false)
    {
        // Initialize two lists to store directory paths and file paths
        List<string> list = new(); // Stores the resulting paths
        List<string> list2 = new(); // Stores directories that have been processed

        // If the provided path is an absolute path
        if (Path.IsPathRooted(path))
        {
            // Return an array of directories or files in the specified path
            return directories ? Directory.GetDirectories(path) : Directory.GetFiles(path);
        }

        // If palettes are not set to be loaded and there are no installed mods
        if (ModManager.ActiveMods.Any())
        {
            // Add the merged mods directory and the root folder directory to the list of directories to be searched
            list2.Add(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"));
            for (int i = 0; i < ModManager.ActiveMods.Count; i++)
            {
                if(ModManager.ActiveMods[i].enabled)
                    list2.Add(ModManager.ActiveMods[i].path);
            }
        }

        // Add the root folder directory to the list of directories to be searched
        list2.Add(Custom.RootFolderDirectory());

        // Iterate over each directory in the list of directories to be searched
        foreach (string item in list2)
        {
            // Combine the item directory path with the provided path
            string path2 = Path.Combine(item, path.ToLowerInvariant());
            // If the combined path does not exist, skip to the next iteration
            if (!Directory.Exists(path2))
            {
                continue;
            }

            // Get an array of directories or files in the combined path
            string[] array = directories ? Directory.GetDirectories(path2) : Directory.GetFiles(path2);
            // Iterate over each directory or file in the array
            for (int j = 0; j < array.Length; j++)
            {
                // Convert the path to lowercase for uniformity
                string text = array[j].ToLowerInvariant();
                // Extract the filename from the path
                string fileName = Path.GetFileName(text);
                // If the filename is not already in the list of processed directories or includeAll flag is set
                if (!list2.Contains(fileName) || includeAll)
                {
                    // Add the path to the result list
                    list.Add(text);
                    // If includeAll flag is not set
                    if (!includeAll)
                    {
                        // Add the filename to the list of processed directories
                        list2.Add(fileName);
                    }
                }
            }
        }

        // Convert the list of paths to an array and return it
        return list.ToArray();
    }

    //Silly check for imgs inside Bundle
    private static bool IsImageFile(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLower();
        return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
    }
}

public class CrtScreen : MonoBehaviour
{
    private static bool menuLoaded = false;

    public Material gameMat;
    [Range(-1, 1)] public float brightness = 0.3f;
    [Range(0, 1)] public float offset = 0.25f;
    [Range(0, 1)] public float contrast = 0.8f;
    private bool usePal = false;

    public Material crtMat;
    [Range(0, 1)] public float verts_force = 0.51f;
    [Range(0, 1)] public float verts_force_2 = 0.255f;
    [Range(0, 1)] public float verts_force_3 = 0.8f;
    [Range(0, 1)] public float screen_dist = 1.0f;
    [Range(0, 1)] public float smear = 0.5f;
    [Range(0, 1)] public float wiggle = 0.5f;
    [Range(0, 30)] public int blur_samples = 15;
    [Range(0, 1)] public float vcr_blur = 1.0f;
    

    private void Awake()
    {
        gameMat = new Material(Plugin.GameShader);
        crtMat = new Material(Plugin.CRTShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (usePal)
        {
            RenderTexture gameTemp = RenderTexture.GetTemporary(src.descriptor);
            gameMat.SetTexture("_BayerTex", Plugin.bayerBlock.GetTexture("_BayerTex"));
            gameMat.SetTexture("_BayPal", Plugin.bayerBlock.GetTexture("_BayPal"));
            gameMat.SetFloat("_Brightness", brightness);
            gameMat.SetFloat("_Offset", offset);
            gameMat.SetFloat("_Contrast", contrast);
            Graphics.Blit(src, gameTemp, gameMat);

            crtMat.SetFloat("_VertsColor", 1 - verts_force);
            crtMat.SetFloat("_VertsColor2", 1 - verts_force_2);
            crtMat.SetFloat("_VertsColor3", 1 - verts_force_3);
            crtMat.SetFloat("_ScreenDist", screen_dist); // No distortion at 0.0, screen edge dist at 1.0
            crtMat.SetFloat("_Smear", smear);
            crtMat.SetFloat("_Wiggle", wiggle);
            crtMat.SetInt("_BlurSamples", blur_samples);
            crtMat.SetFloat("_VCRBlur", vcr_blur);
            Graphics.Blit(gameTemp, dest, crtMat);

            RenderTexture.ReleaseTemporary(gameTemp);
        }
        else
        {
            crtMat.SetFloat("_VertsColor", 1 - verts_force);
            crtMat.SetFloat("_VertsColor2", 1 - verts_force_2);
            crtMat.SetFloat("_VertsColor3", 1 - verts_force_3);
            crtMat.SetFloat("_ScreenDist", screen_dist); // No distortion at 0.0, screen edge dist at 1.0
            crtMat.SetFloat("_Smear", smear);
            crtMat.SetFloat("_Wiggle", wiggle);
            crtMat.SetInt("_BlurSamples", blur_samples);
            crtMat.SetFloat("_VCRBlur", vcr_blur);
            Graphics.Blit(src, dest, crtMat);
        }
    }

    private void Update()
    {
        if (menuLoaded)
        {
            Plugin.bayerBlock.SetTexture("_BayerTex", GetDither(CRTOptions.ditherF));
            Plugin.bayerBlock.SetTexture("_BayPal", GetPalette(CRTOptions.paletteF));
            
            verts_force_3 = CRTOptions.distortionF / 100f;
            verts_force_2 = CRTOptions.intensityF / 100f;
            verts_force = CRTOptions.scanLineDarknessF / 100f;
            brightness = CRTOptions.brightnessF / 100f * 2f - 1;
            contrast = CRTOptions.contrastF / 100f;
            offset = CRTOptions.offsetF / 100f;
            usePal = CRTOptions.usePalF;
            smear = CRTOptions.smearF / 100f;
            wiggle = CRTOptions.wiggleF / 100f;
            blur_samples = CRTOptions.blurSamplesF;
            
            if (CRTOptions.screenDistF)
                screen_dist = 1.0f;
            else
            {
                screen_dist = 0.0f;
            }
            
            if (CRTOptions.vcrBlurF)
                vcr_blur = 1.0f;
            else
            {
                vcr_blur = 0.0f;
            }
        }
        else
        {
            if (CRTOptions.intensityF != 0f || CRTOptions.distortionF != 0f || CRTOptions.scanLineDarknessF != 0f || CRTOptions.brightnessF != 0f || CRTOptions.offsetF != 0f || CRTOptions.contrastF != 0f || CRTOptions.smearF != 0f || CRTOptions.wiggleF != 0f || CRTOptions.blurSamplesF != 0f)
                menuLoaded = true;
            else
            {
                Plugin.bayerBlock.SetTexture("_BayerTex", GetDither(CRTOptions.dither.Value));
                Plugin.bayerBlock.SetTexture("_BayPal", GetPalette(CRTOptions.palette.Value));
                
                verts_force_3 = CRTOptions.distortion.Value / 100f;
                verts_force_2 = CRTOptions.intensity.Value / 100f;
                verts_force = CRTOptions.scanLineDarkness.Value / 100f;
                brightness = CRTOptions.brightness.Value / 100f * 2f - 1;
                contrast = CRTOptions.contrast.Value / 100f;
                offset = CRTOptions.offset.Value / 100f;
                usePal = CRTOptions.usePal.Value;
                smear = CRTOptions.smear.Value / 100f;
                wiggle = CRTOptions.wiggle.Value / 100f;
                blur_samples = CRTOptions.blurSamples.Value;
                
                if (CRTOptions.screenDist.Value)
                    screen_dist = 1.0f;
                else
                {
                    screen_dist = 0.0f;
                }
                if (CRTOptions.vcrBlur.Value)
                    vcr_blur = 1.0f;
                else
                {
                    vcr_blur = 0.0f;
                }
            }
        }
    }

    //This switch case was change to var case
    public Texture2D GetDither(string dither)
    {
        Texture2D ditherTex = dither switch
        {
            "Bayer2" => Plugin.BayerTextures[0],
            "Bayer4" => Plugin.BayerTextures[1],
            "Bayer8" => Plugin.BayerTextures[2],
            _ => Plugin.BayerTextures[3],
        };
        return ditherTex;
    }

    //All the palette list, this was a switch case previously 
    public Texture2D GetPalette(string palette)
    {
        Dictionary<string, Texture2D> paletteMap = new();

        for (int i = 0; i < Plugin.palettes.Count; i++)
        {
            paletteMap.Add(Plugin.palettes[i].name.ToLower(), Plugin.palettes[i]);
        }

        if (paletteMap.ContainsKey(palette.ToLower()))
        {
            return paletteMap[palette.ToLower()];
        }
        else
        {
            return Plugin.palettes[0];
        }
    }

    //RemixMenu helper Method so the name of custom palettes is not null or empty
    public static List<string> GetPaletteNames()
    {
        List<string> paletteNames = new();

        foreach (var palette in Plugin.palettes)
        {
            paletteNames.Add(palette.name);
        }

        return paletteNames;
    }
}

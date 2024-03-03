using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using System.Security;
using System.Reflection;
using MonoMod.RuntimeDetour;
using UnityEngine.Assertions;
using UnityEngine.PlayerLoop;


[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace MerShaderLoader;

[BepInPlugin("cactus.crt", "CRT", "1.1")]
sealed class Plugin : BaseUnityPlugin
{
    static bool loaded = false;
    public static Shader CRTShader;
    public static Shader GameShader;
    public static Texture2D[] BayerTextures = new Texture2D[4];
    public static List<Texture2D> palettes = new List<Texture2D>();
    public static readonly int palNum = 31;

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
            Console.WriteLine("Exception from CRTWorld: " + e);
            throw;
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
        MachineConnector.SetRegisteredOI("cactus.crt", CRTOptions.Instance);
        if (!loaded)
        {
            loaded = true;
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/crtscreen", false)); // Loads the asset bundle from your mod's file path

            bayerBlock = new();

            // Dithering Textures
            BayerTextures[0] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer2tile16.png");
            BayerTextures[1] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer4tile8.png");
            BayerTextures[2] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer8tile4.png");
            BayerTextures[3] = bundle.LoadAsset<Texture2D>("Assets/shaders 1.9.03/bayer16tile2.png");
            
            // Palettes - 4 color
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/demichrome4.png")); // 0 - order of list from here for remix menu
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/hollow4.png")); // 1
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/RetroGB4.png")); // 2
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/jojo4.png")); // 3
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/amber4.png")); // 4
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/blood4.png")); // 5
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/horror4.png")); // 6
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/lava4.png")); // 7
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/miku4.png")); // 8
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/aqua4.png")); // 9
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/wish4.png")); // 10
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/moonlight4.png")); // 11
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/4color/royal4.png")); // 12
            
            // Palettes - 8 color
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/ammo8.png")); // 13
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/bi8.png")); // 14
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/borkfest8.png")); // 15
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/citrink8.png")); // 16
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/dream8.png")); // 17
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/fox8.png")); // 18
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/gothic8.png")); // 19
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/lava8.png")); // 20
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/morning8.png")); // 21
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/nebula8.png")); // 22
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/ocean8.png")); // 23
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/paper8.png")); // 24
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/parchment8.png")); // 25
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/purple8.png")); // 26
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/RetroGB8.png")); // 27
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/rustgold8.png")); // 28
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/submerchimera8.png")); // 29
            palettes.Add(bundle.LoadAsset<Texture2D>("Assets/palettes/8color/winter8.png")); // 30
            
            // Shaders
            GameShader = bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/GameGirlScreen.shader"); // Loads the shader itself from the asset bundle
            CRTShader = bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/CRTScreen.shader"); // Loads the shader itself from the asset bundle
            Camera.main.gameObject.AddComponent<CrtScreen>();
        }
    }
}

sealed class CrtScreen : MonoBehaviour
{
    private static bool menuLoaded = false;
    
    public Material gameMat = new Material(Plugin.GameShader);
    [Range(-1, 1)] public float brightness = 0.3f;
    [Range(0, 1)] public float offset = 0.25f;
    [Range(0, 1)] public float contrast = 0.8f;
    private bool usePal = false;
    
    
    public Material crtMat = new Material(Plugin.CRTShader);
    [Range(0, 1)] public float verts_force = 0.51f;
    [Range(0, 1)] public float verts_force_2 = 0.255f;
    [Range(0, 1)] public float verts_force_3 = 0.8f;
    [Range(0, 1)] public float screen_dist = 1.0f;
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
            Graphics.Blit(gameTemp, dest, crtMat);

            RenderTexture.ReleaseTemporary(gameTemp);
        }
        else
        {
            crtMat.SetFloat("_VertsColor", 1 - verts_force);
            crtMat.SetFloat("_VertsColor2", 1 - verts_force_2);
            crtMat.SetFloat("_VertsColor3", 1 - verts_force_3);
            crtMat.SetFloat("_ScreenDist", screen_dist); // No distortion at 0.0, screen edge dist at 1.0
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
            
            if (CRTOptions.screenDistF)
                screen_dist = 1.0f;
            else
            {
                screen_dist = 0.0f;
            }
        }
        else
        {
            if (CRTOptions.intensityF != 0f || CRTOptions.distortionF != 0f || CRTOptions.scanLineDarknessF != 0f || CRTOptions.brightnessF != 0f || CRTOptions.offsetF != 0f || CRTOptions.contrastF != 0f)
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
                
                if (CRTOptions.screenDist.Value)
                    screen_dist = 1.0f;
                else
                {
                    screen_dist = 0.0f;
                }
            }
        }
    }

    public Texture2D GetDither(string dither)
    {
        Texture2D ditherTex;

        switch (dither)
        {
            case "Bayer2":
                ditherTex = Plugin.BayerTextures[0];
                break;
            case "Bayer4":
                ditherTex = Plugin.BayerTextures[1];
                break;
            case "Bayer8":
                ditherTex = Plugin.BayerTextures[2];
                break;
            default:
                ditherTex = Plugin.BayerTextures[3];
                break;
        }
        return ditherTex;
    }

    public Texture2D GetPalette(string palette)
    {
        Texture2D paletteTex;

        if (Plugin.palettes.Count == Plugin.palNum)
        {
            switch (palette)
            {
                case "Chrome4":
                    paletteTex = Plugin.palettes[0];
                    break;
                case "Hollow4":
                    paletteTex = Plugin.palettes[1];
                    break;
                case "RetroGB4":
                    paletteTex = Plugin.palettes[2];
                    break;
                case "Jojo4":
                    paletteTex = Plugin.palettes[3];
                    break;
                case "Amber4":
                    paletteTex = Plugin.palettes[4];
                    break;
                case "Blood4":
                    paletteTex = Plugin.palettes[5];
                    break;
                case "Horror4":
                    paletteTex = Plugin.palettes[6];
                    break;
                case "Lava4":
                    paletteTex = Plugin.palettes[7];
                    break;
                case "Miku4":
                    paletteTex = Plugin.palettes[8];
                    break;
                case "Aqua4":
                    paletteTex = Plugin.palettes[9];
                    break;
                case "Wish4":
                    paletteTex = Plugin.palettes[10];
                    break;
                case "Moonlight4":
                    paletteTex = Plugin.palettes[11];
                    break;
                case "Royal4":
                    paletteTex = Plugin.palettes[12];
                    break;
                case "Ammo8":
                    paletteTex = Plugin.palettes[13];
                    break;
                case "Bi8":
                    paletteTex = Plugin.palettes[14];
                    break;
                case "Borkfest8":
                    paletteTex = Plugin.palettes[15];
                    break;
                case "Citrink8":
                    paletteTex = Plugin.palettes[16];
                    break;
                case "Dream8":
                    paletteTex = Plugin.palettes[17];
                    break;
                case "Fox8":
                    paletteTex = Plugin.palettes[18];
                    break;
                case "Gothic8":
                    paletteTex = Plugin.palettes[19];
                    break;
                case "Lava8":
                    paletteTex = Plugin.palettes[20];
                    break;
                case "Morning8":
                    paletteTex = Plugin.palettes[21];
                    break;
                case "Nebula8":
                    paletteTex = Plugin.palettes[22];
                    break;
                case "Ocean8":
                    paletteTex = Plugin.palettes[23];
                    break;
                case "Paper8":
                    paletteTex = Plugin.palettes[24];
                    break;
                case "Parchment8":
                    paletteTex = Plugin.palettes[25];
                    break;
                case "Purple8":
                    paletteTex = Plugin.palettes[26];
                    break;
                case "RetroGB8":
                    paletteTex = Plugin.palettes[27];
                    break;
                case "RustGold8":
                    paletteTex = Plugin.palettes[28];
                    break;
                case "Chimera8":
                    paletteTex = Plugin.palettes[29];
                    break;
                case "Winter8":
                    paletteTex = Plugin.palettes[30];
                    break;
                default:
                    paletteTex = Plugin.palettes[0];
                    break;
            }

            return paletteTex;
        }


        return Plugin.palettes[0];

    }


}

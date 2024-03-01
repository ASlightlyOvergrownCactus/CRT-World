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

[BepInPlugin("cactus.crt", "CRT", "1.0")]
sealed class Plugin : BaseUnityPlugin
{
    static bool loaded = false;
    public static Shader CRTShader;
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
            var bundle = AssetBundle.LoadFromFile(AssetManager.ResolveFilePath("assets/crtscreen")); // Loads the asset bundle from your mod's file path
            CRTShader = bundle.LoadAsset<Shader>("Assets/shaders 1.9.03/CRTScreen.shader"); // Loads the shader itself from the asset bundle
            Camera.main.gameObject.AddComponent<CrtScreen>();
        }
    }
}

sealed class CrtScreen : MonoBehaviour
{
    private static bool menuLoaded = false;
    public Material mat = new Material(Plugin.CRTShader);
    [Range(0, 1)] public float verts_force = 0.51f;
    [Range(0, 1)] public float verts_force_2 = 0.255f;
    [Range(0, 1)] public float verts_force_3 = 0.8f;
    [Range(0, 1)] public float screen_dist = 1.0f;
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        mat.SetFloat("_VertsColor", 1-verts_force);
        mat.SetFloat("_VertsColor2", 1-verts_force_2);
        mat.SetFloat("_VertsColor3", 1-verts_force_3);
        mat.SetFloat("_ScreenDist", screen_dist); // No distortion at 0.0, screen edge dist at 1.0
        Graphics.Blit(src, dest, mat);
    }

    private void Update()
    {
        if (menuLoaded)
        {
            verts_force_3 = CRTOptions.distortionF / 100f;
            verts_force_2 = CRTOptions.intensityF / 100f;
            verts_force = CRTOptions.scanLineDarknessF / 100f;
            if (CRTOptions.screenDistF)
                screen_dist = 1.0f;
            else
            {
                screen_dist = 0.0f;
            }
        }
        else
        {
            if (CRTOptions.intensityF != 0f)
                menuLoaded = true;
            else
            {
                verts_force_3 = CRTOptions.distortion.Value / 100f;
                verts_force_2 = CRTOptions.intensity.Value / 100f;
                verts_force = CRTOptions.scanLineDarkness.Value / 100f;
                if (CRTOptions.screenDist.Value)
                    screen_dist = 1.0f;
                else
                {
                    screen_dist = 0.0f;
                }
            }
        }
    }

    public float GetDistortion()
    {
        return verts_force_3;
    }
}

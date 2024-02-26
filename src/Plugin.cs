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
        On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        new Hook(typeof(Futile).GetMethod("get_mousePosition"), newMousePos);
    }

    private Vector2 newMousePos(Func<Vector2> orig)
    {
        Vector2 mousePos = orig();

        mousePos = new Vector2(mousePos.x / Camera.main.pixelWidth, Camera.main.pixelHeight);
        float radius = Camera.main.gameObject.GetComponent<CrtScreen>().GetDistortion();
        float warp = new Vector3((mousePos.x - 0.5f), (mousePos.y - 0.5f), radius).magnitude / new Vector2(0.5f, radius).magnitude;
        warp += 0.05f;
        Vector2 origDis = (new Vector2(mousePos.x - 0.5f, mousePos.y - 0.5f) * warp);
        mousePos =  new Vector2(origDis.x + 0.5f, origDis.y + 0.5f);

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
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        mat.SetFloat("_VertsColor", 1-verts_force);
        mat.SetFloat("_VertsColor2", 1-verts_force_2);
        mat.SetFloat("_VertsColor3", 1-verts_force_3);
        Graphics.Blit(src, dest, mat);
    }

    private void Update()
    {
        if (menuLoaded)
        {
            verts_force_3 = CRTOptions.distortionF / 100f;
            verts_force_2 = CRTOptions.intensityF / 100f;
            verts_force = CRTOptions.scanLineDarknessF / 100f;
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
            }
        }
    }

    public float GetDistortion()
    {
        return verts_force_3;
    }
}

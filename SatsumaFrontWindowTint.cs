using System;
using System.Collections.Generic;
using MSCLoader;
using MscModApi.Shopping;
using UnityEngine;
using UnityEngine.Rendering;

public class SatsumaFrontWindowTint : Mod
{
    public override string ID => "SatsumaFrontWindowTint";
    public override string Name => "Satsuma Front Window Tint";
    public override string Author => "OpenAI";
    public override string Version => "1.0";
    public override string Description => "Front side window tint sold at Teimo's shop.";
    public override Game SupportedGames => Game.MySummerCar;

    private const float PRICE = 500f;
    private const float TINT = 0.35f;

    private const string SAVE_PURCHASED = "purchased";

    private readonly Dictionary<Renderer, Material[]> originalMaterials =
        new Dictionary<Renderer, Material[]>();

    private bool purchased;

    public override void ModSetup()
    {
        SetupFunction(Setup.OnLoad, Mod_OnLoad);
        SetupFunction(Setup.OnSave, Mod_OnSave);
    }

    private void Mod_OnLoad()
    {
        purchased = false;

        try
        {
            if (SaveLoad.ValueExists(this, SAVE_PURCHASED))
                purchased = SaveLoad.ReadValue<bool>(this, SAVE_PURCHASED);
        }
        catch (Exception e)
        {
            ModConsole.Error("[FrontTint] Save load failed: " + e);
        }

        try
        {
            var item = new ShopItem(
                "Тонировка передних стёкол",
                PRICE,
                new Vector3(-1553.865f, 4f, 1182.825f),
                ApplyTint,
                "",
                true
            );

            Shop.Add(new ShopBaseInfo(this, null), Shop.ShopLocation.Teimo, item);
            ModConsole.Print("[FrontTint] Item added to Teimo shop.");

            if (purchased)
                ApplyTint();
        }
        catch (Exception e)
        {
            ModConsole.Error("[FrontTint] Shop registration failed: " + e);
        }
    }

    private void Mod_OnSave()
    {
        try
        {
            SaveLoad.WriteValue(this, SAVE_PURCHASED, purchased);
        }
        catch (Exception e)
        {
            ModConsole.Error("[FrontTint] Save failed: " + e);
        }
    }

    private void ApplyTint()
    {
        purchased = true;

        GameObject satsuma = GameObject.Find("SATSUMA");
        if (satsuma == null)
        {
            ModConsole.Error("[FrontTint] SATSUMA was not found.");
            return;
        }

        int changed = 0;

        Renderer[] renderers = satsuma.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer r in renderers)
        {
            if (!IsFrontSideWindow(r.transform))
                continue;

            if (!originalMaterials.ContainsKey(r))
                originalMaterials[r] = r.materials;

            Material[] mats = r.materials;
            bool changedRenderer = false;

            for (int i = 0; i < mats.Length; i++)
            {
                Material source = mats[i];
                if (source == null)
                    continue;

                // Only tint glass-like materials when the renderer has several materials.
                // If no glass-like material exists, the renderer itself is assumed to be glass.
                bool glassMaterial =
                    source.name.IndexOf("glass", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    source.name.IndexOf("window", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    source.shader != null &&
                    source.shader.name.IndexOf("glass", StringComparison.OrdinalIgnoreCase) >= 0;

                if (mats.Length > 1 && !glassMaterial)
                    continue;

                Material tinted = new Material(source);
                TintMaterial(tinted);
                mats[i] = tinted;
                changedRenderer = true;
            }

            if (changedRenderer)
            {
                r.materials = mats;
                changed++;
                ModConsole.Print("[FrontTint] Tinted: " + GetPath(r.transform));
            }
        }

        if (changed == 0)
        {
            ModConsole.Warning(
                "[FrontTint] No front side window renderer found. " +
                "Open output_log.txt and check the SATSUMA hierarchy/names."
            );
        }
        else
        {
            ModConsole.Print("[FrontTint] Front windows tinted: " + changed);
        }
    }

    private static bool IsFrontSideWindow(Transform t)
    {
        string n = t.name.ToLowerInvariant();
        string path = GetPath(t).ToLowerInvariant();

        bool hasWindow =
            n.Contains("window") ||
            n.Contains("glass") ||
            path.Contains("window") ||
            path.Contains("glass");

        if (!hasWindow)
            return false;

        // Do not tint the windshield or rear/quarter glass.
        if (n.Contains("windshield") || n.Contains("windscreen") ||
            path.Contains("windshield") || path.Contains("windscreen"))
            return false;

        if (n.Contains("rear") || n.Contains("back") || n.Contains("quarter") ||
            path.Contains("rear") || path.Contains("back") || path.Contains("quarter"))
            return false;

        // Prefer objects belonging to a door or explicitly marked left/right/side.
        bool side =
            n.Contains("door") || path.Contains("door") ||
            n.Contains("left") || n.Contains("right") ||
            n.Contains("side") ||
            n.EndsWith("l") || n.EndsWith("r");

        return side;
    }

    private static void TintMaterial(Material m)
    {
        if (m.HasProperty("_Color"))
        {
            Color c = m.GetColor("_Color");
            c.r *= TINT;
            c.g *= TINT;
            c.b *= TINT;
            c.a = Mathf.Min(c.a, 0.75f);
            m.SetColor("_Color", c);
        }

        // Standard/legacy transparent setup. Existing glass shaders are left mostly intact.
        if (m.HasProperty("_Mode"))
            m.SetFloat("_Mode", 3f);


        if (m.HasProperty("_DstBlend"))
            m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);

        if (m.HasProperty("_ZWrite"))
            m.SetInt("_ZWrite", 0);

        if (m.HasProperty("_Glossiness"))
            m.SetFloat("_Glossiness", Mathf.Max(0.2f, m.GetFloat("_Glossiness")));

        m.renderQueue = 3000;
    }

    private static string GetPath(Transform t)
    {
        string result = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            result = t.name + "/" + result;

            if (t.parent == null)
                break;
        }
        return result;
    }
}

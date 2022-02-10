using UnityEngine;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using UnityEditor;

public static class MegalithUtils
{
    public static float Magnitude(this Color32 color)
    {
        return new Vector4(color.r, color.g, color.b, color.a).magnitude;
    }

    public static bool Equals(this Color32 left, Color32 right)
    {
        return left.r == right.r && left.g == right.g && left.b == right.b && left.a == right.a;
    }

    public static Dictionary<string, int> GetEnumValuesAndDescriptions<T>()
    {
        Type enumType = typeof(T);

        if (enumType.BaseType != typeof(Enum))
            throw new ArgumentException("T is not System.Enum");

        Dictionary<string, int> dict = new Dictionary<string, int>();

        foreach (var e in Enum.GetValues(typeof(T)))
        {
            var fi         = e.GetType().GetField(e.ToString());
            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            dict.Add((attributes.Length > 0) ? attributes[0].Description : e.ToString(), (int) e);
        }

        return dict;
    }

    private static Texture2D m_CyanTexture;

    public static Texture2D CyanTexture
    {
        get
        {
            if (m_CyanTexture == null)
            {
                m_CyanTexture = new Texture2D(2, 2);
                m_CyanTexture.SetPixels(new Color[] {Color.cyan, Color.cyan, Color.cyan, Color.cyan});
                m_CyanTexture.Apply();
            }

            return m_CyanTexture;
        }
    }

    private static Texture2D m_DarkGreenTexture;

    public static Texture2D DarkGreenTexture
    {
        get
        {
            if (m_DarkGreenTexture == null)
            {
                m_DarkGreenTexture = new Texture2D(2, 2);
                var c = new Color32(91, 127, 0, 255);
                m_DarkGreenTexture.SetPixels32(new Color32[] {c, c, c, c});
                m_DarkGreenTexture.Apply();
            }

            return m_DarkGreenTexture;
        }
    }

    public static void InvertPixels(ref Color[] pixels)
    {
        var inverted = new Color[pixels.Length];

        for (var i = 0; i < inverted.Length; i++)
        {
            inverted[i] = Color.white - pixels[i];
        }

        pixels = inverted;
    }

    public static void InvertPixelsChannel(ref Color[] pixels, int channel)
    {
        for (var i = 0; i < pixels.Length; i++)
        {
            pixels[i][channel] = 1 - pixels[i][channel];
        }
    }

#if UNITY_EDITOR
    public static Texture2D LoadTEX(String path)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
        return tex;
    }
#endif
}
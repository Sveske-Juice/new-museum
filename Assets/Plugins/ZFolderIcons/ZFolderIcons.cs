#if UNITY_EDITOR && UNITY_2021_3_OR_NEWER
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;

[InitializeOnLoad]
public class ZFolderIcons : EditorWindow
{
    private static GUIStyle labelStyle;
    private const float maxHeight = 18f;

    private static Texture2D scriptIconTexture;

    static ZFolderIcons()
    {
        EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
    }

    private static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
    {
        if (selectionRect.height < maxHeight) 
            return;

        // Get the asset path based on the GUID
        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

        // Check if the asset is a folder and not in the base project folder
        // Use "&& assetPath.Count(c => c == '/') >= 2" to ignore base assets folder    

        // TODO: Impliment AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guid)).IsSubclassOf(typeof(ScriptableObject))

        Type objectType = null;
        if (AssetDatabase.IsValidFolder(assetPath))
        {
            DoFolderIcons(assetPath, selectionRect);
        }
        else
        {
            objectType = AssetDatabase.GetMainAssetTypeFromGUID(new GUID(guid));
        }

        if (objectType == null) return;
        if (objectType == typeof(MonoScript)) // Is script object
        {
            DoMonoScriptIcon(assetPath, selectionRect);
        }
    }

    static void DoFolderIcons(string folderPath, Rect selectionRect)
    {
        // Calculate the position for the custom label
        Rect labelRect = new Rect(selectionRect.x + selectionRect.width * 0.15f, selectionRect.y + selectionRect.width * 0.4f, selectionRect.width, selectionRect.height);

        string folderName = Path.GetFileName(folderPath);

        // Create GUIStyle if it hasn't been created yet
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.alignment = TextAnchor.UpperLeft; // Adjust the font size as desired
            labelStyle.normal.textColor = Color.black; // Set the text color
        }
        labelStyle.fontSize = (int)(selectionRect.width * 0.3f); // Adjust the font size as desired

        string labelText;
        if (TryGetPreDefinedLabels(folderName, out string label))
        {
            labelText = label;
        }
        else if (folderName.Count(char.IsUpper) >= 2)
        {
            labelText = new string(folderName.Where(x => char.IsUpper(x)).Take(3).ToArray());
        }
        else
        {
            labelText = new(folderName.Take(3).ToArray());
        }

        // Draw the custom label with black color
        var previousColor = GUI.color;
        GUI.color = Color.black;
        EditorGUI.LabelField(labelRect, labelText, labelStyle);
        GUI.color = previousColor;
    }

    static void DoMonoScriptIcon(string scriptPath, Rect selectionRect)
    {
        // Calculate the position for the white overlay
        Rect overwriteOverlayRect = new Rect(selectionRect.x + selectionRect.width * 0.25f, selectionRect.y + selectionRect.width * 0.25f, selectionRect.width*0.5f, selectionRect.height*0.5f);

        // Draw the custom rect with white color
        var previousColor = GUI.color;
        float shadeOfWhite = 247;
        GUI.color = new Color(shadeOfWhite / 255f, shadeOfWhite / 255f, shadeOfWhite / 255f);
        EditorGUI.DrawRect(overwriteOverlayRect, GUI.color);
        GUI.color = previousColor;

        string scriptName = Path.GetFileName(scriptPath);

        const int maxLength = 8;
        string labelText;
        {
            // Use LINQ to iterate through each character and insert a line break before uppercase letters
            labelText = new string(scriptName.SelectMany(c => char.IsUpper(c) ? new char[] { '\n', c } : new[] { c }).ToArray());
            labelText = labelText.TrimStart('\n').Replace(".cs", "");
            labelText = string.Join('\n', labelText.Split('\n').Select(x => x.Length > 6 ? new string(x.Take(maxLength).ToArray()) + ".." : x));
        }

        // Calculate the position for the custom label
        Rect labelRect = new Rect(selectionRect.x + selectionRect.width * 0.15f, selectionRect.y + selectionRect.width * 0.1f, selectionRect.width, selectionRect.height);

        // Create GUIStyle if it hasn't been created yet
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(EditorStyles.miniLabel);
            labelStyle.alignment = TextAnchor.UpperLeft; // Adjust the font size as desired
            labelStyle.normal.textColor = Color.black; // Set the text color
        }
        labelStyle.fontSize = (int)(selectionRect.width * 0.15f); // Adjust the font size as desired

        // Draw the custom label with black color
        previousColor = GUI.color;
        GUI.color = Color.black;
        EditorGUI.LabelField(labelRect, labelText, labelStyle);
        GUI.color = previousColor;

        // Custom icon rect
        Rect customIconRect = new Rect(selectionRect.x + selectionRect.width * 0.62f, selectionRect.y + selectionRect.width * 0.645f, selectionRect.width * 0.25f, selectionRect.height * 0.25f);

        // Load texture if null
        if (scriptIconTexture == null)
        {
            if (TryGetScriptIcon(out Texture2D icon))
            {
                scriptIconTexture = icon;
            }
            else
            {
                return; // Couldn't get icon, so don't draw it
            }
        }

        // Draw the custom icon
        GUI.DrawTexture(customIconRect, scriptIconTexture);
    }

    static bool TryGetPreDefinedLabels(string folderName, out string label)
    {
        switch (folderName)
        {
            case "Scripts":
                label = "C#";
                return true;

            case "Plugins":
                label = "Plug";
                return true;

            case "Resources":
                label = "Res";
                return true;
                
            case "Editor":
                label = "Edit";
                return true;
                
            case "Prefabs":
                label = "PF";
                return true;
                
            case "Tiles":
                label = "Tile";
                return true;
                
            case "Settings":
                label = "Cfg";
                return true;

            default:
                label = "Error";
                return false;
        }
    }

    static bool TryGetScriptIcon(out Texture2D icon)
    {
        if (FindFolder("ExtraIcons", out string path))
        {
            icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path + "/ScriptHashtag.png");
            return true;
        }
        else
        {
            icon = null;
            return false;
        }
    }

    static bool FindFolder(string folderName, out string path)
    {
        string[] guids = AssetDatabase.FindAssets("t:Folder " + folderName);

        if (guids.Length > 0)
        {
            path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return true;
        }
        else
        {
            path = null;
            return false;
        }
    }

}
#endif

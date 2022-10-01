using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;

public static class RPGBuilderEditorUtility
{

    public static Texture2D GenerateTexture2D(Texture2D texture, Color color)
    {
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
    
    public static int GetTypeEntryIndex(List<RPGBuilderDatabaseEntry> typeEntries, RPGBuilderDatabaseEntry selectedEntry)
    {
        for (int i = 0; i < typeEntries.Count; i++)
        {
            if (typeEntries[i] == selectedEntry) return i;
        }

        return 0;
    }
    
    public static int GetTypeEntryIndexWithNull(List<RPGBuilderDatabaseEntry> typeEntries, RPGBuilderDatabaseEntry selectedEntry)
    {
        for (int i = 0; i < typeEntries.Count; i++)
        {
            if (typeEntries[i] == selectedEntry) return i;
        }

        return -1;
    }
    
    public static int GetDialogueNodeIndex(List<RPGDialogueTextNode> textNodes, RPGDialogueTextNode selectedNode)
    {
        for (int i = 0; i < textNodes.Count; i++)
        {
            if (textNodes[i] == selectedNode) return i;
        }

        return 0;
    }

    public static List<RPGDialogueTextNode> GetDialogueTextNodes(List<Node> nodes)
    {
        List<RPGDialogueTextNode> textNodes = new List<RPGDialogueTextNode>();

        foreach (var node in nodes)
        {
            textNodes.Add((RPGDialogueTextNode)node);
        }
        return textNodes;
    }

    public static Sprite GetEntryIcon(int ID, string entryType)
    {
        return GetEntryByID(ID, entryType).entryIcon;
    }
    
    public static string[] GetTypeEntriesAsStringArray(RPGBuilderDatabaseEntry[] typeEntries)
    {
        List<string> stringEntries = typeEntries.Select(typeEntry => typeEntry.entryName).ToList();
        return stringEntries.ToArray();
    }
    
    public static string[] GetTypeEntriesAsStringArray(RPGBuilderDatabaseEntry[] typeEntries, bool reorder, string firstEntryName)
    {
        List<string> stringEntries = typeEntries.Select(typeEntry => typeEntry.entryName).ToList();
        if (!reorder || !string.IsNullOrEmpty(firstEntryName)) return stringEntries.ToArray();
        if (!stringEntries.Contains(firstEntryName)) return stringEntries.ToArray();
        stringEntries.RemoveAt(stringEntries.IndexOf(firstEntryName));
        stringEntries.Insert(0, firstEntryName);
        return stringEntries.ToArray();
    }
    
    public static string[] GetDialogueTextNodesAsStringArray(RPGDialogueTextNode[] textNodes)
    {
        List<string> stringEntries = new List<string>();
        foreach (var textNode in textNodes)
        {
            stringEntries.Add(textNode.message);
        }
        return stringEntries.ToArray();
    }
    
    public static string[] GetRanksAsStringArray(int ranksCount)
    {
        List<string> stringEntries = new List<string>();
        for (int i = 0; i < ranksCount; i++)
        {
            string rank = "Rank " + (i + 1);
            stringEntries.Add(rank);
        }
        return stringEntries.ToArray();
    }
    
    public static string[] GetNPCPhasesStringArray(int phasesCount)
    {
        List<string> stringEntries = new List<string>();
        for (int i = 0; i < phasesCount; i++)
        {
            string rank = "Phase " + (i + 1);
            stringEntries.Add(rank);
        }
        return stringEntries.ToArray();
    }
    
    public static RPGBuilderEditorModule GetModuleByName(string moduleName)
    {
        return RPGBuilderEditor.Instance.EditorCategories
            .SelectMany(category => category.modules.Where(module => module.ModuleName == moduleName)).FirstOrDefault();
    }
    public static RPGBuilderEditorModule GetModuleByEntryType(string entryType)
    {
        return RPGBuilderEditor.Instance.EditorCategories
            .SelectMany(category => category.modules.Where(module => module.EntryType == entryType)).FirstOrDefault();
    }
    
    public static RPGBuilderDatabaseEntry GetEntryByID(int entryID, string entryType)
    {
        return RPGBuilderEditor.Instance.EditorCategories.SelectMany(
            category => category.modules.Where(
                module => module.EntryType == entryType).SelectMany(
                module => module.databaseEntries.Where(
                    entry => entry.ID == entryID))).FirstOrDefault();
    }
    
    public static List<RPGBuilderDatabaseEntry> GetEntriesByEntryType (string entryType)
    {
        return (from category in RPGBuilderEditor.Instance.EditorCategories
            from module in category.modules.Where(module => module.EntryType == entryType) 
            select module.databaseEntries).FirstOrDefault();
    }
    
    public static void StartHorizontalMargin(float space, bool beginVertical)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(space);
        if (beginVertical) EditorGUILayout.BeginVertical();
    }

    public static void EndHorizontalMargin(float space, bool endVertical)
    {
        if (endVertical) EditorGUILayout.EndVertical();
        GUILayout.Space(space);
        EditorGUILayout.EndHorizontal();
    }

    public static bool HandleModuleBanner(string BannerTitle, bool isShowing)
    {
        GUIStyle buttonOverlayStyle = RPGBuilderEditor.Instance.EditorSkin.GetStyle("BannerButtonOverlay");
        Rect bannerRect = DrawModuleBanner(BannerTitle, isShowing);
        if (GUI.Button(bannerRect, "", buttonOverlayStyle))
        {
            return !isShowing;
        }

        return isShowing;
    }

    public static bool HandleModuleBanner2(string BannerTitle, bool isShowing)
    {
        GUIStyle buttonOverlayStyle = RPGBuilderEditor.Instance.EditorSkin.GetStyle("BannerButtonOverlay");
        Rect bannerRect = DrawModuleBanner2(BannerTitle, isShowing);
        if (GUI.Button(bannerRect, "", buttonOverlayStyle))
        {
            return !isShowing;
        }

        return isShowing;
    }
    
    

    private static Rect DrawModuleBanner2(string bannerText, bool isShowing)
    {
        var bannerFullRect = GUILayoutUtility.GetRect(0, 0, 30, 0);
        var bannerBeginRect = new Rect(bannerFullRect.position.x + 20, bannerFullRect.position.y, 20, 30);
        var bannerMiddleRect = new Rect(bannerFullRect.position.x + 40, bannerFullRect.position.y,
            bannerFullRect.xMax - 80, 30);
        var bannerEndRect = new Rect(bannerFullRect.xMax - 40, bannerFullRect.position.y, 20, 30);

        Color guiColor = GUI.color;

        float width = bannerBeginRect.width + bannerMiddleRect.width + bannerEndRect.width;
        Rect fullBannerRect = new Rect(bannerBeginRect.x, bannerFullRect.position.y, width, 30);
        GUIStyle bannerTextStyle = RPGBuilderEditor.Instance.EditorSkin.GetStyle("ViewTitle");
        if (isCursorHoverRect(fullBannerRect))
        {
            GUI.color = RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerHovered;
            bannerTextStyle.normal.textColor = RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerTextHovered;
        }
        else
        {
            GUI.color = isShowing
                ? RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerExpanded
                : RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerCollapsed;
            bannerTextStyle.normal.textColor = isShowing
                ? RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerTextExpanded
                : RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerTextCollapsed;
        }

        GUI.DrawTexture(bannerBeginRect, RPGBuilderEditor.Instance.EditorData.bannerBegin, ScaleMode.StretchToFill,
            true);
        GUI.DrawTexture(bannerMiddleRect, RPGBuilderEditor.Instance.EditorData.bannerMiddle, ScaleMode.StretchToFill,
            true);
        GUI.DrawTexture(bannerEndRect, RPGBuilderEditor.Instance.EditorData.bannerEnd, ScaleMode.StretchToFill, true);
        GUI.color = guiColor;

        GUI.Label(bannerFullRect, bannerText, RPGBuilderEditor.Instance.EditorSkin.GetStyle("ViewTitle"));
        return fullBannerRect;
    }

    private static Rect DrawModuleBanner(string bannerText, bool isShowing)
    {
        var bannerFullRect = GUILayoutUtility.GetRect(0, 0, 30, 0);
        var bannerBeginRect = new Rect(bannerFullRect.position.x + 60, bannerFullRect.position.y, 20, 30);
        var bannerMiddleRect = new Rect(bannerFullRect.position.x + 80, bannerFullRect.position.y,
            bannerFullRect.xMax - 170, 30);
        var bannerEndRect = new Rect(bannerFullRect.xMax - 90, bannerFullRect.position.y, 20, 30);

        Color guiColor = GUI.color;

        float width = bannerBeginRect.width + bannerMiddleRect.width + bannerEndRect.width;
        Rect fullBannerRect = new Rect(bannerBeginRect.x, bannerFullRect.position.y, width, 30);
        GUIStyle bannerTextStyle = RPGBuilderEditor.Instance.EditorSkin.GetStyle("ViewTitle");
        if (isCursorHoverRect(fullBannerRect))
        {
            GUI.color = RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerHovered;
            bannerTextStyle.normal.textColor = RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerTextHovered;
        }
        else
        {
            GUI.color = isShowing
                ? RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerExpanded
                : RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerCollapsed;
            bannerTextStyle.normal.textColor = isShowing
                ? RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerTextExpanded
                : RPGBuilderEditor.Instance.EditorSettings.EditorTheme.BannerTextCollapsed;
        }

        GUI.DrawTexture(bannerBeginRect, RPGBuilderEditor.Instance.EditorData.bannerBegin, ScaleMode.StretchToFill,
            true);
        GUI.DrawTexture(bannerMiddleRect, RPGBuilderEditor.Instance.EditorData.bannerMiddle, ScaleMode.StretchToFill,
            true);
        GUI.DrawTexture(bannerEndRect, RPGBuilderEditor.Instance.EditorData.bannerEnd, ScaleMode.StretchToFill, true);
        GUI.color = guiColor;

        GUI.Label(bannerFullRect, bannerText, RPGBuilderEditor.Instance.EditorSkin.GetStyle("ViewTitle"));
        return fullBannerRect;
    }
    
    public static float DrawHorizontalFloatFillBar(float min, float max, float value)
    {
        Rect bannerRect = DrawFillBar(value, max);
        value = GUI.HorizontalSlider(bannerRect, value, min, max, new GUIStyle(), new GUIStyle());
        if (value > max) value = max;
        if (value < min) value = min;
        return value;
    }

    public static bool ShowEntryIcon(string entryType)
    {
        foreach (var category in RPGBuilderEditor.Instance.EditorCategories)
        {
            foreach (var module in category.modules)
            {
                if(module.EntryType != entryType) continue;
                return module.ShowIconInList;
            }
        }

        return false;
    }
    
    private static Rect DrawFillBar(float value, float max)
    {
        var bannerFullRect = GUILayoutUtility.GetRect(0, RPGBuilderEditor.Instance.position.width, 17, 0);
        var bannerMiddleRect = new Rect(RPGBuilderEditor.Instance.LongHorizontalMargin + 
                                        RPGBuilderEditor.Instance.EditorData.labelFieldWidth + 6, bannerFullRect.position.y+1, bannerFullRect.width, 17);

        float width = bannerMiddleRect.width;
        Rect fullBannerRect = new Rect(bannerMiddleRect.x, bannerFullRect.position.y, width, 17);
        
        GUI.DrawTexture(bannerMiddleRect, RPGBuilderEditor.Instance.EditorData.FillBarBackground, ScaleMode.StretchToFill, true);

        if (value > max) value = max;
        float fillValue = value / max;
        fillValue = bannerMiddleRect.width * fillValue;
        GUI.DrawTexture(new Rect(bannerMiddleRect.x, bannerMiddleRect.y, fillValue, bannerMiddleRect.height),
            isCursorHoverRect(fullBannerRect)
                ? RPGBuilderEditor.Instance.EditorData.FillBarHover
                : RPGBuilderEditor.Instance.EditorData.FillBar, ScaleMode.StretchToFill, true);

        return fullBannerRect;
    }

    public static bool isCursorHoverRect(Rect checkRect)
    {
        var e = Event.current;
        return checkRect.Contains(e.mousePosition);
    }

    public static float GetHeightModifier()
    {
        float height = RPGBuilderEditor.Instance.position.height / RPGBuilderEditor.Instance.EditorData.MinEditorHeight;
        return height >= 1 ? height : 1;
    }

    public static float GetWidthModifier()
    {
        return RPGBuilderEditor.Instance.position.width / RPGBuilderEditor.Instance.EditorData.MinEditorWidth;
    }

    public static float GetScreenWidth()
    {
        float width = Screen.width;
        float extraWidthFromScaling = (RPGBuilderEditor.Instance.EditorData.MinEditorWidth * GetWidthModifier()) * (EditorGUIUtility.pixelsPerPoint - 1);
        width -= extraWidthFromScaling;
        return width;
    }

    public static float GetScreenHeight()
    {
        float height = Screen.height;
        float extraHeightFromScaling = (RPGBuilderEditor.Instance.EditorData.MinEditorHeight) *
                                       (EditorGUIUtility.pixelsPerPoint - 1);
        if (EditorGUIUtility.pixelsPerPoint > 1) extraHeightFromScaling += 15;
        height -= extraHeightFromScaling;
        return height;
    }

    private static float GetViewWidth()
    {
        if (RPGBuilderEditor.Instance.ShowFilters)
        {
            return 1 - RPGBuilderEditor.Instance.EditorData.FilterWidthPercent;
        }

        return 1;
    }

    private static float GetCategoryMenuWidth()
    {
        return GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent;
    }

    private static float GetElementListWidth()
    {
        return GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.ElementListWidthPercent;
    }

    private static Rect GetViewRect()
    {
        float panelHeight = GetScreenHeight();
        return new Rect(RPGBuilderEditor.Instance.CategoryWidth + RPGBuilderEditor.Instance.EntryListWidth, 0,
            RPGBuilderEditor.Instance.ViewWidth, panelHeight);
    }

    private static Rect GetSettingsViewRect()
    {
        float panelHeight = GetScreenHeight() -
                            (GetScreenHeight() * RPGBuilderEditor.Instance.EditorData.TopBarHeightPercent);
        return new Rect(0, 0, RPGBuilderEditor.Instance.ViewWidth, panelHeight);
    }

    private static float GetButtonHeight()
    {
        return RPGBuilderEditor.Instance.EditorData.actionButtonsY * GetHeightModifier();
    }

    private static float GetSmallFieldHeight()
    {
        return RPGBuilderEditor.Instance.EditorData.viewSmallFieldHeight/* * GetHeightModifier()*/;
    }

    private static float GetSmallButtonHeight()
    {
        return RPGBuilderEditor.Instance.EditorData.smallButtonHeight * GetHeightModifier();
    }

    private static float GetBigHorizontalSpace()
    {
        return 100 * GetWidthModifier();
    }

    private static float GetMiddleButtonSpace()
    {
        return RPGBuilderEditor.Instance.ViewWidth / 4;
    }

    private static float GetMiddleButtonMaxWidth()
    {
        return RPGBuilderEditor.Instance.ViewWidth / 2;
    }

    public static void UpdateElementListData()
    {
        RPGBuilderEditor.Instance.EntryListWidth = GetElementListWidth();
    }

    public static void UpdateCategoryWidth()
    {
        RPGBuilderEditor.Instance.CategoryWidth = GetCategoryMenuWidth();
    }

    public static void UpdateViewAndFieldData()
    {
        UpdateViewData();
        UpdateFieldData();
    }

    private static void UpdateViewData()
    {
        RPGBuilderEditor.Instance.ViewWidth = GetScreenWidth() * GetViewWidth();
        RPGBuilderEditor.Instance.ViewWidth *= RPGBuilderEditor.Instance.EditorData.ViewWidthPercent;
        RPGBuilderEditor.Instance.ViewRect = GetViewRect();
    }

    private static void UpdateFieldData()
    {
        RPGBuilderEditor.Instance.ButtonHeight = GetButtonHeight();
        RPGBuilderEditor.Instance.FieldHeight = GetSmallFieldHeight();
        RPGBuilderEditor.Instance.SmallButtonHeight = GetSmallButtonHeight();
        RPGBuilderEditor.Instance.LongHorizontalMargin = GetBigHorizontalSpace();
        RPGBuilderEditor.Instance.CenteredMargin = GetMiddleButtonSpace();
        RPGBuilderEditor.Instance.CenteredButtonMaxWidth = GetMiddleButtonMaxWidth();
    }

    public static int GetIndexFromStatName(string[] statList, string curName)
    {
        for (var i = 0; i < statList.Length; i++)
            if (statList[i] == curName)
                return i;
        return 0;
    }

    public static string GetSecondaryDamageNameFromID(string[] statList, int Index)
    {
        return statList[Index];
    }

    public static int GetIndexFromSecondaryDamageName(string[] statList, string curName)
    {
        for (var i = 0; i < statList.Length; i++)
            if (statList[i] == curName)
                return i;
        return 0;
    }

    public static void DisplayDialogueWindow(string title, string message, string buttonText)
    {
        EditorUtility.DisplayDialog(title, message, buttonText);
    }
    
    public static void ResetScrollPositions()
    {
        RPGBuilderEditor.Instance.EntryListScroll = Vector2.zero;
        RPGBuilderEditor.Instance.CachedEntryListScroll = Vector2.zero;
        RPGBuilderEditor.Instance.FiltersScroll = Vector2.zero;
    }
    
    public static int GetIndexFromActionKey(string actionKeyName)
    {
        for (var i = 0; i < RPGBuilderEditor.Instance.GeneralSettings.actionKeys.Count; i++)
        {
            if (RPGBuilderEditor.Instance.GeneralSettings.actionKeys[i].actionName == actionKeyName) return i;
        }

        return -1;
    }
    
    public static List<string> GetActionKeyNamesList()
    {
        return RPGBuilderEditor.Instance.GeneralSettings.actionKeys.Select(actionKey => actionKey.actionName).ToList();
    }
    
}

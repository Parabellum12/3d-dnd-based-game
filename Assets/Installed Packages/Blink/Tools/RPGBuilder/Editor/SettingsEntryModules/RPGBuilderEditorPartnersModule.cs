using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorPartnersModule : RPGBuilderEditorModule
{
    private RPGBuilderPartners currentEntry;
    
    public override void Initialize()
    {
        LoadEntries();
    }

    public override void InstantiateCurrentEntry(int index)
    {
        LoadEntries();
    }

    public override void LoadEntries()
    {
        currentEntry = Resources.Load<RPGBuilderPartners>(RPGBuilderEditor.Instance.EditorData.RPGBEditorDataPath +
                                                                     AssetFolderName + "/" + EntryType);
        if (currentEntry != null)
        {
            currentEntry = Instantiate(currentEntry);
        }

        databaseEntries.Clear();
        databaseEntries.Add(currentEntry);

        RPGBuilderEditor.Instance.CurrentEntry = currentEntry;
    }

    public override void CreateNewEntry()
    {
    }

    public override bool SaveConditionsMet()
    {
        return true;
    }

    public override void UpdateEntryData(RPGBuilderDatabaseEntry updatedEntry)
    {
        
    }

    public override void ClearEntries()
    {
        databaseEntries.Clear();
        currentEntry = null;
    }

    public override void DrawView()
    {
        RPGBuilderEditorUtility.UpdateViewAndFieldData();

        float topSpace = RPGBuilderEditor.Instance.ButtonHeight + 5;
        GUILayout.Space(topSpace);

        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth();
        panelWidth -= panelWidth * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent;
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight();
        Rect panelRect = new Rect(
            RPGBuilderEditorUtility.GetScreenWidth() * RPGBuilderEditor.Instance.EditorData.CategoryMenuWidthPercent, 0,
            panelWidth, panelHeight);

        RPGBuilderEditor.Instance.ViewScroll = EditorGUILayout.BeginScrollView(RPGBuilderEditor.Instance.ViewScroll,
            false, false,
            GUILayout.Width(panelRect.width),
            GUILayout.MaxWidth(panelRect.width),
            GUILayout.ExpandHeight(true));

        RPGBuilderEditorUtility.StartHorizontalMargin(40, true);
        foreach (var partner in currentEntry.partners)
        {
            RPGBuilderEditorFields.DrawPartnerLabel(partner.name, "", 20);
            RPGBuilderEditorFields.DrawPartnerLabel(partner.email, "", 20);
            RPGBuilderEditorFields.DrawPartnerLabel(partner.description, "", 20);
            GUILayout.Space(5);
            if(GUILayout.Button("Visit " + partner.name + "'s Store", RPGBuilderEditor.Instance.EditorSkin.GetStyle("HorizontalAddButton"),
                GUILayout.MinWidth(300), GUILayout.Height(25)))
            {
                Application.OpenURL(partner.link);
            }
            GUILayout.Space(5);
            GUILayout.Box(partner.image.texture, RPGBuilderEditor.Instance.EditorSkin.GetStyle("PartnerBanner"), GUILayout.Width(337),GUILayout.Height(225));

            GUILayout.Space(25);
        }
        RPGBuilderEditorUtility.EndHorizontalMargin(0, true);

        GUILayout.Space(30);
        GUILayout.EndScrollView();
    }
}

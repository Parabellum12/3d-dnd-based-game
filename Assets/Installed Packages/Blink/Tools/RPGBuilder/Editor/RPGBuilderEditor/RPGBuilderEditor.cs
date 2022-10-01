using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class RPGBuilderEditor : EditorWindow
{
    // Instance of the RPG Builder Editor, used by other modules and utility classes
    public static RPGBuilderEditor Instance;

    // Is the RPG Builder Editor currently opened?
    public static bool EditorIsOpen = false;

    // Data used to draw the editor
    public RPGBuilderEditorDATA EditorData;
    public RPGBuilderEditorFilters EditorFilters;
    public GUISkin EditorSkin;

    // Categories and Modules
    public class EditorCategoryData
    {
        public RPGBuilderEditorCategory category;
        public List<RPGBuilderEditorModule> modules = new List<RPGBuilderEditorModule>();
    }

    public List<EditorCategoryData> EditorCategories = new List<EditorCategoryData>();

    public RPGBuilderEditorCategory CurrentEditorCategory = null;
    public RPGBuilderEditorModule CurrentEditorModule = null;

    // The currently selected and viewed Database Entry
    public RPGBuilderDatabaseEntry CurrentEntry;
    public int CurrentEntryIndex;
    private string CurrentEntryCachedName;

    // Filters data
    public List<RPGBuilderEditorFilters.EntryFieldList> EntryFieldList =
        new List<RPGBuilderEditorFilters.EntryFieldList>();

    private bool isSelectingField;
    private int currentFilterEntryDataIndex;
    private List<RPGBuilderEditorFilters.FilterEntryData> currentFilterList;
    public bool ShowFilters;

    // Game & Editor Settings
    public RPGBuilderCombatSettings CombatSettings;
    public RPGBuilderCharacterSettings CharacterSettings;
    public RPGBuilderProgressionSettings ProgressionSettings;
    public RPGBuilderEconomySettings EconomySettings;
    public RPGBuilderWorldSettings WorldSettings;
    public RPGBuilderGeneralSettings GeneralSettings;
    public RPGBuilderUISettings UISettings;
    public RPGBuilderEditorSettings EditorSettings;

    // Current Database Entry list Data
    private class DatabaseEntrySlot
    {
        public string name;
        public Texture texture;
        public bool showIcon;
        public int ID;
        public bool matchFilters;
    }

    private List<DatabaseEntrySlot> displayedDatabaseEntries;
    public static Dictionary<int, bool> cachedRequirementResults = null;

    // Dynamically update the editor view based on the following conditions
    private bool updateEntryList, updateValidateFilters;
    private bool redrawEntryList = true, filtersNeedValidation;
    private bool targetFiltersNeedValidation;

    // Values updated based on the current view
    public float CategoryWidth,
        ViewWidth,
        EntryListWidth,
        ButtonHeight,
        FieldHeight,
        SmallButtonHeight,
        LongHorizontalMargin,
        CenteredMargin,
        CenteredButtonMaxWidth;

    public Rect ViewRect;
    public Vector2 CategoryScroll, EntryListScroll, CachedEntryListScroll = Vector2.zero;
    public Vector2 ViewScroll;
    public Vector2 FiltersScroll;
    public Vector2 BlinkPageScroll;
    private string moduleSearchBarText = "";

    private bool databaseMissing;
    
    // Blink page fields
    private bool updateShowBlinkPage, targetShowBlinkPage, showBlinkPage;

    [MenuItem("BLINK/RPG Builder", false, 0)]
    private static void OpenWindow()
    {
        var window = (RPGBuilderEditor) GetWindow(typeof(RPGBuilderEditor), false, "RPG Builder");
        window.Show();
        window.titleContent = new GUIContent("RPG Builder", Resources.Load<Texture>("EditorData/UI/Textures/Test16"));
        Instance = window;
    }

    private void OnEnable()
    {
        if (Instance == null)
        {
            EditorData = Resources.Load<RPGBuilderEditorDATA>("EditorData/RPGBuilderEditorData");
            EditorSkin = Resources.Load<GUISkin>(EditorData.RPGBEditorDataPath + "RPGBuilderSkin");
            EditorFilters = Resources.Load<RPGBuilderEditorFilters>(EditorData.RPGBEditorDataPath + "RPGBuilderEditorFilters");
            minSize = new Vector2(EditorData.MinEditorWidth, EditorData.MinEditorHeight);

            InitializeEditor();
        }

        SceneManager.activeSceneChanged += EditorSceneManager_ActiveSceneChanged;
        EditorSceneManager.sceneOpened += EditorSceneManager_SceneOpened;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }
    
    private void OnDestroy()
    {
        EditorIsOpen = false;
        SceneManager.activeSceneChanged -= EditorSceneManager_ActiveSceneChanged;
        EditorSceneManager.sceneOpened -= EditorSceneManager_SceneOpened;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;

        foreach (var module in EditorCategories.SelectMany(category => category.modules))
        {
            module.ClearEntries();
        }

        EditorCategories.Clear();
    }
    
    private void InitializeEditor()
    {
        if (Instance == null) Instance = this;
        
        if(!LoadSettings())
        {
            databaseMissing = true;
            return;
        }
        else
        {
            databaseMissing = false;
        }
        InitializeTextures();

        EditorIsOpen = true;
        CurrentEntryIndex = 0;
        currentFilterList = EditorFilters.abilityFilters;

        LoadEditorCategoriesAndModules();

        updateEntryList = true;
        cachedRequirementResults = null;
        if (EditorCategories.Count > 0 && EditorCategories[0] != null)
            SelectEditorCategory(EditorCategories[0].category);

        LoadAllDatabaseEntries();
        SelectDatabaseEntry(0, true);
    }

    public void InitializeTextures()
    {
        EditorData.BackgroundTexture1 = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.BackgroundTexture1,
            EditorSettings.EditorTheme.BackgroundColor1);
        EditorData.BackgroundTexture2 = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.BackgroundTexture2,
            EditorSettings.EditorTheme.BackgroundColor2);
            
        GUIStyle moduleSearchStyle = EditorSkin.GetStyle("ModuleSearchText");
        EditorData.SearchBarTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.SearchBarTexture,
            EditorSettings.EditorTheme.SearchBar);
        EditorData.SearchBarHoveredTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.SearchBarHoveredTexture,
            EditorSettings.EditorTheme.SearchBarHovered);
        moduleSearchStyle.normal.background = EditorData.SearchBarTexture;
        moduleSearchStyle.hover.background = EditorData.SearchBarHoveredTexture;
        
        GUIStyle addButton = EditorSkin.GetStyle("HorizontalAddButton");
        EditorData.AddButtonTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.AddButtonTexture,
            EditorSettings.EditorTheme.AddButton);
        EditorData.AddButtonHoveredTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.AddButtonHoveredTexture,
            EditorSettings.EditorTheme.AddButtonHover);
        addButton.normal.background = EditorData.AddButtonTexture;
        addButton.hover.background = EditorData.AddButtonHoveredTexture;
        
        GUIStyle removeButton = EditorSkin.GetStyle("HorizontalRemoveButton");
        EditorData.RemoveButtonTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.RemoveButtonTexture,
            EditorSettings.EditorTheme.RemoveButton);
        EditorData.RemoveButtonHoveredTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.RemoveButtonHoveredTexture,
            EditorSettings.EditorTheme.RemoveButtonHover);
        removeButton.normal.background = EditorData.RemoveButtonTexture;
        removeButton.hover.background = EditorData.RemoveButtonHoveredTexture;
        
        GUIStyle genericButton = EditorSkin.GetStyle("GenericButton");
        EditorData.GenericButtonTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.GenericButtonTexture,
            EditorSettings.EditorTheme.GenericButton);
        EditorData.GenericButtonHoveredTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.GenericButtonHoveredTexture,
            EditorSettings.EditorTheme.GenericButtonHover);
        genericButton.normal.background = EditorData.GenericButtonTexture;
        genericButton.hover.background = EditorData.GenericButtonHoveredTexture;
        
        GUIStyle addButtonSquare = EditorSkin.GetStyle("SquareAddButton");
        EditorData.AddButtonSquareTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.AddButtonSquareTexture,
            EditorSettings.EditorTheme.AddButton);
        EditorData.AddButtonSquareHoveredTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.AddButtonSquareHoveredTexture,
            EditorSettings.EditorTheme.AddButtonHover);
        addButtonSquare.normal.background = EditorData.AddButtonSquareTexture;
        addButtonSquare.hover.background = EditorData.AddButtonSquareHoveredTexture;
        
        GUIStyle removeButtonSquare = EditorSkin.GetStyle("SquareRemoveButton");
        EditorData.RemoveButtonSquareTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.RemoveButtonSquareTexture,
            EditorSettings.EditorTheme.RemoveButton);
        EditorData.RemoveButtonSquareHoveredTexture = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.RemoveButtonSquareHoveredTexture,
            EditorSettings.EditorTheme.RemoveButtonHover);
        removeButtonSquare.normal.background = EditorData.RemoveButtonSquareTexture;
        removeButtonSquare.hover.background = EditorData.RemoveButtonSquareHoveredTexture;
        
        EditorData.AbilityTooltipBackground = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.AbilityTooltipBackground,
            EditorSettings.EditorTheme.AbilityTooltipColor1);
        EditorData.AbilityTooltipBackgroundHover = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.AbilityTooltipBackgroundHover,
            EditorSettings.EditorTheme.AbilityTooltipColor2);
        
        GUIStyle customLabelField = EditorSkin.GetStyle("CustomLabel");
        customLabelField.normal.textColor = EditorSettings.EditorTheme.TextLabelColor;
        customLabelField.hover.textColor = EditorSettings.EditorTheme.TextLabelColorHover;
        
        GUIStyle titleLabelField = EditorSkin.GetStyle("TitleLabel");
        titleLabelField.normal.textColor = EditorSettings.EditorTheme.TitleLabelColor;
        titleLabelField.hover.textColor = EditorSettings.EditorTheme.TitleLabelColorHover;
        
        GUIStyle customTextField = EditorSkin.GetStyle("CustomTextField");
        EditorData.CustomTextField = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.CustomTextField,
            EditorSettings.EditorTheme.CustomTextField);
        EditorData.CustomTextFieldHover = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.CustomTextFieldHover,
            EditorSettings.EditorTheme.CustomTextFieldHover);
        customTextField.normal.background = EditorData.CustomTextField;
        customTextField.hover.background = EditorData.CustomTextFieldHover;
        customTextField.normal.textColor = EditorSettings.EditorTheme.TextValueColor;
        customTextField.hover.textColor = EditorSettings.EditorTheme.TextValueColorHover;
        
        GUIStyle fillLabel = EditorSkin.GetStyle("FillBarLabel");
        fillLabel.normal.textColor = EditorSettings.EditorTheme.TextLabelColor;
        fillLabel.hover.textColor = EditorSettings.EditorTheme.TextLabelColorHover;
        
        EditorData.FillBarBackground = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.FillBarBackground,
            EditorSettings.EditorTheme.FillBarBackground);
        EditorData.FillBar = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.FillBar,
            EditorSettings.EditorTheme.FillBar);
        EditorData.FillBarHover = RPGBuilderEditorUtility.GenerateTexture2D(EditorData.FillBarHover,
            EditorSettings.EditorTheme.FillBarHover);
    }

    private void LoadEditorCategoriesAndModules()
    {
        EditorCategories.Clear();
        List<RPGBuilderEditorCategory> allCategories = Resources
            .LoadAll<RPGBuilderEditorCategory>(EditorData.RPGBEditorDataPath + "EditorCategories").ToList();
        allCategories.Sort((n1, n2) => n1.OrderIndex.CompareTo(n2.OrderIndex));

        foreach (var category in allCategories)
        {
            EditorCategoryData newCategory = new EditorCategoryData {category = category};
            newCategory.category = category;
            foreach (var module in category.modules)
            {
                newCategory.modules.Add(module);
            }

            EditorCategories.Add(newCategory);
        }
    }
    
    private void LoadAllDatabaseEntries()
    {
        foreach (var module in EditorCategories.SelectMany(category => category.modules))
        {
            module.LoadEntries();
        }
    }
    
    private void OnGUI()
    {
        DrawEditorBackgroundTexture2();

        if (databaseMissing)
        {
            DrawDatabaseMissingMessage();
            return;
        }
        DrawTopBar();
        DrawCategories();
        DrawDatabaseEntryList();
        DrawModuleView();
        DrawFilters();
        DrawBlinkPage();
    }

    private void DrawDatabaseMissingMessage()
    {
        GUIStyle errorStyle = new GUIStyle
        {
            normal = {textColor = Color.red},
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            clipping = TextClipping.Clip
        };
        GUILayout.Space(25);
        EditorGUILayout.TextArea("Your DATABASE could not be found", errorStyle);
        GUILayout.Space(3);
        EditorGUILayout.TextArea("If you just installed RPG Builder this is normal", errorStyle);
        GUILayout.Space(3);
        EditorGUILayout.TextArea("You need to import the DemoDatabase_2.0 package that is under Blink>Tools>RPGBuilder>Packages", errorStyle);
    }
    
    private void Update()
    {
        if (databaseMissing) return;
        if (updateShowBlinkPage)
        {
            showBlinkPage = targetShowBlinkPage;
            updateShowBlinkPage = false;
        }

        if (updateValidateFilters)
        {
            filtersNeedValidation = targetFiltersNeedValidation;
            updateValidateFilters = false;
        }

        if (updateEntryList)
        {
            redrawEntryList = true;
            updateEntryList = false;
        }

        if (EditorSettings.IncreasedEditorRepaint)
        {
            Repaint();
        }
    }

    private void DrawCategories()
    {
        if (showBlinkPage) return;
        RPGBuilderEditorUtility.UpdateCategoryWidth();

        Rect categoryMenuRect = new Rect(0, 0, CategoryWidth, RPGBuilderEditorUtility.GetScreenHeight() - 10);
        DrawEditorBackgroundTexture1(categoryMenuRect);
        categoryMenuRect.y = 5;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("", EditorSkin.GetStyle("RPGBTextLogo"), GUILayout.Width(CategoryWidth * 0.9f),
            GUILayout.Height((CategoryWidth * 0.65f) / 4)))
        {
            
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(3);

        CategoryScroll = GUILayout.BeginScrollView(CategoryScroll, false, false, GUIStyle.none, GUIStyle.none, GUILayout.Width(categoryMenuRect.width));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        foreach (var category in EditorCategories)
        {
            if (!category.category.IsEnabled) continue;
            var selected = false;
            var categoryButtonStyle = EditorSkin.GetStyle("CategoryButton");
            var categoryArrowStyle = EditorSkin.GetStyle("CategoryArrow");
            
            if (category.category == CurrentEditorCategory) selected = true;
            if (selected)
            {
                categoryButtonStyle = EditorSkin.GetStyle("CategoryButtonSelected");
                if (category.category.IsExpanded) categoryArrowStyle = EditorSkin.GetStyle("CategoryArrowSelected");
            }

            float categoryButtonHeight = 35;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("  " + category.category.CategoryName, categoryButtonStyle,
                GUILayout.ExpandWidth(true),
                GUILayout.Height(categoryButtonHeight)))
            {
                CollapseAllCategoriesButTheCurrentOne();
                category.category.IsExpanded = !category.category.IsExpanded;
                if (category.category.IsExpanded)
                {
                    SelectEditorCategory(category.category);
                }
            }
            
            if (GUILayout.Button("", categoryArrowStyle, GUILayout.Width(categoryButtonHeight),
                GUILayout.Height(categoryButtonHeight)))
            {
                CollapseAllCategoriesButTheCurrentOne();
                category.category.IsExpanded = !category.category.IsExpanded;
                if (category.category.IsExpanded)
                {
                    SelectEditorCategory(category.category);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (category.category.IsExpanded)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(40);
                EditorGUILayout.BeginVertical();
                GUILayout.Space(8);
                foreach (var module in category.modules.Where(module => module != null))
                {
                    if (module.IsEnabled)
                    {
                        var selected2 = false;
                        var moduleButtonStyle = EditorSkin.GetStyle("ModuleButton");

                        if (module.ModuleName == CurrentEditorModule.ModuleName)
                        {
                            selected2 = true;
                        }

                        if (selected2)
                        {
                            moduleButtonStyle = EditorSkin.GetStyle("ModuleButtonSelected");
                        }

                        if (GUILayout.Button(module.ModuleName, moduleButtonStyle,
                            GUILayout.Height(categoryButtonHeight)))
                        {
                            if (CurrentEditorModule != module) SelectEditorModule(module);
                        }
                    }

                    GUILayout.Space(10);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(10);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }

    private void DrawModuleView()
    {
        if (showBlinkPage) return;
        if (CurrentEditorModule.IsSettingModule)
            PrepareSettingsViewArea();
        else
            PrepareViewArea();

        CurrentEditorModule.DrawView();
        GUILayout.EndArea();
    }
    
    private void DrawDatabaseEntryList()
    {
        if (showBlinkPage) return;
        if (CurrentEditorModule.IsSettingModule) return;

        if (redrawEntryList)
        {
            GenerateEntryDisplayList();
            redrawEntryList = false;
            targetFiltersNeedValidation = !filtersNeedValidation;
            updateValidateFilters = true;
        }

        DrawEntryList(displayedDatabaseEntries);
    }
    
    private void DrawEntryList(List<DatabaseEntrySlot> entryList)
    {
        if (entryList == null || string.IsNullOrEmpty(CurrentEditorModule.EntryType)) return;

        RPGBuilderEditorUtility.UpdateElementListData();

        float localViewWidth = EntryListWidth;

        if (ShowFilters)
        {
            localViewWidth -= EditorData.FilterWidthPercent;
        }

        float distanceFromTop = EditorData.MinEditorHeight * EditorData.TopBarHeightPercent;
        Rect searchRect = new Rect(CategoryWidth + 12, distanceFromTop, localViewWidth, position.height-distanceFromTop);

        GUILayout.BeginArea(searchRect);
        if (CachedEntryListScroll != EntryListScroll &&
            CachedEntryListScroll != Vector2.zero)
        {
            EntryListScroll = CachedEntryListScroll;
            CachedEntryListScroll = Vector2.zero;
        }

        EntryListScroll = GUILayout.BeginScrollView(EntryListScroll, false, false, GUIStyle.none,
            EditorSkin.verticalScrollbar, GUILayout.Width(localViewWidth), GUILayout.ExpandHeight(true));

        if (filtersNeedValidation)
        {
            if (currentFilterList.Count == 0)
            {
                foreach (var t in entryList)
                {
                    if (!EntryNameMatchSearchText(t.name)) continue;
                    t.matchFilters = true;
                }
            }
            else
            {
                int curElementChecked = 0, maxElementsToCheck = entryList.Count;
                float progress = entryList.Count > 0 ? (float) curElementChecked / (float) maxElementsToCheck : 0;
                EditorUtility.DisplayProgressBar("RPG Builder Filters",
                    "Building entry list: " + curElementChecked + " / " + maxElementsToCheck, progress);
                for (var index = 0; index < entryList.Count; index++)
                {
                    var element = entryList[index];
                    if (currentFilterList.Count == 0)
                    {
                        element.matchFilters = true;
                        continue;
                    }

                    element.matchFilters = EntryMatchFilters(entryList[index], index);

                    if (!EntryNameMatchSearchText(element.name)) element.matchFilters = false;

                    curElementChecked++;
                    progress = (float) ((float) curElementChecked / (float) maxElementsToCheck);
                    EditorUtility.DisplayProgressBar("RPG Builder Filters",
                        "Building entry list: " + curElementChecked + " / " + maxElementsToCheck, progress);
                }

                EditorUtility.ClearProgressBar();
            }

            targetFiltersNeedValidation = !filtersNeedValidation;
            updateValidateFilters = true;
        }
        else
        {
            if (currentFilterList.Count == 0)
            {
                foreach (var t in entryList)
                {
                    t.matchFilters = EntryNameMatchSearchText(t.name);
                }
            }
        }

        var cont = new GUIContent();
        float entryHeight = EditorData.EntryListY;
        float entryWidth = entryList.Count > 17 ? localViewWidth - 15 : localViewWidth;

        for (var i = 0; i < entryList.Count; i++)
        {
            if (!entryList[i].matchFilters) continue;
            if (entryList[i].name == null) continue;
            var buttonStyle = EditorSkin.GetStyle("EntryButton");
            bool selected = CurrentEntryIndex == i;
            if (selected)
            {
                buttonStyle = EditorSkin.GetStyle("EntryButtonSelected");
            }

            
            string entryNameString = entryList[i].name;
            if (entryNameString.Length > 17)
            {
                entryNameString = entryNameString.Remove(16);
                entryNameString += "...";
            }

            if (entryList[i].texture != null)
                cont.image = entryList[i].texture;
            else if (entryList[i].showIcon)
                cont.image = EditorData.defaultEntryIcon.texture;

            cont.text = "  " + entryNameString;

            if (GUILayout.Button(cont, buttonStyle, GUILayout.Width(entryWidth), GUILayout.Height(entryHeight)))
            {
                SelectDatabaseEntry(i, true);
            }

            GUILayout.Space(3);
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void DrawTopBar()
    {
        if (showBlinkPage) return;
        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth() - CategoryWidth;
        Rect panelRect = new Rect(CategoryWidth, 0, panelWidth, RPGBuilderEditorUtility.GetScreenHeight() * EditorData.TopBarHeightPercent);

        GUILayout.BeginArea(panelRect);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);

        float elementHeight = EditorData.ModuleButtonsY;

        if (CurrentEditorModule.ShowEntrySearchBar)
        {
            if (currentFilterList.Count == 0)
            {
                EditorGUILayout.BeginVertical();
                GUILayout.Space(7 * GetHeightModifier());
                moduleSearchBarText = EditorGUILayout.TextField(moduleSearchBarText, EditorSkin.GetStyle("ModuleSearchText"),
                    GUILayout.Height(elementHeight / 1.4f), GUILayout.Width(EntryListWidth));
                EditorGUILayout.EndVertical();
            }
            else
            {
                moduleSearchBarText = "";
            }
        }

        DrawActionButtons(panelRect);

        float logoMargin = RPGBuilderEditorUtility.GetScreenHeight() -
                           ((RPGBuilderEditorUtility.GetScreenHeight() * EditorData.TopBarHeightPercent) * 0.25f);
        logoMargin = RPGBuilderEditorUtility.GetScreenHeight() - logoMargin;
        logoMargin /= 2;
        float logoLength = 100;
        Rect blinkTextRect = new Rect(panelWidth - (logoLength + logoMargin), logoMargin * GetHeightModifier(), logoLength, logoLength * 0.31f);
        Texture2D blinkLogo = EditorData.BlinkLogoOff.texture;

        if (RPGBuilderEditorUtility.isCursorHoverRect(blinkTextRect))
        {
            blinkLogo = EditorData.BlinkLogoOn.texture;
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                targetShowBlinkPage = !showBlinkPage;
                updateShowBlinkPage = true;
            }
        }

        GUI.DrawTexture(blinkTextRect, blinkLogo);

        EditorGUILayout.EndHorizontal();
        GUILayout.EndArea();

    }
    
    private void DrawActionButtons(Rect containerRect)
    {
        if (CurrentEditorModule == null) return;
        containerRect.x = CategoryWidth * 1.3f;

        if (CurrentEditorModule.IsSettingModule) containerRect.x = containerRect.width / 2.5f;

        containerRect.width = RPGBuilderEditorUtility.GetScreenWidth() +
                              (RPGBuilderEditorUtility.GetScreenWidth() * EditorData.FilterWidthPercent);

        GUILayout.BeginArea(containerRect);
        GUILayout.BeginHorizontal();

        GUILayout.Space(15 * GetWidthModifier());

        GUIStyle buttonStyle = EditorSkin.GetStyle("ActionButton");
        GUIStyle buttonStyleDelete = EditorSkin.GetStyle("ActionButtonsDelete");

        float saveWidth = EditorData.SmallActionButtonWidth * GetWidthModifier();
        float dupWidth = EditorData.MediumActionButtonWidth * GetWidthModifier();
        float docWidth = EditorData.BigActionButtonWidth * GetWidthModifier();
        float buttonHeight = EditorData.actionButtonsY * GetHeightModifier();

        if (CurrentEditorModule.ShowSaveButton && GUILayout.Button("Save", buttonStyle, GUILayout.Width(saveWidth), GUILayout.Height(buttonHeight)))
        {
            if (CurrentEditorModule.IsSettingModule)
            {
                SaveSettingsEntry(CurrentEditorModule);
            } else if (CurrentEditorModule.IsTypeModule)
            {
                if (IsTypeNameAvailable(CurrentEditorModule, CurrentEntry))
                {
                    SaveTypeEntry(CurrentEditorModule, CurrentEntry);
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", "This name is already in use, pick a new one", "OK");
                }
            }
            else
            {
                if (IsNameAvailable(CurrentEditorModule, CurrentEntry))
                {
                    SaveDatabaseEntry(CurrentEditorModule, CurrentEntry, false);
                }
                else
                {
                    EditorUtility.DisplayDialog("Warning", "This name is already in use, pick a new one", "OK");
                }
            }
            GUI.FocusControl(null);
        }

        if (CurrentEditorModule.ShowNewButton && GUILayout.Button("New", buttonStyle, GUILayout.Width(saveWidth),
            GUILayout.Height(buttonHeight)))
        {
            CurrentEditorModule.CreateNewEntry();
            CurrentEntryCachedName = "";
            GUI.FocusControl(null);
            
        }

        if (CurrentEditorModule.ShowDuplicateButton && GUILayout.Button("Duplicate", buttonStyle,
            GUILayout.Width(dupWidth), GUILayout.Height(buttonHeight)))
        {
            if (CurrentEditorModule.IsTypeModule)
            {
                DuplicateDatabaseTypeEntry(CurrentEditorModule, CurrentEntry);
            }
            else
            {
                DuplicateDatabaseEntry(CurrentEditorModule, CurrentEntry);
            }
            GUI.FocusControl(null);
        }

        if (CurrentEditorModule.ShowDeleteButton && GUILayout.Button("Delete", buttonStyleDelete,
            GUILayout.Width(dupWidth),
            GUILayout.Height(buttonHeight)))
        {
            if (EditorUtility.DisplayDialog("Confirm DELETE",
                "Are you sure you want to DELETE this " + CurrentEditorModule.EntryType + "?", "YES", "Cancel"))
            {
                if (CurrentEditorModule.IsTypeModule)
                {
                    DeleteDatabaseTypeEntry(CurrentEditorModule, CurrentEntry, true);
                }
                else
                {
                    DeleteDatabaseEntry(CurrentEditorModule, CurrentEntry, true);
                }
                GUI.FocusControl(null);
            }
        }

        GUIStyle buttonStyle2 = EditorSkin.GetStyle("DocumentationButton");

        if (CurrentEditorModule.ShowDocumentationButton && GUILayout.Button("Documentation", buttonStyle2,
            GUILayout.Width(docWidth),
            GUILayout.Height(buttonHeight)))
        {
            Application.OpenURL(CurrentEditorModule.DocumentationLink);
            GUI.FocusControl(null);
        }

        if (CurrentEditorModule.ShowFiltersButton && GUILayout.Button("Filters", buttonStyle,
            GUILayout.Width(saveWidth), GUILayout.Height(buttonHeight)))
        {
            ShowFilters = !ShowFilters;
            if (ShowFilters)
            {
                InitializeFiltersPanel();
            }
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private void DrawBlinkPage()
    {
        if (!showBlinkPage) return;
        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth();
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight();
        Rect panelRect = new Rect(0, 0, panelWidth, panelHeight);

        GUILayout.BeginArea(panelRect);

        float logoMargin = RPGBuilderEditorUtility.GetScreenHeight() -
                           ((RPGBuilderEditorUtility.GetScreenHeight() * EditorData.TopBarHeightPercent) * 0.25f);
        logoMargin = RPGBuilderEditorUtility.GetScreenHeight() - logoMargin;
        logoMargin /= 2;
        float logoLength = 100;
        Rect blinkTextRect = new Rect(panelWidth - (logoLength + logoMargin), logoMargin * GetHeightModifier(), logoLength, logoLength * 0.31f);
        Texture2D blinkLogo = EditorData.BlinkLogoOff.texture;

        if (RPGBuilderEditorUtility.isCursorHoverRect(blinkTextRect))
        {
            blinkLogo = EditorData.BlinkLogoOn.texture;
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                targetShowBlinkPage = !showBlinkPage;
                updateShowBlinkPage = true;
            }
        }

        GUI.DrawTexture(blinkTextRect, blinkLogo);

        EditorGUILayout.EndHorizontal();

        GUILayout.Space(50);
        BlinkPageScroll = EditorGUILayout.BeginScrollView(BlinkPageScroll, false, false,
            GUILayout.Width(panelRect.width), GUILayout.MaxWidth(panelRect.width),
            GUILayout.Height(panelRect.height - 75));
        EditorGUILayout.BeginVertical();
        
        GUILayout.Space(10);
        GUILayout.Label("WHAT IS BLINK ?", EditorSkin.GetStyle("BlinkPageTitle"), GUILayout.Height(50));
        GUILayout.Label(
            "We are a team of passionate developers creating the best Tools & Art for indie developers",
            EditorSkin.GetStyle("BlinkPageText"), GUILayout.Height(50));
        GUILayout.Space(10);

        GUILayout.Space(10);
        GUILayout.Label("JOIN US:", EditorSkin.GetStyle("BlinkPageTitle"), GUILayout.Height(50));

        RPGBuilderEditorUtility.StartHorizontalMargin(100, false);
        if (GUILayout.Button("Store", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            Application.OpenURL("https://assetstore.unity.com/publishers/49855");
        }

        GUILayout.Space(30);
        if (GUILayout.Button("Discord", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            Application.OpenURL("https://discord.gg/fYzpuYwPwJ");
        }


        GUILayout.Space(30);
        if (GUILayout.Button("YouTube", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.ExpandWidth(true),
            GUILayout.Height(40)))
        {
            Application.OpenURL("https://www.youtube.com/c/BlinkStudiosYoutube");
        }

        RPGBuilderEditorUtility.EndHorizontalMargin(100, false);

        GUILayout.Space(30);

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
    
    private void SelectEditorCategory(RPGBuilderEditorCategory newCategory)
    {
        moduleSearchBarText = "";

        ResetFilterWindow();
        RPGBuilderEditorUtility.ResetScrollPositions();
        ViewScroll = Vector2.zero;

        if (CurrentEditorCategory != newCategory)
        {
            CurrentEditorCategory = newCategory;
            if (CurrentEditorCategory.modules.Count > 0 && CurrentEditorCategory.modules[0] != null)
            {
                SelectEditorModule(CurrentEditorCategory.modules[0]);
            }
        }

        CollapseAllCategoriesButTheCurrentOne();
    }

    private void SelectEditorModule(RPGBuilderEditorModule newModule)
    {
        if (CurrentEditorModule != newModule)
        {
            moduleSearchBarText = "";
            if(CurrentEditorModule != null) CurrentEditorModule.OnExitModule();
        }
        CurrentEditorModule = newModule;

        RPGBuilderEditorUtility.ResetScrollPositions();
        ViewScroll = Vector2.zero;
        ResetFilterWindow();

        SelectDatabaseEntry(0, false);
        CurrentEditorModule.Initialize();
        if(CurrentEntry != null) CurrentEntryCachedName = CurrentEntry.entryFileName;

        if (!newModule.IsSettingModule)
        {
            RequestEntryListRedraw();
        }
        else
        {
            RequestEntryListRedraw();
        }
    }
    
    public void SelectDatabaseEntry(int newIndex, bool instantiateEntry)
    {
        CurrentEntryIndex = newIndex;
        if (instantiateEntry) CurrentEditorModule.InstantiateCurrentEntry(CurrentEntryIndex);
        if(CurrentEntry != null) CurrentEntryCachedName = CurrentEntry.entryFileName;
        GUI.FocusControl(null);
    }
    
    public void SaveDatabaseEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry savedEntry, bool isInjectingEntry)
    {
        if (!module.SaveConditionsMet())
        {
            module.ShowSaveErrorMessage();
            return;
        }

        int newID = -1;
        string currentFileName = GetEntryFileName(module, savedEntry.ID);
        RPGBuilderDatabaseEntry entryFile = GetEntryFile(module, currentFileName);

        if (entryFile != null)
        {
            if (isInjectingEntry) entryFile.ID = -1;
            if (entryFile.ID == -1)
            {
                entryFile.ID = GenerateNewEntryID();
            }

            savedEntry.ID = entryFile.ID;
            module.UpdateEntryData(entryFile);

            if (!isInjectingEntry)
            {
                CompleteEntrySave(module, entryFile, currentFileName, savedEntry.entryFileName);
                module.OnSave();
            }
        }
        else
        {
            if (isInjectingEntry) return;
            var ID = GenerateNewEntryID();
            savedEntry.ID = ID;
            CompleteCreateEntry(module, savedEntry);
            newID = ID;
        }

        if (!module.IsSettingModule) RequestEntryListRedraw();
        UpdateEditorViewAfterSavingEntry(module, newID);
    }

    private void SaveSettingsEntry (RPGBuilderEditorModule module)
    {
        RPGBuilderDatabaseEntry entryFile = GetEntryFile(module, module.EntryType);
        
        if (entryFile != null)
        {
            module.UpdateEntryData(entryFile);
            EditorUtility.SetDirty(entryFile);
            AssetActions(entryFile, true);
            EditorUtility.ClearProgressBar();
            module.OnSave();
        }
        else
        {
            Debug.LogError(module.EntryType + " could not be found");
        }
    }
    
    private void SaveTypeEntry (RPGBuilderEditorModule module, RPGBuilderDatabaseEntry savedEntry)
    {
        if (!module.SaveConditionsMet())
        {
            module.ShowSaveErrorMessage();
            return;
        }

        RPGBuilderDatabaseEntry entryFile = GetEntryFile(module, CurrentEntryCachedName);
        
        if (entryFile != null)
        {
            module.UpdateEntryData(entryFile);
            CompleteEntrySave(module, entryFile, CurrentEntryCachedName, savedEntry.entryFileName);
        }
        else
        {
            CompleteCreateEntry(module, savedEntry);
        }

        if (!module.IsSettingModule) RequestEntryListRedraw();
        UpdateEditorViewAfterSavingEntry(module, savedEntry.entryFileName);
    }

    public void GenerateTypeEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry savedEntry)
    {
        AssetDatabase.CreateAsset(savedEntry,
            EditorData.ResourcePath + EditorSettings.DatabasePath +
            module.AssetFolderName + "/" + savedEntry.entryFileName + ".asset");
    }

    public RPGBuilderDatabaseEntry GenerateAndGetTypeEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry savedEntry)
    {

        RPGBuilderDatabaseEntry existingEntryFile = (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(
            EditorData.ResourcePath +
            EditorSettings.DatabasePath +
            module.AssetFolderName + "/" + savedEntry.entryFileName + ".asset", typeof(RPGBuilderDatabaseEntry));

        if (existingEntryFile != null) return existingEntryFile;

        AssetDatabase.CreateAsset(savedEntry,
            EditorData.ResourcePath + EditorSettings.DatabasePath +
            module.AssetFolderName + "/" + savedEntry.entryFileName + ".asset");

        return (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(
            EditorData.ResourcePath + EditorSettings.DatabasePath +
            module.AssetFolderName + "/" + savedEntry.entryFileName + ".asset", typeof(RPGBuilderDatabaseEntry));
    }

    private void DuplicateDatabaseEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry duplicatedEntry)
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder Editor while Unity is compiling");
            return;
        }

        if (CurrentEntryIndex == -1 || CurrentEntry == null) return;

        
        var entryFile = GetEntryFile(CurrentEditorModule, CurrentEntryCachedName);

        if (entryFile == null)
        {
            Debug.LogError("The entry cannot be duplicated because it cannot be found");
            return;
        }

        string availablePath = AssetDatabase.GenerateUniqueAssetPath(EditorData.ResourcePath +
                                                                     EditorSettings.DatabasePath +
                                                                     module.AssetFolderName + "/" + duplicatedEntry.entryName + module.AssetNameSuffix + ".asset");
        var newID = GenerateNewEntryID();
        AssetDatabase.CopyAsset(
            EditorData.ResourcePath + EditorSettings.DatabasePath + module.AssetFolderName + "/" + CurrentEntryCachedName +
            ".asset", availablePath);

        AssetActionsAfterCreate();

        var duplicatedFile = (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(availablePath, typeof(RPGBuilderDatabaseEntry));

        if (duplicatedFile == null)
        {
            Debug.LogError("Something went wrong while duplicating");
            return;
        }
        EditorUtility.SetDirty(duplicatedFile);

        string newEntryName = availablePath.Replace(EditorSettings.ResourcePath +
                                                    EditorSettings.DatabasePath +
                                                    module.AssetFolderName + "/", "");
        newEntryName = newEntryName.Replace(module.AssetNameSuffix, "");
        newEntryName = newEntryName.Replace(".asset", "");;
        duplicatedFile.entryName = newEntryName;
        duplicatedFile.entryFileName = newEntryName + module.AssetNameSuffix;
        AssetDatabase.RenameAsset(availablePath, duplicatedFile.entryFileName);
        duplicatedFile.ID = newID;

        EditorUtility.SetDirty(entryFile);
        EditorUtility.SetDirty(duplicatedFile);
        AssetActionsAfterCreate();

        CurrentEditorModule.LoadEntries();
        SelectDatabaseEntry(newID == -1 ? CurrentEntryIndex : CurrentEditorModule.GetEntryIndexByID(newID), true);

        CachedEntryListScroll = EntryListScroll;
        RequestFilterCheck();
        RequestEntryListRedraw();

        EditorUtility.ClearProgressBar();
    }
    
    private void DuplicateDatabaseTypeEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry duplicatedEntry)
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder Editor while Unity is compiling");
            return;
        }

        if (CurrentEntryIndex == -1 || CurrentEntry == null) return;

        var entryFile = GetEntryFile(CurrentEditorModule, CurrentEntryCachedName);

        if (entryFile == null)
        {
            Debug.LogError("The entry cannot be duplicated because it cannot be found");
            return;
        }

        string availablePath = AssetDatabase.GenerateUniqueAssetPath(EditorSettings.ResourcePath +
                                                                     EditorSettings.DatabasePath +
                                                                     module.AssetFolderName + "/" + duplicatedEntry.entryName + module.AssetNameSuffix + ".asset");
        AssetDatabase.CopyAsset(EditorSettings.ResourcePath + EditorSettings.DatabasePath + module.AssetFolderName + "/" + CurrentEntryCachedName +
                                ".asset", availablePath);

        AssetActionsAfterCreate();

        var duplicatedFile = (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(
            availablePath, typeof(RPGBuilderDatabaseEntry));

        if (duplicatedFile == null)
        {
            Debug.LogError("Something went wrong while duplicating");
            return;
        }
        EditorUtility.SetDirty(duplicatedFile);

        string newEntryName = availablePath.Replace(EditorSettings.ResourcePath +
                                                    EditorSettings.DatabasePath +
                                                    module.AssetFolderName + "/", "");
        newEntryName = newEntryName.Replace(module.AssetNameSuffix, "");
        newEntryName = newEntryName.Replace(".asset", "");
        duplicatedFile.entryName = newEntryName;
        duplicatedFile.entryFileName = newEntryName + module.AssetNameSuffix;

        AssetDatabase.RenameAsset(availablePath, duplicatedFile.entryFileName);

        EditorUtility.SetDirty(entryFile);
        EditorUtility.SetDirty(duplicatedFile);
        AssetActionsAfterCreate();

        CurrentEditorModule.LoadEntries();
        SelectDatabaseEntry(module.GetEntryIndexByName(duplicatedFile.entryName), true);

        CachedEntryListScroll = EntryListScroll;
        RequestFilterCheck();
        RequestEntryListRedraw();

        EditorUtility.ClearProgressBar();
    }

    public void DeleteDatabaseTypeEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry deletedEntry, bool singleDelete)
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        AssetDatabase.DeleteAsset(EditorData.ResourcePath + EditorSettings.DatabasePath + module.AssetFolderName + "/" +
                                  CurrentEntryCachedName + ".asset");

        if (singleDelete)
        {
            module.LoadEntries();
            SelectDatabaseEntry(0, true);

            CachedEntryListScroll = EntryListScroll;
            RequestFilterCheck();
            RequestEntryListRedraw();
        }
    }
    
    public void DeleteDatabaseEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry deletedEntry, bool singleDelete)
    {
        if (EditorApplication.isCompiling)
        {
            Debug.LogError("You cannot interact with the RPG Builder while the editor is compiling");
            return;
        }

        AssetDatabase.DeleteAsset(EditorData.ResourcePath + EditorSettings.DatabasePath + module.AssetFolderName + "/" +
                                  GetEntryFileName(module, deletedEntry.ID) + ".asset");
        

        if (singleDelete)
        {
            module.LoadEntries();
            SelectDatabaseEntry(0, true);

            CachedEntryListScroll = EntryListScroll;
            RequestFilterCheck();
            RequestEntryListRedraw();
        }
    }

    private int GenerateNewEntryID()
    {
        var assetID = -1;
        var currentIDFile = DataSavingSystem.LoadAssetID(CurrentEditorModule.IDFileName);
        if (currentIDFile != null)
        {
            assetID = currentIDFile.id;
            assetID++;
            currentIDFile.id = assetID;
            DataSavingSystem.SaveAssetID(currentIDFile);
        }
        else
        {
            var file = new AssetIDHandler(CurrentEditorModule.IDFileName, 0);
            DataSavingSystem.SaveAssetID(file);
            assetID = 0;
        }

        return assetID;
    }
    
    private void EditorSceneManager_ActiveSceneChanged(Scene arg0, Scene arg1)
    {
        RPGBuilderEditorUtility.ResetScrollPositions();
        InitializeEditor();
    }

    private void EditorSceneManager_SceneOpened(Scene arg0, OpenSceneMode mode)
    {
        RPGBuilderEditorUtility.ResetScrollPositions();
        InitializeEditor();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode && state != PlayModeStateChange.ExitingPlayMode) return;
        Instance.InitializeEditor();
        RPGBuilderEditorUtility.ResetScrollPositions();
    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (!EditorIsOpen) return;
        //OpenWindow();
        Instance.InitializeEditor(); 
        RPGBuilderEditorUtility.ResetScrollPositions();  
    }
 
    private void DrawEditorBackgroundTexture1(Rect rect)
    {
        GUI.DrawTexture(rect, EditorData.BackgroundTexture1);
    }

    private void DrawEditorBackgroundTexture2()
    {
        GUI.DrawTexture(new Rect(0, 0, RPGBuilderEditorUtility.GetScreenWidth(), RPGBuilderEditorUtility.GetScreenHeight()), EditorData.BackgroundTexture2);
    }
    
    private void GenerateEntryDisplayList()
    {
        displayedDatabaseEntries = new List<DatabaseEntrySlot>();

        foreach (var entry in CurrentEditorModule.databaseEntries)
        {
            AddEntryToDisplayList(entry.ID, entry.entryName, CurrentEditorModule.ShowIconInList, entry.entryIcon);
        }
    }

    private void AddEntryToDisplayList(int ID, string entryName, bool showIcon, Sprite icon)
    {
        var newElementDATA = new DatabaseEntrySlot {ID = ID, name = entryName, showIcon = showIcon};
        if (icon != null) newElementDATA.texture = icon.texture;
        displayedDatabaseEntries.Add(newElementDATA);
    }
    
    private float GetHeightModifier()
    {
        float height = position.height / EditorData.MinEditorHeight;
        return height >= 1 ? height : 1;
    }

    private float GetWidthModifier()
    {
        return position.width / EditorData.MinEditorWidth;
    }

    public void InitializeFilters(List<RPGBuilderEditorFilters.FilterEntryData> filterList)
    {
        currentFilterList = filterList;
        foreach (var filter in filterList.Where(filter =>
            !string.IsNullOrEmpty(filter.fieldTypeString) && filter.fieldType == null))
        {
            filter.fieldType = GetType(filter.fieldTypeString);

            if (!filter.fieldType.IsEnum) continue;
            if (filter.enumReference != null) continue;
            if (filter.enumIndex > 0 && filter.fieldType.GetEnumValues().Length >= (filter.enumIndex + 1))
            {
                filter.enumReference = (Enum) filter.fieldType.GetEnumValues().GetValue(filter.enumIndex);
            }
            else
            {
                filter.enumReference = (Enum) filter.fieldType.GetEnumValues().GetValue(0);
            }
        }
    }
    private void DrawFilters()
    {
        if (showBlinkPage) return;
        if (!ShowFilters) return;

        float panel3Width = 1 - (EditorData.CategoryWidthPercent + EditorData.SubCategoryWidthPercent +
                                 EditorData.FilterWidthPercent);

        panel3Width = RPGBuilderEditorUtility.GetScreenWidth() * panel3Width;

        float filterWidth = RPGBuilderEditorUtility.GetScreenWidth() * EditorData.FilterWidthPercent;
        float distanceFromTop = RPGBuilderEditorUtility.GetScreenHeight() * EditorData.TopBarHeightPercent;
        float panelHeight = RPGBuilderEditorUtility.GetScreenHeight() - distanceFromTop;
        panelHeight *= 1.02f;
        Rect panelRect = new Rect(panel3Width, distanceFromTop, filterWidth, panelHeight);

        GUILayout.BeginArea(panelRect);

        float topSpace = ButtonHeight;

        FiltersScroll = EditorGUILayout.BeginScrollView(FiltersScroll, false, false,
            GUILayout.Width(filterWidth), GUILayout.MaxWidth(ViewWidth),
            GUILayout.Height(RPGBuilderEditorUtility.GetScreenHeight() - topSpace - 18));

        if (isSelectingField)
        {
            RPGBuilderEditorFilters.EntryField newField = DrawFieldSelectionView();
            if (newField != null)
            {
                currentFilterList[currentFilterEntryDataIndex].parentFieldNames = newField.parentFieldNames;
                currentFilterList[currentFilterEntryDataIndex].fieldName = newField.fieldName;
                currentFilterList[currentFilterEntryDataIndex].moduleName = newField.mopduleName;
                currentFilterList[currentFilterEntryDataIndex].categoryName = newField.categoryName;
                currentFilterList[currentFilterEntryDataIndex].fieldType = newField.fieldType;
                currentFilterList[currentFilterEntryDataIndex].fieldTypeString = newField.fieldType.ToString();
                currentFilterEntryDataIndex = -1;
            }
        }
        else
        {
            DrawModuleFiltersView();
        }

        GUILayout.Space(30);
        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private bool FilterListContainsFieldName(string fieldName)
    {
        return currentFilterList.Any(filterEntry => filterEntry.fieldName == fieldName);
    }

    private List<string> getSettingsStringList(string listName)
    {
        /*
        switch (listName)
        {
            case "weaponType":
                return EconomySettings.weaponTypeList;
            case "itemType":
                return EconomySettings.itemTypeList;
            case "equipmentSlot":
                return EconomySettings.armorSlotsList;
            case "armorType":
                return EconomySettings.armorTypeList;
            case "slotType":
                return EconomySettings.slotTypeList;
        }*/

        return null;
    }

    private void DrawModuleFiltersView()
    {
        RPGBuilderEditorUtility.StartHorizontalMargin(15, true);

        if (currentFilterList.Count > 0)
        {
            if (GUILayout.Button("Apply Filters", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.Height(25),
                GUILayout.ExpandWidth(true)))
            {
                updateEntryList = true;
                cachedRequirementResults = null;
                RPGBuilderEditorUtility.EndHorizontalMargin(0, true);
                return;
            }
        }

        GUILayout.Space(10);

        if (currentFilterList.Count > 0)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Filter", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.Height(25),
                GUILayout.ExpandWidth(true)))
            {
                var newFilter = new RPGBuilderEditorFilters.FilterEntryData();
                currentFilterList.Add(newFilter);
                GUI.FocusControl(null);
                return;
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Remove", EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(75),
                GUILayout.Height(25), GUILayout.ExpandWidth(true)))
            {
                currentFilterList.RemoveAt(currentFilterList.Count - 1);
                GUI.FocusControl(null);

                if (currentFilterList.Count != 0) return;
                updateEntryList = true;
                cachedRequirementResults = null;
                return;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (GUILayout.Button("Remove All (" + currentFilterList.Count + ")",
                EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(150), GUILayout.Height(25),
                GUILayout.ExpandWidth(true)))
            {
                currentFilterList.Clear();
                GUI.FocusControl(null);

                updateEntryList = true;
                cachedRequirementResults = null;
                return;
            }
        }
        else
        {
            if (GUILayout.Button("Add Filter", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.Height(25),
                GUILayout.ExpandWidth(true)))
            {
                var newFilter = new RPGBuilderEditorFilters.FilterEntryData();
                currentFilterList.Add(newFilter);
                GUI.FocusControl(null);

                return;
            }
        }

        GUILayout.Space(25);
        RPGBuilderEditorUtility.EndHorizontalMargin(15, true);

        for (var index = 0; index < currentFilterList.Count; index++)
        {
            var filterEntryNumber = index + 1;
            var filterEntry = currentFilterList[index];

            RPGBuilderEditorUtility.StartHorizontalMargin(10, true);
            EditorGUILayout.BeginHorizontal();
            if (RPGBuilderEditorFields.DrawSmallRemoveButton())
            {
                currentFilterList.RemoveAt(index);

                if (currentFilterList.Count != 0) return;
                updateEntryList = true;
                cachedRequirementResults = null;
                return;
            }

            string fieldName = "";
            if (!string.IsNullOrEmpty(filterEntry.fieldName))
            {
                GUILayout.Space(5);
                if (GUILayout.Button(">", EditorSkin.GetStyle("SquareAddButton"), GUILayout.Width(20),
                    GUILayout.Height(20)))
                {
                    currentFilterEntryDataIndex = index;
                    isSelectingField = true;
                }

                fieldName = GetFieldDisplayName(filterEntry.moduleName, filterEntry.categoryName, filterEntry.fieldName);
                EditorGUILayout.LabelField(filterEntryNumber + ". " + fieldName,
                    GUILayout.Width(EditorData.filterLabelFieldWidth));

            }
            else
            {
                GUILayout.Space(25);
                if (GUILayout.Button("Select Field", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MaxWidth(150),
                    GUILayout.Height(20)))
                {
                    currentFilterEntryDataIndex = index;
                    isSelectingField = true;
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            RPGBuilderEditorUtility.StartHorizontalMargin(10, true);

            if (filterEntry.fieldType == null && !string.IsNullOrEmpty(filterEntry.fieldTypeString))
            {
                filterEntry.fieldType = GetType(filterEntry.fieldTypeString);
            }

            if (filterEntry.fieldType != null)
            {
                if (filterEntry.isRPGDataReference)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Condition Type", GUILayout.Width(EditorData.filterLabelFieldWidth));
                    filterEntry.numberConditionType =
                        (RPGBuilderEditorFilters.NumberConditionType) EditorGUILayout.EnumPopup(filterEntry
                            .numberConditionType);
                    EditorGUILayout.EndHorizontal();

                    filterEntry.intValue = RPGBuilderEditorFields.DrawHorizontalIntField("Value", "",
                        EditorData.filterLabelFieldWidth,
                        filterEntry.intValue);
                }
                else if (filterEntry.fieldType == typeof(int))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Condition Type", GUILayout.Width(EditorData.filterLabelFieldWidth));
                    filterEntry.numberConditionType =
                        (RPGBuilderEditorFilters.NumberConditionType) EditorGUILayout.EnumPopup(filterEntry
                            .numberConditionType);
                    EditorGUILayout.EndHorizontal();

                    filterEntry.intValue = RPGBuilderEditorFields.DrawHorizontalIntField("Value", "",
                        EditorData.filterLabelFieldWidth,
                        filterEntry.intValue);
                }
                else if (filterEntry.fieldType == typeof(float))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Condition Type", GUILayout.Width(EditorData.filterLabelFieldWidth));
                    filterEntry.numberConditionType =
                        (RPGBuilderEditorFilters.NumberConditionType) EditorGUILayout.EnumPopup(filterEntry
                            .numberConditionType);
                    EditorGUILayout.EndHorizontal();

                    filterEntry.floatValue = RPGBuilderEditorFields.DrawHorizontalFloatField("Value", "",
                        EditorData.filterLabelFieldWidth,
                        filterEntry.floatValue);
                }
                else if (filterEntry.fieldType == typeof(string))
                {
                    if (GetFieldIsStringEnum(filterEntry.moduleName, filterEntry.categoryName, filterEntry.fieldName))
                    {
                        /*
                        var stringIndex =
                            RPGBuilderEditorUtility.getIndexFromName(filterEntry.fieldName, filterEntry.text);
                        List<string> stringList = getSettingsStringList(filterEntry.fieldName);
                        var tempIndex1 = EditorGUILayout.Popup(stringIndex, stringList.ToArray());
                        if (stringList.Count > 0)
                            filterEntry.text = stringList[tempIndex1];
                            */
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Condition Type", GUILayout.Width(EditorData.filterLabelFieldWidth));
                        filterEntry.stringValueType =
                            (RPGBuilderEditorFilters.StringValueType) EditorGUILayout.EnumPopup(filterEntry
                                .stringValueType);
                        EditorGUILayout.EndHorizontal();

                        filterEntry.text = RPGBuilderEditorFields.DrawHorizontalTextField("Text", "",
                            50,
                            filterEntry.text);
                    }
                }
                else if (filterEntry.fieldType == typeof(bool))
                {
                    filterEntry.boolValue = RPGBuilderEditorFields.DrawHorizontalToggle("Is ON ?", "",
                        EditorData.filterLabelFieldWidth,
                        filterEntry.boolValue);
                }
                else if (filterEntry.fieldType == typeof(Sprite))
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Sprite to match", GUILayout.Width(100));
                    filterEntry.sprite = RPGBuilderEditorFields.DrawIcon(filterEntry.sprite, 50, 50);
                    EditorGUILayout.EndHorizontal();
                }
                else if (filterEntry.fieldType == typeof(GameObject))
                {
                    filterEntry.gameObject = (GameObject) EditorGUILayout.ObjectField("Prefab",
                        filterEntry.gameObject, typeof(GameObject), false);
                }
                else if (filterEntry.fieldType == typeof(Material))
                {
                    filterEntry.material = (Material) EditorGUILayout.ObjectField("Material",
                        filterEntry.material, typeof(Material), false);
                }
                else if (filterEntry.fieldType == typeof(Vector3))
                {
                    EditorGUILayout.BeginHorizontal();
                    filterEntry.entryReferenceConditionType =
                        (RPGBuilderEditorFilters.EntryReferenceConditionType) EditorGUILayout.EnumPopup(filterEntry
                            .entryReferenceConditionType);
                    filterEntry.vector3 =
                        RPGBuilderEditorFields.DrawHorizontalVector3("Vector 3", "", 15, filterEntry.vector3);
                    EditorGUILayout.EndHorizontal();
                }
                else if (filterEntry.fieldType.IsEnum)
                {
                    EditorGUILayout.BeginHorizontal();
                    filterEntry.entryReferenceConditionType =
                        (RPGBuilderEditorFilters.EntryReferenceConditionType) EditorGUILayout.EnumPopup(filterEntry
                            .entryReferenceConditionType);
                    if (filterEntry.enumReference == null)
                    {
                        if (filterEntry.enumIndex > 0 &&
                            filterEntry.fieldType.GetEnumValues().Length >= (filterEntry.enumIndex + 1))
                        {
                            filterEntry.enumReference =
                                (Enum) filterEntry.fieldType.GetEnumValues().GetValue(filterEntry.enumIndex);
                        }
                        else
                        {
                            filterEntry.enumReference = (Enum) filterEntry.fieldType.GetEnumValues().GetValue(0);
                        }
                    }

                    filterEntry.enumReference =
                        EditorGUILayout.EnumPopup(
                            (Enum) Enum.ToObject(filterEntry.fieldType, filterEntry.enumReference));
                    filterEntry.enumIndex = GetEnumValueIndexByName(filterEntry.fieldType.GetEnumValues(),
                        filterEntry.enumReference.ToString());
                    EditorGUILayout.EndHorizontal();
                }
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(10, true);
            RPGBuilderEditorUtility.EndHorizontalMargin(15, true);
            GUILayout.Space(15);
        }
    }

    private int GetEnumValueIndexByName(Array enumValues, string enumValueName)
    {
        for (int i = 0; i < enumValues.Length; i++)
        {
            if (enumValues.GetValue(i).ToString() == enumValueName) return i;
        }

        return -1;
    }

    private RPGBuilderEditorFilters.EntryField DrawFieldSelectionView()
    {
        GUILayout.Space(20);
        RPGBuilderEditorUtility.StartHorizontalMargin(15, true);

        if (GUILayout.Button("Cancel", EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(80),
            GUILayout.ExpandWidth(true)))
        {
            currentFilterEntryDataIndex = -1;
            isSelectingField = false;
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        string moduleName = "";
        foreach (var fieldList in EntryFieldList)
        {
            if (moduleName == "") moduleName = fieldList.categoryName;
        }

        if (GUILayout.Button("Minimize", EditorSkin.GetStyle("HorizontalRemoveButton"), GUILayout.MinWidth(75),
            GUILayout.ExpandWidth(true), GUILayout.Height(SmallButtonHeight)))
        {
            ChangeFilterCategoriesShowState(moduleName, false);
        }

        GUILayout.Space(10);
        if (GUILayout.Button("Maximize", EditorSkin.GetStyle("HorizontalAddButton"), GUILayout.MinWidth(75),
            GUILayout.ExpandWidth(true), GUILayout.Height(SmallButtonHeight)))
        {
            ChangeFilterCategoriesShowState(moduleName, true);
        }

        GUILayout.EndHorizontal();

        RPGBuilderEditorUtility.EndHorizontalMargin(15, true);
        GUILayout.Space(10);

        foreach (var fieldList in EntryFieldList)
        {
            if (!GetCategoryDisplayState(moduleName, fieldList.categoryName)) continue;
            bool showCategory = GetCategoryShowState(moduleName, fieldList.categoryName);
            showCategory =
                RPGBuilderEditorUtility.HandleModuleBanner2(GetCategoryDisplayName(moduleName, fieldList.categoryName),
                    showCategory);
            SetCategoryShowState(moduleName, fieldList.categoryName, showCategory);
            GUILayout.Space(10);
            if (!showCategory) continue;
            RPGBuilderEditorUtility.StartHorizontalMargin(10, true);
            foreach (var field in fieldList.fieldList)
            {
                if (!GetFilterFieldDisplayState(moduleName, fieldList.categoryName, field.fieldName)) continue;
                if (FilterListContainsFieldName(field.fieldName)) continue;
                string fieldName = GetFieldDisplayName(moduleName, fieldList.categoryName, field.fieldName);

                RPGBuilderEditorUtility.StartHorizontalMargin(35, false);
                if (GUILayout.Button(fieldName, EditorSkin.GetStyle("FilterFieldButton"), GUILayout.MinWidth(80),
                    GUILayout.ExpandWidth(true)))
                {
                    isSelectingField = false;
                    return field;
                }

                RPGBuilderEditorUtility.EndHorizontalMargin(35, false);
                GUILayout.Space(5);
            }

            RPGBuilderEditorUtility.EndHorizontalMargin(10, true);
            GUILayout.Space(10);
        }


        return null;
    }

    private string GetCategoryDisplayName(string moduleName, string categoryName)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName == categoryName) return category.categoryDisplayName;
            }
        }

        return "-- NAME NOT FOUND --";
    }

    private bool GetCategoryShowState(string moduleName, string categoryName)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName == categoryName) return category.showInEditor;
            }
        }

        return false;
    }

    private bool GetFilterFieldDisplayState(string moduleName, string categoryName, string fieldName)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName != categoryName) continue;
                foreach (var field in category.fields)
                {
                    if (field.fieldBaseName != fieldName) continue;
                    return field.display;
                }
            }
        }

        return false;
    }

    private void SetCategoryShowState(string moduleName, string categoryName, bool show)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName != categoryName) continue;
                category.showInEditor = show;
            }
        }
    }

    private bool GetCategoryDisplayState(string moduleName, string categoryName)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName != categoryName) continue;
                return category.display;
            }
        }

        return false;
    }

    private void ChangeFilterCategoriesShowState(string moduleName, bool open)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                category.showInEditor = open;
            }
        }
    }

    private int editorLanguageIndex = 0;

    private string GetFieldDisplayName(string moduleName, string categoryName, string fieldName)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName != categoryName) continue;
                foreach (var field in category.fields)
                {
                    if (field.fieldBaseName != fieldName) continue;
                    return field.fieldNames[editorLanguageIndex];
                }
            }
        }

        return "-- NAME NOT FOUND --";
    }

    private bool GetFieldIsStringEnum(string moduleName, string categoryName, string fieldName)
    {
        foreach (var module in EditorFilters.modules)
        {
            if (module.moduleName != moduleName) continue;
            foreach (var category in module.categories)
            {
                if (category.categoryName != categoryName) continue;
                foreach (var field in category.fields)
                {
                    if (field.fieldBaseName != fieldName) continue;
                    return field.isStringEnum;
                }
            }
        }

        return false;
    }

    private static Type GetType(string TypeName)
    {
        var type = Type.GetType(TypeName);

        if (type != null)
            return type;

        if (TypeName.Contains("."))
        {
            var assemblyName = TypeName.Substring(0, TypeName.IndexOf('.'));
            var assembly = Assembly.Load(assemblyName);
            if (assembly == null)
                return null;

            type = assembly.GetType(TypeName);
            if (type != null)
                return type;

        }

        var currentAssembly = Assembly.GetExecutingAssembly();
        var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
        foreach (var assemblyName in referencedAssemblies)
        {

            var assembly = Assembly.Load(assemblyName);
            if (assembly == null) continue;
            type = assembly.GetType(TypeName);
            if (type != null)
                return type;
        }

        return null;
    }

    private void HandleFullFieldList()
    {
        EntryFieldList.Clear();
        GenerateEntryFieldCategory(CurrentEditorModule.TypeName, CurrentEditorModule.TypeName, new List<string>(), "",
            CurrentEditorModule.TypeName);
    }

    private void GenerateEntryFieldCategory(string categoryName, string typeName, List<string> parentFieldNames,
        string newFieldName, string moduleName)
    {
        RPGBuilderEditorFilters.EntryFieldList newEntryList = new RPGBuilderEditorFilters.EntryFieldList
        {
            mopduleName = moduleName,
            categoryName = categoryName,
            fieldList = GetEntryFields(GetType(typeName), parentFieldNames, newFieldName, moduleName, categoryName)
        };
        EntryFieldList.Add(newEntryList);

        foreach (var field in newEntryList.fieldList)
        {
            if (field.fieldType.ToString().Contains("+") &&
                field.fieldType.ToString().Contains("System.Collections.Generic.List"))
            {
                if (field.fieldType.ToString().Contains(EntryFieldList[0].categoryName))
                {
                    // THIS LOCAL FIELD LIST HAD A LIST IN IT
                    string fieldTypeToString = field.fieldType.ToString();
                    int indexOfBeginning = fieldTypeToString.IndexOf("[");
                    string finalName = fieldTypeToString.Remove(0, indexOfBeginning + 1);
                    finalName = finalName.Remove(finalName.Length - 1);

                    GenerateEntryFieldCategory(finalName, finalName, field.parentFieldNames, field.fieldName,
                        moduleName);
                    field.display = false;
                }
                else
                {
                    field.display = false;
                }
            }
            else if (field.fieldName.Contains("REF"))
            {
                field.display = false;
            }
        }
    }

    private List<RPGBuilderEditorFilters.EntryField> GetEntryFields(Type type, List<string> parentFieldNames,
        string newFieldName, string moduleName, string categoryName)
    {
        List<RPGBuilderEditorFilters.EntryField> fieldList = new List<RPGBuilderEditorFilters.EntryField>();

        var fieldValues = type.GetFields();

        foreach (var field in fieldValues)
        {
            RPGBuilderEditorFilters.EntryField newVar = new RPGBuilderEditorFilters.EntryField();
            newVar.fieldType = field.FieldType;
            newVar.fieldName = field.Name;
            newVar.mopduleName = moduleName;
            newVar.categoryName = categoryName;
            foreach (var parentFieldName in parentFieldNames)
            {
                newVar.parentFieldNames.Add(parentFieldName);
            }

            newVar.parentFieldNames.Add(newFieldName);
            fieldList.Add(newVar);
        }

        return fieldList;
    }

    private void InitializeFiltersPanel()
    {
        currentFilterEntryDataIndex = -1;
        isSelectingField = false;

        foreach (var filter in currentFilterList.Where(filter => filter.fieldType == null)
            .Where(filter => !string.IsNullOrEmpty(filter.fieldTypeString)))
        {
            filter.fieldType = GetType(filter.fieldTypeString);
            if (filter.fieldType == null)
            {
                Debug.LogError("Could not find TYPE for filter: " + filter.fieldName);
            }
        }

        HandleFullFieldList();
    }
    
    private string GetEntryFileName(RPGBuilderEditorModule module, int ID)
    {
        foreach (var entryFound in Resources.LoadAll<RPGBuilderDatabaseEntry>(
            EditorSettings.DatabasePath + module.AssetFolderName))
        {
            if (entryFound.ID != ID) continue;
            return entryFound.name;
        }

        return "";
    }

    private RPGBuilderDatabaseEntry GetEntryFile(RPGBuilderEditorModule module, string entryFileName)
    {
        return (RPGBuilderDatabaseEntry) AssetDatabase.LoadAssetAtPath(
            EditorData.ResourcePath + EditorSettings.DatabasePath +
            module.AssetFolderName + "/" + entryFileName + ".asset", typeof(RPGBuilderDatabaseEntry));
    }

    private void CompleteEntrySave(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry savedEntry,
        string currentFileName, string newFileName)
    {
        EditorUtility.SetDirty(savedEntry);
        if (currentFileName != newFileName)
            AssetDatabase.RenameAsset(EditorData.ResourcePath +
                                      EditorSettings.DatabasePath + module.AssetFolderName + "/" + currentFileName +
                                      ".asset", newFileName);
        AssetActions(savedEntry, true);
    }

    private void CompleteCreateEntry(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry createdEntry)
    {
        AssetDatabase.CreateAsset(createdEntry,
            EditorData.ResourcePath + EditorSettings.DatabasePath +
            module.AssetFolderName + "/" + createdEntry.entryFileName + ".asset");
        AssetActionsAfterCreate();
    }

    protected virtual void UpdateEditorViewAfterSavingEntry(RPGBuilderEditorModule module, int newID)
    {
        module.LoadEntries();
        SelectDatabaseEntry(newID == -1 ? CurrentEntryIndex : CurrentEditorModule.GetEntryIndexByID(newID), true);

        EditorUtility.ClearProgressBar();
    }
    
    protected virtual void UpdateEditorViewAfterSavingEntry(RPGBuilderEditorModule module, string fileName)
    {
        module.LoadEntries();
        for (int i = 0; i < module.databaseEntries.Count; i++)
        {
            if (module.databaseEntries[i].entryFileName == fileName) CurrentEntryIndex = i;
        }
        SelectDatabaseEntry(CurrentEntryIndex, true);

        EditorUtility.ClearProgressBar();
    }

    private bool IsNameAvailable(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry newEntry)
    {
        bool any = false;
        foreach (var entry in module.databaseEntries)
        {
            if (entry.ID != newEntry.ID && entry.entryName == newEntry.entryName)
            {
                any = true;
                break;
            }
        }

        return !any;
    }
    
    private bool IsTypeNameAvailable(RPGBuilderEditorModule module, RPGBuilderDatabaseEntry newEntry)
    {
        RPGBuilderDatabaseEntry entryFile = GetEntryFile(module, newEntry.entryFileName);
        if (entryFile != null)
        {
            return CurrentEntryIndex != -1;
        }

        return true;
    }

    private void HandleRequirementResults(RPGBuilderEditorFilters.FilterEntryData filter, List<bool> requirementResults, object localFieldValue)
    {
        if (filter.fieldType == null)
        {
            filter.fieldType = GetType(filter.fieldTypeString);
        }

        if (filter.fieldType == typeof(int))
        {
            switch (filter.numberConditionType)
            {
                case RPGBuilderEditorFilters.NumberConditionType.Equal:
                    requirementResults.Add((int) localFieldValue == filter.intValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.EqualOrBelow:
                    requirementResults.Add((int) localFieldValue <= filter.intValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.EqualOrAbove:
                    requirementResults.Add((int) localFieldValue >= filter.intValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.Below:
                    requirementResults.Add((int) localFieldValue < filter.intValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.Above:
                    requirementResults.Add((int) localFieldValue > filter.intValue);
                    break;
            }
        }
        else if (filter.fieldType == typeof(float))
        {
            switch (filter.numberConditionType)
            {
                case RPGBuilderEditorFilters.NumberConditionType.Equal:
                    requirementResults.Add((float) localFieldValue == filter.floatValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.EqualOrBelow:
                    requirementResults.Add((float) localFieldValue <= filter.floatValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.EqualOrAbove:
                    requirementResults.Add((float) localFieldValue >= filter.floatValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.Below:
                    requirementResults.Add((float) localFieldValue < filter.floatValue);
                    break;
                case RPGBuilderEditorFilters.NumberConditionType.Above:
                    requirementResults.Add((float) localFieldValue > filter.floatValue);
                    break;
            }
        }
        else if (filter.fieldType == typeof(string))
        {
            if (string.IsNullOrEmpty(filter.text))
            {
                requirementResults.Add(true);
            }
            else
            {
                if (GetFieldIsStringEnum(filter.moduleName, filter.categoryName, filter.fieldName))
                {
                    requirementResults.Add(localFieldValue.ToString().Equals(filter.text));
                }
                else
                {
                    switch (filter.stringValueType)
                    {
                        case RPGBuilderEditorFilters.StringValueType.Equal:
                            requirementResults.Add(localFieldValue.ToString().ToLower().Equals(filter.text.ToLower()));
                            break;
                        case RPGBuilderEditorFilters.StringValueType.Contains:
                            requirementResults.Add(localFieldValue.ToString().ToLower()
                                .Contains(filter.text.ToLower()));
                            break;
                        case RPGBuilderEditorFilters.StringValueType.DoNotContain:
                            requirementResults.Add(
                                !localFieldValue.ToString().ToLower().Contains(filter.text.ToLower()));
                            break;
                    }
                }
            }
        }
        else if (filter.fieldType == typeof(bool))
        {
            requirementResults.Add((bool) localFieldValue == filter.boolValue);
        }
        else if (filter.fieldType == typeof(Sprite))
        {
            requirementResults.Add((Sprite) localFieldValue == filter.sprite);
        }
        else if (filter.fieldType == typeof(RPGAbility))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGAbility) localFieldValue == filter.Ability
                    : (RPGAbility) localFieldValue != filter.Ability);
        }
        else if (filter.fieldType == typeof(RPGEffect))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGEffect) localFieldValue == filter.Effect
                    : (RPGEffect) localFieldValue != filter.Effect);
        }
        else if (filter.fieldType == typeof(RPGNpc))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGNpc) localFieldValue == filter.NPC
                    : (RPGNpc) localFieldValue != filter.NPC);
        }
        else if (filter.fieldType == typeof(RPGStat))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGStat) localFieldValue == filter.Stat
                    : (RPGStat) localFieldValue != filter.Stat);
        }
        else if (filter.fieldType == typeof(RPGTreePoint))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGTreePoint) localFieldValue == filter.TreePoint
                    : (RPGTreePoint) localFieldValue != filter.TreePoint);
        }
        else if (filter.fieldType == typeof(RPGSpellbook))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGSpellbook) localFieldValue == filter.Spellbook
                    : (RPGSpellbook) localFieldValue != filter.Spellbook);
        }
        else if (filter.fieldType == typeof(RPGFaction))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGFaction) localFieldValue == filter.Faction
                    : (RPGFaction) localFieldValue != filter.Faction);
        }
        else if (filter.fieldType == typeof(RPGWeaponTemplate))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGWeaponTemplate) localFieldValue == filter.WeaponTemplate
                    : (RPGWeaponTemplate) localFieldValue != filter.WeaponTemplate);
        }
        else if (filter.fieldType == typeof(RPGItem))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGItem) localFieldValue == filter.Item
                    : (RPGItem) localFieldValue != filter.Item);
        }
        else if (filter.fieldType == typeof(RPGSkill))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGSkill) localFieldValue == filter.Skill
                    : (RPGSkill) localFieldValue != filter.Skill);
        }
        else if (filter.fieldType == typeof(RPGLevelsTemplate))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGLevelsTemplate) localFieldValue == filter.LevelTemplate
                    : (RPGLevelsTemplate) localFieldValue != filter.LevelTemplate);
        }
        else if (filter.fieldType == typeof(RPGRace))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGRace) localFieldValue == filter.Race
                    : (RPGRace) localFieldValue != filter.Race);
        }
        else if (filter.fieldType == typeof(RPGClass))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGClass) localFieldValue == filter.Class
                    : (RPGClass) localFieldValue != filter.Class);
        }
        else if (filter.fieldType == typeof(RPGLootTable))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGLootTable) localFieldValue == filter.LootTable
                    : (RPGLootTable) localFieldValue != filter.LootTable);
        }
        else if (filter.fieldType == typeof(RPGMerchantTable))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGMerchantTable) localFieldValue == filter.MerchantTable
                    : (RPGMerchantTable) localFieldValue != filter.MerchantTable);
        }
        else if (filter.fieldType == typeof(RPGCurrency))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGCurrency) localFieldValue == filter.Currency
                    : (RPGCurrency) localFieldValue != filter.Currency);
        }
        else if (filter.fieldType == typeof(RPGCraftingRecipe))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGCraftingRecipe) localFieldValue == filter.CraftingRecipe
                    : (RPGCraftingRecipe) localFieldValue != filter.CraftingRecipe);
        }
        else if (filter.fieldType == typeof(RPGCraftingStation))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGCraftingStation) localFieldValue == filter.CraftingStation
                    : (RPGCraftingStation) localFieldValue != filter.CraftingStation);
        }
        else if (filter.fieldType == typeof(RPGTalentTree))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGTalentTree) localFieldValue == filter.TalentTree
                    : (RPGTalentTree) localFieldValue != filter.TalentTree);
        }
        else if (filter.fieldType == typeof(RPGBonus))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGBonus) localFieldValue == filter.Bonus
                    : (RPGBonus) localFieldValue != filter.Bonus);
        }
        else if (filter.fieldType == typeof(RPGGearSet))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGGearSet) localFieldValue == filter.GearSet
                    : (RPGGearSet) localFieldValue != filter.GearSet);
        }
        else if (filter.fieldType == typeof(RPGEnchantment))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGEnchantment) localFieldValue == filter.Enchantment
                    : (RPGEnchantment) localFieldValue != filter.Enchantment);
        }
        else if (filter.fieldType == typeof(RPGTask))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGTask) localFieldValue == filter.Task
                    : (RPGTask) localFieldValue != filter.Task);
        }
        else if (filter.fieldType == typeof(RPGQuest))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGQuest) localFieldValue == filter.Quest
                    : (RPGQuest) localFieldValue != filter.Quest);
        }
        else if (filter.fieldType == typeof(RPGWorldPosition))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGWorldPosition) localFieldValue == filter.WorldPosition
                    : (RPGWorldPosition) localFieldValue != filter.WorldPosition);
        }
        else if (filter.fieldType == typeof(RPGResourceNode))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGResourceNode) localFieldValue == filter.ResourceNode
                    : (RPGResourceNode) localFieldValue != filter.ResourceNode);
        }
        else if (filter.fieldType == typeof(RPGGameScene))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGGameScene) localFieldValue == filter.GameScene
                    : (RPGGameScene) localFieldValue != filter.GameScene);
        }
        else if (filter.fieldType == typeof(RPGDialogue))
        {
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? (RPGDialogue) localFieldValue == filter.Dialogue
                    : (RPGDialogue) localFieldValue != filter.Dialogue);
        }
        else if (filter.fieldType.IsEnum)
        {
            int filterIndex = filter.enumIndex;
            Enum enumRefLocal = (Enum) localFieldValue;
            int localIndex =
                GetEnumValueIndexByName(localFieldValue.GetType().GetEnumValues(), enumRefLocal.ToString());
            requirementResults.Add(
                filter.entryReferenceConditionType == RPGBuilderEditorFilters.EntryReferenceConditionType.Equal
                    ? localIndex == filterIndex
                    : localIndex != filterIndex);
        }

    }

    private void HandleFiltersCheck(List<RPGBuilderEditorFilters.FilterEntryData> filters, object obj, List<bool> requirementResults)
    {
        foreach (var filter in filters)
        {
            if (string.IsNullOrEmpty(filter.fieldName)) continue;
            var localField = obj.GetType().GetField(filter.fieldName);

            if (localField == null)
            {
                bool found = false;
                // FIELD NOT EXISTING IN MAIN CLASS
                foreach (RPGBuilderEditorFilters.EntryFieldList nestedCategory in EntryFieldList)
                {
                    foreach (RPGBuilderEditorFilters.EntryField field in nestedCategory.fieldList)
                    {
                        if (!FilterScanObject(obj, field, filter, requirementResults)) continue;
                        found = true;
                        break;
                    }

                    if (found) break;
                }
            }
            else
            {
                object localFieldValue = localField.GetValue(obj);
                HandleRequirementResults(filter, requirementResults, localFieldValue);
            }
        }
    }

    private bool FilterScanObject(object obj, RPGBuilderEditorFilters.EntryField fieldChecked,
        RPGBuilderEditorFilters.FilterEntryData filter, List<bool> requirementResults, int currentDepth = 0,
        int maxDepth = 10)
    {
        bool debug = false;

        // if we hit max recursion depth, exit
        if (++currentDepth > maxDepth) return false;

        // exit if we dont have a good field filter
        if (string.IsNullOrEmpty(filter.fieldName)) return false;

        Type objType = obj.GetType();

        // if there is a namespace make sure its RPGBuilder
        if (!objType.IsClass || (objType.Namespace != null && !objType.Namespace.Contains("RPGBuilder"))) return false;

        if (debug) Debug.Log("scanning object: " + objType.Name);

        // search the fields for the current object
        foreach (FieldInfo field in objType.GetFields())
        {
            //if (field.Name == filter.fieldName && field.GetType() == filter.fieldType) {
            if (field.Name == filter.fieldName)
            {

                if (debug) Debug.Log("found: " + filter.fieldName + " | " + filter.fieldType);

                HandleRequirementResults(filter, requirementResults, field.GetValue(obj));
                return true;
            }
            else
            {

                // if this isnt what we are looking for we want to search the value of this field
                // we have two scenarios.  list or object.

                // get the value of the current field
                object fieldValue = field.GetValue(obj);

                // skip any system type fields as we do not need to recurse those
                if (fieldValue != null && fieldValue.GetType().IsClass &&
                    (fieldValue.GetType().ToString().Contains("System.Collections.Generic.List") ||
                     (!fieldValue.GetType().ToString().Contains("UnityEngine.") &&
                      !fieldValue.GetType().ToString().Contains("System."))))
                {

                    // we cant recurse if the object is null
                    if (fieldValue == null) continue;

                    if (debug) Debug.Log("fieldname: " + field.Name);

                    // cast to list to see if it is a list
                    IList objectList = fieldValue as IList;

                    // if we have a list, loop each child instance and search recursively
                    if (objectList != null)
                    {
                        if (debug) Debug.Log("scanning list");

                        foreach (object childObj in objectList)
                        {
                            if (FilterScanObject(childObj, fieldChecked, filter, requirementResults, currentDepth,
                                maxDepth))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        // we just want to search this single object
                        return FilterScanObject(fieldValue, fieldChecked, filter, requirementResults, currentDepth,
                            maxDepth);
                    }
                }
            }
        }

        return false;
    }

    private bool EntryMatchFilters(DatabaseEntrySlot element, int index)
    {
        if (cachedRequirementResults == null) cachedRequirementResults = new Dictionary<int, bool>();
        if (cachedRequirementResults.ContainsKey(element.ID))
        {
            return cachedRequirementResults[element.ID];
        }

        List<bool> requirementResults = new List<bool>();
        HandleFiltersCheck(currentFilterList, CurrentEditorModule.databaseEntries[index], requirementResults);

        cachedRequirementResults.Add(element.ID, !requirementResults.Contains(false));
        return !requirementResults.Contains(false);
    }

    private bool EntryNameMatchSearchText(string entryName)
    {
        return string.IsNullOrEmpty(moduleSearchBarText) || entryName.ToLower().Contains(moduleSearchBarText.ToLower());
    }

    private void CollapseAllCategoriesButTheCurrentOne()
    {
        foreach (var category in EditorCategories.Where(category => category.category != CurrentEditorCategory))
        {
            category.category.IsExpanded = false;
        }
    }

    public object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        if (prop == null) return null;

        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }

        return obj;
    }

    private object GetValue_Imp(object source, string name)
    {
        if (source == null)
            return null;
        var type = source.GetType();

        while (type != null)
        {
            var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (f != null)
                return f.GetValue(source);

            var p = type.GetProperty(name,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p != null)
                return p.GetValue(source, null);

            type = type.BaseType;
        }

        return null;
    }

    private object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();

        for (var i = 0; i <= index; i++)
            if (!enm.MoveNext())
                return null;
        return enm.Current;
    }

    private void AssetActions(UnityEngine.Object dirtyObject, bool refresh)
    {
        EditorUtility.DisplayProgressBar("RPG Builder Editor", "Processing Data", 0.5f);
        EditorUtility.SetDirty(dirtyObject);
        AssetDatabase.SaveAssets();
        if (refresh) AssetDatabase.Refresh();
    }

    private void AssetActionsAfterCreate()
    {
        EditorUtility.DisplayProgressBar("RPG Builder Editor", "Processing Data", 0.5f);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    public void RequestFilterCheck()
    {
        targetFiltersNeedValidation = true;
        updateValidateFilters = true;
        cachedRequirementResults = null;
    }

    public void RequestEntryListRedraw()
    {
        cachedRequirementResults = null;
        updateEntryList = true;
    }

    private void PrepareViewArea()
    {
        float viewWidth = 1 - EditorData.ElementListWidthPercent - EditorData.CategoryMenuWidthPercent;
        if (ShowFilters)
        {
            viewWidth -= EditorData.FilterWidthPercent;
        }

        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth() * viewWidth;
        Rect panelRect = new Rect(CategoryWidth + EntryListWidth, 0, panelWidth,
            position.height);

        if (ShowFilters)
        {
            panelRect.x -= 25;
        }

        GUILayout.BeginArea(panelRect);
    }

    private void PrepareSettingsViewArea()
    {
        float viewWidth = 1 - EditorData.CategoryMenuWidthPercent;
        float panelWidth = RPGBuilderEditorUtility.GetScreenWidth() * viewWidth;
        Rect panelRect = new Rect(CategoryWidth, 0, panelWidth,
            position.height);

        GUILayout.BeginArea(panelRect);
    }
    
    public bool LoadSettings()
    {
        EditorSettings = Resources.Load<RPGBuilderEditorSettings>(EditorData.RPGBDatabasePath + "Settings/" + "Editor_Settings");
        if (EditorSettings != null)
        {
            EditorSettings = Instantiate(EditorSettings);
        }
        else
        {
            return false;
        }
        
        CombatSettings = Resources.Load<RPGBuilderCombatSettings>(EditorData.RPGBDatabasePath + "Settings/" + "Combat_Settings");
        if (CombatSettings != null)
        {
            CombatSettings = Instantiate(CombatSettings);
        }
        else
        {
            return false;
        }

        CharacterSettings = Resources.Load<RPGBuilderCharacterSettings>(EditorData.RPGBDatabasePath + "Settings/" + "Character_Settings");
        if (CharacterSettings != null)
        {
            CharacterSettings = Instantiate(CharacterSettings);
        }
        else
        {
            return false;
        }

        EconomySettings = Resources.Load<RPGBuilderEconomySettings>(EditorData.RPGBDatabasePath + "Settings/" + "Economy_Settings");
        if (EconomySettings != null)
        {
            EconomySettings = Instantiate(EconomySettings);
        }
        else
        {
            return false;
        }

        ProgressionSettings = Resources.Load<RPGBuilderProgressionSettings>(EditorData.RPGBDatabasePath + "Settings/" + "Progression_Settings");
        if (ProgressionSettings != null)
        {
            ProgressionSettings = Instantiate(ProgressionSettings);
        }
        else
        {
            return false;
        }

        WorldSettings = Resources.Load<RPGBuilderWorldSettings>(EditorData.RPGBDatabasePath + "Settings/" + "World_Settings");
        if (WorldSettings != null)
        {
            WorldSettings = Instantiate(WorldSettings);
        }
        else
        {
            return false;
        }

        GeneralSettings = Resources.Load<RPGBuilderGeneralSettings>(EditorData.RPGBDatabasePath + "Settings/" + "General_Settings");
        if (GeneralSettings != null)
        {
            GeneralSettings = Instantiate(GeneralSettings);
        }
        else
        {
            return false;
        }

        UISettings = Resources.Load<RPGBuilderUISettings>(EditorData.RPGBDatabasePath + "Settings/" + "UI_Settings");
        if (UISettings != null)
        {
            UISettings = Instantiate(UISettings);
        }
        else
        {
            return false;
        }

        return true;
    }

    private void ResetFilterWindow()
    {
        currentFilterEntryDataIndex = -1;
        isSelectingField = false;
        ShowFilters = false;
    }
}
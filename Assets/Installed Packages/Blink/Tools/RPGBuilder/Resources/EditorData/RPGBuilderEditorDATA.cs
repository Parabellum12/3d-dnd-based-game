using UnityEditor;
using UnityEngine;

public class RPGBuilderEditorDATA : ScriptableObject
{
    public string ResourcePath = "Assets/Blink/Tools/RPGBuilder/Resources/", RPGBDatabasePath = "Database/", RPGBEditorDataPath = "EditorData/";

#if UNITY_EDITOR
    public SceneAsset MainMenuScene;
    public SceneAsset DemoScene;
    
#endif
    
    public bool disableStartupWindow;
    
    public float MinEditorWidth = 1200;
    public float MinEditorHeight = 600;
        
    public Sprite RPGBuilderLogo, BlinkLogoOff, BlinkLogoOn, BlinkBanner, BlinkSmallLogoOff, BlinkSmallLogoOn;
    
    public float CategoryWidthPercent, SubCategoryWidthPercent, CategoryMenuWidthPercent, ElementListWidthPercent, ViewWidthPercent, FilterWidthPercent, TopBarHeightPercent;
    public float viewSmallFieldHeight, smallButtonHeight;
    public float labelFieldWidth = 100f, filterLabelFieldWidth = 50;
    public float SmallActionButtonWidth, MediumActionButtonWidth, BigActionButtonWidth;
    public Texture2D bannerBegin, bannerMiddle, bannerEnd;
    
    public float actionButtonsY;
    public float ModuleButtonsY;
    public float EntryListY;
    
    public Sprite defaultEntryIcon;
    public Texture2D  abilityNullSprite, gearSetsSeparator;

    public GameObject AILogicTemplate;
    
    // Do not delete. This loads the Font for the editor, preventing a visual issue when opening it
    public Font textFont;
    public Font squareRemoveButtonFont;
    // Do not delete. This loads the Font for the editor, preventing a visual issue when opening it

    public Texture2D BackgroundTexture1;
    public Texture2D BackgroundTexture2;
    public Texture2D SearchBarTexture;
    public Texture2D SearchBarHoveredTexture;
    public Texture2D AddButtonTexture;
    public Texture2D AddButtonHoveredTexture;
    public Texture2D RemoveButtonTexture;
    public Texture2D RemoveButtonHoveredTexture;
    public Texture2D GenericButtonTexture;
    public Texture2D GenericButtonHoveredTexture;
    public Texture2D AddButtonSquareTexture;
    public Texture2D AddButtonSquareHoveredTexture;
    public Texture2D RemoveButtonSquareTexture;
    public Texture2D RemoveButtonSquareHoveredTexture;
    public Texture2D AbilityTooltipBackground;
    public Texture2D AbilityTooltipBackgroundHover;
    public Texture2D CustomTextField;
    public Texture2D CustomTextFieldHover;
    public Texture2D FillBarBackground;
    public Texture2D FillBar;
    public Texture2D FillBarHover;
}
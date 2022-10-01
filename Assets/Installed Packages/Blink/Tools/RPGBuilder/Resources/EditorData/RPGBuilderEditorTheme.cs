using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New RPG Builder Editor Theme", menuName = "BLINK/RPG Builder Editor/Theme")]
public class RPGBuilderEditorTheme : ScriptableObject
{
    public Color BackgroundColor1;
    public Color BackgroundColor2;
    
    public Color BannerCollapsed;
    public Color BannerTextCollapsed;
    public Color BannerExpanded;
    public Color BannerTextExpanded;
    public Color BannerHovered;
    public Color BannerTextHovered;
    
    public Color SearchBar;
    public Color SearchBarHovered;
    
    public Color AddButton;
    public Color AddButtonHover;
    public Color RemoveButton;
    public Color RemoveButtonHover;
    public Color GenericButton;
    public Color GenericButtonHover;
    
    public Color AbilityTooltipColor1;
    public Color AbilityTooltipColor2;
    
    public Color CustomTextField;
    public Color CustomTextFieldHover;
    
    public Color FillBarBackground;
    public Color FillBar;
    public Color FillBarHover;

    public Color TextLabelColor;
    public Color TextLabelColorHover;
    public Color TextValueColor;
    public Color TextValueColorHover;
    
    public Color TitleLabelColor;
    public Color TitleLabelColorHover;
}

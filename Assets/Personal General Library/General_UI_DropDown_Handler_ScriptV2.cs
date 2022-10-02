using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class General_UI_DropDown_Handler_ScriptV2 : MonoBehaviour
{
    /*
     * 
     * 
     * turns out i did have the time and will to create a v2, who would've thought a few hrs of bordem would do this to me
     * 
     * also it turns out its only minor improvements over v1 but i use v1 and have calculated stuff for v1 in too many places for me to care enough to change it all right now
     * 
     */
    [SerializeField] bool isHolder = false;
    [SerializeField] bool isCanvasOrUiItem = false;
    [SerializeField] bool dropDownImageFollowChildSize = true;

    [SerializeField] bool engaged = false;

    [SerializeField] Image mainImage = null;
    [SerializeField] GameObject dropDownBackgroundImage = null;
    [SerializeField] Button InteractionButton = null;
    RectTransform buttonRectTransform;
    [SerializeField] RectTransform contentRectTransform;

    [SerializeField] List<General_UI_DropDown_Handler_ScriptV2> childDropDowns = new List<General_UI_DropDown_Handler_ScriptV2>();

    public System.Action updateUICallback = null;

    public GameObject ChildrenObjectHolder;

    [SerializeField] float globalOffsetDist = 0;
    [SerializeField] float itemSeperationDist = 0;

    [SerializeField] float offsetDist = 0;

    float lastOffsetSize = 0;

    private void Start()
    {
        setup();
    }
    bool alreadySetup = false;
    void setup()
    {
        if (alreadySetup)
        {
            return;
        }
        alreadySetup = true;
        if (InteractionButton != null)
        {
            buttonRectTransform = InteractionButton.GetComponent<RectTransform>();
            InteractionButton.onClick.AddListener(() =>
            {
                handleClick();
            });
        }
        if (!isCanvasOrUiItem)
        {
            engaged = false;

            if (dropDownBackgroundImage != null)
            {
                dropDownBackgroundImage.SetActive(false);
            }

            if (ChildrenObjectHolder != null)
            {
                ChildrenObjectHolder.SetActive(false);
                dropDownBackgroundImage.SetActive(false);
                buttonRectTransform.transform.rotation = Quaternion.identity;
            }
        }

        for (int i = 0; i < childDropDowns.Count; i++)
        {
            General_UI_DropDown_Handler_ScriptV2 scr = childDropDowns[i];
            if (scr == null)
            {
                Debug.LogWarning("Scr " + i + " On This Is Null");
            }
            scr.setup();
            scr.updateUICallback = () =>
            {
                setUIPositions();
            };
        }
    }



    public float getSize()
    {
        if (dropDownBackgroundImage != null && dropDownBackgroundImage.activeSelf && !isHolder)
        {
            return dropDownBackgroundImage.GetComponent<RectTransform>().rect.height + mainImage.rectTransform.rect.height;
        }
        else
        {
            return mainImage.rectTransform.rect.height;
        }
    }

    public float getMainImageSize()
    {
        if (mainImage != null)
        {
            return mainImage.rectTransform.rect.height;
        }
        return 0;
    }

    //called after setup and child or self changed
    public void setUIPositions()
    {
        float mainImageHeight = 0;
        if (mainImage != null)
        {
            mainImageHeight = (mainImage.rectTransform.rect.height / 2);
        }
        offsetDist = globalOffsetDist + mainImageHeight + itemSeperationDist + 2;
        for (int i = 0; i < childDropDowns.Count; i++)
        {
            if (!childDropDowns[i].gameObject.activeSelf)
            {
                continue;
            }
            childDropDowns[i].transform.localPosition = new Vector3(0, -offsetDist - (childDropDowns[i].getMainImageSize() / 2), 0);
            offsetDist += childDropDowns[i].getSize() + itemSeperationDist;
            lastOffsetSize = childDropDowns[i].getSize() + itemSeperationDist;
        }
        setPosHelper();
        updateUICallback?.Invoke();
    }

    //call to setup
    public void setUIPositionsNoCallback()
    {
        float mainImageHeight = 0;
        if (mainImage != null)
        {
            mainImageHeight = (mainImage.rectTransform.rect.height / 2f);
        }
        offsetDist = globalOffsetDist + mainImageHeight + itemSeperationDist + 2;
        for (int i = 0; i < childDropDowns.Count; i++)
        {
            if (!childDropDowns[i].gameObject.activeSelf)
            {
                continue;
            }
            childDropDowns[i].setUIPositionsNoCallback();
            childDropDowns[i].transform.localPosition = new Vector3(0, -offsetDist - (childDropDowns[i].getSize()/2), 0);
            offsetDist += childDropDowns[i].getSize() + itemSeperationDist;
            lastOffsetSize = childDropDowns[i].getSize() + itemSeperationDist;
        }
        setPosHelper();
    }

    void setPosHelper()
    {
        if (isHolder)
        {
            return;
        }
        if (!isCanvasOrUiItem)
        {
            if (dropDownImageFollowChildSize)
            {
                RectTransform temp = dropDownBackgroundImage.GetComponent<RectTransform>();
                //Debug.Log(gameObject.name + ":rectFollowSize");
                //temp.rect.Set(temp.rect.x, temp.rect.y, temp.rect.width, offsetDist - globalOffset);
                temp.sizeDelta = new Vector2(temp.rect.width, offsetDist- (mainImage.rectTransform.rect.height / 2f));// - globalOffsetDist - (childDropDowns.Count * itemSeperationDist));
                //Debug.Log(gameObject.name + ":TempHeight5:" + ((temp.rect.size.y / 2f) - globalOffset));
                //temp.localPosition = Vector2.zero;
                temp.localPosition = new Vector2(0, -(temp.rect.size.y / 2) - (gameObject.GetComponent<RectTransform>().rect.height / 2));
                //Debug.Log(gameObject.name + ":TemplocalPos:" + temp.localPosition);
            }
        }
        else
        {
            contentRectTransform.anchorMin = new Vector2(.5f, 1f);
            contentRectTransform.anchorMax = new Vector2(.5f, 1f);
            contentRectTransform.sizeDelta = new Vector2(512, offsetDist - globalOffsetDist);
        }
    }

    public void handleClick()
    {
        if (isCanvasOrUiItem)
        {
            return;
        }

        if (!engaged)
        {
            setDropDownToActive();
        }
        else
        {
            setDropDownToInactive();
        }

        engaged = !engaged;
        setUIPositions();
    }

    public void setDropDownToActive()
    {
        setup();
        dropDownBackgroundImage.SetActive(true);
        buttonRectTransform.rotation = Quaternion.Euler(0, 0, 180);
        ChildrenObjectHolder.SetActive(true);
        setUIPositions();
    }

    public void setDropDownToInactive()
    {
        setup();
        dropDownBackgroundImage.SetActive(false);
        buttonRectTransform.rotation = Quaternion.Euler(0, 0, 0);
        ChildrenObjectHolder.SetActive(false);
        setUIPositions();
    }



    public void addToChildDropDowns(General_UI_DropDown_Handler_ScriptV2 childToAdd)
    {
        childDropDowns.Add(childToAdd);
        childToAdd.updateUICallback = () =>
        {
            setUIPositions();
        };

    }

    public void removeFromChildDropDowns(General_UI_DropDown_Handler_ScriptV2 childToRemove)
    {
        childDropDowns.Remove(childToRemove);
    }

    public void clearChildDropDowns()
    {
        childDropDowns.Clear();
    }


}

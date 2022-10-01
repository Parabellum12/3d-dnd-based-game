using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class UtilClass 
{


    //create world text
    public static TextMeshPro createWorldText(string text, Transform parent = null, Vector3 localPos = default(Vector3), int fontSize = 40, Color color = default(Color), TMPro.TextContainerAnchors textAnchor = TMPro.TextContainerAnchors.Middle, TMPro.TextAlignmentOptions textAlignment = TMPro.TextAlignmentOptions.Center, int sorintOrder = 0)
    {
        if (color == null)
        {
            color = Color.white;
        }
        return createWorldText2(parent, text, localPos, fontSize, color, textAnchor, textAlignment, sorintOrder);
    }

    public static TextMeshPro createWorldText2(Transform parent, string text, Vector3 localPos, int fontSize, Color color, TMPro.TextContainerAnchors textAnchor, TMPro.TextAlignmentOptions textAlignment, int sorintOrder)
    {
        GameObject gameObject = new GameObject("WorldText", typeof(TextMeshPro));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPos;
        TextMeshPro textmesh = gameObject.GetComponent<TextMeshPro>();
        textmesh.enableAutoSizing = true;
        textmesh.fontSizeMin = 5;
        textmesh.alignment = textAlignment;
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(10, 10);
        textmesh.text = text;
        textmesh.fontSize = fontSize;
        textmesh.color = color;
        textmesh.GetComponent<MeshRenderer>().sortingOrder = sorintOrder;
        return textmesh;
    }






    //get mouse world position
    public static Vector3 getMouseWorldPosition()
    {
        Vector3 vec = getMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        vec.z = 0f;
        return vec;
    }

    public static Vector3 getMouseWorldPositionWithZ()
    {
        return getMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
    }

    public static Vector3 getMouseWorldPositionWithZ(Camera worldCam)
    {
        return getMouseWorldPositionWithZ(Input.mousePosition, worldCam);
    }

    public static Vector3 getMouseWorldPositionWithZ(Vector3 screenPos, Camera worldCam)
    {
        Vector3 worldPos = worldCam.ScreenToWorldPoint(screenPos);
        return worldPos;
    }
}


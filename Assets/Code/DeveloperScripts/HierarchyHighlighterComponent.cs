#if UNITY_EDITOR
using UnityEngine;

public class HierarchyHighlighterComponent : MonoBehaviour 
{
    //attach this to the gameobject you want to highlight in the hierarchy

    public bool highlight = true;
    [ColorUsage(true, true)]
    public Color color = Color.yellow;
}
#endif
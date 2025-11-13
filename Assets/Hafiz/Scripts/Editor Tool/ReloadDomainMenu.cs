using UnityEngine;
using UnityEditor;

public class ReloadDomainMenu
{
    [MenuItem("Tools/Reload Domain %#&r")]
    static void ReloadDomain()
    {
        EditorUtility.RequestScriptReload();
    }
}

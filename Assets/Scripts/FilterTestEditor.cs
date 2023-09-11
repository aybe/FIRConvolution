#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(FilterTest))]
internal sealed class FilterTestEditor : Editor
{
    // BUG some more Unity shit -> no VU meter unless class has an editor!

    public override void OnInspectorGUI()
    {
        if (EditorApplication.isPlaying)
        {
            EditorGUILayout.HelpBox("When profiling, collapse this component to avoid VU meter overhead.", MessageType.Warning, true);
        }

        base.OnInspectorGUI();
    }
}
#endif
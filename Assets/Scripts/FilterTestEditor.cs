#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(FilterTest))]
internal sealed class FilterTestEditor : Editor
{
    // BUG some more Unity shit -> no VU meter unless class has an editor!
}
#endif
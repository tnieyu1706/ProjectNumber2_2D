namespace FewClicksDev.Core.Versioning
{
    using UnityEngine;

    [System.Serializable]
    public class VersionDescription
    {
        [SerializeField] private Version currentVersion = default;
        [SerializeField] private string[] changes = null;

        public string VersionString => currentVersion.ToString();
        public string[] Changes => changes;
    }
}

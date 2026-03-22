using UnityEngine;

namespace ArchiveOfEchoes
{
    [CreateAssetMenu(menuName = "Archive/Knowledge Key", fileName = "Key_")]
    public class KnowledgeKeyData : ScriptableObject
    {
        [Tooltip("Uppercase identifier used in save state, e.g. 'MANDATE'")]
        public string keyId;
        public string displayName;
        [TextArea(2, 5)] public string description;
        public Sprite icon;
        public LensType[] relevantLenses;
        [Tooltip("If true, this key must be collected to unlock T5 Convergence")]
        public bool isRequiredForT5;
    }
}

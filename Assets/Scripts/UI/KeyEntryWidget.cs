using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// A single row in the ArchiveNotebook's knowledge-key list.
    /// Populated by ArchiveNotebook.RevealKey() when a key is earned.
    /// </summary>
    public class KeyEntryWidget : MonoBehaviour
    {
        [SerializeField] private Text  keyNameLabel;
        [SerializeField] private Text  keyDescriptionLabel;
        [SerializeField] private Image keyIcon;

        public string KeyId { get; private set; }

        public void Populate(KnowledgeKeyData data)
        {
            KeyId = data.keyId;

            if (keyNameLabel != null)
                keyNameLabel.text = data.displayName;

            if (keyDescriptionLabel != null)
                keyDescriptionLabel.text = data.description;

            if (keyIcon != null && data.icon != null)
                keyIcon.sprite = data.icon;
        }
    }
}

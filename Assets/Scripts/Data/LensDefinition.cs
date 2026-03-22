using UnityEngine;

namespace ArchiveOfEchoes
{
    [CreateAssetMenu(menuName = "Archive/Lens Definition", fileName = "Lens_")]
    public class LensDefinition : ScriptableObject
    {
        public LensType lensType;
        public string displayName;
        public Color lensColor;
        public Sprite lensIcon;
        [Tooltip("Post-process material applied to panel images while this lens is active")]
        public Material lensPostProcessMaterial;
        [Tooltip("Root-note drone clip for ambient audio while this lens is active")]
        public AudioClip ambientNote;
        [TextArea(1, 3)] public string unlockFlavourText;
    }
}

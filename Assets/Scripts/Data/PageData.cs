using UnityEngine;

namespace ArchiveOfEchoes
{
    [CreateAssetMenu(menuName = "Archive/Page Data", fileName = "Page_")]
    public class PageData : ScriptableObject
    {
        public string pageId;
        public PanelData[] panels;
        public PageLayout layout;
        public PageTransition entryTransition;
        [Tooltip("When true the page uses a single full-bleed image with no panel grid")]
        public bool isFullBleed;
    }
}

using UnityEngine;

namespace ArchiveOfEchoes
{
    [CreateAssetMenu(menuName = "Archive/Issue Data", fileName = "Issue_")]
    public class IssueData : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Matches storyboard filename, e.g. 'issue_00'")]
        public string issueId;
        public int issueNumber;
        public string title;
        public string arc;

        [Header("Content")]
        public PageData[] pages;
        public Sprite coverArt;

        [Header("Unlock")]
        [Tooltip("All listed issues must be complete before this one is accessible")]
        public string[] prerequisiteIssueIds;
        public LensType[] unlocksLenses;
        public KnowledgeKeyData[] unlocksKeys;

        [Header("Audio")]
        public AudioClip ambientTrack;
    }
}

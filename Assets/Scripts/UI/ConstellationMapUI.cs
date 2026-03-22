using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Timeline Constellation Map UI — Archive Notebook tab.
    ///
    /// Displays all timeline cluster nodes (T1–T5) as "stars" connected by lines.
    /// Completed clusters glow fully; current cluster pulses; locked clusters are dim.
    /// On completion of Issues 08–09 the nodes' connector lines unconsciously outline
    /// a pyramid silhouette (no label — the shape is the revelation).
    ///
    /// Access: swipe up from bottom → Archive Notebook → Constellation tab.
    /// </summary>
    public class ConstellationMapUI : MonoBehaviour
    {
        [System.Serializable]
        public class ClusterNode
        {
            [Tooltip("Issue IDs that belong to this cluster (any completed = cluster started)")]
            public string[] issueIds;
            public string label;
            public RectTransform nodeTransform;
            public Image nodeGlow;
            public Text nodeLabel;
        }

        [SerializeField] private ClusterNode[] nodes;
        [SerializeField] private LineRenderer constellationLines;

        [Header("Colors")]
        [SerializeField] private Color lockedColor  = new(0.4f, 0.4f, 0.4f, 0.35f);
        [SerializeField] private Color activeColor  = new(1f, 0.92f, 0.55f, 1f);
        [SerializeField] private Color completeColor = new(1f, 1f, 1f, 1f);

        private float _pulseTimer;

        private void OnEnable() => Refresh();

        private void Update()
        {
            // Pulse the active cluster node
            _pulseTimer += Time.deltaTime;
            float pulse = 0.75f + Mathf.Sin(_pulseTimer * 3f) * 0.25f;
            var state = GameManager.Instance?.State;
            if (state == null) return;

            foreach (var node in nodes)
            {
                if (IsCurrentCluster(node, state))
                {
                    var c = node.nodeGlow.color;
                    c.a = pulse;
                    node.nodeGlow.color = c;
                }
            }
        }

        // ── Refresh ───────────────────────────────────────────────────────────────

        public void Refresh()
        {
            if (GameManager.Instance == null) return;
            var state = GameManager.Instance.State;

            var linePoints = new List<Vector3>();

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                bool complete = IsClusterComplete(node, state);
                bool current  = IsCurrentCluster(node, state);
                bool locked   = !complete && !current && !AnyIssueStarted(node, state);

                node.nodeGlow.color = locked ? lockedColor : complete ? completeColor : activeColor;
                node.nodeLabel.text = node.label;
                node.nodeLabel.color = locked ? lockedColor : completeColor;

                if (!locked) linePoints.Add(node.nodeTransform.position);
            }

            // Draw constellation connector lines
            if (constellationLines != null)
            {
                constellationLines.positionCount = linePoints.Count;
                constellationLines.SetPositions(linePoints.ToArray());
            }
        }

        // ── Cluster state queries ─────────────────────────────────────────────────

        private static bool IsClusterComplete(ClusterNode node, ArchiveState state)
        {
            foreach (string id in node.issueIds)
                if (!state.IsIssueComplete(id)) return false;
            return node.issueIds.Length > 0;
        }

        private static bool AnyIssueStarted(ClusterNode node, ArchiveState state)
        {
            foreach (string id in node.issueIds)
                if (state.IsIssueComplete(id) || state.currentIssueId == id) return true;
            return false;
        }

        private static bool IsCurrentCluster(ClusterNode node, ArchiveState state)
        {
            foreach (string id in node.issueIds)
                if (state.currentIssueId == id) return true;
            return false;
        }
    }
}

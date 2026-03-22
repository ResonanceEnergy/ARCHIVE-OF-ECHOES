using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArchiveOfEchoes.Editor
{
    /// <summary>
    /// Programmatically constructs all Unity scenes and prefabs for the vertical slice.
    ///
    /// Scenes produced (Assets/Scenes/):
    ///   Title         — Persistent singletons + title/disclaimer UI
    ///   Frame2100     — 2100 framing scene (opening + epilogue)
    ///   ComicReader   — Core comic reader with all overlay systems
    ///   IssueComplete — Between-issue summary screen
    ///
    /// Prefabs produced (Assets/Prefabs/):
    ///   GameManagerRoot    — GameManager + TouchInputManager + NarrativeState + LensSystem
    ///   AudioManagerPrefab — AudioManager + 3 AudioSources
    ///   PageView           — PageViewController + PanelContainer child
    ///   PanelRenderer      — PanelRenderer + required child Images/Texts
    ///   KeyBadgePrefab     — Simple Text badge used by IssueCompleteController
    ///   KeyEntryWidget     — Key entry row used by ArchiveNotebook
    ///   LensSlotPrefab     — One radial lens slot for LensSelectorUI
    /// </summary>
    public static class ArchiveSceneBuilder
    {
        // iPhone 14 portrait reference resolution
        private static readonly Vector2 RefRes = new(1170f, 2532f);

        // ── Entry ─────────────────────────────────────────────────────────────────

        public static void BuildAll()
        {
            BuildPrefabs();
            BuildTitleScene();
            BuildFrame2100Scene();
            BuildComicReaderScene();
            BuildIssueCompleteScene();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Archive] All scenes and prefabs built.");
        }

        // =====================================================================
        // PREFABS
        // =====================================================================

        private static void BuildPrefabs()
        {
            BuildGameManagerRootPrefab();
            BuildAudioManagerPrefab();
            BuildLensSlotPrefab();
            BuildPanelRendererPrefab();
            BuildPageViewPrefab();
            BuildKeyEntryWidgetPrefab();
            BuildKeyBadgePrefab();
            Debug.Log("[Archive] Prefabs built.");
        }

        // ── GameManagerRoot ───────────────────────────────────────────────────────

        private static void BuildGameManagerRootPrefab()
        {
            var root = new GameObject("GameManagerRoot");
            root.AddComponent<GameManager>();
            root.AddComponent<TouchInputManager>();
            root.AddComponent<NarrativeState>();
            root.AddComponent<LensSystem>();

            SavePrefab(root, "Assets/Prefabs/GameManagerRoot.prefab");
            Object.DestroyImmediate(root);
        }

        // ── AudioManagerPrefab ────────────────────────────────────────────────────

        private static void BuildAudioManagerPrefab()
        {
            var root = new GameObject("AudioManager");
            var am   = root.AddComponent<AudioManager>();

            // Three AudioSources: droneA, droneB, motif
            var srcA    = AddAudioSourceChild(root, "DroneA",   loop: true,  volume: 0.4f);
            var srcB    = AddAudioSourceChild(root, "DroneB",   loop: true,  volume: 0f);
            var srcMotif = AddAudioSourceChild(root, "Motif",   loop: false, volume: 0.8f);

            var so = new SerializedObject(am);
            so.FindProperty("droneA").objectReferenceValue    = srcA;
            so.FindProperty("droneB").objectReferenceValue    = srcB;
            so.FindProperty("motifSource").objectReferenceValue = srcMotif;
            so.ApplyModifiedPropertiesWithoutUndo();

            SavePrefab(root, "Assets/Prefabs/AudioManagerPrefab.prefab");
            Object.DestroyImmediate(root);
        }

        private static AudioSource AddAudioSourceChild(GameObject parent, string name,
                                                        bool loop, float volume)
        {
            var go  = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            var src           = go.AddComponent<AudioSource>();
            src.loop          = loop;
            src.volume        = volume;
            src.playOnAwake   = false;
            src.spatialBlend  = 0f;
            return src;
        }

        // ── LensSlotPrefab ────────────────────────────────────────────────────────

        private static void BuildLensSlotPrefab()
        {
            var root = new GameObject("LensSlot", typeof(RectTransform));
            root.AddComponent<CanvasGroup>();
            var slot = root.AddComponent<LensSlot>();

            // Background
            var bg  = CreateImageChild(root.transform, "Background",
                new Color(0.10f, 0.08f, 0.15f, 0.90f));

            // Icon (no sprite yet)
            var icon = CreateImageChild(root.transform, "Icon", Color.white);
            StretchFull(icon.GetComponent<RectTransform>(), margin: 12f);

            // Name label
            var nameGo  = new GameObject("NameLabel", typeof(RectTransform));
            nameGo.transform.SetParent(root.transform, false);
            var nameLabel = nameGo.AddComponent<Text>();
            SetupText(nameLabel, "", 14, TextAnchor.LowerCenter);
            var nameLabelRT = nameGo.GetComponent<RectTransform>();
            nameLabelRT.anchorMin = new Vector2(0f, 0f);
            nameLabelRT.anchorMax = new Vector2(1f, 0.3f);
            nameLabelRT.offsetMin = nameLabelRT.offsetMax = Vector2.zero;

            // Locked overlay
            var locked = CreateImageChild(root.transform, "LockedOverlay", new Color(0, 0, 0, 0.6f));
            locked.gameObject.SetActive(false);

            // Wire the LensSlot component
            var so = new SerializedObject(slot);
            so.FindProperty("backgroundImage").objectReferenceValue = bg;
            so.FindProperty("iconImage").objectReferenceValue       = icon;
            so.FindProperty("lensNameLabel").objectReferenceValue   = nameLabel;
            so.FindProperty("lockedOverlay").objectReferenceValue   = locked.gameObject;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Size: 100×100 slots
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(100f, 100f);

            SavePrefab(root, "Assets/Prefabs/UI/LensSlotPrefab.prefab");
            Object.DestroyImmediate(root);
        }

        // ── PanelRenderer prefab ──────────────────────────────────────────────────

        private static void BuildPanelRendererPrefab()
        {
            var root = new GameObject("PanelRenderer", typeof(RectTransform));
            root.AddComponent<CanvasGroup>();   // required by [RequireComponent]
            var pr = root.AddComponent<PanelRenderer>();

            // Panel artwork image (full-stretch)
            var imgGo  = new GameObject("PanelImage", typeof(RectTransform));
            imgGo.transform.SetParent(root.transform, false);
            var panelImg = imgGo.AddComponent<Image>();
            panelImg.color = Color.white;
            StretchFull(imgGo.GetComponent<RectTransform>());

            // Corruption mask (full-stretch, starts hidden)
            var maskGo  = CreateImageChild(root.transform, "CorruptionMask",
                new Color(0.75f, 0.08f, 0.08f, 0.50f));
            maskGo.gameObject.SetActive(false);

            // Caption label (bottom third)
            var captionGo = new GameObject("CaptionLabel", typeof(RectTransform));
            captionGo.transform.SetParent(root.transform, false);
            var captionTxt = captionGo.AddComponent<Text>();
            SetupText(captionTxt, "", 14, TextAnchor.LowerLeft, fontStyle: FontStyle.Bold);
            captionTxt.color = Color.white;
            var capRT = captionGo.GetComponent<RectTransform>();
            capRT.anchorMin = new Vector2(0.02f, 0.02f);
            capRT.anchorMax = new Vector2(0.98f, 0.28f);
            capRT.offsetMin = capRT.offsetMax = Vector2.zero;

            // Gutter text label (below image, outside panel frame)
            var gutterGo = new GameObject("GutterLabel", typeof(RectTransform));
            gutterGo.transform.SetParent(root.transform, false);
            var gutterTxt = gutterGo.AddComponent<Text>();
            SetupText(gutterTxt, "", 11, TextAnchor.MiddleCenter, fontStyle: FontStyle.Italic);
            gutterTxt.color = new Color(0.6f, 0.6f, 0.6f, 1f);
            var gutRT = gutterGo.GetComponent<RectTransform>();
            gutRT.anchorMin = new Vector2(0f, -0.10f);
            gutRT.anchorMax = new Vector2(1f,  0f);
            gutRT.offsetMin = gutRT.offsetMax = Vector2.zero;

            // Wire PanelRenderer
            var so = new SerializedObject(pr);
            so.FindProperty("panelImage").objectReferenceValue       = panelImg;
            so.FindProperty("corruptionMask").objectReferenceValue   = maskGo;
            so.FindProperty("captionLabel").objectReferenceValue     = captionTxt;
            so.FindProperty("gutterLabel").objectReferenceValue      = gutterTxt;
            so.ApplyModifiedPropertiesWithoutUndo();

            // Standard panel size (phone width minus margins, variable height)
            var rt = root.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1130f, 400f);

            SavePrefab(root, "Assets/Prefabs/PanelRenderer.prefab");
            Object.DestroyImmediate(root);
        }

        // ── PageView prefab ───────────────────────────────────────────────────────

        private static void BuildPageViewPrefab()
        {
            var panelRendererPrefab =
                AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PanelRenderer.prefab");

            var root = new GameObject("PageView", typeof(RectTransform));
            var pvc  = root.AddComponent<PageViewController>();

            // PanelContainer — vertical stack
            var containerGo  = new GameObject("PanelContainer", typeof(RectTransform));
            containerGo.transform.SetParent(root.transform, false);
            var vlg = containerGo.AddComponent<VerticalLayoutGroup>();
            vlg.spacing            = 16f;
            vlg.childAlignment     = TextAnchor.UpperCenter;
            vlg.childForceExpandWidth  = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(20, 20, 20, 20);

            containerGo.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            StretchFull(containerGo.GetComponent<RectTransform>());

            // Wire PageViewController
            var so = new SerializedObject(pvc);
            so.FindProperty("panelContainer").objectReferenceValue       = containerGo.transform;
            so.FindProperty("panelRendererPrefab").objectReferenceValue  = panelRendererPrefab;
            so.ApplyModifiedPropertiesWithoutUndo();

            StretchFull(root.GetComponent<RectTransform>());

            SavePrefab(root, "Assets/Prefabs/PageView.prefab");
            Object.DestroyImmediate(root);
        }

        // ── KeyEntryWidget prefab ─────────────────────────────────────────────────

        private static void BuildKeyEntryWidgetPrefab()
        {
            var root = new GameObject("KeyEntryWidget", typeof(RectTransform));
            var kew  = root.AddComponent<KeyEntryWidget>();
            var hlg  = root.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing                = 10f;
            hlg.childForceExpandHeight = true;
            hlg.padding                = new RectOffset(8, 8, 6, 6);

            root.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            // Icon placeholder
            var iconGo  = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(root.transform, false);
            var icon = iconGo.AddComponent<Image>();
            icon.color = new Color(0.9f, 0.8f, 0.3f, 1f);
            iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 40f);
            var iconLE = iconGo.AddComponent<LayoutElement>();
            iconLE.preferredWidth  = 40f;
            iconLE.preferredHeight = 40f;

            // Name + description stacked
            var textStack = new GameObject("TextStack", typeof(RectTransform));
            textStack.transform.SetParent(root.transform, false);
            var vlg = textStack.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 2f;
            vlg.childForceExpandWidth = true;
            textStack.AddComponent<ContentSizeFitter>().verticalFit =
                ContentSizeFitter.FitMode.PreferredSize;

            var nameGo   = new GameObject("KeyName", typeof(RectTransform));
            nameGo.transform.SetParent(textStack.transform, false);
            var nameTxt  = nameGo.AddComponent<Text>();
            SetupText(nameTxt, "KEY NAME", 14, TextAnchor.MiddleLeft, fontStyle: FontStyle.Bold);

            var descGo  = new GameObject("Description", typeof(RectTransform));
            descGo.transform.SetParent(textStack.transform, false);
            var descTxt = descGo.AddComponent<Text>();
            SetupText(descTxt, "Description.", 12, TextAnchor.UpperLeft);
            descTxt.color = new Color(0.75f, 0.75f, 0.75f, 1f);

            // Wire
            var so = new SerializedObject(kew);
            so.FindProperty("keyNameLabel").objectReferenceValue        = nameTxt;
            so.FindProperty("keyDescriptionLabel").objectReferenceValue = descTxt;
            so.FindProperty("keyIcon").objectReferenceValue             = icon;
            so.ApplyModifiedPropertiesWithoutUndo();

            root.GetComponent<RectTransform>().sizeDelta = new Vector2(400f, 52f);

            SavePrefab(root, "Assets/Prefabs/UI/KeyEntryWidget.prefab");
            Object.DestroyImmediate(root);
        }

        // ── KeyBadgePrefab ────────────────────────────────────────────────────────

        private static void BuildKeyBadgePrefab()
        {
            var root = new GameObject("KeyBadge", typeof(RectTransform));
            root.AddComponent<Image>().color = new Color(0.9f, 0.8f, 0.3f, 0.22f);

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(root.transform, false);
            var txt = labelGo.AddComponent<Text>();
            SetupText(txt, "KEY", 13, TextAnchor.MiddleCenter, fontStyle: FontStyle.Bold);
            StretchFull(labelGo.GetComponent<RectTransform>(), margin: 4f);

            root.GetComponent<RectTransform>().sizeDelta = new Vector2(120f, 36f);

            SavePrefab(root, "Assets/Prefabs/UI/KeyBadgePrefab.prefab");
            Object.DestroyImmediate(root);
        }

        // =====================================================================
        // SCENES
        // =====================================================================

        // ── Title scene ───────────────────────────────────────────────────────────

        private static void BuildTitleScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Persistent singletons ─────────────────────────────────────────────
            var gmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/GameManagerRoot.prefab");
            var amPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/AudioManagerPrefab.prefab");

            GameObject gmGo = gmPrefab != null
                ? (GameObject)PrefabUtility.InstantiatePrefab(gmPrefab)
                : CreateSingletons();

            if (amPrefab != null)
                PrefabUtility.InstantiatePrefab(amPrefab);

            // ── Camera ────────────────────────────────────────────────────────────
            var camGo = new GameObject("Main Camera");
            var cam   = camGo.AddComponent<Camera>();
            cam.clearFlags       = CameraClearFlags.SolidColor;
            cam.backgroundColor  = new Color(0.03f, 0.02f, 0.05f, 1f);
            cam.orthographic     = true;
            camGo.AddComponent<AudioListener>();
            camGo.tag = "MainCamera";

            // ── Event System ──────────────────────────────────────────────────────
            CreateEventSystem();

            // ── Canvas ────────────────────────────────────────────────────────────
            var canvas = CreateCanvas("TitleCanvas", sortOrder: 0);

            // Full-screen black background
            var bg = CreateImageChild(canvas.transform, "Background",
                new Color(0.03f, 0.02f, 0.05f, 1f));
            StretchFull(bg.GetComponent<RectTransform>());

            // Cover image (full-screen, sits above background — white placeholder)
            var coverGo = new GameObject("Cover", typeof(RectTransform));
            coverGo.transform.SetParent(canvas.transform, false);
            var coverImg = coverGo.AddComponent<Image>();
            coverImg.color = new Color(1f, 1f, 1f, 0f);  // hidden until controller fades in
            StretchFull(coverGo.GetComponent<RectTransform>());
            var coverGroup = coverGo.AddComponent<CanvasGroup>();

            // ── Disclaimer panel ──────────────────────────────────────────────────
            var discPanel  = new GameObject("DisclaimerPanel", typeof(RectTransform));
            discPanel.transform.SetParent(canvas.transform, false);
            var discRT = discPanel.GetComponent<RectTransform>();
            discRT.anchorMin  = new Vector2(0.05f, 0.15f);
            discRT.anchorMax  = new Vector2(0.95f, 0.85f);
            discRT.offsetMin  = discRT.offsetMax = Vector2.zero;
            var discGroup = discPanel.AddComponent<CanvasGroup>();

            var discBg = discPanel.AddComponent<Image>();
            discBg.color = new Color(0.04f, 0.03f, 0.07f, 0.95f);

            var discBody = new GameObject("DisclaimerBody", typeof(RectTransform));
            discBody.transform.SetParent(discPanel.transform, false);
            var discTxt = discBody.AddComponent<Text>();
            SetupText(discTxt, "", 16, TextAnchor.UpperLeft);
            discTxt.color = new Color(0.88f, 0.86f, 0.82f, 1f);
            var discBodyRT = discBody.GetComponent<RectTransform>();
            discBodyRT.anchorMin = new Vector2(0.05f, 0.20f);
            discBodyRT.anchorMax = new Vector2(0.95f, 0.90f);
            discBodyRT.offsetMin = discBodyRT.offsetMax = Vector2.zero;

            var ackBtn = CreateButton(discPanel.transform, "AcknowledgeButton",
                "I UNDERSTAND", new Vector2(200f, 48f),
                new Vector2(0.5f, 0.5f), new Vector2(0, 0.07f));

            // ── Start button (hidden until disclaimer acknowledged) ────────────────
            var startGroup = new GameObject("StartButtonGroup", typeof(RectTransform));
            startGroup.transform.SetParent(canvas.transform, false);
            var startGroupCG = startGroup.AddComponent<CanvasGroup>();
            startGroupCG.alpha           = 0f;
            startGroupCG.interactable    = false;
            startGroupCG.blocksRaycasts  = false;
            var startGroupRT = startGroup.GetComponent<RectTransform>();
            startGroupRT.anchorMin = new Vector2(0.3f, 0.12f);
            startGroupRT.anchorMax = new Vector2(0.7f, 0.20f);
            startGroupRT.offsetMin = startGroupRT.offsetMax = Vector2.zero;

            var startBtn = CreateButton(startGroup.transform, "StartButton",
                "BEGIN", new Vector2(260f, 56f),
                new Vector2(0.5f, 0.5f), Vector2.zero);

            // ── TitleScreenController ─────────────────────────────────────────────
            var ctrlGo = new GameObject("TitleScreenController");
            var ctrl   = ctrlGo.AddComponent<TitleScreenController>();
            var ctrlSO = new SerializedObject(ctrl);
            ctrlSO.FindProperty("coverImage").objectReferenceValue         = coverImg;
            ctrlSO.FindProperty("coverGroup").objectReferenceValue         = coverGroup;
            ctrlSO.FindProperty("disclaimerPanel").objectReferenceValue    = discGroup;
            ctrlSO.FindProperty("acknowledgeButton").objectReferenceValue  = ackBtn;
            ctrlSO.FindProperty("disclaimerBodyText").objectReferenceValue = discTxt;
            ctrlSO.FindProperty("startButton").objectReferenceValue        = startBtn;
            ctrlSO.FindProperty("startButtonGroup").objectReferenceValue   = startGroupCG;
            ctrlSO.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(scene, "Assets/Scenes/Title.unity");
        }

        // ── Frame2100 scene ───────────────────────────────────────────────────────

        private static void BuildFrame2100Scene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.08f, 0.07f, 0.10f, 1f));
            CreateEventSystem();

            var canvas = CreateCanvas("Frame2100Canvas", 0);

            // SceneGroup wraps all content so we can fade the whole scene in
            var sceneGroupGo = new GameObject("SceneGroup", typeof(RectTransform));
            sceneGroupGo.transform.SetParent(canvas.transform, false);
            var sceneGroup = sceneGroupGo.AddComponent<CanvasGroup>();
            sceneGroup.alpha = 0f;
            StretchFull(sceneGroupGo.GetComponent<RectTransform>());

            // Background (ink black)
            var bgImg = CreateImageChild(sceneGroupGo.transform, "Background",
                new Color(0.08f, 0.07f, 0.10f, 1f));
            StretchFull(bgImg.GetComponent<RectTransform>());

            // Comic cover — the one colorful object; centered, palm-sized
            var comicGo = new GameObject("ComicCover", typeof(RectTransform));
            comicGo.transform.SetParent(sceneGroupGo.transform, false);
            var comicImg = comicGo.AddComponent<Image>();
            comicImg.color = Color.white;
            var comicBtn = comicGo.AddComponent<Button>();
            var comicRT  = comicGo.GetComponent<RectTransform>();
            comicRT.anchorMin  = new Vector2(0.25f, 0.30f);
            comicRT.anchorMax  = new Vector2(0.75f, 0.70f);
            comicRT.offsetMin  = comicRT.offsetMax = Vector2.zero;

            // Caption label
            var captionGo = new GameObject("Caption", typeof(RectTransform));
            captionGo.transform.SetParent(sceneGroupGo.transform, false);
            var captionTxt = captionGo.AddComponent<Text>();
            SetupText(captionTxt, "", 18, TextAnchor.UpperCenter);
            captionTxt.color = new Color(0.80f, 0.78f, 0.74f, 1f);
            var capRT = captionGo.GetComponent<RectTransform>();
            capRT.anchorMin = new Vector2(0.05f, 0.74f);
            capRT.anchorMax = new Vector2(0.95f, 0.90f);
            capRT.offsetMin = capRT.offsetMax = Vector2.zero;

            // Classification readout (epilogue only — hidden by default)
            var classGo = new GameObject("ClassificationReadout", typeof(RectTransform));
            classGo.transform.SetParent(sceneGroupGo.transform, false);
            var classTxt = classGo.AddComponent<Text>();
            SetupText(classTxt, "", 16, TextAnchor.MiddleCenter, fontStyle: FontStyle.Bold);
            classTxt.color = new Color(0.90f, 0.25f, 0.25f, 1f);
            var classRT = classGo.GetComponent<RectTransform>();
            classRT.anchorMin = new Vector2(0.1f, 0.20f);
            classRT.anchorMax = new Vector2(0.9f, 0.30f);
            classRT.offsetMin = classRT.offsetMax = Vector2.zero;
            classGo.SetActive(false);

            // Archive window light (epilogue only — hidden glow)
            var lightGo  = new GameObject("ArchiveWindowLight", typeof(RectTransform));
            lightGo.transform.SetParent(sceneGroupGo.transform, false);
            var lightImg = lightGo.AddComponent<Image>();
            lightImg.color = new Color(0.9f, 0.85f, 0.6f, 0f);
            StretchFull(lightGo.GetComponent<RectTransform>());
            lightGo.SetActive(false);

            // Return to title button (epilogue only)
            var returnBtn = CreateButton(sceneGroupGo.transform, "ReturnToTitleButton",
                "RETURN TO ARCHIVE", new Vector2(280f, 52f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -0.30f));
            returnBtn.gameObject.SetActive(false);

            // Frame2100Controller
            var ctrlGo = new GameObject("Frame2100Controller");
            var ctrl   = ctrlGo.AddComponent<Frame2100Controller>();
            var so     = new SerializedObject(ctrl);
            so.FindProperty("sceneGroup").objectReferenceValue          = sceneGroup;
            so.FindProperty("comicCover").objectReferenceValue          = comicImg;
            so.FindProperty("tapComic").objectReferenceValue            = comicBtn;
            so.FindProperty("captionLabel").objectReferenceValue        = captionTxt;
            so.FindProperty("classificationReadout").objectReferenceValue = classGo;
            so.FindProperty("classificationText").objectReferenceValue  = classTxt;
            so.FindProperty("archiveWindowLight").objectReferenceValue  = lightGo;
            so.FindProperty("returnToTitleButton").objectReferenceValue = returnBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(scene, "Assets/Scenes/Frame2100.unity");
        }

        // ── ComicReader scene ─────────────────────────────────────────────────────

        private static void BuildComicReaderScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.03f, 0.02f, 0.05f, 1f));
            CreateEventSystem();

            // Load required prefabs
            var pageViewPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/PageView.prefab");
            var keyEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/UI/KeyEntryWidget.prefab");
            var lensSlotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/UI/LensSlotPrefab.prefab");
            var lensDefMythic  = AssetDatabase.LoadAssetAtPath<LensDefinition>(
                "Assets/ScriptableObjects/Lenses/Lens_Mythic.asset");

            // ── Main page canvas (sort 0) ──────────────────────────────────────────
            var pageCanvas = CreateCanvas("PageCanvas", 0);
            var pageScroll = new GameObject("ScrollView", typeof(RectTransform));
            pageScroll.transform.SetParent(pageCanvas.transform, false);
            pageScroll.AddComponent<ScrollRect>().horizontal = false;
            StretchFull(pageScroll.GetComponent<RectTransform>());

            var vpGo = new GameObject("Viewport", typeof(RectTransform));
            vpGo.transform.SetParent(pageScroll.transform, false);
            vpGo.AddComponent<Image>();
            vpGo.AddComponent<Mask>().showMaskGraphic = false;
            StretchFull(vpGo.GetComponent<RectTransform>());
            pageScroll.GetComponent<ScrollRect>().viewport = vpGo.GetComponent<RectTransform>();

            var pageContainer = new GameObject("PageContainer", typeof(RectTransform));
            pageContainer.transform.SetParent(vpGo.transform, false);
            StretchFull(pageContainer.GetComponent<RectTransform>());
            pageScroll.GetComponent<ScrollRect>().content = pageContainer.GetComponent<RectTransform>();

            // ── Transition overlay (sort 10) ──────────────────────────────────────
            var transCanvas = CreateCanvas("TransitionCanvas", 10);
            var overlayGo   = new GameObject("Overlay", typeof(RectTransform));
            overlayGo.transform.SetParent(transCanvas.transform, false);
            var overlayImg   = overlayGo.AddComponent<Image>();
            overlayImg.color = new Color(0.05f, 0.03f, 0.08f, 0f);
            StretchFull(overlayGo.GetComponent<RectTransform>());
            var overlayGroup = overlayGo.AddComponent<CanvasGroup>();
            overlayGroup.alpha           = 0f;
            overlayGroup.blocksRaycasts  = false;

            // TransitionController
            var transCtrlGo = new GameObject("TransitionController");
            var transCtrl   = transCtrlGo.AddComponent<TransitionController>();
            var transSO     = new SerializedObject(transCtrl);
            transSO.FindProperty("overlay").objectReferenceValue      = overlayGroup;
            transSO.FindProperty("overlayImage").objectReferenceValue = overlayImg;
            transSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Lens selector overlay (sort 20) ───────────────────────────────────
            var lensCanvas  = CreateCanvas("LensOverlayCanvas", 20);
            var selectorBG  = new GameObject("SelectorGroup", typeof(RectTransform));
            selectorBG.transform.SetParent(lensCanvas.transform, false);
            var selectorGroupCG = selectorBG.AddComponent<CanvasGroup>();
            selectorGroupCG.alpha          = 0f;
            selectorGroupCG.interactable   = false;
            selectorGroupCG.blocksRaycasts = false;

            var selectorRoot = new GameObject("SelectorRoot", typeof(RectTransform));
            selectorRoot.transform.SetParent(selectorBG.transform, false);
            var selectorBgImg = selectorRoot.AddComponent<Image>();
            selectorBgImg.color = new Color(0.05f, 0.03f, 0.10f, 0.88f);
            var selectorRT = selectorRoot.GetComponent<RectTransform>();
            selectorRT.anchorMin = selectorRT.anchorMax = new Vector2(0.5f, 0.5f);
            selectorRT.sizeDelta = new Vector2(320f, 320f);

            // Five LensSlot children arranged in a pentagon
            var lensTypes = new[] { LensType.Mythic, LensType.Technologic, LensType.Symbolic,
                                    LensType.Political, LensType.Spiritual };
            var lensSlots = new LensSlot[5];
            float radius  = 110f;
            for (int i = 0; i < 5; i++)
            {
                float angle = Mathf.PI * 2f * i / 5f - Mathf.PI / 2f;
                GameObject slotGo;
                if (lensSlotPrefab != null)
                    slotGo = (GameObject)PrefabUtility.InstantiatePrefab(lensSlotPrefab, selectorRoot.transform);
                else
                {
                    slotGo = new GameObject($"Slot_{lensTypes[i]}", typeof(RectTransform));
                    slotGo.transform.SetParent(selectorRoot.transform, false);
                    slotGo.AddComponent<CanvasGroup>();
                    slotGo.AddComponent<LensSlot>();
                }
                var slotRT = slotGo.GetComponent<RectTransform>();
                slotRT.anchorMin = slotRT.anchorMax = new Vector2(0.5f, 0.5f);
                slotRT.anchoredPosition = new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius);
                slotRT.sizeDelta = new Vector2(100f, 100f);
                slotGo.name = $"Slot_{lensTypes[i]}";
                lensSlots[i] = slotGo.GetComponent<LensSlot>();
            }

            // LensSelectorUI
            var lensUIGo = new GameObject("LensSelectorUI");
            var lensUI   = lensUIGo.AddComponent<LensSelectorUI>();
            var lensSO   = new SerializedObject(lensUI);
            lensSO.FindProperty("selectorGroup").objectReferenceValue = selectorGroupCG;
            lensSO.FindProperty("selectorRoot").objectReferenceValue  =
                selectorRoot.GetComponent<RectTransform>();
            var slotsProp = lensSO.FindProperty("slots");
            slotsProp.arraySize = 5;
            for (int i = 0; i < 5; i++)
                slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = lensSlots[i];
            lensSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Archive notebook (sort 30) ────────────────────────────────────────
            var notebookCanvas = CreateCanvas("NotebookCanvas", 30);
            var notebookGroupGo = new GameObject("NotebookGroup", typeof(RectTransform));
            notebookGroupGo.transform.SetParent(notebookCanvas.transform, false);
            StretchFull(notebookGroupGo.GetComponent<RectTransform>());
            var notebookGroupCG = notebookGroupGo.AddComponent<CanvasGroup>();
            notebookGroupCG.alpha          = 0f;
            notebookGroupCG.interactable   = false;
            notebookGroupCG.blocksRaycasts = false;

            var notebookBg = notebookGroupGo.AddComponent<Image>();
            notebookBg.color = new Color(0.06f, 0.04f, 0.10f, 0.96f);

            var keyEntryContainer = new GameObject("KeyEntryContainer", typeof(RectTransform));
            keyEntryContainer.transform.SetParent(notebookGroupGo.transform, false);
            var kecRT = keyEntryContainer.GetComponent<RectTransform>();
            kecRT.anchorMin = new Vector2(0.02f, 0.50f);
            kecRT.anchorMax = new Vector2(0.98f, 0.96f);
            kecRT.offsetMin = kecRT.offsetMax = Vector2.zero;
            var kecVLG = keyEntryContainer.AddComponent<VerticalLayoutGroup>();
            kecVLG.spacing = 6f;
            kecVLG.childForceExpandWidth  = true;
            kecVLG.childForceExpandHeight = false;

            var detectiveBoardRoot = new GameObject("DetectiveBoardRoot", typeof(RectTransform));
            detectiveBoardRoot.transform.SetParent(notebookGroupGo.transform, false);
            var dbRT = detectiveBoardRoot.GetComponent<RectTransform>();
            dbRT.anchorMin = new Vector2(0.02f, 0.04f);
            dbRT.anchorMax = new Vector2(0.98f, 0.48f);
            dbRT.offsetMin = dbRT.offsetMax = Vector2.zero;

            var pyramidGo  = new GameObject("PyramidSilhouette", typeof(RectTransform));
            pyramidGo.transform.SetParent(detectiveBoardRoot.transform, false);
            var pyramidImg = pyramidGo.AddComponent<Image>();
            pyramidImg.color = new Color(1f, 1f, 1f, 0f);
            StretchFull(pyramidGo.GetComponent<RectTransform>());

            // ArchiveNotebook component
            var notebookCtrlGo = new GameObject("ArchiveNotebook");
            var notebook = notebookCtrlGo.AddComponent<ArchiveNotebook>();
            var nbSO     = new SerializedObject(notebook);
            nbSO.FindProperty("keyEntryPrefab").objectReferenceValue      =
                keyEntryPrefab != null ? keyEntryPrefab.GetComponent<KeyEntryWidget>() : null;
            nbSO.FindProperty("keyEntryContainer").objectReferenceValue   = keyEntryContainer.transform;
            nbSO.FindProperty("detectiveBoardRoot").objectReferenceValue  = detectiveBoardRoot.transform;
            nbSO.FindProperty("pyramidSilhouette").objectReferenceValue   = pyramidImg;
            nbSO.FindProperty("notebookGroup").objectReferenceValue       = notebookGroupCG;
            nbSO.ApplyModifiedPropertiesWithoutUndo();

            // ── Constellation + Ark inventory canvases (sort 25) ──────────────────
            var constCanvas  = CreateCanvas("ConstellationCanvas", 25);
            var constController = new GameObject("ConstellationMapUI");
            constController.AddComponent<ConstellationMapUI>();

            var arkCanvas   = CreateCanvas("ArkInventoryCanvas", 25);
            var arkController = new GameObject("ArkInventoryUI");
            arkController.AddComponent<ArkInventoryUI>();

            // ── Micro-scene root (hidden) ─────────────────────────────────────────
            var microRoot = new GameObject("MicroSceneRoot");
            microRoot.SetActive(false);

            // ── PanelEntryController ──────────────────────────────────────────────
            var pecGo = new GameObject("PanelEntryController");
            var pec   = pecGo.AddComponent<PanelEntryController>();
            var pecSO = new SerializedObject(pec);
            pecSO.FindProperty("transitions").objectReferenceValue  = transCtrl;
            pecSO.FindProperty("microSceneRoot").objectReferenceValue = microRoot;
            pecSO.ApplyModifiedPropertiesWithoutUndo();

            // ── ComicController ───────────────────────────────────────────────────
            var ccGo = new GameObject("ComicController");
            var cc   = ccGo.AddComponent<ComicController>();
            var ccSO = new SerializedObject(cc);
            ccSO.FindProperty("pageViewPrefab").objectReferenceValue  = pageViewPrefab;
            ccSO.FindProperty("pageContainer").objectReferenceValue   = pageContainer.transform;
            ccSO.FindProperty("transitions").objectReferenceValue     = transCtrl;
            ccSO.FindProperty("lensSelector").objectReferenceValue    = lensUI;
            ccSO.FindProperty("archiveNotebook").objectReferenceValue = notebook;
            ccSO.ApplyModifiedPropertiesWithoutUndo();

            // ── ConstellationMapUI & ArkInventoryUI wiring (minimal — no SOs yet) ─
            // (Inspector assignment required after art assets are imported)

            SaveScene(scene, "Assets/Scenes/ComicReader.unity");
        }

        // ── IssueComplete scene ───────────────────────────────────────────────────

        private static void BuildIssueCompleteScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.03f, 0.02f, 0.05f, 1f));
            CreateEventSystem();

            var keyBadgePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/UI/KeyBadgePrefab.prefab");

            var canvas = CreateCanvas("IssueCompleteCanvas", 0);
            var bg     = CreateImageChild(canvas.transform, "Background",
                new Color(0.03f, 0.02f, 0.05f, 1f));
            StretchFull(bg.GetComponent<RectTransform>());

            // ── Header block ──────────────────────────────────────────────────────
            var issueNumGo = MakeText(canvas.transform, "IssueNumberLabel",
                "ISSUE 00", 22, TextAnchor.UpperCenter, FontStyle.Bold,
                new Vector2(0.1f, 0.85f), new Vector2(0.9f, 0.93f));

            var issueTitleGo = MakeText(canvas.transform, "IssueTitleLabel",
                "TITLE", 30, TextAnchor.UpperCenter, FontStyle.Bold,
                new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.86f));

            var arcLabelGo = MakeText(canvas.transform, "ArcLabel",
                "Arc", 16, TextAnchor.UpperCenter, FontStyle.Italic,
                new Vector2(0.1f, 0.72f), new Vector2(0.9f, 0.77f));

            // ── Key badge list ────────────────────────────────────────────────────
            var keyListGo = new GameObject("KeyListContainer", typeof(RectTransform));
            keyListGo.transform.SetParent(canvas.transform, false);
            var klRT = keyListGo.GetComponent<RectTransform>();
            klRT.anchorMin = new Vector2(0.08f, 0.52f);
            klRT.anchorMax = new Vector2(0.92f, 0.70f);
            klRT.offsetMin = klRT.offsetMax = Vector2.zero;
            var klHLG = keyListGo.AddComponent<HorizontalLayoutGroup>();
            klHLG.spacing                = 8f;
            klHLG.childForceExpandHeight = true;
            klHLG.childAlignment         = TextAnchor.MiddleCenter;

            // ── Lens unlock banner ────────────────────────────────────────────────
            var bannerGo  = new GameObject("LensUnlockBanner", typeof(RectTransform));
            bannerGo.transform.SetParent(canvas.transform, false);
            var bannerCG  = bannerGo.AddComponent<CanvasGroup>();
            bannerCG.alpha = 0f;
            var bannerImg = bannerGo.AddComponent<Image>();
            bannerImg.color = new Color(0.82f, 0.65f, 0.20f, 0.12f);
            var bannerRT = bannerGo.GetComponent<RectTransform>();
            bannerRT.anchorMin = new Vector2(0.05f, 0.46f);
            bannerRT.anchorMax = new Vector2(0.95f, 0.52f);
            bannerRT.offsetMin = bannerRT.offsetMax = Vector2.zero;

            var lensUnlockLabelGo = MakeText(bannerGo.transform, "LensUnlockLabel",
                "", 16, TextAnchor.MiddleCenter, FontStyle.Bold,
                Vector2.zero, Vector2.one);

            // ── Next issue preview ────────────────────────────────────────────────
            var nextTitleGo = MakeText(canvas.transform, "NextIssueTitleLabel",
                "Next:", 16, TextAnchor.MiddleCenter, FontStyle.Normal,
                new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.43f));
            nextTitleGo.color = new Color(0.70f, 0.70f, 0.70f, 1f);

            var nextArcGo = MakeText(canvas.transform, "NextIssueArcLabel",
                "", 14, TextAnchor.MiddleCenter, FontStyle.Italic,
                new Vector2(0.1f, 0.30f), new Vector2(0.9f, 0.36f));
            nextArcGo.color = new Color(0.60f, 0.60f, 0.60f, 1f);

            // ── Buttons ───────────────────────────────────────────────────────────
            var continueBtn = CreateButton(canvas.transform, "ContinueButton",
                "CONTINUE", new Vector2(240f, 56f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -0.34f));

            var notebookBtn = CreateButton(canvas.transform, "NotebookButton",
                "ARCHIVE NOTEBOOK", new Vector2(220f, 44f),
                new Vector2(0.5f, 0.5f), new Vector2(0f, -0.42f));
            notebookBtn.GetComponent<Image>().color = new Color(0.15f, 0.12f, 0.22f, 0.9f);

            // ── IssueCompleteController ───────────────────────────────────────────
            var iccGo = new GameObject("IssueCompleteController");
            var icc   = iccGo.AddComponent<IssueCompleteController>();
            var so    = new SerializedObject(icc);
            so.FindProperty("issueNumberLabel").objectReferenceValue  = issueNumGo;
            so.FindProperty("issueTitleLabel").objectReferenceValue   = issueTitleGo;
            so.FindProperty("arcLabel").objectReferenceValue          = arcLabelGo;
            so.FindProperty("keyListContainer").objectReferenceValue  = keyListGo.transform;
            so.FindProperty("keyBadgePrefab").objectReferenceValue    = keyBadgePrefab;
            so.FindProperty("lensUnlockBanner").objectReferenceValue  = bannerCG;
            so.FindProperty("lensUnlockLabel").objectReferenceValue   = lensUnlockLabelGo;
            so.FindProperty("nextIssueTitleLabel").objectReferenceValue = nextTitleGo;
            so.FindProperty("nextIssueArcLabel").objectReferenceValue   = nextArcGo;
            so.FindProperty("continueButton").objectReferenceValue    = continueBtn;
            so.FindProperty("notebookButton").objectReferenceValue    = notebookBtn;
            so.ApplyModifiedPropertiesWithoutUndo();

            SaveScene(scene, "Assets/Scenes/IssueComplete.unity");
        }

        // =====================================================================
        // UI HELPERS
        // =====================================================================

        private static Canvas CreateCanvas(string name, int sortOrder)
        {
            var go     = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = RefRes;
            scaler.matchWidthOrHeight  = 1f;   // height-based (portrait iPhone)

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void CreateCamera(Color bg)
        {
            var go  = new GameObject("Main Camera");
            var cam = go.AddComponent<Camera>();
            cam.clearFlags      = CameraClearFlags.SolidColor;
            cam.backgroundColor = bg;
            cam.orthographic    = true;
            go.AddComponent<AudioListener>();
            go.tag = "MainCamera";
        }

        private static void CreateEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        private static GameObject CreateSingletons()
        {
            var go = new GameObject("GameManagerRoot");
            go.AddComponent<GameManager>();
            go.AddComponent<TouchInputManager>();
            go.AddComponent<NarrativeState>();
            go.AddComponent<LensSystem>();
            return go;
        }

        private static Image CreateImageChild(Transform parent, string name, Color color)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private static Button CreateButton(Transform parent, string name, string label,
                                           Vector2 sizeDelta,
                                           Vector2 anchorPoint, Vector2 anchorOffset)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt  = go.GetComponent<RectTransform>();
            rt.anchorMin        = anchorPoint + anchorOffset - Vector2.one * 0.001f;
            rt.anchorMax        = anchorPoint + anchorOffset + Vector2.one * 0.001f;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta        = sizeDelta;

            var img = go.AddComponent<Image>();
            img.color = new Color(0.15f, 0.12f, 0.22f, 0.92f);
            var btn = go.AddComponent<Button>();

            var txtGo = new GameObject("Label", typeof(RectTransform));
            txtGo.transform.SetParent(go.transform, false);
            var txt = txtGo.AddComponent<Text>();
            SetupText(txt, label, 16, TextAnchor.MiddleCenter, fontStyle: FontStyle.Bold);
            StretchFull(txtGo.GetComponent<RectTransform>());

            return btn;
        }

        /// <summary>Creates a Text child and returns the Text component.</summary>
        private static Text MakeText(Transform parent, string name, string content,
                                     int fontSize, TextAnchor alignment, FontStyle style,
                                     Vector2 anchorMin, Vector2 anchorMax)
        {
            var go  = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var txt = go.AddComponent<Text>();
            SetupText(txt, content, fontSize, alignment, fontStyle: style);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return txt;
        }

        private static void SetupText(Text txt, string content, int fontSize,
                                      TextAnchor alignment,
                                      FontStyle fontStyle = FontStyle.Normal)
        {
            txt.text       = content;
            txt.font       = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize   = fontSize;
            txt.alignment  = alignment;
            txt.fontStyle  = fontStyle;
            txt.color      = new Color(0.90f, 0.88f, 0.84f, 1f);
            txt.supportRichText = true;
        }

        private static void StretchFull(RectTransform rt, float margin = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2( margin,  margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        // ── Asset I/O ─────────────────────────────────────────────────────────────

        private static void SavePrefab(GameObject go, string path)
        {
            PrefabUtility.SaveAsPrefabAsset(go, path);
        }

        private static void SaveScene(Scene scene, string path)
        {
            EditorSceneManager.SaveScene(scene, path);
            Debug.Log($"[Archive] Saved scene → {path}");
        }
    }
}

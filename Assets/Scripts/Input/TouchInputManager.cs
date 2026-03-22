using System;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Centralises all gesture recognition for the entire comic reader.
    /// Fires typed events consumed by panel, page, and puzzle controllers.
    /// Persists across scenes — attach to the GameManager prefab.
    /// </summary>
    public class TouchInputManager : MonoBehaviour
    {
        public static TouchInputManager Instance { get; private set; }

        // ── Thresholds ────────────────────────────────────────────────────────────
        [Header("Tap")]
        [SerializeField] private float tapMaxDuration = 0.2f;
        [SerializeField] private float doubleTapMaxInterval = 0.3f;

        [Header("Long Press")]
        [SerializeField] private float longPressDuration = 0.5f;
        [SerializeField] private float longPressCancelMoveThreshold = 12f;

        [Header("Swipe")]
        [SerializeField] private float swipeMinDistance = 60f;

        [Header("Pinch")]
        [SerializeField] private float pinchMinDelta = 0.05f;

        // ── Events ────────────────────────────────────────────────────────────────
        public event Action<Vector2> OnTap;
        public event Action<Vector2> OnDoubleTap;
        /// <summary>Fired every frame while the player is holding; position + elapsed seconds.</summary>
        public event Action<Vector2, float> OnLongPressUpdate;
        public event Action<Vector2> OnLongPressComplete;
        public event Action<Vector2> OnLongPressCancel;

        public event Action<Vector2> OnSwipeLeft;
        public event Action<Vector2> OnSwipeRight;
        public event Action<Vector2> OnSwipeUp;
        public event Action<Vector2> OnSwipeDown;

        /// <summary>Fired each frame during a two-finger pinch; delta is normalised to screen height.</summary>
        public event Action<float> OnPinch;
        /// <summary>Fired each frame during a two-finger drag; midpoint and frame delta.</summary>
        public event Action<Vector2, Vector2> OnTwoFingerDrag;

        public event Action<Vector2> OnDrag;
        public event Action<Vector2> OnDragEnd;

        // ── Internal state ────────────────────────────────────────────────────────
        private float _touchStartTime;
        private Vector2 _touchStartPosition;
        private float _lastTapTime;
        private bool _isLongPressing;
        private float _longPressElapsed;
        private bool _isDragging;
        private float _previousPinchDistance;

        // ── Public read-only state (needed by puzzles) ────────────────────────────
        /// <summary>Last known touch/mouse position in screen pixels.</summary>
        public Vector2 LastPosition { get; private set; }

        /// <summary>
        /// Angle of the two-finger axis from horizontal, in degrees.
        /// Populated each frame that OnTwoFingerDrag fires.
        /// </summary>
        public float TwoFingerTiltAngle { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleMouseInput();
#else
            HandleTouchInput();
#endif
        }

        // ── Touch input ───────────────────────────────────────────────────────────

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
                ProcessSingleTouch(Input.GetTouch(0).phase, Input.GetTouch(0).position);
            else if (Input.touchCount == 2)
                ProcessPinchAndTwoFingerDrag(Input.GetTouch(0), Input.GetTouch(1));
        }

        // ── Mouse fallback (editor) ───────────────────────────────────────────────

        private void HandleMouseInput()
        {
            Vector2 pos = Input.mousePosition;
            if (Input.GetMouseButtonDown(0))       ProcessSingleTouch(TouchPhase.Began, pos);
            else if (Input.GetMouseButton(0))      ProcessSingleTouch(TouchPhase.Moved, pos);
            else if (Input.GetMouseButtonUp(0))    ProcessSingleTouch(TouchPhase.Ended, pos);
        }

        // ── Single-touch state machine ────────────────────────────────────────────

        private void ProcessSingleTouch(TouchPhase phase, Vector2 position)
        {
            switch (phase)
            {
                case TouchPhase.Began:
                    LastPosition = position;
                    _touchStartTime = Time.time;
                    _touchStartPosition = position;
                    _isLongPressing = false;
                    _longPressElapsed = 0f;
                    _isDragging = false;
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    float elapsed = Time.time - _touchStartTime;
                    float moved = Vector2.Distance(position, _touchStartPosition);

                    if (!_isDragging && moved > longPressCancelMoveThreshold)
                    {
                        if (_isLongPressing) OnLongPressCancel?.Invoke(position);
                        _isDragging = true;
                        _isLongPressing = false;
                    }

                    if (_isDragging)
                    {
                        OnDrag?.Invoke(position);
                    }
                    else
                    {
                        _longPressElapsed = elapsed;
                        if (!_isLongPressing && elapsed >= longPressDuration)
                            _isLongPressing = true;
                        if (_isLongPressing)
                            OnLongPressUpdate?.Invoke(position, elapsed);
                    }
                    break;

                case TouchPhase.Ended:
                    LastPosition = position;
                    if (_isDragging)
                    {
                        OnDragEnd?.Invoke(position);
                        Vector2 swipeDelta = position - _touchStartPosition;
                        if (swipeDelta.magnitude >= swipeMinDistance)
                            FireSwipe(swipeDelta, position);
                    }
                    else if (_isLongPressing)
                    {
                        OnLongPressComplete?.Invoke(position);
                    }
                    else
                    {
                        float duration = Time.time - _touchStartTime;
                        if (duration <= tapMaxDuration)
                        {
                            if ((Time.time - _lastTapTime) <= doubleTapMaxInterval)
                                OnDoubleTap?.Invoke(position);
                            else
                                OnTap?.Invoke(position);
                            _lastTapTime = Time.time;
                        }
                    }
                    _isLongPressing = false;
                    _isDragging = false;
                    break;

                case TouchPhase.Canceled:
                    if (_isLongPressing) OnLongPressCancel?.Invoke(position);
                    _isLongPressing = false;
                    _isDragging = false;
                    break;
            }
        }

        // ── Two-finger gestures ───────────────────────────────────────────────────

        private void ProcessPinchAndTwoFingerDrag(Touch t0, Touch t1)
        {
            float currentDistance = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                _previousPinchDistance = currentDistance;
                return;
            }

            // Pinch delta normalised so value is independent of screen resolution
            float delta = (currentDistance - _previousPinchDistance) / Screen.height;
            if (Mathf.Abs(delta) >= pinchMinDelta)
                OnPinch?.Invoke(delta);

            _previousPinchDistance = currentDistance;

            // Two-finger drag (used by lens radial selector + CarryConstraint)
            Vector2 midpoint = (t0.position + t1.position) * 0.5f;
            Vector2 frameDelta = (t0.deltaPosition + t1.deltaPosition) * 0.5f;
            LastPosition = midpoint;

            // Tilt angle: angle of the vector between the two fingers from horizontal
            Vector2 axis = t1.position - t0.position;
            TwoFingerTiltAngle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg;

            OnTwoFingerDrag?.Invoke(midpoint, frameDelta);
        }

        // ── Swipe direction ───────────────────────────────────────────────────────

        private void FireSwipe(Vector2 delta, Vector2 endPosition)
        {
            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
            {
                if (delta.x < 0) OnSwipeLeft?.Invoke(endPosition);
                else             OnSwipeRight?.Invoke(endPosition);
            }
            else
            {
                if (delta.y < 0) OnSwipeDown?.Invoke(endPosition);
                else             OnSwipeUp?.Invoke(endPosition);
            }
        }
    }
}

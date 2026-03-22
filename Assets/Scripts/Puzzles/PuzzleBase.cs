using System;
using UnityEngine;

namespace ArchiveOfEchoes
{
    /// <summary>
    /// Abstract base for all puzzle types.
    /// Subclasses implement the specific interaction; call Complete() or Fail() to report outcome.
    /// </summary>
    public abstract class PuzzleBase : MonoBehaviour
    {
        public event Action OnPuzzleComplete;
        public event Action OnPuzzleFailed;

        protected PuzzleConfig Config;
        protected bool IsActive;

        public virtual void Initialize(PuzzleConfig config)
        {
            Config = config;
            IsActive = true;
        }

        protected void Complete()
        {
            IsActive = false;
            OnPuzzleComplete?.Invoke();
        }

        protected void Fail()
        {
            OnPuzzleFailed?.Invoke();
        }

        protected virtual void Awake() { }
    }
}

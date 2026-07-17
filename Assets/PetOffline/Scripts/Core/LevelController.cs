using UnityEngine;

namespace PetOffline
{
    public abstract class LevelController : MonoBehaviour
    {
        protected GameSession Session { get; private set; }
        protected GameUI UI => Session.UI;
        protected bool IsReady { get; private set; }

        public abstract bool CanPause { get; }

        public void Initialize(GameSession session)
        {
            Session = session;
            Begin();
            IsReady = true;
        }

        public abstract void HandleInput(PlayerInput input);

        public virtual void ContinueReport()
        {
        }

        public virtual void SubmitChoice(FinalChoice choice)
        {
        }

        protected abstract void Begin();
    }
}

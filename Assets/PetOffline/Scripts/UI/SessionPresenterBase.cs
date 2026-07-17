using PetOffline.Core;
using UnityEngine;

namespace PetOffline.UI
{
    public abstract class SessionPresenterBase : MonoBehaviour
    {
        [SerializeField] private GameSession gameSession;

        private ILevelViewModel _subscribedViewModel;

        protected IGameSessionView SessionView => gameSession;

        protected IGameCommandSink CommandSink => gameSession;

        protected ILevelViewModel ViewModel => _subscribedViewModel;

        protected virtual void OnEnable()
        {
            AddButtonListeners();
            Subscribe();
            Redraw();
        }

        protected virtual void OnDisable()
        {
            Unsubscribe();
            RemoveButtonListeners();
        }

        public void SetGameSession(GameSession value)
        {
            if (gameSession == value)
            {
                return;
            }

            bool shouldResubscribe = isActiveAndEnabled;
            if (shouldResubscribe)
            {
                Unsubscribe();
            }

            gameSession = value;
            if (shouldResubscribe)
            {
                Subscribe();
                Redraw();
            }
        }

        protected abstract void Redraw();

        protected virtual void AddButtonListeners()
        {
        }

        protected virtual void RemoveButtonListeners()
        {
        }

        protected static void SetActive(GameObject target, bool value)
        {
            if (target != null && target.activeSelf != value)
            {
                target.SetActive(value);
            }
        }

        private void Subscribe()
        {
            if (gameSession == null)
            {
                BindViewModel(null);
                return;
            }

            gameSession.Changed += HandleSessionChanged;
            BindViewModel(gameSession.CurrentViewModel);
        }

        private void Unsubscribe()
        {
            if (gameSession != null)
            {
                gameSession.Changed -= HandleSessionChanged;
            }

            BindViewModel(null);
        }

        private void HandleSessionChanged()
        {
            BindViewModel(gameSession != null ? gameSession.CurrentViewModel : null);
            Redraw();
        }

        private void BindViewModel(ILevelViewModel viewModel)
        {
            if (ReferenceEquals(_subscribedViewModel, viewModel))
            {
                return;
            }

            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.Changed -= HandleViewModelChanged;
            }

            _subscribedViewModel = viewModel;
            if (_subscribedViewModel != null)
            {
                _subscribedViewModel.Changed += HandleViewModelChanged;
            }
        }

        private void HandleViewModelChanged()
        {
            Redraw();
        }
    }
}

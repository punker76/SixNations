using System;
using System.Windows.Input;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SixNations.Desktop.Interfaces;
using SixNations.Desktop.Messages;
using SixNations.Desktop.Constants;
using SixNations.Desktop.Models;
using SixNations.Desktop.Helpers;
using System.Reflection;

namespace SixNations.Desktop.ViewModels
{
    public class MainViewModel : ViewModelBase, IShellViewModel
    {
        private readonly INavigationService _navigationService;
        private bool _isFullScreen;

        public MainViewModel(
            BusyStateManager busyStateManager,
            INavigationService navigationService, 
            MvvmDialogs.IDialogService dialogService)
        {
            Title = Assembly.GetEntryAssembly().GetName().Name;

            BusyStateManager = busyStateManager;
            _navigationService = navigationService;
            SelectedIndexManager = _navigationService;
            SelectedIndexManager.SelectedIndex = (int)HamburgerNavItemsIndex.Login;

            DialogService = dialogService;

            StoryFilterCmd = new RelayCommand(OnStoryFilter, () => CanExecuteStoryFilter);
            ClearStoryFilterCmd = new RelayCommand(OnClearStoryFilter, () => CanExecuteStoryFilter);
            FullScreenCmd = new RelayCommand(OnFullScreen, () => CanExecuteFullScreenToggle);
            ExitCmd = new RelayCommand(OnExit);
            
            MessengerInstance.Register<AuthenticatedMessage>(this, OnAuthenticated);
        }

        public event EventHandler<IIsFullScreenChangedEventArgs> IsFullScreenChanged;

        public BusyStateManager BusyStateManager { get; }

        public ISelectedIndexManager SelectedIndexManager { get; }

        public MvvmDialogs.IDialogService DialogService { get; }

        public ICommand ExitCmd { get; }

        public ICommand StoryFilterCmd { get; }

        public ICommand ClearStoryFilterCmd { get; }

        public ICommand FullScreenCmd { get; }

        public bool CanExecuteFullScreenToggle => 
            SelectedIndexManager.SelectedIndex == (int)HamburgerNavItemsIndex.Wall;

        public bool CanExecuteStoryFilter => !BusyStateManager.IsBusy && IsLoggedIn;

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set
            {
                Set(ref _isFullScreen, value);
                RaiseIsFullScreenChangedChanged();
            }
        }

        public bool IsLoggedIn => User.Current.IsLoggedIn;

        public string Title { get; }

        private void OnAuthenticated(AuthenticatedMessage m)
        {
            MessengerInstance.Send(new BusyMessage(true, this));
            User.Current.Initialise(m.Token);
            RaisePropertyChanged(nameof(IsLoggedIn));
            _navigationService.NavigateTo(
                HamburgerNavItemsIndex.Requirements.ToString());
            MessengerInstance.Send(new BusyMessage(false, this));
        }

        private void OnStoryFilter()
        {
            var vm = ServiceLocator.Current.GetInstance<FindStoryDialogViewModel>();
            DialogService.ShowDialog(this, vm);
        }

        private void OnClearStoryFilter()
        {
            MessengerInstance.Send(new StoryFilterMessage(string.Empty));
        }

        private void OnExit()
        {
            App.Current.Shutdown();
        }

        private void OnFullScreen()
        {
            IsFullScreen = true;
            var vm = ServiceLocator.Current.GetInstance<WallDialogViewModel>();
            var result = DialogService.ShowDialog(this, vm);
            IsFullScreen = false;
        }

        private void RaiseIsFullScreenChangedChanged()
        {
            var handler = IsFullScreenChanged;
            handler?.Invoke(this, new IsFullScreenChangedEventArgs(IsFullScreen));
        }
    }

    public class IsFullScreenChangedEventArgs : EventArgs, IIsFullScreenChangedEventArgs
    {
        public IsFullScreenChangedEventArgs(bool isFullScreenValue)
        {
            IsFullScreenValue = isFullScreenValue;
        }

        public bool IsFullScreenValue { get; }
    }
}
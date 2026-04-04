using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReqForge.Models;
using ReqForge.Models.DTOs;
using ReqForge.Services;

namespace ReqForge.ViewModels;

public partial class MainViewModel
{
    
    [ObservableProperty]private bool _isLoggedIn;
    [ObservableProperty]private string _currentUsername = string.Empty;
    [ObservableProperty]private string _loginUserName =  string.Empty;
    [ObservableProperty]private string _loginPassword  = string.Empty;
    [ObservableProperty]private string _authErrorMessage = string.Empty;

    [RelayCommand]
    private void Login()
    {
        AuthErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(LoginUserName) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            AuthErrorMessage = "Username and password are required";
            return;
        }
        if (!_authService.Login(LoginUserName, LoginPassword))
        {
            AuthErrorMessage = "User with this name and password doesn't exist";
            return;
        }

        IsLoggedIn = true;
        CurrentUsername = _authService.CurrentUsername ?? LoginUserName;
        ReloadCollections();
        LoginUserName = string.Empty;
        LoginPassword = string.Empty;
        AuthErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void Register()
    {
        AuthErrorMessage = string.Empty;
        if (string.IsNullOrWhiteSpace(LoginUserName) || string.IsNullOrWhiteSpace(LoginPassword))
        {
            AuthErrorMessage = "Username and password are required";
            return;
        }
        
        if (!_authService.Register(LoginUserName, LoginPassword))
        {
            AuthErrorMessage = "User already exists";
            return;
        }
        
        IsLoggedIn = true;
        CurrentUsername = _authService.CurrentUsername ?? LoginUserName;
        ReloadCollections();
        LoginUserName = string.Empty;
        LoginPassword = string.Empty;
        AuthErrorMessage = string.Empty;
    }
    
    [RelayCommand]
    private void Logout()
    {
        AuthErrorMessage = string.Empty;
        _authService.Logout();
        IsLoggedIn = false;
        CurrentUsername = string.Empty;
        ReloadCollections();
    }

    private void ReloadCollections()
    {
        if (IsLoggedIn && !string.IsNullOrEmpty(CurrentUsername))
        {
            Collections = new ObservableCollection<RequestCollection>(
                _storage.LoadAll(CurrentUsername));

            Environments.Clear();
            var envDtos = _envStorage.LoadAll(CurrentUsername);
            foreach (var dto in envDtos)
            {
                var env = new RequestEnvironment { Name = dto.Name };
                foreach (var vDto in dto.Variables)
                    env.Variables.Add(new EnvironmentVariable(vDto.Key, vDto.Value));
                Environments.Add(env);
            }

            ApplyFilter();
        }
        else
        {
            Collections = new ObservableCollection<RequestCollection>();
            Environments.Clear();
        }

        SelectedCollection = null;
        SelectedEnvironment = null;
    }
}
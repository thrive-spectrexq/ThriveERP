using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Auth;
using ThriveERP.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ThriveERP.Desktop.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly IMediator? _mediator;

    [ObservableProperty]
    private string _username = "";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = "";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public Action<string>? OnLoginSuccess { get; set; }

    public LoginViewModel()
    {
        // Parameterless constructor for designer / simple initialization
        if (App.Services != null)
        {
            _mediator = App.Services.GetService<IMediator>();
        }
    }

    public LoginViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = "";

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        IsLoading = true;

        try
        {
            if (_mediator != null)
            {
                var result = await _mediator.Send(new LoginCommand(Username, Password));
                
                // Set the current user in the singleton CurrentUserService
                var currentUserService = App.Services?.GetService<CurrentUserService>();
                currentUserService?.SetUser(
                    result.UserId, 
                    result.Username, 
                    result.RoleName, 
                    result.Permissions);

                App.CurrentRole = result.RoleName;
                OnLoginSuccess?.Invoke(result.RoleName);
            }
            else
            {
                ErrorMessage = "Application services not available. Please restart.";
            }
        }
        catch (UnauthorizedAccessException)
        {
            ErrorMessage = "Invalid username or password.";
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("crash_log.txt", ex.ToString());
            ErrorMessage = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

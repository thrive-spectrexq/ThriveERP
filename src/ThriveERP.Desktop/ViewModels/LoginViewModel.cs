using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using ThriveERP.Application.Features.Auth;
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
                
                // If we get here without exception, login was successful
                App.CurrentRole = result.RoleName;
                OnLoginSuccess?.Invoke(result.RoleName);
            }
            else
            {
                // Fallback for UI testing if MediatR isn't hooked up correctly
                await Task.Delay(1000);
                if (Username == "admin" && Password == "admin")
                {
                    App.CurrentRole = "Admin";
                    OnLoginSuccess?.Invoke("Admin");
                }
                else if (Username == "cashier" && Password == "cashier")
                {
                    App.CurrentRole = "Cashier";
                    OnLoginSuccess?.Invoke("Cashier");
                }
                else
                {
                    ErrorMessage = "Invalid username or password.";
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }
}

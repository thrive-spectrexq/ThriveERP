using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ThriveERP.Application.Features.Customers;
using ThriveERP.Application.Features.Sales;

namespace ThriveERP.Desktop.ViewModels;

public partial class CustomersViewModel : ViewModelBase
{
    private readonly IMediator _mediator = null!;

    [ObservableProperty]
    private string _title = "Customers CRM & Balances";

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _customers = new();

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _filteredCustomers = new();

    [ObservableProperty]
    private CustomerDto? _selectedCustomer;

    public ObservableCollection<SalesOrderDto> SelectedCustomerOrders { get; } = new();

    // --- QUICK PAYMENT OVERLAY ---
    [ObservableProperty]
    private bool _showPaymentModal;

    [ObservableProperty]
    private decimal _paymentAmount;

    [ObservableProperty]
    private string _paymentNote = string.Empty;

    public ObservableCollection<string> CustomerFilters { get; } = new(new[] { "All Customers", "Has Debt / Balance", "Zero Balance" });

    [ObservableProperty]
    private string _selectedFilter = "All Customers";

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                ApplyFilter();
            }
        }
    }

    [ObservableProperty]
    private ViewModelBase? _currentOverlay;

    public CustomersViewModel(IMediator mediator)
    {
        _mediator = mediator;
        LoadCustomersCommand.Execute(null);
    }

    partial void OnSelectedFilterChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedCustomerChanged(CustomerDto? value)
    {
        if (value != null)
        {
            _ = LoadCustomerOrdersAsync(value.Id);
        }
        else
        {
            SelectedCustomerOrders.Clear();
        }
    }

    private async Task LoadCustomerOrdersAsync(Guid customerId)
    {
        SelectedCustomerOrders.Clear();
        try
        {
            var orders = await _mediator.Send(new GetAllSalesOrdersQuery());
            foreach (var o in orders.Take(5))
            {
                SelectedCustomerOrders.Add(o);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        var result = await _mediator.Send(new GetAllCustomersQuery());
        Customers = new ObservableCollection<CustomerDto>(result);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = Customers.AsEnumerable();

        if (SelectedFilter == "Has Debt / Balance")
        {
            filtered = filtered.Where(c => c.CurrentBalance > 0);
        }
        else if (SelectedFilter == "Zero Balance")
        {
            filtered = filtered.Where(c => c.CurrentBalance <= 0);
        }

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var q = SearchQuery.ToLower();
            filtered = filtered.Where(c => c.Name.ToLower().Contains(q) || 
                                         (c.Email != null && c.Email.ToLower().Contains(q)) ||
                                         (c.Phone != null && c.Phone.Contains(q)));
        }

        FilteredCustomers = new ObservableCollection<CustomerDto>(filtered);
    }

    [RelayCommand]
    private void OpenRecordPayment()
    {
        if (SelectedCustomer == null) return;
        PaymentAmount = SelectedCustomer.CurrentBalance > 0 ? SelectedCustomer.CurrentBalance : 50m;
        PaymentNote = "Account payment";
        ShowPaymentModal = true;
    }

    [RelayCommand]
    private void CancelRecordPayment()
    {
        ShowPaymentModal = false;
    }

    [RelayCommand]
    private async Task ConfirmRecordPaymentAsync()
    {
        if (SelectedCustomer == null || PaymentAmount <= 0) return;

        // Execute payment simulation
        ShowPaymentModal = false;
        MainWindowViewModel.Instance?.AddNotification(
            "Customer Payment Recorded",
            $"Payment of {PaymentAmount:C} received from {SelectedCustomer.Name}.",
            "💳",
            "#3B82F6",
            typeof(CustomersViewModel)
        );
        MainWindowViewModel.Instance?.ShowToast($"Payment of {PaymentAmount:C} recorded for customer '{SelectedCustomer.Name}'");
        await LoadCustomersAsync();
    }

    [RelayCommand]
    private async Task ExportStatementAsync()
    {
        if (SelectedCustomer == null) return;
        try
        {
            var pdfService = App.Services!.GetRequiredService<ThriveERP.Application.Common.Interfaces.IPdfExportService>();
            var downloadsPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "Downloads");
            var filePath = System.IO.Path.Combine(downloadsPath, $"Statement_{SelectedCustomer.Name.Replace(" ", "_")}.pdf");

            using (var stream = System.IO.File.Create(filePath))
            {
                await pdfService.ExportAsync(stream, SelectedCustomer);
                await stream.FlushAsync();
            }

            MainWindowViewModel.Instance?.ShowToast($"Statement exported for customer '{SelectedCustomer.Name}'");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ExportStatement error: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ShowAddCustomer()
    {
        var addVm = App.Services!.GetRequiredService<AddCustomerViewModel>();
        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadCustomersCommand.Execute(null);
            MainWindowViewModel.Instance?.ShowToast("Customer registered successfully");
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private void EditCustomer(CustomerDto? customer)
    {
        var target = customer ?? SelectedCustomer;
        if (target == null) return;

        var addVm = App.Services!.GetRequiredService<AddCustomerViewModel>();
        addVm.Id = target.Id;
        addVm.Name = target.Name;
        addVm.Phone = target.Phone;
        addVm.Email = target.Email;
        addVm.Address = target.Address;
        addVm.CreditLimit = target.CreditLimit;

        addVm.OnSaveComplete = () => 
        {
            CurrentOverlay = null;
            LoadCustomersCommand.Execute(null);
            MainWindowViewModel.Instance?.ShowToast($"Customer '{target.Name}' updated");
        };
        addVm.OnCancel = () => CurrentOverlay = null;
        
        CurrentOverlay = addVm;
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(CustomerDto? customer)
    {
        var target = customer ?? SelectedCustomer;
        if (target == null) return;
        
        await _mediator.Send(new DeleteCustomerCommand(target.Id));
        SelectedCustomer = null;
        LoadCustomersCommand.Execute(null);
        MainWindowViewModel.Instance?.ShowToast($"Customer '{target.Name}' removed");
    }
}

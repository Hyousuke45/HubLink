using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HubLink.Configuration;
using HubLink.Models;
using HubLink.Services;

namespace HubLink.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IShipmentApiService _apiService;
    private readonly List<Shipment> _allShipments = new();
    private DateTimeOffset? _lastSuccessfulSyncUtc;
    private string? _lastSuccessfulBranchCode;

    public MainWindowViewModel()
        : this(new ShipmentApiService(), ConfigurationLoader.LoadApiSettings())
    {
    }

    public MainWindowViewModel(IShipmentApiService apiService, ApiSettings initialSettings)
    {
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        ArgumentNullException.ThrowIfNull(initialSettings);

        BaseUrl = initialSettings.BaseUrl;
        EndpointPath = initialSettings.EndpointPath;
        BranchCode = initialSettings.BranchCode;
        Mode = initialSettings.Mode;
        TimeoutSeconds = initialSettings.TimeoutSeconds;

        StatusFilterOptions = new ObservableCollection<string>
        {
            "All",
            nameof(ShipmentStatus.Pending),
            nameof(ShipmentStatus.InTransit),
            nameof(ShipmentStatus.Delivered),
            nameof(ShipmentStatus.Exception)
        };

        FilteredShipments = new ObservableCollection<Shipment>();
    }

    // --- Configurable Connection Settings (initialized from appsettings.json) ---

    [ObservableProperty]
    private string _baseUrl = "http://127.0.0.1:5080";

    [ObservableProperty]
    private string _endpointPath = "/api/shipments";

    [ObservableProperty]
    private string _branchCode = "BR-001";

    [ObservableProperty]
    private string _mode = string.Empty;

    [ObservableProperty]
    private int _timeoutSeconds = 5;

    // --- Loading, Error, Stale-Data & Empty-Result States ---

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetryCommand))]
    private bool _hasError;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isStaleData;

    [ObservableProperty]
    private string? _staleDataWarning;

    [ObservableProperty]
    private bool _isEmptyResult;

    [ObservableProperty]
    private string? _emptyResultMessage;

    [ObservableProperty]
    private string _syncStatusBanner = "Ready. Click Refresh to load shipments from HubLink.";

    [ObservableProperty]
    private string? _exportFeedbackMessage;

    [ObservableProperty]
    private bool _isExportError;

    // --- Operational Summary Counts (Across ALL Retrieved Branch Shipments) ---

    [ObservableProperty]
    private int _totalCount;

    [ObservableProperty]
    private int _pendingCount;

    [ObservableProperty]
    private int _inTransitCount;

    [ObservableProperty]
    private int _deliveredCount;

    [ObservableProperty]
    private int _exceptionCount;

    // --- Status Filter & Displayed Shipments ---

    public ObservableCollection<string> StatusFilterOptions { get; }

    public ObservableCollection<Shipment> FilteredShipments { get; }

    [ObservableProperty]
    private string _selectedStatusFilter = "All";

    public int DisplayedCount => FilteredShipments.Count;

    partial void OnSelectedStatusFilterChanged(string value)
    {
        ApplyStatusFilter();
    }

    private bool CanFetch() => !IsLoading;

    private bool CanRetry() => !IsLoading && HasError;

    [RelayCommand(CanExecute = nameof(CanFetch))]
    public async Task RefreshAsync()
    {
        IsLoading = true;
        HasError = false;
        ErrorMessage = null;
        IsEmptyResult = false;
        EmptyResultMessage = null;
        ExportFeedbackMessage = null;
        SyncStatusBanner = $"Retrieving shipments for branch '{BranchCode.Trim()}'...";

        try
        {
            var currentSettings = new ApiSettings
            {
                BaseUrl = BaseUrl,
                EndpointPath = EndpointPath,
                BranchCode = BranchCode,
                Mode = Mode,
                TimeoutSeconds = TimeoutSeconds
            };

            var result = await _apiService.GetShipmentsAsync(currentSettings);

            if (result.IsSuccess)
            {
                _allShipments.Clear();
                _allShipments.AddRange(result.Shipments);

                _lastSuccessfulSyncUtc = result.RetrievedAtUtc;
                _lastSuccessfulBranchCode = result.BranchCode;

                IsStaleData = false;
                StaleDataWarning = null;
                HasError = false;
                ErrorMessage = null;

                UpdateSummaryCounts();
                ApplyStatusFilter();

                var formattedTime = result.RetrievedAtUtc
                    .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);

                if (result.IsEmpty)
                {
                    IsEmptyResult = true;
                    EmptyResultMessage =
                        $"Branch '{result.BranchCode}' returned 0 shipments. No records exist for this branch.";
                    SyncStatusBanner =
                        $"Connected — 0 shipments found for {result.BranchCode} (as of {formattedTime}).";
                }
                else
                {
                    IsEmptyResult = false;
                    EmptyResultMessage = null;
                    SyncStatusBanner =
                        $"Loaded {_allShipments.Count} shipment(s) for {result.BranchCode} at {formattedTime}.";
                }
            }
            else
            {
                HandleFailedRetrieval(result.ErrorMessage ?? "An unknown error occurred while retrieving shipments.");
            }
        }
        catch (Exception ex)
        {
            HandleFailedRetrieval($"Unexpected error during retrieval: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanRetry))]
    private async Task RetryAsync()
    {
        await RefreshAsync();
    }

    [RelayCommand]
    private void ResetToNormalEndpoint()
    {
        EndpointPath = "/api/shipments";
        Mode = string.Empty;
        ExportFeedbackMessage = null;
    }

    [RelayCommand]
    private void ExportCsv()
    {
        if (FilteredShipments.Count == 0)
        {
            IsExportError = true;
            ExportFeedbackMessage = "No shipment records are currently displayed to export.";
            return;
        }

        IsExportError = false;
        ExportFeedbackMessage = "CSV export will be connected in Phase 5.";
    }

    private void HandleFailedRetrieval(string failureReason)
    {
        HasError = true;
        ErrorMessage = failureReason;

        if (_lastSuccessfulSyncUtc.HasValue)
        {
            var previousTime = _lastSuccessfulSyncUtc.Value
                .ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
            var previousBranch = _lastSuccessfulBranchCode ?? BranchCode;

            IsStaleData = true;
            StaleDataWarning =
                $"Refresh failed — displaying previously loaded data for {previousBranch} from {previousTime}.";
            SyncStatusBanner = $"Refresh failed (showing previously loaded data from {previousTime}).";
        }
        else
        {
            IsStaleData = false;
            StaleDataWarning = null;
            SyncStatusBanner = "Retrieval failed. Review the error below and click Retry.";
        }
    }

    private void UpdateSummaryCounts()
    {
        TotalCount = _allShipments.Count;
        PendingCount = _allShipments.Count(s => s.Status == ShipmentStatus.Pending);
        InTransitCount = _allShipments.Count(s => s.Status == ShipmentStatus.InTransit);
        DeliveredCount = _allShipments.Count(s => s.Status == ShipmentStatus.Delivered);
        ExceptionCount = _allShipments.Count(s => s.Status == ShipmentStatus.Exception);
    }

    private void ApplyStatusFilter()
    {
        FilteredShipments.Clear();

        IEnumerable<Shipment> query = _allShipments;

        if (!string.IsNullOrWhiteSpace(SelectedStatusFilter) &&
            !SelectedStatusFilter.Equals("All", StringComparison.OrdinalIgnoreCase) &&
            Enum.TryParse<ShipmentStatus>(SelectedStatusFilter, ignoreCase: true, out var targetStatus))
        {
            query = query.Where(s => s.Status == targetStatus);
        }

        foreach (var shipment in query)
        {
            FilteredShipments.Add(shipment);
        }

        OnPropertyChanged(nameof(DisplayedCount));
    }
}
using System.Collections.ObjectModel;
using System.Windows.Input;
using BergerBytes.App.Services;
using BergerBytes.Shared.DTOs;
using BergerBytes.Shared.Models;

namespace BergerBytes.App.ViewModels
{
    public class FoodSearchPageViewModel : BindableObject
    {
        private readonly IFoodService _foodService;
        private readonly IRecentFoodRepository _recentFoodRepository;
        private string _searchQuery = string.Empty;
        private bool _isSearching = false;
        private CancellationTokenSource? _localSearchCts;
        private bool _hasNoResults = false;
        private bool _hasRecentItems = false;
        private bool _isLoadingMoreRecents = false;
        private bool _hasMoreRecents = true;
        private int _recentOffset = 0;
        private const int RecentPageSize = 10;

        public ObservableCollection<FoodProductDTO> SearchResults { get; } = new();
        public ObservableCollection<RecentFoodItem> RecentItems { get; } = new();
        public ObservableCollection<RecentFoodItem> MatchingRecentItems { get; } = new();

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                if (string.IsNullOrWhiteSpace(value))
                    ClearSearchState();
                else
                    _ = SearchLocalWithDebounceAsync(value);
            }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                _isSearching = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowRecentItems));
            }
        }

        public bool HasNoResults
        {
            get => _hasNoResults;
            set
            {
                _hasNoResults = value;
                OnPropertyChanged();
            }
        }

        public bool HasRecentItems
        {
            get => _hasRecentItems;
            set
            {
                _hasRecentItems = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowRecentItems));
            }
        }

        public bool ShowRecentItems => _hasRecentItems && !_isSearching && SearchResults.Count == 0 && !_hasMatchingRecents && !_hasNoResults;

        public bool IsLoadingMoreRecents
        {
            get => _isLoadingMoreRecents;
            set
            {
                _isLoadingMoreRecents = value;
                OnPropertyChanged();
            }
        }

        private bool _hasMatchingRecents = false;
        public bool HasMatchingRecents
        {
            get => _hasMatchingRecents;
            set
            {
                _hasMatchingRecents = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ShowRecentItems));
            }
        }

        private bool _hasApiResults = false;
        public bool HasApiResults
        {
            get => _hasApiResults;
            set
            {
                _hasApiResults = value;
                OnPropertyChanged();
            }
        }

        public ICommand SearchCommand { get; }
        public ICommand SelectProductCommand { get; }
        public ICommand SelectRecentCommand { get; }

        public FoodSearchPageViewModel(IFoodService foodService, IRecentFoodRepository recentFoodRepository)
        {
            _foodService = foodService;
            _recentFoodRepository = recentFoodRepository;
            SearchCommand = new Command(async () => await SearchAsync());
            SelectProductCommand = new Command<FoodProductDTO>(async product => await SelectProductAsync(product));
            SelectRecentCommand = new Command<RecentFoodItem>(async item => await SelectRecentAsync(item));
        }

        public async Task LoadRecentItemsAsync()
        {
            _recentOffset = 0;
            _hasMoreRecents = true;
            var recents = await _recentFoodRepository.GetRecentFoodsAsync(RecentPageSize, 0);
            RecentItems.Clear();
            foreach (var item in recents)
                RecentItems.Add(item);
            _recentOffset = recents.Count;
            _hasMoreRecents = recents.Count == RecentPageSize;
            HasRecentItems = RecentItems.Count > 0;
        }

        public async Task LoadMoreRecentItemsAsync()
        {
            if (_isLoadingMoreRecents || !_hasMoreRecents || !ShowRecentItems) return;

            IsLoadingMoreRecents = true;
            try
            {
                var recents = await _recentFoodRepository.GetRecentFoodsAsync(RecentPageSize, _recentOffset);
                foreach (var item in recents)
                    RecentItems.Add(item);
                _recentOffset += recents.Count;
                _hasMoreRecents = recents.Count == RecentPageSize;
                HasRecentItems = RecentItems.Count > 0;
            }
            finally
            {
                IsLoadingMoreRecents = false;
            }
        }

        private async Task SearchLocalWithDebounceAsync(string query)
        {
            _localSearchCts?.Cancel();
            _localSearchCts = new CancellationTokenSource();
            var cts = _localSearchCts;

            try
            {
                await Task.Delay(300, cts.Token);
                await SearchLocalAsync(query.Trim(), cts.Token);
            }
            catch (OperationCanceledException) { }
        }

        private async Task SearchLocalAsync(string query, CancellationToken cancellationToken = default)
        {
            var localResults = await _recentFoodRepository.SearchRecentFoodsAsync(query);

            if (cancellationToken.IsCancellationRequested) return;

            MatchingRecentItems.Clear();
            foreach (var item in localResults)
                MatchingRecentItems.Add(item);
            HasMatchingRecents = MatchingRecentItems.Count > 0;
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            // Cancel any pending debounced local search — we'll refresh it synchronously below
            _localSearchCts?.Cancel();

            IsSearching = true;
            HasNoResults = false;
            HasApiResults = false;
            SearchResults.Clear();

            try
            {
                // Refresh local results immediately before making the API call
                await SearchLocalAsync(SearchQuery.Trim());

                // API search
                var apiResults = await _foodService.SearchFoodAsync(SearchQuery);
                foreach (var result in apiResults)
                    SearchResults.Add(result);
                HasApiResults = SearchResults.Count > 0;

                HasNoResults = MatchingRecentItems.Count == 0 && SearchResults.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Food search error: {ex.Message}");
                HasNoResults = MatchingRecentItems.Count == 0;
            }
            finally
            {
                IsSearching = false;
                OnPropertyChanged(nameof(ShowRecentItems));
            }
        }

        private void ClearSearchState()
        {
            _localSearchCts?.Cancel();
            HasNoResults = false;
            HasApiResults = false;
            HasMatchingRecents = false;
            SearchResults.Clear();
            MatchingRecentItems.Clear();
            OnPropertyChanged(nameof(ShowRecentItems));
        }

        private async Task SelectProductAsync(FoodProductDTO product)
        {
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "SelectedFoodProduct", product }
            });
        }

        private async Task SelectRecentAsync(RecentFoodItem item)
        {
            await Shell.Current.GoToAsync("..", new Dictionary<string, object>
            {
                { "SelectedRecentFood", item }
            });
        }
    }
}

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
        private bool _hasNoResults = false;
        private bool _hasRecentItems = false;

        public ObservableCollection<FoodProductDTO> SearchResults { get; } = new();
        public ObservableCollection<RecentFoodItem> RecentItems { get; } = new();

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
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

        public bool ShowRecentItems => _hasRecentItems && !_isSearching && SearchResults.Count == 0;

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
            var recents = await _recentFoodRepository.GetRecentFoodsAsync(20);
            RecentItems.Clear();
            foreach (var item in recents)
                RecentItems.Add(item);
            HasRecentItems = RecentItems.Count > 0;
        }

        private async Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
                return;

            IsSearching = true;
            HasNoResults = false;
            SearchResults.Clear();
            OnPropertyChanged(nameof(ShowRecentItems));

            try
            {
                var results = await _foodService.SearchFoodAsync(SearchQuery);
                foreach (var result in results)
                    SearchResults.Add(result);

                HasNoResults = SearchResults.Count == 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Food search error: {ex.Message}");
                HasNoResults = true;
            }
            finally
            {
                IsSearching = false;
                OnPropertyChanged(nameof(ShowRecentItems));
            }
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
                { "FoodName", item.FoodName },
                { "Calories", item.Calories.ToString("F0") },
                { "Protein", item.Protein.ToString("F1") },
                { "Carbs", item.Carbs.ToString("F1") },
                { "Fat", item.Fat.ToString("F1") },
                { "Barcode", item.Barcode },
                { "Quantity", item.Quantity.ToString("G") },
                { "Unit", item.Unit }
            });
        }
    }
}

using BergerBytes.Shared.DTOs;

namespace BergerBytes.App.Services
{
    public interface IFoodService
    {
        Task<FoodProductDTO> GetProductByBarcodeAsync(string barcode, CancellationToken ct = default);
        Task<IList<FoodProductDTO>> SearchFoodAsync(string query, CancellationToken ct = default);
    }
}

using BergerBytes.Shared.DTOs;

namespace BergerBytes.App.Services
{
    public interface IFoodService
    {
        Task<FoodProductDTO> GetProductByBarcodeAsync(string barcode, CancellationToken ct = default);
    }
}

using Tutorial9.DTOs;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<int> AddProductAsync(WarehouseProductDTO warehouseProductDto);
    Task<int> AddProductProcedureAsync(WarehouseProductDTO warehouseProductDto);
}
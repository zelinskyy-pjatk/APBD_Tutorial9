using Tutorial9.DTOs;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;

    public WarehouseService(IConfiguration configuration) => _configuration = configuration;
    
    public Task<int> AddProductAsync(WarehouseProductDTO warehouseProductDto)
    {
        throw new NotImplementedException();
    }

    public async Task<int> AddProductProcedureAsync(WarehouseProductDTO warehouseProductDto)
    {
        throw new NotImplementedException();
    }
}
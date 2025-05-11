using Microsoft.AspNetCore.Mvc;
using Tutorial9.DTOs;
using Tutorial9.Services;

namespace Tutorial9.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehouseController : ControllerBase
{
    private readonly IWarehouseService _warehouseService;
    
    public WarehouseController(IWarehouseService warehouseService) => _warehouseService = warehouseService;

    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] WarehouseProductDTO warehouseProductDto)
    {
        try
        {
            var result = await _warehouseService.AddProductAsync(warehouseProductDto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
    
    [HttpPost("proc")]
    public async Task<IActionResult> AddProductToWarehouseThroughProcedure(
        [FromBody] WarehouseProductDTO warehouseProductDto)
    {
        try
        {
            var result = await _warehouseService.AddProductProcedureAsync(warehouseProductDto);
            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal Server Error: {ex.Message}");
        }
    }
    
}
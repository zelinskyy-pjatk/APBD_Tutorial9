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
    
    // -- POST: /api/warehouse
    // -- Simple POST method to insert Product into Warehouse -- //
    [HttpPost]
    public async Task<IActionResult> AddProductToWarehouse([FromBody] WarehouseProductDTO warehouseProductDto)
    {
        try
        {
            var result = await _warehouseService.AddProductAsync(warehouseProductDto);
            return StatusCode(StatusCodes.Status201Created, result);                        // result -> ID of inserted product into warehouse
        }
        catch (ArgumentException ex)
        { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex)
        { return BadRequest(ex.Message); }
        catch (Exception ex)
        { return StatusCode(500, $"Internal Server Error: {ex.Message}"); }
    }
    
    // -- POST: /api/warehouse/proc
    // -- Method uses predefined procedure to check necessary Product parameters before inserting Product into Warehouse -- //
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
        { return BadRequest(ex.Message); }
        catch (KeyNotFoundException ex)
        { return BadRequest(ex.Message); }
        catch (Exception ex)
        { return StatusCode(500, $"Internal Server Error: {ex.Message}"); }
    }
}
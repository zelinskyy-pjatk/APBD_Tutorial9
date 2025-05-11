using System.Data;
using Microsoft.Data.SqlClient;
using Tutorial9.DTOs;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;

    public WarehouseService(IConfiguration configuration) => _configuration = configuration;
    
    public async Task<int> AddProductAsync(WarehouseProductDTO warehouseProductDto)
    {
        const string checkProductSQL_Command = "SELECT 1 FROM Product WHERE IdProduct = @IdProduct";
        const string checkWarehouseSQL_Command = "SELECT 1 FROM Warehouse WHERE IdWarehouse = @IdWarehouse";
        const string checkOrderSQL_Command = """
                                                SELECT TOP 1 * FROM [Order] 
                                                WHERE IdProduct = @IdProduct
                                                      AND
                                                      Amount >= @Amount
                                                      AND
                                                      CreatedAt < @CreatedAt
                                                ORDER BY CreatedAt DESC;
                                             """;
        const string checkFulfilledOrder_Command = "SELECT 1 FROM Product_Warehouse WHERE IdOrder = @IdOrder";
        const string updateOrderSQL_Command = "UPDATE [Order] SET FulfilledAt = @currentTime WHERE IdOrder = @IdOrder";
        const string selectPriceSQL_Command = "SELECT Price FROM Product WHERE IdProduct = @IdProduct";
        const string insertProductSQL_Command = """
                                                    INSERT INTO Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                                                    VALUES (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)
                                                    SELECT SCOPE_IDENTITY();
                                                """;
        
        
        using (var conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await conn.OpenAsync();
            using (var transaction = conn.BeginTransaction())
            {
                try
                {
                    // -- Check whether product exists -- //
                    using (var checkProductCMD = new SqlCommand(checkProductSQL_Command, conn, transaction))
                    {
                        checkProductCMD.Parameters.AddWithValue("@IdProduct", warehouseProductDto.IdProduct);
                        if (await checkProductCMD.ExecuteScalarAsync() == null)
                            throw new Exception("Product was not found.");
                    }
                    
                    // -- Check whether warehouse exists -- //
                    using (var checkWarehouseCMD = new SqlCommand(checkWarehouseSQL_Command, conn, transaction))
                    {
                        checkWarehouseCMD.Parameters.AddWithValue("@IdWarehouse", warehouseProductDto.IdWarehouse);
                        if (await checkWarehouseCMD.ExecuteScalarAsync() == null) 
                            throw new Exception("Warehouse was not found.");
                    }
                    
                    if (warehouseProductDto.Amount <= 0)
                        throw new Exception("Amount must be greater than zero.");

                    int orderId;

                    using (var checkOrderCMD = new SqlCommand(checkOrderSQL_Command, conn, transaction))
                    {
                        checkOrderCMD.Parameters.AddWithValue("@IdProduct", warehouseProductDto.IdProduct);
                        checkOrderCMD.Parameters.AddWithValue("@Amount", warehouseProductDto.Amount);
                        checkOrderCMD.Parameters.AddWithValue("@CreatedAt", DateTime.Now);


                        using (var reader = await checkOrderCMD.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync()) 
                                throw new Exception("Order was not found.");
                            orderId = (int)reader["IdOrder"];
                        }
                    }

                    using (var fulfilledOrderCmd = new SqlCommand(checkFulfilledOrder_Command, conn, transaction))
                    {
                        fulfilledOrderCmd.Parameters.AddWithValue("@IdOrder", orderId);
                        if (await fulfilledOrderCmd.ExecuteScalarAsync() != null)
                            throw new Exception("Order is already fulfilled.");
                    }

                    using (var updateOrderCMD = new SqlCommand(updateOrderSQL_Command, conn, transaction))
                    {
                        updateOrderCMD.Parameters.AddWithValue("@currentTime", DateTime.Now);
                        updateOrderCMD.Parameters.AddWithValue("@IdOrder", orderId);
                        await updateOrderCMD.ExecuteNonQueryAsync();
                    }

                    decimal pricePerUnit;
                    using (var priceCMD = new SqlCommand(selectPriceSQL_Command, conn, transaction))
                    {
                        priceCMD.Parameters.AddWithValue("@IdProduct", warehouseProductDto.IdProduct);
                        pricePerUnit = (decimal) await priceCMD.ExecuteScalarAsync();
                    }
                    
                    var totalPrice = pricePerUnit * warehouseProductDto.Amount;

                    using (var insertProductCMD = new SqlCommand(insertProductSQL_Command, conn, transaction))
                    {
                        insertProductCMD.Parameters.AddWithValue("@IdWarehouse", warehouseProductDto.IdWarehouse);
                        insertProductCMD.Parameters.AddWithValue("@IdProduct", warehouseProductDto.IdProduct);
                        insertProductCMD.Parameters.AddWithValue("@IdOrder", orderId);
                        insertProductCMD.Parameters.AddWithValue("@Amount", warehouseProductDto.Amount);
                        insertProductCMD.Parameters.AddWithValue("@Price", totalPrice);
                        insertProductCMD.Parameters.AddWithValue("@CreatedAt", DateTime.Now);
                            
                        var insertedID = Convert.ToInt32(await insertProductCMD.ExecuteScalarAsync());
                            
                        transaction.Commit();
                        return insertedID;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }

    public async Task<int> AddProductProcedureAsync(WarehouseProductDTO warehouseProductDto)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand
        {
            Connection = connection,
            CommandText = "AddProductToWarehouse",
            CommandType = CommandType.StoredProcedure
        };
        
        command.Parameters.AddWithValue("@IdProduct", warehouseProductDto.IdProduct);
        command.Parameters.AddWithValue("@IdWarehouse", warehouseProductDto.IdWarehouse);
        command.Parameters.AddWithValue("@Amount", warehouseProductDto.Amount);
        command.Parameters.AddWithValue("@CreatedAt", warehouseProductDto.CreatedAt);
        
        await connection.OpenAsync();
        
        var result = await command.ExecuteScalarAsync();
        if (result == null) throw new Exception("Stored procedure returned null.");
            
        return Convert.ToInt32(result);
    }
}
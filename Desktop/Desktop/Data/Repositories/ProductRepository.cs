using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Desktop.Models;
using Npgsql;

namespace Desktop.Data.Repositories;

public class ProductRepository : BaseRepository
{
    public ProductRepository(string connectionString) : base(connectionString) { }

    public async Task<List<Product>> GetAllAsync()
    {
        var list = new List<Product>();
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"SELECT p.product_id, p.name, p.description, p.current_price, p.stock_quantity, p.category_id, c.name AS category_name
              FROM product p JOIN category c ON p.category_id = c.category_id
              ORDER BY p.name", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            list.Add(new Product
            {
                ProductId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                CurrentPrice = reader.GetDecimal(3),
                StockQuantity = reader.GetInt32(4),
                CategoryId = reader.GetInt32(5),
                CategoryName = reader.GetString(6)
            });
        return list;
    }

    public async Task AddAsync(Product product)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO product (name, description, current_price, stock_quantity, category_id)
              VALUES (@p1, @p2, @p3, @p4, @p5) RETURNING product_id", conn);
        cmd.Parameters.AddWithValue("p1", product.Name);
        cmd.Parameters.AddWithValue("p2", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("p3", product.CurrentPrice);
        cmd.Parameters.AddWithValue("p4", product.StockQuantity);
        cmd.Parameters.AddWithValue("p5", product.CategoryId);
        product.ProductId = (int)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Product product)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            @"UPDATE product SET name=@p1, description=@p2, current_price=@p3, stock_quantity=@p4, category_id=@p5
              WHERE product_id=@p6", conn);
        cmd.Parameters.AddWithValue("p1", product.Name);
        cmd.Parameters.AddWithValue("p2", (object?)product.Description ?? DBNull.Value);
        cmd.Parameters.AddWithValue("p3", product.CurrentPrice);
        cmd.Parameters.AddWithValue("p4", product.StockQuantity);
        cmd.Parameters.AddWithValue("p5", product.CategoryId);
        cmd.Parameters.AddWithValue("p6", product.ProductId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "DELETE FROM product WHERE product_id=@p1", conn);
        cmd.Parameters.AddWithValue("p1", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> GetStockAsync(int productId)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(
            "SELECT stock_quantity FROM product WHERE product_id = @pid", conn);
        cmd.Parameters.AddWithValue("pid", productId);
        var result = await cmd.ExecuteScalarAsync();
        return result is not null and not DBNull ? Convert.ToInt32(result) : 0;
    }

    public async Task<decimal?> GetLastPurchasePriceAsync(int productId)
    {
        await using var conn = CreateConnection();
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(@"
            SELECT si.unit_purchase_price
            FROM supply_item si
            JOIN supply s ON si.supply_id = s.supply_id
            WHERE si.product_id = @pid
            ORDER BY s.supply_date DESC, s.supply_id DESC
            LIMIT 1", conn);
        cmd.Parameters.AddWithValue("pid", productId);
        var result = await cmd.ExecuteScalarAsync();
        return result is not null and not DBNull ? (decimal)result : 0m;
    }
}
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020590.DataLayers.Interfaces;
using SV22T1020590.Models.Catalog;
using SV22T1020590.Models.Common;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SV22T1020590.DataLayers.SQLServer
{
    public class ProductRepository : BaseRepository, IProductRepository
    {
        public ProductRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> AddAsync(Product data)
        {
            const string sql = @"INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
SELECT CAST(SCOPE_IDENTITY() as int);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<int>(sql, data);
            return id;
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var cn = GetConnection();
            await cn.OpenAsync();

            // delete photos
            await cn.ExecuteAsync("DELETE FROM ProductPhotos WHERE ProductID = @productID", new { productID });
            // delete attributes
            await cn.ExecuteAsync("DELETE FROM ProductAttributes WHERE ProductID = @productID", new { productID });
            // delete product
            var affected = await cn.ExecuteAsync("DELETE FROM Products WHERE ProductID = @productID", new { productID });
            return affected > 0;
        }

        public async Task<Product?> GetAsync(int productID)
        {
            const string sql = "SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling FROM Products WHERE ProductID = @productID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<Product>(sql, new { productID });
            return item;
        }

        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            const string sql = "SELECT AttributeID, ProductID, AttributeName, AttributeValue FROM ProductAttributes WHERE ProductID = @productID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var items = (await cn.QueryAsync<ProductAttribute>(sql, new { productID })).ToList();
            return items;
        }

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add("(ProductName LIKE @q OR ProductDescription LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }
            if (input.SupplierID > 0)
            {
                whereClauses.Add("SupplierID = @SupplierID");
                parameters.Add("SupplierID", input.SupplierID);
            }
            if (input.CategoryID > 0)
            {
                whereClauses.Add("CategoryID = @CategoryID");
                parameters.Add("CategoryID", input.CategoryID);
            }
            if (input.MinPrice > 0)
            {
                whereClauses.Add("Price >= @MinPrice");
                parameters.Add("MinPrice", input.MinPrice);
            }
            if (input.MaxPrice > 0)
            {
                whereClauses.Add("Price <= @MaxPrice");
                parameters.Add("MaxPrice", input.MaxPrice);
            }
            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            var sqlCount = $"SELECT COUNT(*) FROM Products {where}";

            string sql;
            if (input.PageSize == 0)
            {
                sql = $"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling FROM Products {where} ORDER BY ProductID";
            }
            else
            {
                sql = $"SELECT ProductID, ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling FROM Products {where} ORDER BY ProductID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();
            var rowCount = await cn.ExecuteScalarAsync<int>(sqlCount, parameters);
            var items = (await cn.QueryAsync<Product>(sql, parameters)).ToList();

            return new PagedResult<Product>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            };
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            const string sql = "SELECT AttributeID, ProductID, AttributeName, AttributeValue FROM ProductAttributes WHERE AttributeID = @attributeID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { attributeID });
            return item;
        }

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            const string sql = @"INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue)
VALUES (@ProductID, @AttributeName, @AttributeValue); SELECT CAST(SCOPE_IDENTITY() as bigint);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<long>(sql, data);
            return id;
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            const string sql = @"UPDATE Products SET ProductName = @ProductName, ProductDescription = @ProductDescription, SupplierID = @SupplierID, CategoryID = @CategoryID, Unit = @Unit, Price = @Price, Photo = @Photo, IsSelling = @IsSelling WHERE ProductID = @ProductID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            const string sql = "UPDATE ProductAttributes SET AttributeName = @AttributeName, AttributeValue = @AttributeValue WHERE AttributeID = @AttributeID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            const string sql = "DELETE FROM ProductAttributes WHERE AttributeID = @attributeID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { attributeID });
            return affected > 0;
        }

        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            const string sql = "SELECT PhotoID, ProductID, Photo, Description FROM ProductPhotos WHERE ProductID = @productID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var items = (await cn.QueryAsync<ProductPhoto>(sql, new { productID })).ToList();
            return items;
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            const string sql = "SELECT PhotoID, ProductID, Photo, Description FROM ProductPhotos WHERE PhotoID = @photoID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { photoID });
            return item;
        }

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            const string sql = @"INSERT INTO ProductPhotos (ProductID, Photo, Description)
VALUES (@ProductID, @Photo, @Description); SELECT CAST(SCOPE_IDENTITY() as bigint);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<long>(sql, data);
            return id;
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            const string sql = "UPDATE ProductPhotos SET Photo = @Photo, Description = @Description WHERE PhotoID = @PhotoID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            const string sql = "DELETE FROM ProductPhotos WHERE PhotoID = @photoID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { photoID });
            return affected > 0;
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM OrderDetails WHERE ProductID = @productID) THEN 1 ELSE 0 END";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var used = await cn.ExecuteScalarAsync<int>(sql, new { productID });
            return used == 1;
        }
    }
}

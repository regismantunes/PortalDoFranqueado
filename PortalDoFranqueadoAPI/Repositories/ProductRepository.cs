using PortalDoFranqueadoAPI.Models;
using PortalDoFranqueadoAPI.Repositories.Util;
using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class ProductRepository(SqlConnection connection, IFileRepository fileRepository) : IProductRepository
    {
        public async Task<IEnumerable<Product>> GetList(int collectionId, int? familyId = null)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var list = new List<Product>();
                using (var cmd = new SqlCommand($"""
                                                SELECT *
                                                FROM Product
                                                WHERE CollectionId = @CollectionId
                                                {(familyId.HasValue ? " AND FamilyId = @FamilyId" : string.Empty)}
                                                """, connection))
                {
                    cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                    if (familyId.HasValue)
                        cmd.Parameters.AddWithValue("@FamilyId", familyId);

                    using var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                    while (await reader.ReadAsync().AsNoContext())
                        list.Add(new Product()
                        {
                            Id = reader.GetInt32("Id"),
                            FileId = reader.GetInt32("FileId"),
                            Description = reader.GetValue("Description") as string,
                            Price = reader.GetDecimal("Price"),
                            FamilyId = reader.GetInt32("FamilyId"),
                            LockedSizes = reader.GetStringArray("LockedSizes"),
                            SupplierId = reader.GetValue("SupplierId") as int?
                        });

                    await reader.CloseAsync().AsNoContext();
                }

                return list;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<int> Insert(int collectionId, Product product)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            INSERT INTO Product
                                                (   CollectionId
                                                ,   FamilyId
                                                ,   FileId
                                                ,   Price
                                                ,   LockedSizes
                                                ,   SupplierId
                                                ,   Description)
                                            OUTPUT INSERTED.Id
                                            VALUES (@CollectionId
                                                ,   @FamilyId
                                                ,   @FileId
                                                ,   @Price
                                                ,   @LockedSizes
                                                ,   @SupplierId
                                                ,   @Description);
                                            """, connection);

                cmd.Parameters.AddWithValue("@CollectionId", collectionId);
                cmd.Parameters.AddWithValue("@FamilyId", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@FileId", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@Price", product.Price.ToDBValue());
                cmd.Parameters.AddWithValue("@LockedSizes", product.LockedSizes.ToDBValue());
                cmd.Parameters.AddWithValue("@SupplierId", product.SupplierId.ToDBValue());
                cmd.Parameters.AddWithValue("@Description", product.Description.ToDBValue());

                var dbid = await cmd.ExecuteScalarAsync().AsNoContext();
                if (dbid == null)
                    throw new Exception(MessageRepositories.InsertFailException);

                return Convert.ToInt32(dbid);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task Update(Product product)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            UPDATE Product
                                                SET FamilyId = @FamilyId
                                                ,   FileId = @FileId
                                                ,   Price = @Price
                                                ,   LockedSizes = @LockedSizes
                                                ,   SupplierId = @SupplierId
                                                ,   Description = @Description
                                            WHERE Id = @Id;
                                            """, connection);

                cmd.Parameters.AddWithValue("@FamilyId", product.FamilyId.ToDBValue());
                cmd.Parameters.AddWithValue("@FileId", product.FileId.ToDBValue());
                cmd.Parameters.AddWithValue("@Price", product.Price.ToDBValue());
                cmd.Parameters.AddWithValue("@LockedSizes", product.LockedSizes.ToDBValue());
                cmd.Parameters.AddWithValue("@SupplierId", product.SupplierId.ToDBValue());
                cmd.Parameters.AddWithValue("@Description", product.Description.ToDBValue());
                cmd.Parameters.AddWithValue("@Id", product.Id);

                if (await cmd.ExecuteNonQueryAsync() == 0)
                    throw new Exception(MessageRepositories.UpdateFailException);
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        public async Task<bool> Delete(int id)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                using var transaction = await connection.BeginTransactionAsync().AsNoContext();

                try
                {
                    var cmd = new SqlCommand("""
                                                SELECT FileId
                                                FROM Product
                                                WHERE Id = @Id;
                                                """, connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@Id", id);

                    var fileIdObj = await cmd.ExecuteScalarAsync().AsNoContext();

                    cmd.CommandText = """
                                        DELETE FROM Product
                                        WHERE Id = @Id; 
                                        """;

                    var sucess = await cmd.ExecuteNonQueryAsync() > 0;

                    if (sucess &&
                        fileIdObj is int fileId)
                        await fileRepository.DeleteFile(fileId, transaction).AsNoContext();

                    await transaction.CommitAsync().AsNoContext();

                    return sucess;
                }
                catch
                {
                    await transaction.RollbackAsync().AsNoContext();
                    throw;
                }
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }
    }
}

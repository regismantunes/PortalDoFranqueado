﻿using System.Data;
using PortalDoFranqueadoAPI.Models.Validations;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using PortalDoFranqueadoAPI.Extensions;
using PortalDoFranqueadoAPI.Repositories.Interfaces;
using PortalDoFranqueadoAPI.Models.Entities;

namespace PortalDoFranqueadoAPI.Repositories
{
    public class PurchaseSuggestionRepository(SqlConnection connection) : IPurchaseSuggestionRepository
    {
        public async Task<int> Save(PurchaseSuggestion purchaseSuggestion)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var newPurchaseSuggestion = purchaseSuggestion.Id == null;

                using var transaction = await connection.BeginTransactionAsync().AsNoContext();

                try
                {
                    var command = newPurchaseSuggestion ?
                                """
                                INSERT INTO Purchase_Suggestion
                                    (   PurchaseId
                                    ,   Target
                                    ,   AverageTicket
                                    ,   PartsPerService
                                    ,   Coverage
                                    ,   TotalSuggestedItems)
                                OUTPUT INSERTED.Id
                                VALUES (@PurchaseId
                                    ,   @Target
                                    ,   @AverageTicket
                                    ,   @PartsPerService
                                    ,   @Coverage
                                    ,   @TotalSuggestedItems);
                                """ :
                                """
                                UPDATE Purchase_Suggestion
                                    SET PurchaseId = @PurchaseId
                                    ,   Target = @Target
                                    ,   AverageTicket = @AverageTicket
                                    ,   PartsPerService = @PartsPerService
                                    ,   Coverage = @Coverage
                                    ,   TotalSuggestedItems = @TotalSuggestedItems
                                WHERE Id = @Id;
                                """;

                    using var cmd = new SqlCommand(command, connection, (SqlTransaction)transaction);

                    cmd.Parameters.AddWithValue("@PurchaseId", purchaseSuggestion.PurchaseId);
                    cmd.Parameters.AddWithValue("@Target", purchaseSuggestion.Target);
                    cmd.Parameters.AddWithValue("@AverageTicket", purchaseSuggestion.AverageTicket);
                    cmd.Parameters.AddWithValue("@PartsPerService", purchaseSuggestion.PartsPerService);
                    cmd.Parameters.AddWithValue("@Coverage", purchaseSuggestion.Coverage);
                    cmd.Parameters.AddWithValue("@TotalSuggestedItems", purchaseSuggestion.TotalSuggestedItems);

                    if (newPurchaseSuggestion)
                    {
                        purchaseSuggestion.Id = (int?)await cmd.ExecuteScalarAsync() ??
                            throw new Exception(MessageRepositories.InsertFailException);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Id", purchaseSuggestion.Id);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.UpdateFailException);

                        cmd.Parameters.Clear();
                        cmd.CommandText = """
                                            DELETE FROM Purchase_Suggestion_Family
                                            WHERE PurchaseSuggestionId = @PurchaseSuggestionId;
                                            """;
                        cmd.Parameters.AddWithValue("@PurchaseSuggestionId", purchaseSuggestion.Id);

                        await cmd.ExecuteNonQueryAsync().AsNoContext();
                    }

                    cmd.CommandText = """
                                        INSERT INTO Purchase_Suggestion_Family
                                            (   PurchaseSuggestionId
                                            ,   FamilyId
                                            ,   [Percentage]
                                            ,   FamilySuggestedItems)
                                        OUTPUT INSERTED.Id
                                        VALUES (@PurchaseSuggestionId
                                            ,   @FamilyId
                                            ,   @Percentage
                                            ,   @FamilySuggestedItems);
                                        """;

                    foreach (var item in purchaseSuggestion.Families.Where(f => f.Percentage > 0))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@PurchaseSuggestionId", purchaseSuggestion.Id);
                        cmd.Parameters.AddWithValue("@FamilyId", item.FamilyId);
                        cmd.Parameters.AddWithValue("@Percentage", item.Percentage);
                        cmd.Parameters.AddWithValue("@FamilySuggestedItems", item.FamilySuggestedItems);

                        item.Id = (int?)await cmd.ExecuteScalarAsync() ??
                            throw new Exception(MessageRepositories.InsertFailException);

                        item.Sizes?.ToList()
                                   .ForEach(s => s.PurchaseSuggestionFamilyId = item.Id);
                    }

                    cmd.CommandText = """
                                        INSERT INTO Purchase_Suggestion_Family_Size
                                            (   PurchaseSuggestionFamilyId
                                            ,   SizeId
                                            ,   [Percentage]
                                            ,   SizeSuggestedItems)
                                        VALUES (@PurchaseSuggestionFamilyId
                                            ,   @SizeId
                                            ,   @Percentage
                                            ,   @SizeSuggestedItems);
                                        """;

                    foreach (var size in purchaseSuggestion.Families.Where(f => f.Sizes != null)
                                                                   .SelectMany(f => f.Sizes.Where(s => s.Percentage > 0)))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@PurchaseSuggestionFamilyId", size.PurchaseSuggestionFamilyId);
                        cmd.Parameters.AddWithValue("@SizeId", size.Size.Size);
                        cmd.Parameters.AddWithValue("@Percentage", size.Percentage);
                        cmd.Parameters.AddWithValue("@SizeSuggestedItems", size.SizeSuggestedItems);

                        if (await cmd.ExecuteNonQueryAsync() == 0)
                            throw new Exception(MessageRepositories.InsertFailException);
                    }

                    await transaction.CommitAsync().AsNoContext();

                    return purchaseSuggestion.Id.Value;
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

        public async Task<PurchaseSuggestion?> GetByPurchaseId(int purchaseId)
        {
            try
            {
                await connection.OpenAsync().AsNoContext();

                if (connection.State != ConnectionState.Open)
                    throw new Exception(MessageRepositories.ConnectionNotOpenException);

                var cmd = new SqlCommand("""
                                            SELECT *
                                            FROM Purchase_Suggestion
                                            WHERE PurchaseId = @PurchaseId;
                                            """, connection);

                cmd.Parameters.AddWithValue("@PurchaseId", purchaseId);

                var reader = await cmd.ExecuteReaderAsync().AsNoContext();

                if (await reader.ReadAsync().AsNoContext())
                {
                    var purchase = LoadPurchaseSuggestion(reader);
                    await reader.CloseAsync().AsNoContext();
                    await LoadPurchaseSuggestionFamilies(purchase).AsNoContext();
                    await LoadPurchaseSuggestionFamiliesSizes(purchase).AsNoContext();

                    return purchase;
                }

                return null;
            }
            finally
            {
                await connection.CloseAsync().AsNoContext();
            }
        }

        private static PurchaseSuggestion LoadPurchaseSuggestion(SqlDataReader reader)
            => new()
            {
                Id = reader.GetInt32("Id"),
                PurchaseId = reader.GetInt32("PurchaseId"),
                Target = (decimal?)reader.GetValue("Target"),
                AverageTicket = (decimal?)reader.GetValue("AverageTicket"),
                PartsPerService = (int?)reader.GetValue("PartsPerService"),
                Coverage = (decimal?)reader.GetValue("Coverage"),
                TotalSuggestedItems = (int?)reader.GetValue("TotalSuggestedItems")
            };

        private async Task LoadPurchaseSuggestionFamilies(PurchaseSuggestion purchaseSuggestion)
        {
            var listItems = new List<PurchaseSuggestionFamily>();
            var connectionWasClosed = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().AsNoContext();
                    if (connection.State != ConnectionState.Open)
                        throw new Exception(MessageRepositories.ConnectionNotOpenException);
                    connectionWasClosed = true;
                }

                using var cmd = new SqlCommand("""
                                                SELECT  psf.*
                                                    ,   f.Name AS FamilyName
                                                FROM Purchase_Suggestion_Family AS psf
                                                    INNER JOIN Family AS f
                                                        ON f.Id = psf.FamilyId
                                                WHERE PurchaseSuggestionId = @PurchaseSuggestionId;
                                                """, connection);

                cmd.Parameters.AddWithValue("@PurchaseSuggestionId", purchaseSuggestion.Id);
                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();
                while (await reader.ReadAsync().AsNoContext())
                {
                    var family = new Family
                    {
                        Id = reader.GetInt32("FamilyId"),
                        Name = reader.GetString("FamilyName"),
                    };

                    listItems.Add(new PurchaseSuggestionFamily()
                    {
                        Id = reader.GetInt32("Id"),
                        PurchaseSuggestionId = reader.GetInt32("PurchaseSuggestionId"),
                        FamilyId = family.Id,
                        Family = family,
                        Percentage = reader.GetDecimal("Percentage"),
                        FamilySuggestedItems = (int?)reader.GetValue("FamilySuggestedItems")
                    });
                }

                await reader.CloseAsync().AsNoContext();

                purchaseSuggestion.Families = listItems.ToArray();
            }
            finally
            {
                if (connectionWasClosed)
                    await connection.CloseAsync().AsNoContext();
            }
        }

        private async Task LoadPurchaseSuggestionFamiliesSizes(PurchaseSuggestion purchaseSuggestion)
        {
            var listItems = new List<PurchaseSuggestionFamilySize>();
            var connectionWasClosed = false;

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync().AsNoContext();
                    if (connection.State != ConnectionState.Open)
                        throw new Exception(MessageRepositories.ConnectionNotOpenException);
                    connectionWasClosed = true;
                }

                using var cmd = new SqlCommand("""
                                                SELECT  psfs.*
                                                    ,   fs.[Order]
                                                FROM Purchase_Suggestion_Family AS psf
                                                    INNER JOIN Purchase_Suggestion_Family_Size AS psfs
                                                        ON psfs.PurchaseSuggestionFamilyId = psf.Id
                                                    INNER JOIN Family AS f
                                                        ON f.Id = psf.FamilyId
                                                    INNER JOIN Family_Size AS fs
                                                        ON fs.SizeId = psfs.SizeId
                                                WHERE PurchaseSuggestionId = @PurchaseSuggestionId;
                                                """, connection);

                cmd.Parameters.AddWithValue("@PurchaseSuggestionId", purchaseSuggestion.Id);
                using var reader = await cmd.ExecuteReaderAsync().AsNoContext();
                while (await reader.ReadAsync().AsNoContext())
                {
                    var size = new ProductSize
                    {
                        Size = reader.GetString("SizeId"),
                        Order = reader.GetInt16("Order")
                    };

                    listItems.Add(new PurchaseSuggestionFamilySize()
                    {
                        Id = reader.GetInt32("Id"),
                        PurchaseSuggestionFamilyId = reader.GetInt32("PurchaseSuggestionFamilyId"),
                        Size = size,
                        Percentage = reader.GetDecimal("Percentage"),
                        SizeSuggestedItems = reader.GetInt32("SizeSuggestedItems")
                    });
                }

                await reader.CloseAsync().AsNoContext();

                purchaseSuggestion.Families?
                    .ToList()
                    .ForEach(f =>
                        f.Sizes = listItems
                            .Where(s => s.PurchaseSuggestionFamilyId == f.Id)
                            .ToArray()
                        );
            }
            finally
            {
                if (connectionWasClosed)
                    await connection.CloseAsync().AsNoContext();
            }
        }
    }
}

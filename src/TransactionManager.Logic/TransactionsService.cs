using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using Dapper;
using Microsoft.AspNetCore.Http;
using TransactionManager.Data.Database;
using TransactionManager.Logic.CsvHelpers;
using TransactionManager.Logic.Exceptions;
using Transaction = TransactionManager.Data.Model.Transaction;

namespace TransactionManager.Logic;

public class TransactionsService(IDbConnectionFactory dbConnectionFactory) : ITransactionsService
{
    public async Task UploadTransactions(IFormFile csvFile)
    {
        var records = ExtractFromCsv(csvFile);

        await using var connection = await dbConnectionFactory.CreateConnectionAsync();
        const string sql = 
            """
            INSERT INTO "Transactions" ("Id", "Name", "Email", "Amount", "Date", "IanaTimeZone", "ClientLocation")
            SELECT DISTINCT ON ("Id") *
            FROM (
            VALUES (@Id, @Name, @Email, @Amount, @Date, @IanaTimeZone, @ClientLocation)
            ) AS x("Id")
            ON CONFLICT ("Id")
            DO UPDATE SET
                "Name" = "excluded"."Name",
                "Email" = "excluded"."Email",
                "Amount" = "excluded"."Amount",
                "Date" = "excluded"."Date",
                "ClientLocation" = "excluded"."ClientLocation",
                "IanaTimeZone" = "excluded"."IanaTimeZone";
            """;
        
        await connection.ExecuteAsync(sql, records);
    }

    public async Task<List<Transaction>> Get(DateTime start, DateTime end, string ianaTimeZone)
    {
        var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZone);
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);
        
        await using var connection = await dbConnectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT "Id", "Name", "Email", "Amount", "Date" AT TIME ZONE "IanaTimeZone" AS "Date", "IanaTimeZone", "ClientLocation"
            FROM "Transactions"
            WHERE "Date" >= @StartUtc AND "Date" < @EndUtc
            """;

        var records = await connection.QueryAsync<Transaction>(sql, new {startUtc, endUtc});
        return records.ToList();
    }

    public async Task<XLWorkbook> ExportExcel(DateTime start, DateTime end, string ianaTimeZone, string[] fields)
    {
        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        PopulateColumnNamesRow();
        await PopulateTransactionRows();
        worksheet.Columns().AdjustToContents();

        return workbook;

        void PopulateColumnNamesRow()
        {
            for (var i = 0; i < fields.Length; i++)
            {
                worksheet.FirstRow().Cell(i + 1).Value = fields[i];
            }
        }

        async Task PopulateTransactionRows()
        {
            var transactions = await Get(start, end, ianaTimeZone);
            
            for (var i = 0; i < transactions.Count; i++)
            {
                for (var j = 0; j < fields.Length; j++)
                {
                    const int firstRowAndZeroIndexOffset = 2;
                    SetCellValueFromField(worksheet.Cell(i + firstRowAndZeroIndexOffset, j + 1), transactions[i], fields[j]); 
                }
            }
        }
    }
    
    private static void SetCellValueFromField(IXLCell cell, Transaction transaction, string property)
    {
        var value = GetCellValueFromField(transaction, property);
        if (value.IsDateTime)
        {
            cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
        }
        cell.SetValue(value);
    }

    private static XLCellValue GetCellValueFromField(Transaction transaction, string property)
    {
        return property switch
        {
            "transaction_id" => transaction.Id,
            "name" => transaction.Name,
            "email" => transaction.Email,
            "amount" => transaction.Amount,
            "transaction_date" => transaction.Date.DateTime,
            "client_location" => transaction.ClientLocation.ToString(),
            _ => throw TransactionManagerException.WrongTransactionField(property)
        };
    }

    private static IEnumerable<Transaction> ExtractFromCsv(IFormFile csvFile)
    {
        using var reader = new StreamReader(csvFile.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<TransactionCsvMap>();
        var records = csv.GetRecordsAsync<Transaction>().ToBlockingEnumerable().ToList();
        return records;
    }
}
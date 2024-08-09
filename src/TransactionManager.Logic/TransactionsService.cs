using System.Globalization;
using ClosedXML.Excel;
using CsvHelper;
using CsvHelper.TypeConversion;
using Dapper;
using Microsoft.AspNetCore.Http;
using TransactionManager.Data.Database;
using TransactionManager.Logic.CsvHelpers;
using TransactionManager.Logic.Exceptions;
using Transaction = TransactionManager.Data.Model.Transaction;

namespace TransactionManager.Logic;

public class TransactionsService(IDbConnectionFactory dbConnectionFactory) : ITransactionsService
{
    public static readonly string[] HeaderFields =
        ["transaction_id", "name", "email", "amount", "transaction_date", "client_location"];
    
    public async Task UploadTransactions(IFormFile csvFile)
    {
        List<Transaction> records;
        try
        {
            records = ExtractFromCsv(csvFile);
        }
        catch (TypeConverterException ex)
        {
            throw TransactionManagerException.CsvFieldParsingError(ex);
        }

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

    public async Task<XLWorkbook> ExportExcel(DateTime start, DateTime end, string ianaTimeZone, string[] fields)
    {
        TimeZoneInfo.TryFindSystemTimeZoneById(ianaTimeZone, out var timeZoneInfo);
        if (timeZoneInfo is null) throw TransactionManagerException.InvalidTimeZone(ianaTimeZone);

        var startUtc = TimeZoneInfo.ConvertTimeToUtc(start, timeZoneInfo);
        var endUtc = TimeZoneInfo.ConvertTimeToUtc(end, timeZoneInfo);


        var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        PopulateColumnNamesRow();
        await PopulateTransactionRows();
        worksheet.Columns().AdjustToContents();

        return workbook;

        void PopulateColumnNamesRow()
        {
            for (var i = 0; i < fields.Length; i++) worksheet.FirstRow().Cell(i + 1).Value = fields[i];
        }

        async Task PopulateTransactionRows()
        {
            var transactions = await FetchTransactions(startUtc, endUtc);

            for (var i = 0; i < transactions.Count; i++)
            for (var j = 0; j < fields.Length; j++)
            {
                const int zeroIndexOffset = 1;
                const int firstRowAndZeroIndexOffset = 1 + zeroIndexOffset;
                SetCellValueFromField(worksheet.Cell(i + firstRowAndZeroIndexOffset, j + zeroIndexOffset),
                    transactions[i], fields[j]);
            }
        }
    }

    private async Task<List<Transaction>> FetchTransactions(DateTime startUtc, DateTime endUtc)
    {
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

    private static void SetCellValueFromField(IXLCell cell, Transaction transaction, string field)
    {
        var value = GetCellValueFromField(transaction, field);
        if (value.IsDateTime) cell.Style.DateFormat.Format = "yyyy-MM-dd HH:mm:ss";
        cell.SetValue(value);
    }

    private static XLCellValue GetCellValueFromField(Transaction transaction, string field)
    {
        return field switch
        {
            "transaction_id" => transaction.Id,
            "name" => transaction.Name,
            "email" => transaction.Email,
            "amount" => transaction.Amount,
            "transaction_date" => transaction.Date.DateTime,
            "client_location" => transaction.ClientLocation.ToString(),
            _ => throw TransactionManagerException.WrongTransactionField(field)
        };
    }

    private static List<Transaction> ExtractFromCsv(IFormFile csvFile)
    {
        if (csvFile.Length == 0)
            throw new TransactionManagerException("Transaction", Errors.WrongFlow, "CSV Parsing Error (File is empty)",
                "The transactions CSV file is empty.");
        
        using var reader = new StreamReader(csvFile.OpenReadStream());
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<TransactionCsvMap>();
        
        csv.Read();
        csv.ReadHeader();
        var areRequiredHeadersPresent = csv.HeaderRecord!.SequenceEqual(HeaderFields);
        if (!areRequiredHeadersPresent)
            throw new TransactionManagerException("Transaction", Errors.WrongFlow, "CSV Parsing Error (Headers)",
                "The transactions CSV file must contain the following headers: " + string.Join(", ", HeaderFields));
        
        var records = csv.GetRecordsAsync<Transaction>().ToBlockingEnumerable().ToList();
        return records;
    }
}
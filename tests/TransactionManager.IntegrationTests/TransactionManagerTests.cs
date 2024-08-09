using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TransactionManager.Data.Database;
using TransactionManager.Data.Model;
using TransactionManager.IntegrationTests.Core;
using TransactionManager.Logic;
using TransactionManager.Logic.Exceptions;

namespace TransactionManager.IntegrationTests;

public class TransactionManagerTests : IClassFixture<IntegrationTestWebAppFactory>, IAsyncLifetime
{
    private readonly ITransactionsService _transactionsService;
    private readonly Func<Task> _resetDatabase;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    private static readonly string[] AllFields = TransactionsService.HeaderFields;

    public TransactionManagerTests(IntegrationTestWebAppFactory apiFactory)
    {
        _transactionsService = apiFactory.Services.GetRequiredService<ITransactionsService>();
        _resetDatabase = apiFactory.ResetDatabaseAsync;
        _dbConnectionFactory = apiFactory.DbConnectionFactory;
    }

    [Fact]
    public async Task Successfully_Writes_5_Transactions()
    {
        // Arrange
        string[] transactionStrings =
        [
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\"",
            "T-2-135.27272727272728_1.52,Brian Gregory,sed.pede@hotmail.ca,$332.05,2024-01-03 10:41:19,\"51.110318592, -77.2466440192\"",
            "T-3-202.9090909090909_2.28,September Bishop,vestibulum.neque@yahoo.ca,$762.99,2024-01-05 01:40:21,\"-1.4714172416, -142.375595008\"",
            "T-4-270.54545454545456_3.04,Kimberley Mcgee,mollis.phasellus@icloud.net,$322.12,2024-01-06 11:51:02,\"25.8747932672, -58.2515326976\"",
            "T-5-338.1818181818182_3.8,Angelica Melton,tempor@outlook.couk,$591.08,2023-12-10 01:49:25,\"10.8277878784, -49.542523904\""
        ];

        var file = CreateTransactionsCsvFile(transactionStrings);

        // Act
        await _transactionsService.UploadTransactions(file);
        // Assert

        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT COUNT(*)
            FROM "Transactions";
            """;
        var recordsCount = await connection.ExecuteScalarAsync<int>(sql);

        Assert.Equal(transactionStrings.Length, recordsCount);
    }

    [Fact]
    public async Task Omits_Duplicates_OnInsertion()
    {
        // Arrange
        string[] transactionStrings =
        [
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\"",
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\""
        ];
        var file = CreateTransactionsCsvFile(transactionStrings);

        // Act
        await _transactionsService.UploadTransactions(file);

        // Assert
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT COUNT(*)
            FROM "Transactions";
            """;
        var recordsCount = await connection.ExecuteScalarAsync<int>(sql);

        Assert.Equal(1, recordsCount);
    }
    
    [Fact]
    public async Task Doesnt_Insert_Duplicates()
    {
        // Arrange
        var file = CreateTransactionsCsvFile([
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\""
        ]);
        await _transactionsService.UploadTransactions(file);

        var fileWithDuplicate = CreateTransactionsCsvFile([
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\""
        ]);
        // Act
        await _transactionsService.UploadTransactions(fileWithDuplicate);

        // Assert
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT COUNT(*)
            FROM "Transactions";
            """;
        var recordsCount = await connection.ExecuteScalarAsync<int>(sql);

        Assert.Equal(1, recordsCount);
    }

    [Fact]
    public async Task Updates_Existing_Transactions()
    {
        // Note: There is no specific logic about which record should be left (i.e. based on timestamp), so the inserted one in taken.

        // Arrange
        var file = CreateTransactionsCsvFile([
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\""
        ]);
        await _transactionsService.UploadTransactions(file);

        const string newUpsertedName = "TEST";
        var fileWithDuplicate = CreateTransactionsCsvFile([
            $"T-1-67.63636363636364_0.76,{newUpsertedName},test@testmail.edu,$999999,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\""
        ]);

        // Act
        await _transactionsService.UploadTransactions(fileWithDuplicate);

        // Assert
        await using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        const string sql =
            """
            SELECT *
            FROM "Transactions"
            """;

        var records = await connection.QueryAsync<Transaction>(sql);

        Assert.Single(records);
        Assert.Equal(newUpsertedName, records.First().Name);
    }
    
    [Fact]
    public async Task Fails_On_Incomplete_Header()
    {
        // Arrange
        string[] transactionStrings =
        [
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\"",
        ];

        string[] incompleteHeader = ["transaction_id"]; // Note: Only 1 out of 6 required headers is present.
        var file = CreateTransactionsCsvFile(incompleteHeader, transactionStrings);

        // Act
        async Task Act() => await _transactionsService.UploadTransactions(file);
        // Assert
        var caughtException = await Assert.ThrowsAsync<TransactionManagerException>(Act);
        Assert.Equal("CSV Parsing Error (Headers)", caughtException.Title);
    }
    
    [Fact]
    public async Task Exports_Excel_Successfully()
    {
        // Arrange
        string[] transactionStrings =
        [
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\"",
            "T-2-135.27272727272728_1.52,Brian Gregory,sed.pede@hotmail.ca,$332.05,2024-01-03 10:41:19,\"51.110318592, -77.2466440192\"",
            "T-3-202.9090909090909_2.28,September Bishop,vestibulum.neque@yahoo.ca,$762.99,2024-01-05 01:40:21,\"-1.4714172416, -142.375595008\""
        ];

        var file = CreateTransactionsCsvFile(transactionStrings);
        await _transactionsService.UploadTransactions(file);
        var filterStart = new DateTime(2024, 1, 1);
        var filterEnd = new DateTime(2024, 1, 15);
        const string kyivIana = "Europe/Kiev";

        // Act
        var excelWorkbook = await _transactionsService.ExportExcel(filterStart, filterEnd, kyivIana, AllFields);

        // Assert
        Assert.NotNull(excelWorkbook);
        Assert.Equal(transactionStrings.Length,
            excelWorkbook.Worksheets.First().Rows().Count() - 1); // Note: "1" to omit columns row.
    }

    [Fact]
    public async Task Export_Respects_TimeZone()
    {
        // Arrange
        var londonLocation = new Location(51.5072, 0.1276); // - London location.
        string[] transactionStrings =
        [
            $"T-1-67.63636363636364_0.76,Sam Maxwell,sam@innitmail.com,$400.00,2023-12-31 23:00:00,\"{londonLocation}\""
        ];
        var file = CreateTransactionsCsvFile(transactionStrings);

        await _transactionsService.UploadTransactions(file);
        var filterStart = new DateTime(2024, 1, 1);
        var filterEnd = new DateTime(2024, 1, 15);
        const string kyivIana = "Europe/Kiev";

        // Act
        var excelWorkbookFilteredForKyiv = await _transactionsService.ExportExcel(
            filterStart, filterEnd, kyivIana, ["transaction_id", "transaction_date"]);
        // Assert
        Assert.Equal(transactionStrings.Length,
            excelWorkbookFilteredForKyiv.Worksheets.First().Rows().Count() - 1); // Note: "1" to omit columns row.
    }

    [Fact]
    public async Task Export_Respects_SelectedFields()
    {
        // Arrange
        string[] transactionStrings =
        [
            "T-1-67.63636363636364_0.76,Adria Pugh,odio.a.purus@protonmail.edu,$375.39,2024-01-10 01:16:23,\"6.602635264, -98.2909591552\""
        ];
        var file = CreateTransactionsCsvFile(transactionStrings);
        await _transactionsService.UploadTransactions(file);
        var filterStart = new DateTime(2024, 1, 1);
        var filterEnd = new DateTime(2024, 1, 15);
        const string kyivIana = "Europe/Kiev";

        string[] selectedFields = ["transaction_id", "name", "transaction_date"];

        // Act
        var excelWorkbookFilteredForKyiv = await _transactionsService.ExportExcel(
            filterStart, filterEnd, kyivIana, selectedFields);
        // Assert
        Assert.Equal(selectedFields.Length, excelWorkbookFilteredForKyiv.Worksheets.First().Columns().Count());
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return _resetDatabase();
    }

    private static IFormFile CreateTransactionsCsvFile(string[] headerRowFields, IEnumerable<string> lines)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.WriteLine(string.Join(',', headerRowFields));
        foreach (var line in lines) writer.WriteLine(line);
        writer.Flush();
        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "some", "test.csv");
    }
    
    private static IFormFile CreateTransactionsCsvFile(IEnumerable<string> lines)
    {
        return CreateTransactionsCsvFile(
            AllFields,
            lines);
    }
}
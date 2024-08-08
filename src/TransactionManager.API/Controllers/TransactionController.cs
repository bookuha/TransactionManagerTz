using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TransactionManager.API.Requests;
using TransactionManager.API.ValidationAttributes;
using TransactionManager.Logic;

namespace TransactionManager.API.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionController(ITransactionsService transactionsService) : ControllerBase
{
    /// <summary>
    /// Inserts transactions from a CSV (.csv) file.
    /// </summary>
    /// <remarks>
    /// Transactions are upserted, which means that if a transaction with the same ID already exists,
    /// it will be updated with newer transaction.
    /// If two or more transactions with same ID within the same file are inserted, only one is taken.
    /// </remarks>
    /// <param name="file"></param>
    /// <response code="200">Successfully inserts the transactions and returns the 0k status code.</response>
    /// <response code="400">If the file is not in .csv extension.</response>
    /// <response code="400">If the file is empty.</response>
    /// <response code="400">If the file is missing required headers.</response>
    /// /// <response code="500">If a critical error occurs.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> InsertRecordsFromCsv([AllowedFileExtensions([".csv"])] IFormFile file)
    {
        await transactionsService.UploadTransactions(file);
        return Ok();
    }

    /// <summary>
    /// Fetches and exports the transactions as an Excel (.xlsx) file.
    /// </summary>
    /// <remarks>
    /// The transactions are filtered by the specified date range and time zone.
    /// The fields to be included in the Excel file are to be specified.
    /// Supported fields are: ["transaction_id", "name", "email", "amount", "transaction_date", "client_location"]
    /// Columns in the resulting Excel file are in the actual order of the specified fields.
    /// </remarks>
    /// <param name="request"></param>
    /// <response code="200">Successfully inserts the transactions and returns the 0k status code.</response>
    /// <response code="400">If dates are invalid.</response>
    /// <response code="400">If the start date is more than the end date.</response>
    /// <response code="400">If the time zone is invalid.</response>
    /// <response code="400">If an invalid field is selected.</response>
    /// <response code="400">If duplicate fields are selected.</response>
    /// <response code="400">If no fields are selected.</response>
    /// <response code="500">If a critical error occurs.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ExportExcel([FromQuery] ExportExcelRequest request)
    {
        using var result = await transactionsService.ExportExcel(request.StartDate, request.EndDate,
            request.IanaTimeZone, request.Fields.ToArray());
        var stream = new MemoryStream(); // Note: File() will close the stream.
        result.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
    }
}
using Microsoft.AspNetCore.Mvc;
using TransactionManager.API.ValidationAttributes;
using TransactionManager.Logic;

namespace TransactionManager.API.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionController(ITransactionsService transactionsService) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult> InsertRecordsFromCsv([AllowedFileExtensions([".csv"])] IFormFile file)
    {
        await transactionsService.UploadTransactions(file);
        return Ok();
    }

    [HttpGet]
    public async Task<ActionResult> ExportExcel([FromQuery] DateTime start, [FromQuery] DateTime end,
        [FromQuery] string ianaTimeZone, [FromQuery, OnlyAllowedTransactionFields] string[] fields)
    {
        using var result = await transactionsService.ExportExcel(start, end, ianaTimeZone, fields);
        var stream = new MemoryStream(); // Note: File() will close the stream.
        result.SaveAs(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "report.xlsx");
    }
    
}



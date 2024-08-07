using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using TransactionManager.Data.Model;

namespace TransactionManager.Logic;

public interface ITransactionsService
{
    public Task UploadTransactions(IFormFile csvFile);
    public Task<XLWorkbook> ExportExcel(DateTime start, DateTime end, string ianaTimeZone, string[] fields);
}
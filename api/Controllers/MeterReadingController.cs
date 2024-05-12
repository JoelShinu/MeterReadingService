using System.Globalization;
using System.Text.RegularExpressions;
using api.Data;
using api.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("/meter-reading-uploads")]
    [ApiController]
    public class MeterReadingController(ApplicationDBContext context) : ControllerBase
    {
        private readonly MeterReadingValidationService _validationService = new MeterReadingValidationService(context);
        private readonly ApplicationDBContext _context = context;

        [HttpPost]
        public async Task<IActionResult> UploadMeterReadings(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            int successCount = 0, failureCount = 0;
            List<string> failureReasons = new List<string>();
            Dictionary<int, DateTime> latestReadingsCache = new Dictionary<int, DateTime>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    MissingFieldFound = null,
                    HeaderValidated = null,
                    IgnoreBlankLines = true
                };

                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<MeterReadingMap>();

                while (csv.Read())
                {
                    try
                    {
                        var record = csv.GetRecord<MeterReading>();
                        if (_validationService.ValidateRecord(record, latestReadingsCache, out string reason))
                        {
                            _context.MeterReadings.Add(record);
                            successCount++;
                            if (!latestReadingsCache.ContainsKey(record.AccountId) || latestReadingsCache[record.AccountId] < record.MeterReadingDateTime)
                            {
                                latestReadingsCache[record.AccountId] = record.MeterReadingDateTime;
                            }
                        }
                        else
                        {
                            failureCount++;
                            // failureReasons.Add(reason);
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        // failureReasons.Add(ex.Message);
                    }
                }
                await _context.SaveChangesAsync();
            }
            return Ok(new { SuccessCount = successCount, FailureCount = failureCount});
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAllMeterReadings()
        {
            var allRecords = _context.MeterReadings.ToList();

            if (allRecords.Count == 0)
            {
                return NotFound("No meter readings found.");
            }

            _context.MeterReadings.RemoveRange(allRecords);
            await _context.SaveChangesAsync();

            return Ok($"{allRecords.Count} records deleted successfully.");
        }

        public class MeterReadingMap : ClassMap<MeterReading>
        {
            public MeterReadingMap()
            {
                Map(m => m.AccountId).Name("AccountId");
                Map(m => m.MeterReadingDateTime).Name("MeterReadingDateTime");
                Map(m => m.MeterReadingValue).Name("MeterReadValue");
            }
        }
    }
}
using System.Text.RegularExpressions;
using api.Data;
using api.Models;

namespace api.Controllers
{
    public class MeterReadingValidationService
    {
        private readonly ApplicationDBContext _context;

        public MeterReadingValidationService(ApplicationDBContext context)
        {
            _context = context;
        }

        public bool ValidateRecord(MeterReading record, Dictionary<int, DateTime> latestReadingsCache, out string reason)
        {
            reason = "";
            return IsAccountValid(record, ref reason) &&
                IsNotDuplicate(record, ref reason) &&
                IsValidMeterReadingValue(record, ref reason) &&
                IsNotOlderThanLatest(record, latestReadingsCache, ref reason);
        }

        private bool IsAccountValid(MeterReading record, ref string reason)
        {
            if (!_context.Accounts.Any(a => a.AccountId == record.AccountId))
            {
                reason = $"No existing data for account. {record.AccountId}";
                return false;
            }
            return true;
        }

        private bool IsNotDuplicate(MeterReading record, ref string reason)
        {
            if (_context.MeterReadings.Any(mr => 
                mr.AccountId == record.AccountId &&
                mr.MeterReadingDateTime == record.MeterReadingDateTime &&
                mr.MeterReadingValue == record.MeterReadingValue))
            {
                reason = "Duplicate entry.";
                return false;
            }
            return true;
        }

        private bool IsValidMeterReadingValue(MeterReading record, ref string reason)
        {
            if (!Regex.IsMatch(record.MeterReadingValue.ToString(), @"^\d{1,5}$") || record.MeterReadingValue > 99999)
            {
                reason = $"Invalid MeterReadingValue format. Value must be a positive number up to 99999. {record.MeterReadingValue}";
                return false;
            }
            return true;
        }

        private bool IsNotOlderThanLatest(MeterReading record, Dictionary<int, DateTime> latestReadingsCache, ref string reason)
        {
            var latestReading = _context.MeterReadings
                .Where(mr => mr.AccountId == record.AccountId)
                .OrderByDescending(mr => mr.MeterReadingDateTime)
                .FirstOrDefault();

            DateTime latestDateTime = latestReading?.MeterReadingDateTime ?? DateTime.MinValue;
            if (latestReadingsCache.ContainsKey(record.AccountId))
            {
                latestDateTime = latestReadingsCache[record.AccountId] > latestDateTime ? latestReadingsCache[record.AccountId] : latestDateTime;
            }

            if (record.MeterReadingDateTime <= latestDateTime)
            {
                reason = $"New read is older than the latest existing read. Latest: {latestDateTime}, New: {record.MeterReadingDateTime}";
                return false;
            }
            return true;
        }
    }
}
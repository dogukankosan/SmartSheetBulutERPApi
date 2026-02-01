using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data;

namespace SmartSheetLicenceApi.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;
        private readonly ILogger<DatabaseService> _logger;
        private readonly bool _cachingEnabled;
        private readonly int _cacheDuration;
        public DatabaseService(IConfiguration configuration, IMemoryCache cache, ILogger<DatabaseService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cache = cache;
            _logger = logger;
            _cachingEnabled = configuration.GetValue<bool>("ApiSettings:EnableCaching", true);
            _cacheDuration = configuration.GetValue<int>("ApiSettings:CacheDurationMinutes", 5);
        }
        private async Task<IDbConnection> CreateConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
        public async Task InitializeDatabaseAsync()
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                _logger.LogInformation("Database connection successful");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection failed");
                throw;
            }
        }
        public async Task<LicenseData> GetLicenseByKeyAsync(string licenseKey)
        {
            string cacheKey = $"license_{licenseKey}";
            if (_cachingEnabled && _cache.TryGetValue(cacheKey, out LicenseData cachedLicense))
            {
                _logger.LogDebug("License retrieved from cache: {LicenseKey}", licenseKey);
                return cachedLicense;
            }
            try
            {
                using var connection = await CreateConnectionAsync();
                string query = @"
                    SELECT Id, LicenseKey, CompanyName, HardwareId, ActivationDate, 
                           ExpiryDate, IsActive, MaxUsers, CreatedDate, LastCheckDate 
                    FROM Licenses WITH (NOLOCK) 
                    WHERE LicenseKey = @LicenseKey";
                LicenseData? license = await connection.QueryFirstOrDefaultAsync<LicenseData>(query, new { LicenseKey = licenseKey });
                if (license != null && _cachingEnabled)
                {
                    MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(_cacheDuration));
                    _cache.Set(cacheKey, license, cacheOptions);
                    _logger.LogDebug("License cached: {LicenseKey}", licenseKey);
                }
                return license;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting license: {LicenseKey}", licenseKey);
                throw;
            }
        }
        public async Task<bool> ActivateLicenseAsync(string licenseKey, string hardwareId, string companyName)
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                string query = @"
                    UPDATE Licenses 
                    SET HardwareId = @HardwareId, 
                        CompanyName = @CompanyName, 
                        ActivationDate = GETDATE(),
                        ExpiryDate = DATEADD(YEAR, 1, GETDATE()),
                        LastCheckDate = GETDATE()
                    WHERE LicenseKey = @LicenseKey AND HardwareId IS NULL";
                int rowsAffected = await connection.ExecuteAsync(query, new
                {
                    LicenseKey = licenseKey,
                    HardwareId = hardwareId,
                    CompanyName = companyName
                }, commandTimeout: 30);
                if (rowsAffected > 0)
                {
                    _cache.Remove($"license_{licenseKey}");
                    _logger.LogInformation("License activated: {LicenseKey}", licenseKey);
                }
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating license: {LicenseKey}", licenseKey);
                throw;
            }
        }
        public async Task UpdateLastCheckDateAsync(string licenseKey)
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                string query = "UPDATE Licenses SET LastCheckDate = GETDATE() WHERE LicenseKey = @LicenseKey";
                await connection.ExecuteAsync(query, new { LicenseKey = licenseKey }, commandTimeout: 10);
                _cache.Remove($"license_{licenseKey}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error updating last check date: {LicenseKey}", licenseKey);
            }
        }
        public async Task<int> GetActiveLicenseCountAsync()
        {
            try
            {
                using var connection = await CreateConnectionAsync();
                string query = "SELECT COUNT(*) FROM Licenses WITH (NOLOCK) WHERE IsActive = 1";
                return await connection.ExecuteScalarAsync<int>(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active license count");
                return 0;
            }
        }
    }
    public class LicenseData
    {
        public int Id { get; set; }
        public string LicenseKey { get; set; }
        public string CompanyName { get; set; }
        public string HardwareId { get; set; }
        public DateTime? ActivationDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsActive { get; set; }
        public int MaxUsers { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastCheckDate { get; set; }
    }
}
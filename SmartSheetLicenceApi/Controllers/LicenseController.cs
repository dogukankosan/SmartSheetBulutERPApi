using Microsoft.AspNetCore.Mvc;
using SmartSheetLicenceApi.Models;
using SmartSheetLicenceApi.Services;

namespace SmartSheetLicenceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LicenseController : ControllerBase
    {
        private readonly DatabaseService _dbService;
        private readonly ILogger<LicenseController> _logger;
        public LicenseController(DatabaseService dbService, ILogger<LicenseController> logger)
        {
            _dbService = dbService;
            _logger = logger;
        }
        [HttpPost("validate")]
        public async Task<ActionResult<LicenseResponse>> ValidateLicense([FromBody] LicenseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LicenseKey) || string.IsNullOrWhiteSpace(request.HardwareId))
                {
                    _logger.LogWarning("Validation failed: Missing required fields");
                    return BadRequest(new LicenseResponse
                    {
                        Success = false,
                        Message = "LicenseKey ve HardwareId gereklidir!"
                    });
                }
                LicenseData license = await _dbService.GetLicenseByKeyAsync(request.LicenseKey);
                if (license == null)
                {
                    _logger.LogWarning("Validation failed: Invalid license key {LicenseKey}", request.LicenseKey);
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Geçersiz lisans anahtarı!"
                    });
                }
                if (!license.IsActive)
                {
                    _logger.LogWarning("Validation failed: Inactive license {LicenseKey}", request.LicenseKey);
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Lisans devre dışı bırakılmış!"
                    });
                }
                if (license.HardwareId != null && license.HardwareId != request.HardwareId)
                {
                    _logger.LogWarning("Validation failed: Hardware mismatch for {LicenseKey}", request.LicenseKey);
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Bu lisans başka bir bilgisayara kayıtlı!"
                    });
                }
                if (license.ExpiryDate.HasValue && license.ExpiryDate.Value < DateTime.Now)
                {
                    _logger.LogWarning("Validation failed: Expired license {LicenseKey}", request.LicenseKey);
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Lisans süresi dolmuş!"
                    });
                }
                _ = Task.Run(async () => await _dbService.UpdateLastCheckDateAsync(request.LicenseKey));
                _logger.LogInformation("License validated successfully: {LicenseKey}", request.LicenseKey);
                return Ok(new LicenseResponse
                {
                    Success = true,
                    Message = "Lisans geçerli!",
                    CompanyName = license.CompanyName,
                    ExpiryDate = license.ExpiryDate,
                    IsActive = license.IsActive
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Validation error for license {LicenseKey}", request.LicenseKey);
                return StatusCode(500, new LicenseResponse
                {
                    Success = false,
                    Message = "Sunucu hatası!"
                });
            }
        }
        [HttpPost("activate")]
        public async Task<ActionResult<LicenseResponse>> ActivateLicense([FromBody] ActivationRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.LicenseKey) ||
                    string.IsNullOrWhiteSpace(request.HardwareId) ||
                    string.IsNullOrWhiteSpace(request.CompanyName))
                {
                    return BadRequest(new LicenseResponse
                    {
                        Success = false,
                        Message = "Tüm alanlar gereklidir!"
                    });
                }
                LicenseData license = await _dbService.GetLicenseByKeyAsync(request.LicenseKey);
                if (license == null)
                {
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Geçersiz lisans anahtarı!"
                    });
                }
                if (license.HardwareId != null)
                {
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Bu lisans zaten aktive edilmiş!"
                    });
                }
                bool activated = await _dbService.ActivateLicenseAsync(
                    request.LicenseKey,
                    request.HardwareId,
                    request.CompanyName);
                if (activated)
                {
                    _logger.LogInformation("License activated: {LicenseKey} for {CompanyName}",
                        request.LicenseKey, request.CompanyName);
                    return Ok(new LicenseResponse
                    {
                        Success = true,
                        Message = "Lisans başarıyla aktive edildi!",
                        CompanyName = request.CompanyName,
                        ExpiryDate = DateTime.Now.AddYears(1),
                        IsActive = true
                    });
                }
                else
                {
                    return Ok(new LicenseResponse
                    {
                        Success = false,
                        Message = "Aktivasyon başarısız!"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Activation error");
                return StatusCode(500, new LicenseResponse
                {
                    Success = false,
                    Message = "Sunucu hatası!"
                });
            }
        }
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                int count = await _dbService.GetActiveLicenseCountAsync();
                return Ok(new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    activeLicenses = count
                });
            }
            catch
            {
                return StatusCode(500, new { status = "unhealthy" });
            }
        }
    }
}
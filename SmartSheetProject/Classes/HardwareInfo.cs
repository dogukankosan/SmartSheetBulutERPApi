using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace SmartSheetProject.Classes
{
    internal class HardwareInfo
    {
        /// <summary>
        /// Bilgisayara özgü benzersiz donanım ID'si döndürür
        /// CPU ID + Anakart Seri No + İlk Sabit Disk Seri No kombinasyonu
        /// </summary>
        internal  static string GetHardwareId()
        {
            try
            {
                string cpuId = GetCpuId();
                string motherboardId = GetMotherboardId();
                string diskId = GetDiskId();
                // Kombinasyonu oluştur
                string combined = $"{cpuId}-{motherboardId}-{diskId}";
                // SHA256 ile hash'le (daha kısa ve güvenli)
                return GetSHA256Hash(combined);
            }
            catch (Exception ex)
            {
                // Hata durumunda varsayılan bir ID döndür
                _= TextLog.LogToSQLiteAsync($"❌ HardwareInfo hatası: {ex.Message}");
                return "HARDWARE-ID-ERROR";
            }
        }
        /// <summary>
        /// CPU Processor ID'sini alır
        /// </summary>
        private static string GetCpuId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    string processorId = obj["ProcessorId"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(processorId))
                        return processorId.Trim();
                }
                return "CPU-UNKNOWN";
            }
            catch
            {
                return "CPU-ERROR";
            }
        }
        /// <summary>
        /// Anakart seri numarasını alır
        /// </summary>
        private static string GetMotherboardId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    string serialNumber = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(serialNumber))
                        return serialNumber.Trim();
                }
                return "MOBO-UNKNOWN";
            }
            catch
            {
                return "MOBO-ERROR";
            }
        }
        /// <summary>
        /// İlk fiziksel diskin seri numarasını alır
        /// </summary>
        private static string GetDiskId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_PhysicalMedia");
                ManagementObjectCollection collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    string serialNumber = obj["SerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(serialNumber))
                        return serialNumber.Trim();
                }
                return "DISK-UNKNOWN";
            }
            catch
            {
                return "DISK-ERROR";
            }
        }
        /// <summary>
        /// String'i SHA256 hash'e çevirir
        /// </summary>
        private static string GetSHA256Hash(string input)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(input);
                    byte[] hash = sha256.ComputeHash(bytes);
                    // Hash'i hex string'e çevir
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hash)
                        sb.Append(b.ToString("x2"));
                    return sb.ToString().ToUpper();
                }
            }
            catch
            {
                return input; // Hash başarısız olursa düz string döndür
            }
        }
        /// <summary>
        /// Donanım bilgilerini detaylı olarak döndürür (debug için)
        /// </summary>
        internal static string GetDetailedHardwareInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"CPU ID: {GetCpuId()}");
            sb.AppendLine($"Motherboard ID: {GetMotherboardId()}");
            sb.AppendLine($"Disk ID: {GetDiskId()}");
            sb.AppendLine($"Hardware ID (SHA256): {GetHardwareId()}");
            return sb.ToString();
        }
        /// <summary>
        /// Kısa formatta Hardware ID döndürür (ilk 16 karakter)
        /// </summary>
        internal static string GetShortHardwareId()
        {
            string fullId = GetHardwareId();
            return fullId.Length > 16 ? fullId.Substring(0, 16) : fullId;
        }
    }
}
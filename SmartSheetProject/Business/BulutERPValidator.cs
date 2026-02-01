using System;
using System.Text.RegularExpressions;

namespace SmartSheetProject.Classes
{
    internal static class BulutERPValidator
    {
        /// <summary>
        /// Tüm Bulut ERP ayarlarını doğrular
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateSettings(
            string clientId,
            string clientSecret,
            string username,
            string password,
            string firmNr,
            string serverUrl,
            string machineID)
        {
            // Client ID kontrolü
            var clientIdResult = ValidateClientId(clientId);
            if (!clientIdResult.IsValid)
                return clientIdResult;

            // Client Secret kontrolü
            var clientSecretResult = ValidateClientSecret(clientSecret);
            if (!clientSecretResult.IsValid)
                return clientSecretResult;

            // Kullanıcı adı kontrolü
            var usernameResult = ValidateUsername(username);
            if (!usernameResult.IsValid)
                return usernameResult;

            // Şifre kontrolü
            var passwordResult = ValidatePassword(password);
            if (!passwordResult.IsValid)
                return passwordResult;

            // Firma No kontrolü
            var firmNrResult = ValidateFirmNr(firmNr);
            if (!firmNrResult.IsValid)
                return firmNrResult;

            // Sunucu Adresi kontrolü
            var serverUrlResult = ValidateServerUrl(serverUrl);
            if (!serverUrlResult.IsValid)
                return serverUrlResult;

            // Machine ID kontrolü
            var machineIDResult = ValidateMachineID(machineID);
            if (!machineIDResult.IsValid)
                return machineIDResult;

            return (true, null);
        }

        /// <summary>
        /// Client ID kontrolü - GUID formatında gelir (örn: 6635dc29-f019-435b-9813-99f044bedd16)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateClientId(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
                return (false, "Client ID boş olamaz!");
            // GUID formatı kontrolü (basit)
            if (clientId.Trim().Length < 32)
                return (false, "Client ID geçersiz format!");
            return (true, null);
        }

        /// <summary>
        /// Client Secret kontrolü - GUID formatında gelir (örn: a85ee555-a8d4-43c7-99f0-bdd7a9db969b)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateClientSecret(string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientSecret))
                return (false, "Client Secret boş olamaz!");
            // GUID formatı kontrolü (basit)
            if (clientSecret.Trim().Length < 32)
                return (false, "Client Secret geçersiz format!");
            return (true, null);
        }

        /// <summary>
        /// Kullanıcı adı kontrolü - Email formatında gelir (örn: malaca@alkoprocess.com)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Kullanıcı Adı boş olamaz!");
            return (true, null);
        }

        /// <summary>
        /// Şifre kontrolü - Özel karakterli olabilir (örn: M.alko050*)
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Şifre boş olamaz!");
            if (password.Length < 3)
                return (false, "Şifre en az 3 karakter olmalıdır!");
            return (true, null);
        }

        /// <summary>
        /// Firma No kontrolü - Genelde 1, 10, 999 gibi değerler gelir
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateFirmNr(string firmNr)
        {
            if (string.IsNullOrWhiteSpace(firmNr))
                return (false, "Firma Numarası boş olamaz!");
            // Sadece rakam kontrolü
            if (!Regex.IsMatch(firmNr.Trim(), @"^\d+$"))
                return (false, "Firma Numarası sadece rakamlardan oluşmalıdır!");
            int firmNumber;
            if (!int.TryParse(firmNr.Trim(), out firmNumber))
                return (false, "Firma Numarası geçersiz!");
            if (firmNumber <= 0)
                return (false, "Firma Numarası 0'dan büyük olmalıdır!");
            return (true, null);
        }

        /// <summary>
        /// Sunucu Adresi (URL) kontrolü - Genelde https://apigateway.logo.cloud gelir
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateServerUrl(string serverUrl)
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
                return (false, "Sunucu Adresi boş olamaz!");
            // Trim işlemi
            serverUrl = serverUrl.Trim();
            // URL formatı kontrolü
            if (!serverUrl.StartsWith("http://") && !serverUrl.StartsWith("https://"))
                return (false, "Sunucu Adresi http:// veya https:// ile başlamalıdır!");
            // URL geçerliliği kontrolü
            if (!Uri.TryCreate(serverUrl, UriKind.Absolute, out Uri uriResult))
                return (false, "Sunucu Adresi geçersiz bir URL formatındadır!");
            return (true, null);
        }

        /// <summary>
        /// Machine ID kontrolü - Genelde ERP-10 gibi değerler gelir
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateMachineID(string machineID)
        {
            if (string.IsNullOrWhiteSpace(machineID))
                return (false, "Machine ID boş olamaz!");
            machineID = machineID.Trim();
            if (machineID.Length < 2)
                return (false, "Machine ID en az 2 karakter olmalıdır!");
            if (machineID.Length > 100)
                return (false, "Machine ID maksimum 100 karakter olabilir!");
            return (true, null);
        }

        /// <summary>
        /// SQL sorgusu için firma numarasını formatlar (PADDING YOK - Olduğu gibi kullan)
        /// </summary>
        public static string FormatFirmNr(string firmNr)
        {
            if (string.IsNullOrWhiteSpace(firmNr))
                return "1";
            // Rakam değilse varsayılan döndür
            if (!int.TryParse(firmNr.Trim(), out int number))
                return "1";
            // PADDING YOK - Olduğu gibi döndür (1 → "1", 10 → "10", 999 → "999")
            return number.ToString();
        }
    }
}
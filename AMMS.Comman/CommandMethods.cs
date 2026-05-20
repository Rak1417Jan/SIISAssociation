namespace MVEA.Comman
{    
    public static class PasswordHasher
    {
        public static bool Verify(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(storedSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(storedHash);
        }
    }
    public static class CommandMethods
    {
        public static PasswordHashResult ConvertToHashResult(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be empty.", nameof(password));

            // Generate random salt
            var salt = new byte[16];
            System.Security.Cryptography.RandomNumberGenerator.Fill(salt);

            // Derive hash using PBKDF2
            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                password, salt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256);

            var hash = pbkdf2.GetBytes(32);

            return new PasswordHashResult
            {
                PasswordHash = hash,
                PasswordSalt = salt
            };
        }
        public static bool ValidatePassword(string password, PasswordHashResult stored)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            using var pbkdf2 = new System.Security.Cryptography.Rfc2898DeriveBytes(
                password, stored.PasswordSalt, 100_000, System.Security.Cryptography.HashAlgorithmName.SHA256);

            var computedHash = pbkdf2.GetBytes(32);

            // Compare byte arrays
            return computedHash.SequenceEqual(stored.PasswordHash);
        }
        public class PasswordHashResult
        {
            public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

            public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        }
    }
}

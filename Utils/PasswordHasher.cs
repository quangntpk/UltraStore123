using System.Security.Cryptography;
using System.Text;

namespace UltraStrore.Utils
{
    public class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Hash the entered password using the same hashing algorithm
            string hashedEnteredPassword = HashPassword(enteredPassword);

            // Compare the newly hashed password with the stored hash
            return string.Equals(hashedEnteredPassword, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
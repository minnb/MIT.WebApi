using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MIT.Utils
{
    public static class RsaUtils
    {
        private const int KEY_SIZE = 2048; // The size of the RSA key to use in bits.
        private static bool fOAEP = false;
        private static RSACryptoServiceProvider rsaProvider = null;
        public static string Sha256(string data)
        {
            using (var sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Convert byte array to a string   
                var builder = new StringBuilder();
                foreach (var t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        //public static string Sha256(string data)
        //{
        //    using (var sha256Hash = SHA256.Create())
        //    {
        //        // ComputeHash - returns byte array  
        //        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

        //        // Convert byte array to a string   
        //        var builder = new StringBuilder();
        //        foreach (var t in bytes)
        //        {
        //            builder.Append(t.ToString("x2"));
        //        }
        //        return builder.ToString();
        //    }
        //}
        public static string Sha1(string data)
        {
            //using (var sha256Hash = SHA1.Create())
            //{
            //    // ComputeHash - returns byte array  
            //    var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

            //    // Convert byte array to a string   
            //    var builder = new StringBuilder();
            //    foreach (var t in bytes)
            //    {
            //        builder.Append(t.ToString("x2"));
            //    }
            //    return builder.ToString();
            //}

            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(data));
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    // can be "x2" if you want lowercase
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }

        private static CspParameters GetCspParameters()
        {
            // Create a new key pair on target CSP
            CspParameters cspParams = new CspParameters();
            cspParams.ProviderType = 1; // PROV_RSA_FULL 
            // cspParams.ProviderName; // CSP name
            // cspParams.Flags = CspProviderFlags.UseArchivableKey;
            cspParams.KeyNumber = (int)KeyNumber.Exchange;

            return cspParams;
        }

        public static int GetMaxDataLength()
        {
            if (fOAEP)
                return ((KEY_SIZE - 384) / 8) + 7;
            return ((KEY_SIZE - 384) / 8) + 37;
        }

        public static bool IsKeySizeValid()
        {
            return KEY_SIZE >= 384 &&
                   KEY_SIZE <= 16384 &&
                   KEY_SIZE % 8 == 0;
        }

        public static void GenerateKeys(out string publicKey, out string privateKey)
        {
            try
            {
                CspParameters cspParams = GetCspParameters();
                cspParams.Flags = CspProviderFlags.UseArchivableKey;

                rsaProvider = new RSACryptoServiceProvider(KEY_SIZE, cspParams);

                // Export public key
                publicKey = rsaProvider.ToXmlString(false);

                // Export private/public key pair 
                privateKey = rsaProvider.ToXmlString(true);
            }
            catch (Exception ex)
            {
                // Any errors? Show them
                throw new Exception("Exception generating a new RSA key pair! More info: " + ex.Message);
            }
            finally
            {
                // Do some clean up if needed
            }

        } // GenerateKeys method
        public static string RSAEncrypt(string publicKey, string plainText)
        {
            if (string.IsNullOrWhiteSpace(plainText))
                throw new ArgumentException("Data are empty");

            int maxLength = GetMaxDataLength();
            if (Encoding.Unicode.GetBytes(plainText).Length > maxLength)
                throw new ArgumentException("Maximum data length is " + maxLength / 2);

            if (!IsKeySizeValid())
                throw new ArgumentException("Key size is not valid");

            if (string.IsNullOrWhiteSpace(publicKey))
                throw new ArgumentException("Key is null or empty");

            byte[] plainBytes = null;
            byte[] encryptedBytes = null;
            string encryptedText = "";

            try
            {
                CspParameters cspParams = GetCspParameters();
                cspParams.Flags = CspProviderFlags.NoFlags;

                rsaProvider = new RSACryptoServiceProvider(KEY_SIZE, cspParams);

                // [1] Import public key
                rsaProvider.FromXmlString(publicKey);

                // [2] Get plain bytes from plain text
                plainBytes = Encoding.Unicode.GetBytes(plainText);

                // Encrypt plain bytes
                encryptedBytes = rsaProvider.Encrypt(plainBytes, false);

                // Get encrypted text from encrypted bytes
                // encryptedText = Encoding.Unicode.GetString(encryptedBytes); => NOT WORKING
                encryptedText = Convert.ToBase64String(encryptedBytes);
            }
            catch (Exception ex)
            {
                // Any errors? Show them
                throw new Exception("Exception encrypting file! More info: " + ex.Message);
            }
            finally
            {
                // Do some clean up if needed
            }

            return encryptedText;

        } // Encrypt method
        public static string RSADecrypt(string privateKey, string encryptedText)
        {
            if (string.IsNullOrWhiteSpace(encryptedText))
                throw new ArgumentException("Data are empty");

            if (!IsKeySizeValid())
                throw new ArgumentException("Key size is not valid");

            if (string.IsNullOrWhiteSpace(privateKey))
                throw new ArgumentException("Key is null or empty");

            byte[] encryptedBytes = null;
            byte[] plainBytes = null;
            string plainText = "";

            try
            {
                CspParameters cspParams = GetCspParameters();
                cspParams.Flags = CspProviderFlags.NoFlags;

                rsaProvider = new RSACryptoServiceProvider(KEY_SIZE, cspParams);

                // [1] Import private/public key pair
                rsaProvider.FromXmlString(privateKey);

                // [2] Get encrypted bytes from encrypted text
                // encryptedBytes = Encoding.Unicode.GetBytes(encryptedText); => NOT WORKING
                encryptedBytes = Convert.FromBase64String(encryptedText);

                // Decrypt encrypted bytes
                plainBytes = rsaProvider.Decrypt(encryptedBytes, false);

                // Get decrypted text from decrypted bytes
                plainText = Encoding.Unicode.GetString(plainBytes);
            }
            catch (Exception ex)
            {
                // Any errors? Show them
                throw new Exception("Exception decrypting file! More info: " + ex.Message);
            }
            finally
            {
                // Do some clean up if needed
            }

            return plainText;

        } // Decrypt method
    }
}

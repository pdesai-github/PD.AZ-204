using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Storage.Blobs;
using System.Text;

namespace KeyVaultApp
{
    internal class Program
    {
        static string stotageAccConnStr = "";
               
        static KeyClient keyClient = new KeyClient(new Uri("https://pdkeyvaultaz204.vault.azure.net/"), new DefaultAzureCredential());
        static string keyName = "az204storageacc";

        static async Task Main(string[] args)
        {
            string encryptedValue = await Encrypt();
            //Console.WriteLine(encryptedValue);
            string decryptedValue = await Decrypt(encryptedValue);
            //Console.WriteLine(decryptedValue);

            await UploadBlob();
        }

        private static async Task UploadBlob()
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(stotageAccConnStr);
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient("keyvaultdemo");

            using (FileStream fs = File.OpenRead("sample.txt"))
            {
                await blobContainerClient.UploadBlobAsync("sample.txt", fs);
                Console.WriteLine("File uploaded");
            }
        }

        private static async Task<string> Encrypt()
        {
            var byteData = Encoding.Unicode.GetBytes(stotageAccConnStr);
            var encryptedValue = await keyClient.GetCryptographyClient(keyName).EncryptAsync(EncryptionAlgorithm.RsaOaep, byteData);
            return Convert.ToBase64String(encryptedValue.Ciphertext);
        }

        private static async Task<string> Decrypt(string encryptedValue)
        {
            var bytedata = Convert.FromBase64String(encryptedValue);
            var decrypted = await keyClient.GetCryptographyClient(keyName).DecryptAsync(EncryptionAlgorithm.RsaOaep, bytedata);
            return Encoding.Unicode.GetString(decrypted.Plaintext);
        }
    }
}

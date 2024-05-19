using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace BlobStorageApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string storageAccountConnStr = "{CONNECTION_STRING}";
            const string imageContainerName = "imagecontainer";
            const string fileContainerName = "filecontainer";
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            /* Used for storage account level tasks like get Containres */
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageAccountConnStr);

            //await UploadAndListImageBlobs(imageContainerName, currentDirectory, blobServiceClient);
            //await UploadAndListFileBlobs(fileContainerName, currentDirectory, blobServiceClient);
            //await UpdateTextBlob(fileContainerName, blobServiceClient,"New content");

        }

        private static async Task UpdateTextBlob(string fileContainerName, BlobServiceClient blobServiceClient, string newFileContent)
        {
            BlobContainerClient textContainerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);
            BlobClient blobClient = textContainerClient.GetBlobClient("Resume.txt");
            if (await blobClient.ExistsAsync())
            {
                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(newFileContent)))
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                    Console.WriteLine("Resume updated");
                }
            }
        }

        private static async Task UploadAndListFileBlobs(string fileContainerName, string currentDirectory, BlobServiceClient blobServiceClient)
        {
            BlobContainerClient fileContainerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);
            await fileContainerClient.CreateIfNotExistsAsync();
            using (FileStream textFileStream = File.OpenRead(Path.Combine(currentDirectory, "Files", "Resume.txt")))
            {
                await fileContainerClient.UploadBlobAsync("Resume.txt", textFileStream);
            }
            Console.WriteLine("List of file blobs");
            await foreach (BlobItem item in fileContainerClient.GetBlobsAsync())
            {
                Console.WriteLine(item.Name);
            }
        }

        private static async Task UploadAndListImageBlobs(string imageContainerName, string currentDirectory, BlobServiceClient blobServiceClient)
        {
            /* Get Container client using Storage account service */
            BlobContainerClient imageContainerClient = blobServiceClient.GetBlobContainerClient(imageContainerName);
            await imageContainerClient.CreateIfNotExistsAsync();

            using (FileStream imageFileStream = File.OpenRead(Path.Combine(currentDirectory, "Images", "bike.jpg")))
            {
                await imageContainerClient.UploadBlobAsync("bike.jpg", imageFileStream);
            }

            Console.WriteLine("List of image blobs");
            await foreach (BlobItem item in imageContainerClient.GetBlobsAsync())
            {
                Console.WriteLine(item.Name);
            }
        }
    }
}

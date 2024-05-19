using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Reflection;
using System.Text;

namespace BlobStorageApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            const string storageAccountConnStr = "{}";
            const string imageContainerName = "imagecontainer";
            const string fileContainerName = "filecontainer";
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            /* Used for storage account level tasks like get Containres */
            BlobServiceClient blobServiceClient = new BlobServiceClient(storageAccountConnStr);

            await UploadAndListImageBlobs(imageContainerName, currentDirectory, blobServiceClient);
            await UploadAndListFileBlobs(fileContainerName, currentDirectory, blobServiceClient);
            await UpdateTextBlob(fileContainerName, blobServiceClient, "New content");
            await ListAllContainersAndBlobs(blobServiceClient);
            await DownloadAllBlobs(blobServiceClient);
            await DeleteBlobAndContainer(imageContainerName, blobServiceClient);
            await GetBlobProperties(fileContainerName, blobServiceClient);
            await SetBlobMetadata(fileContainerName, blobServiceClient);
            await GetBlobMetadata(fileContainerName, blobServiceClient);
        }

        private static async Task GetBlobMetadata(string fileContainerName, BlobServiceClient blobServiceClient)
        {
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient("Resume.txt");
            BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
            IDictionary<string, string> metadata = blobProperties.Metadata;
            foreach (var item in metadata.Keys)
            {
                Console.WriteLine($"Key : {item} Value : {metadata[item]}");
            }
        }

        private static async Task SetBlobMetadata(string fileContainerName, BlobServiceClient blobServiceClient)
        {
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient("Resume.txt");
            Dictionary<string, string> metadata = new Dictionary<string, string>();
            metadata.Add("Position", "Software architect");
            metadata.Add("Gender", "Male");
            await blobClient.SetMetadataAsync(metadata);
            Console.WriteLine("Metadata added");
        }

        private static async Task GetBlobProperties(string fileContainerName, BlobServiceClient blobServiceClient)
        {
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(fileContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient("Resume.txt");
            BlobProperties blobProperties = await blobClient.GetPropertiesAsync();
            PropertyInfo[] properties = blobProperties.GetType().GetProperties();
            foreach (PropertyInfo property in properties)
            {
                Console.WriteLine($"Name : {property.Name} Value : {property.GetValue(blobProperties, null)}");
            }
        }

        private static async Task DeleteBlobAndContainer(string imageContainerName, BlobServiceClient blobServiceClient)
        {
            BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(imageContainerName);
            BlobClient blobClient = blobContainerClient.GetBlobClient("bike.jpg");
            await blobClient.DeleteIfExistsAsync();
            Console.WriteLine("Blob deleted");
            await blobContainerClient.DeleteIfExistsAsync();
            Console.WriteLine("Container deleted");
        }

        private static async Task DownloadAllBlobs(BlobServiceClient blobServiceClient)
        {
            Console.WriteLine("Downloading all blobs");
            await foreach (BlobContainerItem containerItem in blobServiceClient.GetBlobContainersAsync())
            {
                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerItem.Name);
                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    string downloadFilePath = Path.Combine(Environment.CurrentDirectory, "Downloads", blobItem.Name);
                    Directory.CreateDirectory(Path.GetDirectoryName(downloadFilePath));

                    BlobClient blobClient = blobContainerClient.GetBlobClient(blobItem.Name);
                    await blobClient.DownloadToAsync(downloadFilePath);
                }
            }
        }

        private static async Task ListAllContainersAndBlobs(BlobServiceClient blobServiceClient)
        {
            await foreach (BlobContainerItem containerItem in blobServiceClient.GetBlobContainersAsync())
            {
                Console.WriteLine("Showing all the containers and blobs");
                Console.WriteLine($"Container : {containerItem.Name}");

                BlobContainerClient blobContainerClient = blobServiceClient.GetBlobContainerClient(containerItem.Name);
                await foreach (BlobItem blobItem in blobContainerClient.GetBlobsAsync())
                {
                    Console.WriteLine($"--Blob : {blobItem.Name}");
                }
            }
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

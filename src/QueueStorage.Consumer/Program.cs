using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Nanny.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace QueueStorage.Consumer
{
    class Program
    {
        static string _gs;
        static string _tempFolder;
        static string _resolution;

        static JsonConfiguration settings;

        static CloudStorageAccount _storageAccount;
        static CloudQueueClient _queueClient;
        static CloudQueue _queue;
        static CloudQueueMessage _message;

        static SemaphoreSlim _semaphore;
        static DateTime _lastExecution;        

        static void Main(string[] args)
        {
            Initialize();

            while (true)
            {                 

                ProcessJob();

                _semaphore.Wait();
                if ((DateTime.Now - _lastExecution).Seconds <= int.Parse("1"))
                    break;
                else
                {
                    Thread.Sleep(2000);
                    _semaphore.Release();
                }
            }

        }

        private async static void ProcessJob()
        {
            _lastExecution = DateTime.Now;
            await _semaphore.WaitAsync();

            try
            {
                _message = GetMessage();

                if (_message == null)
                    return;

                var pdfFileUrl = _message.AsString;

                Console.WriteLine($"Downloading resource - {pdfFileUrl}");

                var pdfTempFile = string.Empty;

                try
                {
                    pdfTempFile = DownloadTempPdf(pdfFileUrl, _tempFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    Console.WriteLine($"Error: {ex.InnerException}");
                }

                if (!String.IsNullOrWhiteSpace(pdfTempFile))
                {
                    Stream[] pdfPageImageList = null;

                    using (var pdfInput = File.OpenRead(pdfTempFile))
                    {
                        Console.WriteLine("Generating Image stream array");
                        PdfImageConverter imageConverter = new PdfImageConverter(_gs, _tempFolder, _resolution);

                        try
                        {
                            //The array of streams will respect the page number-1, page 1 equal index 0;
                            imageConverter.GenerateImage(pdfInput, ref pdfPageImageList);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error generating pdf images {ex.Message}");
                        }
                    }

                    if (pdfPageImageList == null)
                        Console.WriteLine($"No Pages was generated!");

                    else
                    {
                        Console.WriteLine($"Uploading {pdfPageImageList.Length} Images to final Storage Account");
                        FileInfo info = new FileInfo(pdfTempFile);

                        try
                        {
                            UploadImages(pdfPageImageList, info.Name.ToUpper().Replace(".PDF", ""));
                            await _queue.DeleteMessageAsync(_message);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error Uploading Images: {ex.Message}");
                        }

                    }

                    try
                    {
                        File.Delete(pdfTempFile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error trying to delete the temp file {pdfTempFile}: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"PDF {pdfFileUrl} is being process!");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static void Initialize()
        {
            _semaphore = new SemaphoreSlim(1, 1);
            settings = JsonConfiguration.Build("./settings.json");

            _gs = $@"{ settings["GS_BIN"]}";
            _tempFolder = $@"{ settings["TEMP_FOLDER"]}";
            _resolution = (int.Parse( settings["IMAGE_RESOLUTION"]) / 10).ToString();

            if (!Directory.Exists(_tempFolder))
                Directory.CreateDirectory(_tempFolder);

            var connectionString =  settings["STORAGE_ACCOUNT"];
            var queueName =  settings["QUEUE_NAME"];

            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentException("connectionString is empty");

            try
            {
                _storageAccount = CloudStorageAccount.Parse(connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing the storage connection string {connectionString}: {ex.Message}");
            }

            _queueClient = _storageAccount.CreateCloudQueueClient();
            _queue = GetQueueAsync(queueName).GetAwaiter().GetResult();
        }

        private static async Task<CloudQueue> GetQueueAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is empty!");
            if (_queueClient == null) throw new Exception("_queueClient is null!");

            var queue = _queueClient.GetQueueReference(name);

            var exists = await queue.ExistsAsync();
            if (!exists)
            {
                throw new Exception($"Error queue {name} doesn't exists!");
            }

            return queue;
        }

        private static CloudQueueMessage GetMessage()
        {
            var options = new QueueRequestOptions();
            return _queue.GetMessageAsync(TimeSpan.FromMinutes(10), options, null).GetAwaiter().GetResult();

        }

        private static void UploadImages(Stream[] pdfPageImageList, string filename)
        {
            //Upload Pages in Patch
            var container = GetContainer( settings["IMAGE_STORAGE_ACCOUNT"],  settings["IMAGE_CONTAINER_NAME"]);

            container.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            container.SetPermissionsAsync(new BlobContainerPermissions()
            {
                PublicAccess = BlobContainerPublicAccessType.Container
            }).GetAwaiter().GetResult();


            var tasks = new List<Task>();
            for (int page = 0; page < pdfPageImageList.Length; page++)
            {
                var blobName = $@"{filename}\page_{page + 1}.jpg";
                Console.WriteLine($"Blob: {blobName}");
                var blob = container.GetBlockBlobReference(blobName);
                //tasks.Add(blob.UploadFromStreamAsync(pdfPageImageList[page]));

                try
                {
                    blob.UploadFromStreamAsync(pdfPageImageList[page]).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error uploading image {ex.Message} ");
                }
            }
            //try
            //{
            //    Task.WaitAll(tasks.ToArray());
            //}catch(Exception ex)
            //{
            //    Console.WriteLine($"Error uploading image {ex.Message} ");
            //}

        }

        private static CloudBlobContainer GetContainer(string connectionString, string containerName)
        {
            string conn = connectionString;

            var storageAccount = CloudStorageAccount.Parse(conn);
            var client = storageAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(containerName);

            return container;
        }

        private static string DownloadTempPdf(string pdfFileUrl, string tempFolder)
        {
            var fileUri = new Uri(pdfFileUrl);
            var fileName = $@"{tempFolder}/{fileUri.Segments[fileUri.Segments.Length - 1]}";

            if (File.Exists(fileName))
                return string.Empty;

            using (var client = new WebClient())
            {
                client.DownloadFile(fileUri, fileName);
            }

            return fileName;
        }
    }
}

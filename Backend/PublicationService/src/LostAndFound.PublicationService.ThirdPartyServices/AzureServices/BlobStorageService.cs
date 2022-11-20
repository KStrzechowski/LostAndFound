﻿using Azure.Storage.Blobs;
using LostAndFound.PublicationService.ThirdPartyServices.AzureServices.Interfaces;

namespace LostAndFound.PublicationService.ThirdPartyServices.AzureServices
{
    public class BlobStorageService : IFileStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobStorageService(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        //public async Task UploadFileAsync()
        //{
        //    var containerClient = _blobServiceClient.GetBlobContainerClient("profile-pictures");

        //    var blobClient = containerClient.GetBlobClient("");

        //    //await blobClient.UploadAsync()

        //    throw new NotImplementedException();
        //}
    }
}

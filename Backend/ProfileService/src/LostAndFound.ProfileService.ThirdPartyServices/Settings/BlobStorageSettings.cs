﻿namespace LostAndFound.ProfileService.ThirdPartyServices.Settings
{
    public class BlobStorageSettings
    {
        private const string settingName = "LostAndFoundBlobStorageSettings";

        public static string SettingName => settingName;
        public string ConnectionString { get; set; } = string.Empty;
        public string ProfilePicturesContainerName { get; set; } = string.Empty;
    }
}

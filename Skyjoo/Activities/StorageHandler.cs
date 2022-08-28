using Skyjoo.Images;
using System;
using Xamarin.Essentials;

namespace Skyjoo.Storage
{
    public class StorageHandler
    {
        public StorageHandler()
        {

        }

        public void SetName(string name)
        {
            if (name != DeviceInfo.Name)
                SecureStorage.SetAsync("username", name);
        }

        public string GetName()
        {
            try
            {
                return SecureStorage.GetAsync("username").Result ?? DeviceInfo.Name;
            }
            catch (Exception)
            {
                return DeviceInfo.Name;
            }
        }
        public void SetIconPack(IconPack pack)
        {
            SecureStorage.SetAsync("iconPack", pack.ToString());
        }

        public IconPack GetIconPack()
        {
            try
            {
                return (IconPack)Enum.Parse(typeof(IconPack), SecureStorage.GetAsync("iconPack").Result);
            }
            catch (Exception)
            {
                return IconPack.Default;
            }
        }
    }
}
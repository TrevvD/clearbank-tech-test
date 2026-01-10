using System.Configuration;

namespace ClearBank.DeveloperTest.Data
{
    public class AccountDataStoreFactory : IAccountDataStoreFactory
    {
        public IAccountDataStore Create()
        {
            var dataStoreType = ConfigurationManager.AppSettings["DataStoreType"];

            if (dataStoreType == "Backup")
            {
                return new BackupAccountDataStore();
            }

            return new AccountDataStore();
        }
    }
}

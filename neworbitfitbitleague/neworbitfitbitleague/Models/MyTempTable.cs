using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Models
{
    public class MyTempTable : TableEntity
    {
        public string TempKey { get; set; }
    }
}
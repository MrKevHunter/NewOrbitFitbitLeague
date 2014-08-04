using Microsoft.WindowsAzure.Storage.Table;

namespace neworbitfitbitleague.Controllers
{
    public class MyTempTable : TableEntity
    {
        public string TempKey { get; set; }
    }
}
using System.Data;
using Monitor.Providers;
using Xunit;

public class ExcelFileUrlProviderTests
{
    [Fact]
    void testProviderProgressAndFinish()
    {
        var provider = new excelFileSiteDataProvider(@"../../../TestData/SiteList.xlsx");
        string prev = null;
        string curr = provider.nextField(0);
        while (curr != null)
        {
            Assert.NotEqual(prev, curr);
            prev = curr;
            curr = provider.nextField(0);
        }
        Assert.Null(curr);
    }
}
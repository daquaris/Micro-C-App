using MicroCLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MicroCLib.Tests
{
    [TestClass]
    public class SearchTests
    {
        SearchResults results;

        const string QUERY = "powerspec desktop";
        const string STORE = "141";

        public SearchTests()
        {
            results = Search.LoadQuery(QUERY, STORE, null, Search.OrderByMode.pricelow, 1, token: null).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ResultsExist()
        {
            Assert.IsNotNull(results);
        }

        [TestMethod]
        public void ResultsGotItems()
        {
            Assert.IsTrue(results.Items.Count > 0);
        }

        [TestMethod]
        public void ResultsGotTotalResults()
        {
            Assert.IsTrue(results.TotalResults > 0 && results.TotalResults >= results.Items.Count);
        }

        [TestMethod]
        public void ResultsItemsHaveFields()
        {
            //this should be broken into individual functions probably...
            foreach(var item in results.Items)
            {
                Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Name));
                Assert.IsTrue(!string.IsNullOrWhiteSpace(item.ID) && (item.ID.Length == 6 || item.ID.Length == 7));
                Assert.IsTrue(!string.IsNullOrWhiteSpace(item.SKU) && item.SKU.Length == 6);
                Assert.IsTrue(item.Price > 0);
                Assert.IsTrue(item.OriginalPrice > 0);
                Assert.IsNotNull(item.PictureUrls);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Stock));
                //
                // Brand can actually be empty ex https://www.microcenter.com/product/5003878/-amd-ryzen-5-3600-with-wraith-stealth-cooler,-asus-b450m-a-csm-prime,-cpu---motherboard-bundle
                // CPU+Mobo Bundles
                //Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Brand));
                //
                //
                Assert.IsTrue(!string.IsNullOrWhiteSpace(item.URL));
                Assert.IsTrue(Regex.Match(item.URL, "/product/\\d{6,7}/.*").Success);
            }
        }

        [TestMethod]
        public void ResultsHaveClearance()
        {
            Assert.IsTrue(results.Items.Any(i => i.ClearanceItems.Count > 0));
            Assert.IsTrue(results.Items.Any(i => i.ClearanceItems.Any(c => c.Price > 0)));
        }

        [TestMethod]
        public async Task ResultsLoadAll()
        {
            var allResults = await Search.LoadAll(QUERY, STORE, null, Search.OrderByMode.pricelow, token: null);
            Assert.IsTrue(allResults.Items.Count == allResults.TotalResults);
        }

        // Same staleness problem as ItemTests originally had (see that class): these were 2020-era
        // SKU/UPC values for products that are long discontinued, so the search for them fell back
        // to unrelated filler results instead of resolving to a single match, and LoadFast (which
        // requires exactly a real, positive TotalResults) returned null instead of an item. Replaced
        // with the same evergreen store-brand and name-brand USB flash drives used for the Item.cs
        // fixture - cheap, generic products unlikely to ever go out of stock or get discontinued -
        // and verified live that each resolves to exactly one search result.
        [TestMethod]
        public async Task LoadFastSKU()
        {
            var item = await Search.LoadFast("482091", STORE);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.URL));
        }

        [TestMethod]
        public async Task LoadFastUPC()
        {
            var item = await Search.LoadFast("023942490715", STORE);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.URL));
        }

        [TestMethod]
        public async Task LoadFastFail()
        {
            var item = await Search.LoadFast("garbage", STORE);
            Assert.IsNull(item);
        }

        [TestMethod]
        public async Task LoadFastCategory()
        {
            var result = await Search.LoadCategoryFast(BuildComponent.ComponentType.CPU);
            Assert.IsTrue(result.Items.Count > 1);
        }

        [TestMethod]
        public async Task LoadMultipleFast()
        {
            var result = await Search.LoadMultipleFast(new List<string>() { "326652", "195073" });
            Assert.IsTrue(result.Items.Count == 2);
        }

    }
}

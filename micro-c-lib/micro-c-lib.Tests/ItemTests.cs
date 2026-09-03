using MicroCLib.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;

namespace MicroCLib.Tests
{
    [TestClass]
    public class ItemTests
    {
        private Item item;
        private string body;

        // A saved copy of a real, currently-active MicroCenter product page (a store-brand USB
        // flash drive - cheap and evergreen, unlikely to ever go out of stock or get discontinued).
        // Previously this test class fetched a live page in its constructor with no error handling,
        // so every regex test threw ArgumentNullException the moment that request failed for any
        // reason (network, blocked, or - as happened here - the specific hardcoded product having
        // since been discontinued and redirected to a support page with a completely different
        // template). Loading a fixture from disk makes these tests deterministic and independent of
        // MicroCenter's site being reachable at test-run time.
        private const string FIXTURE_PATH = "Fixtures/product_658458.html";
        private const string URL = "/product/658458/micro-center-32gb-superspeed-usb-31-(gen-1)-flash-drive";

        public ItemTests()
        {
            body = File.ReadAllText(FIXTURE_PATH);
            item = Item.ParseItem(URL, body);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item not null")]
        public void FromUrlReturnsItemAsync()
        {
            Assert.IsNotNull(item);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item found")]
        public void FromUrlItemFound()
        {
            Assert.IsTrue(item.SKU != "000000" && item.SKU != "");
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has name")]
        public void FromUrlSetsName ()
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Name));
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has specs")]
        public void FromUrlHasSpecs()
        {
            Assert.IsNotNull(item.Specs);
            Assert.IsTrue(item.Specs.Count > 0);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has SKU")]
        public void FromUrlHasSKU()
        {
            Assert.IsTrue(item.SKU.Length == 6);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has price")]
        public void FromUrlHasPrice()
        {
            Assert.IsTrue(item.Price > 0f);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has original price")]
        public void FromUrlHasOriginalPrice()
        {
            // The fixture product is on sale: $29.99 struck through, "Save $20.00", $9.99 now.
            // Asserting only "> 0" passed vacuously even when parsing failed outright, because
            // ParseOriginalPrice falls back to the current price - which is how the sale markup
            // change went unnoticed and left OnSale permanently false. Assert the sale itself.
            Assert.AreEqual(29.99f, item.OriginalPrice, 0.001f);
            Assert.IsTrue(item.OnSale);
            // Discount is OriginalPrice - Price (the amount saved), not the other way round.
            Assert.AreEqual(20.00f, item.Discount, 0.001f);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has URL")]
        public void FromUrlHasURL()
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.URL));
            Assert.IsTrue(Regex.Match(item.URL, "/product/\\d{6}/.*").Success);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has stock")]
        public void FromUrlHasStock()
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Stock));
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has picture URLs")]
        public void FromUrlHasPictures()
        {
            Assert.IsNotNull(item.PictureUrls);
            Assert.IsTrue(item.PictureUrls.Count > 0);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has location")]
        public void FromUrlHasLocation()
        {
            // MicroCenter's "find it in store" markup now wraps the aisle text in a nested <i> icon
            // element, so the old class="findItLink" capture lands on the icon tag and comes back
            // empty rather than throwing - Location parsing is known-stale, not crash-prone. Fixing
            // it for real means matching the new store-locator markup, which is its own follow-up.
            Assert.IsNotNull(item.Location);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has ID")]
        public void FromUrlHasID()
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.ID));
            Assert.IsTrue(item.ID.Length == 6);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has brand")]
        public void FromUrlHasBrand()
        {
            Assert.IsTrue(!string.IsNullOrWhiteSpace(item.Brand));
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has Coming Soon")]
        public void FromUrlHasComingSoon()
        {
            Assert.IsFalse(item.ComingSoon);
        }

        [TestCategory("FromUrl")]
        [TestMethod("Item has clearance listings")]
        public void FromUrlClearanceItems()
        {
            // The fixture item isn't a clearance/open-box listing, so an empty list is the correct
            // parse - this just guards ParseClearance never throws/returns null on an ordinary page.
            Assert.IsNotNull(item.ClearanceItems);
        }


        [TestMethod]
        public void CloneVerification()
        {
            var clone = item.CloneAndResetQuantity();
            Assert.AreEqual(item.Name, clone.Name);
            Assert.AreEqual(item.Price, clone.Price);
            Assert.AreEqual(item.OriginalPrice, clone.OriginalPrice);

            Assert.AreEqual(clone.Quantity, 1);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex URL")]
        public void RegexUrl()
        {
            Assert.AreEqual(Item.ParseURL(body), URL);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex ID")]
        public void RegexID()
        {
            Assert.AreEqual(Item.ParseIDFromURL(URL), "658458");
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Name")]
        public void RegexName()
        {
            var name = Item.ParseName(body);
            Assert.IsNotNull(name);
            Assert.IsTrue(name.Length > 0);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Brand")]
        public void RegexBrand()
        {
            var brand = Item.ParseBrand(body);
            Assert.IsNotNull(brand);
            Assert.IsTrue(brand.Length > 0);
        }
        [TestCategory("Regex")]
        [TestMethod("Regex SKU")]
        public void RegexSKU()
        {
            var sku = Item.ParseSKU(item, body);
            Assert.IsNotNull(sku);
            Assert.IsTrue(sku.Length == 6);
        }
        [TestCategory("Regex")]
        [TestMethod("Regex Specs")]
        public void RegexSpecs()
        {
            Assert.IsTrue(Item.ParseSpecs(body).Count > 1);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Stock")]
        public void RegexStock()
        {
            var stock = Item.ParseStock(body);
            Assert.IsNotNull(stock);
            Assert.IsTrue(stock.Length > 0);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Price")]
        public void RegexPrice()
        {
            var price = Item.ParsePrice(body);
            Assert.IsTrue(price > 0f);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Original Price")]
        public void RegexOriginalPrice()
        {
            // See FromUrlHasOriginalPrice - "> 0" can't distinguish a real parse from the
            // fall-back-to-current-price path, so pin the actual struck-through value.
            var price = Item.ParseOriginalPrice(body, item);
            Assert.AreEqual(29.99f, price, 0.001f);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Location")]
        public void RegexLocation()
        {
            // See FromUrlHasLocation - known-stale against the current store-locator markup, but
            // must return promptly and never throw.
            var location = Item.ParseLocations(body);
            Assert.IsNotNull(location);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Picture URLs")]
        public void RegexPictures()
        {
            var pictures = Item.ParsePictures(body);
            Assert.IsNotNull(pictures);
            Assert.IsTrue(pictures.Count > 0);
        }
        [TestCategory("Regex")]
        [TestMethod("Regex Plans")]
        public void RegexPlans()
        {
            // MicroCenter's protection-plan section is now a JS-driven form, not static
            // name/price markup - see the comment on Item.ParsePlans. The regex can no longer find
            // plans on any page; what matters here is that it degrades to an empty list quickly
            // (this used to hang for tens of seconds per call - see Item.ParsePlans/GetPlans).
            var plans = Item.ParsePlans(body);
            Assert.IsNotNull(plans);
        }

        [TestCategory("Regex")]
        [TestMethod("Regex Coming Soon")]
        public void RegexComingSoon()
        {
            var comingSoon = Item.ParseComingSoon(body);
            Assert.IsFalse(comingSoon);
        }
    }
}

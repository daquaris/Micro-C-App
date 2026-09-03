using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static MicroCLib.Models.Reference.PlanReference;

namespace MicroCLib.Models.Reference
{
    public class PlanReference
    {
        public static List<PlanReference> AllPlans = new List<PlanReference>();
        public PlanType Type { get; set; }
        public string Name => Type.FriendlyName();
        public float MinPrice { get; set; }
        public float MaxPrice { get; set; }

        public List<PlanTier> Tiers;

        public enum PlanType
        {
            Replacement,
            Small_Electronic_ADH,
            BYO_Replacement,
            Build_Plan,
            Apple_Plans_iPad,
            Apple_Plans_ADH_iPad,
            Apple_Plans_13,
            Apple_Plans_ADH_13,
            Apple_Plans_15_and_16,
            Apple_Plans_ADH_15_and_16,
            Apple_Plans_iMac,
            Apple_Plans_ADH_iMac,
            Apple_Plans_Mac_Pro,
            Apple_Plans_ADH_Mac_Pro,
            Apple_Plans_Mac_Mini,
            Apple_Plans_ADH_Mac_Mini,
            Laptop_ADH,
            Laptop_DOP,
            Laptop_Extension,
            Desktop_ADH,
            Desktop_DOP,
            Desktop_Extension,
            Tablet_ADH,
            Tablet_DOP,
            Tablet_Extension,
            Carry_In,
            AppleCare_13_MBP,
            AppleCare_13_MBA,
            AppleCare_15,
            AppleCare_Mac_Mini,
            AppleCare_iMac,
            AppleCare_Mac_Pro,
            AppleCare_Apple_TV,
            AppleCare_Display,
            AppleCare_iPod_Touch,
            AppleCare_iPad,
            AppleCare_iPad_Pro,
            AppleCare_iPhone,
            AppleCare_Watch_S3,
            AppleCare_Watch_S4_S5,
            AppleCare_Watch_Stainless,
            AppleCare_HomePod,
            AppleCare_Headphones,

        }

        public PlanReference(PlanType type, float minPrice, float maxPrice, params PlanTier[] tiers)
        {
            Type = type;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            Tiers = tiers.ToList();
        }

        

        public static PlanReference Get(PlanType type, float price)
        {
            // Every Apple_Plans_*/AppleCare_* entry below uses MinPrice = MaxPrice = 0 to mean "flat
            // fee regardless of device price" (e.g. AppleCare for any 13" MacBook is $269.99 whether
            // the specific configuration is $999 or $1999). A plain range check treats that as "only
            // matches price == 0" instead, so none of those ~30 plans could ever be returned for a
            // real (non-zero) device price. No tiered plan uses (0, 0) as an actual boundary - every
            // real tier's MaxPrice is > 0 - so this can't accidentally match one of those instead.
            return AllPlans.FirstOrDefault(p => p.Type == type &&
                ((p.MinPrice == 0f && p.MaxPrice == 0f) || (p.MinPrice <= price && p.MaxPrice >= price)));
        }

        static PlanReference()
        {

            AllPlans.Add(new PlanReference(PlanType.Replacement, 0.00f, 4.99f, new PlanTier(2, 0.75f, "023366"), new PlanTier(3, 1.99f, "024604")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 5.00f, 9.99f, new PlanTier(2, 0.99f, "023432"), new PlanTier(3, 2.49f, "024661")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 10.00f, 14.99f, new PlanTier(2, 1.49f, "023465"), new PlanTier(3, 2.99f, "024844")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 15.00f, 19.99f, new PlanTier(2, 1.99f, "023689"), new PlanTier(3, 3.99f, "024901")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 20.00f, 24.99f, new PlanTier(2, 2.49f, "023739"), new PlanTier(3, 4.99f, "024950")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 25.00f, 49.99f, new PlanTier(2, 4.99f, "024059"), new PlanTier(3, 9.99f, "025007")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 50.00f, 74.99f, new PlanTier(2, 6.99f, "024125"), new PlanTier(3, 14.99f, "025056")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 75.00f, 99.99f, new PlanTier(2, 9.99f, "024158"), new PlanTier(3, 19.99f, "025312")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 100.00f, 199.99f, new PlanTier(2, 19.99f, "024364"), new PlanTier(3, 39.99f, "025692")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 200.00f, 299.99f, new PlanTier(2, 29.99f, "024372"), new PlanTier(3, 59.99f, "025742")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 300.00f, 399.99f, new PlanTier(2, 49.99f, "024430"), new PlanTier(3, 89.99f, "025809")));
            AllPlans.Add(new PlanReference(PlanType.Replacement, 400.00f, 500.00f, new PlanTier(2, 69.99f, "024448"), new PlanTier(3, 139.99f, "025866")));

            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 0.00f, 49.99f,  new PlanTier(1, 5.99f, "021113"), new PlanTier(2, 14.99f, "022293")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 50.00f, 99.99f, new PlanTier(1, 19.99f, "021279"), new PlanTier(2, 39.99f, "022319")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 100.00f, 199.99f, new PlanTier(1, 29.99f, "021618"), new PlanTier(2, 69.99f, "022392")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 200.00f, 299.99f, new PlanTier(1, 49.99f, "021766"), new PlanTier(2, 99.99f, "022426")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 300.00f, 399.99f, new PlanTier(1, 59.99f, "021840"), new PlanTier(2, 139.99f, "022566")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 400.00f, 499.99f, new PlanTier(1, 79.99f, "021873"), new PlanTier(2, 179.99f, "022590")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 500.00f, 749.99f, new PlanTier(1, 99.99f, "022079"), new PlanTier(2, 199.99f, "022715")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 750.00f, 999.99f, new PlanTier(1, 199.99f, "093385"), new PlanTier(2, 299.99f, "093393")));
            AllPlans.Add(new PlanReference(PlanType.Small_Electronic_ADH, 1000.00f, 1499.99f, new PlanTier(1, 299.99f, "806422"), new PlanTier(2, 399.99f, "806430")));

            

            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 0.00f, 99.99f, new PlanTier(1, 19.99f, "012914"), new PlanTier(2, 39.99f, "013433")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 100.00f, 199.99f, new PlanTier(1, 29.99f, "013102"), new PlanTier(2, 79.99f, "013482")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 200.00f, 299.99f, new PlanTier(1, 49.99f, "013169"), new PlanTier(2, 119.99f, "013557")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 300.00f, 399.99f, new PlanTier(1, 59.99f, "013177"), new PlanTier(2, 129.99f, "013664")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 400.00f, 499.99f, new PlanTier(1, 79.99f, "013185"), new PlanTier(2, 149.99f, "013722")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 500.00f, 599.99f, new PlanTier(1, 99.99f, "013300"), new PlanTier(2, 179.99f, "013755")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 600.00f, 699.99f, new PlanTier(1, 119.99f, "013318"), new PlanTier(2, 199.99f, "013839")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 700.00f, 799.99f, new PlanTier(1, 129.99f, "013326"), new PlanTier(2, 239.99f, "013870")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 800.00f, 899.99f, new PlanTier(1, 139.99f, "013342"), new PlanTier(2, 269.99f, "013912")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 900.00f, 999.99f, new PlanTier(1, 149.99f, "013375"), new PlanTier(2, 299.99f, "013938")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 1000.00f, 1099.99f, new PlanTier(1, 199.99f, "459693"), new PlanTier(2, 329.99f, "459982")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 1100.00f, 1199.99f, new PlanTier(1, 219.99f, "459701"), new PlanTier(2, 339.99f, "460006")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 1200.00f, 1299.99f, new PlanTier(1, 239.99f, "459719"), new PlanTier(2, 359.99f, "460014")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 1300.00f, 1399.99f, new PlanTier(1, 259.99f, "459727"), new PlanTier(2, 379.99f, "460022")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 1400.00f, 1499.99f, new PlanTier(1, 279.99f, "459735"), new PlanTier(2, 399.99f, "460030")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 1500.00f, 1999.99f, new PlanTier(1, 299.99f, "459743"), new PlanTier(2, 449.99f, "460071")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 2000.00f, 2999.99f, new PlanTier(1, 339.99f, "459768"), new PlanTier(2, 499.99f, "460105")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 3000.00f, 4999.99f, new PlanTier(1, 369.99f, "459958"), new PlanTier(2, 549.99f, "460121")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_ADH, 5000.00f, 5999.99f, new PlanTier(1, 399.99f, "293787"), new PlanTier(2, 599.99f, "293795")));

            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 0.00f, 99.99f, new PlanTier(2, 19.99f, "283754"), new PlanTier(3, 39.99f, "284380")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 100.00f, 199.99f, new PlanTier(2, 29.99f, "283770"), new PlanTier(3, 79.99f, "284414")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 200.00f, 299.99f, new PlanTier(2, 49.99f, "283820"), new PlanTier(3, 119.99f, "284455")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 300.00f, 399.99f, new PlanTier(2, 59.99f, "283887"), new PlanTier(3, 129.99f, "284505")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 400.00f, 499.99f, new PlanTier(2, 79.99f, "283952"), new PlanTier(3, 149.99f, "284638")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 500.00f, 599.99f, new PlanTier(2, 99.99f, "284018"), new PlanTier(3, 179.99f, "284711")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 600.00f, 699.99f, new PlanTier(2, 119.99f, "284059"), new PlanTier(3, 199.99f, "284729")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 700.00f, 799.99f, new PlanTier(2, 129.99f, "284224"), new PlanTier(3, 239.99f, "284877")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 800.00f, 899.99f, new PlanTier(2, 139.99f, "284307"), new PlanTier(3, 269.99f, "284893")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 900.00f, 999.99f, new PlanTier(2, 149.99f, "284356"), new PlanTier(3, 299.99f, "284901")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 1000.00f, 1099.99f, new PlanTier(2, 199.99f, "460139"), new PlanTier(3, 329.99f, "460261")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 1100.00f, 1199.99f, new PlanTier(2, 219.99f, "460154"), new PlanTier(3, 334.99f, "460279")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 1200.00f, 1299.99f, new PlanTier(2, 239.99f, "460162"), new PlanTier(3, 359.99f, "460287")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 1300.00f, 1399.99f, new PlanTier(2, 259.99f, "460204"), new PlanTier(3, 379.99f, "460295")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 1400.00f, 1499.99f, new PlanTier(2, 279.99f, "460212"), new PlanTier(3, 399.99f, "460303")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 1500.00f, 1999.99f, new PlanTier(2, 299.99f, "460220"), new PlanTier(3, 449.99f, "460311")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 2000.00f, 2999.99f, new PlanTier(2, 339.99f, "460238"), new PlanTier(3, 499.99f, "460337")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_DOP, 3000.00f, 4999.99f, new PlanTier(2, 369.99f, "460246"), new PlanTier(3, 549.99f, "460378")));

            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 0.00f, 99.99f, new PlanTier(1, 19.99f, "014043"), new PlanTier(2, 29.99f, "014811")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 100.00f, 199.99f, new PlanTier(1, 29.99f, "014050"), new PlanTier(2, 59.99f, "015156")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 200.00f, 299.99f, new PlanTier(1, 39.99f, "014076"), new PlanTier(2, 79.99f, "015271")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 300.00f, 399.99f, new PlanTier(1, 49.99f, "014092"), new PlanTier(2, 89.99f, "015347")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 400.00f, 499.99f, new PlanTier(1, 59.99f, "014175"), new PlanTier(2, 99.99f, "015586")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 500.00f, 599.99f, new PlanTier(1, 69.99f, "014183"), new PlanTier(2, 119.99f, "015594")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 600.00f, 699.99f, new PlanTier(1, 79.99f, "014191"), new PlanTier(2, 129.99f, "015941")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 700.00f, 799.99f, new PlanTier(1, 89.99f, "014225"), new PlanTier(2, 159.99f, "015958")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 800.00f, 899.99f, new PlanTier(1, 99.99f, "014241"), new PlanTier(2, 179.99f, "016014")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 900.00f, 999.99f, new PlanTier(1, 109.99f, "014332"), new PlanTier(2, 199.99f, "016055")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 1000.00f, 1099.99f, new PlanTier(1, 139.99f, "460394"), new PlanTier(2, 249.99f, "460550")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 1100.00f, 1199.99f, new PlanTier(1, 149.99f, "460402"), new PlanTier(2, 259.99f, "460592")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 1200.00f, 1299.99f, new PlanTier(1, 159.99f, "460410"), new PlanTier(2, 269.99f, "460600")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 1300.00f, 1399.99f, new PlanTier(1, 169.99f, "460428"), new PlanTier(2, 279.99f, "460618")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 1400.00f, 1499.99f, new PlanTier(1, 179.99f, "460436"), new PlanTier(2, 289.99f, "460659")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 1500.00f, 1999.99f, new PlanTier(1, 199.99f, "460444"), new PlanTier(2, 299.99f, "460675")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 2000.00f, 2999.99f, new PlanTier(1, 199.99f, "460451"), new PlanTier(2, 299.99f, "460709")));
            AllPlans.Add(new PlanReference(PlanType.Laptop_Extension, 3000.00f, 4999.99f, new PlanTier(1, 199.99f, "460493"), new PlanTier(2, 299.99f, "460717")));

            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 000.00f, 199.99f, new PlanTier(2, 79.99f, "791293"), new PlanTier(3, 139.99f)));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 200.00f, 299.99f, new PlanTier(2, 89.99f, "791301"), new PlanTier(3, 139.99f, "791541")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 300.00f, 399.99f, new PlanTier(2, 99.99f, "791319"), new PlanTier(3, 149.99f, "791558")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 400.00f, 499.99f, new PlanTier(2, 139.99f, "791327"), new PlanTier(3, 169.99f, "791556")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 500.00f, 599.99f, new PlanTier(2, 149.99f, "791335"), new PlanTier(3, 179.99f, "791574")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 600.00f, 699.99f, new PlanTier(2, 159.99f, "791343"), new PlanTier(3, 199.99f, "791582")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 700.00f, 799.99f, new PlanTier(2, 179.99f, "791350"), new PlanTier(3, 219.99f, "791590")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 800.00f, 899.99f, new PlanTier(2, 189.99f, "791376"), new PlanTier(3, 229.99f, "791608")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 900.00f, 999.99f, new PlanTier(2, 199.99f, "791384"), new PlanTier(3, 239.99f, "791616")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1000.00f, 1099.99f, new PlanTier(2, 209.99f, "791392"), new PlanTier(3, 249.99f, "791624")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1100.00f, 1199.99f, new PlanTier(2, 219.99f, "791418"), new PlanTier(3, 259.99f, "791632")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1200.00f, 1299.99f, new PlanTier(2, 229.99f, "791426"), new PlanTier(3, 269.99f, "791640")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1300.00f, 1399.99f, new PlanTier(2, 239.99f, "791434"), new PlanTier(3, 279.99f, "791657")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1400.00f, 1499.99f, new PlanTier(2, 239.99f, "791442"), new PlanTier(3, 289.99f, "791673")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1500.00f, 1599.99f, new PlanTier(2, 249.99f, "791467"), new PlanTier(3, 299.99f, "791681")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 1600.00f, 1999.99f, new PlanTier(2, 269.99f, "791475"), new PlanTier(3, 329.99f, "791699")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 2000.00f, 2999.99f, new PlanTier(2, 329.99f, "791509"), new PlanTier(3, 379.99f, "791707")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 3000.00f, 4999.99f, new PlanTier(2, 349.99f, "791517"), new PlanTier(3, 399.99f, "791715")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_ADH, 5000.00f, 5999.99f, new PlanTier(2, 389.99f, "791525"), new PlanTier(3, 449.99f, "791723")));

            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 0.00f, 199.99f, new PlanTier(2, 49.99f, "791731"), new PlanTier(3, 59.99f, "791939")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 200.00f, 299.99f, new PlanTier(2, 59.99f, "791749"), new PlanTier(3, 79.99f, "791947")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 300.00f, 399.99f, new PlanTier(2, 69.99f, "791756"), new PlanTier(3, 89.99f, "791954")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 400.00f, 499.99f, new PlanTier(2, 89.99f, "791764"), new PlanTier(3, 129.99f, "791962")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 500.00f, 599.99f, new PlanTier(2, 109.99f, "791772"), new PlanTier(3, 139.99f, "791970")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 600.00f, 699.99f, new PlanTier(2, 119.99f, "791780"), new PlanTier(3, 149.99f, "791988")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 700.00f, 799.99f, new PlanTier(2, 129.99f, "791798"), new PlanTier(3, 169.99f, "791996")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 800.00f, 899.99f, new PlanTier(2, 139.99f, "791806"), new PlanTier(3, 179.99f, "792002")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 900.00f, 999.99f, new PlanTier(2, 159.99f, "791814"), new PlanTier(3, 189.99f, "792010")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1000.00f, 1099.99f, new PlanTier(2, 179.99f, "791830"), new PlanTier(3, 209.99f, "792028")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1100.00f, 1199.99f, new PlanTier(2, 189.99f, "791848"), new PlanTier(3, 219.99f, "792036")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1200.00f, 1299.99f, new PlanTier(2, 189.99f, "791855"), new PlanTier(3, 239.99f, "792051")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1300.00f, 1399.99f, new PlanTier(2, 199.99f, "791863"), new PlanTier(3, 239.99f, "792069")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1400.00f, 1499.99f, new PlanTier(2, 209.99f, "791871"), new PlanTier(3, 239.99f, "792077")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1500.00f, 1599.99f, new PlanTier(2, 229.99f, "791889"), new PlanTier(3, 259.99f, "792085")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 1600.00f, 1999.99f, new PlanTier(2, 249.99f, "791897"), new PlanTier(3, 279.99f, "792093")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 2000.00f, 2999.99f, new PlanTier(2, 269.99f, "791905"), new PlanTier(3, 299.99f, "792101")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 3000.00f, 4999.99f, new PlanTier(2, 309.99f, "791913"), new PlanTier(3, 349.99f, "792119")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_DOP, 5000.00f, 5999.99f, new PlanTier(2, 329.99f, "791921"), new PlanTier(3, 379.99f, "792135")));

            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 0.00f, 99.99f, new PlanTier(1, 19.99f, "792143"), new PlanTier(2, 39.99f, "792358")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 100.00f, 199.99f, new PlanTier(1, 29.99f, "792150"), new PlanTier(2, 49.99f, "792366")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 200.00f, 299.99f, new PlanTier(1, 39.99f, "792168"), new PlanTier(2, 59.99f, "792374")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 300.00f, 399.99f, new PlanTier(1, 59.99f, "792184"), new PlanTier(2, 69.99f, "792390")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 400.00f, 499.99f, new PlanTier(1, 69.99f, "792192"), new PlanTier(2, 89.99f, "792408")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 500.00f, 599.99f, new PlanTier(1, 79.99f, "792200"), new PlanTier(2, 109.99f, "792424")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 600.00f, 699.99f, new PlanTier(1, 99.99f, "792218"), new PlanTier(2, 119.99f, "792432")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 700.00f, 799.99f, new PlanTier(1, 109.99f, "792226"), new PlanTier(2, 139.99f, "792440")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 800.00f, 899.99f, new PlanTier(1, 119.99f, "792234"), new PlanTier(2, 149.99f, "792457")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 900.00f, 999.99f, new PlanTier(1, 139.99f, "792242"), new PlanTier(2, 169.99f, "792465")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1000.00f, 1099.99f, new PlanTier(1, 149.99f, "792259"), new PlanTier(2, 189.99f, "792473")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1100.00f, 1199.99f, new PlanTier(1, 159.99f, "792267"), new PlanTier(2, 199.99f, "792507")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1200.00f, 1299.99f, new PlanTier(1, 169.99f, "792275"), new PlanTier(2, 199.99f, "792515")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1300.00f, 1399.99f, new PlanTier(1, 179.99f, "792283"), new PlanTier(2, 209.99f, "792523")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1400.00f, 1499.99f, new PlanTier(1, 189.99f, "792291"), new PlanTier(2, 219.99f, "792531")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1500.00f, 1599.99f, new PlanTier(1, 199.99f, "792309"), new PlanTier(2, 239.99f, "792549")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 1600.00f, 1999.99f, new PlanTier(1, 219.99f, "792317"), new PlanTier(2, 259.99f, "792556")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 2000.00f, 2999.99f, new PlanTier(1, 239.99f, "792325"), new PlanTier(2, 279.99f, "792564")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 3000.00f, 4999.99f, new PlanTier(1, 269.99f, "792333"), new PlanTier(2, 299.99f, "792572")));
            AllPlans.Add(new PlanReference(PlanType.Desktop_Extension, 5000.00f, 5999.99f, new PlanTier(1, 279.99f, "792341"), new PlanTier(2, 309.99f, "792580")));

            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_13, 0.00f, 0.00f, new PlanTier(3, 319.99f), new PlanTier(2, 239.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_13, 0.00f, 0.00f, new PlanTier(3, 149.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_15_and_16, 0.00f, 0.00f, new PlanTier(3, 429.99f), new PlanTier(2, 349.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_15_and_16, 0.00f, 0.00f, new PlanTier(3, 149.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_Mac_Mini, 0.00f, 0.00f, new PlanTier(2, 119.99f), new PlanTier(3, 149.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_Mac_Mini, 0.00f, 0.00f, new PlanTier(3, 74.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_Mac_Pro, 0.00f, 0.00f, new PlanTier(3, 299.99f), new PlanTier(2, 219.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_Mac_Pro, 0.00f, 0.00f, new PlanTier(3, 149.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_iMac, 0.00f, 0.00f, new PlanTier(3, 219.99f), new PlanTier(2, 169.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_iMac, 0.00f, 0.00f, new PlanTier(3, 149.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_iPad, 0.00f, 299.99f, new PlanTier(2, 69.99f), new PlanTier(1, 39.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_ADH_iPad, 300.00f, 1699.99f, new PlanTier(2, 99.99f), new PlanTier(1, 69.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_iPad, 0.00f, 299.99f, new PlanTier(3, 59.99f)));
            AllPlans.Add(new PlanReference(PlanType.Apple_Plans_iPad, 300.00f, 1699.99f, new PlanTier(3, 79.99f)));

            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 0.00f, 99.99f, new PlanTier(1, 24.99f, "793752"), new PlanTier(2, 39.99f, "793927")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 100.00f, 199.99f, new PlanTier(1, 39.99f, "793760"), new PlanTier(2, 79.99f, "793935")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 200.00f, 299.99f, new PlanTier(1, 49.99f, "793778"), new PlanTier(2, 119.99f, "793943")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 300.00f, 399.99f, new PlanTier(1, 59.99f, "793802"), new PlanTier(2, 129.99f, "793968")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 400.00f, 499.99f, new PlanTier(1, 79.99f, "793828"), new PlanTier(2, 149.99f, "793992")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 500.00f, 599.99f, new PlanTier(1, 99.99f, "793844"), new PlanTier(2, 179.99f, "794040")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 600.00f, 699.99f, new PlanTier(1, 119.99f, "793851"), new PlanTier(2, 199.99f, "794057")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 700.00f, 799.99f, new PlanTier(1, 129.99f, "793869"), new PlanTier(2, 239.99f, "794073")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 800.00f, 899.99f, new PlanTier(1, 139.99f, "793877"), new PlanTier(2, 269.99f, "794081")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 900.00f, 999.99f, new PlanTier(1, 149.99f, "793893"), new PlanTier(2, 299.99f, "794115")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 1000.00f, 1499.99f, new PlanTier(1, 199.99f, "793901"), new PlanTier(2, 329.99f, "794131")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_ADH, 1500.00f, 5000.00f, new PlanTier(1, 199.99f, "793919"), new PlanTier(2, 329.99f, "794149")));

            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 0.00f, 99.99f, new PlanTier(2, 24.99f, "795245"), new PlanTier(3, 39.99f, "795450")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 100.00f, 199.99f, new PlanTier(2, 39.99f, "795260"), new PlanTier(3, 79.99f, "795468")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 200.00f, 299.99f, new PlanTier(2, 49.99f, "795294"), new PlanTier(3, 119.99f, "795476")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 300.00f, 399.99f, new PlanTier(2, 59.99f, "795302"), new PlanTier(3, 129.99f, "795492")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 400.00f, 499.99f, new PlanTier(2, 79.99f, "795328"), new PlanTier(3, 149.99f, "795518")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 500.00f, 599.99f, new PlanTier(2, 99.99f, "795336"), new PlanTier(3, 179.99f, "795559")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 600.00f, 699.99f, new PlanTier(2, 119.99f, "795344"), new PlanTier(3, 199.99f, "795609")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 700.00f, 799.99f, new PlanTier(2, 129.99f, "795369"), new PlanTier(3, 239.99f, "795625")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 800.00f, 899.99f, new PlanTier(2, 139.99f, "795377"), new PlanTier(3, 269.99f, "795633")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 900.00f, 999.99f, new PlanTier(2, 149.99f, "795393"), new PlanTier(3, 299.99f, "795666")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 1000.00f, 1499.99f, new PlanTier(2, 199.99f, "795427"), new PlanTier(3, 329.99f, "795674")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_DOP, 1500.00f, 5000.00f, new PlanTier(2, 199.99f, "795443"), new PlanTier(3, 329.99f, "795732")));

            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 0.00f, 99.99f, new PlanTier(1, 24.99f, "795740"), new PlanTier(2, 29.99f, "796045")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 100.00f, 199.99f, new PlanTier(1, 39.99f, "795757"), new PlanTier(2, 59.99f, "796078")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 200.00f, 299.99f, new PlanTier(1, 39.99f, "795765"), new PlanTier(2, 79.99f, "796094")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 300.00f, 399.99f, new PlanTier(1, 49.99f, "795799"), new PlanTier(2, 89.99f, "796110")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 400.00f, 499.99f, new PlanTier(1, 59.99f, "795872"), new PlanTier(2, 99.99f, "796128")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 500.00f, 599.99f, new PlanTier(1, 69.99f, "795922"), new PlanTier(2, 119.99f, "796169")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 600.00f, 699.99f, new PlanTier(1, 79.99f, "795948"), new PlanTier(2, 129.99f, "796177")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 700.00f, 799.99f, new PlanTier(1, 89.99f, "795955"), new PlanTier(2, 159.99f, "796219")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 800.00f, 899.99f, new PlanTier(1, 99.99f, "795963"), new PlanTier(2, 179.99f, "796227")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 900.00f, 999.99f, new PlanTier(1, 109.99f, "795997"), new PlanTier(2, 199.99f, "796243")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 1000.00f, 1499.99f, new PlanTier(1, 139.99f, "796011"), new PlanTier(2, 249.99f, "796268")));
            AllPlans.Add(new PlanReference(PlanType.Tablet_Extension, 1500.00f, 5000.00f, new PlanTier(1, 149.99f, "796029"), new PlanTier(2, 249.99f, "796276")));

            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 0.00f, 49.99f, new PlanTier(2, 6.99f, "022749"), new PlanTier(3, 9.99f, "023119")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 50.00f, 99.99f, new PlanTier(2, 14.99f, "022798"), new PlanTier(3, 29.99f, "023127")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 100.00f, 199.99f, new PlanTier(2, 29.99f, "022830"), new PlanTier(3, 49.99f, "023135")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 200.00f, 299.99f, new PlanTier(2, 49.99f, "022855"), new PlanTier(3, 69.99f, "023150")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 300.00f, 399.99f, new PlanTier(2, 69.99f, "022954"), new PlanTier(3, 99.99f, "023168")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 400.00f, 499.99f, new PlanTier(2, 89.99f, "022996"), new PlanTier(3, 129.99f, "023234")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 500.00f, 999.99f, new PlanTier(2, 139.99f, "023036"), new PlanTier(3, 199.99f, "023242")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 1000.00f, 1499.99f, new PlanTier(2, 199.99f, "023044"), new PlanTier(3, 279.99f, "023267")));
            AllPlans.Add(new PlanReference(PlanType.BYO_Replacement, 1500.00f, 3000.00f, new PlanTier(2, 299.99f, "023101"), new PlanTier(3, 429.99f, "023341")));

            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 0.00f,     399.99f,   new PlanTier(2, 49.99f, "947944"),   new PlanTier(3, 59.99f, "948083")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 400.00f,   599.99f,   new PlanTier(2, 69.99f, "948000"),   new PlanTier(3, 99.99f, "948091")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 600.00f,   799.99f,   new PlanTier(2, 99.99f, "948018"),   new PlanTier(3, 129.99f, "948109")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 800.00f,   999.99f,   new PlanTier(2, 119.99f, "948026"),  new PlanTier(3, 149.99f, "948117")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 1000.00f,  1199.99f,  new PlanTier(2, 129.99f, "948034"),  new PlanTier(3, 179.99f, "948125")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 1200.00f,  1499.99f,  new PlanTier(2, 149.99f, "948042"),  new PlanTier(3, 199.99f, "948158")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 1500.00f,  1999.99f,  new PlanTier(2, 199.99f, "948059"),  new PlanTier(3, 249.99f, "948166")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 2000.00f,  3999.99f,  new PlanTier(2, 249.99f, "948067"),  new PlanTier(3, 279.99f, "948174")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 4000.00f,  5999.99f,  new PlanTier(2, 399.99f, "948075"),  new PlanTier(3, 449.99f, "948190")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 6000.00f,  7499.99f,  new PlanTier(2, 549.99f, "074039"),  new PlanTier(3, 699.99f, "074054")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 7500.00f,  9999.99f,  new PlanTier(2, 699.99f, "074047"),  new PlanTier(3, 799.99f, "074088")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 10000.00f, 14999.99f, new PlanTier(2, 1299.99f, "075937"), new PlanTier(3, 1499.99f, "075952")));
            AllPlans.Add(new PlanReference(PlanType.Build_Plan, 15000.00f, 20000.00f, new PlanTier(2, 1499.99f, "075945"), new PlanTier(3, 1999.99f, "075960")));

            AllPlans.Add(new PlanReference(PlanType.Carry_In, 0.00f, 99.99f, new PlanTier(2, 19.99f, "025890"), new PlanTier(3, 29.99f, "026666")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 100.00f, 199.99f, new PlanTier(2, 29.99f, "026021"), new PlanTier(3, 59.99f, "026716")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 200.00f, 299.99f, new PlanTier(2, 39.99f, "026096"), new PlanTier(3, 79.99f, "026781")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 300.00f, 399.99f, new PlanTier(2, 49.99f, "026187"), new PlanTier(3, 89.99f, "026914")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 400.00f, 499.99f, new PlanTier(2, 59.99f, "026229"), new PlanTier(3, 99.99f, "026930")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 500.00f, 599.99f, new PlanTier(2, 69.99f, "026278"), new PlanTier(3, 119.99f, "026955")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 600.00f, 699.99f, new PlanTier(2, 79.99f, "026286"), new PlanTier(3, 129.99f, "026963")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 700.00f, 799.99f, new PlanTier(2, 89.99f, "026369"), new PlanTier(3, 159.99f, "026989")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 800.00f, 899.99f, new PlanTier(2, 99.99f, "026419"), new PlanTier(3, 179.99f, "027045")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 900.00f, 999.99f, new PlanTier(2, 109.99f, "026468"), new PlanTier(3, 199.99f, "027078")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 1000.00f, 1499.99f, new PlanTier(2, 139.99f, "026476"), new PlanTier(3, 249.99f, "027094")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 1500.00f, 1999.99f, new PlanTier(2, 149.99f, "026492"), new PlanTier(3, 249.99f, "027102")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 2000.00f, 2999.99f, new PlanTier(2, 199.99f, "026500"), new PlanTier(3, 299.99f, "027110")));
            AllPlans.Add(new PlanReference(PlanType.Carry_In, 3000.00f, 4999.99f, new PlanTier(2, 249.99f, "026633"), new PlanTier(3, 349.99f, "027185")));

            AllPlans.Add(new PlanReference(PlanType.AppleCare_13_MBA, 0f, 0f, new PlanTier(3, 249.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_13_MBP, 0f, 0f, new PlanTier(3, 269.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_15, 0f, 0f, new PlanTier(3, 379.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Mac_Mini, 0f, 0f, new PlanTier(3, 99.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_iMac, 0f, 0f, new PlanTier(3, 169.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Mac_Pro, 0f, 0f, new PlanTier(3, 299.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Watch_S3, 0f, 0f, new PlanTier(2, 49.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Watch_S4_S5, 0f, 0f, new PlanTier(2, 79.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Watch_Stainless, 0f, 0f, new PlanTier(2, 149.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Apple_TV, 0f, 0f, new PlanTier(2, 29.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Display, 0f, 0f, new PlanTier(3, 99.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_iPod_Touch, 0f, 0f, new PlanTier(2, 59.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_iPad, 0f, 0f, new PlanTier(2, 69.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_iPhone, 0f, 0f, new PlanTier(2, 99.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_iPad_Pro, 0f, 0f, new PlanTier(2, 129.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_HomePod, 0f, 0f, new PlanTier(2, 39.99f)));
            AllPlans.Add(new PlanReference(PlanType.AppleCare_Headphones, 0f, 0f, new PlanTier(2, 29.99f)));
        }
    }

    public static class PlanReferenceExtension
    {
        public static string FriendlyName(this PlanType type) => type switch
        {
            PlanType.Small_Electronic_ADH => "Small Electronics ADH",
            PlanType.BYO_Replacement => "BYO Replacement",
            PlanType.Build_Plan => "BYO Build Plan",
            PlanType.Apple_Plans_iPad => "iPad Date Of Purchase",
            PlanType.Apple_Plans_ADH_iPad => "iPad Accidental Handling",
            PlanType.Apple_Plans_13 => "MacBook 13\" Date Of Purchase",
            PlanType.Apple_Plans_ADH_13 => "MacBook 13\" Accidental Handling",
            PlanType.Apple_Plans_15_and_16 => "MacBook 15/16\" Date Of Purchase",
            PlanType.Apple_Plans_ADH_15_and_16 => "MacBook 15-16\" Accidental Handling",
            PlanType.Apple_Plans_iMac => "iMac Date Of Purchase",
            PlanType.Apple_Plans_ADH_iMac => "iMac Accidental Handling",
            PlanType.Apple_Plans_Mac_Pro => "MacPro Date Of Purchase",
            PlanType.Apple_Plans_ADH_Mac_Pro => "MacPro Accidental Handling",
            PlanType.Apple_Plans_Mac_Mini => "MacMini Date Of Purchase",
            PlanType.Apple_Plans_ADH_Mac_Mini => "MacMini Accidental Handling",
            PlanType.Laptop_ADH => "Laptop Accidental Handling",
            PlanType.Laptop_DOP => "Laptop Date Of Purchase",
            PlanType.Laptop_Extension => "Laptop Extension",
            PlanType.Desktop_ADH => "Desktop Accidental Handling",
            PlanType.Desktop_DOP => "Desktop Date Of Purchase",
            PlanType.Desktop_Extension => "Desktop Extension",
            PlanType.Tablet_ADH => "Tablet Accidental Handling",
            PlanType.Tablet_DOP => "Tablet Date Of Purchase",
            PlanType.Tablet_Extension => "Tablet Extension",
            PlanType.Carry_In => "Carry In Protection",
            PlanType.AppleCare_13_MBP => "AppleCare MBP 13\"",
            PlanType.AppleCare_13_MBA => "AppleCare MBA 13\"",
            PlanType.AppleCare_15 => "AppleCare MacBook 15\"",
            PlanType.AppleCare_Mac_Mini => "AppleCare Mac Mini",
            PlanType.AppleCare_iMac => "AppleCare iMac",
            PlanType.AppleCare_Mac_Pro => "AppleCare Mac Pro",
            PlanType.AppleCare_Apple_TV => "AppleCare Apple TV",
            PlanType.AppleCare_Display => "AppleCare Display",
            PlanType.AppleCare_iPod_Touch => "AppleCare iPod Touch",
            PlanType.AppleCare_iPad => "AppleCare iPad",
            PlanType.AppleCare_iPad_Pro => "AppleCare iPad Pro",
            PlanType.AppleCare_iPhone => "AppleCare iPhone",
            PlanType.AppleCare_Watch_S3 => "AppleCare Watch Series 3",
            PlanType.AppleCare_Watch_S4_S5 => "AppleCare Watch Series 4/5",
            PlanType.AppleCare_Watch_Stainless => "AppleCare Watch Stainless",
            PlanType.AppleCare_HomePod => "AppleCare HomePod",
            PlanType.AppleCare_Headphones => "AppleCare Headphones",
            _ => Enum.GetName(typeof(PlanType), type),
        };
    }
}

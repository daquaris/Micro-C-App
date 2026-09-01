using MicroCLib.Models;
using MicroCLib.Models.Reference;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static MicroCLib.Models.Reference.PlanReference;

namespace MicroCLib.Tests
{
    [TestClass]
    public class PlansTest
    {
        [DataTestMethod]
        public void PlansExist()
        {
            Assert.IsNotNull(PlanReference.AllPlans);
            Assert.IsTrue(PlanReference.AllPlans.Count > 0);
        }

        [TestMethod]
        public void AllTypesHavePlans()
        {
            var planTypes = Enum.GetValues(typeof(PlanType));
            foreach (PlanType type in planTypes)
            {
                var plans = PlanReference.AllPlans.Where(p => p.Type == type);
                Assert.IsTrue(plans.Count() > 0, type.ToString());
            }
        }

        [TestMethod]
        public void PriceTierInTierSanity()
        {
            //checks to make sure higher duration plan costs more
            foreach(var plan in PlanReference.AllPlans)
            {
                float checkPrice = 0f;
                foreach (var tier in plan.Tiers.OrderBy(t => t.Duration))
                {
                    Assert.IsTrue(tier.Price > checkPrice, $"{plan.Type} => {plan.MinPrice} - {plan.MaxPrice} | {tier.Duration}yr");
                    checkPrice = tier.Price;
                }
            }
        }

        [TestMethod]
        public void DurationPriceSanity()
        {
            //checks to see that there are no gaps between plan coverages
            var planTypes = Enum.GetValues(typeof(PlanType));
            foreach(PlanType type in planTypes)
            {
                var plans = PlanReference.AllPlans.Where(p => p.Type == type).OrderBy(p => p.MinPrice).ToList();
                //starts first plan at $0.00
                var lastHigh = -.01f;
                foreach(var plan in plans)
                {
                    Assert.IsTrue(plan.MinPrice == lastHigh + .01f, $"{type} => {plan.MinPrice} does not equal previous max price of {lastHigh}");
                    lastHigh = plan.MaxPrice;
                }
            }
        }

        [TestMethod]
        public void NoDuplicateSkus()
        {
            var skus = PlanReference.AllPlans.SelectMany(p => p.Tiers).GroupBy(t => t.SKU);
            foreach(var group in skus)
            {
                if (group.ElementAt(0).SKU != "000000")
                {
                    Assert.IsTrue(group.Count() == 1, $"{group.ElementAt(0).SKU} appears {group.Count()} times!");
                }
            }
        }

        [TestMethod]
        public void SixDigitSkus()
        {
            var tiers = PlanReference.AllPlans.SelectMany(p => p.Tiers);
            foreach (var tier in tiers)
            {
                Assert.IsTrue(tier.SKU.Length == 6, $"{tier.SKU} length is not 6!");
            }
        }

        [TestMethod]
        [TestCategory("Applicable Plans")]
        public void ReplacementOver500()
        {
            var comp = new BuildComponent();
            comp.Item = new Item()
            {
                Price = 500.01f
            };
            comp.Type = BuildComponent.ComponentType.BluetoothAdapter;

            var plans = comp.ApplicablePlans();
            //Replacement plans not valid over 500
            Assert.IsTrue(!plans.Any(p => p.Type == PlanType.Replacement));
        }

        [TestMethod]
        [TestCategory("Applicable Plans")]
        public void ApplicableDesktopPlans()
        {
            //desktops cannot be replacement/carry in
            var type = BuildComponent.ComponentType.Desktop;
            var comp = new BuildComponent();
            comp.Item = new Item()
            {
                ComponentType = type
            };
            var plans = comp.ApplicablePlans();
            Assert.IsTrue(plans.All(p => p.Type != PlanType.Replacement || p.Type != PlanType.Carry_In));
        }

        [TestMethod]
        [TestCategory("Applicable Plans")]
        public void ApplicableLaptopPlans()
        {
            //laptop cannot be replacement/carry in
            var type = BuildComponent.ComponentType.Laptop;
            var comp = new BuildComponent();
            comp.Item = new Item()
            {
                ComponentType = type
            };
            var plans = comp.ApplicablePlans();
            Assert.IsTrue(plans.All(p => p.Type != PlanType.Replacement || p.Type != PlanType.Carry_In));
        }

        [TestMethod]
        [TestCategory("Applicable Plans")]
        public void ApplicablePlansCustomPrice()
        {
            var comp = new BuildComponent();
            comp.Item = new Item()
            {
                ComponentType = BuildComponent.ComponentType.BuildService,
                Price = 199.99f
            };
            comp.Type = BuildComponent.ComponentType.BuildService;

            var plans = comp.ApplicablePlans(2999.99f);
            Assert.IsTrue(plans.All(p => p.Type == PlanType.Build_Plan));
            Assert.IsTrue(plans.Count() == 1);
            var plan = plans.ElementAt(0);
            Assert.IsTrue(plan.Tiers.All(t => t.Price > 60f));
        }
    }
}

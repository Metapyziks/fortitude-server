using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using FortitudeServer;
using FortitudeServer.Entities;

namespace TestServerTests
{
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        public void CacheDistanceTest()
        {
            double latA = 54.774503, lngA = -1.575948;
            double latB = 51.502972, lngB = 0.003101;
            double targ = 378700.0;
            double dist = Tools.GetDistance(latA, lngA, latB, lngB);

            double error = 0.05;

            Assert.IsTrue(dist <= targ + targ * error,
                String.Format("Distance is too large! Expected {0}, got {1}.", targ, dist));
            Assert.IsTrue(dist >= targ - targ * error,
                String.Format("Distance is too small! Expected {0}, got {1}.", targ, dist));
        }

        [TestMethod]
        public void GrowthStyles()
        {
            GrowthStyle[] styles = new[] {
                GrowthStyle.Constant,
                GrowthStyle.Logarithmic,
                GrowthStyle.Linear,
                GrowthStyle.Quadratic };

            int initial = 50;

            foreach (var style in styles) {
                Debug.WriteLine("Testing growth of {0}:", style);
                for (int i = 0; i < 10; ++i) {
                    Debug.WriteLine("- attack #{0} ", NonPlayerCache.FindNextGarrisonSize(initial, i, style));
                }
            }

            Assert.IsTrue(true);
        }
    }
}

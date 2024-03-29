﻿using System;

namespace FortitudeServer.Entities
{
    public enum GrowthStyle : byte
    {
        Constant = 0,
        Logarithmic = 1,
        Linear = 2,
        Quadratic = 3,

        Slow = 0,
        Moderate = 4,
        Fast = 8
    }

    [DatabaseEntity]
    public class NonPlayerCache : Cache
    {
        public static int FindNextGarrisonSize(int initial, int attacks, GrowthStyle growthStyle)
        {
            if (growthStyle.HasFlag(GrowthStyle.Moderate)) {
                attacks *= 2;
            } else if (growthStyle.HasFlag(GrowthStyle.Fast)) {
                attacks *= 3;
            }

            switch ((GrowthStyle) ((int) growthStyle & 0x3)) {
                case GrowthStyle.Logarithmic:
                    return initial + (int) Math.Round(initial * Math.Log(attacks + 1, 2.0) * 0.25);
                case GrowthStyle.Linear:
                    return initial + (int) Math.Round(initial * attacks * 0.25);
                case GrowthStyle.Quadratic:
                    return initial + (int) Math.Round(initial * Math.Pow(attacks, 1.3) * 0.25);
                default:
                    return initial;
            }
        }

        public static int FindNextAttackDelay(int initial, int attacks, GrowthStyle growthStyle)
        {
            return FindNextGarrisonSize(initial, attacks, growthStyle) * 2 * 60;
        }

        [NotNull]
        public GrowthStyle GrowthStyle { get; set; }

        [CleanUpMethod]
        public void Cleanup()
        {
            DatabaseManager.Delete<NPCInstance>(x => x.CacheID == CacheID);
        }

        public NonPlayerCache()
        {
            AccountID = -1;
        }
    }
}

using System;

namespace Revert.Core.Extensions
{
    public static class NumberExtensions
    {
        public static int getIntBytes(this float value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static int getIntBytes(this double value)
        {
            var bytes = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static float toRadians(this float degrees)
        {
            var units = degrees / 360f;
            return (float)(units * Math.PI * 2f);
        }

        public static double toRadians(this double degrees)
        {
            var units = degrees / 360.0;
            return units * Math.PI * 2.0;
        }


        /// <summary>
        /// Shifts the value n positions, with bits shifting beyond the 32nd place being wrapped around to the beginning
        /// </summary>
        /// <param name="positions">Number of bits to shift the value to the left</param>
        public static int ShiftAndWrap(this int value, int positions)
        {
            positions = positions & 0x1F;

            // Save the existing bit pattern, but interpret it as an unsigned integer.
            var number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded.
            var wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits.
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }

        public static int Translate(this int intToTranslate, float lowRangeFrom, float highRangeFrom, float lowRangeTo, float highRangeTo)
        {
            if (Math.Abs(intToTranslate - lowRangeFrom) > float.Epsilon) // special case addressed when from values are all the same, causes NAN
                return (int) ((intToTranslate - lowRangeFrom)*((highRangeTo - lowRangeTo)/(highRangeFrom - lowRangeFrom)) + lowRangeTo);
            return (int) lowRangeTo;
        }

        public static uint Translate(this uint intToTranslate, float lowRangeFrom, float highRangeFrom, float lowRangeTo, float highRangeTo)
        {
            if (Math.Abs(intToTranslate - lowRangeFrom) > float.Epsilon) // special case addressed when from values are all the same, causes NAN
                return (uint) ((intToTranslate - lowRangeFrom)*((highRangeTo - lowRangeTo)/(highRangeFrom - lowRangeFrom)) + lowRangeTo);
            return (uint) lowRangeTo;
        }

        public static float Translate(this float floatToTranslate, float lowRangeFrom, float highRangeFrom, float lowRangeTo, float highRangeTo)
        {
            if (Math.Abs(floatToTranslate - lowRangeFrom) > float.Epsilon) // special case addressed when from values are all the same, causes NAN
                return ((floatToTranslate - lowRangeFrom)*((highRangeTo - lowRangeTo)/(highRangeFrom - lowRangeFrom)) + lowRangeTo);
            return lowRangeTo;
        }

        /// <summary>
        /// Returns whichever value is the smallest
        /// </summary>
        public static float OrIfSmaller(this float valueToCheck, float valueToCheckAgainst)
        {
            if (valueToCheckAgainst < valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the smallest
        /// </summary>
        public static int OrIfSmaller(this int valueToCheck, int valueToCheckAgainst)
        {
            if (valueToCheckAgainst < valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the smallest
        /// </summary>
        public static uint OrIfSmaller(this uint valueToCheck, uint valueToCheckAgainst)
        {
            if (valueToCheckAgainst < valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the smallest
        /// </summary>
        public static long OrIfSmaller(this long valueToCheck, long valueToCheckAgainst)
        {
            if (valueToCheckAgainst < valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the smallest
        /// </summary>
        public static ulong OrIfSmaller(this ulong valueToCheck, ulong valueToCheckAgainst)
        {
            if (valueToCheckAgainst < valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the highest
        /// </summary>
        public static short OrIfLarger(this short valueToCheck, short valueToCheckAgainst)
        {
            if (valueToCheckAgainst > valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the highest
        /// </summary>
        public static float OrIfLarger(this float valueToCheck, float valueToCheckAgainst)
        {
            if (valueToCheckAgainst > valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the highest
        /// </summary>
        public static int OrIfLarger(this int valueToCheck, int valueToCheckAgainst)
        {
            if (valueToCheckAgainst > valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the highest
        /// </summary>
        public static uint OrIfLarger(this uint valueToCheck, uint valueToCheckAgainst)
        {
            if (valueToCheckAgainst > valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the highest
        /// </summary>
        public static long OrIfLarger(this long valueToCheck, long valueToCheckAgainst)
        {
            if (valueToCheckAgainst > valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }

        /// <summary>
        /// Returns whichever value is the highest
        /// </summary>
        public static ulong OrIfLarger(this ulong valueToCheck, ulong valueToCheckAgainst)
        {
            if (valueToCheckAgainst > valueToCheck) return valueToCheckAgainst;
            return valueToCheck;
        }
    }
}
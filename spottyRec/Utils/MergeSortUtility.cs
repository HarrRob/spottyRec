using System;
using System.Collections.Generic;
using spottyRec.Models;

namespace spottyRec.Utils
{
    public class MergeSortUtility
    {
        public static List<Recommendation> SortByScore(List<Recommendation> list)
        {
            if (list == null || list.Count <= 1)
                return list;

            return MergeSort(list, 0, list.Count - 1);
        }

        private static List<Recommendation> MergeSort(List<Recommendation> list, int left, int right)
        {
            if (left < right)
            {
                int middle = (left + right) / 2;

                MergeSort(list, left, middle);
                MergeSort(list, middle + 1, right);

                Merge(list, left, middle, right);
            }

            return list;
        }

        private static void Merge(List<Recommendation> list, int left, int middle, int right)
        {
            int leftSize = middle - left + 1;
            int rightSize = right - middle;

            // Create temporary arrays
            List<Recommendation> leftArray = new List<Recommendation>(leftSize);
            List<Recommendation> rightArray = new List<Recommendation>(rightSize);

            // Copy data to temporary arrays
            for (int i = 0; i < leftSize; i++)
                leftArray.Add(list[left + i]);

            for (int j = 0; j < rightSize; j++)
                rightArray.Add(list[middle + 1 + j]);

            // Merge the temporary arrays
            int i2 = 0; // Initial index of left subarray
            int j2 = 0; // Initial index of right subarray
            int k = left; // Initial index of merged subarray

            while (i2 < leftSize && j2 < rightSize)
            {
                // Use score for comparison (smaller scores first)
                if (leftArray[i2].Score <= rightArray[j2].Score)
                {
                    list[k] = leftArray[i2];
                    i2++;
                }
                else
                {
                    list[k] = rightArray[j2];
                    j2++;
                }
                k++;
            }

            // Copy remaining elements of leftArray[] if any
            while (i2 < leftSize)
            {
                list[k] = leftArray[i2];
                i2++;
                k++;
            }

            // Copy remaining elements of rightArray[] if any
            while (j2 < rightSize)
            {
                list[k] = rightArray[j2];
                j2++;
                k++;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using System.Diagnostics.Metrics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace BubbleSort
{
    internal class Program
    {
        static void Main(string[] args)
        {            
            //choose number of threads
            int numberOfThreads = 2;
            //int numberOfThreads = 3;
            //int numberOfThreads = 4;
            //int numberOfThreads = 6;

           //measure the time for creating the array of numbers
            var stopwatchFillArray = new Stopwatch();
            stopwatchFillArray.Start();            
            int[] arrayOfNumbers = createArray();
            stopwatchFillArray.Stop();

            //print the time of creating the array
            //and 50 numbers from array from 1000 index to 1050 index
            Console.WriteLine($"Time to create list of numbers in milliseconds: {stopwatchFillArray.ElapsedMilliseconds}");
            Console.WriteLine();
            Console.WriteLine("50 numbers before sorting:");
            printArray(arrayOfNumbers);
            Console.WriteLine();

            //count the length of array,count the length of parts for parallel sorting
            //and define start index of array for parallel sorting
            int sizeOfArray = arrayOfNumbers.Length;
            int sizeOfArrayPart = (int)(sizeOfArray % numberOfThreads == 0
                ? (sizeOfArray / numberOfThreads)
                : (sizeOfArray / numberOfThreads) + 1);
            var startingPosition = 0;

            //defirne list of Tasks for parallel sorting
            var tasks = new List<Task>();

            //start measure time of parallel sorting
            var stopwatch = new Stopwatch();
            stopwatch.Start();            
             
            //parallel sorting
            for (var i = 0; i < numberOfThreads; i++)
            {
                tasks.Add(BubleSortList(arrayOfNumbers, startingPosition, sizeOfArrayPart));
                startingPosition += sizeOfArrayPart;
            }
            Task.WaitAll(tasks.ToArray());

            //stop measure time for parallel sorting
            stopwatch.Stop();

            //final loop of sorting after parallel sorting
            BubleSortListSync(arrayOfNumbers);

            //print 50 sorted numbers from array from index 1000  to index 1050 
            //print number of threads
            //print time for parallel sorting
            Console.WriteLine("50 numbers after sorting:");
            printArray(arrayOfNumbers);
            Console.WriteLine();
            Console.WriteLine($"Number of threads: {numberOfThreads}");
            Console.WriteLine($"Time of bubble sort in milliseconds: {stopwatch.ElapsedMilliseconds}");            
            Console.ReadKey();
        }


        // fill array of numbers with random values
        static int[] createArray()
        {

            int[] listOfNumbers = new int[100_000];
            Random randomNumbers = new Random();
            int MinimumValue = 0;
            int MaximumValue = 100_000;

            
            for (int i = 0; i < 100_000; i++)
            {
                listOfNumbers[i] = randomNumbers.Next(MinimumValue, MaximumValue);
            }
            return listOfNumbers;
        }

        //bubble sort async
        static Task BubleSortList(int[] listOfNumbers, int startIndex, int sizeOfArrayPart)
        {
            return Task.Run(() =>
            {                
                int endIndex = 0;
                if (startIndex + sizeOfArrayPart < listOfNumbers.Length)
                {
                    endIndex = startIndex + sizeOfArrayPart;
                }
                else
                {
                    endIndex = listOfNumbers.Length;
                }

                for (int i = startIndex; i < endIndex; i++)
                {
                    for (int j = i + 1; j < endIndex; j++)
                    {
                        if (listOfNumbers[i] > listOfNumbers[j])
                        {
                            int temp = listOfNumbers[i];
                            listOfNumbers[i] = listOfNumbers[j];
                            listOfNumbers[j] = temp;
                        }
                    }
                }                  

            });
        }

        //bubble sort sync
        static void BubleSortListSync(int[] listOfNumbers)
        {
            for (int i = 0; i < listOfNumbers.Length; i++)
            {
                for (int j = i + 1; j < listOfNumbers.Length; j++)
                {
                    if (listOfNumbers[i] > listOfNumbers[j])
                    {
                        int temp = listOfNumbers[i];
                        listOfNumbers[i] = listOfNumbers[j];
                        listOfNumbers[j] = temp;
                    }
                }
            }
        }

        //print an array of numbers from index 1000 to index 1050
        static void printArray(int[] listOfNumbers)
        {            
            for (int i = 1000; i < 1050; i++)
            {
                Console.Write(listOfNumbers[i] + " ");
            }
            Console.WriteLine();
        }
    }
}

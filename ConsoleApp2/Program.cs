using System;

//install this library to generate barcodes: PM > Install-Package Barcode
using IronBarCode;

using System.Drawing;
using SixLabors.ImageSharp;
using System.Reflection;
using ZXing.QrCode.Internal;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Emit;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using ZXing;
using BitMiracle.LibTiff.Classic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Inventory 
{
    internal class Program    {

        static object lockobject = new object();
        static void Main(string[] args)
        {
            //choose number of threads
            int numberOfThreads = 2;
            //int numberOfThreads = 3;
            //int numberOfThreads = 4;
            //int numberOfThreads = 6;

            //create a list with type of Inventory
            List<Inventory> inventoryList = new List<Inventory>(100_000);

            //measure the time for fill the list async with inventory
            var stopwatchFillList = new Stopwatch();
            stopwatchFillList.Start();
            fillInventoryAsync(inventoryList);
            stopwatchFillList.Stop();

            //print the time of filling the list             
            Console.WriteLine($"Time to fill list async: {stopwatchFillList.ElapsedMilliseconds}");

            //print 5 first items of initialinventoryList
            //Console.WriteLine("5 items of initial result:");
            //printInventory1(inventoryList);

            Console.WriteLine();
            Console.WriteLine();

            //create a list with type of Inventory for result
            List<Inventory> result = new List<Inventory>();

            //create arrays for result items and its type
            int[] numberOfItems = new int[] { 30, 15, 8 };
            int[] typeOfInventory = new int[] { 1, 7, 10 };
            
            //create list of Tasks for parallel 
            var tasks = new List<Task>();

            //start measure the time for parallel tasks
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            //launch parallel method to find elements
            for (int j = 0; j < 3; j++)
            {
                int numberOfItemsForThread = (int)(numberOfItems[j] % numberOfThreads == 0
                    ? (numberOfItems[j] / numberOfThreads)
                    : (numberOfItems[j] / numberOfThreads) + 1);

                for (var i = 0; i < numberOfThreads; i++)
                {
                    tasks.Add(findElement(inventoryList, result, numberOfItemsForThread, typeOfInventory[j]));
                }
            }
            Task.WaitAll(tasks.ToArray());

            //stop measure the time for parallel tasks
            stopwatch.Stop();

            //sort result array by type of inventory
            result.Sort(delegate (Inventory x, Inventory y)
            {
                return x.typeOfInventory.CompareTo(y.typeOfInventory);
            });

            //check the number of item of each type in result array
            for (int i = 0; i < numberOfItems.Length; i++)
            {
                resultCheck(result, typeOfInventory[i], numberOfItems[i]);
            }

            //prinr result array, number of threads and time of parralel tasks
            Console.WriteLine("Result list(number - type of inventory): 30-1, 15-7, 8-10");
            printInventory(result);
            Console.WriteLine();
            Console.WriteLine($"Number of threads: {numberOfThreads}");
            Console.WriteLine($"Time of parallel tasks: {stopwatch.ElapsedMilliseconds}");  
            Console.ReadLine();
        }        
        
        public class Inventory
        {
            public int Id;
            public int typeOfInventory;
            public string description;
            public string barcode;
            public Inventory(int Id, int typeOfInventory, string description, string barcode)
            {
                this.Id = Id;
                this.typeOfInventory = typeOfInventory;
                this.description = description;
                this.barcode = barcode;
            }            
        }

        //method to generate randome type of inventory
        public static int setTypeInventory()
        {
            int Type;
            Random randomNumbers = new Random();
            int MinimumValue = 1;
            int MaximumValue = 101;

            Type = randomNumbers.Next(MinimumValue, MaximumValue);
            return Type;
        }

        //method to generate randome description of inventory
        public static string setDescriptionOfInventory()
        {
            List<string> nameOfInventory = new List<string>() 
            {
                "Carpenters Tool Box", "Tool Belt or Side Pouches", "Hard Hat", "Safety Goggles", "Hearing Protection",
                "Work Boots/Shoes", "Pencils & Pad of Paper", "Hammer", "Retractable Steel Tape", "Utility Knife & Blades",
                "Speed Square", "Fine Point Marker(Sharpie)", "Chalk Box & Chalk", "Flat Bar", "Cats Paw",
                "Screwdriver Combination", "Pliers", "Plumb Bob & Line", "Aviation Snips", "24” to 30” Level",
                "Torpedo Level", "Steel Tape", "Straight Screwdrivers", "Phillips Screwdrivers", "Adjustable Wrench"

            };
            string randomInventory = nameOfInventory.ChooseRandomValue();
            return randomInventory;
        }

        //method to generate barcode
        public static string setBarcode(int type)
        {
            string typeBarcode = type.ToString();            
            var myBarcode = BarcodeWriter.CreateBarcode(typeBarcode, BarcodeWriterEncoding.EAN8);
            string Barcode = myBarcode.ToString();
            return Barcode;
        }

        //method to fill List with Inventory sync
        public static List<Inventory> fillInventory(List<Inventory> inventoryList)
        {
            for (int i = 0; i < 100_000; i++)
            {
                int type = setTypeInventory();               
                string description = setDescriptionOfInventory();                
                string barcode = setBarcode(type);
                inventoryList.Add(new Inventory(i+1, type, description, barcode));
            }
            return inventoryList;
        }


        //method to fill List with Inventory async
        public static List<Inventory> fillInventoryAsync(List<Inventory> inventoryList)
        {
           
            Parallel.For (0, 100_000,(i) =>
            {
                int type = setTypeInventory();
                string description = setDescriptionOfInventory();
                string barcode = setBarcode(type);
                inventoryList.Add(new Inventory(i + 1, type, description, barcode));
                        
            });
            return inventoryList;

        }

        //parallel method to find element in the List
        static Task<int> findElement(List<Inventory> inventoryList, List<Inventory> result, int numberOfItmes, int typesOfInventory)
        {
            return Task.Run(() =>
            {
                lock (lockobject)
                {
                    int count = 0;
                    for (int i = 0; i < inventoryList.Count; i++)
                    {
                        if (inventoryList[i].typeOfInventory == typesOfInventory)
                        {
                            result.Add(inventoryList[i]);
                            count++;
                            inventoryList.Remove(inventoryList[i]);
                            if (count == numberOfItmes)
                            {
                                break;
                            }
                        }
                    }
                    return count;
                }
            });
        }

        //method to check result array
        static void resultCheck(List<Inventory> result, int typeOfInventory, int numberOfItems)
        {
            int count1 = 0;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].typeOfInventory == typeOfInventory)
                {
                    count1++;
                    if (count1 > numberOfItems) { result.Remove(result[i]); }
                }
            }
        }


        //method to print the result list of Inventory
        public static void printInventory(List<Inventory> result)
        {
            for (int i=0; i<result.Count; i++)
            {
                Console.WriteLine($"ID: {result[i].Id, -7} Type: {result[i].typeOfInventory, -5} " +
                    $"description: {result[i].description, -25} barcode: {result[i].barcode}");
                if(i<result.Count-1 && result[i].typeOfInventory != result[i+1].typeOfInventory)
                {
                    Console.WriteLine();
                }
            }
        }


        //method to print 5 items from list of Inventory
        public static void printInventory1(List<Inventory> inventoryList)
        {
            int i=0;
            foreach (Inventory element in inventoryList)
            {
                if( i<5)
                {
                    Console.WriteLine($"ID: {element.Id,-7} Type: {element.typeOfInventory,-5} " +
                        $"description: {element.description,-25} barcode: {element.barcode}");
                    i++;
                }
                else
                {
                    break;
                }
            }
        }

    }
}

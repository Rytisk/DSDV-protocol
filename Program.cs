using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DistanceVector
{
    class Program
    {
       
        static void Main(string[] args)
        {
            DistanceVector distanceVector = new DistanceVector();

            Console.WriteLine("1. Start the simulation \n2. Add new router \n3. Remove router \n4. Remove link \n5. Add link \n6. Print \n7. Send message \n8. Exit");
            string key = "";
            bool run = true;
            while (run)
            {
                Console.WriteLine("Input: ");
                key = Console.ReadLine();
                switch (key)
                {
                    case "1":
                        distanceVector.CopyDataFromFile();
                        distanceVector.Start();
                        break;
                    case "2":
                        int port = int.Parse(Console.ReadLine());
                        int numberOfPairs = int.Parse(Console.ReadLine());
                        int[] indexes = new int[numberOfPairs];
                        int[] weights = new int[numberOfPairs];
                        for (int i = 0; i < numberOfPairs; i++)
                        {
                            indexes[i] = int.Parse(Console.ReadLine());
                            weights[i] = int.Parse(Console.ReadLine());
                        }
                        distanceVector.AddNewRouter(port, indexes, weights);
                        break;
                    case "3":
                        int index = int.Parse(Console.ReadLine());
                        distanceVector.RemoveRouter(index);
                        break;
                    case "4":
                        int first = int.Parse(Console.ReadLine());
                        int second = int.Parse(Console.ReadLine());
                        distanceVector.RemoveLink(first, second);
                        break;
                    case "5":
                        first = int.Parse(Console.ReadLine());
                        second = int.Parse(Console.ReadLine());
                        int weight = int.Parse(Console.ReadLine());
                        distanceVector.AddLink(first, second, weight);
                        distanceVector.AddLink(second, first, weight);
                        break;
                    case "6":
                        index = int.Parse(Console.ReadLine());
                        distanceVector.Print(index);
                        break;
                    case "7":
                        break;
                    case "8":
                        run = false;
                        distanceVector.Clean();
                        break;
                    default:
                        break;

                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSDV_protocol
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
                        string id = Console.ReadLine();
                        int numberOfPairs = int.Parse(Console.ReadLine());
                        string[] ids = new string[numberOfPairs];
                        int[] weights = new int[numberOfPairs];
                        for (int i = 0; i < numberOfPairs; i++)
                        {
                            ids[i] = Console.ReadLine();
                            weights[i] = int.Parse(Console.ReadLine());
                        }
                        distanceVector.AddNewRouter(id, ids, weights);
                        break;
                    case "3":
                        id = Console.ReadLine();
                        distanceVector.RemoveRouter(id);
                        break;
                    case "4":
                        string first = Console.ReadLine();
                        string second = Console.ReadLine();
                        distanceVector.RemoveLink(first, second);
                        break;
                    case "5":
                        first = Console.ReadLine();
                        second = Console.ReadLine();
                        int weight = int.Parse(Console.ReadLine());
                        distanceVector.UpdateLink(first, second, weight);
                        break;
                    case "6":
                        id = Console.ReadLine();
                        distanceVector.Print(id);
                        break;
                    case "7":
                        distanceVector.SendMessage();
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

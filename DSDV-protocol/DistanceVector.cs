using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DSDV_protocol
{
    class DistanceVector
    {
        private SortedDictionary<string, Thread> routersProcesses = new SortedDictionary<string, Thread>();
        private SortedDictionary<string, Router> routers = new SortedDictionary<string, Router>();
        private Graph graph;
        private string filepath = "E:\\Projects\\DSDV-protocol\\DSDV-protocol\\data2.txt";

        public DistanceVector()
        {
            graph = new Graph();
        }

        public void Start()
        {
            foreach (var rt in routersProcesses)
            {
                rt.Value.Start();
            }
        }

        public void AddNewRouter(string _id, string[] _ids, int[] weights)
        {
            Router router = new Router(_id, routers);

            for(int i = 0; i < _ids.Length; i++)
            {
                graph.AddNewPair(router, routers[_ids[i]], weights[i]);
            }
            
            graph.AddNewPair(router, router, 0);
            routers.Add(_id, router);
            Thread th = new Thread(router.Run);
            routersProcesses.Add(_id, th);
            th.Start();
        }

        public void Print(string _id)
        {
            Console.WriteLine(routers[_id].GetRoutingTable().ToString());
        }

        public void RemoveRouter(string _id)
        {
            graph.RemoveRouter(_id);
            routersProcesses[_id].Abort();
            routersProcesses.Remove(_id);
            routers.Remove(_id);
        }

        public void UpdateLink(string _first, string _second, int _distance)
        {
            graph.UpdateLink(routers[_first], routers[_second], _distance);
        }

        public void RemoveLink(string _first, string _second)
        {
            graph.RemoveLink(routers[_first], routers[_second]);
        }

        public void AddLink(string _first, string _second, int _weight)
        {

            graph.AddNewPair(routers[_first], routers[_second], _weight);
        }

        public void CopyDataFromFile()
        {
            string[] file = System.IO.File.ReadAllLines(filepath);
            int numOfRouters = int.Parse(file[0]);
            string[] lines ;
            string[] ids;
            for (int i = 0; i < file.Length; i++)
            {
                lines = file[i].Split(' ');
            }
            int index = 0;
            
            index++;

            lines = file[index].Split(' ');
            ids = lines;
            for (int i = 0; i < numOfRouters; i++)
            {
                Router rt = new Router(lines[i], routers);
                routers.Add(lines[i], rt);
            }
            index += 1;
            for (int i = 0; i < numOfRouters; i++)
            {
                int count = int.Parse(file[index]);
                for (int j = 0; j < count; j++)
                {
                    lines = file[index+1].Split(' ');

                    graph.AddPair(routers[ids[i]], routers[lines[0]], int.Parse(lines[1]));
                    
                    index++;
                }
                graph.AddPair(routers[ids[i]], routers[ids[i]], 0);
                index++;
            }

            foreach (var rt in routers)
            {
                routersProcesses.Add(rt.Key, new Thread(rt.Value.Run));
            }
        }

        public void Clean()
        {
            foreach(var item in routersProcesses)
            {
                item.Value.Abort();
            }
        }
    }
}

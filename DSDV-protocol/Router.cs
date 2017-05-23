using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DSDV_protocol
{
    class Router : IComparable<Router>
    {
        private System.Timers.Timer updateTimer;
        private RoutingTable routingTable;
        private RouterReplica selfReplica;

        private SortedDictionary<string, RouterReplica> neighbours = new SortedDictionary<string, RouterReplica>();
        private SortedDictionary<string, Router> network;
        public List<string> sentNeighbours = new List<string>();

        public string Id
        {
            get;
            set;
        }

        public Router(string _id, SortedDictionary<string, Router> _network)
        {
            Id = _id;
            network = _network;
            updateTimer = new System.Timers.Timer(9000);
            updateTimer.Elapsed += UpdateRoutingTable;
        }

        private void UpdateRoutingTable(object sender, ElapsedEventArgs e)
        {
            sentNeighbours.Clear();
            selfReplica.GenerateSequenceNumber(true);
            SendMessages();
            Thread.Sleep(500);
            ReceiveRoutingTables();

        }

        public void SendMessages()
        {
            for (int i = 0; i < neighbours.Count; i++)
            {
                if(neighbours.ElementAt(i).Key != Id)
                    sentNeighbours.Add(neighbours.ElementAt(i).Key);
            }
        }

        public void ReceiveRoutingTables()
        {
            for(int i = 0; i < network.Count; i++)
            {
                Router neighbour = network.ElementAt(i).Value;
                if (neighbour != null)
                {
                    List<string> neighboursSent = neighbour.sentNeighbours.ToList();
                    for(int j =0; j < neighboursSent.Count; j++)
                    {
                        if (neighboursSent.ElementAt(j) == Id)
                        {
                            SortedDictionary<string, RouterReplica> neighbourData = new SortedDictionary<string, RouterReplica>(neighbour.GetRoutingData());
                            if (!IsMyNeighbour(neighbour.Id))                   
                            {
                                neighbours.Add(neighbour.Id, GenerateReplica(neighbour, GetDistance(neighbourData)));        
                            }
                            routingTable.Update(neighbourData, neighbour.Id);
                        }
                    }
                }
            }
           
        }

        private void ReceiveMessageFrom(string _id)         //Watch Out???
        {
            SortedDictionary<string, RouterReplica> neighbourData = new SortedDictionary<string, RouterReplica>(network[_id].GetRoutingData());
            if (!IsMyNeighbour(_id))
            {
                neighbours.Add(_id, GenerateReplica(network[_id], GetDistance(neighbourData)));
            }
            routingTable.Update(neighbourData, _id);
        }

        private SortedDictionary<string, RouterReplica> CopyRoutingData(SortedDictionary<string, RouterReplica> _data)
        {
            SortedDictionary<string, RouterReplica> neighbourData = new SortedDictionary<string, RouterReplica>();
            for (int i = 0; i < _data.Count; i++)
            {
                neighbourData.Add(_data.ElementAt(i).Key, _data.ElementAt(i).Value);
            }
            return neighbourData;
        }

        private int GetDistance(SortedDictionary<string, RouterReplica> _data)
        {
            for(int i = 0; i < _data.Count; i++)
            {
                if (_data.ElementAt(i).Key == Id)
                    return _data.ElementAt(i).Value.Distance;
            }
            return int.MaxValue;
        }
        
        private bool IsMyNeighbour(string _id)
        {
            for(int i = 0; i < neighbours.Count; i++)
            {
                if (neighbours.ElementAt(i).Key == _id)
                    return true;
            }
            return false;
        }

        public void AddLink(Router _router, int _distance)
        {
            RouterReplica replica = GenerateReplica(_router, _distance);
            
            neighbours.Add(_router.Id, replica);

            if (replica.Id == Id)
                selfReplica = replica;
        }

        public void UpdateLink(Router _router, int _distance)
        {
            RouterReplica replica;
            if (!neighbours.ContainsKey(_router.Id))
            {
                replica = GenerateReplica(_router, _distance);
                replica.LostConnection += LostConnection;
                neighbours.Add(_router.Id, replica);
            }
            else
            {
                replica = neighbours[_router.Id];
                replica.Distance = _distance;
            }
            routingTable.UpdateLink(replica);
        }

        public void LostConnection(object sender, EventArgs e)
        {
            Console.WriteLine("Lost connection");
            RouterReplica replica = sender as RouterReplica;
            neighbours.Remove(replica.Id);
        }

        public void RemoveLink(Router _router)
        {
            neighbours.Remove(_router.Id);
            routingTable.SetLost(_router.Id);
            //UpdateRoutingTable(this, null);                 // Kaimynai turi gaut tik is manes
            selfReplica.GenerateSequenceNumber(true);
            for (int i = 0; i < neighbours.Count; i++)
            {
                network[neighbours.ElementAt(i).Key].ReceiveMessageFrom(Id);
            }
        }

        public void Kill()
        {
            updateTimer.Stop();
            updateTimer.Close();

            sentNeighbours.Clear();
        }

        public void Run()
        {
            routingTable = new RoutingTable(neighbours, this);
            updateTimer.Start();
        }

        public SortedDictionary<string, RouterReplica> GetRoutingData()
        {
            return routingTable.GetData();
        }

        public RoutingTable GetRoutingTable()
        {
            return routingTable;
        }

        public int CompareTo(Router _other)
        {
            return Id.CompareTo(_other.Id);
        }

        private RouterReplica GenerateReplica(Router _router, int _distance)
        {
            RouterReplica replica = new RouterReplica(_router.Id, _router.Id, _router.Id + "-0", _distance);
            replica.LostConnection += LostConnection;
            return replica;
        }
    }
}

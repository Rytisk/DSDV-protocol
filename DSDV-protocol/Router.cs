using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly Object obj = new Object();
        private static readonly Object obj2 = new Object();

        public bool SendPacket
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }

        public Router(string _id, SortedDictionary<string, Router> _network)
        {
            Id = _id;
            network = _network;
            updateTimer = new System.Timers.Timer(6000);
            updateTimer.Elapsed += UpdateRoutingTable;
            SendPacket = false;
        }

        private void UpdateRoutingTable(object sender, ElapsedEventArgs e)
        {
                routingTable.CleanUp();
                sentNeighbours.Clear();
                selfReplica.GenerateSequenceNumber(true);
                SendRoutingTables();
                Thread.Sleep(500);
                ReceiveRoutingTables();
                Debug.Write(". ");
        }

        public void SendThePacket()
        {
            string sendTo = routingTable.GetNextHop(Packet.Destination);
            if (sendTo != "-")
            {
                if (NextHopExists(sendTo))
                {
                    Packet.Current = sendTo;
                    Console.WriteLine("Sent the packet from {0} to {1}.", Id, sendTo);
                }
                else
                {
                    Console.WriteLine("Next router doensn't respond. Please wait for routing tables to update and try again.");
                }
            }
            else
            {
                Console.WriteLine("Can't find the route.");
            }
            if(Packet.Current == Packet.Destination)
            {
                Console.WriteLine("The packet reached it's destination. Sent from {0} to {1}.", Packet.Source, Packet.Destination);
                Packet.ToSend = false;
            }
        }

        private bool NextHopExists(string _nextHop)
        {
            if (network.ContainsKey(_nextHop))
            {
                return true;
            }
            return false;
        }

        public void SendRoutingTables()
        {
            for (int i = 0; i < neighbours.Count; i++)
            {
                if(neighbours.ElementAt(i).Key != Id)
                    sentNeighbours.Add(neighbours.ElementAt(i).Key);
            }
        }

        public void ReceiveRoutingTables()
        {
            lock (obj2)
            {
                for (int i = 0; i < network.Count; i++)
                {
                    Router neighbour = network.ElementAt(i).Value;
                    if (neighbour != null)
                    {
                        List<string> neighboursSent = neighbour.sentNeighbours.ToList();
                        for (int j = 0; j < neighboursSent.Count; j++)
                        {
                            if (neighboursSent.ElementAt(j) == Id)
                            {
                                SortedDictionary<string, RouterReplica> neighbourData = new SortedDictionary<string, RouterReplica>(neighbour.GetRoutingData());
                                if (!IsMyNeighbour(neighbour.Id))
                                {
                                    RouterReplica replica = GenerateReplica(neighbour, GetDistance(neighbourData));
                                    neighbours.Add(neighbour.Id, replica);
                                    routingTable.UpdateLink(replica);
                                    neighbours[neighbour.Id].StartTimer();
                                }
                                else
                                {
                                    neighbours[neighbour.Id].StopTimer();
                                    neighbours[neighbour.Id].StartTimer();
                                }
                                routingTable.Update(neighbourData, neighbour.Id);
                                break;
                            }
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
                RouterReplica replica = GenerateReplica(network[_id], GetDistance(neighbourData));
                neighbours.Add(_id, replica);
                routingTable.UpdateLink(replica);
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
            else
                replica.StartTimer();
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
            RemoveLinkById(replica.Id);
        }

        public void RemoveLink(Router _router)
        {
            RemoveLinkById(_router.Id);
        }

        public void RemoveLinkById(string _id)
        {
            neighbours.Remove(_id);
            routingTable.SetLost(_id);
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

            for(int i = 0; i < neighbours.Count; i++)
            {
                neighbours.ElementAt(i).Value.DisposeTimer();
            }

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

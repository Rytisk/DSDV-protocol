using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSDV_protocol
{
    class Graph
    {
        public List<Router> Routers { get; }

        public Graph()
        {
            Routers = new List<Router>();
        }

        private void AddNeighbours(Router first, Router second, int _distance)
        {
            AddNeighbour(first, second, _distance);
            AddNeighbour(second, first, _distance);
        }

        private void AddNeighbour(Router first, Router second, int _distance)
        {
             first.AddLink(second, _distance);
        }

        private void AddToList(Router router)
        {
            if (!Routers.Contains(router))
            {
                Routers.Add(router);
            }
        }

        public void AddPair(Router _first, Router _second, int _distance)
        {
            AddToList(_first);
            AddToList(_second);
            AddNeighbour(_first, _second, _distance);
        }

        public void UpdateLink(Router _first, Router _second, int _distance)
        {
            _first.UpdateLink(_second, _distance);
            _second.UpdateLink(_first, _distance);
        }

        public void AddNewPair(Router _first, Router _second, int _distance)
        {
            AddToList(_first);
           _first.AddLink(_second, _distance);
        }

        public void RemoveRouter(string _id)
        {
            Router router = Routers.Where(id => id.Id == _id).FirstOrDefault();
            router.Kill();
            Routers.Remove(router);
        }

        public void RemoveLink(Router _first, Router _second)
        {
            _first.RemoveLink(_second);
            _second.RemoveLink(_first);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    class ClientpeerPool
    {
        private Queue<Clientpeer> clientpeerspool;

        public ClientpeerPool(int capacity)
        {
            clientpeerspool = new Queue<Clientpeer>(capacity);
        }


        public void Enqueue(Clientpeer client)
        {
            clientpeerspool.Enqueue(client);

        }

        public Clientpeer Dequeue()
        {
            return clientpeerspool.Dequeue();
        }

    }
}

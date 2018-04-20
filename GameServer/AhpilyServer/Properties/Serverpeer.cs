using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AhpilyServer
{
    public class Serverpeer
    {
        /// <summary>
        /// 服务器端的socket
        /// </summary>
        private Socket serverSocket;

        /// <summary>
        /// 限制同时访问某一资源或资源池的线程数
        /// </summary>
        private Semaphore acceptSemaphore;


        /// <summary>
        /// 客户端对象的连接池
        /// </summary>
        private ClientpeerPool clientPeerPool;

        /// <summary>
        /// 打开服务器连接
        /// </summary>
        /// <param name="prot">端口号</param>
        /// <param name="maxcount">最大连接数</param>
        public void Start(int prot , int maxcount)
        {
            try
            {
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                acceptSemaphore = new Semaphore(maxcount, maxcount);

                clientPeerPool = new ClientpeerPool(maxcount);
                Clientpeer temClientpeer = null;
                for (int i = 0; i < maxcount; i++)
                {
                    temClientpeer = new Clientpeer();
                    temClientpeer.receveargs.Completed += Receive_Completed;
                    temClientpeer.receivecompleted = receiveCompleted;
                    clientPeerPool.Enqueue(temClientpeer);
                }

                serverSocket.Bind(new IPEndPoint(IPAddress.Any, prot));
                serverSocket.Listen(10);
                Console.WriteLine("服务器启动成功！！！");

                

                startAccept(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw new Exception("打开服务器失败！！！！");
            }
        }

        private void startAccept(SocketAsyncEventArgs e)
        {
            if (null == e)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += E_Completed;
            }
            //计数
            acceptSemaphore.WaitOne();
            //AcceptAsync 基于io封装效率更高
            bool result = serverSocket.AcceptAsync(e);
            
            //没在进行，则说明已经处理完成
            if (!result)
            {
                processResult(e);
            }
        }

        private void E_Completed(object sender, SocketAsyncEventArgs e)
        {
            processResult(e);
        }

        private void processResult(SocketAsyncEventArgs e)
        {
            //Socket clientSocket = e.AcceptSocket;

            //保存客户端socket

            Clientpeer client = clientPeerPool.Dequeue();
            client.ClientSocket = e.AcceptSocket;

            //开始接收数据
            StartReceive(client);

            //递归调用
            e.AcceptSocket = null;
            startAccept(e);
        }


        #region 接收数据

        private void StartReceive(Clientpeer client)
        {
            try
            {
                SocketAsyncEventArgs clientrecevearges = client.receveargs;
                bool result = client.ClientSocket.ReceiveAsync(clientrecevearges);
                if (!result)
                {
                    ProcessReceive(clientrecevearges);
                }
            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// 处理接收的请求
        /// </summary>
        /// <param name="e"></param>
        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            Clientpeer client = e.UserToken as Clientpeer;
            //异步套接字结果成功且传输字节大于0
            if (client.receveargs.SocketError == SocketError.Success && client.receveargs.BytesTransferred > 0)
            {
                byte[] packet = new byte[client.receveargs.BytesTransferred];
                Buffer.BlockCopy(client.receveargs.Buffer, 0, packet, 0, client.receveargs.BytesTransferred);

                //客户端自身处理这个数据包，自身解析
                client.StartReceive(packet);

            }
            //断开连接 如果没有传输的字节数 就代表断开连接了
            else if(client.receveargs.BytesTransferred == 0)
            {
                if (client.receveargs.SocketError == SocketError.Success)
                {
                    //客户端主动断开
                    //TODO
                }
                else {
                    //由于网络异常 导致 被动断开连接
                    //TODO
                }
            }
        }


        /// <summary>
        /// 当接收完成时触发的事件
        /// </summary>
        /// <param name="e"></param>
        private void Receive_Completed(object sender, SocketAsyncEventArgs e)
        {

        }


        /// <summary>
        /// 接收完一条消息时的回调
        /// </summary>
        /// <param name="client"></param>
        /// <param name="value"></param>
        private void receiveCompleted(Clientpeer client, SocketMsg value)
        {
            //给应用层 让其使用
            //TODO
        }


        #endregion


        #region 接收连接
        #endregion
        #region 发送数据
        #endregion

    }
}

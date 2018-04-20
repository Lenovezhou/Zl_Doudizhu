using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AhpilyServer
{
    class Clientpeer
    {

        public delegate void ReceiveCompleted(Clientpeer client, SocketMsg value);


        /// <summary>
        /// 一个消息解析完成的回调
        /// </summary>
        public ReceiveCompleted receivecompleted;



        /// <summary>
        /// 客户端Socket
        /// </summary>
        public Socket ClientSocket;

        /// <summary>
        /// 接收的异步套接字请求
        /// </summary>
        public SocketAsyncEventArgs receveargs;

        /// <summary>
        /// 是否正在处理接收的数据
        /// </summary>
        private bool isProcess = false;

        /// <summary>
        /// 设置连接对象
        /// </summary>
        /// <param name="socket"></param>
        public void SetSocket(Socket socket)
        {
            this.ClientSocket = socket;
        }


        #region 接收数据

        /// <summary>
        /// 一旦接收到数据 就存到缓存区内
        /// </summary>
        private List<byte> dataCache = new List<byte>();

        public Clientpeer()
        {
            this.receveargs = new SocketAsyncEventArgs();
            receveargs.UserToken = this;
        }


        /// <summary>
        /// 自身处理数据包
        /// </summary>
        /// <param name="packet"></param>
        public void StartReceive(byte[] packet)
        {
            dataCache.AddRange(packet);
            if (!isProcess)
                ReceiveProcess();
        }

        private void ReceiveProcess()
        {
            isProcess = true;
            byte[] data = EncodTool.DecodeMessage(ref dataCache);

            if (null == data)
            {
                isProcess = false;
                return;
            }

            //TODO需要再次转成一个具体的类型供我们使用
            SocketMsg value = EncodTool.DecodeMsg(data);
            //回调给上层

            receivecompleted?.Invoke(this, value);
            //尾递归
            ReceiveProcess();   
        }


        #endregion


    }
}

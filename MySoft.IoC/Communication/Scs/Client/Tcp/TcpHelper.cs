using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MySoft.IoC.Communication.Scs.Client.Tcp
{
    /// <summary>
    /// This class is used to simplify TCP socket operations.
    /// </summary>
    internal static class TcpHelper
    {
        /// <summary>
        /// This code is used to connect to a TCP socket with timeout option.
        /// </summary>
        /// <param name="endPoint">IP endpoint of remote server</param>
        /// <param name="timeoutMs">Timeout to wait until connect</param>
        /// <returns>Socket object connected to server</returns>
        /// <exception cref="SocketException">Throws SocketException if can not connect.</exception>
        /// <exception cref="TimeoutException">Throws TimeoutException if can not connect within specified timeoutMs</exception>
        public static Socket ConnectToServer(EndPoint endPoint, int timeoutMs)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var _manualReset = new ManualResetEvent(false);

            var e = new SocketAsyncEventArgs();
            e.Completed += IO_Completed;
            e.RemoteEndPoint = endPoint;
            e.AcceptSocket = socket;
            e.UserToken = _manualReset;

            if (!socket.ConnectAsync(e))
            {
                IO_Completed(socket, e);
            }

            if (!_manualReset.WaitOne(timeoutMs, false))
            {
                throw new TimeoutException("The host failed to connect. Timeout occured.");
            }

            return socket;
        }

        /// <summary>
        /// IO_Completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.SocketError == SocketError.Success)
                {
                    if (e.LastOperation == SocketAsyncOperation.Connect)
                    {
                        var _manualReset = e.UserToken as ManualResetEvent;
                        _manualReset.Set();
                    }
                }
                else
                {
                    //close socket.
                    e.AcceptSocket.Close();
                }
            }
            catch (Exception ex) { }
            finally
            {
                e.Completed -= IO_Completed;
                e.RemoteEndPoint = null;
                e.AcceptSocket = null;
                e.UserToken = null;
                e.Dispose();
            }
        }
    }
}
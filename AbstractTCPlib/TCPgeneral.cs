﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace AbstractTCPlib
{
    public class TCPgeneral : IDisposable
    {
        private TcpClient client;
        private NetworkStream stream;
        private ConcurrentQueue<byte[]> sendBuffer;
        private Thread recieve;
        private Thread send;
        private int intLenght;

        private bool isAlive;

        public Action<byte[]> OnRawDataRecieved;
        public Action<ErrorTypes, string> OnError;

        public TCPgeneral(TcpClient client)
        {
            isAlive = true;
            this.client = client;
            stream = this.client.GetStream();

            sendBuffer = new ConcurrentQueue<byte[]>();
            intLenght = BitConverter.GetBytes(int.MaxValue).Length;

            recieve = new Thread(new ThreadStart(recieveTCP));
            send = new Thread(new ThreadStart(sendTCP));
            recieve.Start();
            send.Start();
        }

        private void sendTCP()
        {
            byte[] toSend;
            byte[] rawData;
            byte[] rawLenght;

            while (true)
            {
                bool ok = sendBuffer.TryDequeue(out rawData);
                if (ok)
                {
                    if (rawData.LongLength > int.MaxValue - intLenght)
                    {
                        if (OnError != null)
                        {
                            OnError(ErrorTypes.ExceededByteMaxValueOfInt, "byteArrayShouldbeSmaller");
                        }
                        continue;
                    }
                    rawLenght = BitConverter.GetBytes(rawData.Length); //if lenght exceeds int max range you will send crap
                    toSend = new byte[rawData.Length + rawLenght.Length];
                    Array.Copy(rawLenght, 0, toSend, 0, rawLenght.Length);
                    Array.Copy(rawData, 0, toSend, rawLenght.Length , rawData.Length);

                    try
                    {
                        stream.Write(toSend, 0, toSend.Length);
                    }
                    catch (Exception e)
                    {
                        OnError(ErrorTypes.TCPWriteException, e.Message);
                        isAlive = false;
                        Dispose();
                    }
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void recieveTCP()
        {
            byte[] data = null;
            int bytesToRead = 0;
            int readFromCurrentPoll = 0;

            try
            {
                data = new byte[client.ReceiveBufferSize];
            }
            catch (Exception e)
            {
                OnError(ErrorTypes.ClientReceiveBufferSize, e.Message);
                isAlive = false;
                Dispose();
            }
            List<byte> bufferList = new List<byte>();

            while (true)
            {
                //gets the bytestoRead
                try
                {
                    readFromCurrentPoll = stream.Read(data, 0, client.ReceiveBufferSize);
                    if (bytesToRead == 0)
                    {
                        byte[] noPadding = new byte[readFromCurrentPoll];
                        Array.Copy(data, 0, noPadding, 0, readFromCurrentPoll);
                        bufferList.AddRange(noPadding);

                        if (bufferList.Count >= intLenght)
                        {
                            byte[] number = new byte[intLenght];
                            for (int i = 0; i < intLenght; i++)
                            {
                                number[i] = bufferList[i];
                            }

                            bytesToRead = BitConverter.ToInt32(number, 0);
                            bufferList.RemoveRange(0, intLenght);
                        }
                    }
                    else
                    {
                        byte[] noPadding = new byte[readFromCurrentPoll];
                        Array.Copy(data, 0, noPadding, 0, readFromCurrentPoll);
                        bufferList.AddRange(noPadding);

                        while (true)
                        {
                            if (bytesToRead != 0 && bufferList.Count >= bytesToRead)
                            {
                                byte[] final = new byte[bytesToRead];
                                bufferList.CopyTo(0, final, 0, bytesToRead);
                                if (OnRawDataRecieved != null)
                                {
                                    OnRawDataRecieved(final);
                                }
                                bufferList.RemoveRange(0, bytesToRead);
                                bytesToRead = 0;

                                if (bufferList.Count >= intLenght)
                                {
                                    byte[] number = new byte[intLenght];
                                    for (int i = 0; i < intLenght; i++)
                                    {
                                        number[i] = bufferList[i];
                                    }

                                    bytesToRead = BitConverter.ToInt32(number, 0);
                                    bufferList.RemoveRange(0, intLenght);
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                break;
                            }

                        }

                    }
                }
                catch (Exception e)
                {
                    OnError(ErrorTypes.InRecieveCodeError, e.Message);
                    isAlive = false;
                    Dispose();
                }
            }
        }

        public void SendTCP(byte[] data)
        {
            sendBuffer.Enqueue(data);
        }

        public void Dispose()
        {
            recieve.Abort();
            send.Abort();
            stream.Close();
            client.Close();
        }

        public bool IsAlive { get { return isAlive; } }
    }
}
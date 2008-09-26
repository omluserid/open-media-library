﻿using System;
using System.ServiceModel;

using OMLEngine;

namespace OMLEngineService
{
    [ServiceContract]
    public interface ITranscodingService
    {
        [OperationContract(IsOneWay = true)]
        void Transcode(MediaSource source);

        [OperationContract(IsOneWay = true)]
        void Cancel(string key);

        [OperationContract]
        TranscodingStatus GetStatus(string key);

        [OperationContract(IsOneWay = true)]
        void RegisterNotifyer(string url, bool register);
    }

    public enum TranscodingStatus
    {
        Unknown,
        Initializing,
        BufferReady,
        Stopped,
        Done,
        Error,
    }

    [ServiceContract]
    public interface ITranscodingNotifyingService
    {
        [OperationContract(IsOneWay = true)]
        void StatusChanged(string key, TranscodingStatus status);
    }

    public class MyClientBase<T> : ClientBase<T>, IDisposable
      where T : class
    {
        //public MyClientBase() : base() { }
        public MyClientBase(System.ServiceModel.Channels.Binding binding, EndpointAddress endPointAddress) : base(binding, endPointAddress) { }
        public MyClientBase(string endPoint, string uri) : base(endPoint, uri) { }

        public void Dispose()
        {
            if (this.State == CommunicationState.Opened)
                base.Close();
        }

        public new T Channel { get { return base.Channel; } }

        public static T GetProxy(System.ServiceModel.Channels.Binding binding, EndpointAddress endPointAddress)
        {
            return new MyClientBase<T>(binding, endPointAddress).Channel;
        }
    }
}
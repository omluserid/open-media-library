﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;

namespace FileDownloader {
    public class DownloadEngine {
        public delegate void DownloadEventsHandler(string status);
        public event DownloadEventsHandler Log;
        protected void FireEvent(string msg) {
            if (Log != null)
                Log(msg);
        }

        public void FireEvent(int msg) {
            FireEvent(Convert.ToString(msg));
        }

        public string Url {
            get;
            set;
        }

        public string DownloadedFile {
            get;
            private set;
        }

        public DownloadEngine(string url) {
            Url = url;
        }

        public long TotalBytes {
            get;
            private set;
        }

        public bool Download() {
            int bytesDone = 0;

            Stream readStream = null;
            Stream writeStream = null;

            WebResponse res = null;

            try {
                WebRequest req = WebRequest.Create(Url);
                if (req != null) {
                    res = req.GetResponse();
                    if (res != null) {
                        TotalBytes = res.ContentLength;
                        readStream = res.GetResponseStream();
                        DownloadedFile = _localfile();
                        writeStream = File.Create(DownloadedFile);
                        byte[] buffer = new byte[1024];

                        int bytesRead = 0;

                        do {
                            bytesRead = readStream.Read(buffer, 0, buffer.Length);
                            writeStream.Write(buffer, 0, bytesRead);
                            bytesDone += bytesRead;
                            FireEvent(bytesDone);
                            //int pct = Convert.ToInt32((Convert.ToDouble(bytesDone) / Convert.ToDouble(TotalBytes)) * 100);
                            FireEvent(bytesDone);
                        } while (bytesRead > 0);
                    }
                }
            } catch (Exception) {
                bytesDone = 0;
            } finally {
                if (res != null)
                    res = null;
                if (readStream != null)
                    readStream.Close();
                if (writeStream != null)
                    writeStream.Close();
            }

            if (bytesDone > 0)
                return true;

            return false;
        }

        private string _localfile() {
            return Path.GetTempFileName();
        }
    }
}
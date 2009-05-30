﻿using System;
using System.Collections.Generic;
using System.Text;
using OMLEngine;        // need this for OML Title class
using OMLSDK;           // need this for the IOMLMetadataPlugin
using System.IO;
using System.Net;
using System.Xml;
using System.Globalization;

namespace TVDBMetadata
{
    public class TheTVDBDbResult
    {
        public Title Title { get; set; }
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlThumb { get; set; }

        public TheTVDBDbResult()
        {
            Title = new Title();
        }
    }


    public class TVDBMetadataPlugin : IOMLMetadataPlugin
    {

        IList<string> BackDrops = null;
        private const string API_KEY = "";
        private const string API_URL_SEARCH = "";
        private const string API_URL_INFO = "";

        private List<TheTVDBDbResult> results = null;
        
        
        public string PluginName { get { return "thetvdb.com"; } }


        // these 2 methods must be called in sequence
        public bool Initialize(Dictionary<string, string> parameters)
        {
            return true;
        }

        public bool SearchForMovie(string movieName)
        {
            SearchForMovies(movieName);

            return (results != null && results.Count != 0);
        }

        // these methods are to be called after the 2 methods above

        // get the best match
        public Title GetBestMatch()
        {
            if (results != null)
            {
                if (results.Count != 0)
                {
                    // load up the big image
                    return GetMovieDetails(results[0].Id);
                }
            }
            return null;
        }


        // or choose among all the titles
        public Title[] GetAvailableTitles()
        {
            Title[] titles = new Title[results.Count];
            for (int x = 0; x < results.Count; x++)
                titles[x] = results[x].Title;

            return titles;
        }

        public Title GetTitle(int index)
        {
            return GetMovieDetails(results[index].Id);
        }

        public List<OMLMetadataOption> GetOptions()
        {
            return null;
        }

        public bool SetOptionValue(string option, string value)
        {
            return true;
        }

        /// <summary>
        /// Creates a result object from the movie result node - returns null if it's not a valid result
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private TheTVDBDbResult GetTitleFromMovieNode(XmlTextReader reader)
        {
            this.BackDrops = null;
            bool notMovie = false;
            TheTVDBDbResult result = new TheTVDBDbResult();

            while (reader.Read())
            {
                if (reader.Value == "Your query didn't return any results.")
                    return null;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {



                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement &&
                    reader.Name == "movie")
                    break;

                // if we're not a movie let's move on
                if (notMovie)
                    break;
            }

            return (notMovie) ? null : result;
        }

        private Title GetMovieDetails(int movieId)
        {
            UriBuilder uri = new UriBuilder(API_URL_INFO);
            uri.Query = "api_key=" + API_KEY + "&id=" + movieId.ToString();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri.Uri);

            TheTVDBDbResult title = null;

            // execute the request
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // we will read data via the response stream
                using (Stream resStream = response.GetResponseStream())
                {
                    XmlTextReader reader = new XmlTextReader(resStream);
                    reader.WhitespaceHandling = WhitespaceHandling.None;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "movie")
                            {
                                title = GetTitleFromMovieNode(reader);
                                break;
                            }
                        }
                    }
                }
            }

            // load up the big image
            if (title != null)
            {
                DownloadImage(title.Title, title.ImageUrl);
            }

            return (title != null) ? title.Title : null;
        }

        /// <summary>
        /// Fills the local results with movies
        /// </summary>
        /// <param name="searchQuery"></param>
        private void SearchForMovies(string searchQuery)
        {
            UriBuilder uri = new UriBuilder(API_URL_SEARCH);
            uri.Query = "api_key=" + API_KEY + "&title=" + searchQuery;

            results = new List<TheTVDBDbResult>();

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri.Uri);

            // execute the request
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // we will read data via the response stream
                using (Stream resStream = response.GetResponseStream())
                {
                    XmlTextReader reader = new XmlTextReader(resStream);
                    reader.WhitespaceHandling = WhitespaceHandling.None;

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "movie")
                            {
                                TheTVDBDbResult title = GetTitleFromMovieNode(reader);

                                if (title != null)
                                    results.Add(title);
                            }
                        }
                    }
                }
            }

            // load up all the titles with images
            foreach (TheTVDBDbResult title in results)
            {
                DownloadImage(title.Title, title.ImageUrlThumb);
            }
        }

        /// <summary>
        /// Returns the text field value of a node or empty string if it doesn't have one
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private string GetElementValue(XmlTextReader reader)
        {
            string returnValue = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    returnValue = reader.Value;
                    break;
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    returnValue = string.Empty;
                    break;
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Returns if the 0 first attribute of the current element has the given value
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        private bool IsAttributeValue(XmlTextReader reader, string value)
        {
            bool found = false;
            if (reader.AttributeCount != 0 &&
                string.Equals(reader.GetAttribute(0), value, StringComparison.OrdinalIgnoreCase))
            {
                found = true;
            }

            return found;
        }

        /// <summary>
        /// Downloads the image for the results url and sets it to the internal title object
        /// </summary>
        /// <param name="result"></param>
        private void DownloadImage(Title title, string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                string tempFileName = Path.GetTempFileName();
                WebClient web = new WebClient();
                try
                {
                    web.DownloadFile(imageUrl, tempFileName);
                    title.FrontCoverPath = tempFileName;
                }
                catch
                {
                    File.Delete(tempFileName);
                }
            }
        }

        public bool SupportsBackDrops()
        {
            return false;
        }

        public void DownloadBackDropsForTitle(Title t, int index)
        {
            /*if (results.Count >= index)
            {
                if (this.BackDrops == null)
                    return;

                WebClient web = new WebClient();

                foreach (string backDropUrl in this.BackDrops)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(backDropUrl))
                        {
                            string filename = Path.Combine(FileSystemWalker.ImageDownloadDirectory, Guid.NewGuid().ToString());
                            web.DownloadFile(backDropUrl, filename);

                            t.AddFanArtImage(filename);
                        }
                    }
                    catch
                    {
                    }
                }
            }*/
        }
    }
}

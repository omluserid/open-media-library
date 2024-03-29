﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using OMLSDK;
using System.IO;
using System.Net;
using System.Web;
using System.Xml;
using System.Globalization;
using System.Text.RegularExpressions;


namespace NetFlixMetadata
{
    public class NetFlixDbResult
    {
        public OMLSDKTitle Title { get; set; }
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public string ImageUrlThumb { get; set; }

        public NetFlixDbResult()
        {
            Title = new OMLSDKTitle();
        }
    }

    public class NetFlixDb : IOMLMetadataPlugin
    {
        private const string DEFAULT_API_KEY = @"r548xb5mryvw2d9ewpzwh6bg";
        private const string DEFAULT_SHARED_SECRET = @"udYADAm3W2";
        private string API_KEY;
        private string SHARED_SECRET;
        private const string API_KEY_HIDDEN_TEXT = "[Enter your API key here to override the OML key]";
        private const string SHARED_SECRET_HIDDEN_TEXT = "[Enter your API key here to override the OML secret]";

        private const string HTML_TAG_PATTERN = @"<(.|\n)*?>";

        private List<NetFlixDbResult> results = null;

        public List<MetaDataPluginDescriptor> GetProviders
        {
            get
            {
                List<MetaDataPluginDescriptor> descriptors = new List<MetaDataPluginDescriptor>();

                MetaDataPluginDescriptor descriptor = new MetaDataPluginDescriptor();
                descriptor.DataProviderName = "NetFlix";
                descriptor.DataProviderMessage = "Data provided by NetFlix";
                descriptor.DataProviderLink = "http://www.netflix.com";
                descriptor.DataProviderCapabilities = MetadataPluginCapabilities.SupportsMovieSearch;
                descriptor.PluginDLL = null;
                descriptors.Add(descriptor);
                return descriptors;
            }
        }

        public bool Initialize(string provider, Dictionary<string, string> parameters)
        {
            // Set default api info
            API_KEY = DEFAULT_API_KEY;
            SHARED_SECRET = DEFAULT_SHARED_SECRET;

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                SetOptionValue(parameter.Key, parameter.Value);
            }

            return true;
        }

        public string getSignedUrl(string url)
        {
            string normalizedUrl = string.Empty;
            string normalizedReqParams = string.Empty;
            OAuth.OAuthBase oauth = new OAuth.OAuthBase();
            string signature = oauth.GenerateSignature(new Uri(url), API_KEY, SHARED_SECRET, null, null, "GET", oauth.GenerateTimeStamp(), oauth.GenerateNonce(), out normalizedUrl, out normalizedReqParams);
            
            signature = HttpUtility.UrlEncode(signature);
            normalizedReqParams = string.Join("&", new string[] { normalizedReqParams, string.Format("oauth_signature={0}", signature) });
            string finalUrl = string.Join("?", new string[] { normalizedUrl, normalizedReqParams });
            
            return finalUrl;
        }

        public bool SearchForMovie(string movieName, int maxResults)
        {
            SearchForMovies(movieName, maxResults);
            
            return (results != null && results.Count != 0);
        }

        public OMLSDKTitle GetBestMatch()
        {
            return (results != null && results.Count != 0)
                ? GetMovieDetails(results[0].Id)
                : null;
        }

        public OMLSDKTitle[] GetAvailableTitles()
        {
            OMLSDKTitle[] titles = new OMLSDKTitle[results.Count];
            for (int x = 0; x < results.Count; x++)
                titles[x] = results[x].Title;

            return titles;
        }

        public OMLSDKTitle GetTitle(int index)
        {
            return GetMovieDetails(results[index].Id);
        }

        public List<OMLMetadataOption> GetOptions()
        {
            List<OMLMetadataOption> options = new List<OMLMetadataOption>();

            OMLMetadataOption apikey = null;
            OMLMetadataOption sharedsecret = null;

            if (API_KEY == DEFAULT_API_KEY)
            {
                apikey = new OMLMetadataOption("API Key", API_KEY_HIDDEN_TEXT, null, false);
            }
            else
            {
                apikey = new OMLMetadataOption("API Key", API_KEY, null, false);
            }

            if (SHARED_SECRET == DEFAULT_SHARED_SECRET)
            {
                sharedsecret = new OMLMetadataOption("Shared Secret", SHARED_SECRET_HIDDEN_TEXT, null, false);
            }
            else
            {
                sharedsecret = new OMLMetadataOption("Shared Secret", SHARED_SECRET, null, false);
            }

            options.Add(apikey);
            options.Add(sharedsecret);

            return options;
        }

        public bool SetOptionValue(string option, string value)
        {
            if (string.Compare(option, "API Key", true) == 0)
            {  
                API_KEY = DEFAULT_API_KEY;

                if (!string.IsNullOrEmpty(value))
                {
                    if (value != API_KEY_HIDDEN_TEXT)
                    {
                        API_KEY = value;
                    }
                }
            }

            if (string.Compare(option, "Shared Secret", true) == 0)
            {
                SHARED_SECRET = DEFAULT_SHARED_SECRET;

                if (!string.IsNullOrEmpty(value))
                {
                    if (value != SHARED_SECRET_HIDDEN_TEXT)
                    {
                        SHARED_SECRET = value;
                    }
                }
            }
            return true;
        }

        private NetFlixDbResult GetTitleFromMovieNode(XmlTextReader reader)
        {
            bool isMovie = true;
            NetFlixDbResult result = new NetFlixDbResult();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "id":
                            try { result.Id =int.Parse( FindFirstSubstring(GetElementValue(reader), "[0-9]+$", true)); }
                            catch { }
                            break;

                        case "title":
                            result.Title.Name = GetElementValue(reader, "attribute", "regular");
                            break;
                        
                        // Synopsis and other data comes from other api links
                        case "link":
                            string attrTitle = null;
                            string attrUrl = null;
                            string attrRel = null;
                            
                            if (reader.HasAttributes)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    switch (reader.Name)
                                    {
                                        case "href":
                                            attrUrl = reader.Value;
                                            break;
                                        case "rel":
                                            attrRel = reader.Value;
                                            break;
                                        case "title":
                                            attrTitle = reader.Value;
                                            break;
                                    }
                                }
                                // Move the reader back to the element node.
                                reader.MoveToElement();
                            }
                            
                            // Find out which item we have here
                            string attrSignedUrl = getSignedUrl(attrUrl);
                            switch (attrTitle)
                            {
                                case "synopsis":
                                    try { result.Title.Synopsis = getNonHTML(getExternalData(attrUrl, attrTitle)); }
                                    catch { result.Title.Synopsis = ""; }
                                    break;
                                case "cast":
                                    try 
                                    {
                                        // Cast separated by:  :::
                                        string cast = getNonHTML(getExternalData(attrUrl, attrTitle));
                                        string[] actors = Regex.Split(cast, ":::");
                                        for (int i = 0; i < actors.Length; i++)
                                        {
                                            result.Title.AddActingRole(actors[i], string.Empty);
                                        }
                                    }
                                    catch {  }
                                    break;
                                case "directors":
                                    try 
                                    {
                                        // Cast separated by:  :::
                                        string directors = getNonHTML(getExternalData(attrUrl, attrTitle));
                                        string[] dirs = Regex.Split(directors, ":::");
                                        for (int i = 0; i < dirs.Length; i++)
                                        {
                                            result.Title.AddDirector(new OMLSDKPerson(dirs[i]));
                                        }
                                    }
                                    catch {  }
                                    break;
                                case "web page":
                                    result.Title.OfficialWebsiteURL = attrUrl;
                                    break;
                            }
                            break;

                        case "release_year":
                            try { result.Title.ReleaseDate = new DateTime(int.Parse(GetElementValue(reader)), 1, 1); }
                            catch { }
                            break;

                        case "box_art":
                            result.ImageUrl = GetElementValue(reader, "attribute", "large");
                            result.ImageUrlThumb = result.ImageUrl;
                            break;

                        case "runtime":
                            result.Title.Runtime = int.Parse(GetElementValue(reader))/60;
                            break;

                        case "category":
                            string scheme = GetElementValue(reader, "attribute", "scheme", false);
                            string label = GetElementValue(reader, "attribute", "label");
                            if (scheme == "http://api.netflix.com/categories/genres")
                            {
                                result.Title.AddGenre(label);
                            }
                            else if (scheme == "http://api.netflix.com/categories/mpaa_ratings")
                            {
                                result.Title.ParentalRatingReason = label;
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "catalog_title") break;

                // See if Television is in the genre list to determine if its a movie or not
                // if we're not a movie let's move on
                if (!isMovie) break;
            }

            return (!isMovie) ? null : result;
        }

        // Data is located on another page, get it here
        private string getExternalData(string exUrl, string field)
        {
            string foundVal = null;
            exUrl = getSignedUrl(exUrl);
            string startPath = null;
            string collectionOf = null;
            string valueType = null;
            string valueFrom = null;
            switch (field)
            {
                case "synopsis":
                    startPath = "xml/";
                    collectionOf = "synopsis";
                    valueType = "cdata";
                    break;
                case "directors":
                case "cast":
                    startPath = "xml/people/";
                    collectionOf = "person";
                    valueFrom = "name";
                    //valueType = "cdata";
                    break;
            }

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(exUrl);
            request.AllowAutoRedirect = true;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // we will read data via the response stream
                using (Stream resStream = response.GetResponseStream())
                {
                    XmlTextReader reader = new XmlTextReader(resStream);
                    string pathSoFar = "";
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Whitespace) continue;

                        pathSoFar += reader.Name + "/";
                        if (startPath.Substring(0, pathSoFar.Length) != pathSoFar) return "";
                        if (pathSoFar == startPath)
                        {
                            foundVal = null;
                            while (reader.Read())
                            {
                                if(reader.Name == collectionOf)
                                {
                                    if (reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Whitespace) continue;
                                    if (valueFrom != null)
                                    {
                                        while (reader.Read())
                                        {
                                            if (reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Whitespace) continue;
                                            if (reader.Name != valueFrom) continue;
                                            if (valueType == "cdata") reader.Read();
                                            string tmpName = reader.Name;
                                            if (foundVal != null) foundVal += ":::";
                                            string tmpVal = GetElementValue(reader, valueType, "");
                                            foundVal += tmpVal;
                                        }
                                    }
                                    else
                                    {
                                        if (valueType == "cdata") reader.Read();
                                        if (foundVal != null) foundVal += "|";
                                        foundVal += GetElementValue(reader, valueType, "");
                                    }
                                }
                            }
                            
                        }
                    }
                }
            }
            return foundVal;
        }
        private OMLSDKTitle GetMovieDetails(int movieId)
        {
            // load up all the titles with images
            foreach (NetFlixDbResult title in results)
            {
                if (title.Id != movieId) continue;

                DownloadImage(title.Title, title.ImageUrlThumb);
                return title.Title;
            }

            return null;
        }

        private void SearchForMovies(string query, int maxResults)
        {
            results = new List<NetFlixDbResult>();

            string queryUrl = getSignedUrl("http://api.netflix.com/catalog/titles?term=" + query + "&max_results=10");
            

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(queryUrl);

            // execute the request
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                // we will read data via the response stream
                using (Stream resStream = response.GetResponseStream())
                {
                    XmlTextReader reader = new XmlTextReader(resStream);

                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            if (reader.Name == "catalog_title")
                            {
                                NetFlixDbResult title = GetTitleFromMovieNode(reader);
                                if (title != null)results.Add(title);
                                if (results.Count >= maxResults) break;
                            }
                        }
                    }
                }
            }

            // load up all the titles with images
            foreach (NetFlixDbResult title in results)
            {
                if (title.ImageUrlThumb != null)
                {
                    title.Title.FrontCoverPath = title.ImageUrlThumb;
                }
                //DownloadImage(title.Title, title.ImageUrlThumb);
            }
        }

        private string GetElementValue(XmlTextReader reader)
        {
            return GetElementValue(reader, "value", null);
        }
        private string GetElementValue(XmlTextReader reader, string valueType, string valueTarget)
        {
            return GetElementValue(reader, valueType, valueTarget, true);
        }
        private string GetElementValue(XmlTextReader reader, string valueType, string valueTarget, bool postAttrAdvance)
        {
            // Check by attribute value
            if (valueType == "attribute")
            {
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        if (reader.Name == valueTarget) return reader.Value;
                    }
                    // Move the reader back to the element node.
                    if (postAttrAdvance) reader.MoveToElement();
                }
                return "";
            }

            // CHeck by CDATA
            if (valueType == "cdata")
            {
                if (reader.NodeType == XmlNodeType.CDATA)
                {
                    string foundVal = reader.Value;
                    // Move the reader back to the element node.
                    if (postAttrAdvance) reader.MoveToElement();
                    return foundVal;
                }
            }

            // Check by value
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

        private void DownloadImage(OMLSDKTitle title, string imageUrl)
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

        public List<string> GetBackDropUrlsForTitle()
        {
            return null;
        }

        private static string getNonHTML(string inputString)
        {
            return Regex.Replace(inputString, HTML_TAG_PATTERN, string.Empty);
        }


        public bool SearchForTVSeries(string SeriesName, string EpisodeName, int? SeriesNo, int? EpisodeNo, int maxResults, bool SearchTVShowOnly)
        {
            return false;
        }
        public bool SearchForTVDrillDown(int id, string EpisodeName, int? SeriesNo, int? EpisodeNo, int maxResults)
        {
            return false;
        }



        public static Match[] FindSubstrings(string source, string matchPattern, bool findAllUnique)
        {
            SortedList uniqueMatches = new SortedList();
            Match[] retArray = null;

            Regex RE = new Regex(matchPattern, RegexOptions.Multiline);
            MatchCollection theMatches = RE.Matches(source);

            if (findAllUnique)
            {
                for (int counter = 0; counter < theMatches.Count; counter++)
                {
                    if (!uniqueMatches.ContainsKey(theMatches[counter].Value))
                    {
                        uniqueMatches.Add(theMatches[counter].Value,
                                          theMatches[counter]);
                    }
                }

                retArray = new Match[uniqueMatches.Count];
                uniqueMatches.Values.CopyTo(retArray, 0);
            }
            else
            {
                retArray = new Match[theMatches.Count];
                theMatches.CopyTo(retArray, 0);
            }

            return (retArray);
        }


        public static string FindFirstSubstring(string source, string matchPattern, bool findAllUnique)
        {
            Match[] matches = FindSubstrings(source, matchPattern, findAllUnique);

            if (matches.Length > 0)
            {
                return matches[0].ToString();
            }

            return "";
        } 
    }
}

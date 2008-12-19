﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using OMLEngine;
using OMLSDK;

namespace VMCDVDLibraryPlugin
{
    public class DVDLibraryImporter : OMLPlugin, IOMLPlugin
    {
        private enum DirectoryType
        {
            HDDvd,
            BluRay,
            DVD_Ripped,
            DVD_Flat,
            Normal
        }

        public DVDLibraryImporter()
            : base()
        {
            Utilities.DebugLine("[DVDLibraryImporter] created");
        }

        public override bool IsSingleFileImporter()
        {
            return false;
        }

        public override string SetupDescription()
        {
            return GetName() + @" will search for and import all files used by the VMC DVD Library as well as video files";
        }

        protected override bool GetFolderSelect()
        {
            return true;
        }
        private static double MajorVersion = 0.9;
        private static double MinorVersion = 0.1;
        protected override double GetVersionMajor()
        {
            return MajorVersion;
        }

        protected override double GetVersionMinor()
        {
            return MinorVersion;
        }
        protected override string GetMenu()
        {
            return "Scan Folders For DVDs and Videos";
        }
        protected override string GetName()
        {
            return "Scan Folders For DVDs and Videos";
        }
        protected override string GetAuthor()
        {
            return "OML Development Team";
        }
        protected override string GetDescription()
        {
            return @"DVD Library and Video importer v" + Version;
        }
        public override bool Load(string filename)
        {
            GetMovies(filename);
            return true;
        }

        public override void DoWork(string[] thework)
        {
            GetMovies(thework[0]);
        }


        public void GetMovies( string startFolder )
        {
            try
            {
                List<string> dirList = new List<string>();
                List<string> fileList = new List<string>();
                GetSubFolders(startFolder, dirList);

                // the share or link may not exist nowbb
                if (Directory.Exists(startFolder))
                {
                    dirList.Add(startFolder);
                }
 
                foreach (string currentFolder in dirList)
                {
                    Utilities.DebugLine("DVDImporter: folder " + currentFolder);
                    if (currentFolder.Contains("MSDVR"))
                    {
                        // bool b = true;
                    }
                    Title dvd = GetDVDMetaData(currentFolder);
                    string[] fileNames = null;
                    try
                    {
                        fileNames = Directory.GetFiles(currentFolder);
                    }
                    catch
                    {
                        fileNames = null;
                    }

                    if (dvd != null)
                    {
                        // if any video files are found in the DVD folder assume they are trailers
                        if (fileNames != null)
                        {
                            foreach (string video in fileNames)
                            {
                                string extension = Path.GetExtension(video).ToUpper();
                                if (SupportedVideoExtensions.Contains(extension))
                                {
                                    dvd.Trailers.Add(video);
                                }
                            }
                        }
                        AddTitle(dvd);
                    }// found dvd
                    else
                    {
                        if( fileNames != null )
                        {
                            foreach (string video in fileNames)
                            {
                                string extension = Path.GetExtension(video).ToUpper().Substring(1);
                                extension = extension.Replace("-", "");
                                if (SupportedVideoExtensions.Contains(extension))
                                {
                                    Title newVideo = new Title();
                                    newVideo.Name = GetSuggestedMovieName(Path.GetFileNameWithoutExtension(video));
                                    Disk disk = new Disk();
                                    disk.Path = video;
                                    disk.Name = "Disk 1";
                                    disk.Format = (VideoFormat)Enum.Parse(typeof(VideoFormat), extension, true);

                                    string pathWithNoExtension = Path.GetDirectoryName(video) + "\\" + Path.GetFileNameWithoutExtension(video);
                                    if (File.Exists(pathWithNoExtension + ".jpg"))
                                    {
                                        SetFrontCoverImage(ref newVideo, pathWithNoExtension + ".jpg");
                                    }
                                    else if (File.Exists(video + ".jpg"))
                                    {
                                        SetFrontCoverImage(ref newVideo, video + ".jpg");
                                    }
                                    else if (File.Exists(Path.GetDirectoryName(video) + "\\folder.jpg"))
                                    {
                                        SetFrontCoverImage(ref newVideo, Path.GetDirectoryName(video) + "\\folder.jpg");
                                    }

                                    newVideo.Disks.Add(disk);
                                    AddTitle(newVideo);
                                }
                            }
                        }

                    }
                } // loop through the sub folders
            }
            catch (Exception ex)
            {
                Utilities.DebugLine("[DVDLibraryImporter] An error occured: " + ex.Message);
            }
        }

        private Title GetDVDMetaData(string folderName)
        {
            try
            {
                string xmlFile = "";

                DirectoryType dirType = GetDirectoryType(folderName);

                if (dirType != DirectoryType.Normal )
                {
                    string[] xmlFiles = Directory.GetFiles(folderName, "*dvdid.xml");
                    if (xmlFiles.Length > 0)
                    {
                        //xmlFile = Path.GetFileName(xmlFiles[0]); // get the first one
                        xmlFile = xmlFiles[0];
                    }
                    //else
                    //{
                    //    xmlFiles = Directory.GetFiles(folderName, "*xml");
                    //    if (xmlFiles.Length > 0)
                    //        //xmlFile = Path.GetFileName(xmlFiles[0]); // get the first one
                    //        xmlFile = xmlFiles[0]; // get the first one
                    //    else
                    //        xmlFile = "";
                    //}

                    Title t = null;
                    // xmlFile contains the dvdid - then we need to lookup in the cache folder based on this id
                    if (xmlFile != "" && File.Exists(xmlFile))
                    {
                        string dvdid = GetDVDID(xmlFile);
                        string xmlDetailsFile = GetDVDCacheFileName(dvdid);
                        t = ReadMetaData(folderName, xmlDetailsFile);
                    }

                    if( t == null )
                    {
                        // for DVDs with no dvdid.xml add a stripped down title with just a suggested name
                        t = new Title();
                        t.Name = GetSuggestedMovieName(folderName);

                    }

                    if (!File.Exists(t.FrontCoverPath))
                    {
                        if( File.Exists( folderName + "\\folder.jpg") )
                            t.FrontCoverPath = folderName + "\\folder.jpg";
                    }

                    t.ImporterSource = "VMCDVDLibraryPlugin";
                    Disk disk = new Disk();
                    disk.Name = "Disk 1";                    

                    switch (dirType)
                    {
                        case DirectoryType.BluRay:
                            disk.Format = VideoFormat.BLURAY;
                            disk.Path = folderName;
                            break;

                        case DirectoryType.HDDvd:
                            disk.Format = VideoFormat.HDDVD;
                            disk.Path = folderName;
                            break;

                        case DirectoryType.DVD_Flat:
                            disk.Format = VideoFormat.DVD;
                            disk.Path = folderName;
                            break;

                        case DirectoryType.DVD_Ripped:
                            disk.Format = VideoFormat.DVD;
                            disk.Path = Path.Combine(folderName, "VIDEO_TS");
                            break;
                    }                    

                    t.Disks.Add(disk);
                    t.MetadataSourceName = "VMC DVD Library";
                    return t;
                }
            }
            catch (Exception ex)
            {
                Utilities.DebugLine("[DVDLibraryImporter] An error occured: " + ex.Message);
            }

            return null;
        }

        private Title ReadMetaData(string movieFolder, string dvdCacheXMLFile)
        {
            Title t = new Title();
            try
            {
                if (File.Exists(dvdCacheXMLFile))
                {
                    XmlReaderSettings xmlSettings = new XmlReaderSettings();
                    xmlSettings.IgnoreWhitespace = false;
                    using (XmlReader reader = XmlReader.Create(dvdCacheXMLFile, xmlSettings))
                    {
                        bool bFound = reader.ReadToFollowing("MDR-DVD");
                        if (!bFound) return null;

                        bFound = reader.ReadToFollowing("dvdTitle");
                        if (!bFound) return null;
                        t.Name = reader.ReadString().Trim();

                        bFound = reader.ReadToFollowing("studio");
                        if (bFound)
                            t.Studio = reader.ReadString().Trim();

                        bFound = reader.ReadToFollowing("leadPerformer");
                        if (bFound)
                        {
                            string leadPerformer = reader.ReadString();
                            string[] actors = leadPerformer.Split(new char[] { ';', ',', '|' });
                            foreach (string actor in actors)
                            {
                                t.AddActingRole(actor.Trim(), "");
                            }
                        }

                        bFound = reader.ReadToFollowing("director");
                        if (bFound)
                        {
                            string directorList = reader.ReadString();
                            string[] directors = directorList.Split(new char[] { ';', ',', '|' });
                            foreach (string director in directors)
                            {
                                t.AddDirector(new Person(director.Trim()));
                            }
                        }

                        bFound = reader.ReadToFollowing("MPAARating");
                        if (bFound)
                            t.ParentalRating = reader.ReadString().Trim();

                        bFound = reader.ReadToFollowing("language");
                        if (bFound)
                            t.AddLanguageFormat(reader.ReadString().Trim());

                        bFound = reader.ReadToFollowing("releaseDate");
                        if (bFound)
                        {
                            string releaseDate = reader.ReadString().Trim();
                            DateTime dt;
                            if (DateTime.TryParse(releaseDate, out dt))
                            {
                                t.ReleaseDate = dt;
                            }
                        }

                        bFound = reader.ReadToFollowing("genre");
                        if (bFound)
                        {
                            string genreList = reader.ReadString();
                            string[] genres = genreList.Split(new char[] { ';', ',', '|' });
                            foreach (string genre in genres)
                            {
                                t.AddGenre(genre.Trim());
                            }
                        }

                        string imageFileName = "";
                        bFound = reader.ReadToFollowing("largeCoverParams");
                        if (bFound)
                        {
                            imageFileName = reader.ReadString().Trim();
                            if (!File.Exists(imageFileName) && File.Exists(movieFolder + "\\folder.jpg"))
                            {
                                imageFileName = movieFolder + "\\folder.jpg";
                            }
                        }
                        else if( File.Exists(movieFolder + "\\folder.jpg" ) )
                        {
                            imageFileName = movieFolder + "\\folder.jpg";
                        }

                        SetFrontCoverImage(ref t, imageFileName);

                        bFound = reader.ReadToFollowing("duration");
                        if (bFound)
                        {
                            int runtime;
                            if( Int32.TryParse(reader.ReadString(), out runtime))
                            {
                                t.Runtime = runtime;
                            }
                        }

                        bFound = reader.ReadToFollowing("synopsis");
                        if (bFound)
                            t.Synopsis = reader.ReadString().Trim();

                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Utilities.DebugLine("[DVDLibraryImporter] An error occured: " + ex.Message);
                return null;
            }

            return t;
        }

        //
        // the dvid.xml file contains the Name of the movie and the DVDID
        //
        private string GetDVDID( string dvdidXMLFile )
        {
            string dvdid = "";
            try
            {
                XmlReaderSettings xmlSettings = new XmlReaderSettings();
                xmlSettings.IgnoreWhitespace = true;
                using (XmlReader reader = XmlReader.Create(dvdidXMLFile, xmlSettings))
                {
                    reader.MoveToContent();
                    string currentElement = reader.Name;

                    // some dvdid.xml files use upper case, some used mixed case
                    // we cannot read an element based on name because i
                    if (currentElement.ToUpper() == "DISC")
                    {
                        reader.Read();
                        while (!reader.IsStartElement()) reader.Read(); // skip to next element
                        currentElement = reader.Name;

                        if (currentElement.ToUpper() == "NAME")
                        {
                            reader.ReadStartElement(reader.Name);
                            //m_DVDName = reader.ReadString();
                            reader.ReadString();
                            reader.Read();
                            while (!reader.IsStartElement()) reader.Read(); // skip to next element
                            currentElement = reader.Name;
                            if (currentElement.ToUpper() == "ID")
                            {
                                reader.ReadStartElement(reader.Name);
                                dvdid = reader.ReadString();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.DebugLine("[DVDLibraryImporter] An error occured: " + ex.Message);
            }
            return dvdid;
        }

        private string GetDVDCacheFileName(string dvdid)
        {
            string xmlFileName = dvdid.Replace("|", "-") + ".xml";
            return DVDCacheFolder + "\\" + xmlFileName;
        }

        // gets the local machine's VMC DVD cache folder
        private string DVDCacheFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\eHome\\DvdInfoCache"; }
        }

        private string GetSuggestedMovieName(string fullPath)
        {
            string suggestedName = Path.GetFileName(fullPath);
            suggestedName = suggestedName.Trim();
            suggestedName = suggestedName.Replace('_', ' ');
            suggestedName = suggestedName.Replace('-', ' ');
            suggestedName = suggestedName.Replace('.', ' ');
            return suggestedName;
        }

        private void GetSubFolders(string startFolder, List<string> folderList)
        {
            DirectoryInfo[] diArr = null;
            try
            {
                DirectoryInfo di = new DirectoryInfo(startFolder);
                diArr = di.GetDirectories();
                foreach (DirectoryInfo folder in diArr)
                {
                    folderList.Add(folder.FullName);

                    // ignore subdirectories of movie folders
                    if (GetDirectoryType(folder.FullName) == DirectoryType.Normal)
                    {
                        GetSubFolders(folder.FullName, folderList);
                    }
                }

            }
            catch (Exception ex)
            {
                Utilities.DebugLine("[DVDLibraryImporter] An error occured: " + ex.Message);
                // ignore any permission errors
            }
        }

        /// <summary>
        /// Gets what kind of movie this directory may contain
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private DirectoryType GetDirectoryType(string folder)
        {
            if (Directory.Exists(Path.Combine(folder, "VIDEO_TS")))
            {
                return DirectoryType.DVD_Ripped;
            }
            else if (File.Exists(Path.Combine(folder, "VIDEO_TS.IFO")) ||
                       File.Exists(Path.Combine(folder, "vts_01_1.vob")))
            {
                return DirectoryType.DVD_Flat;
            }
            else if (Directory.Exists(Path.Combine(folder, "BDMV")))
            {
                return DirectoryType.BluRay;
            }
            else if (Directory.Exists(Path.Combine(folder, "HVDVD_TS")))
            {
                return DirectoryType.HDDvd;
            }

            return DirectoryType.Normal;
        }

        private void UpdateTitleFromOMLXML(Title t)
        {
        }

        HashSet<string> SupportedVideoExtensions = new HashSet<string>() 
        {
        "ASF",
        "AVC",
        "AVI", // DivX, Xvid, etc
        "B5T", // BlindWrite image
        "B6T", // BlindWrite image
        "BIN", // using an image loader lib and load/play this as a DVD
        "BWT", // BlindWrite image
        "CCD", // CloneCD image
        "CDI", // DiscJuggler Image
        "CUE", // cue sheet
        "DVR-MS", // MPG
        "DVRMS", // MPG
        "H264", // AVC OR MP4
        "IMG", // using an image loader lib and load/play this as a DVD
        "ISO", // Standard ISO image
        "ISZ", // Compressed ISO image
        "MDF", // using an image loader lib and load/play this as a DVD
        "MDS", // Media Descriptor file
        "MKV", // Likely h264
        "MOV", // Quicktime
        "MPG",
        "MPEG",
        "MP4", // DivX, AVC, or H264
        "NRG", // Nero image
        "OGM", // Similar to MKV
        "PDI", // Instant CD/DVD image
        "TS", // MPEG2
        "UIF",
        "WMV",
        //"VOB", // MPEG2 - make sure it's not part of a DVD
        "WVX", // wtf is this?
        "ASX", // wtf is this?
        "WPL" // playlist file?
        };
    }
}
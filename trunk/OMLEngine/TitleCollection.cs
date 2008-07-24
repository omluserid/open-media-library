using System;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace OMLEngine
{
    [Serializable()]
    public class TitleCollection : ISerializable
    {
        private List<Title> _list = new List<Title>();
        private SourceDatabase _source_database_to_use;
        private string _database_filename;
        private Dictionary<string, Title> _moviesByFilename = new Dictionary<string, Title>();
        private Dictionary<int, Title> _moviesByItemId = new Dictionary<int, Title>();

        public Dictionary<int, Title> MoviesByItemId
        {
            get { return _moviesByItemId; }
        }

        public bool ContainsDisks(List<Disk> disks)
        {
            if (disks.Count == 0) return false;
            return (_moviesByFilename.ContainsKey(GetDiskHash(disks)));
        }

        public List<Title>.Enumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public Title this[int index]
        {
            get 
            {
                if (index >= 0 && index < _list.Count)
                {
                    return _list[index];
                }
                else
                {
                    return null;
                }
            }
        }

        public void Sort()
        {
            _list.Sort();
        }

        public int Count
        {
            get { return _list.Count; }
        }


        public void Add(Title newTitle)
        {
            _list.Add(newTitle);

            string key = GetDiskHash(newTitle.Disks);
            if ( newTitle.Disks.Count > 0 && !_moviesByFilename.ContainsKey(key))
            {
                _moviesByFilename.Add(key, newTitle);
            }
            if (!_moviesByItemId.ContainsKey(newTitle.InternalItemID))
            {
                _moviesByItemId.Add(newTitle.InternalItemID, newTitle);
            }
        }

        public void Remove(Title newTitle)
        {
            _moviesByItemId.Remove(newTitle.InternalItemID);
            _moviesByFilename.Remove(GetDiskHash(newTitle.Disks));
            _list.Remove(newTitle);

        }

        private string GetDiskHash(List<Disk> disks)
        {
            string temp = "";
            foreach (Disk d in disks)
            {
                // we will concatonate the path + name for each disk and take the MD5 hash of that to determine if
                // the disk collection changed
                temp += d.Name + d.Path;
            }
            return CalculateMD5Hash(temp);
        }

        public void Replace(Title newTitle, Title oldTitle)
        {
            Remove(oldTitle);
            Add(newTitle);
        }

        public void Replace(Title title)
        {

            Utilities.DebugLine("[TitleCollection] Title ("+title.Name+") has been replaced");
            Title t = GetTitleById(title.InternalItemID);

            if (t != null)
            {
                int index = _list.IndexOf(t);
                _list[index] = title;
            }
            else
            {
                Add(title);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Title GetTitleById(int id)
        {
            Utilities.DebugLine("[TitleCollection] Title Lookup by Id: "+id);
            if (_moviesByItemId.ContainsKey(id))
            {
                return _moviesByItemId[id];
            }

            return null;
        }
        /// <summary>
        /// Get/Set the Source Database type to use
        /// </summary>
        public SourceDatabase SourceDatabaseToUse
        {
            get { return _source_database_to_use; }
            set
            {
                _source_database_to_use = value;
            }
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="database_filename">full path of filename to use as the database</param>
        public TitleCollection(string database_filename)
        {
            Utilities.DebugLine("[TitleCollection] TitleCollection(database_filename)");
            _source_database_to_use = SourceDatabase.OML;
            _database_filename = database_filename;
        }
        /// <summary>
        /// Generic constructor
        /// (Uses the default database file)
        /// </summary>
        public TitleCollection()
        {
            Utilities.DebugLine("[TitleCollection] TitleCollection()");
            _source_database_to_use = SourceDatabase.OML;
            _database_filename = Path.Combine(FileSystemWalker.PublicRootDirectory, @"oml.dat");
        }
        /// <summary>
        /// Default destructor
        /// </summary>
        ~TitleCollection()
        {
            Utilities.DebugLine("[TitleCollection] ~TitleCollection(): Holding " + _list.Count + " titles");
        }

        /// <summary>
        /// Saves the current list of titles to the db file
        /// </summary>
        /// <returns>True on success</returns>
        public bool saveTitleCollection()
        {
            Utilities.DebugLine("[TitleCollection] saveTitleCollection()");
            return _saveTitleCollectionForOML();
        }

        private bool _saveTitleCollectionForOML()
        {
            Stream stream;
            try
            {
                stream = File.OpenWrite(_database_filename);
            }
            catch (Exception e)
            {
                Utilities.DebugLine("[TitleCollection] Error reading file: " + e.Message);
                return false;
            }

            try
            {
                BinaryFormatter bformatter = new BinaryFormatter();
                bformatter.Serialize(stream, _list.Count);

                foreach (Title title in _list)
                {
                    //REMOVE DUPLICATES HERE FROM THE PRIMARY COLLECTION!!!!
                    bformatter.Serialize(stream, title);
                }

                stream.Close();
                return true;
            }
            catch (Exception e)
            {
                Utilities.DebugLine("Error writing file: " + e.Message);
                return false;
            }
        }

        /// <summary>
        /// Determines which db type to use and loads data from that source
        /// </summary>
        /// <returns>True on success</returns>
        public bool loadTitleCollection()
        {
            Utilities.DebugLine("[TitleCollection] :loadTitleCollection()");
            return _loadTitleCollectionFromOML();
        }

        /// <summary>
        /// Loads data from OML Database
        /// </summary>
        /// <returns>True on successful load</returns>
        private bool _loadTitleCollectionFromOML()
        {
            Utilities.DebugLine("[TitleCollection] Using OML database");
            Stream stm;
            try
            {
                stm = File.OpenRead(_database_filename);
            }
            catch (Exception ex)
            {
                Utilities.DebugLine("[TitleCollection] Error loading title collection: " + ex.Message);
                return false;
            }

            if (stm.Length > 0)
            {
                try
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    int numTitles = (int)bf.Deserialize(stm);
                    for (int i = 0; i < numTitles; i++)
                    {
                        Title t = (Title)bf.Deserialize(stm);
                        //Utilities.DebugLine("[TitleCollection] Adding Title: "+t.Name);
                        Add(t);
                    }
                    stm.Close();
                    Utilities.DebugLine("[TitleCollection] Loaded: " + numTitles + " titles");
                    return true;
                }
                catch (Exception e)
                {
                    Utilities.DebugLine("[TitleCollection] Failed to load db file: " + e.Message);
                }
            }
            return false;
        }

        private string CalculateMD5Hash(string input)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            return (Convert.ToBase64String(hash));            
        }



        #region serialization methods
        public TitleCollection(SerializationInfo info, StreamingContext ctxt)
        {
            Utilities.DebugLine("[TitleCollection] TitleCollection (Serialized)");
            List<Title> tc = (List <Title>)info.GetValue("TitleCollection", typeof(TitleCollection));
            foreach (Title title in tc)
                Add(title);
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            Utilities.DebugLine("[TitleCollection] GetObjectData()");
            info.AddValue("TitleCollection", _list);
        }
        #endregion
    }
}

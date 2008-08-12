﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using OMLEngine;

namespace OMLDatabaseEditor
{
    public partial class frmSearchResult : Form
    {
        Title[] _titles = null;
        Title _selectedTitle = null;
        bool _overwriteMetadata;

        public bool OverwriteMetadata
        {
            get { return _overwriteMetadata; }
            set { _overwriteMetadata = value; }
        }

        public Title SelectedTitle
        {
            get { return _selectedTitle; }
        }

        public frmSearchResult()
        {
            InitializeComponent();
        }

        private void frmSearchResult_Load(object sender, EventArgs e)
        {

        }

        private string MakeStringFromList(List<string> list)
        {
            string ret = "";
            if (list != null)
            {
                foreach (string s in list)
                {
                    if (ret.Length > 0) ret += ", ";
                    ret += s;
                }
            }
            return ret;
        }

        private string MakeStringFromPersonList(List<Person> list)
        {
            string ret = "";
            if (list != null)
            {
                foreach (Person p in list)
                {
                    if (ret.Length > 0) ret += ", ";
                    ret += p.full_name;
                }
            }
            return ret;
        }

        private string MakeStringFromDictionary(Dictionary<string,string> list)
        {
            string ret = "";
            if (list != null)
            {
                foreach (KeyValuePair<string, string> kvp in list)
                {
                    if (ret.Length > 0) ret += ", ";
                    ret += kvp.Key;
                }
            }
            return ret;
        }

        public DialogResult ShowResults(Title[] titles)
        {
            _titles = titles;
            if (titles != null)
            {
                int i = 0;
                foreach (Title t in titles)
                {
                    if (t != null)
                    {
                        Image coverArt = null;

                        if (t.FrontCoverPath != null && File.Exists(t.FrontCoverPath))
                        {
                            coverArt = Utilities.ReadImageFromFile(t.FrontCoverPath);
                        }
                        grdTitles.Rows.Add(i.ToString(), coverArt, t.Name, t.Synopsis, t.ReleaseDate.ToShortDateString(), MakeStringFromList(t.Genres), MakeStringFromPersonList(t.Directors), MakeStringFromDictionary(t.ActingRoles));
                        i++;
                    }
                }
            }
            return ShowDialog();
        }

        private void btnSelectMovie_Click(object sender, EventArgs e)
        {
            if (grdTitles.SelectedRows != null && grdTitles.SelectedRows.Count > 0)
            {
                if (chkUpdateMissingDataOnly.Checked) 
                    _overwriteMetadata = false;
                else
                    _overwriteMetadata = true;

                _selectedTitle = _titles[grdTitles.SelectedRows[0].Index];
            }
            else
            {
                _selectedTitle = null;
            }
        }
    }
}

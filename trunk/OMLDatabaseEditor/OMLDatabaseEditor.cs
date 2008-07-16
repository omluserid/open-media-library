﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OMLEngine;
using OMLSDK;

namespace OMLDatabaseEditor
{
    public partial class OMLDatabaseEditor : Form
    {
        private static TitleCollection _titleCollection = new TitleCollection();
        private TreeNode m_OldSelectNode;

        private static List<OMLPlugin> plugins = new List<OMLPlugin>();

        public OMLDatabaseEditor()
        {
            InitializeComponent();
            _titleCollection.loadTitleCollection();
            SetupTitleList();

            LoadPlugins();
            SetupPluginList();
        }

        private static void LoadPlugins()
        {
            List<PluginServices.AvailablePlugin> Pluginz = new List<PluginServices.AvailablePlugin>();
            string path = FileSystemWalker.PluginsDirectory;
            Pluginz = PluginServices.FindPlugins(path, "OMLSDK.IOMLPlugin");
            OMLPlugin objPlugin;
            // Loop through available plugins, creating instances and adding them
            foreach (PluginServices.AvailablePlugin oPlugin in Pluginz)
            {
                objPlugin = (OMLPlugin)PluginServices.CreateInstance(oPlugin);
                plugins.Add(objPlugin);
            }
            Pluginz = null;
        }

        private void SetupTitleList()
        {
            foreach (Title t in _titleCollection)
            {
                tvSourceList_AddItem(t.Name, t.InternalItemID.ToString(), "Movies");
            }
            tvSourceList.Sort();
        }

        private void SetupPluginList()
        {
            int i = 0;
            foreach (OMLPlugin plugin in plugins)
            {
                tvSourceList_AddItem(plugin.Menu, i.ToString(), "Importers");
                i++;
            }
        }

        private void tvSourceList_AddItem(string text, string name, string tag)
        {
            TreeNode node = new TreeNode();
            node.Name = name;
            node.Text = text;
            node.Tag = tag;
            tvSourceList.Nodes["OML Database"].Nodes[tag].Nodes.Add(node);
            tvSourceList.Nodes["OML Database"].ExpandAll();
            tvSourceList.Nodes["OML Database"].Nodes[tag].ExpandAll();
        }

        private void SaveTitleChangesToDB(Title t)
        {
            _titleCollection.Replace(t);
            _titleCollection.saveTitleCollection();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.tabsMediaPanel.SelectedTab != null)
            {
                Controls.MediaEditor _currentEditor = (Controls.MediaEditor)this.tabsMediaPanel.SelectedTab.Controls[0];
                Title _currentTitle = (Title)_titleCollection.MoviesByItemId[_currentEditor.itemID];

                _currentEditor.SaveToTitle(_currentTitle);
                SaveTitleChangesToDB(_currentTitle);
            }
        }

        private void SaveAll()
        {
            foreach (TabPage page in tabsMediaPanel.TabPages)
            {
                Controls.MediaEditor editor = (Controls.MediaEditor)page.Tag;
                if (editor.Status ==  global::OMLDatabaseEditor.Controls.MediaEditor.TitleStatus.UnsavedChanges)
                {
                    Title _currentTitle = (Title)_titleCollection.MoviesByItemId[editor.itemID];

                    editor.SaveToTitle(_currentTitle);
                    _titleCollection.Replace(_currentTitle);
                }
            }
            _titleCollection.saveTitleCollection();

        }

        private void tsbNewTitle_Click(object sender, EventArgs e)
        {
            Title t = new Title();
            t.Name = "New Movie";
            _titleCollection.Add(t);

            tvSourceList_AddItem("New Movie", t.InternalItemID.ToString(), "Movies");
        }

        
        private void tsbClose_Click(object sender, EventArgs e)
        {
            Controls.MediaEditor _currentEditor = (Controls.MediaEditor)tabsMediaPanel.SelectedTab.Controls[0];
            bool _bClose = true;

            if (_currentEditor.Status == global::OMLDatabaseEditor.Controls.MediaEditor.TitleStatus.UnsavedChanges)
            {
                DialogResult result = MessageBox.Show("Do you want to save the changes to the current movie?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (result == DialogResult.Cancel)
                {
                    _bClose = false;
                }
                else if (result == DialogResult.Yes)
                {
                    Title _currentTitle = (Title)_titleCollection.MoviesByItemId[_currentEditor.itemID];

                    _currentEditor.SaveToTitle(_currentTitle);
                    SaveTitleChangesToDB(_currentTitle);
                }
                else
                {
                }
            }

            if (_bClose)
            {
                TabPage _currentTab = tabsMediaPanel.SelectedTab;
                tabsMediaPanel.TabPages.Remove(_currentTab);
            }
        }

        private void MenuItemEditTab_Click(object sender, EventArgs e)
        {
            if (tvSourceList.SelectedNode.Tag != null)
            {
                if (tvSourceList.SelectedNode.Tag.ToString() == "Movies")
                {
                    int itemId = int.Parse(tvSourceList.SelectedNode.Name);
                    TabPage existingPage = GetPageForTitle(itemId);
                    if (existingPage != null)
                    {
                        tabsMediaPanel.SelectTab(existingPage);
                    }
                    else
                    {
                        EditNewTab(itemId);
                    }
                }
                else if (tvSourceList.SelectedNode.Tag.ToString() == "Importers")
                {
                    int pluginID = int.Parse(tvSourceList.SelectedNode.Name);

                    StartImport(pluginID);
                }
            }
        }

        private void StartImport(int pluginID)
        {
            bool showFolderSelection = false;

            OMLPlugin plugin = new OMLPlugin();
            plugin = plugins[pluginID];
            //plugin.FileFound += new OMLPlugin.FileFoundEventHandler(FileFound);
            //if (plugin.CanCopyImages) AskIfShouldCopyImages();
            plugin.CopyImages = true;// Program._copyImages;
            plugin.DoWork(plugin.GetWork());
            LoadTitlesIntoDatabase(plugin);
        }

        public static void LoadTitlesIntoDatabase(OMLPlugin plugin)
        {
            try
            {
                Utilities.DebugLine("[OMLImporter] Titles loaded, beginning Import process");
                //TitleCollection tc = new TitleCollection();
                List<Title> titles = plugin.GetTitles();
                Utilities.DebugLine("[OMLImporter] " + titles.Count + " titles found in input file");
                //Console.WriteLine("Found " + titles.Count + " titles");

                int numberOfTitlesAdded = 0;
                int numberOfTitlesSkipped = 0;
                bool YesToAll = true;// false;

                //if (Console.In.Peek() > 0)
                //    Console.In.ReadToEnd(); // flush out anything still in there

                foreach (Title t in titles)
                {
                    if (_titleCollection.ContainsDisks(t.Disks))
                    {
                        //Console.WriteLine("Title {0} skipped because already in the collection", t.Name);
                        numberOfTitlesSkipped++;
                        continue;
                    }


                    //Console.WriteLine("Adding: " + t.Name);
                    if (YesToAll == false)
                    {
                        /*Console.WriteLine("Would you like to add this title? (y/n/a)");
                        string response = Console.ReadLine();
                        switch (response.ToUpper())
                        {
                            case "Y":
                                mainTitleCollection.Add(t);
                                numberOfTitlesAdded++;
                                break;
                            case "N":
                                numberOfTitlesSkipped++;
                                break;
                            case "A":
                                YesToAll = true;
                                mainTitleCollection.Add(t);
                                numberOfTitlesAdded++;
                                break;
                            default:
                                break;
                        }*/
                    }
                    else
                    {
                        _titleCollection.Add(t);
                        numberOfTitlesAdded++;
                    }
                }

                plugin.GetTitles().Clear();

                /*if (numberOfTitlesAdded > 0) isDirty = true;
                Console.WriteLine();
                Console.WriteLine("Added " + numberOfTitlesAdded + " titles");
                Console.WriteLine("Skipped " + numberOfTitlesSkipped + " titles");*/
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception in LoadTitlesIntoDatabase: %1", e.Message);
                //Console.WriteLine("Exception in LoadTitlesIntoDatabase: %1", e.Message);
            }
            //tc.saveTitleCollection();
            //Console.WriteLine("Complete");
        }

        private void EditNewTab(int itemID)
        {
            Controls.MediaEditor Editor = new Controls.MediaEditor();

            Editor.AutoScroll = true;
            Editor.AutoSize = true;
            Editor.BackColor = System.Drawing.SystemColors.Control;
            Editor.Dock = System.Windows.Forms.DockStyle.Fill;
            Editor.Location = new System.Drawing.Point(0, 0);
            Editor.Name = "ME" + itemID.ToString();

            Title currentTitle = new Title();
            currentTitle = (Title)_titleCollection.MoviesByItemId[itemID];

            TabPage newpage = new TabPage(currentTitle.Name);
            newpage.Controls.Add(Editor);
            newpage.Size = new System.Drawing.Size(620, 772);
            newpage.Margin = new System.Windows.Forms.Padding(0);
            newpage.Padding = new System.Windows.Forms.Padding(0);
            newpage.Tag = Editor;

            tabsMediaPanel.TabPages.Add(newpage);
            
            Editor.LoadTitle(currentTitle);
            Editor.TitleChanged += new Controls.MediaEditor.TitleChangeEventHandler(this.TitleChanges);
            Editor.TitleNameChanged += new Controls.MediaEditor.TitleNameChangeEventHandler(this.TitleNameChanges);
            Editor.SavedTitle += new Controls.MediaEditor.SavedEventHandler(this.SavedTitle);

            tabsMediaPanel.SelectTab(newpage);
        }

        private void tvSourceList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Show menu only if the right mouse button is clicked.
            if (e.Button == MouseButtons.Right)
            {

                // Point where the mouse is clicked.
                Point p = new Point(e.X, e.Y);

                // Get the node that the user has clicked.
                TreeNode node = tvSourceList.GetNodeAt(p);
                if (node != null)
                {

                    // Select the node the user has clicked.
                    // The node appears selected until the menu is displayed on the screen.
                    m_OldSelectNode = tvSourceList.SelectedNode;
                    tvSourceList.SelectedNode = node;

                    // Find the appropriate ContextMenu depending on the selected node.
                    switch (Convert.ToString(node.Tag))
                    {
                        case "Movies":
                            MenuStripTitle.Show(tvSourceList, p);
                            break;
                    }

                    // Highlight the sel`ected node.
                    tvSourceList.SelectedNode = m_OldSelectNode;
                    m_OldSelectNode = null;
                }
            }
        }

        private void MarkChangedItem(Controls.MediaEditor editor, bool changed)
        {
            foreach (TabPage page in tabsMediaPanel.TabPages)
            {
                if (page.Tag == editor)
                {
                    if (changed)
                        page.Text = "*" + editor.TitleName;
                    else
                        page.Text = editor.TitleName;
                    break;
                }
            }
        }

        private TabPage GetPageForTitle(int itemId)
        {
            Controls.MediaEditor editor = null;
            foreach (TabPage page in tabsMediaPanel.TabPages)
            {
                editor = (Controls.MediaEditor)page.Tag;
                if (editor.itemID == itemId)
                {
                    return page;
                }
            }

            return null;
        }


        private void TitleChanges(object sender, EventArgs e)
        {
            Controls.MediaEditor _currentEditor = (Controls.MediaEditor)sender;
            MarkChangedItem(_currentEditor, true);
        }

        private void TitleNameChanges(object sender, EventArgs e)
        {
            Controls.MediaEditor _currentEditor = (Controls.MediaEditor)sender;
            MarkChangedItem(_currentEditor, true);
        }

        private void SavedTitle(object sender, EventArgs e)
        {
            Controls.MediaEditor _currentEditor = (Controls.MediaEditor)sender;
            MarkChangedItem(_currentEditor, false);
        }
        
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tvSourceList.SelectedNode.Tag != null)
            {
                if (tvSourceList.SelectedNode.Tag.ToString() == "Movies")
                {
                    Title titleToRemove = _titleCollection.MoviesByItemId[int.Parse(tvSourceList.SelectedNode.Name)];
                    DialogResult result = MessageBox.Show("Are you sure you want to delete " + titleToRemove.Name + "?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (result == DialogResult.Yes)
                    {
                        tvSourceList.Nodes.Remove(tvSourceList.SelectedNode);
                        _titleCollection.Remove(titleToRemove);
                        _titleCollection.saveTitleCollection();
                    }                   
                }
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAll();
        }

        private void tsbSaveAll_Click(object sender, EventArgs e)
        {
            SaveAll();
        }

        public static DialogResult GetFile(ref string file_to_import, OMLPlugin plugin)
        {
            OpenFileDialog ofDiag = new OpenFileDialog();
            ofDiag.InitialDirectory = "c:\\";
            ofDiag.Filter = "Xml files (*.xml)|*.xml|DVR-MS Files (*.dvr-ms)|*.dvr-ms|All files (*.*)|*.*";
            ofDiag.FilterIndex = 1;
            ofDiag.RestoreDirectory = true;
            ofDiag.AutoUpgradeEnabled = true;
            ofDiag.CheckFileExists = true;
            ofDiag.CheckPathExists = true;
            ofDiag.Multiselect = false;
            ofDiag.Title = "Select " + plugin.Name + " file to import";
            DialogResult dlgRslt = ofDiag.ShowDialog();
            if (dlgRslt == DialogResult.OK)
            {
                Utilities.DebugLine("[OMLImporter] Valid file found (" + ofDiag.FileName + ")");
                file_to_import = ofDiag.FileName;
            }
            return dlgRslt;
        }
    }
}



﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using OMLEngine;

namespace OMLDatabaseEditor
{
    public partial class OMLDatabaseEditor : Form
    {
        private TitleCollection _titleCollection;
        private TreeNode m_OldSelectNode;

        public OMLDatabaseEditor()
        {
            InitializeComponent();
            _titleCollection = new TitleCollection();
            _titleCollection.loadTitleCollection();
            SetupTitleList();

        }

        private void SetupTitleList()
        {
            foreach (Title t in _titleCollection)
            {
                tvSourceList_AddItem(t.Name, t.InternalItemID, "Movies");
            }
        }

        private void tvSourceList_AddItem(string text, int id, string type)
        {
            TreeNode nod = new TreeNode();
            nod.Name = id.ToString();
            nod.Text = text;
            nod.Tag = "Movies";
            tvSourceList.Nodes["OML Database"].Nodes[type].Nodes.Add(nod);
            tvSourceList.Nodes["OML Database"].ExpandAll();
            tvSourceList.Nodes["OML Database"].Nodes[type].ExpandAll();
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

            tvSourceList_AddItem("New Movie", t.InternalItemID, "Movies");
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
            }
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

        private void OMLDatabaseEditor_Load(object sender, EventArgs e)
        {

        }

    }
}



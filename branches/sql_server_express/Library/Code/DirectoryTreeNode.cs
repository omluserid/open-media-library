﻿using System;
using System.IO;
using System.Diagnostics;

namespace Library
{
    public class DirectoryTreeNode : TreeNode
    {
        public string FullPath
        {
            get { return _fullPath; }
            set
            {
                _fullPath = value;
                HasChildNodes = (Directory.Exists(_fullPath) &&
                                 Directory.GetDirectories(_fullPath).Length > 0);
            }
        }

        public DirectoryTreeNode(String title, String fullPath, TreeView treeView)
            : base(title, fullPath)
        {
            FullPath = fullPath;
            TreeView = treeView;
            TreeView.CheckedNodeChanged +=
                new EventHandler<TreeNodeEventArgs>(TreeView_OnCheckedNodeChanged);
        }

        public override void GetChildNodes()
        {
            if (!String.IsNullOrEmpty(FullPath))
            {
                ChildNodes.Clear();
                string[] directories = Directory.GetDirectories(FullPath);

                foreach (string directory in directories)
                {
                    try
                    {
                        DirectoryTreeNode node =
                            new DirectoryTreeNode(Path.GetFileName(directory), directory, TreeView);
                        node.Level = Level + 1;
                        node.TreeView = TreeView;
                        TreeView.CheckedNodeChanged +=
                            new EventHandler<TreeNodeEventArgs>(TreeView_OnCheckedNodeChanged);
                        node.HasChildNodes = (Directory.GetDirectories(node.FullPath).Length > 0);
                        ChildNodes.Add(node);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        OMLApplication.DebugLine("[DirectoryTreeNode] " + ex.Message);
                    }
                    catch (DriveNotFoundException ex)
                    {
                        OMLApplication.DebugLine("[DirectoryTreeNode] " + ex.Message);
                    }
                    catch (IOException ex)
                    {
                        OMLApplication.DebugLine("[DirectoryTreeNode] " + ex.Message);
                    }
                }
                HasChildNodes = (ChildNodes.Count > 0);
            }

            base.GetChildNodes();
        }
        public override string ToString()
        {
            return FullPath;
        }

        private void TreeView_OnCheckedNodeChanged(object sender, TreeNodeEventArgs e)
        {
            OMLApplication.DebugLine("[DirectoryTreeNode] CheckedNodeChanged: " + e.Node.Title);
            Checked.Value = (e.Node == this);
        }

        private string _fullPath = String.Empty;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

using OMLEngine;

namespace OMLDatabaseEditor
{
    public partial class Options : DevExpress.XtraEditors.XtraForm
    {
        public Options()
        {
            InitializeComponent();
        }

        public Boolean OptionsDirty = false;
        private Boolean MPAAdirty = false;
        private List<String> MPAAList;

        private void Options_Load(object sender, EventArgs e)
        {
            this.lbcSkins.DataSource = ((MainEditor)this.Owner).DXSkins;
            String skin = Properties.Settings.Default.gsAppSkin;
            int idx = this.lbcSkins.FindItem(skin);
            if (idx < 0)
            {
                skin = ((MainEditor)this.Owner).LookAndFeel.SkinName;
                idx = this.lbcSkins.FindItem(skin);
            }
            this.lbcSkins.SetSelected(idx, true);
            this.ceUseMPAAList.Checked = Properties.Settings.Default.gbUseMPAAList;
            MPAAList = new List<string>();
            MPAAList.AddRange(Properties.Settings.Default.gsMPAARatings.Split('|'));
            this.lbcMPAA.DataSource = MPAAList;

            this.ceUseGenreList.Checked = Properties.Settings.Default.gbUseGenreList;
            if (Properties.Settings.Default.gsValidGenres == null || Properties.Settings.Default.gsValidGenres.Count == 0)
            {
                if (MessageBox.Show("No allowable genres have been defined. Would you like to load them from your current movie collection?", "No Genres", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Properties.Settings.Default.gsValidGenres = new System.Collections.Specialized.StringCollection();
                    Properties.Settings.Default.gsValidGenres.AddRange(MainEditor._titleCollection.GetAllGenres().ToArray());
                }
            }
            lbGenres.DataSource = Properties.Settings.Default.gsValidGenres;
        }

        private void SimpleButtonClick(object sender, EventArgs e)
        {
            if (sender == this.sbOK)
            {
                Boolean bDirty = false;
                String skin = (String)this.lbcSkins.SelectedValue;
                if (skin != Properties.Settings.Default.gsAppSkin)
                {
                    bDirty = true;
                    Properties.Settings.Default.gsAppSkin = skin;
                }
                if (Properties.Settings.Default.gbUseMPAAList != this.ceUseMPAAList.Checked)
                {
                    bDirty = true;
                    Properties.Settings.Default.gbUseMPAAList = this.ceUseMPAAList.Checked;
                }
                if (MPAAdirty)
                {
                    bDirty = true;
                    String MPAAs = String.Join("|", MPAAList.ToArray());
                    Properties.Settings.Default.gsMPAARatings = MPAAs;
                }
                if (Properties.Settings.Default.gbUseGenreList != this.ceUseGenreList.Checked)
                {
                    bDirty = true;
                    Properties.Settings.Default.gbUseGenreList = this.ceUseGenreList.Checked;
                }
                if (bDirty)
                {
                    OptionsDirty = bDirty;
                    Properties.Settings.Default.Save();
                }
            }
            else if (sender == this.sbCancel)
            {
                String skin = Properties.Settings.Default.gsAppSkin;
                ((MainEditor)this.Owner).defaultLookAndFeel1.LookAndFeel.SkinName = skin;
            }
            this.Close();
        }

        private void lbcSkins_SelectedValueChanged(object sender, EventArgs e)
        {
            String skin = (String)this.lbcSkins.SelectedValue;
            ((MainEditor)this.Owner).defaultLookAndFeel1.LookAndFeel.SkinName = skin;
        }

        private void beMPAA_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if ((sender == beMPAA) && (e.Button.Kind == ButtonPredefines.Plus))
            {
                if (!String.IsNullOrEmpty((String)beMPAA.Text))
                {
                    MPAAList.Add((String)beMPAA.Text);
                    MPAAdirty = true;
                    lbcMPAA.Refresh();
                }
            }
        }

        private void lbcMPAA_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (lbcMPAA.SelectedItem != null)
                {
                    MPAAList.Remove((String)lbcMPAA.SelectedItem);
                    MPAAdirty = true;
                    lbcMPAA.Refresh();
                }
            }
        }

        private void btnGenre_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (Properties.Settings.Default.gsValidGenres.Contains(btnGenre.Text)) return;

            Properties.Settings.Default.gsValidGenres.Add(btnGenre.Text);

            btnGenre.Text = "";
        }

        private void lbGenres_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lbGenres.SelectedItems.Count > 0)
            {
                foreach (object item in lbGenres.SelectedItems)
                {
                    string genre = item as string;
                    List<Title> titles = MainEditor._titleCollection.FindByGenre(genre);
                    if (titles.Count > 0)
                    {
                        StringBuilder message = new StringBuilder(titles.Count + " movie(s) in your collection are associated with the " + genre + " genre. Would you like to remove the association?\r\n\r\n");
                        foreach (Title title in titles)
                            message.Append(title.Name + "\r\n");
                        if (MessageBox.Show(message.ToString(), "Remove Genre", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            foreach (Title title in MainEditor._titleCollection.FindByGenre(genre))
                            {
                                title.Genres.Remove(genre);
                            }
                        }
                    }
                    Properties.Settings.Default.gsValidGenres.Remove(genre);
                }
            }
        }
    }
}
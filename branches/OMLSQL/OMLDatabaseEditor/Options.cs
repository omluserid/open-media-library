using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System.Linq;

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
        private Boolean GenreDirty = false;
        private List<String> MPAAList;
        private List<String> GenreList;

        private void Options_Load(object sender, EventArgs e)
        {
            int genreCount = 0;
            if (Properties.Settings.Default.gsValidGenres != null)
                genreCount = Properties.Settings.Default.gsValidGenres.Count;

            String[] arrGenre = new String[genreCount];
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
            MPAAList = new List<String>();
            MPAAList.AddRange(Properties.Settings.Default.gsMPAARatings.Split('|'));
            MPAAList.Sort();
            this.lbcMPAA.DataSource = MPAAList;

            this.ceUseGenreList.Checked = Properties.Settings.Default.gbUseGenreList;
            if (Properties.Settings.Default.gsValidGenres == null || Properties.Settings.Default.gsValidGenres.Count == 0)
            {
                if (XtraMessageBox.Show("No allowable genres have been defined. Would you like to load them from your current movie collection?", "No Genres", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Properties.Settings.Default.gsValidGenres = new StringCollection();

                    IEnumerable<FilteredCollection> genres = TitleCollectionManager.GetAllGenres(null);
                    string[] genreNames = new string[genres.Count()];

                    int index = 0;
                    foreach (FilteredCollection genre in genres)
                        genreNames[index++] = genre.Name;

                    Properties.Settings.Default.gsValidGenres.AddRange(genreNames);
                }
            }
            GenreList = new List<String>();
            // I disabled this line because gsValidGenres is not just empty, its undef
            //Properties.Settings.Default.gsValidGenres.CopyTo(arrGenre, 0);
            GenreList.AddRange(arrGenre);
            GenreList.Sort();
            lbGenres.DataSource = GenreList;
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
                if (GenreDirty)
                {
                    bDirty = true;
                    Properties.Settings.Default.gsValidGenres.Clear();
                    Properties.Settings.Default.gsValidGenres.AddRange(GenreList.ToArray());
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
                    beMPAA.Text = String.Empty;
                }
            }
        }

        private void lbcMPAA_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lbcMPAA.SelectedItems.Count > 0)
            {
                foreach (object item in lbcMPAA.SelectedItems)
                {
                    String MPAA = item as String;
                    MPAAList.Remove(MPAA);
                    MPAAdirty = true;
                }
                lbcMPAA.Refresh();
            }
        }

        private void btnGenre_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            if (GenreList.Contains(btnGenre.Text)) return;

            GenreList.Add(btnGenre.Text);
            GenreDirty = true;
            lbGenres.Refresh();

            btnGenre.Text = String.Empty;
        }

        private void lbGenres_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && lbGenres.SelectedItems.Count > 0)
            {
                foreach (object item in lbGenres.SelectedItems)
                {
                    string genre = item as string;
                    IEnumerable<Title> titles = TitleCollectionManager.GetFilteredTitles(TitleFilterType.Genre, genre);

                    int titleCount = titles.Count();

                    if (titleCount > 0)
                    {
                        StringBuilder message = new StringBuilder(titleCount + " movie(s) in your collection are associated with the " + genre + " genre. Would you like to remove the association?\r\n\r\n");
                        foreach (Title title in titles)
                            message.Append(title.Name + "\r\n");

                        if (XtraMessageBox.Show(message.ToString(), "Remove Genre", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            foreach (Title title in TitleCollectionManager.GetFilteredTitles(TitleFilterType.Genre, genre))
                            {
                                title.Genres.Remove(genre);
                            }
                        }
                    }
                    GenreList.Remove(genre);
                    GenreDirty = true;
                    //Properties.Settings.Default.gsValidGenres.Remove(genre);
                }
                lbGenres.Refresh();
            }
        }
    }
}
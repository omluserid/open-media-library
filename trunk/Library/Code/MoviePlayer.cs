﻿using System;
using System.Collections.Generic;
using System.Text;
using OMLEngine;
using Microsoft.MediaCenter.Hosting;
using Microsoft.MediaCenter;
using Microsoft.MediaCenter.UI;
using System.IO;
using System.Diagnostics;

namespace Library
{
    /// <summary>
    /// an interface required for movie players
    /// </summary>
    public interface IPlayMovie
    {
        /// <summary>
        /// Plays a movie.
        /// </summary>
        /// <returns></returns>
        bool PlayMovie();
    }

    /// <summary>
    /// A factory class to create the movie player based on file type
    /// </summary>
    public class MoviePlayerFactory
    {
        /// <summary>
        /// Creates the movie player based on the the video formatin in the Title.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <returns></returns>
        static public IPlayMovie CreateMoviePlayer(MovieItem title)
        {
            // for now play just online titles. add offline capabilities soon
            if (File.Exists(title.FileLocation) || Directory.Exists(title.FileLocation))
            {
                if (title.TitleObject.VideoFormat == VideoFormat.DVD)
                {
                    Trace.WriteLine("DVDMoviePlayer created");
                    return new DVDPlayer(title);
                }
                else if (title.TitleObject.NeedToMountBeforePlaying())
                {
                    Trace.WriteLine("MountImageMoviePlayer created");
                    return new MountImagePlayer(title);
                }
                else
                {
                    Trace.WriteLine("VideoPlayer created");
                    return new VideoPlayer(title);
                }
            }
            else
            {
                Trace.WriteLine("UnavailableMoviePlayer created");
                return new UnavailableMoviePlayer(title);
            }
        }
    }

    /// <summary>
    /// DVDPlayer class for playing a DVD
    /// </summary>
    public class DVDPlayer : IPlayMovie
    {
        public DVDPlayer(MovieItem title)
        {
            _title = title;
        }

        public bool PlayMovie()
        {
            string media = "DVD://" + _title.FileLocation;
            media.Replace('\\', '/');
            if (AddInHost.Current.MediaCenterEnvironment.PlayMedia(MediaType.Dvd, media, false))
            {
                if (AddInHost.Current.MediaCenterEnvironment.MediaExperience != null)
                {
                    AddInHost.Current.MediaCenterEnvironment.MediaExperience.GoToFullScreen();
                }
                return true;
            }
            else
            {
                return false;
            }
            
        }

        MovieItem _title;
    }

    /// <summary>
    /// VideoPlayer class for playing standard videos (AVIs, etc)
    /// </summary>
    public class VideoPlayer : IPlayMovie
    {
        public VideoPlayer(MovieItem title)
        {
            _title = title;
        }

        public bool PlayMovie()
        {
            if (AddInHost.Current.MediaCenterEnvironment.PlayMedia(MediaType.Video, _title.FileLocation, false))
            {
                if (AddInHost.Current.MediaCenterEnvironment.MediaExperience != null)
                {
                    AddInHost.Current.MediaCenterEnvironment.MediaExperience.GoToFullScreen();
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        MovieItem _title;
    }

    public class UnavailableMoviePlayer : IPlayMovie
    {
        public UnavailableMoviePlayer(MovieItem title)
        {
            _title = title;
        }

        public bool PlayMovie()
        {
            // show a popup or a page explaining the error
            //return false;
            AddInHost.Current.MediaCenterEnvironment.Dialog(
                "Could not find file [" + _title.FileLocation + "] or don't know how to play this file type",
                "Error", DialogButtons.Ok, 0, true);
            return false;
        }

        MovieItem _title;
    }

    public class MountImagePlayer : IPlayMovie
    {
        public MountImagePlayer(MovieItem title)
        {
            _title = title;
        }

        public bool PlayMovie()
        {
            if (MountTitle(_title))
            {
                string mount_location = GetMountLocation();
                if (mount_location != null && mount_location.Length > 0)
                {
                    _title.FileLocation = mount_location;
                    IPlayMovie player = new DVDPlayer(_title);
                    return player.PlayMovie();
                }
            }
            return false;
        }

        private string GetMountLocation()
        {
            return string.Empty;
        }

        private bool MountTitle(MovieItem title)
        {
            if (DaemonToolsFound())
            {
            }

            return false;
        }

        private bool DaemonToolsFound()
        {
            return false;
        }

        MovieItem _title;
    }

}

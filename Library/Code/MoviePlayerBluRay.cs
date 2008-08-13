﻿using System;
using OMLEngine;
using Microsoft.MediaCenter.Hosting;
using Microsoft.MediaCenter;
using Microsoft.MediaCenter.UI;
using System.IO;

namespace Library
{
    public class BluRayPlayer : IPlayMovie
    {
        MovieItem _title;

        public BluRayPlayer(MovieItem title)
        {
            _title = title;
        }

        public bool PlayMovie()
        {
            string media = "BLURAY://" + _title.SelectedDisk.Path;
            media.Replace('\\', '/');
            if (AddInHost.Current.MediaCenterEnvironment.PlayMedia(MediaType.Video, media, false))
            {
                if (AddInHost.Current.MediaCenterEnvironment.MediaExperience != null)
                {
                    Utilities.DebugLine("BluRayPlayer.PlayMovie: movie {0} Playing", _title.Name);
                    OMLApplication.Current.NowPlayingMovieName = _title.Name;
                    OMLApplication.Current.NowPlayingStatus = PlayState.Playing;
                    AddInHost.Current.MediaCenterEnvironment.MediaExperience.Transport.PropertyChanged += MoviePlayerFactory.Transport_PropertyChanged;
                    AddInHost.Current.MediaCenterEnvironment.MediaExperience.GoToFullScreen();
                }
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}

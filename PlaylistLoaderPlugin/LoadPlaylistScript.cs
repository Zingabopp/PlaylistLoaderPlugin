using System.IO;
using System;
using Newtonsoft.Json.Linq;
using SongCore;
using System.Collections.Generic;
using System.Linq;
using BeatSaberPlaylistsLib;
using BeatSaberPlaylistsLib.Types;

namespace PlaylistLoaderLite
{
    public class LoadPlaylistScript
    {
        public static IPlaylist[] load()
        {
            BeatSaberPlaylistsLib.Utilities.Logger = (s, ex) =>
            {
                if (s != null)
                    Plugin.Log.Warn(s);
                if (ex != null)
                    Plugin.Log.Error(ex);
            };
            PlaylistManager manager = PlaylistManager.DefaultManager;
            //string playlistDirectory = 
            //    Path.Combine(Environment.CurrentDirectory, "Playlists") + "\\";
            string[] playlistPaths 
                = Directory.EnumerateFiles(manager.PlaylistPath, "*.*").ToArray();
            Plugin.Log.Info($"Found {playlistPaths.Length} files in '{Path.GetFullPath(manager.PlaylistPath)}'.");
            List<IPlaylist> playlists = new List<IPlaylist>();
            for(int i = 0; i < playlistPaths.Length; i++)
            {
                string fileName = Path.GetFileNameWithoutExtension(playlistPaths[i]);
                Plugin.Log.Debug($"Loading playlist {Path.GetFileName(playlistPaths[i])}");
                try
                {
                    BeatSaberPlaylistsLib.Types.IPlaylist playlist = manager.GetPlaylist(fileName);
                    IPlaylistSong[] unmatched = playlist.Where(s => s.PreviewBeatmapLevel == null).ToArray();
                    foreach (IPlaylistSong song in unmatched)
                    {
                        Plugin.Log.Warn($"Unmatched song: {song.Name}|{song.LevelId}|{song.Hash}|{song.Key}");
                        playlist.Remove(song);
                    }
                    if (playlist != null)
                        playlists.Add(playlist);
                    else
                        Plugin.Log.Warn($"Playlist '{fileName}' was null.");
                }catch(Exception ex)
                {
                    Plugin.Log.Error($"Error reading playlist '{fileName}': {ex.Message}");
                    Plugin.Log.Debug(ex);
                }
            }
            //for (int i = 0; i < playlistPaths.Length; i++)
            //{
            //    try
            //    {
            //        JObject playlistJSON = JObject.Parse(File.ReadAllText(playlistPaths[i]));
            //        if (playlistJSON["songs"] != null)
            //        {
            //            JArray songs = (JArray)playlistJSON["songs"];
            //            List<IPreviewBeatmapLevel> beatmapLevels = new List<IPreviewBeatmapLevel>();
            //            for (int j = 0; j < songs.Count; j++)
            //            {
            //                IPreviewBeatmapLevel beatmapLevel = null;
            //                String hash = (string)songs[j]["hash"];
            //                if (!string.IsNullOrEmpty(hash))
            //                    beatmapLevel = MatchSong(hash);
            //                if (beatmapLevel != null)
            //                    beatmapLevels.Add(beatmapLevel);
            //                else
            //                {
            //                    String levelID = (string)(songs[j]["levelId"] ?? songs[j]["levelid"] ?? songs[j]["levelID"]);
            //                    if (!string.IsNullOrEmpty(levelID))
            //                    {
            //                        beatmapLevel = MatchSongById(levelID);
            //                        if (beatmapLevel != null)
            //                            beatmapLevels.Add(beatmapLevel);
            //                        else
            //                            Plugin.Log.Warn($"Song not downloaded, : {(string.IsNullOrEmpty(levelID) ? " unknown levelID!" : ("levelID " + levelID + "!"))}");
            //                    }
            //                    else
            //                        Plugin.Log.Warn($"Song not downloaded, : {(string.IsNullOrEmpty(hash) ? " unknown hash!" : ("hash " + hash + "!"))}");
            //                }
            //            }
            //            CustomBeatmapLevelCollectionSO customBeatmapLevelCollection = CustomBeatmapLevelCollectionSO.CreateInstance(beatmapLevels.ToArray());
            //            String playlistTitle = "Untitled Playlist";
            //            String playlistImage = CustomPlaylistSO.DEFAULT_IMAGE;
            //            if ((string)playlistJSON["playlistTitle"] != null)
            //                playlistTitle = (string)playlistJSON["playlistTitle"];
            //            if ((string)playlistJSON["image"] != null)
            //                playlistImage = (string)playlistJSON["image"];
            //            playlists.Add(CustomPlaylistSO.CreateInstance(playlistTitle, playlistImage, customBeatmapLevelCollection));
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        Plugin.Log.Critical($"Error loading Playlist File: " + playlistPaths[i] + " Exception: " + e.Message);
            //    }
            //}
            return playlists.ToArray();
        }

        private static IPreviewBeatmapLevel MatchSongById(string levelId)
        {
            if (!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
            {
                Plugin.Log.Info("Songs not loaded. Not Matching songs for playlist.");
                return null;
            }
            IPreviewBeatmapLevel x = null;
            try
            {
                if (!string.IsNullOrEmpty(levelId))
                {
                    if (!levelId.StartsWith(CustomLevelLoader.kCustomLevelPrefixId))
                        return Loader.GetOfficialLevelById(levelId).PreviewBeatmapLevel;
                    else
                    {
                        x = MatchSong(Collections.hashForLevelID(levelId));
                    }
                }
            }
            catch (Exception)
            {
                Plugin.Log.Warn($"Unable to match song with {(string.IsNullOrEmpty(levelId) ? " unknown levelId!" : ("levelId " + levelId + " !"))}");
            }
            return x;
        }

        private static IPreviewBeatmapLevel MatchSong(String hash)
        {
            if (!SongCore.Loader.AreSongsLoaded || SongCore.Loader.AreSongsLoading)
            {
                Plugin.Log.Info("Songs not loaded. Not Matching songs for playlist.");
                return null;
            }
            IPreviewBeatmapLevel x = null;
            try
            {
                if (!string.IsNullOrEmpty(hash))
                    x = SongCore.Loader.GetLevelByHash(hash);
            }
            catch (Exception)
            {
                Plugin.Log.Warn($"Unable to match song with {(string.IsNullOrEmpty(hash) ? " unknown hash!" : ("hash " + hash + " !"))}");
            }
            return x;
        }
    }
}

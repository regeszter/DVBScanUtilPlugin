using System;
using System.Collections.Generic;
using System.Threading;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvLibrary.Channels;
using System.Xml.Serialization;
using System.Xml;
using MediaPortal.Playlists;

namespace DVBScanUtilPlugin
{
  public class DVBIPScanUtilPlugin
  {
    static public TvService.TVController Controller;
    private int _cardNumber;
    private String _defaultTVGroup;
    //private IEnumerator<PlayListItem> _dvbIPChannels = playlist.GetEnumerator();
    PlayList playlist = new PlayList();


    #region Constructor

    /// <summary>
    /// Creates a new EPGUtilPlugin plugin
    /// </summary>
    public DVBIPScanUtilPlugin() { }

    #endregion

    private void DoScan()
    {
        int tvChannelsNew = 0;
        int radioChannelsNew = 0;
        int tvChannelsUpdated = 0;
        int radioChannelsUpdated = 0;

        IUser user = new User();
        user.CardId = _cardNumber;
      try
      {
        // First lock the card, because so that other parts of a hybrid card can't be used at the same time
        RemoteControl.Instance.EpgGrabberEnabled = false;

        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        int index = -1;
        IEnumerator<PlayListItem> enumerator = playlist.GetEnumerator();

        while (enumerator.MoveNext())
        {
          string url = enumerator.Current.FileName.Substring(enumerator.Current.FileName.LastIndexOf('\\') + 1);
          string name = enumerator.Current.Description;

          DVBIPChannel tuneChannel = new DVBIPChannel();
          tuneChannel.Url = url;
          tuneChannel.Name = name;
          string line = String.Format("{0}- {1} - {2}", 1 + index, tuneChannel.Name, tuneChannel.Url);
          Log.Debug(line);

          RemoteControl.Instance.Tune(ref user, tuneChannel, -1);
          IChannel[] channels;
          channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);

          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}- {1} - {2} :No Signal", 1 + index, tuneChannel.Url, tuneChannel.Name);
              Log.Debug(line);
              continue;
            }
            else
            {
              line = String.Format("{0}- {1} - {2} :Nothing found", 1 + index, tuneChannel.Url, tuneChannel.Name);
              Log.Debug(line);
              continue;
            }
          }

          int newChannels = 0;
          int updatedChannels = 0;

          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBIPChannel channel = (DVBIPChannel)channels[i];
            if (channels.Length > 1)
            {
              if (channel.Name.IndexOf("Unknown") == 0)
              {
                channel.Name = name + (i + 1);
              }
            }
            else
            {
              channel.Name = name;
            }
            bool exists;
            TuningDetail currentDetail;
            //Check if we already have this tuningdetail. According to DVB-IP specifications there are two ways to identify DVB-IP
            //services: one ONID + SID based, the other domain/URL based. At this time we don't fully and properly implement the DVB-IP
            //specifications, so the safest method for service identification is the URL. The user has the option to enable the use of
            //ONID + SID identification and channel move detection...
            if (true)
            {
              currentDetail = layer.GetTuningDetail(channel.NetworkId, channel.ServiceId,
                                                                 TvBusinessLayer.GetChannelType(channel));
            }
            else
            {
              currentDetail = layer.GetTuningDetail(channel.Url, TvBusinessLayer.GetChannelType(channel));
            }

            if (currentDetail == null)
            {
              //add new channel
              exists = false;
              dbChannel = layer.AddNewChannel(channel.Name, channel.LogicalChannelNumber);
              dbChannel.SortOrder = 10000;
              if (channel.LogicalChannelNumber >= 1)
              {
                dbChannel.SortOrder = channel.LogicalChannelNumber;
              }
              dbChannel.IsTv = channel.IsTv;
              dbChannel.IsRadio = channel.IsRadio;
              dbChannel.Persist();
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.ReferencedChannel();
            }

            layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);

            if (_defaultTVGroup != "")
            {
              layer.AddChannelToGroup(dbChannel, _defaultTVGroup);
            }
            if (currentDetail == null)
            {
              layer.AddTuningDetails(dbChannel, channel);
            }
            else
            {
              //update tuning details...
              TuningDetail td = layer.UpdateTuningDetails(dbChannel, channel, currentDetail);
              td.Persist();
            }

            if (channel.IsTv)
            {
              if (exists)
              {
                tvChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                tvChannelsNew++;
                newChannels++;
              }
            }
            if (channel.IsRadio)
            {
              if (exists)
              {
                radioChannelsUpdated++;
                updatedChannels++;
              }
              else
              {
                radioChannelsNew++;
                newChannels++;
              }
            }
            layer.MapChannelToCard(card, dbChannel, false);
            line = String.Format("{0}- {1} :New:{2} Updated:{3}", 1 + index, tuneChannel.Name, newChannels,
                                 updatedChannels);
            Log.Debug(line);
          }
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.StopCard(user);
      }
    }

    public void DoWork()
    {
      var layer = new TvBusinessLayer();

      _defaultTVGroup = layer.GetSetting("DVBIPScanUtilPluginSetupDefaultGroup", "New").Value;

      String EPGUtilPluginTuningXML = layer.GetSetting("DVBIPScanUtilPluginSetupTuningXML", "").Value;

      if (EPGUtilPluginTuningXML == "")
      {
        Log.Error("DVBIPScanUtilPlugin: missing tuning config");
        return;
      }

      IPlayListIO playlistIO =
            PlayListFactory.CreateIO(String.Format(@"{0}\TuningParameters\dvbip\{1}.m3u", PathManager.GetDataPath, EPGUtilPluginTuningXML));
      playlistIO.Load(playlist,
                      String.Format(@"{0}\TuningParameters\dvbip\{1}.m3u", PathManager.GetDataPath, EPGUtilPluginTuningXML));

      if (playlist.Count == 0) return;

      IList<Card> dbsCards = Card.ListAll();
      foreach (Card card in dbsCards)
      {
        if (CardType.DvbIP == RemoteControl.Instance.Type(card.IdCard))
        {
          Log.Debug("DoScan card: " + card.IdCard);

          _cardNumber = card.IdCard;

          DoScan();
        }
      }
    }

    public object LoadList(string fileName, Type ListType)
    {
      try
      {
        XmlReader parFileXML = XmlReader.Create(fileName);
        XmlSerializer xmlSerializer = new XmlSerializer(ListType);
        object result = xmlSerializer.Deserialize(parFileXML);
        parFileXML.Close();
        return result;
      }
      catch (Exception ex)
      {
        Log.Error("Error loading tuningdetails: {0}", ex.ToString());
        return null;
      }
    }
  }
}


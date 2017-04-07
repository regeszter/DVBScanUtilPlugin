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


namespace DVBScanUtilPlugin
{
  public class DVBCScanUtilPlugin
  {
    static public TvService.TVController Controller;
    private int _cardNumber;
    private String _defaultTVGroup;
    private List<DVBCTuning> _DVBCChannels = new List<DVBCTuning>();


    #region Constructor

    /// <summary>
    /// Creates a new EPGUtilPlugin plugin
    /// </summary>
    public DVBCScanUtilPlugin() { }

    #endregion

    private void DoScan()
    {
      suminfo tv = new suminfo();
      suminfo radio = new suminfo();
      IUser user = new User();
      user.CardId = _cardNumber;

      try
      {
        if (_DVBCChannels.Count == 0)
          return;

        RemoteControl.Instance.EpgGrabberEnabled = false;


        TvBusinessLayer layer = new TvBusinessLayer();
        Card card = layer.GetCardByDevicePath(RemoteControl.Instance.CardDevice(_cardNumber));

        for (int index = 0; index < _DVBCChannels.Count; ++index)
        {
          DVBCTuning curTuning = _DVBCChannels[index];
          DVBCChannel tuneChannel = new DVBCChannel(curTuning);
          string line = String.Format("{0}tp- {1}", 1 + index, tuneChannel.TuningInfo.ToString());
          Log.Debug(line);

          if (index == 0)
          {
            RemoteControl.Instance.Scan(ref user, tuneChannel, -1);
          }

          IChannel[] channels = RemoteControl.Instance.Scan(_cardNumber, tuneChannel);

          if (channels == null || channels.Length == 0)
          {
            if (RemoteControl.Instance.TunerLocked(_cardNumber) == false)
            {
              line = String.Format("{0}tp- {1} {2} {3}:No signal", 1 + index, tuneChannel.Frequency,
                     tuneChannel.ModulationType, tuneChannel.SymbolRate);

              Log.Error(line);
              continue;
            }
            line = String.Format("{0}tp- {1} {2} {3}:Nothing found", 1 + index, tuneChannel.Frequency,
                                 tuneChannel.ModulationType, tuneChannel.SymbolRate);

            Log.Error(line);
            continue;
          }

          radio.newChannel = 0;
          radio.updChannel = 0;
          tv.newChannel = 0;
          tv.updChannel = 0;

          for (int i = 0; i < channels.Length; ++i)
          {
            Channel dbChannel;
            DVBCChannel channel = (DVBCChannel)channels[i];
            bool exists;
            TuningDetail currentDetail;
            //Check if we already have this tuningdetail. The user has the option to enable channel move detection...
            if (true)
            {
              //According to the DVB specs ONID + SID is unique, therefore we do not need to use the TSID to identify a service.
              //The DVB spec recommends that the SID should not change if a service moves. This theoretically allows us to
              //track channel movements.
              currentDetail = layer.GetTuningDetail(channel.NetworkId, channel.ServiceId,
                                                                 TvBusinessLayer.GetChannelType(channel));
            }
            else
            {
              //There are certain providers that do not maintain unique ONID + SID combinations.
              //In those cases, ONID + TSID + SID is generally unique. The consequence of using the TSID to identify
              //a service is that channel movement tracking won't work (each transponder/mux should have its own TSID).
              currentDetail = layer.GetTuningDetail(channel.NetworkId, channel.TransportId, channel.ServiceId,
                                                                 TvBusinessLayer.GetChannelType(channel));
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
              dbChannel.GrabEpg = true;
              dbChannel.Persist();

              if (dbChannel.IsTv)
              {
                layer.AddChannelToGroup(dbChannel, TvConstants.TvGroupNames.AllChannels);

                if (_defaultTVGroup != "")
                {
                  layer.AddChannelToGroup(dbChannel, _defaultTVGroup);
                }

              }
              if (dbChannel.IsRadio)
              {
                layer.AddChannelToRadioGroup(dbChannel, TvConstants.RadioGroupNames.AllChannels);

                if (_defaultTVGroup != "")
                {
                  layer.AddChannelToRadioGroup(dbChannel, _defaultTVGroup);
                }
              }
            }
            else
            {
              exists = true;
              dbChannel = currentDetail.ReferencedChannel();
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
                tv.updChannel++;
              }
              else
              {
                tv.newChannel++;
                tv.newChannels.Add(channel);
              }
            }
            if (channel.IsRadio)
            {
              if (exists)
              {
                radio.updChannel++;
              }
              else
              {
                radio.newChannel++;
                radio.newChannels.Add(channel);
              }
            }
            layer.MapChannelToCard(card, dbChannel, false);
            line = String.Format("{0}tp- {1} {2} {3}:New TV/Radio:{4}/{5} Updated TV/Radio:{6}/{7}", 1 + index,
                                 tuneChannel.Frequency, tuneChannel.ModulationType, tuneChannel.SymbolRate,
                                 tv.newChannel, radio.newChannel, tv.updChannel, radio.updChannel);
            Log.Debug(line);
          }
          tv.updChannelSum += tv.updChannel;
          radio.updChannelSum += radio.updChannel;
        }
      }
      catch (Exception ex)
      {
        Log.Write(ex);
      }
      finally
      {
        RemoteControl.Instance.StopCard(user);
        RemoteControl.Instance.EpgGrabberEnabled = true;
      }

      if (radio.newChannels.Count == 0)
      {
        Log.Debug("No new radio channels");
      }
      else
      {
        foreach (IChannel newChannel in radio.newChannels)
        {
          String line = String.Format("Radio  -> new channel: {0}", newChannel.Name);
          Log.Debug(line);
        }
      }

      if (tv.newChannels.Count == 0)
      {
        Log.Debug("No new TV channels");
      }
      else
      {
        foreach (IChannel newChannel in tv.newChannels)
        {
          String line = String.Format("TV  -> new channel: {0}", newChannel.Name);
          Log.Debug(line);
        }
      }
    }

    public void DoWork()
    {
      Thread.Sleep(5 * 1000);

      var layer = new TvBusinessLayer();

      _defaultTVGroup = layer.GetSetting("DVBTScanUtilPluginSetupDefaultGroup", "New").Value;

      String EPGUtilPluginTuningXML = layer.GetSetting("DVBCScanUtilPluginSetupTuningXML", "").Value;

      if (EPGUtilPluginTuningXML == "")
      {
        Log.Error("DVBCScanUtilPlugin: missing tuning config");
        return;
      }

      EPGUtilPluginTuningXML = String.Format(@"{0}\TuningParameters\DVBC\{1}", PathManager.GetDataPath, EPGUtilPluginTuningXML);

      _DVBCChannels = (List<DVBCTuning>)LoadList(EPGUtilPluginTuningXML, typeof(List<DVBCTuning>));

      if (_DVBCChannels == null)
      {
        _DVBCChannels = new List<DVBCTuning>();
      }

      IList<Card> dbsCards = Card.ListAll();
      foreach (Card card in dbsCards)
      {
        if (CardType.DvbC == RemoteControl.Instance.Type(card.IdCard))
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


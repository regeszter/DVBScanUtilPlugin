using System;
using System.Collections.Generic;
using System.Threading;
using TvControl;
using TvDatabase;
using TvLibrary.Interfaces;
using TvLibrary.Log;
using TvEngine;
using System.Xml.Serialization;
using System.Xml;


namespace DVBScanUtilPlugin
{
  public class DVBScanUtilPlugin : ITvServerPlugin
  {
    static public TvService.TVController Controller;

    #region Constructor

    /// <summary>
    /// Creates a new EPGUtilPlugin plugin
    /// </summary>
    public void DVBTScanUtilPlugin() { }

    #endregion
    /// Starts the plugin
    /// </summary>
    public void Start(IController controller)
    {
      Controller = controller as TvService.TVController;

      Log.Debug("DVBScanUtilPlugin Started");

      Thread DVBScanUtilPlugin = new Thread(DoWork);
      DVBScanUtilPlugin.IsBackground = true;
      DVBScanUtilPlugin.Name = "DVBScanUtilPlugin";
      DVBScanUtilPlugin.Start();
    }

    /// <summary>
    /// Stops the plugin
    /// </summary>
    public void Stop()
    {
    }

    public string Author
    {
      get { return "regeszter"; }
    }

    /// <summary>
    /// Should this plugin run only on a master tvserver?
    /// </summary>
    public bool MasterOnly
    {
      get { return true; }
    }

    /// <summary>
    /// Name of this plugin
    /// </summary>
    public string Name
    {
      get { return "DVBScanUtilPlugin"; }
    }

    /// <summary>
    /// Plugin version
    /// </summary>
    public string Version
    {
      get { return "1.0.0.0"; }
    }

    public SetupTv.SectionSettings Setup
    {
      get { return new SetupTv.Sections.DVBScanUtilPluginSetup(); }
    }
    
    private void DoWork()
    {
      RemoteControl.Instance.EpgGrabberEnabled = false;

      Thread.Sleep(5 * 1000);

      try
      {
        DVBTScanUtilPlugin DVBTScanUtilPlugin = new DVBTScanUtilPlugin();
        DVBTScanUtilPlugin.DoWork();

        DVBCScanUtilPlugin DVBCScanUtilPlugin = new DVBCScanUtilPlugin();
        DVBCScanUtilPlugin.DoWork();
      }
      catch (Exception e)
      {
        Log.Error(e.Message);
      }

      RemoteControl.Instance.EpgGrabberEnabled = true;

      Log.Debug("DVBScanUtilPlugin finished");
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
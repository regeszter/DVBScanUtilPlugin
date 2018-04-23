using System;
using System.Windows.Forms;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class DVBScanUtilPluginSetup : SetupTv.SectionSettings
  {
    private TextBox textBoxDefaultGroup;
    private Label label2;
    private Label label3;
    private Label label1;
    private TextBox textBoxDVBCTuningXML;
    private Label label4;
    private TextBox textBoxDVBIPTuningXML;
    private TextBox textBoxDVBTTuningXML;
    #region constructors

    public DVBScanUtilPluginSetup()
    {
      InitializeComponent();
    }

    #endregion

    public override void LoadSettings()
    {
      try
      {
        var layer = new TvBusinessLayer();
        String DVBTScanUtilPluginSetupDefaultGroup = layer.GetSetting("DVBTScanUtilPluginSetupDefaultGroup", "New").Value;
        String DVBTScanUtilPluginSetupTuningXML = layer.GetSetting("DVBTScanUtilPluginSetupTuningXML", "").Value;
        String DVBCScanUtilPluginSetupTuningXML = layer.GetSetting("DVBCScanUtilPluginSetupTuningXML", "").Value;
        String DVBIPScanUtilPluginSetupTuningXML = layer.GetSetting("DVBIPScanUtilPluginSetupTuningXML", "").Value;

        textBoxDefaultGroup.Text = DVBTScanUtilPluginSetupDefaultGroup;
        textBoxDVBTTuningXML.Text = DVBTScanUtilPluginSetupTuningXML;
        textBoxDVBCTuningXML.Text = DVBCScanUtilPluginSetupTuningXML;
        textBoxDVBIPTuningXML.Text = DVBIPScanUtilPluginSetupTuningXML;
      }
      catch { }
    }

    public override void SaveSettings()
    {
      try
      {
        var layer = new TvBusinessLayer();
        Setting DVBTScanUtilPluginSetupDefaultGroup = layer.GetSetting("DVBTScanUtilPluginSetupDefaultGroup");
        DVBTScanUtilPluginSetupDefaultGroup.Value = textBoxDefaultGroup.Text;
        DVBTScanUtilPluginSetupDefaultGroup.Persist();

        Setting DVBTScanUtilPluginSetupTuningXML = layer.GetSetting("DVBTScanUtilPluginSetupTuningXML");
        DVBTScanUtilPluginSetupTuningXML.Value = textBoxDVBTTuningXML.Text;
        DVBTScanUtilPluginSetupTuningXML.Persist();

        Setting DVBCScanUtilPluginSetupTuningXML = layer.GetSetting("DVBCScanUtilPluginSetupTuningXML");
        DVBCScanUtilPluginSetupTuningXML.Value = textBoxDVBCTuningXML.Text;
        DVBCScanUtilPluginSetupTuningXML.Persist();

        Setting DVBIPScanUtilPluginSetupTuningXML = layer.GetSetting("DVBIPScanUtilPluginSetupTuningXML");
        DVBIPScanUtilPluginSetupTuningXML.Value = textBoxDVBIPTuningXML.Text;
        DVBIPScanUtilPluginSetupTuningXML.Persist();
      }
      catch { }
    }

    public override void OnSectionDeActivated()
    {
      Log.Info("EPGUtilPluginSetup: Configuration deactivated");
      SaveSettings();
      base.OnSectionDeActivated();
    }

    public override void OnSectionActivated()
    {
      Log.Info("EPGUtilPluginSetup: Configuration activated");
      LoadSettings();
      base.OnSectionActivated();
    }

    private void InitializeComponent()
    {
      this.textBoxDVBTTuningXML = new System.Windows.Forms.TextBox();
      this.textBoxDefaultGroup = new System.Windows.Forms.TextBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.label1 = new System.Windows.Forms.Label();
      this.textBoxDVBCTuningXML = new System.Windows.Forms.TextBox();
      this.label4 = new System.Windows.Forms.Label();
      this.textBoxDVBIPTuningXML = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // textBoxDVBTTuningXML
      // 
      this.textBoxDVBTTuningXML.Location = new System.Drawing.Point(132, 47);
      this.textBoxDVBTTuningXML.Name = "textBoxDVBTTuningXML";
      this.textBoxDVBTTuningXML.Size = new System.Drawing.Size(212, 20);
      this.textBoxDVBTTuningXML.TabIndex = 2;
      // 
      // textBoxDefaultGroup
      // 
      this.textBoxDefaultGroup.Location = new System.Drawing.Point(132, 114);
      this.textBoxDefaultGroup.Name = "textBoxDefaultGroup";
      this.textBoxDefaultGroup.Size = new System.Drawing.Size(212, 20);
      this.textBoxDefaultGroup.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(-3, 50);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(97, 13);
      this.label2.TabIndex = 4;
      this.label2.Text = "DVBT Tuning XML";
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(-3, 117);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(123, 13);
      this.label3.TabIndex = 5;
      this.label3.Text = "Group for New Channels";
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(-3, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(97, 13);
      this.label1.TabIndex = 7;
      this.label1.Text = "DVBC Tuning XML";
      // 
      // textBoxDVBCTuningXML
      // 
      this.textBoxDVBCTuningXML.Location = new System.Drawing.Point(132, 12);
      this.textBoxDVBCTuningXML.Name = "textBoxDVBCTuningXML";
      this.textBoxDVBCTuningXML.Size = new System.Drawing.Size(212, 20);
      this.textBoxDVBCTuningXML.TabIndex = 1;
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(-3, 85);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(100, 13);
      this.label4.TabIndex = 9;
      this.label4.Text = "DVBIP Tuning XML";
      // 
      // textBoxDVBIPTuningXML
      // 
      this.textBoxDVBIPTuningXML.Location = new System.Drawing.Point(132, 82);
      this.textBoxDVBIPTuningXML.Name = "textBoxDVBIPTuningXML";
      this.textBoxDVBIPTuningXML.Size = new System.Drawing.Size(212, 20);
      this.textBoxDVBIPTuningXML.TabIndex = 8;
      // 
      // DVBScanUtilPluginSetup
      // 
      this.Controls.Add(this.label4);
      this.Controls.Add(this.textBoxDVBIPTuningXML);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.textBoxDVBCTuningXML);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.textBoxDefaultGroup);
      this.Controls.Add(this.textBoxDVBTTuningXML);
      this.Name = "DVBScanUtilPluginSetup";
      this.Size = new System.Drawing.Size(401, 317);
      this.ResumeLayout(false);
      this.PerformLayout();

    }
  }
}

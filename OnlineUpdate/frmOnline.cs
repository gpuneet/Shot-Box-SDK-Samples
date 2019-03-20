using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Beesys.Wasp.Workflow;
using System.Configuration;
using System.IO;
using System.Collections;

namespace OnlineUpdate
{
    public partial class frmOnline : Form
    {

        #region Class variables

        #region Constant variables

        const string m_sUrl = "net.tcp://{0}:{1}/TcpBinding/WcfTcpLink";

        #endregion

        private ShotBox m_objShotBox = null;
        private Link m_objLink = null;
        private LinkManager m_objLinkManager = null;
        private string m_sServerIp = string.Empty;
        private string m_sLinkType = string.Empty;
        private string m_sPort = string.Empty;
        bool m_bIsPause = false;
        private UserTagCollection m_objUserTag;
        private FileInfo m_objFileInfo = null;

        #endregion


        #region Constructor

        public frmOnline()
        {
            InitializeComponent();
        }

        #endregion


        /// <summary>
        /// fires when engine is connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void objLink_OnEngineConnected(object sender, EngineArgs e)
        {
            btnConnect.BackColor = Color.DarkGreen;
        }

        /// <summary>
        /// used to connect with the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click_1(object sender, EventArgs e)
        {
            try
            {
                if ((Equals(m_objShotBox, null) && !string.IsNullOrEmpty(txtServerIp.Text)))
                {
                    switch (m_sLinkType.ToLower())
                    {
                        case "namedpipe":
                            m_sServerIp = string.Format("net.pipe://{0}/WcfNamedPipeLink", txtServerIp.Text);
                            break;
                        default:
                            m_sServerIp = string.Format(m_sUrl, txtServerIp.Text, m_sPort);
                            break;
                    }
                    m_objLink.Connect(m_sServerIp);
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("connecting", ex.Message);
            }
        }
        /// <summary>
        /// used for preparing the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadScene_Click_1(object sender, EventArgs e)
        {
            string sXml = string.Empty;
            string sShotBoxID = null;
            bool isTicker;
            try
            {
                #region sceneone

                if (m_objShotBox == null)
                {
                    sXml = Util.getSGFromWSL(fileDialog.FileName);
                    if (!string.IsNullOrEmpty(sXml))
                    {
                        m_objShotBox = m_objLink.GetShotBox(sXml, out sShotBoxID, out isTicker) as ShotBox;
                        if (!Equals(m_objShotBox, null))
                        {
                            m_objShotBox.SetEngineUrl(m_sServerIp);
                            if (m_objShotBox is IAddinInfo)
                            {
                                // S.No.			: -	1
                                //(m_objShotBox as IAddinInfo).Init(new InstanceInfo() { Type = "wsl", InstanceId = string.Empty, TemplateId = fileDialog.FileName, ThemeId = "default" });
                                (m_objShotBox as IAddinInfo).Init(new InstanceInfo() { Type = "wsl", InstanceId = fileDialog.FileName, TemplateId = fileDialog.FileName, ThemeId = "default" });
                        
                            }
                            m_objShotBox.OnShotBoxStatus += new EventHandler<SHOTBOXARGS>(m_objShotBox_OnShotBoxStatus);
                            m_objShotBox.Prepare(m_sServerIp, 0, RENDERMODE.PROGRAM);

                        }
                        m_objUserTag = m_objShotBox.UserTags;
                        //  if (cmbUserTag.SelectedIndex==-1)
                        // {
                        for (int i = 0; i < m_objUserTag.Count; i++)
                        {
                            if (!cmbUserTag.Items.Contains(m_objUserTag[i].Name))
                                cmbUserTag.Items.Add(m_objUserTag[i].Name);
                        }
                        // }
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in preparing the scenegraph", ex.Message);
            }

        }
        /// <summary>
        /// this event fires when scenegraph is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_objShotBox_OnShotBoxStatus(object sender, SHOTBOXARGS e)
        {
            if (Equals(e.SHOTBOXRESPONSE, SHOTBOXMSG.PREPARED))
            {
                btnPlay.BackColor = Color.DarkGreen;
                btnProgram.BackColor = Color.DarkGreen;
            }
        }
        /// <summary>
        /// used to selecting the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFileDialog_Click_1(object sender, EventArgs e)
        {
            try
            {
                fileDialog.Filter = "wsl files|*.wsl";
                fileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (Equals(fileDialog.ShowDialog(), DialogResult.OK))
                {
                    txtSceneName.Text = string.Empty;
                    txtSceneName.Tag = string.Empty;
                    m_objFileInfo = new FileInfo(fileDialog.FileName);
                    txtSceneName.Text = m_objFileInfo.Name;
                    txtSceneName.Tag = m_objFileInfo.FullName;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in selecting the scenegraph", ex.Message);
            }


        }
        /// <summary>
        /// used to play the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnPlay.BackColor = Color.DarkGray;
                    if (!m_bIsPause)
                    {
                        m_objShotBox.Play(true, true);
                    }
                    else
                        m_objShotBox.Play(false, false);
                    m_bIsPause = false;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in playing the scenegraph", ex.Message);
            }
        }
        /// <summary>
        /// used to pause the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnPause.BackColor = Color.DarkGray;
                    m_objShotBox.Pause();
                    m_bIsPause = true;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in pausing the scenegraph", ex.Message);
            }
        }
        /// <summary>
        /// used to stop the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnStop.BackColor = Color.DarkGray;
                    m_objShotBox.Stop();
                    m_bIsPause = false;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in stoping the scenegraph", ex.Message);
            }

        }
        /// <summary>
        /// used to taking the scenegraph on air
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOnAir_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetRender(true);
                    btnOnAir.BackColor = Color.DarkGray;
                    btnOffAir.BackColor = Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in taking the scenegraph on air", ex.Message);
            }
        }
        /// <summary>
        /// used to taking the scenegraph off air
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOffAir_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetRender(false);
                    btnOnAir.BackColor = Color.DarkGreen;
                    btnOffAir.BackColor = Color.DarkGray;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in taking the scenegraph off air", ex.Message);
            }

        }

        /// <summary>
        /// used to setting the mode as program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnProgram_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetMode(RENDERMODE.PROGRAM);
                    btnProgram.BackColor = Color.DarkGray;
                    btnPreview.BackColor = Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in setting the mode:program", ex.Message);
            }
        }
        /// <summary>
        /// used to setting the mode as preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPreview_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetMode(RENDERMODE.PREVIEW);
                    btnPreview.BackColor = Color.DarkGray;
                    btnProgram.BackColor = Color.DarkGreen;
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in setting the mode:preview", ex.Message);
            }
        }
        /// <summary>
        /// used to update the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click_1(object sender, EventArgs e)
        {
            TagData tagData;
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    if (!string.IsNullOrEmpty(cmbUserTag.SelectedItem.ToString()) && !string.IsNullOrEmpty(txtUserValue.Text))
                    {
                        tagData = new TagData();
                        tagData.IsOnAirUpdate = true;
                        tagData.SgXml = Util.getSGFromWSL(fileDialog.FileName);
                        
                        tagData.UserTags = new string[] { cmbUserTag.SelectedItem.ToString() };
                        tagData.Values = new string[] { txtUserValue.Text };
                        tagData.Indexes = new string[] { "-1" };
                        m_objShotBox.UpdateSceneGraph(tagData);
                    }
                }
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in updating the scenegraph", ex.Message);
            }
        }
        /// <summary>
        /// used to getting the link
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmOnline_Load(object sender, EventArgs e)
        {
            string sLinkID = string.Empty;
            try
            {
                txtServerIp.Text = ConfigurationManager.AppSettings["ipconfig"].ToString();
                m_sPort = ConfigurationManager.AppSettings["port"].ToString();

                m_sLinkType = ConfigurationManager.AppSettings["linktype"].ToString();
                m_objLinkManager = new LinkManager();
                if (!Equals(m_objLinkManager, null))
                {
                    if (string.Compare(m_sLinkType, "TCP", StringComparison.OrdinalIgnoreCase) == 0)
                        m_objLink = m_objLinkManager.GetLink(LINKTYPE.TCP, out sLinkID);
                    if (string.Compare(m_sLinkType, "NAMEDPIPE", StringComparison.OrdinalIgnoreCase) == 0)
                        m_objLink = m_objLinkManager.GetLink(LINKTYPE.NAMEDPIPE, out sLinkID);
                }
                if (!Equals(m_objLink, null))
                {
                    m_objLink.OnEngineConnected += new EventHandler<EngineArgs>(objLink_OnEngineConnected);
                }
                this.FormClosing += new FormClosingEventHandler(frmOnline_FormClosing);
            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("loading", ex.Message);
            }
        }

        void frmOnline_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.DeleteSg();
                }
                if (!Equals(m_objLink, null))
                {
                    m_objLink.DisconnectAll();
                }

            }
            catch (Exception ex)
            {
                LogWriter.WriteLog("error in form closing", ex.Message);
            }
        }

    }
}

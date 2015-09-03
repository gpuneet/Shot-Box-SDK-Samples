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

namespace GettingStarted
{
    public partial class GettingStart : Form
    {

        #region class members

        private              ShotBox                      m_objShotBox = null;
        private              Link                         m_objLink = null;
        private              LinkManager                  m_objLinkManager = null;
        private              string                       m_sServerIp = string.Empty;
        private              string                       m_sUrl = "net.tcp://{0}:{1}/TcpBinding/WcfTcpLink";
        private              bool                         m_isPause = false;
        private              FileInfo                     m_objFileInfo = null;
        private              string                       m_sLinkType = string.Empty;
        private              string                       m_sPort = string.Empty;
        private              const string                 MODULENAME = "WASP";

        #endregion


        #region constructor

        public GettingStart()
        {
            InitializeComponent();
        }//end (GettingStart)

        #endregion


        #region Events

        /// <summary>
        /// used to connect with the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {

            try
            {

                if (Equals(m_objShotBox, null) && !string.IsNullOrEmpty(txtServerIp.Text))
                {
                    switch (m_sLinkType.ToLower())
                    {
                        case "namedpipe":
                            m_sServerIp = string.Format("net.pipe://{0}/WcfNamedPipeLink", txtServerIp.Text);
                            break;
                        default:
                            m_sServerIp = string.Format(m_sUrl, txtServerIp.Text, m_sPort);
                            break;
                    }//end (switch)
                    m_objLink.Connect(m_sServerIp);
                }//end (if)

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnConnect_Click)


        /// <summary>
        /// fires when engine is connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void objLink_OnEngineConnected(object sender, EngineArgs e)
        {
            btnConnect.BackColor = Color.DarkGreen;
        }//end (objLink_OnEngineConnected)


        /// <summary>
        /// this event fires when user wants to close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GettingStart_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                    m_objShotBox.DeleteSg();
                if (!Equals(m_objLink, null))
                    m_objLink.DisconnectAll();
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (GettingStart_FormClosing)


        /// <summary>
        /// used to selecting the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnFileDialog_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.Filter = "wsl or w3d files (*.wsl;*.w3d)|*.wsl;*.w3d";
                openFileDialog1.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
                if (Equals(openFileDialog1.ShowDialog(), DialogResult.OK))
                {
                    m_objFileInfo = new FileInfo(openFileDialog1.FileName);
                    txtSceneName.Text = m_objFileInfo.Name;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnFileDialog_Click)


        /// <summary>
        /// used to setting the mode as program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProgram_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetMode(RENDERMODE.PROGRAM);
                    btnProgram.BackColor = Color.DarkGray;
                    btnPreview.BackColor = Color.DarkGreen;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnProgram_Click)


        /// <summary>
        /// used to setting the mode as preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetMode(RENDERMODE.PREVIEW);
                    btnPreview.BackColor = Color.DarkGray;
                    btnProgram.BackColor = Color.DarkGreen;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnPreview_Click)


        /// <summary>
        /// used to pause the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnPause.BackColor = Color.DarkGray;
                    m_objShotBox.Pause();
                    m_isPause = true;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnPause_Click)


        /// <summary>
        /// used to stop the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnStop.BackColor = Color.DarkGray;
                    m_objShotBox.Stop();
                    m_isPause = false;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnStop_Click)


        /// <summary>
        /// used to play the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnPlay.BackColor = Color.DarkGray;
                    if (!m_isPause)
                    {
                        m_objShotBox.Play(true, true);
                    }//end (if)
                    else
                        m_objShotBox.Play(false, false);
                    m_isPause = false;
                }//end (if)

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)


        }//end (btnPlay_Click)


        /// <summary>
        /// used for preparing the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadScene_Click(object sender, EventArgs e)
        {
            try
            {
                #region sceneone
                string sSgxml = Util.getSGFromWSL(openFileDialog1.FileName);
                string shotBoxID = null;
                bool isTicker;
                string filetype = string.Empty;

                if (!string.IsNullOrEmpty(sSgxml))
                {

                    m_objShotBox = m_objLink.GetShotBox(sSgxml, out shotBoxID, out isTicker) as ShotBox;
                    filetype = Path.GetExtension(openFileDialog1.FileName).Split(new string[] { "." }, StringSplitOptions.None)[1];
                    if (!Equals(m_objShotBox, null))
                    {
                        m_objShotBox.SetEngineUrl(m_sServerIp);
                        if (m_objShotBox is IAddinInfo)
                            (m_objShotBox as IAddinInfo).Init(new InstanceInfo() 
                            {
                                Type = filetype, 
                                InstanceId = string.Empty, 
                                TemplateId = openFileDialog1.FileName, 
                                ThemeId = "default",
                                 IsPreview=false
                            });
                        m_objShotBox.OnShotBoxStatus += new EventHandler<SHOTBOXARGS>(m_objShotBox_OnShotBoxStatus);
                        m_objShotBox.Prepare(m_sServerIp, 0, RENDERMODE.PROGRAM);

                    }//end (if)

                }//end (if)
                #endregion


            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)


        }//end (btnLoadScene_Click)

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
            }//end (if)
        }//end (m_objShotBox_OnShotBoxStatus)


        /// <summary>
        /// used to getting the link
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GettingStart_Load(object sender, EventArgs e)
        {
            try
            {
                string sLinkID = null;

                m_sPort = ConfigurationManager.AppSettings["port"].ToString();
                txtServerIp.Text = ConfigurationManager.AppSettings["ipconfig"].ToString();
                m_sLinkType = ConfigurationManager.AppSettings["linktype"].ToString();
                m_objLinkManager = new LinkManager();
                if (!Equals(m_objLinkManager, null))
                {
                    if (string.Compare(m_sLinkType, "TCP", StringComparison.OrdinalIgnoreCase) == 0)
                        m_objLink = m_objLinkManager.GetLink(LINKTYPE.TCP, out sLinkID);
                    if (string.Compare(m_sLinkType, "NAMEDPIPE", StringComparison.OrdinalIgnoreCase) == 0)
                        m_objLink = m_objLinkManager.GetLink(LINKTYPE.NAMEDPIPE, out sLinkID);

                }//end (if)

                if (!Equals(m_objLink, null))
                    m_objLink.OnEngineConnected += new EventHandler<EngineArgs>(objLink_OnEngineConnected);

                this.FormClosing += new FormClosingEventHandler(GettingStart_FormClosing);

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (GettingStart_Load)

        #endregion


     
    }
}

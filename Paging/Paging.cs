using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Configuration;
using System.Windows.Forms;

using Beesys.Wasp.Workflow;
namespace PagingApp
{
    public partial class Paging : Form
    {

        #region class members

        private         LinkManager               m_objLinkManager = null;
        private         Link                      m_objLink = null;
        private         ShotBox                   m_objShotBox = null;
        private         string                    m_sLinkType = string.Empty;
        private         string                    m_sEngineIP = string.Empty;
        private         string                    m_sEngineUrl = string.Empty;
        private         string                    m_sWslPath = string.Empty;
        private         bool                      m_bIsStop;
        private         bool                      m_bIsPauseInfinite;
        private         bool                      m_bIsPause;
        private         string                    m_sSGvariable;
        private         ArrayList                 m_objArrPageText = null;
        private         string                    m_sPort = string.Empty;
        private         int                       m_iPlayCount;
        private         TagData                   m_objTagData;
        private         const string              MODULENAME = "WASP";

        #endregion


        #region constructor

        public Paging()
        {
            InitializeComponent();
            m_objArrPageText = new ArrayList();
            m_objTagData = new TagData();
        }//end (Paging)

        #endregion


        #region events

        /// <summary>
        /// fires when engine is connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void objLink_OnEngineConnected(object sender, EventArgs e)
        {
            btnConnect.BackColor = Color.DarkGreen;
        }//end (objLink_OnEngineConnected)


        /// <summary>
        /// used to connect with the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            string sLinkFormat = null;
            try
            {
                if (m_objShotBox == null)
                {
                    if (!string.IsNullOrEmpty(txtServerIp.Text.Trim()))
                    {
                        m_sEngineIP = txtServerIp.Text;
                        switch (m_sLinkType.ToLower())
                        {

                            case "namedpipe":
                                sLinkFormat = string.Format("net.pipe://{0}/WcfNamedPipeLink", txtServerIp.Text);
                                break;
                            default:
                                sLinkFormat = string.Format("net.tcp://{0}:{1}/TcpBinding/WcfTcpLink", txtServerIp.Text, m_sPort);
                                break;
                        }//end (switch)
                        m_sEngineUrl = sLinkFormat;
                        m_objLink.Connect(sLinkFormat);

                    }//end (if)

                }//end (if)

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnConnect_Click)


        /// <summary>
        /// used to select and read the text file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTextFile_Click(object sender, EventArgs e)
        {
            FileInfo objFileInfo = null;
            try
            {
                openFileDialog.Filter = "Text Files (*.txt)|*.txt";
                if (Equals(openFileDialog.ShowDialog(), DialogResult.OK))
                {
                    if (!String.IsNullOrEmpty(openFileDialog.FileName))
                    {
                        txtTextFile.Text = String.Empty;
                        objFileInfo = new FileInfo(openFileDialog.FileName);
                        txtTextFile.Text = objFileInfo.Name;
                        txtTextFile.Tag = openFileDialog.FileName;
                        ReadFile(objFileInfo.FullName);
                    }//end (if)

                }//end (if)
                btnLoadScene.Enabled = true;
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnTextFile_Click)


        /// <summary>
        /// used to select the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScene_Click(object sender, EventArgs e)
        {
            FileInfo objFileInfo = null;
            try
            {
                openFileDialog.Filter = "wsl or w3d files (*.wsl;*.w3d)|*.wsl;*.w3d";
                openFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["WSL_PATH"];
                if (m_objShotBox == null)
                {
                    if (Equals(openFileDialog.ShowDialog(), DialogResult.OK))
                    {
                        txtSceneGraph.Text = string.Empty;
                        txtSceneGraph.Tag = string.Empty;
                        objFileInfo = new FileInfo(openFileDialog.FileName);
                        txtSceneGraph.Text = objFileInfo.Name;
                        txtSceneGraph.Tag = objFileInfo.FullName;
                        m_sWslPath = objFileInfo.FullName;
                    }//end (if)

                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
            finally
            {
                objFileInfo = null;
            }//end (finally)

        }//end (btnScene_Click)


        /// <summary>
        /// used to prepare the scenegraph
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoadScene_Click(object sender, EventArgs e)
        {
            string sSgXml = string.Empty;
            string sShotBoxID = string.Empty;
            bool bIsTicker;
            string filetype = string.Empty;
            try
            {

                if (Equals(m_objShotBox, null) && !Equals(txtTextFile.Tag, null))
                {

                    sSgXml = Util.getSGFromWSL(m_sWslPath);
                    filetype = Path.GetExtension(m_sWslPath).Split(new string[] { "." }, StringSplitOptions.None)[1];
                    if (!Equals(sSgXml, null))
                    {
                        m_sSGvariable = sSgXml;
                        m_objShotBox = m_objLink.GetShotBox(sSgXml, typeof(ShotBox), out sShotBoxID, out bIsTicker) as ShotBox;

                        m_objShotBox.SetEngineUrl(m_sEngineUrl);
                        if (m_objShotBox is IAddinInfo)
                            (m_objShotBox as IAddinInfo).Init(new InstanceInfo() { Type = filetype, InstanceId = string.Empty, TemplateId = m_sWslPath, ThemeId = "default" });                        
                        m_objShotBox.OnShotBoxStatus += new EventHandler<SHOTBOXARGS>(m_objShotBox_OnShotBoxStatus);
                        m_objShotBox.OnShotBoxControllerStatus += new EventHandler<SHOTBOXARGS>(m_objShotBox_OnShotBoxControllerStatus);
                        m_objShotBox.Prepare(m_sEngineIP, 0, string.Empty, RENDERMODE.PROGRAM);

                    }//end (if)
                }//end (if)


            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnLoadScene_Click)

      

        /// <summary>
        /// fires when controller send the pageout status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_objShotBox_OnShotBoxControllerStatus(object sender, SHOTBOXARGS e)
        {
            switch (e.SHOTBOXRESPONSE)
            {
                case SHOTBOXMSG.PAGEOUT: HandlePageOut();
                    break;

            }//end (switch)
        }//end (m_objShotBox_OnShotBoxControllerStatus)



        /// <summary>
        /// used to taking the scenegraph on air
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOnAir_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.SetRender(true);
                }//end (if)

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnOnAir_Click)


        /// <summary>
        /// used to taking the scenegraph off air
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOffAir_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                    m_objShotBox.SetRender(false);
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnOffAir_Click)


        /// <summary>
        /// used to change the mode as program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnProgram_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnProgram.BackColor = Color.DimGray;
                    btnPreview.BackColor = Color.DarkGreen;
                    m_objShotBox.SetMode(RENDERMODE.PROGRAM);

                }//end (if)
            }//end (try
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)

        }//end (btnProgram_Click)


        /// <summary>
        /// used to change the mode as preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    btnProgram.BackColor = Color.DarkGreen;
                    btnPreview.BackColor = Color.DimGray;
                    m_objShotBox.SetMode(RENDERMODE.PREVIEW);

                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnPreview_Click)


        /// <summary>
        /// used to get the link from the link manager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Paging_Load(object sender, EventArgs e)
        {
            string sLinkID = null;
            try
            {
                m_objLinkManager = new LinkManager();
                m_sPort = ConfigurationManager.AppSettings["port"].ToString();
                m_sLinkType = ConfigurationManager.AppSettings["linktype"].ToString();
                txtServerIp.Text = ConfigurationManager.AppSettings["ipconfig"];
                if (!Equals(m_objLinkManager, null))
                {
                    if (string.Compare(m_sLinkType, "TCP", StringComparison.OrdinalIgnoreCase) == 0)
                        m_objLink = m_objLinkManager.GetLink(LINKTYPE.TCP, out sLinkID);
                    if (string.Compare(m_sLinkType, "NAMEDPIPE", StringComparison.OrdinalIgnoreCase) == 0)
                        m_objLink = m_objLinkManager.GetLink(LINKTYPE.NAMEDPIPE, out sLinkID);

                }//end (if)
                //cbPlayText.Enabled = false;
                m_objLink.OnEngineConnected += new EventHandler<EngineArgs>(objLink_OnEngineConnected);
                this.FormClosing += new FormClosingEventHandler(Paging_FormClosing);

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (Paging_Load)


        /// <summary>
        /// fires when user wants to close the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Paging_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_objShotBox.DeleteSg();
                }//end (if)
                if (!Equals(m_objLink, null))
                {
                    m_objLink.DisconnectAll();
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (Paging_FormClosing)


        /// <summary>
        /// used to play the default controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlayDefaultController_Click(object sender, EventArgs e)
        {
            try
            {
                btnPlayDefaultController.BackColor = Color.DimGray;
                if (m_objShotBox != null)
                {
                    m_objTagData.UserTags = new string[] { m_objShotBox.UserTags[0].Name, m_objShotBox.UserTags[1].Name };
                    m_objTagData.Indexes = new string[] { "-1", "-1" };
                    m_objTagData.IsOnAirUpdate = true;
                    m_objTagData.SgXml = m_sSGvariable;
                    m_objTagData.TagType = new DataTargetType[] { DataTargetType.UserTag, DataTargetType.UserTag };
                    m_objTagData.Values = new string[] { "Header", m_objArrPageText[0].ToString().Trim() };
                    m_objShotBox.UpdateSceneGraph(m_objTagData);
                    m_objShotBox.Play(true,true);
                    m_iPlayCount++;
                   // m_objShotBox.Play();

                    #region old code
                    // m_objShotBox.Controllers[0].Play();
                    //if (m_bIsPause)
                    //{
                    //    m_objShotBox.Play(false, false);
                    //    m_objShotBox.Controllers[0].Play(false,false);
                    //    m_bIsPause = false;
                    //}//end (if)
                    //else
                    //{
                    //    if (m_bIsStop)
                    //    {
                    //        m_objShotBox.Controllers[0].Stop();
                    //        m_iPlayCount = 0;
                    //        m_objArrPageText.Clear();
                    //        ReadFile(txtTextFile.Tag.ToString());
                    //        m_bIsStop = false;
                    //    }//end (if)
                    //    m_objTagData.UserTags = new string[] { m_objShotBox.UserTags[0].Name, m_objShotBox.UserTags[1].Name };
                    //    m_objTagData.Indexes = new string[] { "-1", "-1" };
                    //    m_objTagData.IsOnAirUpdate = true;
                    //    m_objTagData.SgXml = m_sSGvariable;
                    //    m_objTagData.TagType = new DataTargetType[] { DataTargetType.UserTag, DataTargetType.UserTag };
                    //    m_objTagData.Values = new string[] { "Header", m_objArrPageText[0].ToString().Trim() };

                    //    m_objShotBox.UpdateSceneGraph(m_objTagData);
                    //    m_objShotBox.Play(true, true);
                    //    m_iPlayCount++;
                    //}//end (else)
                    #endregion
                   


                }//end (if)
            }//end (btnPlayDefaultController_Click)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnPlayDefaultController_Click)


        /// <summary>
        /// used to pause the default controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPauseDefaultController_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {

                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);

            }//end (catch)

        }//end (btnPauseDefaultController_Click)


        /// <summary>
        /// used to stop the default controller
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopDefaultController_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {
                    m_bIsPause = false;
                    m_bIsStop = true;
                    m_objShotBox.Stop();                  
                    m_iPlayCount = 0;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnStopDefaultController_Click)


        /// <summary>
        /// used to play the controller 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlayController_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Equals(m_objShotBox, null))
                {                
                    if (m_bIsPause)
                    {
                       m_objShotBox.Controllers[0].Play(false, false);
                        m_bIsPause = false;
                    }//end (if)
                    else
                    {
                        if (m_bIsStop)
                        {
                            //m_objShotBox.Controllers[0].Stop();
                            m_iPlayCount = 0;
                            m_objArrPageText.Clear();
                            ReadFile(txtTextFile.Tag.ToString());
                            m_bIsStop = false;
                        }//end (if)
                        m_objTagData.UserTags = new string[] { m_objShotBox.UserTags[0].Name, m_objShotBox.UserTags[1].Name };
                        m_objTagData.Indexes = new string[] { "-1", "-1" };
                        m_objTagData.IsOnAirUpdate = true;
                        m_objTagData.SgXml = m_sSGvariable;
                        m_objTagData.TagType = new DataTargetType[] { DataTargetType.UserTag, DataTargetType.UserTag };
                        m_objTagData.Values = new string[] { "Header", m_objArrPageText[0].ToString().Trim() };
                        m_objShotBox.UpdateSceneGraph(m_objTagData);
                        m_objShotBox.Play(true, true);
                        m_iPlayCount++;
                    }//end (else)

                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);

            }//end (catch)
        }//end (btnPlayController_Click)


        /// <summary>
        /// used to pause the controller 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPauseController_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_objShotBox != null)
                {
                   // m_objShotBox.Controllers[0].Pause();

                   // m_objShotBox.Pause();
                    m_objShotBox.Controllers[0].Pause();
                    m_bIsPause = true;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnPauseController_Click)


        /// <summary>
        /// used to stop the controller 1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopController_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_objShotBox != null)
                {
                     //m_objShotBox.Stop();
                    m_objShotBox.Controllers[0].Stop();
                    m_bIsPause = false;
                    m_bIsStop = true;
                   
                    m_iPlayCount = 0;
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (btnStopController_Click)



        /// <summary>
        /// this event fires when scenegraph is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void m_objShotBox_OnShotBoxStatus(object sender, SHOTBOXARGS e)
        {
            if (Equals(e.SHOTBOXRESPONSE, SHOTBOXMSG.PREPARED))
            {
                btnProgram.BackColor = Color.DarkGreen;
                btnPlayDefaultController.BackColor = Color.DarkGreen;
            }//end (if)
        }//end (m_objShotBox_OnShotBoxStatus)

        #endregion


        #region private methods

        /// <summary>
        /// handle pageout
        /// </summary>
        private void HandlePageOut()
        {
            try
            {
                if (m_objArrPageText.Count > m_iPlayCount)
                {
                    UpdatePlayController();
                }//end (if)
                else if (Equals(m_objArrPageText.Count, m_iPlayCount))
                {
                    ContinuePlay();
                }//end (else if)

            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);

            }//end (catch)

        }//end (HandlePageOut)


        private void ContinuePlay()
        {
            try
            {
                //if (!Equals(cbPlayText.CheckState, CheckState.Checked))
                //    m_objShotBox.Play();
                if (Equals(cbPlayText.CheckState, CheckState.Checked))
                {
                    m_iPlayCount = 0;
                    m_objArrPageText.Clear();
                    ReadFile(txtTextFile.Tag.ToString());
                    UpdatePlayController();

                }//end (else if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);
            }//end (catch)
        }//end (ContinuePlay)


        /// <summary>
        /// update the controller
        /// </summary>
        private void UpdatePlayController()
        {
            try
            {
                m_objTagData.UserTags = new string[] { m_objShotBox.UserTags[1].Name };
                m_objTagData.Indexes = new string[] { "-1" };
                m_objTagData.IsOnAirUpdate = true;
                m_objTagData.SgXml = m_sSGvariable;
                m_objTagData.TagType = new DataTargetType[] { DataTargetType.UserTag };
                m_objTagData.Values = new string[] { m_objArrPageText[m_iPlayCount].ToString().Trim() };             
                m_objShotBox.Controllers[0].GoTo(0);
                m_objShotBox.UpdateSceneGraph(m_objTagData);                
                m_objShotBox.Controllers[0].Play();
                m_iPlayCount++;
            }//end (try)
            catch(Exception ex)
            {
                LogWriter.WriteLog(MODULENAME,ex);
            }//end (catch)
           

        }//end (UpdatePlayController)


        /// <summary>
        /// used for reading the file
        /// </summary>
        /// <param name="sFile"></param>
        private void ReadFile(string sFile)
        {
            string sLine = null;
            try
            {
                if (!string.IsNullOrEmpty(sFile))
                {
                    TextReader objTextReader = new StreamReader(sFile);
                    while (!Equals(sLine = objTextReader.ReadLine(), null))
                    {
                        m_objArrPageText.Add(sLine);
                    }//end (while)
                    objTextReader.Close();
                }//end (if)
            }//end (try)
            catch (Exception ex)
            {
                LogWriter.WriteLog(MODULENAME, ex);

            }//end (catch)
        }//end (ReadFile)

        #endregion



    }


}

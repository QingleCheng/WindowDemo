using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Output;
using System.Collections.Generic;
using ESRI.ArcGIS.DataSourcesGDB;

namespace ArcGisView
{
    public sealed partial class MainForm : Form
    {
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;

        private IMoveEnvelopeFeedback pSmallViewerEnvelope;//ӥ��С��ͼ�ĺ��,IMoveEnvelopeFeedback,�����ƶ�Envelope�Ľӿ�
        private IPoint pSmallViewerMouseDownPt;//�϶�ʱ������
        private bool isTrackingSmallViewer = false; //��ʶ�Ƿ����϶�
        static int moveCount = 0;//��¼�ƶ��ĸ�����Ϊ�ƶ���������ʾ����á�

        private ArcGisPublic agp = new ArcGisPublic();

        private IToolbarMenu m_ToolbarMenu = new ToolbarMenuClass();//�Ҽ��˵�



        #endregion

        #region class constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            pEn = axMapControl1.Extent;//ӥ�ۺ���ʼ��
            CreateOverviewSymbol();
            //get the MapControl
            axMapControl1.KeyIntercept = (int)esriKeyIntercept.esriKeyInterceptArrowKeys;//�趨���������ŵ�ͼ��Ч
            axMapControl1.AutoMouseWheel = true;//�趨axMapControl1���������ŵ�ͼ��Ч
            axMapControl1.AutoKeyboardScrolling = true;//�趨axMapControl1���������ŵ�ͼ��Ч
            axMapControl2.AutoMouseWheel = false;//�趨axMapControl2���������ŵ�ͼ��Ч
            axMapControl2.AutoKeyboardScrolling = false;//�趨axMapControl2���������ŵ�ͼ��Ч

            m_mapControl = (IMapControl3)axMapControl1.Object;

            //disable the Save menu (since there is no document yet)
            menuSaveDoc.Enabled = false;
            BindRightMenu();
        }

        #region Main Menu event handlers
        private void menuNewDoc_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuOpenDoc_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuSaveDoc_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (m_mapControl.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocumentClass();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("��ͼ�ļ��Ѿ�׼�����!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)m_mapControl.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            //execute SaveAs Document command
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuExitApp_Click(object sender, EventArgs e)
        {
            //exit the application
            Application.Exit();
        }
        #endregion

        //listen to MapReplaced evant in order to update the statusbar and the Save menu
        #region ӥ�۵�ʵ�֣��������Ҽ��϶�
        IMapDocument pMapDocument = new MapDocumentClass();
        IEnvelope pEn = new EnvelopeClass();
        object oFillobject = new object();
        private void CreateOverviewSymbol()
        {
            IRgbColor iRgb = new RgbColorClass();
            iRgb.RGB = 255;
            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Color = iRgb;
            pOutline.Width = 2.3;
            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            pSimpleFillSymbol.Outline = pOutline;
            pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSHollow;
            oFillobject = pSimpleFillSymbol;
        }

        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {

            axMapControl2.LoadMxFile(axMapControl1.DocumentFilename);
            axMapControl2.Extent = axMapControl1.FullExtent;
            //get the current document name from the MapControl
            m_mapDocumentName = m_mapControl.DocumentFilename;

            //if there is no MapDocument, diable the Save menu and clear the statusbar
            if (m_mapDocumentName == string.Empty)
            {
                menuSaveDoc.Enabled = false;
                statusBarXY.Text = string.Empty;
            }
            else
            {
                //enable the Save manu and write the doc name to the statusbar
                menuSaveDoc.Enabled = true;
                statusBarXY.Text = System.IO.Path.GetFileName(m_mapDocumentName);
            }
        }

        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            pEn = e.newEnvelope as IEnvelope;
            axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
        }

        private void axMapControl1_OnAfterDraw(object sender, IMapControlEvents2_OnAfterDrawEvent e)
        {
            esriViewDrawPhase viewDrawPhase = (esriViewDrawPhase)e.viewDrawPhase;
            if (viewDrawPhase == esriViewDrawPhase.esriViewForeground)
            {
                axMapControl2.DrawShape(pEn, ref oFillobject);
            }
        }

        private void axMapControl2_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 1)//��������
            {
                pEn = axMapControl2.TrackRectangle();
                axMapControl1.Extent = pEn;
                axMapControl2.DrawShape(pEn, ref oFillobject);
            }
            if (e.button == 2)//�Ҽ��϶����
            {
                pSmallViewerMouseDownPt = new PointClass();
                pSmallViewerMouseDownPt.PutCoords(e.mapX, e.mapY);
                axMapControl1.CenterAt(pSmallViewerMouseDownPt);

                isTrackingSmallViewer = true;
                if (pSmallViewerEnvelope == null)
                {
                    pSmallViewerEnvelope = new MoveEnvelopeFeedbackClass();
                    pSmallViewerEnvelope.Display = axMapControl2.ActiveView.ScreenDisplay;
                    pSmallViewerEnvelope.Symbol = (ISymbol)oFillobject;
                }
                pSmallViewerEnvelope.Start(pEn, pSmallViewerMouseDownPt);
            }
        }

        private void axMapControl2_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (isTrackingSmallViewer)
            {
                moveCount++;
                if (moveCount % 4 == 0)//��Ϊһˢ�£�����û�ˡ�����ÿ�ƶ�4�ξ�ˢ��һ�£����ֺ��������ԡ�
                    axMapControl2.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewForeground, null, null);
                pSmallViewerMouseDownPt.PutCoords(e.mapX, e.mapY);
                pSmallViewerEnvelope.MoveTo(pSmallViewerMouseDownPt);
            }
        }

        private void axMapControl2_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (pSmallViewerEnvelope != null)
            {
                pEn = pSmallViewerEnvelope.Stop();
                axMapControl1.Extent = pEn;
                isTrackingSmallViewer = false;
            }
        }
        #endregion

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.button == 1 && search)//����ǲ�ѯ״̬������������ 
            {
                identifyDialog.OnMouseMove(e.mapX, e.mapY);
            }
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }


        private IdentifyDialog identifyDialog;

        private void ShowIdentifyDialog()
        {
            //�½����Բ�ѯ����
            identifyDialog = IdentifyDialog.CreateInstance(axMapControl1);
            identifyDialog.Owner = this;
            identifyDialog.Show();
        }

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 1 && insertbiaozhu)
            {
                agp.CreateTextElment(axMapControl1, e.mapX, e.mapY, "����");
            }
            if (e.button == 1 && insertpoint)
            {
                IFeatureLayer oFeatureLayer = axMapControl1.get_Layer(axMapControl1.LayerCount - 1) as IFeatureLayer;
                agp.AddPointByStore(axMapControl1, oFeatureLayer, e.mapX, e.mapY);
            }

            if (e.button == 1 && insertline)
            {
                IFeatureLayer oFeatureLayer = axMapControl1.get_Layer(axMapControl1.LayerCount - 1) as IFeatureLayer;
                agp.AddLineByWrite(this.axMapControl1 ,oFeatureLayer, e.mapX, e.mapY);
            }

            if (e.button == 1 && insertPolygon)
            {
                IFeatureLayer oFeatureLayer = axMapControl1.get_Layer(axMapControl1.LayerCount - 1) as IFeatureLayer;
                agp.AddPolygonByWrite(this.axMapControl1, oFeatureLayer, e.mapX, e.mapY);
            }

            if (e.button == 1 && search)//����ǲ�ѯ״̬������������ 
            {

                if (identifyDialog.IsDisposed)
                {
                    ShowIdentifyDialog();
                }
                identifyDialog.OnMouseDown(e.button, e.mapX, e.mapY);
                //IMap pMap;
                //int i;
                //IPoint pPoint;
                //pMap = axMapControl1.Map;
                //pPoint = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(e.x, e.y);

                //IEnvelope pEnv;
                //pEnv = axMapControl1.ActiveView.Extent;
                //pEnv.Height = 1;
                //pEnv.Width = 1;
                //pEnv.CenterAt(pPoint);

                ////ִ�в�ѯ��ȡ����������Ҫ��
                //List<IFeature> pFList = agp.GetSeartchFeatures(axMapControl1.get_Layer(0) as IFeatureLayer, pEnv);
                //if (pFList.Count > 0)
                //{
                //    //��Ϣ��ʾ��ѯĿ�����Ϣ
                //    SearchViewInfo frm = null;
                //    if (frm == null || frm.IsDisposed)
                //        frm = new SearchViewInfo(pFList[0]);

                //    frm.Show();
                //    frm.TopMost = true;
                //}
                //else

                //{
                //    MessageBox.Show ("û���ҵ��κνڵ�!");
                //}
            }

            if (e.button == 2 && !bSearch)//������Ҽ����Ҳ��ǲ�ѯ״̬
            {

                if (axMapControl1.DocumentFilename != "")
                {
                    m_ToolbarMenu.PopupMenu(e.x, e.y, axMapControl1.hWnd);
                }
            }

            //if (bSearch && e.button == 1)//����ǲ�ѯ״̬������������ 
            //{
            //    //���������ʽΪʮ��˿
            //    this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
            //    //��ȡ������Χ��Geometry
            //    IGeometry pGeometry = this.axMapControl1.TrackPolygon();
            //    //��Ӱ�͸����ʱͼ��
            //    agp.AddTransTempEle(this.axMapControl1, pGeometry, true);



            //    //for (int a = 0; a < axMapControl1.LayerCount; a++)
            //    //{


            //    IFeatureLayer pFeatureLayer = this.axMapControl1.get_Layer(1) as IFeatureLayer;

            //    attribute pAttribute = new attribute(pFeatureLayer);
            //    //ִ�в�ѯ��ȡ����������Ҫ��
            //    List<IFeature> pFList = agp.GetSeartchFeatures(pFeatureLayer, pGeometry);
            //    //������Ϣ��ʾ������DataGridView������
            //    //��������pFList.Count+1�����ֶ�����һ�м���ͷ
            //    pAttribute.dataGridView1.RowCount = pFList.Count + 1;
            //    //���ñ߽���
            //    pAttribute.dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Sunken;
            //    //��������
            //    pAttribute.dataGridView1.ColumnCount = pFList[0].Fields.FieldCount;
            //    //������һ��Ҫ�ص��ֶ����ڸ���ͷ��ֵ���ֶε����ƣ�
            //    for (int m = 0; m < pFList[0].Fields.FieldCount; m++)
            //    {
            //        pAttribute.dataGridView1.Columns[m].HeaderText = pFList[0].Fields.get_Field(m).AliasName;
            //    }
            //    //����Ҫ��
            //    for (int i = 0; i < pFList.Count; i++)
            //    {
            //        IFeature pFeature = pFList[i];
            //        for (int j = 0; j < pFeature.Fields.FieldCount; j++)
            //        {
            //            //����ֶ�ֵ
            //            pAttribute.dataGridView1[j, i].Value = pFeature.get_Value(j).ToString();
            //        }
            //    }

            //    pAttribute.Show();
            //    //}
            //}
        }

        private void BindRightMenu()//����Ҽ��˵�
        {
            m_ToolbarMenu.AddItem(new FixeZoomIn(), 1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);//��ͼ�Ŵ�
            m_ToolbarMenu.AddItem(new FixeZoomOut(), 1, 1, false, esriCommandStyles.esriCommandStyleTextOnly);//��ͼ��С
            m_ToolbarMenu.AddItem(new FullExtent(), 1, 2, false, esriCommandStyles.esriCommandStyleTextOnly);//��ʾȫͼ
            m_ToolbarMenu.AddItem(new PanClass(), 1, 3, false, esriCommandStyles.esriCommandStyleTextOnly);//��ͼƽ��
            m_ToolbarMenu.AddItem(new Measuredis(), 1, 4, false, esriCommandStyles.esriCommandStyleTextOnly);//��ͼ����
            m_ToolbarMenu.AddItem(new PrintPage(axMapControl1), 1, 5, false, esriCommandStyles.esriCommandStyleTextOnly);//��ͼ��ӡ
            m_ToolbarMenu.AddItem(new ExportImg(axMapControl1, this), 1, 6, false, esriCommandStyles.esriCommandStyleTextOnly);//����ͼƬ
            m_ToolbarMenu.SetHook(axMapControl1);
        }


        bool bSearch = false;��//����bool����������������β�ѯ����

        private void �ؼ��ֲ�ѯToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchKey frm = null;
            if (frm == null || frm.IsDisposed)
                frm = new SearchKey(axMapControl1);

            frm.Show();
            frm.TopMost = true;
        }

        //private void �����ѯToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //���ͼ�ؼ�����ڴ�ͼ��
        //        IFeatureLayer pFeatureLayer = agp.AddFeatureLayerByMemoryWS(this.axMapControl1, this.axMapControl1.SpatialReference);
        //        string a = pFeatureLayer.DataSourceType;
        //        this.axMapControl1.AddLayer(pFeatureLayer);
        //        //���������ʽΪʮ��˿
        //        this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
        //        //������Χ��ѯ����
        //        bSearch = true;
        //    }
        //    catch
        //    { }
        //}
        bool insertpoint = false;
        private void ��ӵ���ͼ��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //����Ҫ����
            #region �����µ��ڴ湤���ռ�
            IWorkspaceFactory pWSF = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName pWSName = pWSF.Create("", "Temp", null, 0);

            IName pName = (IName)pWSName;
            IWorkspace pMemoryWS = (IWorkspace)pName.Open();
            #endregion

            IField oField = new FieldClass();
            IFields oFields = new FieldsClass();
            IFieldsEdit oFieldsEdit = null;
            IFieldEdit oFieldEdit = null;
            IFeatureClass oFeatureClass = null;
            IFeatureLayer oFeatureLayer = null;

            oFieldsEdit = oFields as IFieldsEdit;
            oFieldEdit = oField as IFieldEdit;
            oFieldEdit.Name_2 = "OBJECTID";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            oFieldEdit.IsNullable_2 = false;
            oFieldEdit.Required_2 = false;
            oFieldsEdit.AddField(oField);

            oField = new FieldClass();
            oFieldEdit = oField as IFieldEdit;
            IGeometryDef pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.AvgNumPoints_2 = 5;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            pGeoDefEdit.GridCount_2 = 1;
            pGeoDefEdit.HasM_2 = false;
            pGeoDefEdit.HasZ_2 = false;
            pGeoDefEdit.SpatialReference_2 = axMapControl1.SpatialReference;
            oFieldEdit.Name_2 = "SHAPE";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            oFieldEdit.GeometryDef_2 = pGeoDef;
            oFieldEdit.IsNullable_2 = true;
            oFieldEdit.Required_2 = true;
            oFieldsEdit.AddField(oField);

            oField = new FieldClass();
            oFieldEdit = oField as IFieldEdit;
            oFieldEdit.Name_2 = "Code";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
            //oFieldEdit.Length = 10;
            oFieldEdit.IsNullable_2 = true;
            oFieldsEdit.AddField(oField);
            //����Ҫ����
            oFeatureClass = (pMemoryWS as IFeatureWorkspace).CreateFeatureClass("Temp", oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            oFeatureLayer = new FeatureLayerClass();
            oFeatureLayer.Name = "PointLayer";
            oFeatureLayer.FeatureClass = oFeatureClass;
            //����Ψһֵ���Ż�����


            IUniqueValueRenderer pURender = new UniqueValueRendererClass();
            pURender.FieldCount = 1;
            pURender.set_Field(0, "Code");
            pURender.UseDefaultSymbol = false;
            //����SimpleMarkerSymbolClass����
            ISimpleMarkerSymbol pSimpleMarkerSymbol = new SimpleMarkerSymbolClass();
            //����RgbColorClass����ΪpSimpleMarkerSymbol������ɫ
            IRgbColor pRgbColor = new RgbColorClass();
            pRgbColor.Red = 255;
            pSimpleMarkerSymbol.Color = pRgbColor as IColor;
            //����pSimpleMarkerSymbol����ķ������ͣ�ѡ����ʯ
            pSimpleMarkerSymbol.Style = esriSimpleMarkerStyle.esriSMSDiamond;
            //����pSimpleMarkerSymbol�����С������Ϊ��
            pSimpleMarkerSymbol.Size = 5;
            //��ʾ�����
            pSimpleMarkerSymbol.Outline = true;
            //Ϊ�����������ɫ
            IRgbColor pLineRgbColor = new RgbColorClass();
            pLineRgbColor.Green = 255;
            pSimpleMarkerSymbol.OutlineColor = pLineRgbColor as IColor;
            //��������ߵĿ��
            pSimpleMarkerSymbol.OutlineSize = 1; 

            //��͸����ɫ

 


            pURender.AddValue("1", "", pSimpleMarkerSymbol as ISymbol);

            //Ψһֵ���Ż��ڴ�ͼ��
            (oFeatureLayer as IGeoFeatureLayer).Renderer = pURender as IFeatureRenderer;
            ILayerEffects pLyrEffect = oFeatureLayer as ILayerEffects;
            //͸����
            pLyrEffect.Transparency = 0;


            oFeatureLayer.Visible = true;

            this.axMapControl1.AddLayer(oFeatureLayer,axMapControl1.LayerCount);
            insertpoint = true;
        }

        private bool search = false;
        private void �����ѯToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!search)
            {
                �����ѯToolStripMenuItem.Text = "ȡ������ѯ";
                ShowIdentifyDialog();
            }
            else
            {
                �����ѯToolStripMenuItem.Text = "����ѯ";
                if (!identifyDialog.IsDisposed)
                {
                    identifyDialog.Dispose();
                }
            }
            search = !search;

        }

        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            if (e.button == 1 && search)//����ǲ�ѯ״̬������������ 
            {
                identifyDialog.OnMouseUp(e.mapX, e.mapY);
            }
        }

        bool insertline = false;
        private void �������ͼ��ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //����Ҫ����
            #region �����µ��ڴ湤���ռ�
            IWorkspaceFactory pWSF = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName pWSName = pWSF.Create("", "Temp", null, 0);

            IName pName = (IName)pWSName;
            IWorkspace pMemoryWS = (IWorkspace)pName.Open();
            #endregion

            IField oField = new FieldClass();
            IFields oFields = new FieldsClass();
            IFieldsEdit oFieldsEdit = null;
            IFieldEdit oFieldEdit = null;
            IFeatureClass oFeatureClass = null;
            IFeatureLayer oFeatureLayer = null;

            oFieldsEdit = oFields as IFieldsEdit;
            oFieldEdit = oField as IFieldEdit;
            oFieldEdit.Name_2 = "OBJECTID";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            oFieldEdit.IsNullable_2 = false;
            oFieldEdit.Required_2 = false;
            oFieldsEdit.AddField(oField);

            oField = new FieldClass();
            oFieldEdit = oField as IFieldEdit;
            IGeometryDef pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.AvgNumPoints_2 = 5;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline;
            pGeoDefEdit.GridCount_2 = 1;
            pGeoDefEdit.HasM_2 = false;
            pGeoDefEdit.HasZ_2 = false;
            pGeoDefEdit.SpatialReference_2 = axMapControl1.SpatialReference;
            oFieldEdit.Name_2 = "SHAPE";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            oFieldEdit.GeometryDef_2 = pGeoDef;
            oFieldEdit.IsNullable_2 = true;
            oFieldEdit.Required_2 = true;
            oFieldsEdit.AddField(oField);

            oField = new FieldClass();
            oFieldEdit = oField as IFieldEdit;
            oFieldEdit.Name_2 = "Code";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
            oFieldEdit.IsNullable_2 = true;
            oFieldsEdit.AddField(oField);
            //����Ҫ����
            oFeatureClass = (pMemoryWS as IFeatureWorkspace).CreateFeatureClass("Temp", oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            oFeatureLayer = new FeatureLayerClass();
            oFeatureLayer.Name = "LineLayer";
            oFeatureLayer.FeatureClass = oFeatureClass;
            //����Ψһֵ���Ż�����
            IUniqueValueRenderer pURender = new UniqueValueRendererClass();
            pURender.FieldCount = 1;
            pURender.set_Field(0, "Code");
            pURender.UseDefaultSymbol = false;
            ISimpleFillSymbol pFillSym = new SimpleFillSymbolClass();
            pFillSym.Style = esriSimpleFillStyle.esriSFSSolid;
            //��͸����ɫ
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 255;
            pFillSym.Color = pColor;
            pURender.AddValue("1", "", pFillSym as ISymbol);
            pFillSym = new SimpleFillSymbolClass();
            pFillSym.Style = esriSimpleFillStyle.esriSFSSolid;
            //Ψһֵ���Ż��ڴ�ͼ��
            (oFeatureLayer as IGeoFeatureLayer).Renderer = pURender as IFeatureRenderer;
            ILayerEffects pLyrEffect = oFeatureLayer as ILayerEffects;
            //͸����
            pLyrEffect.Transparency = 0;



            this.axMapControl1.AddLayer(oFeatureLayer, axMapControl1.LayerCount);
            insertline = true;
        }

        private void �ر���ӵ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            insertline=false;
            insertpoint = false;
            insertPolygon = false;
        }

        private bool insertbiaozhu = false;
        private void ��ʼ���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            insertbiaozhu = true;
        }

        private void �������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            insertbiaozhu = false;
        }

        private bool insertPolygon = false;
        private void ������ε���ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //����Ҫ����
            #region �����µ��ڴ湤���ռ�
            IWorkspaceFactory pWSF = new InMemoryWorkspaceFactoryClass();
            IWorkspaceName pWSName = pWSF.Create("", "Temp", null, 0);

            IName pName = (IName)pWSName;
            IWorkspace pMemoryWS = (IWorkspace)pName.Open();
            #endregion

            IField oField = new FieldClass();
            IFields oFields = new FieldsClass();
            IFieldsEdit oFieldsEdit = null;
            IFieldEdit oFieldEdit = null;
            IFeatureClass oFeatureClass = null;
            IFeatureLayer oFeatureLayer = null;

            oFieldsEdit = oFields as IFieldsEdit;
            oFieldEdit = oField as IFieldEdit;
            oFieldEdit.Name_2 = "OBJECTID";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            oFieldEdit.IsNullable_2 = false;
            oFieldEdit.Required_2 = false;
            oFieldsEdit.AddField(oField);

            oField = new FieldClass();
            oFieldEdit = oField as IFieldEdit;
            IGeometryDef pGeoDef = new GeometryDefClass();
            IGeometryDefEdit pGeoDefEdit = (IGeometryDefEdit)pGeoDef;
            pGeoDefEdit.AvgNumPoints_2 = 5;
            pGeoDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
            pGeoDefEdit.GridCount_2 = 1;
            pGeoDefEdit.HasM_2 = false;
            pGeoDefEdit.HasZ_2 = false;
            pGeoDefEdit.SpatialReference_2 = axMapControl1.SpatialReference;
            oFieldEdit.Name_2 = "SHAPE";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            oFieldEdit.GeometryDef_2 = pGeoDef;
            oFieldEdit.IsNullable_2 = true;
            oFieldEdit.Required_2 = true;
            oFieldsEdit.AddField(oField);

            oField = new FieldClass();
            oFieldEdit = oField as IFieldEdit;
            oFieldEdit.Name_2 = "Code";
            oFieldEdit.Type_2 = esriFieldType.esriFieldTypeSmallInteger;
            oFieldEdit.IsNullable_2 = true;
            oFieldsEdit.AddField(oField);
            //����Ҫ����
            oFeatureClass = (pMemoryWS as IFeatureWorkspace).CreateFeatureClass("Temp", oFields, null, null, esriFeatureType.esriFTSimple, "SHAPE", "");
            oFeatureLayer = new FeatureLayerClass();
            oFeatureLayer.Name = "PolygonLayer";
            oFeatureLayer.FeatureClass = oFeatureClass;
            //����Ψһֵ���Ż�����
            IUniqueValueRenderer pURender = new UniqueValueRendererClass();
            pURender.FieldCount = 1;
            pURender.set_Field(0, "Code");
            pURender.UseDefaultSymbol = false;
            ISimpleFillSymbol pFillSym = new SimpleFillSymbolClass();
            pFillSym.Style = esriSimpleFillStyle.esriSFSSolid;
            //��͸����ɫ
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 255;
            pFillSym.Color = pColor;
            pURender.AddValue("1", "", pFillSym as ISymbol);
            pFillSym = new SimpleFillSymbolClass();
            pFillSym.Style = esriSimpleFillStyle.esriSFSSolid;
            //Ψһֵ���Ż��ڴ�ͼ��
            (oFeatureLayer as IGeoFeatureLayer).Renderer = pURender as IFeatureRenderer;
            ILayerEffects pLyrEffect = oFeatureLayer as ILayerEffects;
            //͸����
            pLyrEffect.Transparency = 0;



            this.axMapControl1.AddLayer(oFeatureLayer, axMapControl1.LayerCount);
            insertPolygon = true;
        }

        private void ɾ������ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!search)
            {
                �����ѯToolStripMenuItem.Text = "ȡ������ѯ";
                ShowIdentifyDialog();
            }
            else
            {
                �����ѯToolStripMenuItem.Text = "����ѯ";
                if (!identifyDialog.IsDisposed)
                {
                    identifyDialog.Dispose();
                }
            }
            search = !search;
        }
    }
}
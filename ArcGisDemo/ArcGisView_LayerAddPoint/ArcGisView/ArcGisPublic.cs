using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.DataSourcesGDB;
using ESRI.ArcGIS.Output;
using ESRI.ArcGIS.Analyst3D;



namespace ArcGisView
{
    public class ArcGisPublic
    {
        public void export(AxMapControl MapCtrl, Form hwin)//������ͼƬ
        {
            try
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "(*.tif)|*.tif|(*.jpeg)|*.jpeg|(*.pdf)|*.pdf|(*.bmp)|*.bmp";
                if (sfd.ShowDialog(hwin) == DialogResult.OK)
                {
                    IExporter pExport = null;
                    if (1 == sfd.FilterIndex)
                    {
                        pExport = new TiffExporter() as IExporter;
                        pExport.ExportFileName = sfd.FileName;
                    }
                    else if (2 == sfd.FilterIndex)
                    {
                        pExport = new JpegExporter() as IExporter;
                        pExport.ExportFileName = sfd.FileName;
                    }
                    else if (3 == sfd.FilterIndex)
                    {
                        pExport = new PDFExporter() as IExporter;
                        pExport.ExportFileName = sfd.FileName;
                    }
                    else if (4 == sfd.FilterIndex)
                    {
                        pExport = new DibExporter() as IExporter; pExport.ExportFileName = sfd.FileName;
                    }
                    short res = 96;
                    pExport.Resolution = res;
                    tagRECT exportRECT = MapCtrl.ActiveView.ExportFrame;
                    IEnvelope pENV = new EnvelopeClass();
                    pENV.PutCoords(exportRECT.left, exportRECT.top, exportRECT.right, exportRECT.bottom);
                    pExport.PixelBounds = pENV;
                    int Hdc = pExport.StartExporting();
                    IEnvelope pVisibleBounds = null;
                    ITrackCancel pTrack = null;
                    MapCtrl.ActiveView.Output(Hdc, (int)pExport.Resolution, ref exportRECT, pVisibleBounds, pTrack);
                    Application.DoEvents();
                    pExport.FinishExporting();
                }
            }
            catch { }
        }

        /// <summary>
        /// ����Ҫ�ؼ����������ͼ���ཻ��Ҫ�أ��������α�
        /// </summary>
        /// <param name="LineFeatClass"></param>
        /// <param name="geo"></param>
        /// <returns></returns>
        public static IFeatureCursor SearchIntersectLineFeat(IFeatureClass LineFeatClass, IGeometry geo)
        {
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = geo;
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
            pSpatialFilter.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;
            IFeatureCursor pfeatCursor = LineFeatClass.Search(pSpatialFilter, false);
            return pfeatCursor;
        }
        /// <summary>
        /// ��������ͼ�������ڵ�Ҫ��
        /// </summary>
        /// <param name="fc"></param>
        /// <param name="geo"></param>
        /// <returns></returns>
        public static IFeatureCursor SearchContainFeat(IFeatureClass fc, IGeometry geo)
        {
            ISpatialFilter pSpatialFilter = new SpatialFilterClass();
            pSpatialFilter.Geometry = geo;
            pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
            pSpatialFilter.SearchOrder = esriSearchOrder.esriSearchOrderSpatial;
            IFeatureCursor pfeatCursor = fc.Search(pSpatialFilter, false);
            return pfeatCursor;
        }

        ///<summary>
        ///�ڳ�������ʱ���ڴ��д���ʸ��Ҫ�ز㣬���ӵ���ͼ�ؼ����
        ///</summary>
        ///<param name="pMapCtrl">��ͼ�ؼ�</param>
        ///<returns>IFeatureLayer �¼ӵ�Ҫ�ز�</returns>
        public IFeatureLayer AddFeatureLayerByMemoryWS(AxMapControl pMapCtrl, ISpatialReference pSReference)
        {
            try
            {
                if (pMapCtrl == null)
                    return null;

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
                try
                {
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
                    pGeoDefEdit.SpatialReference_2 = pSReference;
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
                    oFeatureLayer.Name = "TransTemp";
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
                    pLyrEffect.Transparency = 80;
                }
                catch (Exception Err)
                {
                    MessageBox.Show(Err.Message);
                }
                finally
                {
                    try
                    {
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oField);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFields);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldsEdit);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFieldEdit);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pName);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pWSF);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pWSName);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(pMemoryWS);
                        System.Runtime.InteropServices.Marshal.ReleaseComObject(oFeatureClass);
                    }
                    catch
                    {
                    }
                    GC.Collect();
                }
                return oFeatureLayer;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }


        /// <summary>
        /// ��ȡ��ѯҪ��
        /// </summary>
        /// <param name="pFeatureLayer">Ҫ��ͼ��</param>
        /// <param name="pGeometry">ͼ�η�Χ����</param>
        /// <returns>��������Ҫ�ؼ���</returns>
        public List<IFeature> GetSeartchFeatures(IFeatureLayer pFeatureLayer, IGeometry pGeometry)
        {
            try
            {
                List<IFeature> pList = new List<IFeature>();
                //����SpatialFilter�ռ����������
                ISpatialFilter pSpatialFilter = new SpatialFilterClass();
                IQueryFilter pQueryFilter = pSpatialFilter as ISpatialFilter;
                //���ù�������Geometry
                pSpatialFilter.Geometry = pGeometry;
                //���ÿռ��ϵ����
                pSpatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                //��ȡFeatureCursor�α�
                IFeatureCursor pFeatureCursor = pFeatureLayer.Search(pQueryFilter, false);
                //����FeatureCursor
                IFeature pFeature = pFeatureCursor.NextFeature();
                while (pFeature != null)
                {
                    //��ȡҪ�ض���
                    pList.Add(pFeature);
                    pFeature = pFeatureCursor.NextFeature();
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pFeatureCursor);
                return pList;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }


        /// <summary> �ڵ�ͼ�ؼ������͸����ʱͼԪ/// </summary>
        /// <param name="pMapCtrl">��ͼ�ؼ�</param>
        /// <param name="pGeo">Envelope��Polygon����ʵ��</param>
        /// <param name="bAutoClear">�Ƿ����ԭ������</param>
        public void AddTransTempEle(AxMapControl pMapCtrl, IGeometry pGeo, bool bAutoClear)
        {
            try
            {
                if (pMapCtrl == null) return;
                if (pGeo == null) return;
                if (pGeo.IsEmpty) return;
                IGeometry pPolygon = null;
                if (pGeo is IEnvelope)
                {
                    object Miss = Type.Missing;
                    pPolygon = new PolygonClass();
                    IGeometryCollection pGeoColl = pPolygon as IGeometryCollection;
                    pGeoColl.AddGeometry(pGeo, ref Miss, ref Miss);
                }
                else if (pGeo is IPolygon)
                {
                    (pGeo as ITopologicalOperator).Simplify();
                    pPolygon = pGeo;
                }
                else
                {
                    MessageBox.Show("����ʵ�����Ͳ�ƥ��", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                //��ȡ͸��Ҫ�ز�
                IFeatureLayer pFlyr = null;
                for (int i = 0; i < pMapCtrl.LayerCount; i++)
                {
                    if (pMapCtrl.get_Layer(i).Name == "TransTemp")
                    {
                        pFlyr = pMapCtrl.get_Layer(i) as IFeatureLayer;
                        break;
                    }
                }
                //͸����ʱ�㲻������Ҫ����
                if (pFlyr == null)
                {
                    pFlyr = AddFeatureLayerByMemoryWS(pMapCtrl, pMapCtrl.SpatialReference);
                    if (pFlyr == null)
                    {
                        MessageBox.Show("����͸����ʱͼ�㷢���쳣", "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                IFeatureClass pFC = pFlyr.FeatureClass;
                if (bAutoClear)
                {
                    if (pFC.FeatureCount(null) > 0)
                    {
                        IFeatureCursor pFCursor = pFC.Search(null, false);
                        if (pFCursor != null)
                        {

                            IFeature pFeature = pFCursor.NextFeature();
                            if (pFeature != null)
                            {
                                while (pFeature != null)
                                {
                                    pFeature.Delete();
                                    pFeature = pFCursor.NextFeature();
                                }
                            }
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pFCursor);
                        }
                    }
                }
                //����Ҫ��
                IFeature pNFeature = pFC.CreateFeature();
                pNFeature.Shape = pPolygon;
                pNFeature.set_Value(pFC.FindField("Code"), "1");
                pNFeature.Store();
                pMapCtrl.Refresh(esriViewDrawPhase.esriViewGeography, pFlyr, pFlyr.AreaOfInterest);
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void AddPointByStore(AxMapControl axMapControl1, IFeatureLayer l, double x, double y)
        {
            ESRI.ArcGIS.Geometry.esriGeometryType featype = l.FeatureClass.ShapeType;
            if (featype == esriGeometryType.esriGeometryPoint)//�жϲ��Ƿ�Ϊ���
            {
                //�õ�Ҫ��ӵ����ͼ�� 
                //IFeatureLayer l = MapCtr.Map.get_Layer(0) as IFeatureLayer;
                //����һ��������,��Ҫ�༭��ͼ��ת��Ϊ����ĵ����� 
                IFeatureClass fc = l.FeatureClass;
                //�ȶ���һ���༭�Ĺ����ռ�,Ȼ���ת��Ϊ���ݼ�,���ת��Ϊ�༭�����ռ�, 
                IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                IFeature f;
                IPoint p;
                //��ʼ������� 
                w.StartEditing(false);
                //��ʼ�༭ 
                w.StartEditOperation();

                //����һ������ 
                f = fc.CreateFeature();
                p = new PointClass();
                //���õ������ 
                p.PutCoords(x, y);



                ////ȷ��ͼ������ 
                f.Shape = p;
                //������� 
                f.Store();

                //�����༭ 
                w.StopEditOperation();
                //����������� 
                w.StopEditing(true);
                AddPoint(axMapControl1, x, y);
                //UniqueValueRenderFlyr(axMapControl1, l);
                //axMapControl1.Refresh();
            }
        }

        /// <summary>
        /// ��ӵ�
        /// </summary>
        public void AddPoint(AxMapControl axMapControl1, double x, double y)
        {
            try
            {
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


                string AppPath = Application.StartupPath;
                IPoint Pt = new PointClass();
                Pt.PutCoords(x, y);

                IMarkerElement ipMarkerElement = new MarkerElementClass();
                ipMarkerElement.Symbol = pSimpleMarkerSymbol as IMarkerSymbol;
                IElement ipElement = ipMarkerElement as IElement;
                ipElement.Geometry = Pt as IGeometry;
                axMapControl1.ActiveView.GraphicsContainer.AddElement(ipElement, 0);
                axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, ipElement, null);
            }
            catch
            {

            }
        }

        public void AddPointByWrite(IFeatureLayer l, double x, double y)
        {
            ESRI.ArcGIS.Geometry.esriGeometryType featype = l.FeatureClass.ShapeType;
            if (featype == esriGeometryType.esriGeometryPoint)//�жϲ��Ƿ�Ϊ���
            {
                // IFeatureLayer l = MapCtr.Map.get_Layer(0) as IFeatureLayer;
                IFeatureClass fc = l.FeatureClass;
                IFeatureClassWrite fr = fc as IFeatureClassWrite;
                IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                IFeature f;
                IPoint p;

                w.StartEditing(true);
                w.StartEditOperation();

                f = fc.CreateFeature();
                p = new PointClass();
                p.PutCoords(x, y);
                f.Shape = p;
                fr.WriteFeature(f);

                w.StopEditOperation();
                w.StopEditing(true);
            }
        }

        public void AddPointByBuffer(IFeatureLayer l, double x, double y)
        {
            ESRI.ArcGIS.Geometry.esriGeometryType featype = l.FeatureClass.ShapeType;
            if (featype == esriGeometryType.esriGeometryPoint)//�жϲ��Ƿ�Ϊ���
            {
                //IFeatureLayer l = MapCtr.Map.get_Layer(0) as IFeatureLayer;
                IFeatureClass fc = l.FeatureClass;
                IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                w.StartEditing(true);
                w.StartEditOperation();
                IPoint p;
                IFeatureBuffer f;
                IFeatureCursor cur = fc.Insert(true);

                f = fc.CreateFeatureBuffer();
                p = new PointClass();
                p.PutCoords(x, y);
                f.Shape = p;
                cur.InsertFeature(f);
                w.StopEditOperation();
                w.StopEditing(true);
            }
        }

        public void AddLineByWrite(AxMapControl axMapControl1, IFeatureLayer l, double x, double y)
        {
            ESRI.ArcGIS.Geometry.esriGeometryType featype = l.FeatureClass.ShapeType;
            if (featype == esriGeometryType.esriGeometryPolyline)//�жϲ��Ƿ�Ϊ�߲�
            {
                //IFeatureLayer l = MapCtr.Map.get_Layer(0) as IFeatureLayer;
                IFeatureClass fc = l.FeatureClass;
                IFeatureClassWrite fr = fc as IFeatureClassWrite;
                IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                IFeature f;
                //��ѡ���������� 
                object Missing = Type.Missing;
                IPoint p = new PointClass();
                w.StartEditing(true);
                w.StartEditOperation();

                f = fc.CreateFeature();
                //����һ�������߶��� 
                IRgbColor color = new RgbColor();
                // ������ɫ����
                color.Red = 255;
                color.Transparency = 255;

                //ISimpleLineSymbol PlyLine = new SimpleLineSymbolClass();
                //PlyLine.Color = color;
                //PlyLine.Style = esriSimpleLineStyle.esriSLSInsideFrame;
                //PlyLine.Width = 1;

                IGeometry iGeom = axMapControl1.TrackLine();

                AddLine(axMapControl1, iGeom);

                f.Shape = iGeom;
                fr.WriteFeature(f);
                w.StopEditOperation();
                w.StopEditing(true);
            }
        }

        /// <summary>
        /// �����
        /// </summary>
        public void AddLine(AxMapControl axMapControl1, IGeometry GeomLine)
        {
            ISimpleLineSymbol ipLine = new SimpleLineSymbolClass();
            ipLine.Width = 1;
            IRgbColor ipColor = new RgbColorClass();
            ipColor.RGB = 0x0000ff;
            ipLine.Color = ipColor;
            ipLine.Style = esriSimpleLineStyle.esriSLSDashDotDot;
            ILineElement ipLineElem = new LineElementClass();
            ipLineElem.Symbol = ipLine;
            IElement ipElement = ipLineElem as IElement;
            ipElement.Geometry = GeomLine;
            axMapControl1.ActiveView.GraphicsContainer.AddElement(ipElement, 0);
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, ipElement, null);
        }

        public void AddRegion(AxMapControl axMapControl1, IGeometry GeomArea)
        {
            IPolygonElement ipPolygonElem = new PolygonElementClass();
            ILineSymbol ipLine = new SimpleLineSymbolClass();
            ipLine.Width = 1;
            IRgbColor ipColor = new RgbColorClass();
            ipColor.RGB = 0x0000ff;
            ipLine.Color = ipColor;
            ISimpleFillSymbol ipFillSym = new SimpleFillSymbolClass();
            IRgbColor ipColorFill = new RgbColorClass();
            ipColorFill.RGB = 0xff0000;
            ipFillSym.Outline = ipLine;
            ipFillSym.Color = ipColorFill;
            ipFillSym.Style = esriSimpleFillStyle.esriSFSCross;
            IFillShapeElement ipShape = ipPolygonElem as IFillShapeElement;
            ipShape.Symbol = ipFillSym;
            IElement ipElement = ipPolygonElem as IElement;
            ipElement.Geometry = GeomArea;
            axMapControl1.ActiveView.GraphicsContainer.AddElement(ipElement, 0);
            axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, ipElement, null);
        }

        public void AddPolygonByWrite(AxMapControl axMapControl1, IFeatureLayer l, double x, double y)
        {
            ESRI.ArcGIS.Geometry.esriGeometryType featype = l.FeatureClass.ShapeType;
            if (featype == esriGeometryType.esriGeometryPolygon)//�жϲ��Ƿ�Ϊ�߲�
            {
                //IFeatureLayer l = MapCtr.Map.get_Layer(0) as IFeatureLayer;
                IFeatureClass fc = l.FeatureClass;
                IFeatureClassWrite fr = fc as IFeatureClassWrite;
                IWorkspaceEdit w = (fc as IDataset).Workspace as IWorkspaceEdit;
                IFeature f;
                //��ѡ���������� 
                object Missing = Type.Missing;
                IPoint p = new PointClass();
                w.StartEditing(true);
                w.StartEditOperation();

                f = fc.CreateFeature();
                //����һ�������߶��� 
                IRgbColor color = new RgbColor();
                // ������ɫ����
                color.Red = 255;
                color.Transparency = 255;
                IGeometry iGeom = axMapControl1.TrackPolygon();
                AddRegion(axMapControl1, iGeom);
                f.Shape = iGeom;
                fr.WriteFeature(f);
                w.StopEditOperation();
                w.StopEditing(true);


            }
        }

        ///<summary>
        ///�����ʱԪ�ص���ͼ������
        ///</summary>
        ///<param name="pMapCtrl">��ͼ�ؼ�</param>
        ///<param name="pEle">����Ԫ��</param>
        ///<param name="pEleColl">Ԫ�ؼ���</param>
        public void AddTempElement(AxMapControl pMapCtrl, IElement pEle, IElementCollection pEleColl)
        {
            try
            {
                IMap pMap = pMapCtrl.Map;
                IGraphicsContainer pGCs = pMap as IGraphicsContainer;
                if (pEle != null)
                    pGCs.AddElement(pEle, 0);

                if (pEleColl != null)
                    if (pEleColl.Count > 0)
                        pGCs.AddElements(pEleColl, 0);
                IActiveView pAV = (IActiveView)pMap;
                //��Ҫˢ�²��ܼ�ʱ��ʾ
                pAV.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, pAV.Extent);
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        ///<summary>
        ///��ȡ���ſ��з���
        ///</summary>
        ///<param name="sServerStylePath">���ſ�ȫ·������</param>
        ///<param name="sGalleryClassName">GalleryClass����</param>
        ///<param name="symbolName">��������</param>
        ///<returns>����</returns>
        private ISymbol GetSymbol(string sServerStylePath, string sGalleryClassName, string symbolName)
        {
            try
            {
                //ServerStyleGallery����
                IStyleGallery pStyleGaller = new ServerStyleGalleryClass();
                IStyleGalleryStorage pStyleGalleryStorage = pStyleGaller as IStyleGalleryStorage;

                IEnumStyleGalleryItem pEnumSyleGalleryItem = null;
                IStyleGalleryItem pStyleGallerItem = null;
                IStyleGalleryClass pStyleGalleryClass = null;
                //ʹ��IStyleGalleryStorage�ӿڵ�AddFile��������ServerStyle�ļ�
                pStyleGalleryStorage.AddFile(sServerStylePath);
                //����ServerGallery�е�Class
                for (int i = 0; i < pStyleGaller.ClassCount; i++)
                {
                    pStyleGalleryClass = pStyleGaller.get_Class(i);
                    if (pStyleGalleryClass.Name != sGalleryClassName)
                        continue;
                    //��ȡEnumStyleGalleryItem����
                    pEnumSyleGalleryItem = pStyleGaller.get_Items(sGalleryClassName, sServerStylePath, "");
                    pEnumSyleGalleryItem.Reset();
                    //����pEnumSyleGalleryItem
                    pStyleGallerItem = pEnumSyleGalleryItem.Next();
                    while (pStyleGallerItem != null)
                    {
                        if (pStyleGallerItem.Name == symbolName)
                        {
                            //��ȡ����
                            ISymbol pSymbol = pStyleGallerItem.Item as ISymbol;
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumSyleGalleryItem);
                            System.Runtime.InteropServices.Marshal.ReleaseComObject(pStyleGalleryClass);
                            return pSymbol;
                        }
                        pStyleGallerItem = pEnumSyleGalleryItem.Next();
                    }
                }
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pEnumSyleGalleryItem);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(pStyleGalleryClass);
                return null;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return null;
            }
        }

        ///<summary>
        ///����Ҫ��ͼ��Ψһֵ���Ż�
        ///</summary>
        ///<param name="pFeatureLayer"></param>
        private void UniqueValueRenderFlyr(AxMapControl axMapControl1, IFeatureLayer pFeatureLayer)
        {
            try
            {
                //����UniqueValueRendererClass����
                IUniqueValueRenderer pUVRender = new UniqueValueRendererClass();
                List<string> pFieldValues = new List<string>();
                pFieldValues.Add("Hospital 2");
                pFieldValues.Add("School 1");
                pFieldValues.Add("Airport");
                for (int i = 0; i < pFieldValues.Count; i++)
                {
                    ISymbol pSymbol = new SimpleMarkerSymbolClass();
                    pSymbol = GetSymbol(@"D:\Program Files\ArcGIS\Styles\ESRI.ServerStyle", "Marker Symbols", pFieldValues[i]);
                    //���Ψһֵ���Ż��ֶ�ֵ�����Ӧ�ķ���
                    pUVRender.AddValue(pFieldValues[i], pFieldValues[i], pSymbol);
                }
                //����Ψһֵ���Ż����ֶθ������ֶ���
                pUVRender.FieldCount = 1;
                pUVRender.set_Field(0, "���");
                IGeoFeatureLayer pGFeatureLyr = pFeatureLayer as IGeoFeatureLayer;
                //����IGeofeatureLayer��Renderer����
                pGFeatureLyr.Renderer = pUVRender as IFeatureRenderer;
            }
            catch (Exception Err)
            {
                MessageBox.Show(Err.Message, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// �����ı���ʾ��
        /// </summary>
        /// <param name="x">��ʾ���ʶ��λ��X����</param>
        /// <param name="y">��ʾ���ʶ��λ��Y����</param>
        public void CreateTextElment(AxMapControl axMapControl1, double x, double y, string strText)
        {
            IPoint pPoint = new PointClass();
            IMap pMap = axMapControl1.Map;
            IActiveView pActiveView = pMap as IActiveView;
            IGraphicsContainer pGraphicsContainer;
            IElement pElement = new MarkerElementClass();
            IElement pTElement = new TextElementClass();
            pGraphicsContainer = (IGraphicsContainer)pActiveView;
            IFormattedTextSymbol pTextSymbol = new TextSymbolClass();
            IBalloonCallout pBalloonCallout = CreateBalloonCallout(x, y);
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 150;
            pColor.Green = 0;
            pColor.Blue = 0;
            pTextSymbol.Color = pColor;
            ITextBackground pTextBackground;
            pTextBackground = (ITextBackground)pBalloonCallout;
            pTextSymbol.Background = pTextBackground;
            ((ITextElement)pTElement).Symbol = pTextSymbol;
            ((ITextElement)pTElement).Text = strText;

            IPoint p = new PointClass();
            //���õ������ 
            p.PutCoords(x, y);
            IElementProperties ipElemProp;
            IMarkerElement ipMarkerElement = new MarkerElementClass();
            IPictureMarkerSymbol ipPicMarker = new PictureMarkerSymbolClass();
            ipPicMarker.CreateMarkerSymbolFromFile(esriIPictureType.esriIPictureBitmap, "D:\\pro\\ArcGisView\\ArcGisView\\1.bmp");
            ipPicMarker.Size = 24;
            IRgbColor ipRGBTrans = new RgbColorClass();
            ipRGBTrans.RGB = 0xffffff;
            ipPicMarker.BitmapTransparencyColor = ipRGBTrans as IColor;
            ipMarkerElement.Symbol = ipPicMarker as IMarkerSymbol;
            IElement ipElement = ipMarkerElement as IElement;
            ipElement.Geometry = p as IGeometry;
            axMapControl1.ActiveView.GraphicsContainer.AddElement(ipElement, 0);

            pPoint.X = x + 42;
            pPoint.Y = y + 42;
            pTElement.Geometry = pPoint;
            pGraphicsContainer.AddElement(pTElement, 1);
            pActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        }
        /// <summary>
        /// ����balloon����ʾ��
        /// </summary>
        /// <param name="x">��ʾ������λ��X����</param>
        /// <param name="y">��ʾ������λ��Y����</param>
        /// <returns></returns>
        public IBalloonCallout CreateBalloonCallout(double x, double y)
        {
            IRgbColor pRgbClr = new RgbColorClass();
            pRgbClr.Red = 255;
            pRgbClr.Blue = 255;
            pRgbClr.Green = 255;
            ISimpleFillSymbol pSmplFill = new SimpleFillSymbolClass();
            pSmplFill.Color = pRgbClr;
            pSmplFill.Style = esriSimpleFillStyle.esriSFSSolid;
            IBalloonCallout pBllnCallout = new BalloonCalloutClass();
            pBllnCallout.Style = esriBalloonCalloutStyle.esriBCSRoundedRectangle;
            pBllnCallout.Symbol = pSmplFill;
            pBllnCallout.LeaderTolerance = 1;
            IPoint pPoint = new ESRI.ArcGIS.Geometry.PointClass();
            pPoint.X = x;
            pPoint.Y = y;
            pBllnCallout.AnchorPoint = pPoint;
            return pBllnCallout;
        }

        /// <summary>
        /// ɾ��ͼԪ
        /// </summary>
        public void DelFeature(AxMapControl axMapControl1, IFeatureLayer feaLayer, string FeaKey)
        {
            IDataset ipDataset = feaLayer.FeatureClass as IDataset;
            IWorkspaceEdit ipwsEdit = ipDataset.Workspace as IWorkspaceEdit;
            IQueryFilter ipQueryFilter = new QueryFilterClass();
            ipQueryFilter.WhereClause = string.Format("{0}={1}", feaLayer.FeatureClass.OIDFieldName, FeaKey);
            IFeatureCursor ipFeatureCursor = feaLayer.FeatureClass.Search(ipQueryFilter, false);
            if (ipFeatureCursor != null)
            {
                IFeature ipFt = ipFeatureCursor.NextFeature();
                if (ipFt != null)
                {
                    //if (!ipwsEdit.IsBeingEdited())
                    //{
                    //    ipwsEdit.StartEditing(false);
                    //}

                    //ipwsEdit.StopEditing(true);
                    //DelFeatureByKeyName("MARKER_PIN_" + SelEditLayer + ipFt.OID.ToString());
                    axMapControl1.ActiveView.Refresh();

                    // this._mapObject.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, GetLayerByName(SelEditLayer), null);
                }
            }
        }

        /// <summary>
        /// ����ͼԪ����ֵ
        /// </summary>
        /// <param name="FieldTable">ͼԪ��Ϣ�б�</param>
        /// <param name="feaLayer">ѡ���ͼ����</param>
        public bool UpdateFeatureValue(Dictionary<string, string> FieldTable, IFeatureLayer feaLayer)
        {
            string FtOId = "";

            FtOId = FieldTable[feaLayer.FeatureClass.OIDFieldName.ToLower()];

            IQueryFilter ipQueryFilter = new QueryFilterClass();
            ipQueryFilter.WhereClause = string.Format("{0}={1}", feaLayer.FeatureClass.OIDFieldName, FtOId);
            IFeatureCursor ipFeatureCursor = feaLayer.FeatureClass.Update(ipQueryFilter, false);
            if (ipFeatureCursor != null)
            {
                IFeature ipFt = ipFeatureCursor.NextFeature();
                if (ipFt != null)
                {
                    for (int i = 0; i < ipFt.Fields.FieldCount; i++)
                    {
                        string strKey = ipFt.Fields.get_Field(i).Name.ToLower();
                        if (FieldTable.ContainsKey(strKey))
                        {
                            switch (ipFt.Fields.get_Field(i).Type)
                            {
                                case esriFieldType.esriFieldTypeString:
                                    {
                                        string PropeValue = FieldTable[strKey];
                                        if (ipFt.Fields.get_Field(i).Length >= PropeValue.Length)
                                        {
                                            if (ipFt.Fields.get_Field(i).CheckValue(PropeValue))
                                            {
                                                ipFt.set_Value(i, PropeValue);
                                            }
                                            else
                                            {
                                                MessageBox.Show("ȡֵ����ȷ",
                                                    "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                                return false;
                                            }
                                        }
                                        else
                                        {

                                            MessageBox.Show("���ܳ���" + ipFt.Fields.get_Field(i).Length + "�ֽ�",
                                                "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                            return false;

                                        }
                                    }
                                    break;
                                case esriFieldType.esriFieldTypeSmallInteger:
                                    {
                                        if (FieldTable[strKey].Trim().Equals(""))
                                            break;
                                        ushort PropeValue = Convert.ToUInt16(FieldTable[strKey]);
                                        //if (ipFt.Fields.get_Field(i).Length >= PropeValue.ToString().Length)
                                        //{
                                            if (ipFt.Fields.get_Field(i).CheckValue(PropeValue))
                                            {
                                                ipFt.set_Value(i, PropeValue);
                                            }
                                            else
                                            {
                                                MessageBox.Show("ȡֵ����ȷ",
                                                    "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                                return false;
                                            }
                                        //}
                                        //else
                                        //{

                                        //    MessageBox.Show("���ܳ���" + ipFt.Fields.get_Field(i).Length + "λ����",
                                        //        "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                        //    return false;

                                        //}
                                    }
                                    break;
                                case esriFieldType.esriFieldTypeInteger:
                                    {
                                        if (FieldTable[strKey].Trim().Equals(""))
                                            break;
                                        int PropeValue = Convert.ToInt32(FieldTable[strKey]);
                                        if (ipFt.Fields.get_Field(i).Length >= PropeValue.ToString().Length)
                                        {
                                            if (ipFt.Fields.get_Field(i).CheckValue(PropeValue))
                                            {
                                                ipFt.set_Value(i, PropeValue);
                                            }
                                            else
                                            {
                                                MessageBox.Show("ȡֵ����ȷ",
                                                    "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                                return false;
                                            }
                                        }
                                        else
                                        {


                                            MessageBox.Show("���ܳ���" + ipFt.Fields.get_Field(i).Length + "λ����",
                                                "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                            return false;

                                        }
                                    }
                                    break;
                                case esriFieldType.esriFieldTypeDouble:
                                    {
                                        if (FieldTable[strKey].Trim().Equals(""))
                                            break;
                                        double PropeValue = Convert.ToDouble(FieldTable[strKey]);
                                        if (ipFt.Fields.get_Field(i).Length >= PropeValue.ToString().Replace(".", "").Length)
                                        {
                                            if (ipFt.Fields.get_Field(i).CheckValue(PropeValue))
                                            {
                                                ipFt.set_Value(i, PropeValue);
                                            }
                                            else
                                            {
                                                MessageBox.Show("ȡֵ����ȷ",
                                                    "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                                return false;
                                            }
                                        }
                                        else
                                        {

                                            {
                                                MessageBox.Show("���ܳ���" + ipFt.Fields.get_Field(i).Length + "λ����",
                                                    "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                                return false;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    ipFeatureCursor.UpdateFeature(ipFt);
                }
                else
                {
                    string msg = "";
                    msg = "������ɾ����";
                    MessageBox.Show(msg, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                return true;
            }
            else
            {
                string msg = "";

                msg = "������ɾ����";
                MessageBox.Show(msg, "��ʾ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }


        /// <summary>
        /// �������ƻ�ȡ��ͼͼ��ӿ�
        /// </summary>
        /// <param name="layer">ͼ����</param>
        public ILayer GetLayerByName(AxMapControl axMapControl1, string layer)
        {
            for (int i = 0; i < axMapControl1.LayerCount; i++)
            {
                if (axMapControl1.get_Layer(i).Name.Equals(layer))
                {
                    return axMapControl1.get_Layer(i);
                }
            }
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.SystemUI;
using System.Windows.Forms;

namespace ArcGisView
{

   

    public sealed class FixeZoomIn : BaseCommand
    {
        private IMapControl3 m_mapControl;
        public FixeZoomIn()
        {
            base.m_caption = "��ͼ�Ŵ�";
        }

        public override void OnClick()
        {
            ICommand command = new ESRI.ArcGIS.Controls.ControlsMapZoomInFixedCommandClass();//��ͼ�Ŵ�
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }

    public sealed class FixeZoomOut : BaseCommand
    {
        private IMapControl3 m_mapControl;
        public FixeZoomOut()
        {
            base.m_caption = "��ͼ��С";
        }

        public override void OnClick()
        {
            ICommand command = new ESRI.ArcGIS.Controls.ControlsMapZoomOutFixedCommandClass();//��ͼ�Ŵ�
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }

    public sealed class FullExtent : BaseCommand
    {
        private IMapControl3 m_mapControl;
        public FullExtent()
        {
            base.m_caption = "��ʾȫ��";
        }

        public override void OnClick()
        {
            ICommand command = new ESRI.ArcGIS.Controls.ControlsMapFullExtentCommandClass();//��ʾ��ͼ��ȫ��
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }

    public sealed class PanClass : BaseCommand
    {
        private IMapControl3 m_mapControl;
        public PanClass()
        {
            base.m_caption = "��ͼƽ��";
        }

        public override void OnClick()
        {
            ICommand command = new ESRI.ArcGIS.Controls.ControlsMapPanToolClass();//ƽ��
            command.OnCreate(m_mapControl.Object);
            m_mapControl.CurrentTool = command as ITool;//��ť���µ�״̬
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }

    public sealed class Measuredis : BaseCommand
    {
        private IMapControl3 m_mapControl;
        public Measuredis()
        {
            base.m_caption = "��ͼ����";
        }

        public override void OnClick()
        {
            ITool pTool = new MeasuredisTool();
            ICommand command = pTool as ICommand;//����
            command.OnCreate(m_mapControl.Object);
            m_mapControl.CurrentTool = command as ITool;
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }

    public sealed class PrintPage : BaseCommand
    {
        private IMapControl3 m_mapControl;
        private AxMapControl axMapControl1;
        private string sPath;
        public PrintPage(AxMapControl MapCtrl)
        {
            base.m_caption = "��ͼ��ӡ";
            axMapControl1 = MapCtrl;
        }

        public override void OnClick()
        {
            PrintPageLayout frm = null;
            sPath = axMapControl1.DocumentFilename;
            if (frm == null || frm.IsDisposed)
                frm = new PrintPageLayout(sPath);

            frm.Show();
            frm.TopMost = true;
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }

    public sealed class ExportImg : BaseCommand
    {
        private IMapControl3 m_mapControl;
        private AxMapControl axMapControl1;
        private Form form1;
        public ExportImg(AxMapControl MapCtrl, Form hwin)
        {
            base.m_caption = "����ͼƬ";
            axMapControl1 = MapCtrl;
            form1 = hwin;
        }

        public override void OnClick()
        {
            ArcGisPublic agp = new ArcGisPublic();
            agp.export(axMapControl1, form1);
        }

        public override void OnCreate(object hook)
        {
            m_mapControl = (IMapControl3)hook;
        }
    }
}

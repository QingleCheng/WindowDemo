using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;

namespace ArcGisView
{
    public partial class SearchViewInfo : Form
    {
        IFeature pFeature;
        public SearchViewInfo(IFeature ft)
        {
            InitializeComponent();
            pFeature = ft;
        }

        private void SearchViewInfo_Load(object sender, EventArgs e)
        {
            ListView1.Columns.Add("�ֶ�", 80, HorizontalAlignment.Center);
            ListView1.Columns.Add("��ֵ", 130, HorizontalAlignment.Left);
            //������һ��Ҫ�ص��ֶ����ڸ���ͷ��ֵ���ֶε����ƣ�
            for (int m = 0; m < pFeature.Fields.FieldCount; m++)
            {
                ListViewItem lv = ListView1.Items.Add(pFeature.Fields.get_Field(m).AliasName);
                lv.SubItems.Add(pFeature.get_Value(m).ToString());
            }
            
        }


    }
}
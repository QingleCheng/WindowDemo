using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Data;
using System.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Telerik.Web.UI;

using ServiceDirect.Schedule.DAL;
using ServiceDirect.Schedule.Billing.BLL;

public partial class EmailSettingForm : System.Web.UI.Page 
{
    #region ����
    /// <summary>
    /// ����
    /// </summary>
    private string KeyId
    {
        get
        {
            object o = ViewState["KeyId"];
            return o == null ? string.Empty : o.ToString();
        }
        set { ViewState["KeyId"] = value; }
    }
    #endregion

    ScheduleTasksBLL BLL_ScheduleTasks;//��������Ŀ�����

    tblScheduler SchedulerObj;

    string successFlag;//���صĲ�����Ϣ

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            //��ʼ��״̬
            pnlMessage.Visible = false;
            pnlRight.Visible = false;
            pnlError.Visible = false;

            GetDataReturnFormOtherPageFromCookie();
            if (Request.QueryString["KeyGuid"] != null && Request.QueryString["KeyGuid"] != string.Empty)
            {
                this.KeyId = Request.QueryString["KeyGuid"].ToString();
                GetData();
                FindValue();
            }
        }
    }

    #region ����ID��ѯ����
    protected void GetData()
    {
        if (this.KeyId != string.Empty)
        {
            BLL_ScheduleTasks = new ScheduleTasksBLL();
            SchedulerObj = BLL_ScheduleTasks.FindSchedulerById(this.KeyId);
        }
    }
    #endregion

    #region ���ؼ���ֵ
    protected void FindValue()
    {
        if (this.KeyId != string.Empty)
        {
            if (SchedulerObj != null)
            {
                rtxtTo.Text = SchedulerObj.EmailTo;   
            }
        }
    }
    #endregion

    #region ȡ����������ѡ��ҳ��
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        if (this.KeyId == string.Empty)
        {
            Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx");
        }else
        {
            Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx?KeyGuid=" + this.KeyId);
        }
    }
    #endregion

    #region ȷ�ϱ��浽���ݿ�
    protected void btnOK_Click(object sender, EventArgs e)
    {
        Guid KeyIdGuid;
        if (this.KeyId == string.Empty)
        {
            //�����ݱ��浽�ͻ���cookie
            HttpCookie myCookie = new HttpCookie("EmailSetting");

            myCookie.Values.Add("EmailTo", rtxtTo.Text.Trim());
            myCookie.Expires = System.DateTime.Now.AddDays(1);
            Response.AppendCookie(myCookie);

            Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx");
        }
        else
        {
            KeyIdGuid = new Guid(this.KeyId);//ת����Guid����
            SchedulerObj = new tblScheduler();
            SchedulerObj.EmailTo = rtxtTo.Text.Trim();
            SchedulerObj.ScheduleID = KeyIdGuid;
            BLL_ScheduleTasks = new ScheduleTasksBLL();
            successFlag = BLL_ScheduleTasks.UpdateInEmailForm(SchedulerObj);
            if (!successFlag.Equals("InsertError"))
            {
                Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx?KeyGuid=" + this.KeyId);
            }
            else
            {
                //������ʾ��Ϣ
                MessageBox(false, false, true,
                        GetGlobalResourceObject("WebResource", "EmailSettingForm_UpdateEmailSetting_ErrorMessage").ToString());
            }
        }
    }
    #endregion

    #region ��cookieȡ����,��ȡcookie����ĵ�ǰҳ����
    protected void GetDataReturnFormOtherPageFromCookie()
    {
        HttpCookie TeskDetailData = Request.Cookies.Get("EmailSetting");

        if (TeskDetailData!=null)
        {
            rtxtTo.Text = TeskDetailData.Values["EmailTo"];
        }
    }
    #endregion

    #region ��ʾ��ʾ��Ϣ
    protected void MessageBox(Boolean correct, Boolean Warning, Boolean incorrect, string message)
    {
        pnlError.Visible = incorrect;//������ʾ��Ϣ
        pnlMessage.Visible = Warning;//����
        pnlRight.Visible = correct;//��ȷ

        if (incorrect == true)
        {
            lnkError.Text = message;//��ʾ������Ϣ
        }

        if (correct == true)
        {
            lnkRight.Text = message;//��ʾ��ȷ��Ϣ
        }

        if (Warning == true)
        {
            lnkMessage.Text = message;//��ʾ��ʾ��Ϣ
        }
    }
    #endregion
}

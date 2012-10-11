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
using System.Data.Objects;
using System.Linq;

/**
*Description:���е�Cycle���ݼ����Ǹ���Cycle�ֶν�������UTPҲ�����,SD�������Cycle
*/

public partial class BillingForm : System.Web.UI.Page 
{
    #region ����
    /// <summary>
    /// ����
    /// </summary>
    private string OrderBy
    {
        get
        {
            object o = ViewState["OrderBy"];
            return o == null ? string.Empty : o.ToString();
        }
        set { ViewState["OrderBy"] = value; }
    }

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

    vUTPCustomerBLL BLL_vUTPCustomer;
    ScheduleTasksBLL BLL_ScheduleTasks;//��������Ŀ�����

    ObjectQuery<vUTPCompany> vUTPCompanyObjs;
    ObjectQuery<vUTPCycle> vUTPCycleObjs;
    ObjectQuery<vUTPCustomerState> vUTPCustomerStateObjs;

    tblScheduler SchedulerObj;//��������

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            pnlMessage.Visible = false;
            pnlRight.Visible = false;
            pnlError.Visible = false;
            
            AllComboBoxBind();//��ѯ���ݣ�������ComboBox������
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

    #region ������ҳ��ؼ���ֵ
    protected void FindValue()
    {
        vUTPCycle startingCycle;//��ʼ����
        vUTPCycle endingCycle;//��ֹ����

        Boolean startCycle=false;//�Ƿ�����ʼ����
        Boolean endCycle=false;//�Ƿ��ǽ�������
        RadComboBoxItem endingCycleComboBox;
        RadComboBoxItem startingCycleComboBox;

        string[] cycle;

        if (this.KeyId != string.Empty)
        {
            if (SchedulerObj.Company != null)
            {
                rctxtCompany.SelectedValue = SchedulerObj.Company.Trim();
            }
            if (SchedulerObj.Cycle!=null)
            {
                cycle = SchedulerObj.Cycle.Split(',');
                if (SchedulerObj.Cycle != null)
                {
                    cycle = SchedulerObj.Cycle.Split(',');
                    if (cycle!=null)
                    {
                        int i=0;
                        foreach (var item in cycle)
                        {
                            if(i == 0)
                            {
                                if (item.Equals("N/A"))
                                {
                                    //�����ʼ����
                                    rctxtStartingCycle.Items.Clear();

                                    startingCycleComboBox = new RadComboBoxItem("N/A", "N/A");
                                    rctxtStartingCycle.Items.Add(startingCycleComboBox);
                                }
                                else
                                {
                                    rctxtStartingCycle.SelectedValue = item.Trim();
                                }
                                i++;
                                //�Ƿ����ݼ���first
                                if (!item.Equals("N/A"))
                                {
                                    this.OrderBy = " it.Cycle asc";
                                    BLL_vUTPCustomer = new vUTPCustomerBLL();
                                    startingCycle = BLL_vUTPCustomer.FindFirstCycleByCompanyId(rctxtCompany.SelectedValue, this.OrderBy);
                                    if (item.Equals(startingCycle.Cycle.ToString()))
                                    {
                                        startCycle = true;
                                    }
                                }
                            }else if(i == 1)
                            {
                                if (item.Equals("N/A"))
                                {
                                    //�����ֹ����
                                    rctxtEndingCycle.Items.Clear();

                                    endingCycleComboBox = new RadComboBoxItem("N/A", "N/A");
                                    rctxtEndingCycle.Items.Add(endingCycleComboBox);
                                }
                                else
                                {
                                    rctxtEndingCycle.SelectedValue = item.Trim();
                                }
                                //�Ƿ����ݵ�Last
                                if (!item.Equals("N/A"))
                                {
                                    endingCycle = BLL_vUTPCustomer.FindLastCycleByCompanyId(rctxtCompany.SelectedValue, this.OrderBy);
                                    if (item.Equals(endingCycle.Cycle.ToString()))
                                    {
                                        endCycle = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (SchedulerObj.Copy!=null&&SchedulerObj.Copy.Equals("True"))
            {
                chkCopy.Checked=true;
            }
            if (SchedulerObj.Calc != null && SchedulerObj.Calc.Equals("True"))
            {
                chkCalc.Checked = true;
            }
            if (SchedulerObj.Status != null)
            {
                if (SchedulerObj.Status.Trim().Equals("All"))
                {
                    rctxtStatusCode.SelectedValue = "All";
                }else
                {
                    rctxtStatusCode.SelectedValue = SchedulerObj.Status.Trim();
                }
            }

            //�����ʼ����ͽ������������ݵ�frist��last����AllCycleΪtrue
            if (startCycle == true && endCycle==true)
            {
                chkAllCycles.Checked = true;
                //����ؼ��û�
                rctxtEndingCycle.Enabled = false;
                rctxtStartingCycle.Enabled = false;
            }
        }
    }
    #endregion

    #region ȷ�ϱ��浽Cookie
    protected void rbtnOK_Click(object sender, EventArgs e)
    {
        System.Guid KeyIdGuid;
        string successFlag;//������ݿ�����Ƿ�ɹ�

        int startCycleIndex=0;
        int endCycleIndex = 0;
        startCycleIndex = rctxtStartingCycle.SelectedIndex;
        endCycleIndex = rctxtStartingCycle.FindItemIndexByValue(rctxtEndingCycle.SelectedValue);

        if (rctxtCompany.SelectedValue != "N/A" && rctxtStartingCycle.SelectedValue != "N/A" && rctxtEndingCycle.SelectedValue != "N/A")
        {
            if (startCycleIndex < endCycleIndex || startCycleIndex == endCycleIndex)
            {
                if (this.KeyId == string.Empty)//����
                {
                    //�����ݱ��浽�ͻ���cookie
                    HttpCookie myCookie = new HttpCookie("Billing");

                    if (rctxtCompany.SelectedItem != null)
                    {
                        myCookie.Values.Add("Company", rctxtCompany.SelectedItem.Value);
                    }

                    if (rctxtStartingCycle.SelectedItem != null)
                    {
                        myCookie.Values.Add("StartingCycle", rctxtStartingCycle.SelectedItem.Value);
                    }

                    if (rctxtEndingCycle.SelectedItem != null)
                    {
                        myCookie.Values.Add("EndingCycle", rctxtEndingCycle.SelectedItem.Value);
                    }

                    if (rctxtStatusCode.SelectedItem != null)
                    {
                        myCookie.Values.Add("StatusCode", rctxtStatusCode.SelectedItem.Value);
                    }

                    myCookie.Values.Add("AllCycles", chkAllCycles.Checked.ToString());
                    myCookie.Values.Add("Copy", chkCopy.Checked.ToString());
                    myCookie.Values.Add("Calc", chkCalc.Checked.ToString());

                    myCookie.Expires = System.DateTime.Now.AddDays(1);
                    Response.AppendCookie(myCookie);
                    Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx");
                }
                else//����
                {
                    SchedulerObj = new tblScheduler();
                    HttpCookie UserData = Request.Cookies.Get("UserIdCookies");
                    if (UserData != null)
                    {
                        SchedulerObj.UTPUser = UserData.Value;
                    }
                    HttpCookie UserPasswordData = Request.Cookies.Get("UserPasswordCookies");
                    if (UserPasswordData != null)
                    {
                        SchedulerObj.UTPPwd = UserPasswordData.Value;
                    }
                    if (rctxtCompany.SelectedItem != null)
                    {
                        SchedulerObj.Company = rctxtCompany.SelectedItem.Value;
                    }

                    if (rctxtStartingCycle.SelectedItem != null && rctxtEndingCycle.SelectedItem != null)
                    {
                        SchedulerObj.Cycle = rctxtStartingCycle.SelectedItem.Value + "," + rctxtEndingCycle.SelectedItem.Value;
                    }

                    if (rctxtStatusCode.SelectedItem != null)
                    {
                        SchedulerObj.Status = rctxtStatusCode.SelectedItem.Value;
                    }

                    SchedulerObj.Copy = chkCopy.Checked.ToString();
                    SchedulerObj.Calc = chkCalc.Checked.ToString();

                    KeyIdGuid = new Guid(this.KeyId);//ת����Guid����

                    SchedulerObj.ScheduleID = KeyIdGuid;

                    BLL_ScheduleTasks = new ScheduleTasksBLL();
                    successFlag = BLL_ScheduleTasks.UpdateInBillingPage(SchedulerObj);
                    if (!successFlag.Equals("InsertError"))
                    {
                        //�����ݱ��浽�ͻ���cookie
                        HttpCookie myCookie = new HttpCookie("Billing");

                        myCookie.Values.Add("billingEdit", "billingEdit");

                        myCookie.Expires = System.DateTime.Now.AddDays(1);
                        Response.AppendCookie(myCookie);

                        Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx?KeyGuid=" + this.KeyId);
                    }
                    else
                    {
                        //������ʾ��Ϣ
                        MessageBox(false, false, true,
                                GetGlobalResourceObject("WebResource", "BillingForm_SaveMessage_ErrorMessage").ToString());
                    }
                }
            }
            else//EndCycle<startCycle
            {
                MessageBox(false, true, false,
                                GetGlobalResourceObject("WebResource", "BillingForm_SelectCycleMessage_Message").ToString());
            }
        }
        else//Cycle��������Ч
        {
            MessageBox(false, true, false,
                                GetGlobalResourceObject("WebResource", "BillingForm_ConpanyCycle_Message").ToString());
        }
    }
    #endregion

    #region ȡ����������ѡ��ҳ��
    protected void rbtnCancel_Click(object sender, EventArgs e)
    {
        if (this.KeyId!=null)
        {
            Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx?KeyGuid=" + this.KeyId);
        }
        else
        {
            Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx");
        }
    }
    #endregion

    #region ��Company��StatusCode��StateCode��ComboBox�ؼ���ֵ
    protected void AllComboBoxBind()
    {
        RadComboBoxItem Status;//Telerik��״̬�ؼ�

        vUTPCompany vUTPCompanyObj;

        this.OrderBy = " it.CompName desc ";
        string whereStr;

        whereStr = "";
        BLL_vUTPCustomer = new vUTPCustomerBLL();
        vUTPCompanyObjs = BLL_vUTPCustomer.GetUTPCompanys(whereStr, this.OrderBy);

        if (vUTPCompanyObjs.Count() > 0)
        {
            rctxtCompany.DataSource = vUTPCompanyObjs;
            rctxtCompany.DataValueField = "CompanyID";
            rctxtCompany.DataTextField = "Comp";
            rctxtCompany.DataBind();
        }
        vUTPCompanyObj = vUTPCompanyObjs.ToList().First();

        CycleComboBoxBind(vUTPCompanyObj.CompanyID);

        whereStr = " and it.Billable=true";
        this.OrderBy = " it.StatType desc ";
        vUTPCustomerStateObjs = BLL_vUTPCustomer.GetUTPCustomerStates(whereStr, this.OrderBy);

        if (vUTPCustomerStateObjs.Count() > 0)
        {
            try
            {
                rctxtStatusCode.DataSource = vUTPCustomerStateObjs;
                rctxtStatusCode.DataValueField = "CustomerStateID";
                rctxtStatusCode.DataTextField = "StatCode";
                rctxtStatusCode.DataBind();

                Status = new RadComboBoxItem("All", "All");
                rctxtStatusCode.Items.Add(Status);
                rctxtStatusCode.SelectedValue = "All";
            }
            catch (Exception)
            {
                rctxtStatusCode.Items.Clear();

                Status = new RadComboBoxItem("All", "All");
                rctxtStatusCode.Items.Add(Status);
                rctxtStatusCode.SelectedValue = "All";
            }
        }
    }
    #endregion

    #region ��StartCycle��EndCycle��ComboBox�ؼ���ֵ,����company��id
    protected void CycleComboBoxBind(string UTPCompanyId)
    {
        RadComboBoxItem startingCycle;//��ʼ����Telerik�ؼ�
        RadComboBoxItem endingCycle;//��������Telerik�ؼ�
        string whereStr;

        whereStr = " and it.CompanyID='" + UTPCompanyId + "'";
        this.OrderBy = " it.Cycle asc ";

        try
        {
            vUTPCycleObjs = BLL_vUTPCustomer.GetUTPCycles(whereStr, this.OrderBy);
        }
        catch (NullReferenceException)
        {
            BLL_vUTPCustomer = new vUTPCustomerBLL();
            vUTPCycleObjs = BLL_vUTPCustomer.GetUTPCycles(whereStr, this.OrderBy);
        }

        if (vUTPCycleObjs.Count() > 0)
        {
            try
            {
                //��ʼ����
                rctxtStartingCycle.DataSource = vUTPCycleObjs;
                rctxtStartingCycle.DataValueField = "Cycle";
                rctxtStartingCycle.DataTextField = "Cycle";
                rctxtStartingCycle.DataBind();

                //��ֹ����
                rctxtEndingCycle.DataSource = vUTPCycleObjs;
                rctxtEndingCycle.DataValueField = "Cycle";
                rctxtEndingCycle.DataTextField = "Cycle";
                rctxtEndingCycle.DataBind();
            }
            catch (Exception)
            {
                //�����ֹ����
                rctxtEndingCycle.Items.Clear();

                endingCycle = new RadComboBoxItem("N/A", "N/A");
                rctxtEndingCycle.Items.Add(endingCycle);

                //�����ʼ����
                rctxtStartingCycle.Items.Clear();

                startingCycle = new RadComboBoxItem("N/A", "N/A");
                rctxtStartingCycle.Items.Add(startingCycle);
            }
        }
        else
        {
            //�����ֹ����
            rctxtEndingCycle.Items.Clear();

            endingCycle = new RadComboBoxItem("N/A", "N/A");
            rctxtEndingCycle.Items.Add(endingCycle);

            //�����ʼ����
            rctxtStartingCycle.Items.Clear();

            startingCycle = new RadComboBoxItem("N/A", "N/A");
            rctxtStartingCycle.Items.Add(startingCycle);

            //������chkAllCycles�û�
            chkAllCycles.Checked = false;
            chkAllCycles.Enabled = false;
        }
    }
    #endregion

    #region ��cookieȡ����,��ȡcookie����ĵ�ǰҳ����
    protected void GetDataReturnFormOtherPageFromCookie()
    {
        HttpCookie BillingData = Request.Cookies.Get("Billing");

        if (BillingData != null)
        {
            if (BillingData.Values["Company"] != null)
            {
                rctxtCompany.SelectedValue = BillingData.Values["Company"].Trim();
            }
            if (BillingData.Values["StartingCycle"] != null)
            {
                rctxtStartingCycle.SelectedValue = BillingData.Values["StartingCycle"].Trim();
            }

            if (BillingData.Values["EndingCycle"] != null)
            {
                rctxtEndingCycle.SelectedValue = BillingData.Values["EndingCycle"].Trim();
            }

            if (BillingData.Values["StatusCode"] != null)
            {
                rctxtStatusCode.SelectedValue = BillingData.Values["StatusCode"].Trim();
            }
            if (BillingData.Values["AllCycles"] != null)
            {
                chkAllCycles.Checked = Convert.ToBoolean(BillingData.Values["AllCycles"]);
                if (chkAllCycles.Checked==true)
                {
                    //����ؼ��û�
                    rctxtEndingCycle.Enabled = false;
                    rctxtStartingCycle.Enabled = false;
                }
            }
            if (BillingData.Values["Copy"] != null)
            {
                chkCopy.Checked = Convert.ToBoolean(BillingData.Values["Copy"]);
            }
            if (BillingData.Values["Calc"] != null)
            {
                chkCalc.Checked = Convert.ToBoolean(BillingData.Values["Calc"]);
            }
        }
    }
    #endregion

    #region ����Company��ѡ��ֵ������Cycle���������ݼ�
    protected void rctxtCompany_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
    {
        if (rctxtCompany.SelectedValue != "N/A")
        {
            CycleComboBoxBind(rctxtCompany.SelectedValue);
        }
    }
    #endregion

    #region ѡ�����е�����ʱ��
    protected void chkAllCycles_CheckedChanged(object sender, EventArgs e)
    {
        this.OrderBy = " it.Cycle asc";

        vUTPCycle startingCycle;//��ʼ����
        vUTPCycle endingCycle;//��ֹ����
        
        string vUTPCompanyId;

        if (chkAllCycles.Checked==true)
        {
            vUTPCompanyId=rctxtCompany.SelectedValue;
            if (vUTPCompanyId != "N/A" && !rctxtEndingCycle.SelectedValue.Equals("N/A")
                                            && !rctxtStartingCycle.SelectedValue.Equals("N/A"))
            {
                BLL_vUTPCustomer = new vUTPCustomerBLL();
                startingCycle = BLL_vUTPCustomer.FindFirstCycleByCompanyId(vUTPCompanyId, this.OrderBy);
                endingCycle = BLL_vUTPCustomer.FindLastCycleByCompanyId(vUTPCompanyId, this.OrderBy);

                //���ռ丳ֵ
                rctxtStartingCycle.SelectedValue = startingCycle.Cycle.ToString().Trim();

                rctxtEndingCycle.SelectedValue = endingCycle.Cycle.ToString().Trim();

                //����ؼ��û�
                rctxtEndingCycle.Enabled = false;
                rctxtStartingCycle.Enabled = false;
            }
        }
        else
        {
            rctxtEndingCycle.Enabled = true;
            rctxtStartingCycle.Enabled = true;
        }
    }
    #endregion

    #region ѡ��ʼ�����Զ����ؽ����������ݼ�
    protected void rctxtStartingCycle_SelectedIndexChanged(object sender, RadComboBoxSelectedIndexChangedEventArgs e)
    {
        string whereStr;

        vUTPCycle endingCycle;//��ֹ����

        if (rctxtCompany.SelectedValue != "N/A")
        {
            whereStr = "  and it.CompanyID='" + rctxtCompany.SelectedValue + "'";
            this.OrderBy = " it.Cycle asc ";

            BLL_vUTPCustomer = new vUTPCustomerBLL();
            endingCycle = BLL_vUTPCustomer.FindLastCycleByCompanyId(rctxtCompany.SelectedValue, this.OrderBy);

            whereStr = " and it.CompanyID='" + rctxtCompany.SelectedValue + "' and it.Cycle >= " +
                                                    rctxtStartingCycle.SelectedValue + " and it.Cycle <= " + endingCycle.Cycle + "";

            vUTPCycleObjs = BLL_vUTPCustomer.GetUTPCycles(whereStr, this.OrderBy);

            if (vUTPCycleObjs.Count()>0)
            {
                rctxtEndingCycle.DataSource = vUTPCycleObjs;
                rctxtEndingCycle.DataValueField = "Cycle";
                rctxtEndingCycle.DataTextField = "Cycle";
                rctxtEndingCycle.DataBind();
            }
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

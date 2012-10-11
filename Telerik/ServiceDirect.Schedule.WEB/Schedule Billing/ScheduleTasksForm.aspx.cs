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

using ServiceDirect.Schedule.Billing;
using ServiceDirect.Schedule.Billing.BLL;
using ServiceDirect.Schedule.DAL;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using ServiceDirect.Schedule.Billing.JOB;
using System.Text;

public partial class ScheduleTasksForm : System.Web.UI.Page 
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
    #endregion

    string successFlag;//���������Ƿ�ɹ��ı��

    ObjectQuery<tblScheduler> SchedulerObjList;
    ObjectQuery<vSchedulerTasks> vSysJobsObjs;
    ObjectQuery<vOnlySysJobs> vOnlyJobsObjs;

    List<TasksTemp> TasksTempList;

    TasksTemp TasksTempObj;
    tblScheduler SchedulerObj;

    vSysJobsBLL BLL_vSysJobs;
    ScheduleTasksBLL BLL_ScheduleTasks;
    JOB BLL_JOB;
    vScheduleLogBLL BLL_ScheduleLog;

    int count;//ɾ��Ӱ���job����
    string SuccessFlag;//�½�job�ɹ����

    protected void Page_Load(object sender, EventArgs e)
    {
        if(!IsPostBack)
        {
            pnlMessage.Visible = false;
            pnlRight.Visible = false;
            pnlError.Visible = false;

            //��ʾ����job�ɹ�
            if (Request.QueryString["SuccessFlag"] != null && Request.QueryString["SuccessFlag"] != string.Empty)
            {
                SuccessFlag = Request.QueryString["SuccessFlag"].ToString();
                if (Convert.ToBoolean(SuccessFlag))
                {
                    MessageBox(true, false, false,
                        GetGlobalResourceObject("WebResource", "ScheduleTasksForm_CreateJob_Message").ToString());
                }
            }
        }
        this.GridViewDataBinding();
    }

    #region GridView������
    public void GridViewDataBinding()
    {
        Boolean isExtit;
        StringBuilder jobExpetion;
        this.OrderBy = " it.StartTime desc ";
        string whereStr;

        whereStr = "";
        BLL_ScheduleTasks=new ScheduleTasksBLL();
        SchedulerObjList = BLL_ScheduleTasks.GetSchedulers(whereStr, OrderBy);//��ѯSD��job��Ϣ

        BLL_vSysJobs = new vSysJobsBLL();
        if (SchedulerObjList.Count()>0)
        {
            BLL_ScheduleLog = new vScheduleLogBLL();
            TasksTempList = new List<TasksTemp>();

            foreach (tblScheduler item in SchedulerObjList)
            {
                this.OrderBy = " it.name desc";
                whereStr = " and it.name='" + item.TaskName + "'";
                
                vSysJobsObjs = BLL_vSysJobs.GetVSysJobs(whereStr, this.OrderBy);

                if (vSysJobsObjs.Count() > 0)
                {//job����
                    foreach (vSchedulerTasks Temp in vSysJobsObjs)
                    {
                        TasksTempObj = new TasksTemp();
                        TasksTempObj.ScheduleID = item.ScheduleID;
                        TasksTempObj.TaskName = item.TaskName.Substring(0, item.TaskName.LastIndexOf("-"));//��ȡ-CreateSD;
                        TasksTempObj.ScheduleType = item.ScheduleType;
                        TasksTempObj.Enable=Temp.enabled;
                        if (TasksTempObj.Enable.Equals(1))
                        {//job����
                            TasksTempObj.StatusImageURL = GetGlobalResourceObject("WebResource", "ScheduleTasksForm_CustomerMessage_Message").ToString();
                            TasksTempObj.Status = "Enable";
                        }
                        else
                        {//jobֹͣ����
                            TasksTempObj.StatusImageURL = GetGlobalResourceObject("WebResource", "ScheduleTasksForm_CustomerMessage_Message").ToString();
                            TasksTempObj.Status = "Disable";
                        }
                        TasksTempObj.Next_run_date = Temp.RunOnlyEnd.ToString();
                        TasksTempObj.Last_run_date = Temp.RunOnlyStart.ToString();

                        TasksTempObj.LogResult = BLL_ScheduleLog.FindJobResultByScheduleId(
                                                                            item.ScheduleID.ToString(), 
                                                                                        item.RunOnlyStart.ToString(), 
                                                                                                            item.RunOnlyEnd.ToString());
                    }
                    TasksTempList.Add(TasksTempObj);
                }
                else
                {//job������
                    TasksTempObj = new TasksTemp();
                    TasksTempObj.ScheduleID = item.ScheduleID;
                    TasksTempObj.TaskName = item.TaskName.Substring(0, item.TaskName.LastIndexOf("-"));//��ȡ-CreateSD;;
                    TasksTempObj.ScheduleType = item.ScheduleType;
                    TasksTempObj.StatusImageURL = GetGlobalResourceObject("WebResource", "ScheduleTasksForm_CustomerMessage_Message").ToString();
                    TasksTempObj.Status = "Disable";
                    TasksTempList.Add(TasksTempObj);
                }
            }
        }

        this.OrderBy = " it.name desc";
        whereStr = "";
        vOnlyJobsObjs = BLL_vSysJobs.GetVOnlyJobs(whereStr, this.OrderBy);//��ѯjob��ϵͳ��Ϣ

        jobExpetion = new StringBuilder();
        foreach (vOnlySysJobs job in vOnlyJobsObjs)
        {
            isExtit=SchedulerObjList.Any(it => it.TaskName == job.name);//��SD���Ƿ������Ӧ��Taskname
            if (!isExtit)//������
            {
                if (job.name.IndexOf("-CreateSD") > 0)//�Ƿ�SD����
                {
                    pnlMessage.Visible = true;//��ʾ��ʾ��Ϣ
                    //��ϵͳ�д���Ϊ-CreateSD��β��job������SD���ݿ��в�����
                    if (jobExpetion.ToString() ==string.Empty )
                    {
                        jobExpetion.Append("Job Excepetion:"+" ");
                        jobExpetion.Append("\""+job.name+"\"");
                    }
                    else
                    {
                        jobExpetion.Append(",\""+job.name+"\"");
                    }
                }
            }
        }
        if (pnlMessage.Visible == true && SuccessFlag == null)//��ʾ�쳣job��Ϣ
        {
            jobExpetion.Append(" ");
            jobExpetion.Append(GetGlobalResourceObject("WebResource", "ScheduleTasksForm_JobExcepeciton_Message").ToString());
            MessageBox(false, true, false,
                        jobExpetion.ToString());
        }
        else
        {
            pnlMessage.Visible = false;
        }

        RadGrid.DataSource = TasksTempList;
    }
    #endregion

    #region GridViewɾ���¼�
    protected void RadGrid_DeleteCommand(object sender, GridCommandEventArgs e)
    {
        string strWhere;
        string getScheduleID;//��ȡ�İ�MasterTableView��Guidֵ

        getScheduleID = (e.Item as GridDataItem).GetDataKeyValue("ScheduleID").ToString();

        BLL_ScheduleTasks = new ScheduleTasksBLL();
        SchedulerObj = BLL_ScheduleTasks.FindSchedulerById(getScheduleID);

        this.OrderBy = " it.name desc ";
        strWhere = " and it.name='" + SchedulerObj.TaskName + "'";
        BLL_vSysJobs = new vSysJobsBLL();
        vSysJobsObjs = BLL_vSysJobs.GetVSysJobs(strWhere, this.OrderBy);

        if (vSysJobsObjs.Count() > 0)
        {
            BLL_JOB = new JOB();
            count = BLL_JOB.DeleteJob(SchedulerObj.JobID.ToString());//ɾ��job

            if (count == 1)//ɾ����ҵ�ɹ�
            {
                successFlag = BLL_ScheduleTasks.LogicDelete(getScheduleID);
                if (!successFlag.Equals("InsertError"))//ɾ��ScheduleTasks�ڵ����ݳɹ�
                {
                    GridViewDataBinding();

                    //ɾ��ScheduleTask���ݳɹ�
                    MessageBox(true, false, false,
                        GetGlobalResourceObject("WebResource", "ScheduleTasksForm_DeleteTasksMessage_RightMessage").ToString());
                    Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "reload", "<script>window.location=window.self.location;</script>");//ǿ��ˢҳ
                }
                else
                {
                    //ɾ��ScheduleTask����ʧ��
                    MessageBox(false, false, true,
                        GetGlobalResourceObject("WebResource", "ScheduleTasksForm_DeleteTasksMessage_ErrorMessage").ToString());
                }
            }
            else
            {
                //ɾ��jobʧ��
                MessageBox(false, false, true,
                    GetGlobalResourceObject("WebResource", "ScheduleTasksForm_DeleteJobMessage_ErrorMessage").ToString());
            }
        }
        else
        {
            successFlag = BLL_ScheduleTasks.LogicDelete(getScheduleID);
            if (!successFlag.Equals("InsertError"))//ɾ��ScheduleTasks�ڵ����ݳɹ�
            {
                GridViewDataBinding();

                //ɾ��ScheduleTask���ݳɹ�
                MessageBox(true, false, false,
                    GetGlobalResourceObject("WebResource", "ScheduleTasksForm_DeleteTasksMessage_RightMessage").ToString());
                Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "reload", "<script>window.location=window.self.location;</script>");//ǿ��ˢҳ
            }
            else
            {
                //ɾ��ScheduleTask����ʧ��
                MessageBox(false, false, true,
                    GetGlobalResourceObject("WebResource", "ScheduleTasksForm_DeleteTasksMessage_ErrorMessage").ToString());
            }
        }
    }
    #endregion

    #region GridView�����¼�
    protected void RadGrid_UpdateCommand(object sender, GridCommandEventArgs e)
    {
        string getScheduleID;//��ȡ
        //��ȡGrid������ֵ
        getScheduleID = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["ScheduleID"].ToString();
        if (getScheduleID!=string.Empty)
        {
            ClearCookie();
            Response.Redirect("~/Schedule Billing/TaskDetailFormView.aspx?KeyGuid=" + getScheduleID);
        }
    }
    #endregion

    #region GridView�¼����ദ��
    protected void RadGrid_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == RadGrid.DeleteCommandName)
        {
            RadGrid_DeleteCommand(sender, e);//ɾ����¼
        }

        if (e.CommandName == RadGrid.EditCommandName)
        {
            RadGrid_UpdateCommand(sender, e);//�༭��¼
        }

        if (e.CommandName == RadGrid.UpdateCommandName)
        {
            RadGrid_EnableCommand(sender, e);//�޸�job����
        }
        if (e.CommandName == "LogResult")
        {
            string scheduleID;//��ȡ
            //��ȡGrid������ֵ
            scheduleID = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["ScheduleID"].ToString();
            FindScheduleLog(scheduleID);
        }
    }
    #endregion

    #region ��job�Ƿ��������-enable/disable���ԣ��Լ���job��tblScheduler���ݲ�һ��ʱ����tblScheduler�������Ϊ׼
    protected void RadGrid_EnableCommand(object sender, GridCommandEventArgs e)
    {
        //this.OrderBy = " it.StartTime desc ";
        //string flagSuccess;//�޸�job�Ƿ�ɹ����

        //vSchedulerTasks vSchedulerObj;

        //string getScheduleID;//��ȡ
        ////��ȡGrid������ֵ
        //getScheduleID = e.Item.OwnerTableView.DataKeyValues[e.Item.ItemIndex]["ScheduleID"].ToString();

        //BLL_vSysJobs = new vSysJobsBLL();
        //vSchedulerObj = BLL_vSysJobs.FindTaskSchedulerByTaskId(getScheduleID);

        //if (vSchedulerObj != null)
        //{
        //    //1Ϊ����,0Ϊֹͣ
        //    if (vSchedulerObj.enabled == 1)
        //    {
        //        //�޸�jobΪֹͣ
        //        BLL_JOB = new JOB();
        //        flagSuccess = BLL_JOB.Create(vSchedulerObj.ScheduleID.ToString(), "disable");
        //    }
        //    else
        //    {
        //        //�޸�jobΪ����
        //        BLL_JOB = new JOB();
        //        flagSuccess = BLL_JOB.Create(vSchedulerObj.ScheduleID.ToString(), "enable");
        //    }

        //    if (flagSuccess != string.Empty)
        //    {
        //        BLL_ScheduleTasks = new ScheduleTasksBLL();
        //        SchedulerObj=BLL_ScheduleTasks.FindSchedulerById(vSchedulerObj.ScheduleID.ToString());
        //        System.Guid KeyIdGuid = new Guid(flagSuccess);
        //        SchedulerObj.JobID = KeyIdGuid;
        //        BLL_ScheduleTasks.UpdateInTaskView(SchedulerObj);
                
        //        //�ɹ������ݣ�ˢ��ҳ��
        //        GridViewDataBinding();

        //        //��ʾ�޸�job״̬�ɹ�
        //        MessageBox(true, false, false,
        //            GetGlobalResourceObject("WebResource", "ScheduleTasksForm_EbleDisableTasksMessage_RightMessage").ToString());
        //        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "reload", "<script>window.location=window.self.location;</script>");//ǿ��ˢҳ
        //    }
        //    else//�޸�jobʧ��
        //    {
        //        MessageBox(false, false, true,
        //            GetGlobalResourceObject("WebResource", "ScheduleTasksForm_UpdateFailed_ErrorMessage").ToString());
        //    }
        //}
        //else//Job������
        //{
        //    MessageBox(false, false, true, 
        //            GetGlobalResourceObject("WebResource", "ScheduleTasksForm_JobMessage_ErrorMessage").ToString());
        //}
    }
    #endregion

    #region ����һ����¼,���cookies��Ϊ�գ�������մ���
    protected void rbtnInsert_Click(object sender, EventArgs e)
    {
        //���cookies
        ClearCookie();
        //���Cookies����GridViewҳ��
        Response.Redirect("~/Schedule Billing/TaskDetailForm.aspx");
    }
    #endregion

    #region ��ʽ��ϵͳint32ʱ�����ͣ�Next_run_date��Last_run_date
    protected string FormatDateTime(int Date, int time)
    {
        string dateTime;
        string temp;

        string year="0000";
        string months="00";
        string date="00";

        string hours="00";
        string minutes="00";

        //��ʽ�������� ��MM/dd/yyyy��
        if (Date>0)
        {
            year=Date.ToString().Substring(0, 4);
            months = Date.ToString().Substring(4, 2);
            date = Date.ToString().Substring(6, 2);
        }
        //��ʽ����ʱ��  ��HH:mm��
        if (time>0)
        {
            if (time.ToString().Length < 6 )//�����㣨HH:mm:ss������0��ǰ����
            {
                temp = time.ToString().PadLeft(6, '0');
                hours = temp.ToString().Substring(0, 2);
                minutes = temp.ToString().Substring(2, 2);
            }
            else
            {
                hours = time.ToString().Substring(0, 2);
                minutes = time.ToString().Substring(2, 2);
            }
            
        }
        dateTime = months + "/" + date + "/" + year + " " + hours + ":" + minutes;
        return dateTime;
    }
    #endregion

    #region ��ʾ��ʾ��Ϣ
    protected void MessageBox(Boolean correct, Boolean Warning, Boolean incorrect, string message)
    {
        pnlError.Visible = incorrect;//������ʾ��Ϣ
        pnlMessage.Visible = Warning;//����
        pnlRight.Visible = correct;//��ȷ

        if (incorrect==true)
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

    #region ���cookie
    protected void ClearCookie()
    {
        //���cookies
        HttpCookie BillingCookie;
        HttpCookie TeskDetailCookie;
        HttpCookie EmailSettingCookie;
        HttpCookie backupSettingCookie;

        BillingCookie = Request.Cookies["Billing"];
        if (BillingCookie != null)
        {
            BillingCookie.Expires = DateTime.Now.AddDays(-31);
            Response.Cookies.Add(BillingCookie);
        }

        EmailSettingCookie = Request.Cookies["EmailSetting"];
        if (EmailSettingCookie != null)
        {
            EmailSettingCookie.Expires = DateTime.Now.AddDays(-31);
            Response.Cookies.Add(EmailSettingCookie);
        }

        backupSettingCookie = Request.Cookies["BuckupSetting"];
        if (backupSettingCookie != null)
        {
            backupSettingCookie.Expires = DateTime.Now.AddDays(-31);
            Response.Cookies.Add(backupSettingCookie);
        }

        TeskDetailCookie = Request.Cookies["TeskDetail"];
        if (TeskDetailCookie != null)
        {
            TeskDetailCookie.Expires = DateTime.Now.AddDays(-31);
            Response.Cookies.Add(TeskDetailCookie);
        }
    }
    #endregion

    #region ��ת��Logҳ��
    protected void FindScheduleLog(string scheduleID)
    {
        Response.Redirect("~/Schedule Billing/ScheduleLogForm.aspx?KeyGuid=" + scheduleID);
    }
    #endregion

    #region ϵͳ����
    protected void rbtnBackupSetting_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Schedule Setting/BackupSettingForm.aspx");
    }
    #endregion
}

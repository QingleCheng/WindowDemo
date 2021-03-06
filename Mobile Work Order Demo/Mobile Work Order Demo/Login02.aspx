﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login02.aspx.cs" Inherits="Mobile_Work_Order_Demo.Login02" %>

<%@ Register Assembly="Telerik.Web.UI" Namespace="Telerik.Web.UI" TagPrefix="telerik" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script src="Scripts/jquery-1.4.1.js" type="text/javascript"></script>
    <script src="Scripts/jquery-1.4.1-vsdoc.js" type="text/javascript"></script>
    <style type="text/css">
        /*居中*/
        .center
        {
            margin: 0px auto;
            width: 990px;
        }
        
        #LogCss
        {
            width: 80%;
            height: 100%;
        }
        .bottom .box_outer
        {
            border-bottom: #FFFFFFb 1px solid;
            border-left: #FFFFFFb 1px solid;
            padding-bottom: 2px;
            background-color: #FFFFFF;
            padding-left: 2px;
            padding-right: 2px;
            border-top: #FFFFFFb 1px solid; /*上边线*/
            border-right: #FFFFFFb 1px solid; /*右边线*/
            padding-top: 2px;
        }
        .divcenter
        {
            width: 200px;
            height: 200px;
        }
    </style>
    <!--0000000-->
    <style type="text/css">
        #bodycontent
        {
            margin: auto;
            width: 990px;
        }
        .rightcontent-top
        {
            line-height: 0;
            background: no-repeat;
            height: 6px;
            font-size: 0px;
        }
        .rightcontent-top
        {
            background-position: -140px top;
        }
        .rightcontent-top
        {
            background: url(http://img.baidu.com/hi/img/ihome/sysappborder.gif) no-repeat 0px -15px;
        }
        
        .rightcontent-middle
        {
            border-bottom: #e0e0e0 0px solid;
            border-left: #e0e0e0 1px solid;
            padding-bottom: 4px;
            background-color: #fff;
            padding-left: 10px;
            padding-right: 10px;
            zoom: 1;
            border-top: #e0e0e0 0px solid;
            border-right: #e0e0e0 1px solid;
            padding-top: 4px;
        }
        
        
        
        .rightcontent-middle .r-r-top
        {
            background-position: left top;
        }
        .rightcontent-middle .r-r-bottom
        {
            background-position: left bottom;
        }
        .rightcontent-middle .r-r-middle
        {
            padding-bottom: 4px;
            background-color: #f5f5f5;
            padding-left: 10px;
            padding-right: 10px;
            padding-top: 4px;
        }
        /*左右边框*/
        .rightcontent-middle
        {
            border-bottom-color: #2aa8de;
            border-top-color: #2aa8de;
            border-right-color: #2aa8de;
            border-left-color: #2aa8de;
        }
        
        .rightcontent-bottom
        {
            line-height: 0;
            background: no-repeat;
            height: 6px;
            font-size: 0px;
        }
        .rightcontent-bottom
        {
            background-position: -140px bottom;
        }
        .rightcontent-bottom
        {
            background: url(http://img.baidu.com/hi/img/ihome/sysappborder.gif) 0px -24px;
        }
        
        /*div 的宽度*/
        .grid-85
        {
            /*width: 850px;*/
            width: 850px;
        }
    </style>
    <style type="text/css">
        .test
        {
            /*渐变*/
            filter: progid:DXImageTransform.Microsoft.Gradient(gradientType=0,startColorStr=#CCCCCC,endColorStr=#FFFFFF); /*IE6*/
            background: -moz-linear-gradient(top,#CCCCCC,#FFFFFF); /*非IE6的其它*/
            background: -webkit-gradient(linear, 0% 0%, 0% 80%, from(#CCCCCC), to(#FFFFFF)); /*非IE6的其它*/
            height: 100%;
        }
        .style2
        {
            color: #FFFFFF;
            font-size: small;
        }
        .style3
        {
            font-size: x-large;
        }
    </style>
    <style type="text/css">
        /*超链接默认颜色*/
        a.a1
        {
            color: #FFFFFF;
        }
        /*超链接鼠标放上时的颜色*/
        a.a1:hover { color:#FFFFFF;}
        
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <div id="content" class="center">
            <!--LOG In -->
            <div id="LogCss" style="width: 250px; height: 0px; position: relative; left: 800px;
                top: 5px;" class="style2">
              <a href="#" class="a1">Log In</a> or <a href="#" class="a1"> Register </a> (Why?)
            </div>
            <!--LOG In -->
            <div class="bottom">
                <div class="box_outer">
                    <!--横向菜单-->
                    <div>
                        <telerik:RadScriptManager ID="RadScriptManager1" runat="server">
                        </telerik:RadScriptManager>
                        <!--LOGO-->
                        <div style="background-color: #000000; height: 100px;">
                            <br />
                            <asp:Image ID="Image1" runat="server" ImageUrl="~/Image/logo.png" />
                            <br />
                        </div>
                        <telerik:RadMenu ID="RadMenu1" runat="server" Skin="Black" Width="986px">
                        </telerik:RadMenu>
                    </div>
                    <!--横向菜单-->
                    <div class="test">
                        &nbsp;
                        <br />
                        &nbsp;&nbsp;<span class="style3">Your Service Direct Account</span>
                        <br />
                        &nbsp;
                        <!--中间白色DIV-->
                        <%--     <div class="grid-85 right" style="width: 850px; height: 0px; position: relative;
                            left: 20px; top: 10px;">
                            <div class="rightcontent-top">
                            </div>
                            <div class="rightcontent-middle">
                                <p>
                                    sdfsdfsdfsdf
                                </p>
                                <p>
                                    &nbsp;</p>
                                <p>
                                    sd</p>
                                <p>
                                    sd</p>
                                <p>
                                    sd</p>
                            </div>
                            <div class="rightcontent-bottom">
                            </div>
                        </div>--%>
                        <table width="950" align="center" style="height: 200px">
                            <tr>
                                <td>
                                    <div style="margin: 0 4px; background: #FFFFFF; height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="margin: 0 2px; border: 1px solid #FFFFFF; border-width: 0 2px; background: #FFFFFF;
                                        height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="margin: 0 1px; border: 1px solid #FFFFFF; border-width: 0 1px; background: #FFFFFF;
                                        height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="margin: 0 1px; border: 1px solid #FFFFFF; border-width: 0 1px; background: #FFFFFF;
                                        height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="background: #FFFFFF; border: 1px solid #FFFFFF; border-width: 0 1px;">
                                        <div style="background: #FFF; margin: 0 3px; font-size: 11px; font-family: Verdana;
                                            color: #333; padding: 5px 10px; overflow: hidden;">
                                            11111111111111
                                            <br />
                                            sdf<br />
                                            sdf<br />
                                            s<br />
                                            df<br />
                                            sdf<br />
                                            sd<br />
                                            fs<br />
                                            df<br />
                                            sdf<br />
                                            sd<br />
                                            fs<br />
                                            df<br />
                                            sd<br />
                                            df</div>
                                    </div>
                                    <div style="margin: 0 1px; border: 1px solid #FFFFFF; border-width: 0 1px; background: #FFFFFF;
                                        height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="margin: 0 1px; border: 1px solid #FFFFFF; border-width: 0 2px; background: #FFFFFF;
                                        height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="margin: 0 2px; border: 1px solid #FFFFFF; border-width: 0 2px; background: #FFFFFF;
                                        height: 1px; overflow: hidden;">
                                    </div>
                                    <div style="margin: 0 4px; background: #FFFFFF; height: 1px; overflow: hidden;">
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <!--中间白色DIV-->
                    </div>
                </div>
            </div>
        </div>
    </div>
    </form>
</body>
</html>

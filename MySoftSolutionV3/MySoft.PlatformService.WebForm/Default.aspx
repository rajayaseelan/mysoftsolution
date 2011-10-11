<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MySoft.PlatformService.WebForm._Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>服务统计信息</title>
    <style type="text/css">
        li
        {
            list-style-type: none;
        }
        
        ul
        {
            margin: 0px;
            padding: 0px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <% if (status != null)
       { %>
    <div>
        <ul>
            <li><b>服务端状态信息如下：</b>(<% =status.StartDate%>)<asp:Button ID="btnClear" runat="server"
                OnClick="btnClear_Click" Text="清除所有状态" />
            </li>
            <li>
                <hr />
            </li>
        </ul>
    </div>
    <div style="float: left; width: 250px;">
        <ul>
            <li><b>汇总状态信息</b></li>
            <li>运行总时间：
                <%=status.TotalSeconds%>
                秒</li>
            <li>请求数:
                <%=status.Summary.RequestCount%>
                次</li>
            <li>成功数:
                <%=status.Summary.SuccessCount%>
                次</li>
            <li>错误数:
                <%=status.Summary.ErrorCount%>
                次</li>
            <li>总耗时:
                <%=status.Summary.ElapsedTime%>
                毫秒</li>
            <li>总流量:
                <%= Math.Round(status.Summary.DataFlow * 1.0 / 1024, 4)%>
                KB</li>
            <li>请求总时间：
                <%=status.Summary.RunningSeconds%>
                秒</li>
            <li>每秒请求数:
                <%=status.Summary.AverageRequestCount%>
                次</li>
            <li>平均成功数:
                <%=status.Summary.AverageSuccessCount%>
                次</li>
            <li>平均错误数:
                <%=status.Summary.AverageErrorCount%>
                次</li>
            <li>平均耗时数:
                <%=status.Summary.AverageElapsedTime%>
                毫秒</li>
            <li>每秒流量数:
                <%=Math.Round(status.Summary.AverageDataFlow * 1.0 / 1024, 4)%>
                KB</li>
        </ul>
    </div>
    <div style="float: left; width: 250px;">
        <ul>
            <li><b>当前状态信息</b></li>
            <li>时间:
                <%=status.Latest.CounterTime%></li>
            <li>请求数:
                <%=status.Latest.RequestCount%>
                次</li>
            <li>成功数:
                <%=status.Latest.SuccessCount%>
                次</li>
            <li>错误数:
                <%=status.Latest.ErrorCount%>
                次</li>
            <li>总耗时:
                <%=status.Latest.ElapsedTime%>
                毫秒</li>
            <li>总流量:
                <%=Math.Round(status.Latest.DataFlow * 1.0 / 1024, 4)%>
                KB</li>
            <li>平均耗时:
                <%=status.Latest.AverageElapsedTime%>
                毫秒</li>
            <li>平均流量:
                <%=Math.Round(status.Latest.AverageDataFlow * 1.0 / 1024, 4)%>
                KB</li>
        </ul>
    </div>
    <div>
        <ul>
            <li><b>最高状态信息</b></li>
            <li>最大请求数:
                <%=status.Highest.RequestCount%>
                次 (<%= status.Highest.RequestCountCounterTime%>)</li>
            <li>最大成功数:
                <%=status.Highest.SuccessCount%>
                次 (<%= status.Highest.SuccessCountCounterTime%>)</li>
            <li>最大错误数:
                <%=status.Highest.ErrorCount%>
                次 (<%= status.Highest.ErrorCountCounterTime%>)</li>
            <li>最大耗时:
                <%=status.Highest.ElapsedTime%>
                毫秒 (<%= status.Highest.ElapsedTimeCounterTime%>)</li>
            <li>最大流量:
                <%=Math.Round(status.Highest.DataFlow * 1.0 / 1024, 4)%>
                KB (<%= status.Highest.DataFlowCounterTime%>)</li>
        </ul>
    </div>
    <% } %>
    <div style="clear: both;">
    </div>
    <div>
        <ul>
            <% if (clients != null)
               { %>
            <li>
                <hr />
            </li>
            <li><b>客户端连接信息</b></li>
            <%
                   int index = 1;
                   foreach (MySoft.IoC.Status.ConnectionInfo client in clients)
                   {%>
            <li style="width: 200px; float: left;">
                <% = index %>
                =>
                <%= client.IP %>(<font color="red"><%= client.Count %></font>)</li>
            <%  index++;
                   }
               } %>
        </ul>
    </div>
    </form>
    <script type="text/javascript">

        var timer = function () {
            document.location.reload();
        };

        setInterval(function () {
            timer();
        }, 5000);
    </script>
</body>
</html>

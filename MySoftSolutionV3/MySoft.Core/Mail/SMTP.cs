using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using MySoft.Logger;

namespace MySoft.Mail
{
    #region 邮件接收类

    /// <summary>
    /// 邮件接收类
    /// </summary>
    public class POP3
    {
        #region Fields

        string POPServer;
        string mPOPUserName;
        string mPOPPass;
        int mPOPPort;
        NetworkStream ns;
        StreamReader sr;

        #endregion

        #region Constructors

        /// <summary>
        /// POP3
        /// </summary>
        /// <param name="server">POP3服务器名称</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">用户密码</param>
        public POP3(string server, string userName, string password)
            : this(server, 110, userName, password)
        {
        }

        /// <summary>
        /// POP3
        /// </summary>
        /// <param name="server">POP3服务器名称</param>
        /// <param name="port">端口号</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">用户密码</param>
        public POP3(string server, int port, string userName, string password)
        {
            POPServer = server;
            mPOPUserName = userName;
            mPOPPass = password;
            mPOPPort = port;
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// 获得新邮件数量
        /// </summary>
        /// <returns>新邮件数量</returns>
        public int GetNumberOfNewMessages()
        {
            byte[] outbytes;
            string input;

            try
            {
                Connect();

                input = "stat" + "\r\n";
                outbytes = System.Text.Encoding.ASCII.GetBytes(input.ToCharArray());
                ns.Write(outbytes, 0, outbytes.Length);
                string resp = sr.ReadLine();
                string[] tokens = resp.Split(new Char[] { ' ' });

                Disconnect();

                return Convert.ToInt32(tokens[1]);
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// 获取新邮件内容
        /// </summary>
        /// <param name="subj">邮件主题</param>
        /// <returns>新邮件内容</returns>
        public List<MailMessage> GetNewMessages(string subj)
        {

            int newcount;
            List<MailMessage> newmsgs = new List<MailMessage>();

            try
            {
                newcount = GetNumberOfNewMessages();
                Connect();

                for (int n = 1; n < newcount + 1; n++)
                {
                    List<string> msglines = GetRawMessage(n);
                    string msgsubj = GetMessageSubject(msglines);
                    if (msgsubj.CompareTo(subj) == 0)
                    {
                        MailMessage msg = new MailMessage();
                        msg.Subject = msgsubj;
                        msg.From = new MailAddress(GetMessageFrom(msglines));
                        msg.Body = GetMessageBody(msglines);
                        newmsgs.Add(msg);
                        DeleteMessage(n);
                    }
                }

                Disconnect();
                return newmsgs;
            }
            catch (Exception e)
            {
                return newmsgs;
            }
        }

        /// <summary>
        /// 获取新邮件内容
        /// </summary>
        /// <param name="nIndex">新邮件索引</param>
        /// <returns>新邮件内容</returns>
        public MailMessage GetNewMessages(int nIndex)
        {
            int newcount;
            MailMessage msg = new MailMessage();

            try
            {
                newcount = GetNumberOfNewMessages();
                Connect();
                int n = nIndex + 1;

                if (n < newcount + 1)
                {
                    List<string> msglines = GetRawMessage(n);
                    string msgsubj = GetMessageSubject(msglines);


                    msg.Subject = msgsubj;
                    msg.From = new MailAddress(GetMessageFrom(msglines));
                    msg.Body = GetMessageBody(msglines);
                }

                Disconnect();
                return msg;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Private

        private bool Connect()
        {
            TcpClient sender = new TcpClient(POPServer, mPOPPort);
            byte[] outbytes;
            string input;

            try
            {
                ns = sender.GetStream();
                sr = new StreamReader(ns);

                sr.ReadLine();
                input = "user " + mPOPUserName + "\r\n";
                outbytes = System.Text.Encoding.ASCII.GetBytes(input.ToCharArray());
                ns.Write(outbytes, 0, outbytes.Length);
                sr.ReadLine();

                input = "pass " + mPOPPass + "\r\n";
                outbytes = System.Text.Encoding.ASCII.GetBytes(input.ToCharArray());
                ns.Write(outbytes, 0, outbytes.Length);
                sr.ReadLine();
                return true;

            }
            catch
            {
                return false;
            }
        }

        private void Disconnect()
        {
            string input = "quit" + "\r\n";
            Byte[] outbytes = System.Text.Encoding.ASCII.GetBytes(input.ToCharArray());
            ns.Write(outbytes, 0, outbytes.Length);
            ns.Close();
        }

        private List<string> GetRawMessage(int messagenumber)
        {
            Byte[] outbytes;
            string input;
            string line = "";

            input = "retr " + messagenumber.ToString() + "\r\n";
            outbytes = System.Text.Encoding.ASCII.GetBytes(input.ToCharArray());
            ns.Write(outbytes, 0, outbytes.Length);

            List<string> msglines = new List<string>();
            do
            {
                line = sr.ReadLine();
                msglines.Add(line);
            } while (line != ".");
            msglines.RemoveAt(msglines.Count - 1);

            return msglines;
        }

        private string GetMessageSubject(List<string> msglines)
        {
            string[] tokens;
            IEnumerator msgenum = msglines.GetEnumerator();
            while (msgenum.MoveNext())
            {
                string line = (string)msgenum.Current;
                if (line.StartsWith("Subject:"))
                {
                    tokens = line.Split(new Char[] { ' ' });
                    return tokens[1].Trim();
                }
            }
            return "None";
        }

        private string GetMessageFrom(List<string> msglines)
        {
            string[] tokens;
            IEnumerator msgenum = msglines.GetEnumerator();
            while (msgenum.MoveNext())
            {
                string line = (string)msgenum.Current;
                if (line.StartsWith("From:"))
                {
                    tokens = line.Split(new Char[] { '<' });
                    return tokens[1].Trim(new Char[] { '<', '>' });
                }
            }
            return "None";
        }

        private string GetMessageBody(List<string> msglines)
        {
            string body = "";
            string line = " ";
            IEnumerator msgenum = msglines.GetEnumerator();

            while (line.CompareTo("") != 0)
            {
                msgenum.MoveNext();
                line = (string)msgenum.Current;
            }

            while (msgenum.MoveNext())
            {
                body = body + (string)msgenum.Current + "\r\n";
            }
            return body;
        }

        private void DeleteMessage(int messagenumber)
        {
            Byte[] outbytes;
            string input;

            try
            {
                input = "dele " + messagenumber.ToString() + "\r\n";
                outbytes = System.Text.Encoding.ASCII.GetBytes(input.ToCharArray());
                ns.Write(outbytes, 0, outbytes.Length);
            }
            catch (Exception e)
            {
                return;
            }

        }

        #endregion

        #endregion
    }

    #endregion

    #region 邮件发送类

    public class SMTP
    {
        #region Fields

        private string mMailFrom;
        private string mMailDisplyName;
        private string[] mMailTo;
        private string[] mMailCc;
        private string[] mMailBcc;
        private string mMailSubject;
        private string mMailBody;
        private string[] mMailAttachments;
        private string mSMTPServer;
        private int mSMTPPort;
        private string mSMTPUsername;
        private string mSMTPPassword;
        private bool mSMTPSSL;
        private MailPriority mPriority = MailPriority.Normal;
        private bool mIsBodyHtml = false;

        #endregion

        #region Properties

        /// <summary>
        /// 发件人地址
        /// </summary>
        public string MailFrom
        {
            set { mMailFrom = value; }
            get { return mMailFrom; }
        }

        /// <summary>
        /// 显示的名称
        /// </summary>
        public string MailDisplyName
        {
            set { mMailDisplyName = value; }
            get { return mMailDisplyName; }
        }

        /// <summary>
        /// 收件人地址
        /// </summary>
        public string[] MailTo
        {
            set { mMailTo = value; }
            get { return mMailTo; }
        }

        /// <summary>
        /// 抄送
        /// </summary>
        public string[] MailCc
        {
            set { mMailCc = value; }
            get { return mMailCc; }
        }

        /// <summary>
        /// 密件抄送
        /// </summary>
        public string[] MailBcc
        {
            set { mMailBcc = value; }
            get { return mMailBcc; }
        }

        /// <summary>
        /// 邮件主题
        /// </summary>
        public string MailSubject
        {
            set { mMailSubject = value; }
            get { return mMailSubject; }
        }

        /// <summary>
        /// 邮件正文
        /// </summary>
        public string MailBody
        {
            set { mMailBody = value; }
            get { return mMailBody; }
        }

        /// <summary>
        /// 附件
        /// </summary>
        public string[] MailAttachments
        {
            set { mMailAttachments = value; }
            get { return mMailAttachments; }
        }

        /// <summary>
        /// SMTP 服务器
        /// </summary>
        public string SMTPServer
        {
            set { mSMTPServer = value; }
            get { return mSMTPServer; }
        }

        /// <summary>
        /// 发送端口号(默认为 25)
        /// </summary>
        public int SMTPPort
        {
            set { mSMTPPort = value; }
            get { return mSMTPPort; }
        }

        /// <summary>
        /// 用户名
        /// </summary>
        public string SMTPUsername
        {
            set { mSMTPUsername = value; }
            get { return mSMTPUsername; }
        }

        /// <summary>
        /// 密码
        /// </summary>
        public string SMTPPassword
        {
            set { mSMTPPassword = value; }
            get { return mSMTPPassword; }
        }

        /// <summary>
        /// 是否使用安全套接字层 (SSL) 加密连接
        /// 默认为 false
        /// </summary>
        public Boolean SMTPSSL
        {
            set { mSMTPSSL = value; }
            get { return mSMTPSSL; }
        }

        /// <summary>
        /// 邮件的优先级
        /// </summary>
        public MailPriority Priority
        {
            get { return mPriority; }
            set { mPriority = value; }
        }

        /// <summary>
        /// 示邮件正文是否为 Html 格式的值
        /// </summary>
        public bool IsBodyHtml
        {
            get { return mIsBodyHtml; }
            set { mIsBodyHtml = value; }
        }

        #endregion

        #region Constructors

        public SMTP() { }

        /// <summary>
        /// 邮件发送类
        /// 主机信息从配置文件中获取
        /// 参考:ms-help://MS.VSCC.v80/MS.MSDN.v80/MS.NETDEVFX.v20.chs/dv_fxgenref/html/54f0f153-17e5-4f49-afdc-deadb940c9c1.htm
        /// </summary>
        /// <param name="mailFrom">发件人地址</param>
        /// <param name="mailTo">收件人地址</param>
        /// <param name="mailSubject">邮件主题</param>
        /// <param name="mailBody">邮件正文</param>
        public SMTP(string[] mailTo, string mailSubject, string mailBody)
        {
            System.Configuration.Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            MailSettingsSectionGroup mailSettings = NetSectionGroup.GetSectionGroup(config).MailSettings;

            mMailFrom = mailSettings.Smtp.From;
            mMailDisplyName = mailSettings.Smtp.From;
            mMailTo = mailTo;
            mMailCc = null;
            mMailBcc = null;
            mMailSubject = mailSubject;
            mMailBody = mailBody;
            mMailAttachments = null;
            mSMTPServer = mailSettings.Smtp.Network.Host;
            mSMTPPort = mailSettings.Smtp.Network.Port;
            mSMTPUsername = mailSettings.Smtp.Network.UserName;
            mSMTPPassword = mailSettings.Smtp.Network.Password;
            mSMTPSSL = false;
        }

        /// <summary>
        /// 邮件发送类
        /// </summary>
        /// <param name="mailFrom">发件人地址</param>
        /// <param name="mailTo">收件人地址</param>
        /// <param name="mailSubject">邮件主题</param>
        /// <param name="mailBody">邮件正文</param>
        /// <param name="smtpServer">SMTP 服务器</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public SMTP(string mailFrom, string[] mailTo, string mailSubject, string mailBody,
            string smtpServer, string userName, string password)
            : this(mailFrom, mailFrom, mailTo, mailSubject, mailBody, null, smtpServer, userName, password)
        {
        }

        /// <summary>
        /// 邮件发送类
        /// </summary>
        /// <param name="mailFrom">发件人地址</param>
        /// <param name="displayName">显示的名称</param>
        /// <param name="mailTo">收件人地址</param>
        /// <param name="mailSubject">邮件主题</param>
        /// <param name="mailBody">邮件正文</param>
        /// <param name="attachments">附件,多个时用逗号隔开(可为空)</param>
        /// <param name="smtpServer">SMTP 服务器</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public SMTP(string mailFrom, string[] mailTo, string mailSubject, string mailBody,
            string[] attachments, string smtpServer, string userName, string password)
            : this(mailFrom, mailFrom, mailTo, mailSubject, mailBody,
            attachments, smtpServer, userName, password)
        {
        }

        /// <summary>
        /// 邮件发送类
        /// </summary>
        /// <param name="mailFrom">发件人地址</param>
        /// <param name="displayName">显示的名称</param>
        /// <param name="mailTo">收件人地址</param>
        /// <param name="mailSubject">邮件主题</param>
        /// <param name="mailBody">邮件正文</param>
        /// <param name="attachments">附件,多个时用逗号隔开(可为空)</param>
        /// <param name="smtpServer">SMTP 服务器</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public SMTP(string mailFrom, string displayName, string[] mailTo, string mailSubject, string mailBody,
            string[] attachments, string smtpServer, string userName, string password)
            : this(mailFrom, displayName, mailTo, null, null, mailSubject, mailBody,
            attachments, smtpServer, 25, userName, password, false)
        {
        }

        /// <summary>
        /// 邮件发送类
        /// </summary>
        /// <param name="mailFrom">发件人地址</param>
        /// <param name="displayName">显示的名称</param>
        /// <param name="mailTo">收件人地址</param>
        /// <param name="mailCc">抄送,多个收件人用逗号隔开(可为空)</param>
        /// <param name="mailBcc">密件抄送,多个收件人用逗号隔开(可为空)</param>
        /// <param name="mailSubject">邮件主题</param>
        /// <param name="mailBody">邮件正文</param>
        /// <param name="attachments">附件,多个时用逗号隔开(可为空)</param>
        /// <param name="smtpServer">SMTP 服务器</param>
        /// <param name="smtpPort">发送端口号(默认为 25)</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="smtpSsl">是否使用安全套接字层 (SSL) 加密连接</param>
        public SMTP(string mailFrom, string displayName, string[] mailTo, string[] mailCc, string[] mailBcc, string mailSubject, string mailBody,
            string[] attachments, string smtpServer, int smtpPort, string userName, string password, bool smtpSsl)
        {
            mMailFrom = mailFrom;
            mMailDisplyName = displayName;
            mMailTo = mailTo;
            mMailCc = mailCc;
            mMailBcc = mailBcc;
            mMailSubject = mailSubject;
            mMailBody = mailBody;
            mMailAttachments = attachments;
            mSMTPServer = smtpServer;
            mSMTPPort = smtpPort;
            mSMTPUsername = userName;
            mSMTPPassword = password;
            mSMTPSSL = smtpSsl;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 同步发送邮件
        /// </summary>
        /// <returns></returns>
        public SendResult Send()
        {
            var result = SendMail(false);

            string log = string.Format("SyncSendMail from ({0}) to ({1}), {2}.", mMailFrom, String.Join("|", mMailTo), result.Message);

            //写邮件发送日志
            SimpleLog.Instance.WriteLogForDir("Mail", string.Format("{0} subject: {1}", log, mMailSubject));

            return result;
        }

        /// <summary>
        /// 异步发送邮件
        /// </summary>
        public void SendAsync()
        {
            SendMail(true);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="isAsync"></param>
        /// <returns></returns>
        private SendResult SendMail(bool isAsync)
        {
            //状态码为0表示成功
            var result = new SendResult { Success = true, Message = "邮件发送成功！" };

            try
            {
                #region 设置属性值

                string[] mailTos = mMailTo;
                string[] mailCcs = mMailCc;
                string[] mailBccs = mMailBcc;
                string[] attachments = mMailAttachments;

                // build the email message
                MailMessage Email = new MailMessage();
                MailAddress MailFrom = new MailAddress(mMailFrom, mMailDisplyName);
                Email.From = MailFrom;
                Email.Subject = mMailSubject;
                Email.Body = mMailBody;
                Email.Priority = mPriority;
                Email.IsBodyHtml = mIsBodyHtml;
                Email.SubjectEncoding = Encoding.UTF8;
                Email.BodyEncoding = Encoding.UTF8;

                if (mailTos != null)
                {
                    foreach (string mailto in mailTos)
                    {
                        if (!string.IsNullOrEmpty(mailto))
                        {
                            Email.To.Add(mailto);
                        }
                    }
                }

                if (mailCcs != null)
                {
                    foreach (string cc in mailCcs)
                    {
                        if (!string.IsNullOrEmpty(cc))
                        {
                            Email.CC.Add(cc);
                        }
                    }
                }

                if (mailBccs != null)
                {
                    foreach (string bcc in mailBccs)
                    {
                        if (!string.IsNullOrEmpty(bcc))
                        {
                            Email.Bcc.Add(bcc);
                        }
                    }
                }

                if (attachments != null)
                {
                    foreach (string file in attachments)
                    {
                        if (!string.IsNullOrEmpty(file))
                        {
                            Attachment att = new Attachment(file);
                            Email.Attachments.Add(att);
                        }
                    }
                }

                // Smtp Client
                SmtpClient SmtpMail = new SmtpClient(mSMTPServer, mSMTPPort);
                SmtpMail.Credentials = new NetworkCredential(mSMTPUsername, mSMTPPassword);
                SmtpMail.EnableSsl = mSMTPSSL;
                //SmtpMail.UseDefaultCredentials = false;

                #endregion

                if (isAsync)
                {
                    SmtpMail.SendCompleted += new SendCompletedEventHandler(SendCompletedCallback);
                    SmtpMail.SendAsync(Email, null);
                }
                else
                {
                    SmtpMail.Send(Email);
                    result.Success = true;
                }
            }
            catch (SmtpFailedRecipientsException ex)
            {
                result.Message = ErrorHelper.GetInnerException(ex).Message;
                result.Success = false;
            }
            catch (Exception ex)
            {
                result.Message = ErrorHelper.GetInnerException(ex).Message;
                result.Success = false;
            }

            return result;
        }

        /// <summary>
        /// 发送完成回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SendCompletedCallback(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            string log = string.Empty;
            if (e.Cancelled)
            {
                //输出日志
                log = string.Format("AsyncSendMail from ({0}) to ({1}), Send canceled.", mMailFrom, String.Join("|", mMailTo));
            }
            if (e.Error != null)
            {
                //输出日志
                log = string.Format("AsyncSendMail from ({1}) to ({2}), {3}.", DateTime.Now, mMailFrom, String.Join("|", mMailTo), e.Error.Message);
            }
            else
            {
                //输出日志
                log = string.Format("AsyncSendMail from ({1}) to ({2}), Send success.", DateTime.Now, mMailFrom, String.Join("|", mMailTo));
            }

            //写邮件发送日志
            SimpleLog.Instance.WriteLogForDir("Mail", string.Format("{0} subject: {1}", log, mMailSubject));
        }

        #endregion
    }

    #endregion
}

#region 附加内容

#region POP3 命令简介


/*

什么是 POP3
POP3 (Post Office Protocol 3) 即邮局协议的第 3 个版本,它规定怎样将个人计算机连接到 Internet 的邮件服务器和下载电子邮件的电子协议。它是因特网电子邮件的 第一个离线协议标准, POP3 允许用户从服务器上把邮件存储到本地主机（即自己的计算机）上,同时删除保存在邮件服务器上的邮件，而POP3服务器则是遵循 POP3 协议的接收邮件服务器，用来接收电子邮件的。


POP3 命令
POP3 命令包括：

USER username 认证用户名

PASS password 认证密码认证，认证通过则状态转换 
APOP name,digest 认可一种安全传输口令的办法，执行成功导致状态转换，请参见 RFC 1321 。

STAT 处理请求 server 回送邮箱统计资料，如邮件数、 邮件总字节数
UIDL n 处理 server 返回用于该指定邮件的唯一标识， 如果没有指定，返回所有的。

LIST n 处理 server 返回指定邮件的大小等 
RETR n 处理 server 返回邮件的全部文本 
DELE n 处理 server 标记删除，QUIT 命令执行时才真正删除
RSET 处理撤消所有的 DELE 命令 
TOP n,m 处理 返回 n 号邮件的前 m 行内容，m 必须是自然数 
NOOP 处理 server 返回一个肯定的响应 
QUIT 希望结束会话。如果 server 处于"处理" 状态，则现在进入"更新"状态，删除那些标记成删除的邮件。如果 server 处于"认可"状态，则结束会话时 server 不进入"更新"状态 。


使用 telnet 连接 Winmail Server 收信
例如：安装 Winmail 的邮件服务器 IP 是 192.168.0.1（蓝色字体内容由客户端输入，红色字体内容是服务返回的） 

telnet 119.119.119.212 110 ----------------------------- 使用 telnet 命令连接服务器 110 端口
Trying 119.119.119.212... ------------------------------ 正在连接服务器 110 端口
Connected to 119.119.119.212. -------------------------- 连接服务器 110 端口成功
+OK Winmail Mail Server POP3 ready
user username ------------------------------------------ 输入用户名, username 为具体的用户名

+OK ---------------------------------------------------- 执行命令成功
pass password ------------------------------------------ 输入用户密码，password 为具体的密码
+OK 2 messages ----------------------------------------- 密码认证通过 
(-ERR authorization failed ----------------------------- 密码认证失败)
stat --------------------------------------------------- 邮箱状态

+OK 2 6415 --------------------------------------------- 2 为该信箱总邮件数，6415 为总字节数 
list --------------------------------------------------- 列出每封邮件的字节数 
+OK ---------------------------------------------------- 执行命令成功，开始显示，左边为邮件的序号，右边为该邮件的大小 
1 537 -------------------------------------------------- 第 1 封邮件，大小为 537 字节 
2 5878 ------------------------------------------------- 第 2 封邮件，大小为 5878 字节 
.
top 1 -------------------------------------------------- 接收第 1 封邮件 
+OK ---------------------------------------------------- 接收成功, 返回第 1 封邮件头
Return-Path: <test1@look.com>
Delivered-To: test2@look.com
Received: (winmail server invoked for smtp delivery); Mon, 25 Oct 2004 14:24:27 +0800
From: test1@look.com
To: test2@look.com
Date: Mon, 25 Oct 2004 14:24:27 +0800
Subject: test mail 
.
retr 1 ------------------------------------------------- 接收第 1 封邮件 
+OK ---------------------------------------------------- 接收成功, 返回第 1 封邮件全部内容

Return-Path: <test1@look.com>
Delivered-To: test2@look.com
Received: (winmail server invoked for smtp delivery); Mon, 25 Oct 2004 14:24:27 +0800
From: test1@look.com
To: test2@look.com
Date: Mon, 25 Oct 2004 14:24:27 +0800
Subject: test mail 

Hi, test2 
This is a test mail, you don't reply it.

.
dele 1 ------------------------------------------------- 删除第 1 封邮件 
+OK ---------------------------------------------------- 删除成功 
dele 2 ------------------------------------------------- 删除第 2 封邮件 
+OK ---------------------------------------------------- 删除成功 
quit --------------------------------------------------- 结束会话 
+OK ---------------------------------------------------- 执行命令成功 


*/

#endregion

#endregion

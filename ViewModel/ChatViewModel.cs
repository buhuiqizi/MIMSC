﻿using MISMC.Model;
using MyMVVM;
using Newtonsoft.Json.Linq;
using SocketAsyncEventArgsOfficeDemo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MISMC.ViewModel
{
    class ChatViewModel : NotifyObject
    {
        public ChatViewModel()
        {
            Mess = "";
            isSend = "false";
            MessageMixGroup = new ObservableCollection<MessageMix>();
            this.DbMessCount = 0;
            //启动一个线程，这个线程负责更新聊天消息
            scanThread = new Thread(MessageUpdata);
            scanThread.Start();
        }

        public void MessageUpdata()
        {
            while (true)
            {

                Application.Current.Dispatcher.Invoke(
                new Action(() =>
                {
                    //从数据库中更新聊天消息
                    SqliteConnect.QueryMessage(this.NowName, this.UserName, this.FriendId, ref _messageMixGroup);
                })
                );

                Thread.Sleep(2000);
            }
        }

        public void  ChatSet(String nowname, String friendId, String UserName, String RealName, String Sex, String BirthDay, String Address, String Email, String PhoneNumber, String Remarks)
        {
            //当前用户名
            this.NowName = nowname;
            this.FriendId = friendId;
            //好友的名称
            this.UserName = UserName;
            this.RealName = RealName;
            this.Sex = Sex;
            this.BirthDay = BirthDay;
            this.Address = Address;
            this.Email = Email;
            this.PhoneNumber = PhoneNumber;
            this.Remark = Remarks;
        }

        //记录该好友消息表的消息总数，以此来确定是否有新的聊天消息
        public int DbMessCount;

        //扫描线程
        Thread scanThread;
        double oldHeight = 0;
        double newHeight = 0;

        private String nowname;
        public String NowName
        {
            get { return nowname; }
            set
            {
                if (nowname != value)
                {
                    nowname = value;
                    RaisePropertyChanged("NowName");
                }
            }
        }

        private String friendid;
        public String FriendId
        {
            get { return friendid; }
            set
            {
                if (friendid != value)
                {
                    friendid = value;
                    RaisePropertyChanged("FriendId");
                }
            }
        }

        private String username;
        public String UserName
        {
            get { return username; }
            set
            {
                if (username != value)
                {
                    username = value;
                    RaisePropertyChanged("UserName");
                }
            }
        }


        private String realname;
        public String RealName
        {
            get { return realname; }
            set
            {
                if (realname != value)
                {
                    realname = value;
                    RaisePropertyChanged("Realname");
                }
            }
        }

        private String sex;
        public String Sex
        {
            get { return sex; }
            set
            {
                if (sex != value)
                {
                    sex = value;
                    RaisePropertyChanged("Sex");
                }
            }
        }

        private String birthday;
        public String BirthDay
        {
            get { return birthday; }
            set
            {
                if (birthday != value)
                {
                    birthday = value;
                    RaisePropertyChanged("BirthDay");
                }
            }
        }

        private String address;
        public String Address
        {
            get { return address; }
            set
            {
                if (address != value)
                {
                    address = value;
                    RaisePropertyChanged("Address");
                }
            }
        }

        private String email;
        public String Email
        {
            get { return email; }
            set
            {
                if (email != value)
                {
                    email = value;
                    RaisePropertyChanged("Email");
                }
            }
        }

        private String phonenumber;
        public String PhoneNumber
        {
            get { return phonenumber; }
            set
            {
                if (phonenumber != value)
                {
                    phonenumber = value;
                    RaisePropertyChanged("PhoneNumber");
                }
            }
        }

        private String remark;
        public String Remark
        {
            get { return remark; }
            set
            {
                if (remark != value)
                {
                    remark = value;
                    RaisePropertyChanged("Remark");
                }
            }
        }

        //绑定聊天框的消息
        private String mess;
        public String Mess
        {
            get { return mess; }
            set
            {
                if (mess != value)
                {
                    mess = value;
                    RaisePropertyChanged("Mess");
                }
            }
        }

        private String issend;
        public String isSend
        {
            get { return issend; }
            set
            {
                if (issend != value)
                {
                    issend = value;
                    RaisePropertyChanged("isSend");
                }
            }
        }

        private ObservableCollection<MessageMix> _messageMixGroup;
        public ObservableCollection<MessageMix> MessageMixGroup
        {
            get
            {
                return _messageMixGroup;
            }
            set
            {
                _messageMixGroup = value;
                RaisePropertyChanged("MessageMixGroup");
            }
        }

        //消息发送函数
        private MyCommand btSendMessage;
        public MyCommand BtSendMessage
        {
            get
            {
                if (btSendMessage == null)
                    btSendMessage = new MyCommand(
                        new Action<object>(
                            o =>
                            {
                                //将消息发送给服务端
                                JObject obj = new JObject();
                                obj["Message"] = this.Mess;
                                String nowTime = DateTime.Now.ToString();
                                obj["MessageDate"] = nowTime;
                                obj["ReceiveId"] = FriendId;
                                String str = obj.ToString();
                                MClient mClient = MClient.CreateInstance();
                                mClient.SendChat(str);
                                //将消息保存到本地数据库
                                SqliteConnect.SaveChat(FriendId, this.Mess, nowTime);
                                //直接修改消息List
                                //这里直接追加到最后面是没有问题的
                                MessageMix message = new MessageMix();
                                MessageMixGroup.Add(message);
                                message.FriendId = FriendId;
                                message.Message = this.Mess;
                                message.MessageDate = nowTime;
                                message.FriendName = UserName;
                                message.UserName = NowName;
                                //因为是自己发给对方，所以消息要放到右边
                                message.Type = "Right";
                                //消息清空
                                this.Mess = "";
                            }));
                return btSendMessage;
            }
        }

        private MyCommand chatWindowClose;
        public MyCommand ChatWindowClose
        {
            get
            {
                if (chatWindowClose == null)
                    chatWindowClose = new MyCommand(
                        new Action<object>(
                            o =>
                            {
                                //退出扫描线程
                                this.scanThread.Abort();
                            }));
                return chatWindowClose;
            }
        }

        private MyCommand<TextBlock> tbMess;
        public MyCommand<TextBlock> TbMess
        {
            get
            {
                if (tbMess == null)
                    tbMess = new MyCommand<TextBlock>(
                            para =>
                            {
                                String splist = "";
                                para.Inlines.Clear();

                                if (Encoding.Default.GetByteCount(Mess) > 255)
                                {
                                    isSend = "false";
                                    splist = "长度为1-255个字节";
                                }
                                else if (Encoding.Default.GetByteCount(Mess) <= 0)
                                {
                                    isSend = "false";
                                    splist = "";
                                }
                                else
                                {
                                    isSend = "true";
                                    splist = "";
                                }
                                para.Inlines.Add(new Run(splist) { Foreground = Brushes.Red });
                                //this.RigistButtonCheck();
                            });
                return tbMess;
            }
        }

        private MyCommand<ScrollViewer> btGetOldMessage;
        public MyCommand<ScrollViewer> BtGetOldMessage
        {
            get
            {
                if (btGetOldMessage == null)
                    btGetOldMessage = new MyCommand<ScrollViewer>(
                        (
                            scorll =>
                            {
                                if (scorll.Visibility == Visibility.Visible)
                                {
                                    if (scorll.VerticalOffset == 0)
                                    {
                                                                              
                                        //第一次，更新oldHeight,而且更新聊天信息
                                        if (oldHeight == 0 && newHeight == 0)
                                        {
                                            oldHeight = scorll.ScrollableHeight;
                                            //更新旧的聊天信息
                                            SqliteConnect.QueryMessageOld(this.NowName, this.UserName, this.FriendId, ref _messageMixGroup);
                                        }
                                        else if (oldHeight != 0 && newHeight == 0)
                                        {
                                            newHeight = scorll.ScrollableHeight;

                                            //向下移动
                                            scorll.ScrollToVerticalOffset(newHeight - oldHeight);

                                            //MessageBox.Show("改变了位置");

                                            oldHeight = 0;
                                            newHeight = 0;
                                        }

                                    }
                                }
                                
                            }));
                return btGetOldMessage;
            }
        }
    }

    class MessageMix
    {
        public String FriendId { get; set; }
        public String FriendName{ get; set; }
        public String UserName { get; set; }
        public String Message { get; set; }
        public String MessageDate { get; set; }
        public String Type { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Server_正式版_
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //创建一个负责监听ip的socket
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);//需要三个参数：表示我的IP版本是v4  socket类型  你传输数据使用哪种协议
                                                                                                            //但此时我电脑的IP地址还未绑定到这个socket上，它现在作用是一个信号塔，电话还未连到移动的平台上
                                                                                                            //接下来，我要做的是把这个信号塔和电话绑定


                //既然要绑定，那么就必须保证有电话和电话号码
                //保证服务器有电话
                IPAddress ip = IPAddress.Any;  //new IPAddress(long.Parse(textBox1.Text));
                                               //保证电话能被打，就是有电话号码
                IPEndPoint iep = new IPEndPoint(ip, int.Parse(textBox2.Text));

                //把这个电话和电话号码上传到电话公司，让他们帮我们把电话 号码与信号塔绑定，以让信号塔监视电话
                socket.Bind(iep);
                //告诉大家我已经把我要的监听系统和电话绑定好，服务器已经练好了
                ShowMsg("监听成功");


                //下面就可以去监视是否有客户端请求来连接我的IP地址和端口号，如果有客户端想连接，那么服务器的信号塔（监视socket）就会把这个信号传给对方的手机，也就是建立一个通信机制
                //但是我在接收这个信号的时候，发现信号有点多，怎么办？让他们去排个队
                //设置个监听队列
                socket.Listen(10);


                Thread th = new Thread(Listen);

                th.IsBackground = true;
                th.Start(socket);
            }
            catch
            { }
        }

        /// <summary>
        /// 只是为了展示消息，把最终的监听和连接成果反馈
        /// </summary>
        /// <param name="str"></param>
        void ShowMsg(string str)
        {
            textBox3.AppendText(str + "\r\n");
        }

        //存储连接过的客户端的IP和端口号
        Dictionary<string, Socket> dicSocket = new Dictionary<string, Socket>();
        //负责通信的socket
        Socket socketSend;
        
        /// <summary>
        /// 接收信号，建立一个socket
        /// </summary>
        /// <param name="socket"></param>
        void Listen(Object o)
        {
            Socket socket = o as Socket;
            while (true)
            {
                try
                {
                    //信号塔接收到信号之后，建了一个通信的工具。负责通信的socket
                     socketSend = socket.Accept();
                    //将远程连接的IP和端口号存入集合
                    dicSocket.Add(socketSend.RemoteEndPoint.ToString(), socketSend);
                    //将远程IP存入下拉框中
                    comboBox1.Items.Add(socketSend.RemoteEndPoint.ToString());
                    ShowMsg(socketSend.RemoteEndPoint.ToString() + "\t连接成功");
                    
                    //建立一个新的线程，负责在客户端连接成功后，去接收客户端发来的消息
                    Thread th = new Thread(Send);
                    th.IsBackground = true;
                    th.Start(socketSend);
                }
                catch
                { }

            }
        }
        

        
       

        /// <summary>
        /// 不停的接收客户端发来的消息，并把这个消息反应在我的消息框中
        /// </summary>
        /// <param name="o"></param>
        void Send(Object o)
        {
            while (true)
            {
                try
                {
                    Socket socketSend = o as Socket;
                    //客户端连接成功后，服务器开始接收客户端发来的消息
                    byte[] buffer = new byte[1024 * 1024 * 5];
                    int r = socketSend.Receive(buffer);
                    if (r == 0)
                    {
                        break;
                    }
                    string str = Encoding.UTF8.GetString(buffer, 0, r);
                    string str2 = socketSend.RemoteEndPoint.ToString() + "\t" + str;
                    ShowMsg(str2);
                }
                catch
                { }
            }

        }


        /// <summary>
        /// 窗体加载的时候取消对跨线程的访问
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        /// <summary>
        /// 点击它的时候，服务器会把第二个文本框中的消息发给客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            string str = textBox4.Text;
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(str);
            List<byte> list = new List<byte>();
            list.Add(0);
            list.AddRange(buffer);
            byte[] bytes = list.ToArray();
            //socketSend.Send(buffer);
            
            //获得用户在下拉框中选中的IP地址与端口号
            string ip = comboBox1.SelectedItem.ToString();
            dicSocket[ip].Send(bytes);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "所有文件|*.*";
            ofd.InitialDirectory = @"F:\";
            ofd.Multiselect = true;
            ofd.ShowDialog();
            string path = ofd.FileName;
            textBox5.Text = path;


            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (FileStream fs = new FileStream(textBox5.Text, FileMode.OpenOrCreate, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024 * 1024 * 5];
                    int r = fs.Read(buffer, 0, buffer.Length);
                    List<byte> list = new List<byte>();
                    list.Add(1);
                    list.AddRange(buffer);
                    byte[] newBuffer = list.ToArray();

                    dicSocket[comboBox1.SelectedItem.ToString()].Send(newBuffer, 0, r+1, SocketFlags.None);
                }
            }
            catch
            { }
        }

        /// <summary>
        /// 发送震动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            byte[] buffer = new byte[1];
            buffer[0] = 2;
            dicSocket[comboBox1.SelectedItem.ToString()].Send(buffer, 0, 1, SocketFlags.None);
        }
    }
}

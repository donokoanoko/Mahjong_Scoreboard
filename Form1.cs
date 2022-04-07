using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace Mahjong_Scoreboard
{
    public partial class mahjong_scoreboard : Form
    {
        int status = 0;
        const int message_buffer_size = 256;
        ClientWebSocket ws = null;
        string top_player = null;
        string score_txt_pass;

        public mahjong_scoreboard()
        {
            InitializeComponent();
        }

        //start
        private void mahjong_scoreboard_Load(object sender, EventArgs e)
        {
            ws_server_boot(ws_address.Text);
            ws_client(ws_address.Text.Replace("http", "ws"));

            player_a.Text = a_name.Text;
            player_b.Text = b_name.Text;
            player_c.Text = c_name.Text;
            player_d.Text = d_name.Text;

            location_a.SelectedIndex = Properties.Settings.Default.player_a_location;
            location_b.SelectedIndex = Properties.Settings.Default.player_b_location;
            location_c.SelectedIndex = Properties.Settings.Default.player_c_location;
            location_d.SelectedIndex=Properties.Settings.Default.player_d_location;

            a_color.BackColor = Properties.Settings.Default.player_a_color;
            b_color.BackColor = Properties.Settings.Default.player_b_color;
            c_color.BackColor = Properties.Settings.Default.player_c_color;
            d_color.BackColor = Properties.Settings.Default.player_d_color;

            scoreboard.Rows.Add(player_a.Text, 0, 0, 0, 0, 0);
            scoreboard.Rows.Add(player_b.Text, 0, 0, 0, 0, 0);
            scoreboard.Rows.Add(player_c.Text, 0, 0, 0, 0, 0);
            scoreboard.Rows.Add(player_d.Text, 0, 0, 0, 0, 0);

            if (!yonma.Checked)
            {
                sanma.Checked = true;
            }

            if (display_score_txt_pass.Text.Length == 0)
            {
                score_txt_pass = System.Environment.CurrentDirectory + "\\mahjong_score.txt";
            }
            else
            {
                score_txt_pass = display_score_txt_pass.Text;
            }
            send_message();
        }


        //main
        private void start_Click(object sender, EventArgs e)
        {
            switch (status)
            {
                case 0:
                case 1:
                    start.Text = "終了";
                    status = 2;
                    send_message();
                    break;
                case 2:
                    int[] ranking = new int[4];
                    List<int> used = new List<int>();

                    start.Text = "開始";
                    status = 1;

                    scoreboard[1, 0].Value = int.Parse(scoreboard[1, 0].Value.ToString()) + (int)score_a.Value;
                    scoreboard[1, 1].Value = int.Parse(scoreboard[1, 1].Value.ToString()) + (int)score_b.Value;
                    scoreboard[1, 2].Value = int.Parse(scoreboard[1, 2].Value.ToString()) + (int)score_c.Value;
                    scoreboard[1, 3].Value = int.Parse(scoreboard[1, 3].Value.ToString()) + (int)score_d.Value;

                    score_a.Value = 0;
                    score_b.Value = 0;
                    score_c.Value = 0;
                    score_d.Value = 0;

                    scoreboard[6, 0].Value = int.Parse(scoreboard[1, 0].Value.ToString()) + int.Parse(scoreboard[3, 0].Value.ToString()) - int.Parse(scoreboard[5, 0].Value.ToString());
                    scoreboard[6, 1].Value = int.Parse(scoreboard[1, 1].Value.ToString()) + int.Parse(scoreboard[3, 1].Value.ToString()) - int.Parse(scoreboard[5, 1].Value.ToString());
                    scoreboard[6, 2].Value = int.Parse(scoreboard[1, 2].Value.ToString()) + int.Parse(scoreboard[3, 2].Value.ToString()) - int.Parse(scoreboard[5, 2].Value.ToString());
                    scoreboard[6, 3].Value = int.Parse(scoreboard[1, 3].Value.ToString()) + int.Parse(scoreboard[3, 3].Value.ToString()) - int.Parse(scoreboard[5, 3].Value.ToString());

                    if (yonma.Checked)
                    {

                        int[] tmp = { int.Parse(scoreboard[6, 0].Value.ToString()), int.Parse(scoreboard[6, 1].Value.ToString()), int.Parse(scoreboard[6, 2].Value.ToString()), int.Parse(scoreboard[6, 3].Value.ToString()) };

                        Array.Sort(tmp);
                        for (int i = 0; i <= 3; i++)
                        {
                            for (int j = 0; j <= 3; j++)
                            {
                                if (tmp[3 - i] == int.Parse(scoreboard[6, j].Value.ToString()) && !used.Contains(j))
                                {
                                    ranking[i] = j;
                                    used.Add(j);
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        int[] tmp = { int.Parse(scoreboard[6, 0].Value.ToString()), int.Parse(scoreboard[6, 1].Value.ToString()), int.Parse(scoreboard[6, 2].Value.ToString()) };
                        Array.Sort(tmp);
                        for (int i = 0; i <= 2; i++)
                        {
                            for (int j = 0; j <= 2; j++)
                            {
                                if (tmp[2 - i] == int.Parse(scoreboard[6, j].Value.ToString()) && !used.Contains(j))
                                {
                                    ranking[i] = j;
                                    used.Add(j);
                                    break;
                                }
                            }
                        }
                    }

                    string score = style.Text;
                    score = score.Replace("{player_A}", scoreboard[0, 0].Value.ToString());
                    score = score.Replace("{player_B}", scoreboard[0, 1].Value.ToString());
                    score = score.Replace("{player_C}", scoreboard[0, 2].Value.ToString());

                    score = score.Replace("{player_1st}", scoreboard[0, ranking[0]].Value.ToString());
                    score = score.Replace("{player_2nd}", scoreboard[0, ranking[1]].Value.ToString());
                    score = score.Replace("{player_3rd}", scoreboard[0, ranking[2]].Value.ToString());

                    score = score.Replace("{score_A}", scoreboard[6, 0].Value.ToString());
                    score = score.Replace("{score_B}", scoreboard[6, 1].Value.ToString());
                    score = score.Replace("{score_C}", scoreboard[6, 2].Value.ToString());

                    score = score.Replace("{score_1st}", scoreboard[6, ranking[0]].Value.ToString());
                    score = score.Replace("{score_2nd}", scoreboard[6, ranking[1]].Value.ToString());
                    score = score.Replace("{score_3rd}", scoreboard[6, ranking[2]].Value.ToString());

                    if (yonma.Checked)
                    {
                        score = score.Replace("{player_D}", scoreboard[0, 3].Value.ToString());
                        score = score.Replace("{player_4th}", scoreboard[0, ranking[3]].Value.ToString());
                        score = score.Replace("{score_D}", scoreboard[6, 3].Value.ToString());
                        score = score.Replace("{score_4th}", scoreboard[6, ranking[3]].Value.ToString());
                    }
                    else
                    {
                        score = score.Replace("{player_D}", "");
                        score = score.Replace("{player_4th}", "");
                        score = score.Replace("{score_D}", "");
                        score = score.Replace("{score_4th}", "");
                    }

                    obs_txt.Text = score;

                    StreamWriter score_obs = new StreamWriter(score_txt_pass, false, Encoding.GetEncoding("utf-8"));
                    score_obs.WriteLine(score);
                    score_obs.Close();

                    top_player = scoreboard[0, ranking[0]].Value.ToString();
                    send_message();
                    break;
            }
        }
        //reset
        private void reset_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= 3; i++)
            {
                for (int j = 1; j <= 6; j++)
                {
                    scoreboard[j, i].Value = 0;
                }
            }

            obs_txt.Text = "";

            StreamWriter score_obs = new StreamWriter(score_txt_pass, false, Encoding.GetEncoding("utf-8"));
            score_obs.WriteLine("");
            score_obs.Close();

            top_player = null;

            start.Text = "開始";
            status = 1;
            send_message();
        }

        //location
        private void duplication_check()
        {
            if (yonma.Checked)
            {
                ComboBox[] location = new ComboBox[] { location_a, location_b, location_c, location_d };
                for (int i = 0; i <= 3; i++)
                {
                    for (int j = 0; j <= 3; j++)
                    {
                        if (location[i].SelectedIndex == location[j].SelectedIndex && i != j)
                        {
                            location[i].BackColor = Color.Yellow;
                            break;
                        }
                        else
                        {
                            location[i].BackColor = Color.White;
                        }
                    }
                }
            }
            else
            {
                ComboBox[] location = new ComboBox[] { location_a, location_b, location_c };
                for (int i = 0; i <= 2; i++)
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        if (location[i].SelectedIndex == location[j].SelectedIndex && i != j)
                        {
                            location[i].BackColor = Color.Yellow;
                            break;
                        }
                        else
                        {
                            location[i].BackColor = Color.White;
                        }
                    }
                }
            }
            send_message();
        }
        private void location_a_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.player_a_location = location_a.SelectedIndex;
            duplication_check();
        }
        private void location_b_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.player_b_location = location_b.SelectedIndex;
            duplication_check();
        }
        private void location_c_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.player_c_location = location_c.SelectedIndex;
            duplication_check();
        }
        private void location_d_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.player_d_location = location_d.SelectedIndex;
            duplication_check();
        }

        //bonus&onus
        private void bonus_a_Click(object sender, EventArgs e)
        {
            scoreboard[2, 0].Value = int.Parse(scoreboard[2, 0].Value.ToString()) + 1;
            scoreboard[3, 0].Value = int.Parse(scoreboard[3, 0].Value.ToString()) + (int)bonus_point_a.Value;
        }
        private void onus_a_Click(object sender, EventArgs e)
        {
            scoreboard[4, 0].Value = int.Parse(scoreboard[4, 0].Value.ToString()) + 1;
            scoreboard[5, 0].Value = int.Parse(scoreboard[5, 0].Value.ToString()) + (int)onus_point_a.Value;
        }
        private void bonus_b_Click(object sender, EventArgs e)
        {
            scoreboard[2, 1].Value = int.Parse(scoreboard[2, 1].Value.ToString()) + 1;
            scoreboard[3, 1].Value = int.Parse(scoreboard[3, 1].Value.ToString()) + (int)bonus_point_b.Value;
        }
        private void onus_b_Click(object sender, EventArgs e)
        {
            scoreboard[4, 1].Value = int.Parse(scoreboard[4, 1].Value.ToString()) + 1;
            scoreboard[5, 1].Value = int.Parse(scoreboard[5, 1].Value.ToString()) + (int)onus_point_b.Value;
        }
        private void bonus_c_Click(object sender, EventArgs e)
        {
            scoreboard[2, 2].Value = int.Parse(scoreboard[2, 2].Value.ToString()) + 1;
            scoreboard[3, 2].Value = int.Parse(scoreboard[3, 2].Value.ToString()) + (int)bonus_point_c.Value;
        }
        private void onus_c_Click(object sender, EventArgs e)
        {
            scoreboard[4, 2].Value = int.Parse(scoreboard[4, 2].Value.ToString()) + 1;
            scoreboard[5, 2].Value = int.Parse(scoreboard[5, 2].Value.ToString()) + (int)onus_point_c.Value;
        }
        private void bonus_d_Click(object sender, EventArgs e)
        {
            scoreboard[2, 3].Value = int.Parse(scoreboard[2, 3].Value.ToString()) + 1;
            scoreboard[3, 3].Value = int.Parse(scoreboard[3, 3].Value.ToString()) + (int)bonus_point_d.Value;
        }
        private void onus_d_Click(object sender, EventArgs e)
        {
            scoreboard[4, 3].Value = int.Parse(scoreboard[4, 3].Value.ToString()) + 1;
            scoreboard[5, 3].Value = int.Parse(scoreboard[5, 3].Value.ToString()) + (int)onus_point_d.Value;
        }



        //setting

        //game mode
        private void yonma_CheckedChanged(object sender, EventArgs e)
        {
            D.Visible = yonma.Checked;
            player_d.Visible = yonma.Checked;
            score_d.Visible = yonma.Checked;
            location_d.Visible = yonma.Checked;
            bonus_point_d.Visible = yonma.Checked;
            bonus_d.Visible = yonma.Checked;
            onus_d.Visible = yonma.Checked;
            onus_point_d.Visible = yonma.Checked;
            scoreboard.Rows[3].Visible = yonma.Checked;
            send_message();
            duplication_check();
        }
        //crown view
        private void crown_CheckedChanged(object sender, EventArgs e)
        {
            send_message();
        }

        //player_setting
        private void a_name_TextChanged(object sender, EventArgs e)
        {
            player_a.Text = a_name.Text;
            scoreboard[0, 0].Value = a_name.Text;
            send_message();
        }
        private void a_color_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                a_color.BackColor = colorDialog1.Color;
                Properties.Settings.Default.player_a_color = colorDialog1.Color;
            }
            send_message();
        }
        private void b_name_TextChanged(object sender, EventArgs e)
        {
            player_b.Text = b_name.Text;
            scoreboard[0, 1].Value = b_name.Text;
            send_message();
        }
        private void b_color_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                b_color.BackColor = colorDialog1.Color;
                Properties.Settings.Default.player_b_color = colorDialog1.Color;
            }
            send_message();
        }
        private void c_name_TextChanged(object sender, EventArgs e)
        {
            player_c.Text = c_name.Text;
            scoreboard[0, 2].Value = c_name.Text;
            send_message();
        }
        private void c_color_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                c_color.BackColor = colorDialog1.Color;
                Properties.Settings.Default.player_c_color = colorDialog1.Color;
            }
            send_message();
        }
        private void d_name_TextChanged(object sender, EventArgs e)
        {
            player_d.Text = d_name.Text;
            scoreboard[0, 3].Value = d_name.Text;
            send_message();
        }
        private void d_color_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                d_color.BackColor = colorDialog1.Color;
                Properties.Settings.Default.player_d_color = colorDialog1.Color;
            }
            send_message();
        }


        //ws
        //server
        static List<WebSocket> client = new List<WebSocket>();
        static async void ws_server_boot(string address)
        {
            var http_listener = new HttpListener();
            http_listener.Prefixes.Add(address);
            http_listener.Start();

            while (true)
            {
                var listener = await http_listener.GetContextAsync();
                if (listener.Request.IsWebSocketRequest)
                {
                    ws_server(listener);
                }
                else
                {
                    listener.Response.StatusCode = 400;
                    listener.Response.Close();
                }
            }
        }
        static async void ws_server(HttpListenerContext listener)
        {
            //get ws object
            var ws = (await listener.AcceptWebSocketAsync(subProtocol: null)).WebSocket;

            client.Add(ws);

            //message send and receive
            while (ws.State == WebSocketState.Open)
            {
                try
                {
                    var buff = new ArraySegment<byte>(new byte[1024]);

                    //wait message
                    var ret = await ws.ReceiveAsync(buff, System.Threading.CancellationToken.None);

                    //send message
                    if (ret.MessageType == WebSocketMessageType.Text)
                    {
                        Parallel.ForEach(client, p => p.SendAsync(new ArraySegment<byte>(buff.Take(ret.Count).ToArray()), WebSocketMessageType.Text, true, System.Threading.CancellationToken.None));
                    }
                }
                catch
                {
                    break;
                }
            }
        }

        //client
        private async void ws_client(string address)
        {
            if(ws==null)
            {
                ws = new ClientWebSocket();
            }
            if(ws.State!=WebSocketState.Open)
            {
                await ws.ConnectAsync(new Uri(address), CancellationToken.None);
            }
        }
        private void send_message()
        {

            string message = "{";
            message += "\"status\" : \"" + status + "\",";

            if (yonma.Checked)
            {
                ComboBox[] location = new ComboBox[] { location_a, location_b, location_c, location_d };
                TextBox[] players = new TextBox[] { a_name, b_name, c_name, d_name };
                PictureBox[] color = new PictureBox[] { a_color, b_color, c_color, d_color };
                for (int i = 0; i <= 3; i++)
                {
                    switch (location[i].SelectedIndex)
                    {
                        case 0:
                            message += "\"jicha\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                        case 1:
                            message += "\"shimotya\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                        case 2:
                            message += "\"toimen\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                        case 3:
                            message += "\"kamitya\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                    }
                    if(i!=3)
                    {
                        message += "\"},";
                    }
                    else
                    {
                        message += "\"}";
                    }
                }
            }
            else
            {
                ComboBox[] location = new ComboBox[] { location_a, location_b, location_c };
                TextBox[] players = new TextBox[] { a_name, b_name, c_name };
                PictureBox[] color = new PictureBox[] { a_color, b_color, c_color };
                for (int i = 0; i <= 2; i++)
                {
                    switch (location[i].SelectedIndex)
                    {
                        case 0:
                            message += "\"jicha\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                        case 1:
                            message += "\"shimotya\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                        case 2:
                            message += "\"toimen\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                        case 3:
                            message += "\"kamitya\" : {";
                            message += "\"name\" : \"" + players[i].Text + "\",";
                            message += "\"color\" : \"" + ColorTranslator.ToHtml(color[i].BackColor);
                            break;
                    }
                    if (i != 2)
                    {
                        message += "\"},";
                    }
                    else
                    {
                        message += "\"}";
                    }
                }
            }
            message += "}";
            
            if (crown.Checked && status != 0 && top_player != null)
            {
                message = message.Replace(top_player, "👑" + top_player);
            }

            var buff = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            if(ws.State==WebSocketState.Open)
            {
                ws.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        //shutdown
        private void mahjong_scoreboard_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

  
    }
}



using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Hive;
using Photon.Hive.Plugin;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace TestPlugin
{
    public class RaiseEventTestPlugin : PluginBase
    {
        private string recvdMessage;
        private string connStr;
        private MySqlConnection conn;
        bool isRegistered;
        //Photon.SocketServer.Protocol.TryRegisterCustomType(typeof(MyCustomType), myCustomTypeCode, MyCustomType.Serialize, MyCustomType.Deserialize);

        public string ServerString
        {
            get;
            private set;
        }
        public int CallsCount
        {
            get;
            private set;
        }
        public RaiseEventTestPlugin()
        {
            isRegistered = Photon.SocketServer.Protocol.TryRegisterCustomType(typeof(MyCustomType), 1, MyCustomType.Serialize, MyCustomType.Deserialize);

            this.UseStrictMode = true;
            this.ServerString = "ServerMessage";
            this.CallsCount = 0;
            
            // --- Connect to MySQL.
            ConnectToMySQL();
        }
        public override string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            try
            {
                base.OnRaiseEvent(info);

                //if (isRegistered)
                //    //Console.WriteLine("~ Register COMPLETED ~");
                //    PluginHost.LogDebug("~ Register COMPLETED ~");
                //else
                //    //Console.WriteLine("~ Register FAILED ~");
                //    PluginHost.LogDebug("~ Register FAILED ~");
            }
            catch (Exception e)
            {
                this.PluginHost.BroadcastErrorInfoEvent(e.ToString(), info);
                return;
            }

            switch (info.Request.EvCode)
            {
                case 1:
                    {
                        //return msg (0 - fail, 1 - create, 2 - success)
                        recvdMessage = Encoding.Default.GetString((byte[])info.Request.Data);
                        //string playerName = Encoding.Default.GetString((byte[])info.Request.Data);

                        string playerName = GetStringDataFromMessage("PlayerName");
                        string playerPassword = GetStringDataFromMessage("Password");
                        string ReturnMessage = "2"; 

                        if (!CheckUserDatabase(playerName, playerPassword)) //first check to see if login fail or there exist not such user
                        {
                            if (!CheckForUser(playerName)) // check if user exist (true->password failed)(false->user does not exist)
                            {   //create
                                string sql = "INSERT INTO users (name,password,date_created) VALUES('" + playerName + "', '" + playerPassword + "', now())";
                                MySqlCommand cmd = new MySqlCommand(sql, conn);
                                cmd.ExecuteNonQuery();
                                ReturnMessage = "1";
                            }
                            else //failed
                                ReturnMessage = "0";

                        }
                        
                        //++this.CallsCount;
                        //int cnt = this.CallsCount;
                        //string ReturnMessage = info.Nickname + " clicked the button. Now the count is " + cnt.ToString();                       
                         this.PluginHost.BroadcastEvent(target: ReciverGroup.Group, senderActor: 0, targetGroup: 1, data: new Dictionary<byte, object>() { {
                        (byte)245, ReturnMessage } }, evCode: info.Request.EvCode, cacheOp: 0);

                        //this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: info.Request.EvCode,
                        //    data: new Dictionary<byte, object>() { { (byte)245, ReturnMessage } }, cacheOp: 0);
                    }
                    break;

                case 2:
                    {
                        recvdMessage = Encoding.Default.GetString((byte[])info.Request.Data);

                        string playerName = GetStringDataFromMessage("PlayerName");           
                        string json = GetStringDataFromMessage("Json");
                        PluginHost.LogDebug(json);
                        string itemName = GetStringDataFromMessage("ItemName");
                        string pos = GetStringDataFromMessage("Pos");

                        if (CheckForUser(playerName)) // check if user exist in db, if exist take data
                        {

                            //string sql = "SELECT name, data FROM users WHERE name = '" + playerName + "' AND data = '" + _password + "'";
                            //MySqlCommand cmd = new MySqlCommand(sql, conn);
                            //cmd.Parameters.Add(new MySqlParameter("name", _username));
                            //cmd.Parameters.Add(new MySqlParameter("password", _password));
                            //MySqlDataReader rdr = cmd.ExecuteReader();
                            //bool bLogin = rdr.HasRows;
                            //rdr.Close();


                            //string sql = "UPDATE users SET data = '" + json + "' WHERE name = '" + playerName + "' AND data IS NULL";
                            //PluginHost.LogDebug(sql);
                            //MySqlCommand cmd = new MySqlCommand(sql, conn);
                            //int rowAffected = cmd.ExecuteNonQuery();

                            //PluginHost.LogDebug("Updated data");

                            //if(rowAffected > 0)
                            //{
                            //    sql = "UPDATE users SET data = '" + json + "' WHERE name = '" + playerName + "'"
                            //}

                            string sql = "UPDATE users SET data = '" + json + "', pos = '"+ pos +"', item = '"+ itemName +"' WHERE name = '" + playerName + "'";
                            PluginHost.LogDebug(sql);
                            MySqlCommand cmd = new MySqlCommand(sql, conn);
                            cmd.ExecuteNonQuery();

                            PluginHost.LogDebug("Updated data");
                        }
                    }
                    break;

                case 3: // Get json data
                    {
                        recvdMessage = Encoding.Default.GetString((byte[])info.Request.Data);
                        string playerName = GetStringDataFromMessage("PlayerName");
                        string json = "";
                        string pos = "";
                        string item = "";

                        string sql = "SELECT data, pos, item FROM users WHERE name = '" + playerName + "'";
                        MySqlCommand cmd = new MySqlCommand(sql, conn);
                        PluginHost.LogDebug(playerName);
                        //cmd.Parameters.Add(new MySqlParameter("name", playerName));
                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {
                            while(rdr.Read())
                            {
                                json = rdr["data"].ToString();
                                pos = rdr["pos"].ToString();
                                item = rdr["item"].ToString();
                            }

                            PluginHost.LogDebug(json);
                            rdr.Close();
                        }

                        json = json.Trim('{','}');
                        pos = pos.Trim('{', '}');

                        if (item == null || item.Length <= 0)
                            item = "";

                        item = "\"" + item + "\"";

                        string itemPrefix = @"""item"":";
                        string jsonData = "{" + json + "," + pos + "," + itemPrefix + item + "}";

                        this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, data: new Dictionary<byte, object>() { {
                        (byte)245, jsonData } }, evCode: info.Request.EvCode, cacheOp: 0);
                    }
                    break;
            }

            #region Commented_Out
            //try
            //{
            //    base.OnRaiseEvent(info);
            //}
            //catch (Exception e)
            //{
            //    this.PluginHost.BroadcastErrorInfoEvent(e.ToString(), info);
            //    return;
            //}

            //if (info.Request.EvCode == 1)
            //{
            //    recvdMessage = Encoding.Default.GetString((byte[])info.Request.Data);
            //    //string playerName = Encoding.Default.GetString((byte[])info.Request.Data);

            //    string playerName = GetStringDataFromMessage("PlayerName");
            //    string playerPassword = GetStringDataFromMessage("Password");
            //    string ReturnMessage = "Login in success";

            //    if (!CheckUserDatabase(playerName, playerPassword)) //first check to see if login fail or there exist not such user
            //    {
            //        if (!CheckForUser(playerName)) // check if user exist (true->password failed)(false->user does not exist)
            //        {
            //            string sql = "INSERT INTO users (name,password,date_created) VALUES('" + playerName + "', '" + playerPassword + "', now())";
            //            MySqlCommand cmd = new MySqlCommand(sql, conn);
            //            cmd.ExecuteNonQuery();
            //            ReturnMessage = "New account created!";
            //        }
            //        else
            //            ReturnMessage = "Password is wrong please try again!";
    
            //    }

            //    //++this.CallsCount;
            //    //int cnt = this.CallsCount;
            //    //string ReturnMessage = info.Nickname + " clicked the button. Now the count is " + cnt.ToString();
            //    // this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, data: new Dictionary<byte, object>() { {
            //    //(byte)245, ReturnMessage } }, evCode: info.Request.EvCode, cacheOp: 0);

            //    this.PluginHost.BroadcastEvent(target: ReciverGroup.All, senderActor: 0, targetGroup: 0, evCode: info.Request.EvCode, 
            //        data: new Dictionary<byte, object>() { { (byte)245, ReturnMessage } }, cacheOp: 0);
            //}
            #endregion
        }

        public void ConnectToMySQL()
        {
            // Connect to MySQL
            //  port 3306
            connStr = "server=localhost;user=root;database=photon;port=3306;password=161692w";
            conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void DisconnectFromMySQL()
        {
            conn.Close();
        }

        string GetStringDataFromMessage(string key)
        {
            int start, end;
            string temp = recvdMessage;
            start = end = 0;
            PluginHost.LogDebug("enter with key: " + key + "    temp: " + temp);

            if (temp.Contains(key))
            {
                if (key == "PlayerName")
                {
                    start = temp.IndexOf("=") + 1;
                    if(temp.Contains("/"))
                        end = temp.IndexOf("/");
                    else
                        end = temp.Length;
                    //Console.WriteLine("Start: " + start.ToString() + "\nEnd: " + end.ToString());
                }
                else if (key == "Password")
                {
                    start = temp.LastIndexOf("=") + 1;
                    end = temp.Length;
                    //Console.WriteLine("Start: " + start.ToString() + "\nEnd: " + end.ToString());
                }
                else if (key == "Json")
                {
                    start = temp.IndexOf("Json") + key.Length + 1;
                    temp = temp.Substring(start, temp.Length - start);
                    start = 0;
                    end = temp.IndexOf("/"); // find ,
                    PluginHost.LogDebug("Json_Start:" + start);
                    PluginHost.LogDebug("Json_End:" + end);
                    //end = temp.Length;
                }
                else if(key == "ItemName")
                {
                    start = temp.IndexOf("ItemName") + key.Length + 1;
                    temp = temp.Substring(start, temp.Length - start);
                    PluginHost.LogDebug("iName_Start:" + start);
                    start = 0;
                    end = temp.IndexOf("/");
                    PluginHost.LogDebug("iName_End:" + end);
                }
                else if (key == "Pos")
                {
                    start = temp.IndexOf("Pos") + key.Length + 1;
                    end = temp.Length;
                }

                return temp.Substring(start, end - start);
            }

            return "";
        }

        bool CheckForUser(string _username)
        {
            string sql = "SELECT name FROM users WHERE name = '" + _username + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add(new MySqlParameter("name", _username));
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool bLogin = rdr.HasRows;
            rdr.Close();

            return bLogin;
        }

        bool CheckUserDatabase(string _username, string _password)
        {
            string sql = "SELECT name, password FROM users WHERE name = '" + _username + "' AND password = '" + _password + "'";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add(new MySqlParameter("name", _username));
            cmd.Parameters.Add(new MySqlParameter("password", _password));
            MySqlDataReader rdr = cmd.ExecuteReader();
            bool bLogin = rdr.HasRows;
            rdr.Close();

            return bLogin;
        }

    }
}
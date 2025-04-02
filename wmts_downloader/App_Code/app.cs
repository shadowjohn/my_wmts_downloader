using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace wmts_downloader.App_Code
{
    public class App
    {
        Program theform = null;
        public App(Program form)
        {
            theform = form;
        }
        public void input_vertify(string[] args)
        {
            // 驗證輸入參數，與設定參數
            // 可為 test 載入預設值
            // 或 -url -ltx -lty -rbx -rby -sz -ez -f -o
            // -url 網址
            // -ltx 左上角 X
            // -lty 左上角 Y
            // -rbx 右下角 X
            // -rby 右下角 Y
            // -sz 開始層級
            // -ez 結束層級
            // -thread thread 數量
            // -f 輸出格式
            // -o 輸出目錄
            if (args.Length == 0)
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
            else
            {
                if (args.Length == 1 && args[0] == "test")
                {
                    //預設值
                    theform.URL = "https://c.tile.openstreetmap.org/${z}/${x}/${y}.png";
                    theform.START_LEVEL = 0;
                    theform.END_LEVEL = 15;
                    theform.FORMAT = "DIR";
                    theform.OUTPUT_PATH = "C:\\temp\\osm";
                }
                else
                {
                    // 解 -url 這些參數
                    for (int i = 0; i < args.Length; i++)
                    {
                        if (args[i] == "-url")
                        {
                            theform.URL = args[i + 1];
                        }
                        if (args[i] == "-ltx")
                        {
                            theform.p3826["LT_X"] = Convert.ToDouble(args[i + 1]);
                        }
                        if (args[i] == "-lty")
                        {
                            theform.p3826["LT_Y"] = Convert.ToDouble(args[i + 1]);
                        }
                        if (args[i] == "-rbx")
                        {
                            theform.p3826["RB_X"] = Convert.ToDouble(args[i + 1]);
                        }
                        if (args[i] == "-rby")
                        {
                            theform.p3826["RB_Y"] = Convert.ToDouble(args[i + 1]);
                        }
                        if (args[i] == "-sz")
                        {
                            theform.START_LEVEL = Convert.ToInt32(args[i + 1]);
                        }
                        if (args[i] == "-ez")
                        {
                            theform.END_LEVEL = Convert.ToInt32(args[i + 1]);
                        }
                        if (args[i] == "-thread")
                        {
                            theform.THREAD_workers = Convert.ToInt32(args[i + 1]);
                        }
                        if (args[i] == "-f")
                        {
                            theform.FORMAT = args[i + 1].ToUpper();
                        }
                        if (args[i] == "-o")
                        {
                            theform.OUTPUT_PATH = args[i + 1];
                        }
                    }
                }
            }
            //判斷是不是真的 3826
            if (theform.p3826["LT_X"] < 300)
            {
                // 使用者已輸入 4326
                theform.p4326 = new Dictionary<string, double>(theform.p3826); // copy 原本的
                theform.p3826 = theform.my.p4326_to_p3826(theform.p3826);
            }

            // 檢查 FORMAT 格式
            if (theform.FORMAT != "DIR" && theform.FORMAT != "ZIP" && theform.FORMAT != "SQLITE")
            {
                theform.my.echo("FORMAT: " + theform.FORMAT + " is not DIR or ZIP or SQLITE...");
                theform.my.exit();
            }
            // 路徑修正 \\ 變 \
            theform.OUTPUT_PATH = theform.OUTPUT_PATH.Replace("\\\\", "\\");

            // 繼續檢查格式
            theform.my.echo("theform.FORMAT: " + theform.FORMAT + "................");
            switch (theform.FORMAT)
            {
                case "DIR":
                    // 看 output 是否是檔案
                    if (theform.my.is_file(theform.OUTPUT_PATH))
                    {
                        theform.my.echo("OUTPUT_PATH: " + theform.OUTPUT_PATH + " is not path...");
                        theform.my.exit();
                    }
                    break;
                case "ZIP":
                    // 檢查是否已存在，如果有的話，要檢查是否 zip 格式
                    if (theform.my.is_file(theform.OUTPUT_PATH))
                    {
                        if (!theform.my.is_zip(theform.OUTPUT_PATH))
                        {
                            theform.my.echo("OUTPUT_PATH: " + theform.OUTPUT_PATH + " is not zip file...");
                            theform.my.exit();
                        }
                    }
                    break;
                case "SQLITE":
                    // 檢查是否已存在，如果有的話，要檢查是否 sqlite 格式
                    {
                        if (theform.my.is_file(theform.OUTPUT_PATH))
                        {
                            if (!theform.my.is_sqlite(theform.OUTPUT_PATH))
                            {
                                theform.my.echo("OUTPUT_PATH: " + theform.OUTPUT_PATH + " is not sqlite file...");
                                theform.my.exit();
                            }
                        }
                        // 檢查有沒有 tiles 這個 table ，且要有 x y z tile_data
                        string SQL = "SELECT name FROM sqlite_master WHERE type='table' AND name='tiles'";
                        if (theform.my.sqliteFile_selectSQL_SAFE(theform.OUTPUT_PATH, SQL).Rows.Count == 0)
                        {
                            // 建立 table tiles
                            SQL = @"
                                CREATE TABLE `tiles` (
                                    `x` INTEGER, 
                                    `y` INTEGER, 
                                    `z` INTEGER, 
                                    `tile_data` BLOB, 
                                    PRIMARY KEY (x, y, z)
                                )
                            ";
                            theform.my.sqliteFile_execSQL_SAFE(theform.OUTPUT_PATH, SQL);
                        }
                    }
                    break;
            }
            // 檢查 URL
            fix_URL();
        }
        public void fix_URL()
        {
            //處理 URL 問題
            theform.URL = theform.URL.Replace("$", ""); //osm $ 的問題
            theform.URL = Regex.Replace(theform.URL, "{z}", "{TileMatrix}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{x}", "{TileCol}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{y}", "{TileRow}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{TileMatrixSet}", "GoogleMapsCompatible", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{Style}", "default", RegexOptions.IgnoreCase);

            //如果 URL 沒有 {TileMatrix} 代表還要再從 capabilities 反解
            if (!theform.my.is_string_like(theform.URL, "{TileMatrix}"))
            {

                //嘗試下載
                string data = theform.my.b2s(theform.my.file_get_contents(theform.URL));
                //如果不是 xml 就失敗
                if (!theform.my.is_string_like(data, "xml version="))
                {
                    theform.my.echo("\r\nCan't understand capabilities...\r\n" + theform.URL + "\r\n");
                    theform.my.exit();
                }
                //取 <Contents> 前後，用 </Layer> 裁切, 且 layer = <ows:Identifier>B100000</ows:Identifier>
                var uri = new Uri(theform.URL);
                //uri 必需要有 layer=xxxx
                var Q = theform.my.QueryParse(theform.URL);
                string _Layer = Q["layer"].ToString();
                if (!Q.ContainsKey("layer"))
                {
                    theform.my.echo("\r\nUrl need layer...\r\n" + theform.URL + "\r\n");
                    theform.my.exit();
                }
                //裁切
                string _xml_contents = theform.my.get_between(data, "<Contents>", "</Contents>").Trim();
                if (_xml_contents == "")
                {
                    theform.my.echo("\r\nNo <Contents>...</Contents>\r\n" + theform.URL + "\r\n");
                    theform.my.exit();
                }
                var m = theform.my.explode("</Layer>", _xml_contents);
                bool isFound = false;
                for (int i = 0, max_i = m.Count(); i < max_i; i++)
                {
                    //符合 <ows:Identifier>B100000</ows:Identifier>
                    if (theform.my.is_string_like(m[i], "<ows:Identifier>" + _Layer + "</ows:Identifier>"))
                    {
                        isFound = true;
                        //抓 URL
                        theform.URL = theform.my.get_between(m[i], "resourceType=\"tile\" template=\"", "\"/>");
                        break;
                    }
                }
                if (isFound == false)
                {
                    theform.my.echo("\r\nNo layer found..." + _Layer + "\r\n");
                    theform.my.exit();
                }
            } //從 Capabilities 抓完

            //再次處理 URL 問題
            theform.URL = Regex.Replace(theform.URL, "{z}", "{TileMatrix}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{x}", "{TileCol}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{y}", "{TileRow}", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{TileMatrixSet}", "GoogleMapsCompatible", RegexOptions.IgnoreCase);
            theform.URL = Regex.Replace(theform.URL, "{Style}", "default", RegexOptions.IgnoreCase);

        }
        SemaphoreSlim semaphore = null;
        async Task DoWork(int id, int step, string _URL, int _z, int _x, int _y)
        {
            // _URL 如果有 [a-c] 代表要隨機選一個
            if (theform.my.is_string_like(_URL, "[a-c]"))
            {
                string abc = "abc";
                _URL = _URL.Replace("[a-c]", abc[theform.my.rand(0, 3)].ToString());
            }
            try
            {

                switch (theform.FORMAT)
                {
                    case "DIR":
                        {
                            string dn = theform.OUTPUT_PATH;
                            string OPMN = dn + "\\" + _z + "\\" + _x + "\\" + _y + ".jpg";
                            string OPDN = dn + "\\" + _z + "\\" + _x;
                            if (!theform.my.is_dir(OPDN))
                            {
                                theform.my.mkdir(OPDN);
                            }

                            theform.my.echo("LEVEL: " + _z + "/ " + theform.END_LEVEL.ToString() + ", ( " + (step).ToString() + " / " + theform.total_pics.ToString() + " ): " + _URL);
                            if (theform.my.is_file(OPMN))
                            {
                                return;
                            }
                            byte[] b = theform.my.file_get_contents(_URL);
                            theform.my.file_put_contents(OPMN, b);
                            Array.Clear(b, 0, b.Length);
                            b = null;
                        }
                        break;
                    case "ZIP":
                        {
                            string OPMN = _z + "/" + _x + "/" + _y + ".jpg";
                            theform.my.echo("LEVEL: " + _z.ToString() + "/ " + theform.END_LEVEL.ToString() + ", ( " + (step).ToString() + " / " + theform.total_pics.ToString() + " ): " + _URL);
                            if (theform.zip.IsFile(OPMN))
                            {
                                return;
                            }
                            byte[] b = theform.my.file_get_contents(_URL);
                            theform.zip.AddByteToQueue(b, OPMN);
                            Array.Clear(b, 0, b.Length);
                            b = null;
                        }
                        break;
                    case "SQLITE":
                        {
                            theform.my.echo($"LEVEL: {_z}/{theform.END_LEVEL}, ({step} / {theform.total_pics}): {_URL}");
                            var pa = new Dictionary<string, string>
                            {
                                ["x"] = _x.ToString(),
                                ["y"] = _y.ToString(),
                                ["z"] = _z.ToString()
                            };
                            if (theform.my.sqlitePDO_selectSQL_SAFE(theform.pdodb, "SELECT 1 FROM tiles WHERE x = @x AND y = @y AND z = @z", pa).Rows.Count > 0)
                            {
                                return;
                            }
                            try
                            {
                                byte[] data = null;
                                data = theform.my.file_get_contents(_URL);

                                var m = new Dictionary<string, object>
                                {
                                    ["x"] = _x.ToString(),
                                    ["y"] = _y.ToString(),
                                    ["z"] = _z.ToString(),
                                    ["tile_data"] = data
                                };

                                theform.my.sqlitePDO_insertSQL(theform.pdodb, "tiles", m);
                                Array.Clear(data, 0, data.Length);
                                data = null;
                                // 交易提交
                                if (id % 30 == 0)
                                {
                                    lock (transaction)
                                    {
                                        transaction.Commit();
                                    }
                                    transaction = theform.pdodb.BeginTransaction();
                                }
                            }
                            catch
                            {
                                return;
                            }

                        }

                        break;
                }

            }
            finally
            {
                semaphore.Release();
            }
        }
        SqliteTransaction transaction = null;
        public async Task downloadTiles()
        {
            //下載圖資                     

            int step = 0;
            int step_totals = 0;

            int maxThreads = theform.THREAD_workers; // 設定最大執行緒數量
            semaphore = new SemaphoreSlim(maxThreads);
            theform.my.echo("maxThreads: " + maxThreads.ToString() + "............");
            List<Task> runningTasks = new List<Task>(); // 用來保存正在運行的任務
            // 交易模式
            if (theform.FORMAT == "SQLITE")
            {
                transaction = theform.pdodb.BeginTransaction();
            }
            for (int _z = theform.START_LEVEL; _z <= theform.END_LEVEL; _z++)
            {
                for (int x = theform.how_many_z[_z]["LT_X"]; x <= theform.how_many_z[_z]["RB_X"]; x++)
                {
                    // 查這層已轉過多少個 Y 了，重複可以跳過
                    HashSet<string> Y_Log = new HashSet<string>();
                    // 依不同類型取得 Y_Log
                    switch (theform.FORMAT)
                    {
                        case "DIR":
                            {
                                string dn = theform.OUTPUT_PATH;
                                string OPDN = dn + "\\" + _z + "\\" + x;
                                if (theform.my.is_dir(OPDN))
                                {
                                    var fp = theform.my.glob(OPDN, "*.jpg");
                                    foreach (string f in fp)
                                    {
                                        string[] a = theform.my.explode("\\", f); // C:\temp\osm\0\0\123.jpg
                                        string y = theform.my.explode(".", a[a.Length - 1])[0]; // 123.jpg -> 123
                                        Y_Log.Add(y);
                                    }
                                }
                            }
                            break;
                        case "SQLITE":
                            {
                                string SQL = @"
                                    SELECT y FROM tiles WHERE z = @z AND x = @x AND IFNULL(tile_data,'') != ''
                                ";
                                var pa = new Dictionary<string, string>
                                {
                                    ["z"] = _z.ToString(),
                                    ["x"] = x.ToString()
                                };
                                var dt = theform.my.sqlitePDO_selectSQL_SAFE(theform.pdodb, SQL, pa);
                                foreach (System.Data.DataRow dr in dt.Rows)
                                {
                                    Y_Log.Add(dr["y"].ToString());
                                }
                            }
                            break;
                    }

                    for (int y = theform.how_many_z[_z]["LT_Y"]; y <= theform.how_many_z[_z]["RB_Y"]; y++)
                    {
                        string _URL = theform.URL;
                        string _x = x.ToString();
                        string _y = y.ToString();

                        // 在 Y_Log 裡面就跳過
                        if (Y_Log.Contains(_y))
                        {
                            // 印出跳過哪一筆
                            theform.my.echo("Skip: " + _z + "/" + x + "/" + y);
                            step_totals++;
                            step++;
                            continue;
                        }

                        _URL = _URL.Replace("{TileMatrix}", _z.ToString()); // z
                        _URL = _URL.Replace("{TileCol}", _x);
                        _URL = _URL.Replace("{TileRow}", _y);
                        step_totals++;
                        int taskId = step_totals; // ⭐⭐ 這行很重要！確保每個 Task 都有自己的 taskId
                        step++;
                        taskId %= maxThreads;

                        // 這裡用 SemaphoreSlim 保證同時執行的最大任務數量
                        await semaphore.WaitAsync();

                        var task = Task.Run(async () => await DoWork(
                            taskId, step, _URL, _z, x, y
                        ));
                        // 將任務加入到運行中的列表中
                        runningTasks.Add(task);
                        // 使用 Task.WhenAny 等待任務完成
                        if (runningTasks.Count >= maxThreads)
                        {
                            // 等待其中一個任務完成後再繼續執行
                            await Task.WhenAny(runningTasks);
                            // 移除已完成的任務
                            runningTasks.RemoveAll(t => t.IsCompleted);
                        }
                    }
                }

            }
            await Task.WhenAll(runningTasks);
            Console.WriteLine("所有工作完成！");
            // 如果是 zip 稍微等 6 秒
            if (theform.FORMAT == "ZIP")
            {
                System.Threading.Thread.Sleep(6000);
                theform.zip.Dispose();
            }
            if (theform.FORMAT == "SQLITE")
            {
                lock (transaction)
                {
                    transaction.Commit();
                }
                theform.pdodb.Dispose();
            }
            //return check;
        }
    }
}

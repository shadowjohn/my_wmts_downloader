using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using utility;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Policy;

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
                    theform.p3826["LT_X"] = 121.383;
                    theform.p3826["LT_Y"] = 23.548;
                    theform.p3826["RB_X"] = 121.408;
                    theform.p3826["RB_Y"] = 23.523;
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
                        if (!theform.my.is_file(theform.OUTPUT_PATH))
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

        public bool downloadTiles()
        {
            string dn = theform.OUTPUT_PATH;
            bool check = true;
            int step = 0;
            int step_totals = 0;
            SqliteTransaction transaction = null;

            // 交易模式
            if (theform.FORMAT == "SQLITE")
            {
                transaction = theform.pdodb.BeginTransaction();
            }

            int maxThreads = theform.THREAD_workers; // 設定最大執行緒數量
            var semaphore = new SemaphoreSlim(maxThreads);
            var tasks = new List<Task>();
            /*for (int z = theform.START_LEVEL; z <= theform.END_LEVEL; z++)
            {
                theform.my.echo("Z: " + z);
                // 如果 z < 10 可以多抓一些
                if (z < 10)
                {
                    theform.how_many_z[z]["LT_X"] -= 2;
                    theform.how_many_z[z]["LT_Y"] -= 2;
                    theform.how_many_z[z]["RB_X"] += 2;
                    theform.how_many_z[z]["RB_Y"] += 2;
                    // 需大於 0
                    if (theform.how_many_z[z]["LT_X"] < 0) theform.how_many_z[z]["LT_X"] = 0;
                    if (theform.how_many_z[z]["LT_Y"] < 0) theform.how_many_z[z]["LT_Y"] = 0;
                }
            }*/
            HashSet<string> processedZ_Y_X = new HashSet<string>();
            for (int z = theform.START_LEVEL; z <= theform.END_LEVEL; z++)
            {
                theform.my.echo("Z: " + z);
                for (int x = theform.how_many_z[z]["LT_X"]; x <= theform.how_many_z[z]["RB_X"]; x++)
                {
                    for (int y = theform.how_many_z[z]["LT_Y"]; y <= theform.how_many_z[z]["RB_Y"]; y++)
                    {
                        if (z < 10)
                        {
                            if (processedZ_Y_X.Contains($"{z}-{y}-{x}")) continue;
                            processedZ_Y_X.Add($"{z}-{y}-{x}");
                        }

                        // 限制同時下載數量
                        step_totals++;
                        semaphore.Wait();
                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {

                                Interlocked.Increment(ref step);
                                string _URL = theform.URL.Replace("{TileMatrix}", z.ToString())
                                                         .Replace("{TileCol}", x.ToString())
                                                         .Replace("{TileRow}", y.ToString());
                                //theform.my.echo($"LEVEL: {z}/{theform.END_LEVEL}, ({step} / {theform.total_pics})");

                                switch (theform.FORMAT)
                                {
                                    case "DIR":
                                        {
                                            string OPMN = $"{dn}\\{z}\\{y}\\{x}.jpg";
                                            string OPDN = $"{dn}\\{z}\\{y}";

                                            if (!theform.my.is_dir(OPDN))
                                            {
                                                theform.my.mkdir(OPDN);
                                            }

                                            theform.my.echo($"LEVEL: {z}/{theform.END_LEVEL}, ({step} / {theform.total_pics}): {_URL}");
                                            if (theform.my.is_file(OPMN)) return;


                                            try
                                            {
                                                await Task.Delay(theform.my.rand(100, 600)); // 避免阻塞
                                                var data = theform.my.file_get_contents(_URL);
                                                theform.my.file_put_contents(OPMN, data);
                                            }
                                            catch
                                            {
                                                return;
                                            }
                                        }
                                        break;

                                    case "ZIP":
                                        {
                                            theform.my.echo($"LEVEL: {z}/{theform.END_LEVEL}, ({step} / {theform.total_pics}): {_URL}");
                                            string op = $"{z}/{y}/{x}.jpg";
                                            if (theform.zip.IsFile(op)) return;
                                            try
                                            {
                                                await Task.Delay(theform.my.rand(100, 600)); // 避免阻塞
                                                var data = theform.my.file_get_contents(_URL);
                                                theform.zip.AddByteToQueue(data, op);
                                                Array.Clear(data, 0, data.Length);
                                                data = null;
                                            }
                                            catch
                                            {
                                                return;
                                            }
                                        }
                                        break;
                                    case "SQLITE":
                                        {
                                            theform.my.echo($"LEVEL: {z}/{theform.END_LEVEL}, ({step} / {theform.total_pics}): {_URL}");

                                            var pa = new Dictionary<string, string>
                                            {
                                                ["x"] = x.ToString(),
                                                ["y"] = y.ToString(),
                                                ["z"] = z.ToString()
                                            };

                                            if (theform.my.sqlitePDO_selectSQL_SAFE(theform.pdodb, "SELECT 1 FROM tiles WHERE x = @x AND y = @y AND z = @z", pa).Rows.Count > 0)
                                            {
                                                return;
                                            }
                                            byte[] data = null;
                                            try
                                            {
                                                await Task.Delay(theform.my.rand(100, 600)); // 避免阻塞
                                                data = theform.my.file_get_contents(_URL);                                            

                                                var m = new Dictionary<string, object>
                                                {
                                                    ["x"] = x.ToString(),
                                                    ["y"] = y.ToString(),
                                                    ["z"] = z.ToString(),
                                                    ["tile_data"] = data
                                                };

                                                theform.my.sqlitePDO_insertSQL(theform.pdodb, "tiles", m);
                                                Array.Clear(data, 0, data.Length);
                                                data = null;
                                                // 交易提交
                                                if (step_totals % 3000 == 0)
                                                {
                                                    lock (transaction)
                                                    {
                                                        transaction.Commit();
                                                        transaction = theform.pdodb.BeginTransaction();
                                                    }
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
                        }));

                    }
                }
                Task.WhenAll(tasks.ToArray()); // 等待所有的Task完成後再繼續處理下一個z層
            }

            Task.WaitAll(tasks.ToArray());

            if (theform.FORMAT == "ZIP")
            {
                //wait 10 sec
                Thread.Sleep(6 * 1000);
            }
            if (theform.FORMAT == "SQLITE")
            {
                lock (transaction)
                {
                    transaction.Commit();
                }
            }


            return check;
        }
    }
}

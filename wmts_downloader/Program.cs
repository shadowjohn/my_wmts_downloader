using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using utility;
using wmts_downloader.App_Code;

namespace wmts_downloader
{
    public class Program
    {
        public myinclude my = new myinclude();
        //輸出暫存的目錄
        public string TMP_PATH = "";
        public App app = null;
        //WMTS 網址
        public string URL = "https://c.tile.openstreetmap.org/${z}/${x}/${y}.png";
        //坐標 3826
        public Dictionary<string, double> p3826 = new Dictionary<string, double>();
        //坐標 4326
        public Dictionary<string, double> p4326 = new Dictionary<string, double>();
        //範圍抓取 xyz ZOOM,LT_X,LT_Y,RB_X,RB_Y
        public Dictionary<string, int> ptile = new Dictionary<string, int>();
        //Z 有幾張 X 有幾張 Y 有幾張，LT_X,LT_Y,RB_X,RB_Y
        public Dictionary<int, Dictionary<string, int>> how_many_z = new Dictionary<int, Dictionary<string, int>>();

        //共幾張
        public int total_pics = 0;
        //合併後的圖資
        public Bitmap bp;
        //面積
        public double AREA;

        //開始 START_LEVEL
        public int START_LEVEL = 0;

        //結束 END_LEVEL
        public int END_LEVEL = 15;

        //輸出目錄
        public string OUTPUT_PATH = "C:\\temp\\output_osm";
        public string MESSAGE = @"
Usage :
  wmts_downloader.exe ""URL"" ""LT_X"" ""LT_Y"" ""RB_X"" ""RB_Y"" ""START_LEVEL"" ""END_LEVEL"" ""OUTPUT_PATH""
  wmts_downloader.exe test
  wmts_downloader.exe ""https://wmts.nlsc.gov.tw/wmts?layer=B5000"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" 0 15 ""C:\\temp\\B5000""
  wmts_downloader.exe ""https://wmts.nlsc.gov.tw/wmts?layer=TOPO50K_109"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" 0 15 ""C:\\temp\\TOPO50K_109"" 
  wmts_downloader.exe ""https://wmts.nlsc.gov.tw/wmts/B5000/{Style}/{TileMatrixSet}/{TileMatrix}/{TileRow}/{TileCol}"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" 1 15 ""C:\\temp\\B5000"" 
  wmts_downloader.exe ""https://c.tile.openstreetmap.org/${z}/${x}/${y}.png"" ""289115.13"" ""2605063.03"" ""291660.12"" ""2602287.44"" 1 15 ""C:\\temp\\osm"" 
  wmts_downloader.exe ""https://c.tile.openstreetmap.org/${z}/${x}/${y}.png"" ""121.383"" ""23.548"" ""121.408"" ""23.523"" 1 15 ""C:\\temp\\osm"" 
";

        static void Main(string[] args)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            Program F1 = new Program();
            //輸出暫存的目錄
            F1.TMP_PATH = F1.my.getSystemKey("tmp_path");
            if (!F1.my.is_dir(F1.TMP_PATH))
            {
                F1.my.mkdir(F1.TMP_PATH);
            }
            //preset
            F1.p3826["LT_X"] = 289115.13;
            F1.p3826["LT_Y"] = 2602287.44;
            F1.p3826["RB_X"] = 291660.12;
            F1.p3826["RB_Y"] = 2605063.03;
            F1.app = new App(F1);
            F1.app.input_vertify(args); //處理輸入參數
            //修正四角位置
            F1.p3826 = F1.my.fix_LT_RB(F1.p3826);
            //定義 4326
            F1.p4326 = F1.my.p3826_to_p4326(F1.p3826);

            //取得 START_LEVEL ~ END_LEVEL 階抓取的 XYZ 範圍
            F1.total_pics = 0;
            for (int z = F1.START_LEVEL; z <= F1.END_LEVEL; z++)
            {
                F1.ptile = F1.my.p4326_to_ptile(z, F1.p4326);

                //取得 x、y、共有幾張                
                var d = new Dictionary<string, int>();
                d["RB_X"] = F1.ptile["RB_X"];
                d["LT_X"] = F1.ptile["LT_X"];
                d["RB_Y"] = F1.ptile["RB_Y"];
                d["LT_Y"] = F1.ptile["LT_Y"];
                d["HOW_MARY_X"] = (F1.ptile["RB_X"] - F1.ptile["LT_X"]) + 1;
                d["HOW_MARY_Y"] = (F1.ptile["RB_Y"] - F1.ptile["LT_Y"]) + 1;
                d["TOTAL_PICS"] = (d["HOW_MARY_X"] * d["HOW_MARY_Y"]);
                F1.how_many_z[z] = d;
                F1.total_pics += d["TOTAL_PICS"];
            }

            //面積不能過大
            //utility.PointF[] p = F1.my.p3826_to_pointf(F1.p3826);
            //F1.AREA = F1.my.area_of_polygon(p);
            F1.my.echo("");
            F1.my.echo("URL：" + F1.URL);
            F1.my.echo("坐標 (EPSG:3826)：" + F1.my.json_encode(F1.p3826));
            F1.my.echo("坐標 (EPSG:4326)：" + F1.my.json_encode(F1.p4326));
            //F1.my.echo("面積：" + string.Format("{0:0.00}", F1.AREA) + " 平方公尺");
            //F1.my.echo("XYZ：" + F1.my.json_encode(F1.ptile));
            //F1.my.echo("X：" + F1.how_many_x);
            //F1.my.echo("Y：" + F1.how_many_y);
            F1.my.echo(F1.my.json_encode(F1.how_many_z));
            F1.my.echo("共幾張：" + F1.total_pics);
            F1.my.echo("");
            //開始下載
            F1.my.echo("圖資暫存位置：" + F1.TMP_PATH);
            if (!F1.app.downloadTiles())
            {
                F1.my.echo("執行失敗...");
                F1.my.exit();
            }
            F1.my.echo("輸出檔案：" + F1.OUTPUT_PATH);
            F1.my.echo("工作完成...");
        }
    }
}

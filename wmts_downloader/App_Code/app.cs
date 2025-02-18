using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

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
            if (args.Count() < 1)
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
            if (args.Count() == 8)
            {
                if (theform.my.is_string_like(args[0], "https://") || theform.my.is_string_like(args[0], "http://"))
                {
                    theform.URL = args[0];
                }
                //處理傳參數
                theform.p3826["LT_X"] = Convert.ToDouble(args[1]);
                theform.p3826["LT_Y"] = Convert.ToDouble(args[2]);
                theform.p3826["RB_X"] = Convert.ToDouble(args[3]);
                theform.p3826["RB_Y"] = Convert.ToDouble(args[4]);

                theform.START_LEVEL = Convert.ToInt32(args[5]);
                theform.END_LEVEL = Convert.ToInt32(args[6]);

                theform.OUTPUT_PATH = args[7];

                //判斷是不是真的 3826
                if (theform.p3826["LT_X"] < 300)
                {
                    theform.p3826 = theform.my.p4326_to_p3826(theform.p3826);
                }

            }
            else if (args.Count() == 1 && args[0] == "test")
            {

            }
            else
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
            if (!theform.my.is_string_like(theform.URL, "https://") && !theform.my.is_string_like(theform.URL, "http://"))
            {
                theform.my.echo(theform.MESSAGE);
                theform.my.exit();
            }
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
            //下載圖資
            string dn = theform.OUTPUT_PATH;
            if (theform.my.is_file(dn))
            {
                theform.my.echo("OUTPUT_PATH: " + dn + " is not path...");
                return false;
            }

            if (!theform.my.is_dir(dn))
            {
                theform.my.mkdir(dn);
            }
            bool check = true;
            int step = 0;
            for (int _z = theform.START_LEVEL; _z <= theform.END_LEVEL; _z++)
            {
                for (int x = theform.how_many_z[_z]["LT_X"]; x <= theform.how_many_z[_z]["RB_X"]; x++)
                {
                    for (int y = theform.how_many_z[_z]["LT_Y"]; y <= theform.how_many_z[_z]["RB_Y"]; y++)
                    {
                        string _URL = theform.URL;
                        string _x = x.ToString();
                        string _y = y.ToString();

                        _URL = _URL.Replace("{TileMatrix}", _z.ToString()); // z
                        _URL = _URL.Replace("{TileCol}", _x);
                        _URL = _URL.Replace("{TileRow}", _y);
                        string OPMN = dn + "\\" + _z.ToString() + "\\" + _y + "\\" + _x + ".jpg";
                        string OPDN = dn + "\\" + _z.ToString() + "\\" + _y;
                        if (!theform.my.is_dir(OPDN))
                        {
                            theform.my.mkdir(OPDN);
                        }

                        theform.my.echo("LEVEL: " + _z.ToString() + "/ " + theform.END_LEVEL.ToString() + ", ( " + (++step).ToString() + " / " + theform.total_pics.ToString() + " ): " + _URL);                        
                        if (theform.my.is_file(OPMN))
                        {
                            continue;
                        }
                        theform.my.file_put_contents(OPMN, theform.my.file_get_contents(_URL));
                    }
                }
            }
            return check;
        }
    }
}

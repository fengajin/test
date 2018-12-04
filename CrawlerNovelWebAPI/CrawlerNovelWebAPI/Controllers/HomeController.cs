using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace CrawlerNovelWebAPI.Controllers
{

    public class HomeController : ApiController
    {


    }
    public class ImageReptiles
    {
        public string path { get; set; } = "Images";//图片主目录
        List<ImgInfo> urls = new List<ImgInfo>(); //存储用户定义的Url列表
        public delegate void UrlOverEventHandler(string msg);//处理完成
        public event UrlOverEventHandler urlOver;
        public delegate void OnErrorEventHandler(string errmsg);//发送错误
        public event OnErrorEventHandler onError;

        public struct ImgInfo//url及图片存储子目录
        {
            public string Path { get; set; }
            public string Url { get; set; }
        }

        public List<ImgInfo> Urls { get { return urls; } }
        public void AddUrl(string url, string path)//添加url
        {
            urls.Add(new ImgInfo() { Url = url, Path = path });
        }
        public void AddUrl(string url)
        {
            urls.Add(new ImgInfo() { Url = url });
        }
        public void StartGetImage()//调用此方法开始抓取图片
        {
            if (urls?.Count <= 0)
            {
                onError?.Invoke($"传入Url集合为空,请调用{nameof(AddUrl)}方法传入url地址！");
            }
            urlOver?.Invoke("开始抓取图片,请稍后..........");
            foreach (ImgInfo url in urls)
            {
                string html = GetHtml(url.Url);
                List<string> list = GetImgUrlList(html);
                urlOver?.Invoke($"url:{url.Url}" + SaveImg(list, url.Path));
            }
            urlOver?.Invoke("全部操作完成！");
        }
        string GetHtml(string uri)//请求指定url取得返回html数据
        {
            Stream rsp = null;
            StreamReader sr = null;
            try
            {
                WebRequest http = WebRequest.Create(uri);
                rsp = http.GetResponse().GetResponseStream();
                sr = new StreamReader(rsp, Encoding.UTF8);
                return "成功:" + sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                return "失败:" + ex.Message;
            }
            finally
            {
                sr?.Close();
                rsp?.Close();
            }
        }
        List<string> GetImgUrlList(string html)//从返回html数据中分析提取图片地址
        {
            if (html?.Substring(0, 2) != "成功")
            {
                return null;
            }
            List<string> list = new List<string>();

            MatchCollection mc = Regex.Matches(html, @"[A-Za-z]{4,5}://[^?!\s]*\.jpg", RegexOptions.Multiline);
            foreach (Match m in mc)
            {
                list.Add(m.Groups[0].Value);
            }
            return list;
        }
        String SaveImg(List<string> list, string subpath)//保存图片到本地
        {
            if (list?.Count <= 0)
            {
                return "未解析到图片地址！";
            }
            string dic = path + "\\" + subpath;
            //检查存储路径
            if (!Directory.Exists(dic))
            {
                Directory.CreateDirectory(dic);
            }
            int s = 0, f = 0;
            string msg = "一共抓到{0}个图片地址,成功下载{1}张图片,下载失败{2}张,图片保存路径{3}";
            foreach (string url in list)
            {
                //取文件名
                string name = url.Substring(url.LastIndexOf('/') + 1, url.Length - url.LastIndexOf('/') - 5);
                WebClient wc = new WebClient();
                try
                {
                    wc.DownloadFile(url, dic + "\\" + name + ".jpg");
                    s++;
                    urlOver?.Invoke($"从{url}抓取图片{ name + ".jpg"}成功！");
                }
                catch
                {
                    f++;
                    urlOver?.Invoke($"从{url}抓取图片{name + ".jpg"}失败！");

                }
                finally { wc.Dispose(); }
            }
            return string.Format(msg, list.Count, s, f, dic);
        }

    }

}


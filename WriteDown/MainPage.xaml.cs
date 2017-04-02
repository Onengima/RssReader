using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Syndication;
using System.Xml;
using System.Xml.Serialization;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace WriteDown
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {

        SyndicationClient client;
        SyndicationFeed feed;
        List<RssItem> rssItems;
        List<string> rssItemTitles;        //rssitem的标题
        List<string> rssSrcNames;       //rss资源的名字
        List<RssSrc> rssSrcs;      //rss资源的地址


        public MainPage()
        {
            this.InitializeComponent();
            init();
        }

        private void Button_Click(object sender, RoutedEventArgs e)=> Splitter.IsPaneOpen = (Splitter.IsPaneOpen == true) ? false : true;

        // 汉堡菜单里面的按钮事件处理
        private void Button_Click_1(object sender, RoutedEventArgs e)=> Splitter.IsPaneOpen = false;

        public async Task WriteFile()
        {
            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            await applicationFolder.CreateFileAsync("h.html", CreationCollisionOption.OpenIfExists);
            IStorageFile file = await applicationFolder.GetFileAsync("h.html");
            Stream s = await file.OpenStreamForWriteAsync();
            StreamWriter writer = new StreamWriter(s);
            writer.Write("<p>Hello World</p>");
            writer.Dispose();
            s.Dispose();

        }
        public async Task ReadFile()
        {
            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await applicationFolder.GetFileAsync("x.xml");
            Stream s = await file.OpenStreamForReadAsync();
            StreamReader sr = new StreamReader(s);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private async void init()
        {
            rssSrcs = new List<RssSrc>();
            try
            {
                XmlSerializer x = new XmlSerializer(rssSrcs.GetType());
                IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
                IStorageFile file = await applicationFolder.CreateFileAsync("x.xml", CreationCollisionOption.OpenIfExists);
                Stream s = await file.OpenStreamForReadAsync();
                rssSrcs = (List<RssSrc>)x.Deserialize(s);
            }
            catch(Exception)
            {
                rssSrcs = new List<RssSrc>();
            }

            try
            {
                client = new SyndicationClient();
                rssSrcNames = new List<string>();
                rssItems = new List<RssItem>();
                for (int i = 0; i < rssSrcs.Count; i++)
                {
                    rssSrcNames.Add(rssSrcs[i].Name);
                }

                lvRssSrc.ItemsSource = rssSrcNames;
                await rssClient(rssSrcs[0].Address);
            }
            catch (Exception) { }

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                //设置全屏模式 
                //var applicationView = ApplicationView.GetForCurrentView();
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Color.FromArgb(100, 16, 110, 190); //背景色
                statusBar.BackgroundOpacity = 1;
                //statusBar.ForegroundColor = Colors.White; //信号 时间等绘制颜色
                //statusbar.ProgressIndicator.Text = "test";  //显示提示字和 。。。
                //statusbar.ProgressIndicator.ShowAsync();
            }

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = Color.FromArgb(100, 25, 147, 250);
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonHoverBackgroundColor = Colors.LightBlue;
            titleBar.ButtonBackgroundColor = Color.FromArgb(100, 25, 147, 250);
            titleBar.ButtonForegroundColor = Colors.White;
            
        }
        
        /// <summary>
        /// 连接Rss资源
        /// </summary>
        /// <param Name="s"></param>
        /// <returns></returns>
        private async Task rssClient(string s)
        {
            Uri uri = new Uri(s);
            rssItemTitles = new List<string>();
            client.SetRequestHeader("User-Agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            feed = await client.RetrieveFeedAsync(uri);
            rssItems = new List<RssItem>();
            string context = "";
            foreach (SyndicationItem item in feed.Items)
            {
                RssItem rss = new RssItem(item);
                rssItems.Add(rss);
                rssItemTitles.Add(rss.itemTitle);
            }
            web.NavigateToString(context);
            lvNote.ItemsSource = rssItemTitles;
        }

        private void lvNote_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvNote.SelectedItems.Count != 0)
            {
                if (Window.Current.Bounds.Width > 640) web.NavigateToString(rssItems[lvNote.SelectedIndex].itemSummary);
                else Frame.Navigate(typeof(ContentPage), rssItems[lvNote.SelectedIndex].itemSummary);
                detailFrame.Navigate(typeof(ContentPage), rssItems[lvNote.SelectedIndex].itemSummary);
            }
        }

        /// <summary>
        /// 获取rss资源集合
        /// </summary>
        /// <returns></returns>
        private List<RssSrc> GetRssSrc()
        {
            List<RssSrc> rssAdress = new List<RssSrc>();
            rssAdress.Add(new RssSrc("http://www.ifanr.com/feed", "爱范儿"));
            rssAdress.Add(new RssSrc("http://www.geekpark.net/rss", "极客公园"));
            rssAdress.Add(new RssSrc("http://livesino.net/feed", "livesino"));
            return rssAdress;
        }

        private async void lvRssSrc_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await rssClient(rssSrcs[lvRssSrc.SelectedIndex].Address);
            Splitter.IsPaneOpen = false;
        }

        private async void XmlS()
        {
            XmlSerializer x = new XmlSerializer(rssSrcs.GetType());
            IStorageFolder applicationFolder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await applicationFolder.CreateFileAsync("x.xml", CreationCollisionOption.ReplaceExisting);
            Stream s = await file.OpenStreamForWriteAsync();
            x.Serialize(s, rssSrcs);
            s.Dispose();
            Stream s2 = await file.OpenStreamForReadAsync();
            StreamReader sr = new StreamReader(s2);
        }
    }


    /// <summary>
    /// Rss内容对象
    /// </summary>
    class RssItem
    {
        public string itemTitle;
        public string itemLink;
        public string itemContent;
        public string itemSummary;
        public RssItem(SyndicationItem item)
        {
            itemTitle = item.Title == null ? "No title" : item.Title.Text;
            itemLink = item.Links == null ? "No link" : item.Links.FirstOrDefault().NodeValue;
            itemContent = item.Content == null ? "" : item.Content.Text;
            itemSummary = "<head><style>img{height: auto; width: auto\\9; width:100%;}</style></head>" + 
                "<body width=320px style=\"word-wrap:break-word; font-family:Arial\">" + item.Summary.Text + "</body>";
        }
    }

    /// <summary>
    /// Rss资源对象
    /// </summary>
    //[XmlRoot("RssSrc")]
    public class RssSrc
    {
        //[XmlAttribute("Address")]
        public string Address { get; set; }
        //[XmlAttribute("Name")]
        public string Name { get; set; }
        public RssSrc(string a,string b)
        {
            Address = a;
            Name = b;
        }
        public RssSrc() { }
    }
}

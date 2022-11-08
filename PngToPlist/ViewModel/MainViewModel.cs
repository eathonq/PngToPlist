using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System;

namespace PngToPlist.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
            BrowseCommand = new RelayCommand(Browse);
            GenerateCommand = new RelayCommand(Generate);

            SortFormatList = new ObservableCollection<string>()
            {
                "垂直","水平"
            };
        }

        #region 属性
        private string imgPath = "";
        public string ImgPath
        {
            get => imgPath;
            set => SetProperty(ref imgPath, value);
        }

        private double imgWidth = 0;
        public double ImgWidth
        {
            get => imgWidth;
            set => SetProperty(ref imgWidth, value);
        }

        private double imgHeight = 0;
        public double ImgHeight
        {
            get => imgHeight;
            set => SetProperty(ref imgHeight, value);
        }

        public ObservableCollection<string> SortFormatList { get; set; }

        private string sortFormat = "";
        public string SortFormat
        {
            get => sortFormat;
            set => SetProperty(ref sortFormat, value);
        }

        private int itemCount = 1;
        public int ItemCount
        {
            get => itemCount;
            set
            {
                SetProperty(ref itemCount, value);
                updateImgSize();
            }
        }

        private double itemWidth = 1;
        public double ItemWidth
        {
            get => itemWidth;
            set => SetProperty(ref itemWidth, value);
        }

        private double itemHeight = 1;
        public double ItemHeight
        {
            get => itemHeight;
            set => SetProperty(ref itemHeight, value);
        }

        private string info = "";
        public string Info
        {
            get => info;
            set => SetProperty(ref info, value);
        }
        #endregion

        private void updateImgSize()
        {
            if (imgWidth == 0 || imgHeight == 0) return;

            if (sortFormat == "垂直")
            {
                ItemWidth = ImgWidth;
                ItemHeight = ImgHeight / ItemCount;
            }
            else
            {
                ItemWidth = ImgWidth / ItemCount;
                ItemHeight = ImgHeight;
            }
        }

        #region 命令
        public ICommand BrowseCommand { get; }
        private void Browse()
        {
            Microsoft.Win32.OpenFileDialog dialog = new()
            {
                Multiselect = false,    //该值确定是否可以选择多个文件
                Title = "请选择组合图片",
                Filter = "图片文件(*.png)|*.png"
            };
            if (dialog.ShowDialog() == true)
            {
                ImgPath = dialog.FileName;

                // 图片数据校验
                BitmapImage img = new(new Uri(imgPath, UriKind.Absolute));
                if (img != null)
                {
                    ImgWidth = img.Width;
                    ImgHeight = img.Height;

                    Info = $"图片尺寸：{ImgWidth}x{ImgHeight}";

                    // 自动配置参数
                    if(sortFormat == "垂直")
                    {
                        if(imgWidth == itemWidth && imgHeight % itemHeight == 0)
                        {
                            ItemCount = (int)(imgHeight / itemHeight);
                        }
                    }
                    else
                    {
                        if (imgHeight == itemHeight && imgWidth % itemWidth == 0)
                        {
                            ItemCount = (int)(imgWidth / itemWidth);
                        }
                    }
                }
                else 
                {
                    ImgWidth = 0;
                    ImgHeight = 0;

                    Info = "图片数据校验失败";
                }
            }
        }

        public ICommand GenerateCommand { get; }
        private void Generate()
        {
            if (string.IsNullOrEmpty(ImgPath))
            {
                Info = "请选择图片";
                return;
            }
            if (string.IsNullOrEmpty(SortFormat))
            {
                Info = "请选择排列方式";
                return;
            }
            if (ItemCount <= 0)
            {
                Info = "请设置子项数量";
                return;
            }

            // 生成plist文件
            string plistPath = Path.ChangeExtension(ImgPath, ".plist");
            FileStream fs = new(plistPath, FileMode.Create);
            XmlTextWriter writer = new(fs, System.Text.Encoding.UTF8);

            writer.WriteStartDocument();
            writer.WriteComment("DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd");
            writer.WriteStartElement("plist");
            writer.WriteAttributeString("version", "1.0");
            writer.WriteStartElement("dict");

            writer.WriteStartElement("key");
            writer.WriteString("frames");
            writer.WriteEndElement();
            writer.WriteStartElement("dict");
            for(int i = 0; i < ItemCount; i++)
            {
                writer.WriteStartElement("key");
                writer.WriteString($"{i}.png");
                writer.WriteEndElement();
                writer.WriteStartElement("dict");
                writer.WriteStartElement("key");
                writer.WriteString("frame");
                writer.WriteEndElement();
                writer.WriteStartElement("string");
                if(sortFormat == "垂直")
                {
                    writer.WriteString($"{{{{0,{i * ItemHeight}}},{{{ItemWidth},{ItemHeight}}}}}");
                }
                else
                {
                    writer.WriteString($"{{{{{i * ItemWidth},0}},{{{ItemWidth},{ItemHeight}}}}}");
                }
                writer.WriteEndElement();
                writer.WriteStartElement("key");
                writer.WriteString("offset");
                writer.WriteEndElement();
                writer.WriteStartElement("string");
                writer.WriteString("{0,0}");
                writer.WriteEndElement();
                writer.WriteStartElement("key");
                writer.WriteString("rotated");
                writer.WriteEndElement();
                writer.WriteStartElement("false");
                writer.WriteEndElement();
                writer.WriteStartElement("key");
                writer.WriteString("sourceColorRect");
                writer.WriteEndElement();
                writer.WriteStartElement("string");
                if (sortFormat == "垂直")
                {
                    writer.WriteString($"{{{{0,{i * ItemHeight}}},{{{ItemWidth},{ItemHeight}}}}}");
                }
                else
                {
                    writer.WriteString($"{{{{{i * ItemWidth},0}},{{{ItemWidth},{ItemHeight}}}}}");
                }
                writer.WriteEndElement();
                writer.WriteStartElement("key");
                writer.WriteString("sourceSize");
                writer.WriteEndElement();
                writer.WriteStartElement("string");
                writer.WriteString($"{{{ItemWidth},{ItemHeight}}}");
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("key");
            writer.WriteString("metadata");
            writer.WriteEndElement();
            writer.WriteStartElement("dict");
            writer.WriteStartElement("key");
            writer.WriteString("format");
            writer.WriteEndElement();
            writer.WriteStartElement("integer");
            writer.WriteString("2");
            writer.WriteEndElement();
            writer.WriteStartElement("key");
            writer.WriteString("realTextureFileName");
            writer.WriteEndElement();
            writer.WriteStartElement("string");
            writer.WriteString(Path.GetFileName(ImgPath));
            writer.WriteEndElement();
            writer.WriteStartElement("key");
            writer.WriteString("size");
            writer.WriteEndElement();
            writer.WriteStartElement("string");
            writer.WriteString($"{{{ItemWidth},{ItemCount * ItemHeight}}}");
            writer.WriteEndElement();
            writer.WriteStartElement("key");
            writer.WriteString("smartupdate");
            writer.WriteEndElement();
            writer.WriteStartElement("string");
            writer.WriteString($"${{filename}}");
            writer.WriteEndElement();
            writer.WriteStartElement("key");
            writer.WriteString("textureFileName");
            writer.WriteEndElement();
            writer.WriteStartElement("string");
            writer.WriteString(Path.GetFileName(ImgPath));
            writer.WriteEndElement();
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();

            writer.Close();
            fs.Close();

            Info = "生成成功," + plistPath;
        }

        #endregion
    }
}

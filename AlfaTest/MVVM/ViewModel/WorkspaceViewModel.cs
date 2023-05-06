using AlfaTest.MVVM.Commands;
using System;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AlfaTest.MVVM.Model;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows;
using AlfaTest.MVVM.View;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using log4net;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using TopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder;
using BottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder;
using LeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder;
using RightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using Text = DocumentFormat.OpenXml.Spreadsheet.Text;
using System.Diagnostics;

namespace AlfaTest.MVVM.ViewModel
{
    public class WorkspaceViewModel : ViewModelBase
    {
        #region Variables
        private readonly ILog log = LogManager.GetLogger(typeof(WorkspaceViewModel));
        private MainWindow currentWindow;
        private bool isSortAScending = false;
        #endregion

        #region Constructor
        public WorkspaceViewModel()
        {
            log4net.Config.XmlConfigurator.Configure();
            foreach (var window in App.Current.Windows)
            {
                if (window is MainWindow)
                {
                    currentWindow = (MainWindow)window;
                }
                IsJsonExport = false;
                IsWordExport = false;
                IsExcelExport = false;
            }
        }
        #endregion

        #region Fields & properties

        private bool isJsonExport;
        public bool IsJsonExport
        {
            get { return isJsonExport; }
            set
            {
                isJsonExport = value;
                OnPropertyChanged(nameof(IsJsonExport));
            }
        }


        private bool isWordExport;
        public bool IsWordExport
        {
            get { return isWordExport; }
            set
            {
                isWordExport = value;
                OnPropertyChanged(nameof(IsWordExport));
            }
        }


        private bool isExcelExport;
        public bool IsExcelExport
        {
            get { return isExcelExport; }
            set
            {
                isExcelExport = value;
                OnPropertyChanged(nameof(IsExcelExport));
            }
        }


        private ObservableCollection<ChannelItem> channelItemsCollection;
        public ObservableCollection<ChannelItem> ChannelItemsCollection =>
            channelItemsCollection ??= new ObservableCollection<ChannelItem>();

        private ICollectionView channelCollectionView;
        public ICollectionView ChannelCollectionView
        {
            get { return channelCollectionView; }
            set
            {
                channelCollectionView = value;
                OnPropertyChanged(nameof(ChannelCollectionView));
            }
        }


        private ChannelModel channel;
        public ChannelModel Channel
        {
            get { return channel; }
            set
            {
                channel = value;
                OnPropertyChanged(nameof(Channel));
            }
        }
        #endregion

        #region Commands
        private RelayCommand _loadDataCommand;
        public RelayCommand LoadDataCommand
        {
            get
            {
                return _loadDataCommand ??= new RelayCommand(async obj =>
                {
                    ChannelItemsCollection.Clear();
                    Channel = await LoadDataFromXMLAsync();
                    if (Channel != null)
                    {
                        foreach (var item in Channel.Items)
                        {
                            ChannelItemsCollection.Add(item);
                        }
                        ChannelCollectionView = CollectionViewSource.GetDefaultView(ChannelItemsCollection);
                        ChannelCollectionView.Filter = null;
                        ChannelCollectionView.SortDescriptions.Clear();
                        currentWindow.InfoBarBox.ShowMessage("Данные успешно загружены", InfoBar.InfoBarStatus.SUCCESS, InfoBar.InfoBarPosition.TOP, 3000);
                        log.Info("The data was successfully read from the file");
                    }
                    else
                    {
                        currentWindow.InfoBarBox.ShowMessage("Проблема при загрузке данных", InfoBar.InfoBarStatus.CAUTION, InfoBar.InfoBarPosition.TOP, 3000);
                        log.Warn("Error while loading data from file");
                    }
                });
            }
        }



        private RelayCommand sortCommand;
        public RelayCommand SortCommand
        {
            get
            {
                return sortCommand ??= new RelayCommand(obj =>
                {
                    if (ChannelCollectionView != null)
                    {
                        ListSortDirection direction = isSortAScending == true ? ListSortDirection.Ascending : ListSortDirection.Descending;
                        ChannelCollectionView.SortDescriptions.Clear();
                        ChannelCollectionView.SortDescriptions.Add(new SortDescription("Date", direction));
                        isSortAScending = !isSortAScending;
                        currentWindow.InfoBarBox.ShowMessage("Данные успешно отсортированы", InfoBar.InfoBarStatus.SUCCESS, InfoBar.InfoBarPosition.TOP, 3000);
                    }
                    else
                    {
                        currentWindow.InfoBarBox.ShowMessage("Нет данных для сортировки", InfoBar.InfoBarStatus.CAUTION, InfoBar.InfoBarPosition.TOP, 3000);
                    }
                });
            }
        }


        private RelayCommand filterCommand;
        public RelayCommand FilterCommand
        {
            get
            {
                return filterCommand ??= new RelayCommand(obj =>
                {
                    if (ChannelCollectionView != null)
                    {
                        ChannelCollectionView.Filter = FilterByCategory;
                        currentWindow.InfoBarBox.ShowMessage("Данные успешно отфильтрованы по категории 'Политика'", InfoBar.InfoBarStatus.SUCCESS, InfoBar.InfoBarPosition.TOP, 4000);
                    }
                    else
                    {
                        currentWindow.InfoBarBox.ShowMessage("Нет данных для фильтрации!", InfoBar.InfoBarStatus.CAUTION, InfoBar.InfoBarPosition.TOP, 3000);
                    }
                });
            }
        }


        private RelayCommand openExportCommand;
        public RelayCommand OpenExportCommand
        {
            get
            {
                return openExportCommand ??= new RelayCommand(obj =>
                {
                    Animation.OpacityAnimation((obj as Grid), 0, 1, 0.2);
                    Animation.ScaleAnimation((obj as Grid), 0.2, 1, 0.35);
                });
            }
        }


        private RelayCommand exportCommand;
        public RelayCommand ExportCommand
        {
            get
            {
                return exportCommand ??= new RelayCommand(obj =>
                {
                    if (ChannelItemsCollection != null && ChannelItemsCollection.Count > 0)
                    {
                        if (IsJsonExport == true)
                        {
                            ExportAsJson();
                        }
                        if (IsWordExport == true)
                        {
                            ExportAsWord();
                        }
                        if (IsExcelExport == true)
                        {
                            ExportAsExcel();
                        }
                    }
                    else
                    {
                        currentWindow.InfoBarBox.ShowMessage("Нет данных для экспорта!", InfoBar.InfoBarStatus.CAUTION, InfoBar.InfoBarPosition.TOP, 3000);
                        log.Warn("No data for export");
                    }
                    Animation.OpacityAnimation((obj as Grid), 1, 0, 0.2);
                    Animation.ScaleAnimation((obj as Grid), 1, 0.2, 0.35);
                });
            }
        }


        private RelayCommand changeThemeCommand;
        public RelayCommand ChangeThemeCommand
        {
            get
            {
                return changeThemeCommand ??= new RelayCommand(obj =>
                {
                    ResourceDictionary[] resourceDictionaries = new ResourceDictionary[3] { new(), new(), new() };
                    if ((obj as ToggleButton).IsChecked == true)
                    {
                        resourceDictionaries[0].Source = new Uri("pack://application:,,,/InfoBar;component/Resources/Theme.Light.xaml", UriKind.RelativeOrAbsolute);
                        resourceDictionaries[1].Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Light.Red.xaml", UriKind.RelativeOrAbsolute);
                        resourceDictionaries[2].Source = new Uri("pack://application:,,,/Resources/Styles/Colors/Theme.Light.xaml", UriKind.RelativeOrAbsolute);
                    }
                    else
                    {
                        resourceDictionaries[0].Source = new Uri("pack://application:,,,/InfoBar;component/Resources/Theme.Dark.xaml", UriKind.RelativeOrAbsolute);
                        resourceDictionaries[1].Source = new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Themes/Dark.Red.xaml", UriKind.RelativeOrAbsolute);
                        resourceDictionaries[2].Source = new Uri("pack://application:,,,/Resources/Styles/Colors/Theme.Dark.xaml", UriKind.RelativeOrAbsolute);
                    }
                    foreach (var item in resourceDictionaries)
                    {
                        App.Current.Resources.MergedDictionaries.Add(item);
                    }
                });
            }
        }


        private RelayCommand openLinkCommand;
        public RelayCommand OpenLinkCommand
        {
            get
            {
                return openLinkCommand ??= new RelayCommand(obj =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = obj.ToString(),
                        UseShellExecute = true
                    });
                });
            }
        }

        #endregion

        #region Methods
        async void ExportAsJson()
        {
            await Task.Run(async () =>
            {
                try
                {
                    using (FileStream fs = new("dataJSON.json", FileMode.OpenOrCreate))
                    {
                        var options = new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                            WriteIndented = true,
                        };
                        await JsonSerializer.SerializeAsync<ChannelModel>(fs, Channel, options);
                        log.Info("Data serialized in JSON");
                    }
                }
                catch (Exception exc)
                {
                    log.Error("Error while export data in JSON", exc);
                    MessageBox.Show(exc.Message);
                }
            });
            Process.Start(new ProcessStartInfo
            {
                FileName = "dataJSON.json",
                UseShellExecute = true
            });
        }


        async void ExportAsWord()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (WordprocessingDocument document = WordprocessingDocument.Create("dataWord.docx", WordprocessingDocumentType.Document))
                    {
                        document.AddMainDocumentPart();
                        document.MainDocumentPart.Document = new Document();
                        Body body = document.MainDocumentPart.Document.AppendChild(new Body());

                        Table table = new();
                        TableProperties props = new(
                            new TableBorders(
                            new TopBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 12
                            },
                            new BottomBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 12
                            },
                            new LeftBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 12
                            },
                            new RightBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 12
                            },
                            new InsideHorizontalBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 12
                            },
                            new InsideVerticalBorder
                            {
                                Val = new EnumValue<BorderValues>(BorderValues.Single),
                                Size = 12
                            }));
                        table.AppendChild(props);

                        TableRow headerRow = new(); ;
                        foreach (var prop in typeof(ChannelItem).GetProperties())
                        {
                            if (prop.Name != "Date")
                            {
                                headerRow.Append(new TableCell(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(prop.Name)))));
                            }
                        }

                        table.Append(headerRow);

                        foreach (var item in Channel.Items)
                        {
                            TableRow row = new();
                            foreach (var prop in typeof(ChannelItem).GetProperties())
                            {
                                if (prop.Name != "Date")
                                {
                                    row.Append(new TableCell(new Paragraph(new Run(new DocumentFormat.OpenXml.Wordprocessing.Text(prop.GetValue(item)?.ToString() ?? "")))));
                                }
                            }
                            table.Append(row);
                        }
                        body.Append(table);

                        document.MainDocumentPart.Document.Save();
                        document.Dispose();
                    }
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "dataWord.docx",
                        UseShellExecute = true
                    });
                }
                catch (Exception exc)
                {
                    log.Error("Error while export data in Word or open file", exc);
                    MessageBox.Show(exc.Message);
                }
            });
        }


        async void ExportAsExcel()
        {
            await Task.Run(() =>
            {
                try
                {
                    using (SpreadsheetDocument document = SpreadsheetDocument.Create("dataExcel.xlsx", SpreadsheetDocumentType.Workbook))
                    {
                        var workbook = document.AddWorkbookPart();
                        workbook.Workbook = new Workbook();
                        var worksheet = workbook.AddNewPart<WorksheetPart>();
                        worksheet.Worksheet = new Worksheet(new SheetData());

                        var sheets = document.WorkbookPart.Workbook.AppendChild(new Sheets());

                        var sheet = new Sheet()
                        {
                            Id = document.WorkbookPart.GetIdOfPart(worksheet),
                            SheetId = 1,
                            Name = "Channel items"
                        };

                        sheets.Append(sheet);

                        var sheetData = worksheet.Worksheet.GetFirstChild<SheetData>();
                        var headerRow = new Row();

                        foreach (var prop in typeof(ChannelItem).GetProperties())
                        {
                            if (prop.Name != "Date")
                            {
                                var cell = new Cell(new InlineString(new Text(prop.Name)))
                                {
                                    DataType = CellValues.InlineString,
                                };
                                headerRow.AppendChild(cell);
                            }
                        }

                        sheetData.AppendChild(headerRow);

                        foreach (var item in Channel.Items)
                        {
                            var dataRow = new Row();
                            foreach (var prop in typeof(ChannelItem).GetProperties())
                            {
                                if (prop.Name != "Date")
                                {
                                    var cell = new Cell(new InlineString(new Text(prop.GetValue(item)?.ToString() ?? "")))
                                    {
                                        DataType = CellValues.InlineString
                                    };
                                    dataRow.AppendChild(cell);
                                }
                            }
                            sheetData.AppendChild(dataRow);
                        }

                        workbook.Workbook.Save();
                    }

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "dataExcel.xlsx",
                        UseShellExecute = true
                    });
                }
                catch (Exception exc)
                {
                    log.Error("Error while export data in Excel or open file", exc);
                    MessageBox.Show(exc.Message);
                }
            });
        }





        private bool FilterByCategory(object obj)
        {
            if ((obj as ChannelItem).Categories.Contains("Политика"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private async Task<ChannelModel> LoadDataFromXMLAsync()
        {
            try
            {
                XmlRootAttribute rootAttr = new("channel");
                XmlSerializer xmlSerializer = new(typeof(ChannelModel), rootAttr);
                using (FileStream fs = new("data.xml", FileMode.OpenOrCreate))
                {
                    log.Info("The data has been successfully deserialized");
                    return xmlSerializer.Deserialize(fs) as ChannelModel;
                }
            }
            catch (Exception exc)
            {
                currentWindow.InfoBarBox.ShowMessage(exc.ToString(), InfoBar.InfoBarStatus.CRITICAL, InfoBar.InfoBarPosition.TOP, 8000);
                log.Error("Error while deserializing data from XML", exc);
            }
            log.Warn("A data deserialization error has occurred");
            return null;
        }
        #endregion
    }
}

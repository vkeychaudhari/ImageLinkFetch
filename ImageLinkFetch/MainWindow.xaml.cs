using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ImageLinkFetch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // ObservableCollection to bind images dynamically
        public ObservableCollection<ImageSource> ImageUrls { get; set; } = new ObservableCollection<ImageSource>();

        public MainWindow()
        {
            InitializeComponent();
            ImageListBox.ItemsSource = ImageUrls; // Bind listbox to ImageUrls collection
        }

        private async Task<List<string>> GetImageUrlsAsync(string url)
        {
            return await Task.Run(() =>
            {
                List<string> imageUrls = new List<string>();

                try
                {
                    var options = new ChromeOptions();
                    options.AddArgument("--headless=new");
                    options.AddArgument("--disable-blink-features=AutomationControlled");
                    options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                    var service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;


                    // Ensure previous instances are closed
                    foreach (var process in Process.GetProcessesByName("chromedriver"))
                    {
                        process.Kill();
                    }

                    using (IWebDriver driver = new ChromeDriver(service, options))
                    {
                        driver.Navigate().GoToUrl(url);
                        System.Threading.Thread.Sleep(5000);

                        var imgElements = driver.FindElements(By.XPath("//img[contains(@class, 'lazy')]"));
                        imgElements.Take(10).ToList();
                        foreach (var img in imgElements)
                        {
                            string src = img.GetAttribute("src");
                            if (!string.IsNullOrEmpty(src))
                            {
                                imageUrls.Add(src);
                            }
                        }
                    }
                }
                catch (Exception)
                {

                }
                finally
                {
                    //driver?.Quit();
                    //driver?.Dispose();
                    //service?.Dispose();
                }

                return imageUrls;
            });
        }

        private bool ContainsUnsafeContent(string input)
        {
            // List of unsafe words (you can expand this)
            List<string> unsafeWords = new List<string>
        {
            "adult", "violence", "drugs", "explicit", "nsfw", "xxx", "porn", "gambling", "illegal"
        };

            // Convert input to lowercase to make it case-insensitive
            string lowerInput = input.ToLower();

            // Check if any unsafe word exists in the input string
            foreach (var word in unsafeWords)
            {
                if (lowerInput.Contains(word))
                {
                    return true; // Unsafe content detected
                }
            }

            return false; // Safe content
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnFetch.IsEnabled = false;
                LoaderImage.Visibility = Visibility.Visible;

                string query = MyTextBox.Text;
                if (!ContainsUnsafeContent(query))
                {
                    ImageUrls.Clear(); // Clear previous images
                    if (query.ToLower().Contains("hot"))
                    {
                        query = query + " education related images";
                    }

                    string url = $"https://duckduckgo.com/?t=h_&q={query}&iax=images&ia=images";

                    List<string> imageUrls = await GetImageUrlsAsync(url); // Fully async now
                    
                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        imageUrls = imageUrls.Take(10).ToList(); // Limit to 10 images
                        foreach (var img in imageUrls)
                        {
                            ImageUrls.Add(LoadImageFromUrl(img)); // Add images to ObservableCollection
                        }
                        //foreach (var img in imageUrls)
                        //{
                        //    MyImageControl.Source = LoadImageFromUrl(img);
                        //    break;
                        //}
                    }
                    else
                    {
                        MessageBox.Show("Image not found Try again after sometime..");
                    }
                }
                else
                {
                    MessageBox.Show("Unsafe content detected. Please try another search query.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                btnFetch.IsEnabled = true;
                LoaderImage.Visibility = Visibility.Collapsed;
            }
        }


        private BitmapImage LoadImageFromUrl(string url)
        {
            try
            {
                // Create a BitmapImage and set its source to the downloaded image
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(url, UriKind.Absolute);
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad; // Ensure the image is fully loaded
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
                return null;
            }
        }
    }
}

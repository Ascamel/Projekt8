using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;

namespace Projekt8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        string imgPath;

        Bitmap picture;
        Bitmap newpicture;
        Bitmap newpicture2;




        public int ThresholdBlack(List<MyValue> myValues, int threshold)
        {
            int newthreshold = 0;
            int sum = 0;
            int iter = 0;

            int tmp = myValues.FindIndex(o => o.value == threshold);

            for (int i = 0; i < tmp; i++)
            {
                sum+= myValues[i].value;
                iter++;
            }
            if(iter ==0)
            { return 0; }
            newthreshold = sum / iter;

            return newthreshold;
        }

        public int ThresholdWhite(List<MyValue> myValues, int threshold)
        {
            int newthreshold = 0;
            int sum = 0;
            int iter = 0;

            int tmp = myValues.FindIndex(o => o.value == threshold);

            if(tmp==-1)
            { return 0; }

            for (int i = tmp; i < myValues.Count; i++)
            {
                sum += myValues[i].value;
                iter++;
            }

            newthreshold = sum / iter;

            return newthreshold;
        }

        private void LoadImageButtonClicked2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                imgPath = op.FileName;
                MyImage2.Source = new BitmapImage(new Uri(imgPath));

                picture = new(imgPath);
            }

            List<MyValue> LUT = new();

            MyValue myValue;

            int threshold = 127;


            int average = 0;

            for (int i = 0; i < picture.Width; ++i)
            {
                for (int j = 0; j < picture.Height; ++j)
                {
                    System.Drawing.Color pixel = picture.GetPixel(i, j);
                    int r = Convert.ToInt16(pixel.R),
                        g = Convert.ToInt16(pixel.G),
                        b = Convert.ToInt16(pixel.B);

                    average = (r + g + b) / 3;

                    myValue = new(i, j, average);
                    LUT.Add(myValue);
                }
            }

            List<MyValue> SortedList = LUT.OrderBy(o => o.value).ToList();

            int TB = ThresholdBlack(SortedList, threshold);
            int TW = ThresholdWhite(SortedList, threshold);

            while (true)
            {
                if (((TW + TB) / 2) != threshold)
                {
                    threshold = (TW + TB) / 2;
                }
                else
                {
                    break;
                }

                TB = ThresholdBlack(SortedList, threshold);
                TW = ThresholdWhite(SortedList, threshold);
            }


            for (int i = 0; i < picture.Width; ++i)
            {
                for (int j = 0; j < picture.Height; ++j)
                {
                    System.Drawing.Color pixel = picture.GetPixel(i, j);
                    int r = Convert.ToInt16(pixel.R),
                        g = Convert.ToInt16(pixel.G),
                        b = Convert.ToInt16(pixel.B);

                    average = (r+g+b)/3;

                    if (average > threshold)
                    {
                        pixel = Color.White;
                        picture.SetPixel(i, j, pixel);
                    }
                    else
                    {
                        pixel = Color.Black;
                        picture.SetPixel(i, j, pixel);
                    }
                }
            }
            MemoryStream memoryStream = new();
            picture.Save(memoryStream, ImageFormat.Png);
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            MyImage2.Source = bitmapImage;

        }

        public void SaveImage()
        {
            MemoryStream memoryStream = new();
            newpicture.Save(memoryStream, ImageFormat.Png);
            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            MyImage2.Source = bitmapImage;
        }

        private void TranformButtonClick2(object sender, RoutedEventArgs e)
        {
            newpicture = new(picture.Width, picture.Height);

            if (DylatationButton.IsChecked == true)
            {
                Dylatation();
            }
            if (ErosionButton.IsChecked == true)
            {
                Erosion();
            }
            if (OpeningButton.IsChecked == true)
            {
                Erosion();
                newpicture = new(picture.Width, picture.Height);
                Dylatation();
            }
            if (ClosingButton.IsChecked == true)
            {
                Dylatation();
                newpicture = new(picture.Width, picture.Height);
                Erosion();
            }
            if (HitOrMissButton.IsChecked == true)
            {
                Thinning(1);
                newpicture2 = new(newpicture); 
                Thinning(2);
                for (int i = 0; i < newpicture.Width; i++)
                {
                    for (int j = 0; j < newpicture.Height; j++)
                    {
                        if (newpicture2.GetPixel(i, j) == Color.FromArgb(255, 255, 255, 255))
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                        }
                    }
                }
                SaveImage();
            }

            if (HitOrMissThickButton.IsChecked == true)
            {
                Thickening(1);
                newpicture2 = new(newpicture);
                Thickening(2);
                for (int i = 0; i < newpicture.Width; i++)
                {
                    for (int j = 0; j < newpicture.Height; j++)
                    {
                        if (newpicture2.GetPixel(i, j) == Color.FromArgb(255, 255, 255, 255))
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                        }
                    }
                }
                SaveImage();
            }
        }

        private void Thickening(int num)
        {
            int[] Structure1 = new int[] {
                1,1,2,
                1,0,2,
                1,2,0
            };
            int[] Structure2 = new int[] {
                2,1,1,
                2,0,1,
                0,2,1
            };

            for (int i = 0; i<picture.Width; i++)
            {
                for (int j = 0; j<picture.Height; j++)
                {
                    int[] ActiveStructure = new int[9];
                    int tmp = 0;

                    for (int k = i -1; k <= i+1; k++)
                    {
                        if (k >=0 && k < picture.Width)
                        {
                            for (int l = j -1; l <= j+1; l++)
                            {
                                if (l >=0 && l < picture.Height)
                                {

                                    if (picture.GetPixel(k, l) == Color.FromArgb(255, 0, 0, 0))
                                    {
                                        ActiveStructure[tmp] = 1;
                                    }
                                    else
                                    {
                                        ActiveStructure[tmp] = 0;
                                    }

                                    if (num == 1)
                                    {
                                        if (tmp == 2 || tmp == 5 || tmp == 7)
                                        {
                                            ActiveStructure[tmp] = 2;
                                        }
                                    }
                                    else
                                    {
                                        if (tmp == 0 || tmp == 3|| tmp == 7)
                                        {
                                            ActiveStructure[tmp] = 2;
                                        }
                                    }

                                    tmp++;
                                }
                            }
                        }
                    }
                    if (num == 1)
                    {

                        if (ActiveStructure.SequenceEqual(Structure1))
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));

                        }
                        else
                        {
                            newpicture.SetPixel(i, j, picture.GetPixel(i,j));

                        }
                    }
                    else
                    {
                        if (ActiveStructure.SequenceEqual(Structure2))
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));

                        }
                        else
                        {
                            newpicture.SetPixel(i, j, picture.GetPixel(i, j));
                        }
                    }



                }
            }
            SaveImage();
        }

        private void Thinning(int num)
        {

            int[] Structure1 = new int[] {
                0,0,0,
                2,1,2,
                1,1,1
            };
            int[] Structure2 = new int[] {
                2,0,0,
                1,1,0,
                2,1,2
            };

            for (int i = 0; i<picture.Width; i++)
            {
                for (int j = 0; j<picture.Height; j++)
                {
                    int[] ActiveStructure = new int[9];
                    int tmp = 0;

                    for (int k = i -1; k <= i+1; k++)
                    {
                        if (k >=0 && k < picture.Width)
                        {
                            for (int l = j -1; l <= j+1; l++)
                            {
                                if (l >=0 && l < picture.Height)
                                {

                                    if(picture.GetPixel(k, l) == Color.FromArgb(255, 0, 0, 0))
                                    {
                                        ActiveStructure[tmp] = 1;
                                    }
                                    else
                                    {
                                        ActiveStructure[tmp] = 0;
                                    }

                                    if(num == 1)
                                    {
                                        if (tmp == 3 || tmp == 5)
                                        {
                                            ActiveStructure[tmp] = 2;
                                        }
                                    }
                                    else
                                    {
                                        if (tmp == 0 || tmp == 6|| tmp == 8)
                                        {
                                            ActiveStructure[tmp] = 2;
                                        }
                                    }

                                    tmp++;
                                }
                            }
                        }
                    }
                    if(num == 1)
                    {

                        if (ActiveStructure.SequenceEqual(Structure1))
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));

                        }
                        else
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));

                        }
                    }
                    else
                    {
                        if (ActiveStructure.SequenceEqual(Structure2))
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));

                        }
                        else
                        {
                            newpicture.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));

                        }
                    }

                   
                }
            }
            //picture = newpicture;
            SaveImage();
        }

        private void Erosion()
        {
            for (int i = 0; i<picture.Width; i++)
            {
                for (int j = 0; j<picture.Height; j++)
                {
                    //////
                    bool found = false;
                    for (int k = i -1; k <= i+1; k++)
                    {
                        if (k >=0 && k < picture.Width)
                        {
                            for (int l = j -1; l <= j+1; l++)
                            {
                                if (l >=0 && l < picture.Height)
                                {
                                    if (picture.GetPixel(k, l) == Color.FromArgb(255, 255, 255, 255))
                                    {
                                        found = true;
                                    }
                                }
                            }
                        }
                    }
                    if (found)
                    {
                        newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                    else
                    {
                        newpicture.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                    }
                }
            }
            picture = newpicture;
            SaveImage();
        }

        private void Dylatation()
        {
            for(int i = 0; i<picture.Width;i++)
            {
                for(int j = 0; j<picture.Height;j++)
                {
                    //////
                    bool found = false;
                    for(int k = i -1; k <= i+1; k++)
                    {
                        if(k >=0 && k < picture.Width)
                        {
                            for (int l = j -1; l <= j+1; l++)
                            {
                                if(l >=0 && l < picture.Height)
                                {
                                    if (picture.GetPixel(k, l) == Color.FromArgb(255, 0, 0, 0))
                                    {
                                        found = true;
                                    }
                                }
                            }
                        }     
                    }
                    if(found)
                    {
                        newpicture.SetPixel(i, j, Color.FromArgb(255, 0, 0, 0));
                    }
                    else
                    {
                        newpicture.SetPixel(i, j, Color.FromArgb(255, 255, 255, 255));
                    }
                }
            }
            picture = newpicture;
            SaveImage();
        }
    }
    public class MyValue
    {
        public int i { get; set; }
        public int j { get; set; }
        public int value { get; set; }

        public MyValue(int i, int j, int value)
        {
            this.i=i;
            this.j=j;
            this.value=value;
        }
    }
}



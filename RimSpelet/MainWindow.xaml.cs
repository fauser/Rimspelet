using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data.EntityClient;
using System.Data.SqlServerCe;
using System.Data;
using System.IO;

namespace RimSpelet
{
    public enum PicturyType
    {
        Base, Rime, Faulty1, Faulty2
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int m_Points = 0;

        public MainWindow()
        {
            InitializeComponent();
            PopulateComboBox();
            ShowNewRime();


        }

        #region Events
        private void canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CorrectImage((Canvas)sender);
            ShowNewRime();
        }
        #endregion

        #region Private Methods
        private void PopulateComboBox()
        {
            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    var rimes = (from r in context.Rime
                                 select r).Distinct().ToList();


                    foreach (Rime r in rimes)
                    {
                        ListBoxItem item = new ListBoxItem();
                        item.Content = r.RimeWord;
                        item.Tag = r;

                        lbGroup.Items.Add(item);
                    }

                }
            }
        }

        private void CorrectImage(Canvas canvas)
        {
            if (canvas.Tag != null && (PicturyType)canvas.Tag == PicturyType.Rime)
            {
                m_Points++;
            }
            lblPoints.Content = m_Points.ToString();
        }

        private void ShowNewRime()
        {
            long rimeId = GetNewRimeId();
            long wordId = GetNextWordId(rimeId);
            Dictionary<PicturyType, long> allWordIds = GetAllWordIds(rimeId, wordId);
            //foreach (long l in allWordIds)
            //{
            //  Console.WriteLine(l);
            //}
            ChangePictures(allWordIds);
        }

        private void ChangePictures(Dictionary<PicturyType, long> allWordIds)
        {
            ShowPicture(allWordIds, PicturyType.Base, canvas2);

            List<Canvas> ds = new List<Canvas>() { canvas3, canvas4, canvas5 };

            Random r = new Random();
            int index = r.Next(3);
            ds[index].Tag = PicturyType.Rime;
            ShowPicture(allWordIds, PicturyType.Rime, ds[index]);
            ds.RemoveAt(index);

            index = r.Next(2);
            ds[index].Tag = PicturyType.Faulty1;
            ShowPicture(allWordIds, PicturyType.Faulty1, ds[index]);
            ds.RemoveAt(index);

            ds[0].Tag = PicturyType.Faulty2;
            ShowPicture(allWordIds, PicturyType.Faulty2, ds[0]);
        }

        private Dictionary<PicturyType, long> GetAllWordIds(long rimeId, long wordId)
        {
            Dictionary<PicturyType, long> wordIdList = new Dictionary<PicturyType, long>();

            if (rimeId == 0 || wordId == 0)
                return wordIdList;

            wordIdList.Add(PicturyType.Base, wordId);

            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    var words = (from w in context.Word
                                 where w.RimeID == rimeId && w.WordID != wordId
                                 select w.WordID).ToList();

                    if (words.Count > 0)
                    {
                        Random randomizer = new Random();
                        long rimeWordId = words[randomizer.Next(words.Count)];
                        wordIdList.Add(PicturyType.Rime, rimeWordId);

                        words = (from w in context.Word
                                 where w.RimeID != rimeId && w.WordID != wordId && w.WordID != rimeWordId
                                 select w.WordID).ToList();

                        if (words.Count > 1)
                        {
                            int maxTries = 10;
                            int tries = 0;
                            randomizer = new Random();
                            while (wordIdList.Count < 4 && tries < maxTries)
                            {
                                long tmpRimeWordId = words[randomizer.Next(words.Count)];
                                if (!wordIdList.ContainsValue(tmpRimeWordId))
                                {
                                    if (!wordIdList.ContainsKey(PicturyType.Faulty1))
                                        wordIdList.Add(PicturyType.Faulty1, tmpRimeWordId);
                                    else
                                        wordIdList.Add(PicturyType.Faulty2, tmpRimeWordId);
                                }

                                tries++;
                            }
                        }
                    }
                }
            }

            return wordIdList;
        }

        private long GetNextWordId(long rimeId)
        {
            if (rimeId == 0)
                return 0;

            long nextWordId = 0;
            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    var words = (from w in context.Word
                                 where w.RimeID == rimeId
                                 select w.WordID).ToList();

                    Random randomizer = new Random();
                    nextWordId = words[randomizer.Next(words.Count)];
                }
            }
            return nextWordId;
        }

        private long GetNewRimeId()
        {
            long nextRimeId = 0;
            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    List<long> rimes = (from r in context.Rime
                                        select r.RimeID).Distinct().ToList();

                    Random randomizer = new Random();
                    nextRimeId = rimes[randomizer.Next(rimes.Count)];
                }
            }
            return nextRimeId;
        }

        private void SavePicture(long wordId, byte[] picture)
        {
            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    var word = (from w in context.Word
                                where w.WordID == wordId
                                select w).FirstOrDefault();

                    if (word != null)
                    {
                        //MemoryStream stream = new MemoryStream(picture);
                        word.Picture = picture;
                        context.SaveChanges();
                    }
                }
            }
        }

        private void ShowPicture(Dictionary<PicturyType, long> allWordIds, PicturyType picturyType, Canvas canvas)
        {

            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    long wordId = allWordIds[picturyType];

                    var word = (from w in context.Word
                                where w.WordID == wordId
                                select w).FirstOrDefault();

                    if (word != null)
                    {
                        if (word.Picture != null)
                        {
                            MemoryStream stream = new MemoryStream(word.Picture);

                            BitmapImage bi = new BitmapImage();
                            bi.BeginInit();
                            bi.StreamSource = stream;
                            bi.EndInit();

                            Image i = new Image();
                            i.Stretch = Stretch.Fill;
                            i.Source = bi;

                            if (picturyType == PicturyType.Base)
                            {
                                i.Width = 312;
                                i.Height = 312;
                            }
                            else
                            {
                                i.Width = 100;
                                i.Height = 100;
                            }
                            canvas.Children.Clear();
                            canvas.Children.Add(i);

                            //i.SetValue(Image.SourceProperty, stream.ToArray());
                            //myBitmapImage.SignatureImage = new Bitmap(stream);
                        }
                        else
                        {
                            TextBlock t = new TextBlock();
                            t.Text = word.WordString;
                            canvas.Children.Clear();
                            canvas.Children.Insert(0, t);
                        }
                    }
                }
            }
        }
        #endregion

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if (dlg.ShowDialog() ?? true)
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);

                byte[] data = new byte[fs.Length];
                fs.Read(data, 0, System.Convert.ToInt32(fs.Length));

                fs.Close();

                if (lbWords.SelectedItem != null)
                {
                    Word w = (Word)((ListBoxItem)lbWords.SelectedItem).Tag;
                    SavePicture(w.WordID, data);
                }
            }
        }

        private void lbGroup_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            lbWords.Items.Clear();
            using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
            {
                using (DatabaseEntities context = new DatabaseEntities())
                {
                    GetItemFromMouseClick(lbGroup, e);

                    if (lbGroup.SelectedItem != null)
                    {
                        Rime r = (Rime)((ListBoxItem)lbGroup.SelectedItem).Tag;

                        var words = (from w in context.Word
                                     where w.RimeID == r.RimeID
                                     select w).Distinct().ToList();


                        foreach (Word w in words)
                        {
                            ListBoxItem item = new ListBoxItem();
                            item.Content = w.WordString;
                            item.Tag = w;

                            lbWords.Items.Add(item);
                        }
                    }
                }
            }
        }

        private void GetItemFromMouseClick(ListBox lb, MouseButtonEventArgs e)
        {
            Point clickPoint = e.GetPosition(lb);

            var listBoxItem =
                         lb.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;

            if (listBoxItem != null)
            {
                var nPotenialIndex = (int)(clickPoint.Y / listBoxItem.ActualHeight);

                if (nPotenialIndex > -1 && nPotenialIndex < lb.Items.Count)
                {
                    lb.SelectedItem = lb.Items[nPotenialIndex];
                }
            }
        }

        private void btnNewGroup_Click(object sender, RoutedEventArgs e)
        {
            AddEditGroup d = new AddEditGroup();
            d.ShowDialog();


            ListBoxItem item = new ListBoxItem();
            item.Content = d.ReturnValue.RimeWord;

            item.Tag = d.ReturnValue;

            lbGroup.Items.Add(item);

            lbGroup.Items.Refresh();

            d.Close();

        }

        private void btnRemoveGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lbGroup.SelectedItem != null)
            {

                Rime selectedRime = (Rime)((ListBoxItem)lbGroup.SelectedItem).Tag;

                using (EntityConnection connection = new EntityConnection("Name = DatabaseEntities"))
                {
                    using (DatabaseEntities context = new DatabaseEntities())
                    {
                        var words = (from w in context.Word
                                     where w.RimeID == selectedRime.RimeID
                                     select w).Distinct().ToList();

                        foreach (Word w in words)
                        {
                            context.Word.DeleteObject(w);
                        }

                        var rime = from r in context.Rime
                                   where r.RimeID == selectedRime.RimeID
                                   select r;

                        foreach (Rime r in rime)
                        {
                            context.Rime.DeleteObject(r);
                        }


                        context.SaveChanges();
                    }
                }
                lbGroup.Items.Refresh();
            }
        }

        private void lblChangeGroup_Click(object sender, RoutedEventArgs e)
        {
            if (lbGroup.SelectedItem != null)
            {
                Rime r = (Rime)((ListBoxItem)lbGroup.SelectedItem).Tag;

                AddEditGroup d = new AddEditGroup(r);
                d.ShowDialog();

                ((ListBoxItem)lbGroup.SelectedItem).Tag = d.ReturnValue;



            }
        }

        private void btnRemoveWord_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
